"""
Shared configuration settings for the AI Trading Platform
"""
from pydantic_settings import BaseSettings
from typing import Optional, List
import os


class Settings(BaseSettings):
    # Application Settings
    app_name: str = "AI Trading Platform"
    app_version: str = "1.0.0"
    debug: bool = False
    
    # Database URLs
    database_url: str = "postgresql://postgres:postgres@localhost:5432/trading_platform"
    mongodb_url: str = "mongodb://admin:password@localhost:27017"
    redis_url: str = "redis://localhost:6379"
    
    # Message Queue
    rabbitmq_url: str = "amqp://admin:password@localhost:5672"
    
    # AWS Configuration
    aws_region: str = "us-east-1"
    aws_access_key_id: Optional[str] = None
    aws_secret_access_key: Optional[str] = None
    
    # AWS SQS/SNS
    sqs_queue_url: Optional[str] = None
    sns_topic_arn: Optional[str] = None
    
    # AI/ML Configuration
    ollama_url: str = "http://localhost:11434"
    openroute_api_key: Optional[str] = None
    openai_api_key: Optional[str] = None
    
    # Payment Configuration
    stripe_secret_key: Optional[str] = None
    stripe_webhook_secret: Optional[str] = None
    stripe_price_id: Optional[str] = None
    
    # Security
    secret_key: str = "your-secret-key-change-in-production"
    algorithm: str = "HS256"
    access_token_expire_minutes: int = 30
    refresh_token_expire_days: int = 7
    
    # CORS
    allowed_hosts: List[str] = ["*"]
    
    # Monitoring
    enable_metrics: bool = True
    log_level: str = "INFO"
    
    class Config:
        env_file = ".env"
        env_file_encoding = "utf-8"


# Global settings instance
settings = Settings()


class DatabaseConfig:
    """Database connection configurations"""
    
    @staticmethod
    def get_postgres_url() -> str:
        return settings.database_url
    
    @staticmethod
    def get_mongodb_url() -> str:
        return settings.mongodb_url
    
    @staticmethod
    def get_redis_url() -> str:
        return settings.redis_url


class MessageQueueConfig:
    """Message queue configurations"""
    
    @staticmethod
    def get_rabbitmq_url() -> str:
        return settings.rabbitmq_url
    
    @staticmethod
    def get_sqs_queue_url() -> Optional[str]:
        return settings.sqs_queue_url
    
    @staticmethod
    def get_sns_topic_arn() -> Optional[str]:
        return settings.sns_topic_arn


class AIConfig:
    """AI/ML service configurations"""
    
    @staticmethod
    def get_ollama_url() -> str:
        return settings.ollama_url
    
    @staticmethod
    def get_openroute_api_key() -> Optional[str]:
        return settings.openroute_api_key
    
    @staticmethod
    def get_openai_api_key() -> Optional[str]:
        return settings.openai_api_key 