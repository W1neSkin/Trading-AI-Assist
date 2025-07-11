"""
Payment Service - Stripe Integration for Payments and Billing
"""
import asyncio
import json
import hmac
import hashlib
from contextlib import asynccontextmanager
from typing import Dict, Any, List, Optional
from datetime import datetime, timedelta
import logging

from fastapi import FastAPI, HTTPException, Request, Depends, BackgroundTasks
from fastapi.responses import JSONResponse
from pydantic import BaseModel, Field
import stripe
import structlog

# Import shared modules
import sys
import os
sys.path.append(os.path.join(os.path.dirname(__file__), '..', '..'))

from shared.config import settings
from shared.database import get_postgres_session
from shared.messaging import messaging_manager

# Configure logging
logger = structlog.get_logger()

# Configure Stripe
stripe.api_key = settings.stripe_secret_key
stripe.api_version = "2023-10-16"


class PaymentRequest(BaseModel):
    """Payment request model"""
    amount: int = Field(..., description="Amount in cents")
    currency: str = Field(default="usd", description="Currency code")
    customer_id: Optional[str] = Field(None, description="Stripe customer ID")
    payment_method_id: Optional[str] = Field(None, description="Payment method ID")
    description: Optional[str] = Field(None, description="Payment description")
    metadata: Optional[Dict[str, str]] = Field(default_factory=dict)


class SubscriptionRequest(BaseModel):
    """Subscription request model"""
    customer_id: str = Field(..., description="Stripe customer ID")
    price_id: str = Field(..., description="Stripe price ID")
    payment_method_id: str = Field(..., description="Payment method ID")
    trial_days: Optional[int] = Field(None, description="Trial period in days")
    metadata: Optional[Dict[str, str]] = Field(default_factory=dict)


class CustomerRequest(BaseModel):
    """Customer creation request"""
    email: str = Field(..., description="Customer email")
    name: Optional[str] = Field(None, description="Customer name")
    phone: Optional[str] = Field(None, description="Customer phone")
    metadata: Optional[Dict[str, str]] = Field(default_factory=dict)


class KYCRequest(BaseModel):
    """KYC verification request"""
    customer_id: str = Field(..., description="Customer ID")
    document_type: str = Field(..., description="Document type (id, passport, etc.)")
    document_front: str = Field(..., description="Front document URL or data")
    document_back: Optional[str] = Field(None, description="Back document URL or data")
    personal_info: Dict[str, Any] = Field(..., description="Personal information")


class BillingManager:
    """Billing and subscription management"""
    
    def __init__(self):
        self.webhook_secret = settings.stripe_webhook_secret
    
    async def create_customer(self, request: CustomerRequest) -> Dict[str, Any]:
        """Create a new Stripe customer"""
        try:
            customer = stripe.Customer.create(
                email=request.email,
                name=request.name,
                phone=request.phone,
                metadata=request.metadata
            )
            
            # Store customer in database
            await self._store_customer(customer)
            
            return {
                "customer_id": customer.id,
                "email": customer.email,
                "created": customer.created
            }
            
        except stripe.error.StripeError as e:
            logger.error(f"Stripe customer creation failed: {e}")
            raise HTTPException(status_code=400, detail=str(e))
    
    async def create_payment_intent(self, request: PaymentRequest) -> Dict[str, Any]:
        """Create a payment intent"""
        try:
            intent_data = {
                "amount": request.amount,
                "currency": request.currency,
                "metadata": request.metadata,
                "automatic_payment_methods": {"enabled": True}
            }
            
            if request.customer_id:
                intent_data["customer"] = request.customer_id
            
            if request.payment_method_id:
                intent_data["payment_method"] = request.payment_method_id
                intent_data["confirmation_method"] = "manual"
                intent_data["confirm"] = True
            
            if request.description:
                intent_data["description"] = request.description
            
            payment_intent = stripe.PaymentIntent.create(**intent_data)
            
            # Store payment intent in database
            await self._store_payment_intent(payment_intent)
            
            return {
                "client_secret": payment_intent.client_secret,
                "payment_intent_id": payment_intent.id,
                "status": payment_intent.status
            }
            
        except stripe.error.StripeError as e:
            logger.error(f"Payment intent creation failed: {e}")
            raise HTTPException(status_code=400, detail=str(e))
    
    async def create_subscription(self, request: SubscriptionRequest) -> Dict[str, Any]:
        """Create a subscription"""
        try:
            # Attach payment method to customer
            stripe.PaymentMethod.attach(
                request.payment_method_id,
                customer=request.customer_id
            )
            
            # Update customer's default payment method
            stripe.Customer.modify(
                request.customer_id,
                invoice_settings={
                    "default_payment_method": request.payment_method_id
                }
            )
            
            # Create subscription
            subscription_data = {
                "customer": request.customer_id,
                "items": [{"price": request.price_id}],
                "expand": ["latest_invoice.payment_intent"],
                "metadata": request.metadata
            }
            
            if request.trial_days:
                subscription_data["trial_period_days"] = request.trial_days
            
            subscription = stripe.Subscription.create(**subscription_data)
            
            # Store subscription in database
            await self._store_subscription(subscription)
            
            return {
                "subscription_id": subscription.id,
                "status": subscription.status,
                "current_period_start": subscription.current_period_start,
                "current_period_end": subscription.current_period_end,
                "trial_end": subscription.trial_end
            }
            
        except stripe.error.StripeError as e:
            logger.error(f"Subscription creation failed: {e}")
            raise HTTPException(status_code=400, detail=str(e))
    
    async def handle_webhook(self, request: Request) -> Dict[str, Any]:
        """Handle Stripe webhooks"""
        try:
            payload = await request.body()
            sig_header = request.headers.get("stripe-signature")
            
            # Verify webhook signature
            event = stripe.Webhook.construct_event(
                payload, sig_header, self.webhook_secret
            )
            
            # Process event
            await self._process_webhook_event(event)
            
            return {"status": "success"}
            
        except ValueError as e:
            logger.error(f"Invalid payload: {e}")
            raise HTTPException(status_code=400, detail="Invalid payload")
        except stripe.error.SignatureVerificationError as e:
            logger.error(f"Invalid signature: {e}")
            raise HTTPException(status_code=400, detail="Invalid signature")
    
    async def _process_webhook_event(self, event: Dict[str, Any]):
        """Process Stripe webhook events"""
        event_type = event["type"]
        data = event["data"]["object"]
        
        logger.info(f"Processing webhook event: {event_type}")
        
        if event_type == "payment_intent.succeeded":
            await self._handle_payment_success(data)
        elif event_type == "payment_intent.payment_failed":
            await self._handle_payment_failure(data)
        elif event_type == "invoice.payment_succeeded":
            await self._handle_invoice_payment_success(data)
        elif event_type == "invoice.payment_failed":
            await self._handle_invoice_payment_failure(data)
        elif event_type == "customer.subscription.created":
            await self._handle_subscription_created(data)
        elif event_type == "customer.subscription.updated":
            await self._handle_subscription_updated(data)
        elif event_type == "customer.subscription.deleted":
            await self._handle_subscription_deleted(data)
        else:
            logger.info(f"Unhandled event type: {event_type}")
    
    async def _handle_payment_success(self, payment_intent: Dict[str, Any]):
        """Handle successful payment"""
        try:
            # Update payment status in database
            async with get_postgres_session() as session:
                await session.execute(
                    """
                    UPDATE payments 
                    SET status = 'succeeded', updated_at = NOW()
                    WHERE stripe_payment_intent_id = :pi_id
                    """,
                    {"pi_id": payment_intent["id"]}
                )
            
            # Publish payment success event
            await messaging_manager.publish_event(
                "payment_succeeded",
                {
                    "payment_intent_id": payment_intent["id"],
                    "amount": payment_intent["amount"],
                    "currency": payment_intent["currency"],
                    "customer_id": payment_intent.get("customer")
                }
            )
            
            logger.info(f"Payment succeeded: {payment_intent['id']}")
            
        except Exception as e:
            logger.error(f"Failed to handle payment success: {e}")
    
    async def _handle_payment_failure(self, payment_intent: Dict[str, Any]):
        """Handle failed payment"""
        try:
            # Update payment status in database
            async with get_postgres_session() as session:
                await session.execute(
                    """
                    UPDATE payments 
                    SET status = 'failed', updated_at = NOW()
                    WHERE stripe_payment_intent_id = :pi_id
                    """,
                    {"pi_id": payment_intent["id"]}
                )
            
            # Publish payment failure event
            await messaging_manager.publish_event(
                "payment_failed",
                {
                    "payment_intent_id": payment_intent["id"],
                    "amount": payment_intent["amount"],
                    "currency": payment_intent["currency"],
                    "customer_id": payment_intent.get("customer"),
                    "failure_reason": payment_intent.get("last_payment_error", {}).get("message")
                }
            )
            
            logger.error(f"Payment failed: {payment_intent['id']}")
            
        except Exception as e:
            logger.error(f"Failed to handle payment failure: {e}")
    
    async def _handle_subscription_created(self, subscription: Dict[str, Any]):
        """Handle subscription creation"""
        try:
            # Update subscription status in database
            async with get_postgres_session() as session:
                await session.execute(
                    """
                    UPDATE subscriptions 
                    SET status = :status, updated_at = NOW()
                    WHERE stripe_subscription_id = :sub_id
                    """,
                    {"status": subscription["status"], "sub_id": subscription["id"]}
                )
            
            # Publish subscription created event
            await messaging_manager.publish_event(
                "subscription_created",
                {
                    "subscription_id": subscription["id"],
                    "customer_id": subscription["customer"],
                    "status": subscription["status"]
                }
            )
            
        except Exception as e:
            logger.error(f"Failed to handle subscription creation: {e}")
    
    async def _store_customer(self, customer):
        """Store customer in database"""
        try:
            async with get_postgres_session() as session:
                await session.execute(
                    """
                    INSERT INTO customers (stripe_customer_id, email, name, phone, metadata, created_at)
                    VALUES (:customer_id, :email, :name, :phone, :metadata, NOW())
                    ON CONFLICT (stripe_customer_id) DO UPDATE SET
                        email = :email,
                        name = :name,
                        phone = :phone,
                        metadata = :metadata,
                        updated_at = NOW()
                    """,
                    {
                        "customer_id": customer.id,
                        "email": customer.email,
                        "name": customer.name,
                        "phone": customer.phone,
                        "metadata": json.dumps(customer.metadata or {})
                    }
                )
        except Exception as e:
            logger.error(f"Failed to store customer: {e}")
    
    async def _store_payment_intent(self, payment_intent):
        """Store payment intent in database"""
        try:
            async with get_postgres_session() as session:
                await session.execute(
                    """
                    INSERT INTO payments (
                        stripe_payment_intent_id, amount, currency, status, 
                        customer_id, metadata, created_at
                    )
                    VALUES (:pi_id, :amount, :currency, :status, :customer_id, :metadata, NOW())
                    ON CONFLICT (stripe_payment_intent_id) DO UPDATE SET
                        status = :status,
                        updated_at = NOW()
                    """,
                    {
                        "pi_id": payment_intent.id,
                        "amount": payment_intent.amount,
                        "currency": payment_intent.currency,
                        "status": payment_intent.status,
                        "customer_id": payment_intent.customer,
                        "metadata": json.dumps(payment_intent.metadata or {})
                    }
                )
        except Exception as e:
            logger.error(f"Failed to store payment intent: {e}")
    
    async def _store_subscription(self, subscription):
        """Store subscription in database"""
        try:
            async with get_postgres_session() as session:
                await session.execute(
                    """
                    INSERT INTO subscriptions (
                        stripe_subscription_id, customer_id, status, 
                        current_period_start, current_period_end, trial_end,
                        metadata, created_at
                    )
                    VALUES (:sub_id, :customer_id, :status, :start, :end, :trial_end, :metadata, NOW())
                    ON CONFLICT (stripe_subscription_id) DO UPDATE SET
                        status = :status,
                        current_period_start = :start,
                        current_period_end = :end,
                        trial_end = :trial_end,
                        updated_at = NOW()
                    """,
                    {
                        "sub_id": subscription.id,
                        "customer_id": subscription.customer,
                        "status": subscription.status,
                        "start": datetime.fromtimestamp(subscription.current_period_start),
                        "end": datetime.fromtimestamp(subscription.current_period_end),
                        "trial_end": datetime.fromtimestamp(subscription.trial_end) if subscription.trial_end else None,
                        "metadata": json.dumps(subscription.metadata or {})
                    }
                )
        except Exception as e:
            logger.error(f"Failed to store subscription: {e}")


class KYCManager:
    """KYC and identity verification management"""
    
    def __init__(self):
        self.verification_session_cache = {}
    
    async def initiate_kyc(self, request: KYCRequest) -> Dict[str, Any]:
        """Initiate KYC verification process"""
        try:
            # Create Stripe Identity verification session
            verification_session = stripe.identity.VerificationSession.create(
                type="document",
                metadata={
                    "customer_id": request.customer_id,
                    "internal_id": f"kyc_{request.customer_id}_{int(datetime.now().timestamp())}"
                },
                options={
                    "document": {
                        "allowed_types": ["driving_license", "passport", "id_card"],
                        "require_id_number": True,
                        "require_live_capture": True,
                        "require_matching_selfie": True
                    }
                }
            )
            
            # Store verification session
            await self._store_verification_session(verification_session, request.customer_id)
            
            return {
                "verification_session_id": verification_session.id,
                "client_secret": verification_session.client_secret,
                "url": verification_session.url,
                "status": verification_session.status
            }
            
        except stripe.error.StripeError as e:
            logger.error(f"KYC initiation failed: {e}")
            raise HTTPException(status_code=400, detail=str(e))
    
    async def check_kyc_status(self, verification_session_id: str) -> Dict[str, Any]:
        """Check KYC verification status"""
        try:
            verification_session = stripe.identity.VerificationSession.retrieve(
                verification_session_id
            )
            
            # Update status in database
            await self._update_verification_status(verification_session)
            
            return {
                "session_id": verification_session.id,
                "status": verification_session.status,
                "verified_outputs": verification_session.verified_outputs,
                "last_verification_report": verification_session.last_verification_report
            }
            
        except stripe.error.StripeError as e:
            logger.error(f"KYC status check failed: {e}")
            raise HTTPException(status_code=400, detail=str(e))
    
    async def _store_verification_session(self, session, customer_id: str):
        """Store verification session in database"""
        try:
            async with get_postgres_session() as session_db:
                await session_db.execute(
                    """
                    INSERT INTO kyc_verifications (
                        verification_session_id, customer_id, status, created_at
                    )
                    VALUES (:session_id, :customer_id, :status, NOW())
                    """,
                    {
                        "session_id": session.id,
                        "customer_id": customer_id,
                        "status": session.status
                    }
                )
        except Exception as e:
            logger.error(f"Failed to store verification session: {e}")
    
    async def _update_verification_status(self, session):
        """Update verification status in database"""
        try:
            async with get_postgres_session() as session_db:
                await session_db.execute(
                    """
                    UPDATE kyc_verifications 
                    SET status = :status, 
                        verified_outputs = :outputs,
                        updated_at = NOW()
                    WHERE verification_session_id = :session_id
                    """,
                    {
                        "status": session.status,
                        "outputs": json.dumps(session.verified_outputs or {}),
                        "session_id": session.id
                    }
                )
        except Exception as e:
            logger.error(f"Failed to update verification status: {e}")


# Initialize managers
billing_manager = BillingManager()
kyc_manager = KYCManager()


@asynccontextmanager
async def lifespan(app: FastAPI):
    """Application lifespan manager"""
    # Startup
    logger.info("Starting Payment Service")
    
    try:
        # Initialize messaging
        await messaging_manager.initialize()
        
        logger.info("Payment Service startup completed")
        
        yield
        
    finally:
        # Shutdown
        logger.info("Shutting down Payment Service")


# Create FastAPI application
app = FastAPI(
    title="Payment Service",
    description="Stripe payment processing and billing management",
    version="1.0.0",
    lifespan=lifespan
)


@app.post("/customers")
async def create_customer(request: CustomerRequest) -> Dict[str, Any]:
    """Create a new customer"""
    return await billing_manager.create_customer(request)


@app.post("/payment-intents")
async def create_payment_intent(request: PaymentRequest) -> Dict[str, Any]:
    """Create a payment intent"""
    return await billing_manager.create_payment_intent(request)


@app.post("/subscriptions")
async def create_subscription(request: SubscriptionRequest) -> Dict[str, Any]:
    """Create a subscription"""
    return await billing_manager.create_subscription(request)


@app.post("/kyc/initiate")
async def initiate_kyc(request: KYCRequest) -> Dict[str, Any]:
    """Initiate KYC verification"""
    return await kyc_manager.initiate_kyc(request)


@app.get("/kyc/status/{verification_session_id}")
async def check_kyc_status(verification_session_id: str) -> Dict[str, Any]:
    """Check KYC verification status"""
    return await kyc_manager.check_kyc_status(verification_session_id)


@app.post("/webhooks/stripe")
async def stripe_webhook(request: Request) -> Dict[str, Any]:
    """Handle Stripe webhooks"""
    return await billing_manager.handle_webhook(request)


@app.get("/customers/{customer_id}/subscriptions")
async def get_customer_subscriptions(customer_id: str) -> Dict[str, Any]:
    """Get customer subscriptions"""
    try:
        subscriptions = stripe.Subscription.list(customer=customer_id)
        
        return {
            "customer_id": customer_id,
            "subscriptions": [
                {
                    "id": sub.id,
                    "status": sub.status,
                    "current_period_start": sub.current_period_start,
                    "current_period_end": sub.current_period_end,
                    "trial_end": sub.trial_end
                }
                for sub in subscriptions.data
            ]
        }
        
    except stripe.error.StripeError as e:
        logger.error(f"Failed to get customer subscriptions: {e}")
        raise HTTPException(status_code=400, detail=str(e))


@app.post("/subscriptions/{subscription_id}/cancel")
async def cancel_subscription(subscription_id: str) -> Dict[str, Any]:
    """Cancel a subscription"""
    try:
        subscription = stripe.Subscription.modify(
            subscription_id,
            cancel_at_period_end=True
        )
        
        return {
            "subscription_id": subscription.id,
            "status": subscription.status,
            "cancel_at_period_end": subscription.cancel_at_period_end,
            "current_period_end": subscription.current_period_end
        }
        
    except stripe.error.StripeError as e:
        logger.error(f"Failed to cancel subscription: {e}")
        raise HTTPException(status_code=400, detail=str(e))


@app.get("/invoices/{customer_id}")
async def get_customer_invoices(customer_id: str) -> Dict[str, Any]:
    """Get customer invoices"""
    try:
        invoices = stripe.Invoice.list(customer=customer_id, limit=10)
        
        return {
            "customer_id": customer_id,
            "invoices": [
                {
                    "id": inv.id,
                    "amount_due": inv.amount_due,
                    "amount_paid": inv.amount_paid,
                    "status": inv.status,
                    "created": inv.created,
                    "due_date": inv.due_date
                }
                for inv in invoices.data
            ]
        }
        
    except stripe.error.StripeError as e:
        logger.error(f"Failed to get customer invoices: {e}")
        raise HTTPException(status_code=400, detail=str(e))


@app.get("/health")
async def health_check():
    """Health check endpoint"""
    return {
        "status": "healthy",
        "timestamp": datetime.now().isoformat(),
        "stripe_configured": bool(settings.stripe_secret_key),
        "webhook_configured": bool(settings.stripe_webhook_secret)
    }


if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="0.0.0.0", port=8004) 