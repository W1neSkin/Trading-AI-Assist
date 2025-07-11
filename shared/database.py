"""
Shared database utilities and connections
"""
import asyncio
from typing import AsyncGenerator, Optional
from contextlib import asynccontextmanager

import asyncpg
from sqlalchemy.ext.asyncio import create_async_engine, AsyncSession, async_sessionmaker
from sqlalchemy.orm import declarative_base
from motor.motor_asyncio import AsyncIOMotorClient
import redis.asyncio as redis

from .config import settings


# SQLAlchemy Base
Base = declarative_base()

# Database engines and sessions
postgres_engine = None
postgres_session_factory = None
mongodb_client = None
redis_client = None


async def init_postgres():
    """Initialize PostgreSQL connection"""
    global postgres_engine, postgres_session_factory
    
    postgres_engine = create_async_engine(
        settings.database_url,
        echo=settings.debug,
        pool_size=20,
        max_overflow=0,
        pool_pre_ping=True,
        pool_recycle=300,
    )
    
    postgres_session_factory = async_sessionmaker(
        postgres_engine,
        class_=AsyncSession,
        expire_on_commit=False
    )


async def init_mongodb():
    """Initialize MongoDB connection"""
    global mongodb_client
    
    mongodb_client = AsyncIOMotorClient(settings.mongodb_url)
    # Test connection
    await mongodb_client.admin.command('ping')


async def init_redis():
    """Initialize Redis connection"""
    global redis_client
    
    redis_client = redis.from_url(
        settings.redis_url,
        encoding="utf-8",
        decode_responses=True,
        max_connections=20
    )
    # Test connection
    await redis_client.ping()


@asynccontextmanager
async def get_postgres_session() -> AsyncGenerator[AsyncSession, None]:
    """Get PostgreSQL session context manager"""
    if not postgres_session_factory:
        await init_postgres()
    
    async with postgres_session_factory() as session:
        try:
            yield session
            await session.commit()
        except Exception:
            await session.rollback()
            raise
        finally:
            await session.close()


def get_mongodb_database(database_name: str = "trading_docs"):
    """Get MongoDB database instance"""
    if not mongodb_client:
        raise RuntimeError("MongoDB client not initialized")
    return mongodb_client[database_name]


def get_redis_client():
    """Get Redis client instance"""
    if not redis_client:
        raise RuntimeError("Redis client not initialized")
    return redis_client


async def close_databases():
    """Close all database connections"""
    global postgres_engine, mongodb_client, redis_client
    
    if postgres_engine:
        await postgres_engine.dispose()
    
    if mongodb_client:
        mongodb_client.close()
    
    if redis_client:
        await redis_client.close()


# Database initialization
async def init_databases():
    """Initialize all database connections"""
    await asyncio.gather(
        init_postgres(),
        init_mongodb(),
        init_redis(),
        return_exceptions=True
    )


class PostgreSQLManager:
    """PostgreSQL connection manager with advanced features"""
    
    def __init__(self):
        self.engine = None
        self.session_factory = None
    
    async def initialize(self):
        """Initialize PostgreSQL with advanced configuration"""
        self.engine = create_async_engine(
            settings.database_url,
            echo=settings.debug,
            pool_size=20,
            max_overflow=10,
            pool_pre_ping=True,
            pool_recycle=3600,
            connect_args={
                "command_timeout": 60,
                "server_settings": {
                    "jit": "off",
                    "application_name": "ai_trading_platform"
                }
            }
        )
        
        self.session_factory = async_sessionmaker(
            self.engine,
            class_=AsyncSession,
            expire_on_commit=False
        )
    
    @asynccontextmanager
    async def get_session(self) -> AsyncGenerator[AsyncSession, None]:
        """Get database session with automatic transaction management"""
        if not self.session_factory:
            await self.initialize()
        
        async with self.session_factory() as session:
            try:
                yield session
                await session.commit()
            except Exception:
                await session.rollback()
                raise
            finally:
                await session.close()
    
    async def execute_raw_query(self, query: str, params: dict = None):
        """Execute raw SQL query"""
        async with self.engine.begin() as conn:
            result = await conn.execute(query, params or {})
            return result
    
    async def backup_database(self, backup_path: str):
        """Create database backup (placeholder for actual implementation)"""
        # This would typically use pg_dump
        pass
    
    async def restore_database(self, backup_path: str):
        """Restore database from backup (placeholder for actual implementation)"""
        # This would typically use pg_restore
        pass


class MongoDBManager:
    """MongoDB connection manager with advanced features"""
    
    def __init__(self):
        self.client = None
        self.database = None
    
    async def initialize(self, database_name: str = "trading_docs"):
        """Initialize MongoDB connection"""
        self.client = AsyncIOMotorClient(
            settings.mongodb_url,
            maxPoolSize=50,
            minPoolSize=10,
            maxIdleTimeMS=30000,
            waitQueueMultiple=10
        )
        
        self.database = self.client[database_name]
        
        # Test connection
        await self.client.admin.command('ping')
    
    def get_collection(self, collection_name: str):
        """Get MongoDB collection"""
        if not self.database:
            raise RuntimeError("MongoDB not initialized")
        return self.database[collection_name]
    
    async def create_indexes(self):
        """Create MongoDB indexes for performance"""
        # Document processing indexes
        documents = self.get_collection("documents")
        await documents.create_index([("user_id", 1), ("created_at", -1)])
        await documents.create_index([("document_type", 1)])
        await documents.create_index([("status", 1)])
        
        # AI embeddings indexes
        embeddings = self.get_collection("embeddings")
        await embeddings.create_index([("document_id", 1)])
        await embeddings.create_index([("vector", "2dsphere")])  # For vector similarity
    
    async def backup_collection(self, collection_name: str, backup_path: str):
        """Backup MongoDB collection"""
        # Implementation for mongodump
        pass
    
    async def restore_collection(self, collection_name: str, backup_path: str):
        """Restore MongoDB collection"""
        # Implementation for mongorestore
        pass


# Global database managers
postgres_manager = PostgreSQLManager()
mongodb_manager = MongoDBManager() 