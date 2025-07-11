"""
API Gateway - Central entry point for the AI Trading Platform
"""
import asyncio
from contextlib import asynccontextmanager
from typing import Dict, Any
import logging

from fastapi import FastAPI, Request, HTTPException, Depends
from fastapi.middleware.cors import CORSMiddleware
from fastapi.middleware.trustedhost import TrustedHostMiddleware
from fastapi.responses import JSONResponse
import uvicorn
from prometheus_client import make_asgi_app, Counter, Histogram, Gauge
import structlog

# Import shared modules
import sys
import os
sys.path.append(os.path.join(os.path.dirname(__file__), '..', '..'))

from shared.config import settings
from shared.database import init_databases, close_databases
from shared.messaging import messaging_manager

from routers import auth, trading, ai, payments, documents, users
from middleware import RateLimitMiddleware, LoggingMiddleware, MetricsMiddleware
from dependencies import get_current_user, check_api_key


# Configure logging
structlog.configure(
    processors=[
        structlog.stdlib.filter_by_level,
        structlog.stdlib.add_logger_name,
        structlog.stdlib.add_log_level,
        structlog.stdlib.PositionalArgumentsFormatter(),
        structlog.processors.TimeStamper(fmt="iso"),
        structlog.processors.StackInfoRenderer(),
        structlog.processors.format_exc_info,
        structlog.processors.UnicodeDecoder(),
        structlog.processors.JSONRenderer()
    ],
    context_class=dict,
    logger_factory=structlog.stdlib.LoggerFactory(),
    wrapper_class=structlog.stdlib.BoundLogger,
    cache_logger_on_first_use=True,
)

logger = structlog.get_logger()

# Prometheus metrics
REQUEST_COUNT = Counter('api_requests_total', 'Total API requests', ['method', 'endpoint', 'status'])
REQUEST_DURATION = Histogram('api_request_duration_seconds', 'API request duration')
ACTIVE_CONNECTIONS = Gauge('api_active_connections', 'Active connections')


@asynccontextmanager
async def lifespan(app: FastAPI):
    """Application lifespan manager"""
    # Startup
    logger.info("Starting AI Trading Platform API Gateway")
    
    try:
        # Initialize databases
        await init_databases()
        logger.info("Databases initialized")
        
        # Initialize messaging
        await messaging_manager.initialize()
        logger.info("Messaging system initialized")
        
        # Additional startup tasks
        await startup_tasks()
        
        logger.info("API Gateway startup completed")
        
        yield
        
    finally:
        # Shutdown
        logger.info("Shutting down API Gateway")
        
        try:
            await messaging_manager.close()
            await close_databases()
            logger.info("Cleanup completed")
        except Exception as e:
            logger.error(f"Error during shutdown: {e}")


async def startup_tasks():
    """Additional startup tasks"""
    # Warm up connections
    # Pre-load models
    # Initialize caches
    pass


# Create FastAPI application
app = FastAPI(
    title="AI Trading Platform API",
    description="An AI-driven platform for trading and crypto workflows",
    version=settings.app_version,
    lifespan=lifespan,
    docs_url="/docs" if settings.debug else None,
    redoc_url="/redoc" if settings.debug else None,
    openapi_url="/openapi.json" if settings.debug else None
)

# Add middleware
app.add_middleware(
    CORSMiddleware,
    allow_origins=settings.allowed_hosts,
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
    expose_headers=["X-Request-ID", "X-Rate-Limit-Remaining"]
)

app.add_middleware(
    TrustedHostMiddleware,
    allowed_hosts=settings.allowed_hosts
)

# Custom middleware
app.add_middleware(MetricsMiddleware)
app.add_middleware(LoggingMiddleware)
app.add_middleware(RateLimitMiddleware)

# Add Prometheus metrics endpoint
metrics_app = make_asgi_app()
app.mount("/metrics", metrics_app)

# Include routers
app.include_router(auth.router, prefix="/api/v1/auth", tags=["authentication"])
app.include_router(users.router, prefix="/api/v1/users", tags=["users"])
app.include_router(trading.router, prefix="/api/v1/trading", tags=["trading"])
app.include_router(ai.router, prefix="/api/v1/ai", tags=["ai"])
app.include_router(payments.router, prefix="/api/v1/payments", tags=["payments"])
app.include_router(documents.router, prefix="/api/v1/documents", tags=["documents"])


@app.get("/", response_model=Dict[str, Any])
async def root():
    """Root endpoint with API information"""
    return {
        "name": settings.app_name,
        "version": settings.app_version,
        "status": "healthy",
        "services": {
            "user_service": "http://user-service:8001",
            "trading_service": "http://trading-service:8002", 
            "ai_service": "http://ai-service:8003",
            "payment_service": "http://payment-service:8004",
            "notification_service": "http://notification-service:8005",
            "document_service": "http://document-service:8006"
        }
    }


@app.get("/health")
async def health_check():
    """Health check endpoint"""
    health_status = {
        "status": "healthy",
        "timestamp": asyncio.get_event_loop().time(),
        "services": {
            "database": "unknown",
            "messaging": "unknown",
            "cache": "unknown"
        }
    }
    
    try:
        # Check database connectivity
        from shared.database import get_postgres_session, get_redis_client
        
        async with get_postgres_session() as session:
            await session.execute("SELECT 1")
            health_status["services"]["database"] = "healthy"
        
        # Check Redis
        redis_client = get_redis_client()
        await redis_client.ping()
        health_status["services"]["cache"] = "healthy"
        
        # Check messaging
        if messaging_manager.initialized:
            health_status["services"]["messaging"] = "healthy"
        
    except Exception as e:
        logger.error(f"Health check failed: {e}")
        health_status["status"] = "unhealthy"
        health_status["error"] = str(e)
        return JSONResponse(status_code=503, content=health_status)
    
    return health_status


@app.get("/api/v1/status")
async def api_status(current_user: dict = Depends(get_current_user)):
    """API status endpoint (requires authentication)"""
    return {
        "user_id": current_user["user_id"],
        "timestamp": asyncio.get_event_loop().time(),
        "features": {
            "hybrid_llm": True,
            "payments": True,
            "document_processing": True,
            "real_time_trading": True
        }
    }


@app.exception_handler(HTTPException)
async def http_exception_handler(request: Request, exc: HTTPException):
    """Custom HTTP exception handler"""
    return JSONResponse(
        status_code=exc.status_code,
        content={
            "error": {
                "code": exc.status_code,
                "message": exc.detail,
                "request_id": request.headers.get("X-Request-ID")
            }
        }
    )


@app.exception_handler(Exception)
async def general_exception_handler(request: Request, exc: Exception):
    """General exception handler"""
    logger.error(f"Unhandled exception: {exc}", exc_info=True)
    
    return JSONResponse(
        status_code=500,
        content={
            "error": {
                "code": 500,
                "message": "Internal server error",
                "request_id": request.headers.get("X-Request-ID")
            }
        }
    )


if __name__ == "__main__":
    uvicorn.run(
        "main:app",
        host="0.0.0.0",
        port=8000,
        reload=settings.debug,
        log_level=settings.log_level.lower(),
        access_log=True
    ) 