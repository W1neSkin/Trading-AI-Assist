from fastapi import FastAPI, HTTPException, Depends, status, Security
from fastapi.security import HTTPBearer, HTTPAuthorizationCredentials
from fastapi.middleware.cors import CORSMiddleware
from sqlalchemy.ext.asyncio import AsyncSession
from sqlalchemy.orm import sessionmaker
from sqlalchemy import select, update, delete
from pydantic import BaseModel, EmailStr
from typing import List, Optional, Dict, Any
from datetime import datetime, timedelta
import jwt
import bcrypt
import uuid
import logging
from contextlib import asynccontextmanager
import asyncio

# Local imports
import sys
import os
sys.path.append(os.path.dirname(os.path.dirname(os.path.dirname(__file__))))

from shared.config import settings
from shared.database import postgresql_manager
from shared.messaging import hybrid_messaging_manager

# Configure logging
logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

# Security
security = HTTPBearer()

# Pydantic models
class UserCreate(BaseModel):
    email: EmailStr
    password: str
    first_name: str
    last_name: str
    role: str = "user"

class UserResponse(BaseModel):
    id: str
    email: str
    first_name: str
    last_name: str
    is_active: bool
    is_verified: bool
    role: str
    created_at: datetime
    updated_at: datetime

class UserLogin(BaseModel):
    email: EmailStr
    password: str

class TokenResponse(BaseModel):
    access_token: str
    token_type: str
    expires_in: int

class RolePermission(BaseModel):
    role: str
    permissions: List[str]

# User management class
class UserManager:
    def __init__(self):
        self.secret_key = settings.secret_key
        self.algorithm = "HS256"
        self.access_token_expire_minutes = 30

    async def hash_password(self, password: str) -> str:
        """Hash password using bcrypt"""
        return bcrypt.hashpw(password.encode('utf-8'), bcrypt.gensalt()).decode('utf-8')

    async def verify_password(self, password: str, hashed_password: str) -> bool:
        """Verify password against hash"""
        return bcrypt.checkpw(password.encode('utf-8'), hashed_password.encode('utf-8'))

    async def create_access_token(self, data: dict) -> str:
        """Create JWT access token"""
        to_encode = data.copy()
        expire = datetime.utcnow() + timedelta(minutes=self.access_token_expire_minutes)
        to_encode.update({"exp": expire})
        return jwt.encode(to_encode, self.secret_key, algorithm=self.algorithm)

    async def verify_token(self, token: str) -> dict:
        """Verify JWT access token"""
        try:
            payload = jwt.decode(token, self.secret_key, algorithms=[self.algorithm])
            return payload
        except jwt.ExpiredSignatureError:
            raise HTTPException(status_code=401, detail="Token expired")
        except jwt.JWTError:
            raise HTTPException(status_code=401, detail="Invalid token")

    async def create_user(self, user_data: UserCreate) -> UserResponse:
        """Create new user"""
        try:
            # Hash password
            hashed_password = await self.hash_password(user_data.password)
            
            # Create user record
            user_id = str(uuid.uuid4())
            query = """
                INSERT INTO users (id, email, password_hash, first_name, last_name, role)
                VALUES ($1, $2, $3, $4, $5, $6)
                RETURNING id, email, first_name, last_name, is_active, is_verified, role, created_at, updated_at
            """
            
            async with postgresql_manager.get_connection() as conn:
                result = await conn.fetchrow(
                    query, user_id, user_data.email, hashed_password,
                    user_data.first_name, user_data.last_name, user_data.role
                )
                
                if result:
                    # Send welcome notification
                    await hybrid_messaging_manager.publish_message(
                        exchange="notifications",
                        routing_key="user.created",
                        message={
                            "user_id": user_id,
                            "email": user_data.email,
                            "name": f"{user_data.first_name} {user_data.last_name}",
                            "event": "user_created"
                        }
                    )
                    
                    return UserResponse(**dict(result))
                    
        except Exception as e:
            logger.error(f"Error creating user: {e}")
            raise HTTPException(status_code=400, detail="Email already exists")

    async def authenticate_user(self, email: str, password: str) -> Optional[UserResponse]:
        """Authenticate user login"""
        query = """
            SELECT id, email, password_hash, first_name, last_name, is_active, is_verified, role, created_at, updated_at
            FROM users WHERE email = $1 AND is_active = true
        """
        
        async with postgresql_manager.get_connection() as conn:
            result = await conn.fetchrow(query, email)
            
            if result and await self.verify_password(password, result['password_hash']):
                return UserResponse(**{k: v for k, v in dict(result).items() if k != 'password_hash'})
            
            return None

    async def get_user(self, user_id: str) -> Optional[UserResponse]:
        """Get user by ID"""
        query = """
            SELECT id, email, first_name, last_name, is_active, is_verified, role, created_at, updated_at
            FROM users WHERE id = $1
        """
        
        async with postgresql_manager.get_connection() as conn:
            result = await conn.fetchrow(query, user_id)
            
            if result:
                return UserResponse(**dict(result))
            
            return None

    async def update_user(self, user_id: str, update_data: Dict[str, Any]) -> Optional[UserResponse]:
        """Update user information"""
        if not update_data:
            return await self.get_user(user_id)
            
        # Build dynamic update query
        set_clauses = []
        values = []
        param_count = 1
        
        for key, value in update_data.items():
            if key not in ['id', 'created_at', 'password_hash']:
                set_clauses.append(f"{key} = ${param_count}")
                values.append(value)
                param_count += 1
        
        if not set_clauses:
            return await self.get_user(user_id)
            
        set_clauses.append(f"updated_at = ${param_count}")
        values.append(datetime.utcnow())
        values.append(user_id)  # WHERE clause parameter
        
        query = f"""
            UPDATE users SET {', '.join(set_clauses)}
            WHERE id = ${param_count + 1}
            RETURNING id, email, first_name, last_name, is_active, is_verified, role, created_at, updated_at
        """
        
        async with postgresql_manager.get_connection() as conn:
            result = await conn.fetchrow(query, *values)
            
            if result:
                return UserResponse(**dict(result))
            
            return None

# RBAC implementation
class RBACManager:
    def __init__(self):
        self.permissions = {
            "admin": [
                "user.create", "user.read", "user.update", "user.delete",
                "trading.create", "trading.read", "trading.update", "trading.delete",
                "payment.create", "payment.read", "payment.update", "payment.delete",
                "document.create", "document.read", "document.update", "document.delete",
                "system.monitor", "system.configure"
            ],
            "trader": [
                "user.read", "user.update",
                "trading.create", "trading.read", "trading.update",
                "payment.read", "payment.create",
                "document.read", "document.create"
            ],
            "user": [
                "user.read", "user.update",
                "trading.read",
                "payment.read",
                "document.read"
            ]
        }

    def has_permission(self, user_role: str, permission: str) -> bool:
        """Check if user role has specific permission"""
        return permission in self.permissions.get(user_role, [])

    def get_role_permissions(self, role: str) -> List[str]:
        """Get all permissions for a role"""
        return self.permissions.get(role, [])

# Initialize managers
user_manager = UserManager()
rbac_manager = RBACManager()

# Dependency functions
async def get_current_user(credentials: HTTPAuthorizationCredentials = Security(security)) -> UserResponse:
    """Get current authenticated user"""
    try:
        token = credentials.credentials
        payload = await user_manager.verify_token(token)
        user_id = payload.get("sub")
        
        if not user_id:
            raise HTTPException(status_code=401, detail="Invalid token")
            
        user = await user_manager.get_user(user_id)
        if not user:
            raise HTTPException(status_code=401, detail="User not found")
            
        return user
        
    except Exception as e:
        logger.error(f"Authentication error: {e}")
        raise HTTPException(status_code=401, detail="Invalid authentication")

def require_permission(permission: str):
    """Decorator to require specific permission"""
    def permission_dependency(current_user: UserResponse = Depends(get_current_user)):
        if not rbac_manager.has_permission(current_user.role, permission):
            raise HTTPException(
                status_code=403,
                detail=f"Insufficient permissions. Required: {permission}"
            )
        return current_user
    return permission_dependency

# Lifespan context manager
@asynccontextmanager
async def lifespan(app: FastAPI):
    # Startup
    logger.info("Starting User Service...")
    
    # Initialize database connection
    await postgresql_manager.initialize()
    
    # Initialize messaging
    await hybrid_messaging_manager.initialize()
    
    # Create database tables
    await create_tables()
    
    logger.info("User Service started successfully")
    
    yield
    
    # Shutdown
    logger.info("Shutting down User Service...")
    await postgresql_manager.close()
    await hybrid_messaging_manager.close()

# Create FastAPI app
app = FastAPI(
    title="User Service",
    description="User management, authentication, and RBAC",
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
        CREATE TABLE IF NOT EXISTS users (
            id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
            email VARCHAR(255) UNIQUE NOT NULL,
            password_hash VARCHAR(255) NOT NULL,
            first_name VARCHAR(100) NOT NULL,
            last_name VARCHAR(100) NOT NULL,
            is_active BOOLEAN DEFAULT true,
            is_verified BOOLEAN DEFAULT false,
            role VARCHAR(50) DEFAULT 'user',
            created_at TIMESTAMP DEFAULT NOW(),
            updated_at TIMESTAMP DEFAULT NOW()
        )
        """,
        """
        CREATE INDEX IF NOT EXISTS idx_users_email ON users(email);
        """,
        """
        CREATE INDEX IF NOT EXISTS idx_users_role ON users(role);
        """,
        """
        CREATE TABLE IF NOT EXISTS user_sessions (
            id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
            user_id UUID REFERENCES users(id) ON DELETE CASCADE,
            token_hash VARCHAR(255) NOT NULL,
            expires_at TIMESTAMP NOT NULL,
            created_at TIMESTAMP DEFAULT NOW()
        )
        """,
        """
        CREATE INDEX IF NOT EXISTS idx_user_sessions_user_id ON user_sessions(user_id);
        """,
        """
        CREATE INDEX IF NOT EXISTS idx_user_sessions_expires_at ON user_sessions(expires_at);
        """
    ]
    
    async with postgresql_manager.get_connection() as conn:
        for query in queries:
            await conn.execute(query)

# API Routes
@app.get("/health")
async def health_check():
    """Health check endpoint"""
    return {"status": "healthy", "service": "user-service"}

@app.post("/register", response_model=UserResponse)
async def register_user(user_data: UserCreate):
    """Register new user"""
    return await user_manager.create_user(user_data)

@app.post("/login", response_model=TokenResponse)
async def login_user(login_data: UserLogin):
    """Login user"""
    user = await user_manager.authenticate_user(login_data.email, login_data.password)
    
    if not user:
        raise HTTPException(status_code=401, detail="Invalid credentials")
    
    # Create access token
    token_data = {"sub": user.id, "email": user.email, "role": user.role}
    access_token = await user_manager.create_access_token(token_data)
    
    return TokenResponse(
        access_token=access_token,
        token_type="bearer",
        expires_in=user_manager.access_token_expire_minutes * 60
    )

@app.get("/me", response_model=UserResponse)
async def get_current_user_info(current_user: UserResponse = Depends(get_current_user)):
    """Get current user information"""
    return current_user

@app.put("/me", response_model=UserResponse)
async def update_current_user(
    update_data: Dict[str, Any],
    current_user: UserResponse = Depends(get_current_user)
):
    """Update current user information"""
    # Remove sensitive fields
    update_data.pop('password', None)
    update_data.pop('password_hash', None)
    update_data.pop('id', None)
    update_data.pop('created_at', None)
    
    return await user_manager.update_user(current_user.id, update_data)

@app.get("/users/{user_id}", response_model=UserResponse)
async def get_user_by_id(
    user_id: str,
    current_user: UserResponse = Depends(require_permission("user.read"))
):
    """Get user by ID (admin only)"""
    user = await user_manager.get_user(user_id)
    if not user:
        raise HTTPException(status_code=404, detail="User not found")
    return user

@app.get("/users", response_model=List[UserResponse])
async def list_users(
    skip: int = 0,
    limit: int = 100,
    current_user: UserResponse = Depends(require_permission("user.read"))
):
    """List users (admin only)"""
    query = """
        SELECT id, email, first_name, last_name, is_active, is_verified, role, created_at, updated_at
        FROM users
        ORDER BY created_at DESC
        LIMIT $1 OFFSET $2
    """
    
    async with postgresql_manager.get_connection() as conn:
        results = await conn.fetch(query, limit, skip)
        return [UserResponse(**dict(row)) for row in results]

@app.get("/roles/{role}/permissions", response_model=RolePermission)
async def get_role_permissions(
    role: str,
    current_user: UserResponse = Depends(require_permission("system.monitor"))
):
    """Get permissions for a role"""
    permissions = rbac_manager.get_role_permissions(role)
    return RolePermission(role=role, permissions=permissions)

@app.post("/verify/{user_id}")
async def verify_user(
    user_id: str,
    current_user: UserResponse = Depends(require_permission("user.update"))
):
    """Verify user account (admin only)"""
    user = await user_manager.update_user(user_id, {"is_verified": True})
    if not user:
        raise HTTPException(status_code=404, detail="User not found")
    
    # Send verification notification
    await hybrid_messaging_manager.publish_message(
        exchange="notifications",
        routing_key="user.verified",
        message={
            "user_id": user_id,
            "email": user.email,
            "event": "user_verified"
        }
    )
    
    return {"message": "User verified successfully"}

@app.delete("/users/{user_id}")
async def delete_user(
    user_id: str,
    current_user: UserResponse = Depends(require_permission("user.delete"))
):
    """Delete user (admin only)"""
    query = "DELETE FROM users WHERE id = $1 RETURNING id"
    
    async with postgresql_manager.get_connection() as conn:
        result = await conn.fetchrow(query, user_id)
        
        if not result:
            raise HTTPException(status_code=404, detail="User not found")
    
    return {"message": "User deleted successfully"}

if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="0.0.0.0", port=8001) 