from fastapi import FastAPI, HTTPException, BackgroundTasks
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel, EmailStr
from typing import List, Optional, Dict, Any, Union
from datetime import datetime
from enum import Enum
import uuid
import asyncio
import logging
import json
import smtplib
from email.mime.text import MIMEText
from email.mime.multipart import MIMEMultipart
import boto3
from contextlib import asynccontextmanager

# Local imports
import sys
import os
sys.path.append(os.path.dirname(os.path.dirname(os.path.dirname(__file__))))

from shared.config import settings
from shared.database import postgresql_manager, mongodb_manager
from shared.messaging import hybrid_messaging_manager

# Configure logging
logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

# Enums
class NotificationType(str, Enum):
    EMAIL = "email"
    SMS = "sms"
    PUSH = "push"
    IN_APP = "in_app"
    WEBHOOK = "webhook"

class NotificationStatus(str, Enum):
    PENDING = "pending"
    SENT = "sent"
    DELIVERED = "delivered"
    FAILED = "failed"
    BOUNCED = "bounced"

class Priority(str, Enum):
    LOW = "low"
    NORMAL = "normal"
    HIGH = "high"
    URGENT = "urgent"

# Pydantic models
class NotificationTemplate(BaseModel):
    id: str
    name: str
    type: NotificationType
    subject: Optional[str]
    body: str
    variables: List[str]
    is_active: bool = True
    created_at: datetime
    updated_at: datetime

class CreateNotificationTemplate(BaseModel):
    name: str
    type: NotificationType
    subject: Optional[str] = None
    body: str
    variables: List[str] = []

class Notification(BaseModel):
    id: str
    user_id: str
    type: NotificationType
    recipient: str
    subject: Optional[str]
    body: str
    status: NotificationStatus
    priority: Priority
    scheduled_at: Optional[datetime]
    sent_at: Optional[datetime]
    delivered_at: Optional[datetime]
    error_message: Optional[str]
    metadata: Dict[str, Any] = {}
    created_at: datetime
    updated_at: datetime

class CreateNotification(BaseModel):
    user_id: str
    type: NotificationType
    recipient: str
    subject: Optional[str] = None
    body: str
    priority: Priority = Priority.NORMAL
    scheduled_at: Optional[datetime] = None
    metadata: Dict[str, Any] = {}

class BulkNotification(BaseModel):
    user_ids: List[str]
    type: NotificationType
    subject: Optional[str] = None
    body: str
    priority: Priority = Priority.NORMAL
    scheduled_at: Optional[datetime] = None

class NotificationPreferences(BaseModel):
    user_id: str
    email_enabled: bool = True
    sms_enabled: bool = True
    push_enabled: bool = True
    in_app_enabled: bool = True
    marketing_emails: bool = False
    trade_alerts: bool = True
    account_alerts: bool = True
    system_alerts: bool = True

# Email Service
class EmailService:
    def __init__(self):
        self.smtp_server = settings.smtp_server
        self.smtp_port = settings.smtp_port
        self.smtp_username = settings.smtp_username
        self.smtp_password = settings.smtp_password
        self.from_email = settings.from_email
        
    async def send_email(self, to_email: str, subject: str, body: str, is_html: bool = True) -> bool:
        """Send email"""
        try:
            msg = MIMEMultipart('alternative')
            msg['Subject'] = subject
            msg['From'] = self.from_email
            msg['To'] = to_email
            
            # Attach body
            if is_html:
                msg.attach(MIMEText(body, 'html'))
            else:
                msg.attach(MIMEText(body, 'plain'))
            
            # Send email
            with smtplib.SMTP(self.smtp_server, self.smtp_port) as server:
                server.starttls()
                server.login(self.smtp_username, self.smtp_password)
                server.send_message(msg)
                
            logger.info(f"Email sent successfully to {to_email}")
            return True
            
        except Exception as e:
            logger.error(f"Failed to send email to {to_email}: {e}")
            return False

# SMS Service
class SMSService:
    def __init__(self):
        if hasattr(settings, 'aws_access_key_id') and settings.aws_access_key_id:
            self.sns_client = boto3.client(
                'sns',
                aws_access_key_id=settings.aws_access_key_id,
                aws_secret_access_key=settings.aws_secret_access_key,
                region_name=settings.aws_region
            )
        else:
            self.sns_client = None
            logger.warning("AWS credentials not configured, SMS service disabled")
    
    async def send_sms(self, phone_number: str, message: str) -> bool:
        """Send SMS via AWS SNS"""
        if not self.sns_client:
            logger.error("SMS service not configured")
            return False
            
        try:
            response = self.sns_client.publish(
                PhoneNumber=phone_number,
                Message=message
            )
            
            logger.info(f"SMS sent successfully to {phone_number}")
            return True
            
        except Exception as e:
            logger.error(f"Failed to send SMS to {phone_number}: {e}")
            return False

# Push Notification Service
class PushNotificationService:
    def __init__(self):
        # Initialize Firebase/APNs clients here
        pass
    
    async def send_push_notification(self, device_token: str, title: str, body: str, data: Dict = None) -> bool:
        """Send push notification"""
        try:
            # Implement Firebase Cloud Messaging or Apple Push Notification service
            logger.info(f"Push notification sent to device {device_token[:10]}...")
            return True
            
        except Exception as e:
            logger.error(f"Failed to send push notification: {e}")
            return False

# Webhook Service
class WebhookService:
    async def send_webhook(self, url: str, payload: Dict[str, Any]) -> bool:
        """Send webhook notification"""
        try:
            import aiohttp
            
            async with aiohttp.ClientSession() as session:
                async with session.post(url, json=payload) as response:
                    if response.status == 200:
                        logger.info(f"Webhook sent successfully to {url}")
                        return True
                    else:
                        logger.error(f"Webhook failed with status {response.status}")
                        return False
                        
        except Exception as e:
            logger.error(f"Failed to send webhook to {url}: {e}")
            return False

# Template Manager
class TemplateManager:
    async def create_template(self, template_data: CreateNotificationTemplate) -> NotificationTemplate:
        """Create notification template"""
        template_id = str(uuid.uuid4())
        
        query = """
            INSERT INTO notification_templates (id, name, type, subject, body, variables)
            VALUES ($1, $2, $3, $4, $5, $6)
            RETURNING *
        """
        
        async with postgresql_manager.get_connection() as conn:
            result = await conn.fetchrow(
                query, template_id, template_data.name, template_data.type,
                template_data.subject, template_data.body, template_data.variables
            )
            
            if result:
                return NotificationTemplate(**dict(result))

    async def get_template(self, template_id: str) -> Optional[NotificationTemplate]:
        """Get template by ID"""
        query = "SELECT * FROM notification_templates WHERE id = $1 AND is_active = true"
        
        async with postgresql_manager.get_connection() as conn:
            result = await conn.fetchrow(query, template_id)
            
            if result:
                return NotificationTemplate(**dict(result))

    async def get_template_by_name(self, name: str) -> Optional[NotificationTemplate]:
        """Get template by name"""
        query = "SELECT * FROM notification_templates WHERE name = $1 AND is_active = true"
        
        async with postgresql_manager.get_connection() as conn:
            result = await conn.fetchrow(query, name)
            
            if result:
                return NotificationTemplate(**dict(result))

    async def render_template(self, template: NotificationTemplate, variables: Dict[str, Any]) -> Dict[str, str]:
        """Render template with variables"""
        subject = template.subject or ""
        body = template.body
        
        # Simple variable substitution
        for var_name, var_value in variables.items():
            placeholder = f"{{{var_name}}}"
            subject = subject.replace(placeholder, str(var_value))
            body = body.replace(placeholder, str(var_value))
        
        return {"subject": subject, "body": body}

# Notification Manager
class NotificationManager:
    def __init__(self):
        self.email_service = EmailService()
        self.sms_service = SMSService()
        self.push_service = PushNotificationService()
        self.webhook_service = WebhookService()
        self.template_manager = TemplateManager()
        
    async def create_notification(self, notification_data: CreateNotification) -> Notification:
        """Create and queue notification"""
        notification_id = str(uuid.uuid4())
        
        query = """
            INSERT INTO notifications (
                id, user_id, type, recipient, subject, body, status, 
                priority, scheduled_at, metadata
            ) VALUES ($1, $2, $3, $4, $5, $6, $7, $8, $9, $10)
            RETURNING *
        """
        
        async with postgresql_manager.get_connection() as conn:
            result = await conn.fetchrow(
                query, notification_id, notification_data.user_id,
                notification_data.type, notification_data.recipient,
                notification_data.subject, notification_data.body,
                NotificationStatus.PENDING, notification_data.priority,
                notification_data.scheduled_at, json.dumps(notification_data.metadata)
            )
            
            if result:
                notification = Notification(**dict(result))
                
                # Queue for immediate sending if not scheduled
                if not notification_data.scheduled_at:
                    await self.queue_notification(notification)
                
                return notification

    async def queue_notification(self, notification: Notification):
        """Queue notification for sending"""
        await hybrid_messaging_manager.publish_message(
            exchange="notifications",
            routing_key=f"send.{notification.type}",
            message={
                "notification_id": notification.id,
                "type": notification.type,
                "recipient": notification.recipient,
                "subject": notification.subject,
                "body": notification.body,
                "priority": notification.priority,
                "metadata": notification.metadata
            }
        )

    async def send_notification(self, notification_id: str) -> bool:
        """Send notification"""
        # Get notification
        query = "SELECT * FROM notifications WHERE id = $1"
        
        async with postgresql_manager.get_connection() as conn:
            result = await conn.fetchrow(query, notification_id)
            
            if not result:
                logger.error(f"Notification {notification_id} not found")
                return False
            
            notification = Notification(**dict(result))
            
            # Check preferences
            if not await self.check_user_preferences(notification):
                await self.update_notification_status(
                    notification_id, NotificationStatus.FAILED, 
                    "User preferences disabled"
                )
                return False
            
            # Send based on type
            success = False
            error_message = None
            
            try:
                if notification.type == NotificationType.EMAIL:
                    success = await self.email_service.send_email(
                        notification.recipient, notification.subject or "", notification.body
                    )
                elif notification.type == NotificationType.SMS:
                    success = await self.sms_service.send_sms(
                        notification.recipient, notification.body
                    )
                elif notification.type == NotificationType.PUSH:
                    success = await self.push_service.send_push_notification(
                        notification.recipient, notification.subject or "", notification.body
                    )
                elif notification.type == NotificationType.IN_APP:
                    success = await self.create_in_app_notification(notification)
                elif notification.type == NotificationType.WEBHOOK:
                    success = await self.webhook_service.send_webhook(
                        notification.recipient, {
                            "subject": notification.subject,
                            "body": notification.body,
                            "metadata": notification.metadata
                        }
                    )
                    
            except Exception as e:
                error_message = str(e)
                logger.error(f"Error sending notification {notification_id}: {e}")
            
            # Update status
            status = NotificationStatus.SENT if success else NotificationStatus.FAILED
            await self.update_notification_status(notification_id, status, error_message)
            
            return success

    async def create_in_app_notification(self, notification: Notification) -> bool:
        """Create in-app notification"""
        try:
            collection = mongodb_manager.get_collection("in_app_notifications")
            
            doc = {
                "_id": notification.id,
                "user_id": notification.user_id,
                "subject": notification.subject,
                "body": notification.body,
                "is_read": False,
                "created_at": datetime.utcnow(),
                "metadata": notification.metadata
            }
            
            await collection.insert_one(doc)
            return True
            
        except Exception as e:
            logger.error(f"Failed to create in-app notification: {e}")
            return False

    async def check_user_preferences(self, notification: Notification) -> bool:
        """Check if user has enabled this notification type"""
        query = "SELECT * FROM notification_preferences WHERE user_id = $1"
        
        async with postgresql_manager.get_connection() as conn:
            result = await conn.fetchrow(query, notification.user_id)
            
            if not result:
                # Default preferences allow all
                return True
            
            prefs = NotificationPreferences(**dict(result))
            
            if notification.type == NotificationType.EMAIL:
                return prefs.email_enabled
            elif notification.type == NotificationType.SMS:
                return prefs.sms_enabled
            elif notification.type == NotificationType.PUSH:
                return prefs.push_enabled
            elif notification.type == NotificationType.IN_APP:
                return prefs.in_app_enabled
            
            return True

    async def update_notification_status(self, notification_id: str, status: NotificationStatus, error_message: str = None):
        """Update notification status"""
        sent_at = datetime.utcnow() if status == NotificationStatus.SENT else None
        
        query = """
            UPDATE notifications SET 
                status = $1, 
                sent_at = $2, 
                error_message = $3,
                updated_at = NOW()
            WHERE id = $4
        """
        
        async with postgresql_manager.get_connection() as conn:
            await conn.execute(query, status, sent_at, error_message, notification_id)

    async def get_user_notifications(self, user_id: str, notification_type: NotificationType = None) -> List[Notification]:
        """Get notifications for user"""
        if notification_type:
            query = "SELECT * FROM notifications WHERE user_id = $1 AND type = $2 ORDER BY created_at DESC"
            params = [user_id, notification_type]
        else:
            query = "SELECT * FROM notifications WHERE user_id = $1 ORDER BY created_at DESC"
            params = [user_id]
        
        async with postgresql_manager.get_connection() as conn:
            results = await conn.fetch(query, *params)
            
            return [Notification(**dict(row)) for row in results]

    async def get_in_app_notifications(self, user_id: str, unread_only: bool = False) -> List[Dict[str, Any]]:
        """Get in-app notifications"""
        collection = mongodb_manager.get_collection("in_app_notifications")
        
        filter_query = {"user_id": user_id}
        if unread_only:
            filter_query["is_read"] = False
        
        cursor = collection.find(filter_query).sort("created_at", -1)
        return await cursor.to_list(length=100)

    async def mark_in_app_read(self, user_id: str, notification_id: str) -> bool:
        """Mark in-app notification as read"""
        collection = mongodb_manager.get_collection("in_app_notifications")
        
        result = await collection.update_one(
            {"_id": notification_id, "user_id": user_id},
            {"$set": {"is_read": True, "read_at": datetime.utcnow()}}
        )
        
        return result.modified_count > 0

    async def send_bulk_notification(self, bulk_data: BulkNotification) -> List[str]:
        """Send bulk notifications"""
        notification_ids = []
        
        for user_id in bulk_data.user_ids:
            # Get user's preferred contact method
            recipient = await self.get_user_contact(user_id, bulk_data.type)
            
            if recipient:
                notification_data = CreateNotification(
                    user_id=user_id,
                    type=bulk_data.type,
                    recipient=recipient,
                    subject=bulk_data.subject,
                    body=bulk_data.body,
                    priority=bulk_data.priority,
                    scheduled_at=bulk_data.scheduled_at
                )
                
                notification = await self.create_notification(notification_data)
                notification_ids.append(notification.id)
        
        return notification_ids

    async def get_user_contact(self, user_id: str, contact_type: NotificationType) -> Optional[str]:
        """Get user contact information"""
        # This would typically query the user service
        # For now, return placeholder
        if contact_type == NotificationType.EMAIL:
            return f"user{user_id}@example.com"
        elif contact_type == NotificationType.SMS:
            return "+1234567890"
        
        return None

    async def process_scheduled_notifications(self):
        """Process scheduled notifications"""
        query = """
            SELECT * FROM notifications 
            WHERE status = 'pending' 
            AND scheduled_at <= NOW()
        """
        
        async with postgresql_manager.get_connection() as conn:
            results = await conn.fetch(query)
            
            for row in results:
                notification = Notification(**dict(row))
                await self.queue_notification(notification)

# Message consumer
class NotificationConsumer:
    def __init__(self, notification_manager: NotificationManager):
        self.notification_manager = notification_manager
        
    async def start_consuming(self):
        """Start consuming notification messages"""
        await hybrid_messaging_manager.consume_messages(
            queue="notification_queue",
            callback=self.process_notification_message
        )

    async def process_notification_message(self, message: Dict[str, Any]):
        """Process notification message"""
        try:
            notification_id = message.get("notification_id")
            if notification_id:
                await self.notification_manager.send_notification(notification_id)
                
        except Exception as e:
            logger.error(f"Error processing notification message: {e}")

# Initialize services
notification_manager = NotificationManager()
notification_consumer = NotificationConsumer(notification_manager)

# Scheduled task manager
class ScheduledTaskManager:
    def __init__(self):
        self.running = False
        
    async def start(self):
        """Start scheduled tasks"""
        self.running = True
        asyncio.create_task(self.scheduled_notifications_task())
        
    async def stop(self):
        """Stop scheduled tasks"""
        self.running = False
        
    async def scheduled_notifications_task(self):
        """Periodic task to process scheduled notifications"""
        while self.running:
            try:
                await notification_manager.process_scheduled_notifications()
                await asyncio.sleep(60)  # Check every minute
                
            except Exception as e:
                logger.error(f"Error in scheduled notifications task: {e}")
                await asyncio.sleep(60)

scheduled_task_manager = ScheduledTaskManager()

# Lifespan context manager
@asynccontextmanager
async def lifespan(app: FastAPI):
    # Startup
    logger.info("Starting Notification Service...")
    
    # Initialize database connections
    await postgresql_manager.initialize()
    await mongodb_manager.initialize()
    
    # Initialize messaging
    await hybrid_messaging_manager.initialize()
    
    # Create database tables
    await create_tables()
    
    # Start message consumer
    asyncio.create_task(notification_consumer.start_consuming())
    
    # Start scheduled tasks
    await scheduled_task_manager.start()
    
    logger.info("Notification Service started successfully")
    
    yield
    
    # Shutdown
    logger.info("Shutting down Notification Service...")
    await scheduled_task_manager.stop()
    await postgresql_manager.close()
    await mongodb_manager.close()
    await hybrid_messaging_manager.close()

# Create FastAPI app
app = FastAPI(
    title="Notification Service",
    description="Multi-channel notification and messaging service",
    version="1.0.0",
    lifespan=lifespan
)

# CORS middleware
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# Database table creation
async def create_tables():
    """Create database tables"""
    queries = [
        """
        CREATE TABLE IF NOT EXISTS notification_templates (
            id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
            name VARCHAR(255) UNIQUE NOT NULL,
            type VARCHAR(20) NOT NULL,
            subject VARCHAR(500),
            body TEXT NOT NULL,
            variables TEXT[] DEFAULT ARRAY[]::TEXT[],
            is_active BOOLEAN DEFAULT true,
            created_at TIMESTAMP DEFAULT NOW(),
            updated_at TIMESTAMP DEFAULT NOW()
        )
        """,
        """
        CREATE TABLE IF NOT EXISTS notifications (
            id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
            user_id UUID NOT NULL,
            type VARCHAR(20) NOT NULL,
            recipient VARCHAR(500) NOT NULL,
            subject VARCHAR(500),
            body TEXT NOT NULL,
            status VARCHAR(20) DEFAULT 'pending',
            priority VARCHAR(20) DEFAULT 'normal',
            scheduled_at TIMESTAMP,
            sent_at TIMESTAMP,
            delivered_at TIMESTAMP,
            error_message TEXT,
            metadata JSONB DEFAULT '{}',
            created_at TIMESTAMP DEFAULT NOW(),
            updated_at TIMESTAMP DEFAULT NOW()
        )
        """,
        """
        CREATE TABLE IF NOT EXISTS notification_preferences (
            id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
            user_id UUID UNIQUE NOT NULL,
            email_enabled BOOLEAN DEFAULT true,
            sms_enabled BOOLEAN DEFAULT true,
            push_enabled BOOLEAN DEFAULT true,
            in_app_enabled BOOLEAN DEFAULT true,
            marketing_emails BOOLEAN DEFAULT false,
            trade_alerts BOOLEAN DEFAULT true,
            account_alerts BOOLEAN DEFAULT true,
            system_alerts BOOLEAN DEFAULT true,
            created_at TIMESTAMP DEFAULT NOW(),
            updated_at TIMESTAMP DEFAULT NOW()
        )
        """,
        # Indexes
        "CREATE INDEX IF NOT EXISTS idx_notifications_user_id ON notifications(user_id);",
        "CREATE INDEX IF NOT EXISTS idx_notifications_status ON notifications(status);",
        "CREATE INDEX IF NOT EXISTS idx_notifications_scheduled_at ON notifications(scheduled_at);",
        "CREATE INDEX IF NOT EXISTS idx_notification_preferences_user_id ON notification_preferences(user_id);"
    ]
    
    async with postgresql_manager.get_connection() as conn:
        for query in queries:
            await conn.execute(query)

# API Routes
@app.get("/health")
async def health_check():
    """Health check endpoint"""
    return {"status": "healthy", "service": "notification-service"}

# Template endpoints
@app.post("/templates", response_model=NotificationTemplate)
async def create_template(template_data: CreateNotificationTemplate):
    """Create notification template"""
    return await notification_manager.template_manager.create_template(template_data)

@app.get("/templates/{template_id}", response_model=NotificationTemplate)
async def get_template(template_id: str):
    """Get template by ID"""
    template = await notification_manager.template_manager.get_template(template_id)
    if not template:
        raise HTTPException(status_code=404, detail="Template not found")
    return template

# Notification endpoints
@app.post("/notifications", response_model=Notification)
async def create_notification(notification_data: CreateNotification):
    """Create notification"""
    return await notification_manager.create_notification(notification_data)

@app.post("/notifications/bulk")
async def send_bulk_notification(bulk_data: BulkNotification):
    """Send bulk notifications"""
    notification_ids = await notification_manager.send_bulk_notification(bulk_data)
    return {"notification_ids": notification_ids, "count": len(notification_ids)}

@app.get("/notifications/user/{user_id}", response_model=List[Notification])
async def get_user_notifications(user_id: str, type: Optional[NotificationType] = None):
    """Get user notifications"""
    return await notification_manager.get_user_notifications(user_id, type)

@app.get("/notifications/in-app/{user_id}")
async def get_in_app_notifications(user_id: str, unread_only: bool = False):
    """Get in-app notifications"""
    return await notification_manager.get_in_app_notifications(user_id, unread_only)

@app.put("/notifications/in-app/{notification_id}/read")
async def mark_notification_read(notification_id: str, user_id: str):
    """Mark in-app notification as read"""
    success = await notification_manager.mark_in_app_read(user_id, notification_id)
    if not success:
        raise HTTPException(status_code=404, detail="Notification not found")
    return {"message": "Notification marked as read"}

# Preference endpoints
@app.get("/preferences/{user_id}", response_model=NotificationPreferences)
async def get_user_preferences(user_id: str):
    """Get user notification preferences"""
    query = "SELECT * FROM notification_preferences WHERE user_id = $1"
    
    async with postgresql_manager.get_connection() as conn:
        result = await conn.fetchrow(query, user_id)
        
        if result:
            return NotificationPreferences(**dict(result))
        else:
            # Return default preferences
            return NotificationPreferences(user_id=user_id)

@app.put("/preferences/{user_id}", response_model=NotificationPreferences)
async def update_user_preferences(user_id: str, preferences: NotificationPreferences):
    """Update user notification preferences"""
    query = """
        INSERT INTO notification_preferences (
            user_id, email_enabled, sms_enabled, push_enabled, in_app_enabled,
            marketing_emails, trade_alerts, account_alerts, system_alerts
        ) VALUES ($1, $2, $3, $4, $5, $6, $7, $8, $9)
        ON CONFLICT (user_id) DO UPDATE SET
            email_enabled = EXCLUDED.email_enabled,
            sms_enabled = EXCLUDED.sms_enabled,
            push_enabled = EXCLUDED.push_enabled,
            in_app_enabled = EXCLUDED.in_app_enabled,
            marketing_emails = EXCLUDED.marketing_emails,
            trade_alerts = EXCLUDED.trade_alerts,
            account_alerts = EXCLUDED.account_alerts,
            system_alerts = EXCLUDED.system_alerts,
            updated_at = NOW()
        RETURNING *
    """
    
    async with postgresql_manager.get_connection() as conn:
        result = await conn.fetchrow(
            query, user_id, preferences.email_enabled, preferences.sms_enabled,
            preferences.push_enabled, preferences.in_app_enabled,
            preferences.marketing_emails, preferences.trade_alerts,
            preferences.account_alerts, preferences.system_alerts
        )
        
        if result:
            return NotificationPreferences(**dict(result))

if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="0.0.0.0", port=8005) 