from fastapi import FastAPI, HTTPException, Depends, BackgroundTasks
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel, validator
from typing import List, Optional, Dict, Any
from datetime import datetime, timedelta
from decimal import Decimal
from enum import Enum
import uuid
import asyncio
import logging
from contextlib import asynccontextmanager
import json
import time

# Local imports
import sys
import os
sys.path.append(os.path.dirname(os.path.dirname(os.path.dirname(__file__))))

from shared.config import settings
from shared.database import postgresql_manager, redis_manager
from shared.messaging import hybrid_messaging_manager

# Configure logging
logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

# Enums
class OrderType(str, Enum):
    MARKET = "market"
    LIMIT = "limit"
    STOP = "stop"
    STOP_LIMIT = "stop_limit"

class OrderSide(str, Enum):
    BUY = "buy"
    SELL = "sell"

class OrderStatus(str, Enum):
    PENDING = "pending"
    OPEN = "open"
    FILLED = "filled"
    PARTIALLY_FILLED = "partially_filled"
    CANCELLED = "cancelled"
    REJECTED = "rejected"

class AccountType(str, Enum):
    DEMO = "demo"
    LIVE = "live"
    PAPER = "paper"

# Pydantic models
class TradingAccount(BaseModel):
    id: str
    user_id: str
    account_type: AccountType
    balance: Decimal
    available_balance: Decimal
    equity: Decimal
    margin: Decimal
    free_margin: Decimal
    margin_level: Decimal
    risk_level: str
    currency: str = "USD"
    leverage: int = 1
    is_active: bool = True
    created_at: datetime
    updated_at: datetime

class CreateTradingAccount(BaseModel):
    account_type: AccountType
    initial_balance: Decimal = Decimal('10000')
    currency: str = "USD"
    leverage: int = 1
    risk_level: str = "medium"

class Order(BaseModel):
    id: str
    user_id: str
    account_id: str
    symbol: str
    order_type: OrderType
    side: OrderSide
    quantity: Decimal
    price: Optional[Decimal]
    stop_price: Optional[Decimal]
    status: OrderStatus
    filled_quantity: Decimal = Decimal('0')
    average_price: Optional[Decimal]
    commission: Decimal = Decimal('0')
    created_at: datetime
    updated_at: datetime
    executed_at: Optional[datetime]

class CreateOrder(BaseModel):
    account_id: str
    symbol: str
    order_type: OrderType
    side: OrderSide
    quantity: Decimal
    price: Optional[Decimal] = None
    stop_price: Optional[Decimal] = None

    @validator('quantity')
    def quantity_must_be_positive(cls, v):
        if v <= 0:
            raise ValueError('Quantity must be positive')
        return v

class Position(BaseModel):
    id: str
    account_id: str
    symbol: str
    side: OrderSide
    quantity: Decimal
    average_price: Decimal
    current_price: Decimal
    unrealized_pnl: Decimal
    realized_pnl: Decimal
    commission: Decimal
    opened_at: datetime
    updated_at: datetime

class Portfolio(BaseModel):
    account_id: str
    total_value: Decimal
    cash_balance: Decimal
    positions_value: Decimal
    unrealized_pnl: Decimal
    realized_pnl: Decimal
    daily_return: Decimal
    total_return: Decimal
    positions: List[Position]
    updated_at: datetime

class MarketData(BaseModel):
    symbol: str
    bid: Decimal
    ask: Decimal
    last: Decimal
    volume: Decimal
    high: Decimal
    low: Decimal
    change: Decimal
    change_percent: Decimal
    timestamp: datetime

# High-Performance Trading Engine with nanosecond precision
class HighPerformanceTradingEngine:
    def __init__(self):
        self.active_orders = {}
        self.market_data_cache = {}
        self.event_queue = asyncio.Queue()
        self.processing_stats = {
            "orders_processed": 0,
            "avg_processing_time_ns": 0,
            "total_processing_time_ns": 0
        }
        
    async def initialize(self):
        """Initialize trading engine with high-performance components"""
        await self.load_active_orders()
        await self.start_market_data_feed()
        await self.start_event_processor()
        
    async def load_active_orders(self):
        """Load active orders from database"""
        query = """
            SELECT * FROM orders 
            WHERE status IN ('pending', 'open', 'partially_filled')
        """
        
        async with postgresql_manager.get_connection() as conn:
            results = await conn.fetch(query)
            
            for row in results:
                order_data = dict(row)
                self.active_orders[order_data['id']] = Order(**order_data)
                
        logger.info(f"Loaded {len(self.active_orders)} active orders")

    async def start_market_data_feed(self):
        """Start high-frequency market data feed"""
        asyncio.create_task(self.market_data_simulator())

    async def start_event_processor(self):
        """Start nanosecond-precision event processor"""
        asyncio.create_task(self.process_events())

    async def process_events(self):
        """Process trading events with nanosecond precision"""
        while True:
            try:
                event = await self.event_queue.get()
                start_time_ns = time.perf_counter_ns()
                
                await self.handle_event(event)
                
                end_time_ns = time.perf_counter_ns()
                processing_time_ns = end_time_ns - start_time_ns
                
                # Update performance statistics
                self.processing_stats["orders_processed"] += 1
                self.processing_stats["total_processing_time_ns"] += processing_time_ns
                self.processing_stats["avg_processing_time_ns"] = (
                    self.processing_stats["total_processing_time_ns"] / 
                    self.processing_stats["orders_processed"]
                )
                
                if processing_time_ns > 1_000_000:  # Log if > 1ms
                    logger.warning(f"Slow event processing: {processing_time_ns/1_000_000:.2f}ms")
                
            except Exception as e:
                logger.error(f"Error processing event: {e}")

    async def handle_event(self, event: Dict[str, Any]):
        """Handle trading event"""
        event_type = event.get("type")
        
        if event_type == "market_data_update":
            await self.handle_market_data_update(event)
        elif event_type == "order_execution":
            await self.handle_order_execution(event)
        elif event_type == "risk_check":
            await self.handle_risk_check(event)

    async def handle_market_data_update(self, event: Dict[str, Any]):
        """Handle market data update with nanosecond precision"""
        symbol = event["symbol"]
        market_data = event["market_data"]
        
        # Update cache
        self.market_data_cache[symbol] = market_data
        
        # Check for order execution opportunities
        await self.check_order_execution(symbol, MarketData(**market_data))

    async def market_data_simulator(self):
        """Simulate high-frequency market data"""
        symbols = ['EURUSD', 'GBPUSD', 'USDJPY', 'AUDUSD', 'USDCAD', 'BTCUSD', 'ETHUSD']
        
        while True:
            try:
                for symbol in symbols:
                    # Simulate realistic price movement with microsecond timestamps
                    current_price = self.market_data_cache.get(symbol, {}).get('last', Decimal('1.0000'))
                    
                    # Random price movement (more realistic simulation)
                    import random
                    volatility = 0.0001 if 'USD' in symbol else 0.01  # Different volatility for crypto
                    change = Decimal(str(random.uniform(-volatility, volatility)))
                    new_price = max(current_price + change, Decimal('0.0001'))
                    
                    spread = new_price * Decimal('0.0002')  # 2 pip spread
                    
                    market_data = {
                        "symbol": symbol,
                        "bid": new_price - spread,
                        "ask": new_price + spread,
                        "last": new_price,
                        "volume": Decimal(str(random.randint(1000, 50000))),
                        "high": new_price + Decimal(str(random.uniform(0, volatility))),
                        "low": new_price - Decimal(str(random.uniform(0, volatility))),
                        "change": change,
                        "change_percent": (change / current_price) * 100 if current_price > 0 else Decimal('0'),
                        "timestamp": datetime.utcnow()
                    }
                    
                    # Queue market data event
                    await self.event_queue.put({
                        "type": "market_data_update",
                        "symbol": symbol,
                        "market_data": market_data,
                        "timestamp_ns": time.perf_counter_ns()
                    })
                    
                    # Cache in Redis with nanosecond precision
                    await redis_manager.set(
                        f"market_data:{symbol}",
                        json.dumps(market_data, default=str),
                        expire=5
                    )
                
                # High-frequency updates (100 times per second)
                await asyncio.sleep(0.01)
                
            except Exception as e:
                logger.error(f"Market data simulation error: {e}")
                await asyncio.sleep(1)

    async def check_order_execution(self, symbol: str, market_data: MarketData):
        """Check if any orders should be executed with nanosecond precision"""
        orders_to_execute = []
        
        for order_id, order in self.active_orders.items():
            if order.symbol == symbol:
                should_execute = False
                execution_price = None
                
                if order.order_type == OrderType.MARKET:
                    should_execute = True
                    execution_price = market_data.ask if order.side == OrderSide.BUY else market_data.bid
                    
                elif order.order_type == OrderType.LIMIT:
                    if order.side == OrderSide.BUY and market_data.ask <= order.price:
                        should_execute = True
                        execution_price = order.price
                    elif order.side == OrderSide.SELL and market_data.bid >= order.price:
                        should_execute = True
                        execution_price = order.price
                        
                if should_execute:
                    # Queue execution event
                    await self.event_queue.put({
                        "type": "order_execution",
                        "order": order,
                        "execution_price": execution_price,
                        "timestamp_ns": time.perf_counter_ns()
                    })

    async def handle_order_execution(self, event: Dict[str, Any]):
        """Execute order with nanosecond precision tracking"""
        order = event["order"]
        execution_price = event["execution_price"]
        
        try:
            # Calculate commission (0.1% of trade value)
            trade_value = order.quantity * execution_price
            commission = trade_value * Decimal('0.001')
            
            # Update order status with nanosecond timestamp
            execution_timestamp = datetime.utcnow()
            
            query = """
                UPDATE orders SET 
                    status = 'filled',
                    filled_quantity = quantity,
                    average_price = $1,
                    commission = $2,
                    executed_at = $3,
                    updated_at = NOW()
                WHERE id = $4
                RETURNING *
            """
            
            async with postgresql_manager.get_connection() as conn:
                result = await conn.fetchrow(
                    query, execution_price, commission, execution_timestamp, order.id
                )
                
                if result:
                    # Remove from active orders
                    self.active_orders.pop(order.id, None)
                    
                    # Update account balance
                    await self.update_account_balance(order.account_id, order, execution_price, commission)
                    
                    # Create or update position
                    await self.update_position(order, execution_price, commission)
                    
                    # Publish execution event with nanosecond precision
                    await hybrid_messaging_manager.publish_message(
                        exchange="trading",
                        routing_key="order.executed",
                        message={
                            "order_id": order.id,
                            "user_id": order.user_id,
                            "account_id": order.account_id,
                            "symbol": order.symbol,
                            "side": order.side,
                            "quantity": str(order.quantity),
                            "execution_price": str(execution_price),
                            "commission": str(commission),
                            "executed_at": execution_timestamp.isoformat(),
                            "execution_timestamp_ns": event["timestamp_ns"],
                            "processing_latency_ns": time.perf_counter_ns() - event["timestamp_ns"]
                        }
                    )
                    
                    logger.info(f"Order {order.id} executed at {execution_price} (latency: {(time.perf_counter_ns() - event['timestamp_ns'])/1_000_000:.2f}ms)")
                    
        except Exception as e:
            logger.error(f"Error executing order {order.id}: {e}")

    async def update_account_balance(self, account_id: str, order: Order, execution_price: Decimal, commission: Decimal):
        """Update account balance after order execution"""
        trade_value = order.quantity * execution_price
        
        if order.side == OrderSide.BUY:
            balance_change = -(trade_value + commission)
        else:
            balance_change = trade_value - commission
            
        query = """
            UPDATE trading_accounts SET 
                available_balance = available_balance + $1,
                updated_at = NOW()
            WHERE id = $2
        """
        
        async with postgresql_manager.get_connection() as conn:
            await conn.execute(query, balance_change, account_id)

    async def update_position(self, order: Order, execution_price: Decimal, commission: Decimal):
        """Create or update position with high precision"""
        # Check for existing position
        query = """
            SELECT * FROM positions 
            WHERE account_id = $1 AND symbol = $2
        """
        
        async with postgresql_manager.get_connection() as conn:
            existing_position = await conn.fetchrow(query, order.account_id, order.symbol)
            
            if existing_position:
                # Update existing position
                await self.merge_position(existing_position, order, execution_price, commission)
            else:
                # Create new position
                await self.create_position(order, execution_price, commission)

    async def create_position(self, order: Order, execution_price: Decimal, commission: Decimal):
        """Create new position"""
        position_id = str(uuid.uuid4())
        
        query = """
            INSERT INTO positions (
                id, account_id, symbol, side, quantity, average_price,
                current_price, unrealized_pnl, realized_pnl, commission
            ) VALUES ($1, $2, $3, $4, $5, $6, $7, $8, $9, $10)
        """
        
        async with postgresql_manager.get_connection() as conn:
            await conn.execute(
                query, position_id, order.account_id, order.symbol,
                order.side, order.quantity, execution_price,
                execution_price, Decimal('0'), Decimal('0'), commission
            )

    async def merge_position(self, existing_position: dict, order: Order, execution_price: Decimal, commission: Decimal):
        """Merge order into existing position"""
        existing_quantity = existing_position['quantity']
        existing_avg_price = existing_position['average_price']
        existing_commission = existing_position['commission']
        
        if existing_position['side'] == order.side:
            # Same side - add to position
            new_quantity = existing_quantity + order.quantity
            new_avg_price = ((existing_quantity * existing_avg_price) + 
                           (order.quantity * execution_price)) / new_quantity
            new_commission = existing_commission + commission
            
            query = """
                UPDATE positions SET 
                    quantity = $1,
                    average_price = $2,
                    commission = $3,
                    updated_at = NOW()
                WHERE id = $4
            """
            
            async with postgresql_manager.get_connection() as conn:
                await conn.execute(
                    query, new_quantity, new_avg_price, new_commission,
                    existing_position['id']
                )
        else:
            # Opposite side - reduce or close position
            if order.quantity >= existing_quantity:
                # Close position completely
                realized_pnl = (execution_price - existing_avg_price) * existing_quantity
                if existing_position['side'] == OrderSide.SELL:
                    realized_pnl = -realized_pnl
                    
                query = "DELETE FROM positions WHERE id = $1"
                async with postgresql_manager.get_connection() as conn:
                    await conn.execute(query, existing_position['id'])
                    
                # If order quantity > position quantity, create new position for remainder
                if order.quantity > existing_quantity:
                    remaining_quantity = order.quantity - existing_quantity
                    await self.create_position(
                        Order(**{**order.dict(), 'quantity': remaining_quantity}),
                        execution_price, commission
                    )

    async def get_performance_stats(self) -> Dict[str, Any]:
        """Get trading engine performance statistics"""
        return {
            "orders_processed": self.processing_stats["orders_processed"],
            "avg_processing_time_ns": self.processing_stats["avg_processing_time_ns"],
            "avg_processing_time_ms": self.processing_stats["avg_processing_time_ns"] / 1_000_000,
            "active_orders": len(self.active_orders),
            "cached_symbols": len(self.market_data_cache),
            "event_queue_size": self.event_queue.qsize()
        }

# Account Manager
class AccountManager:
    async def create_account(self, user_id: str, account_data: CreateTradingAccount) -> TradingAccount:
        """Create new trading account"""
        account_id = str(uuid.uuid4())
        
        query = """
            INSERT INTO trading_accounts (
                id, user_id, account_type, balance, available_balance,
                equity, margin, free_margin, margin_level, risk_level,
                currency, leverage
            ) VALUES ($1, $2, $3, $4, $4, $4, $5, $4, $6, $7, $8, $9)
            RETURNING *
        """
        
        async with postgresql_manager.get_connection() as conn:
            result = await conn.fetchrow(
                query, account_id, user_id, account_data.account_type,
                account_data.initial_balance, Decimal('0'), Decimal('100'),
                account_data.risk_level, account_data.currency, account_data.leverage
            )
            
            if result:
                return TradingAccount(**dict(result))

    async def get_account(self, account_id: str) -> Optional[TradingAccount]:
        """Get trading account by ID"""
        query = "SELECT * FROM trading_accounts WHERE id = $1"
        
        async with postgresql_manager.get_connection() as conn:
            result = await conn.fetchrow(query, account_id)
            
            if result:
                return TradingAccount(**dict(result))

    async def get_user_accounts(self, user_id: str) -> List[TradingAccount]:
        """Get all accounts for a user"""
        query = "SELECT * FROM trading_accounts WHERE user_id = $1 ORDER BY created_at DESC"
        
        async with postgresql_manager.get_connection() as conn:
            results = await conn.fetch(query, user_id)
            
            return [TradingAccount(**dict(row)) for row in results]

    async def get_portfolio(self, account_id: str) -> Portfolio:
        """Get portfolio for account"""
        # Get account
        account = await self.get_account(account_id)
        if not account:
            raise HTTPException(status_code=404, detail="Account not found")
            
        # Get positions
        positions_query = "SELECT * FROM positions WHERE account_id = $1"
        
        async with postgresql_manager.get_connection() as conn:
            position_results = await conn.fetch(positions_query, account_id)
            
        positions = []
        positions_value = Decimal('0')
        unrealized_pnl = Decimal('0')
        realized_pnl = Decimal('0')
        
        for row in position_results:
            position_data = dict(row)
            
            # Update current price from market data
            symbol = position_data['symbol']
            market_data = trading_engine.market_data_cache.get(symbol)
            
            if market_data:
                current_price = Decimal(str(market_data['last']))
                position_data['current_price'] = current_price
                
                # Calculate unrealized PnL
                if position_data['side'] == OrderSide.BUY:
                    position_unrealized_pnl = (current_price - position_data['average_price']) * position_data['quantity']
                else:
                    position_unrealized_pnl = (position_data['average_price'] - current_price) * position_data['quantity']
                    
                position_data['unrealized_pnl'] = position_unrealized_pnl
                unrealized_pnl += position_unrealized_pnl
                
            positions_value += position_data['quantity'] * position_data['current_price']
            realized_pnl += position_data['realized_pnl']
            
            positions.append(Position(**position_data))
        
        total_value = account.available_balance + positions_value
        
        return Portfolio(
            account_id=account_id,
            total_value=total_value,
            cash_balance=account.available_balance,
            positions_value=positions_value,
            unrealized_pnl=unrealized_pnl,
            realized_pnl=realized_pnl,
            daily_return=Decimal('0'),  # Would calculate from historical data
            total_return=((total_value - account.balance) / account.balance) * 100 if account.balance > 0 else Decimal('0'),
            positions=positions,
            updated_at=datetime.utcnow()
        )

# Order Manager
class OrderManager:
    async def create_order(self, user_id: str, order_data: CreateOrder) -> Order:
        """Create new order with high-performance validation"""
        start_time_ns = time.perf_counter_ns()
        
        # Validate account ownership
        account_query = "SELECT * FROM trading_accounts WHERE id = $1 AND user_id = $2"
        
        async with postgresql_manager.get_connection() as conn:
            account = await conn.fetchrow(account_query, order_data.account_id, user_id)
            
            if not account:
                raise HTTPException(status_code=404, detail="Account not found")
            
            # Validate sufficient balance for buy orders
            if order_data.side == OrderSide.BUY:
                required_balance = order_data.quantity * (order_data.price or Decimal('1'))
                if account['available_balance'] < required_balance:
                    raise HTTPException(status_code=400, detail="Insufficient balance")
            
            # Create order
            order_id = str(uuid.uuid4())
            
            order_query = """
                INSERT INTO orders (
                    id, user_id, account_id, symbol, order_type, side,
                    quantity, price, stop_price, status
                ) VALUES ($1, $2, $3, $4, $5, $6, $7, $8, $9, $10)
                RETURNING *
            """
            
            result = await conn.fetchrow(
                order_query, order_id, user_id, order_data.account_id,
                order_data.symbol, order_data.order_type, order_data.side,
                order_data.quantity, order_data.price, order_data.stop_price,
                OrderStatus.PENDING
            )
            
            if result:
                order = Order(**dict(result))
                
                # Add to active orders in trading engine
                trading_engine.active_orders[order_id] = order
                
                # Reserve balance for buy orders
                if order_data.side == OrderSide.BUY:
                    balance_query = """
                        UPDATE trading_accounts SET 
                            available_balance = available_balance - $1
                        WHERE id = $2
                    """
                    await conn.execute(balance_query, required_balance, order_data.account_id)
                
                processing_time_ns = time.perf_counter_ns() - start_time_ns
                logger.info(f"Order created in {processing_time_ns/1_000_000:.2f}ms")
                
                return order

# Initialize services
trading_engine = HighPerformanceTradingEngine()
account_manager = AccountManager()
order_manager = OrderManager()

# Lifespan context manager
@asynccontextmanager
async def lifespan(app: FastAPI):
    # Startup
    logger.info("Starting High-Performance Trading Service...")
    
    # Initialize database connections
    await postgresql_manager.initialize()
    await redis_manager.initialize()
    
    # Initialize messaging
    await hybrid_messaging_manager.initialize()
    
    # Create database tables
    await create_tables()
    
    # Initialize high-performance trading engine
    await trading_engine.initialize()
    
    logger.info("High-Performance Trading Service started successfully")
    
    yield
    
    # Shutdown
    logger.info("Shutting down Trading Service...")
    await postgresql_manager.close()
    await redis_manager.close()
    await hybrid_messaging_manager.close()

# Create FastAPI app
app = FastAPI(
    title="High-Performance Trading Service",
    description="Nanosecond-precision trading operations, portfolio management, and market data",
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
        CREATE TABLE IF NOT EXISTS trading_accounts (
            id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
            user_id UUID NOT NULL,
            account_type VARCHAR(20) NOT NULL,
            balance DECIMAL(20,8) DEFAULT 0,
            available_balance DECIMAL(20,8) DEFAULT 0,
            equity DECIMAL(20,8) DEFAULT 0,
            margin DECIMAL(20,8) DEFAULT 0,
            free_margin DECIMAL(20,8) DEFAULT 0,
            margin_level DECIMAL(10,2) DEFAULT 100,
            risk_level VARCHAR(20) DEFAULT 'medium',
            currency VARCHAR(10) DEFAULT 'USD',
            leverage INTEGER DEFAULT 1,
            is_active BOOLEAN DEFAULT true,
            created_at TIMESTAMP DEFAULT NOW(),
            updated_at TIMESTAMP DEFAULT NOW()
        )
        """,
        """
        CREATE TABLE IF NOT EXISTS orders (
            id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
            user_id UUID NOT NULL,
            account_id UUID REFERENCES trading_accounts(id),
            symbol VARCHAR(20) NOT NULL,
            order_type VARCHAR(20) NOT NULL,
            side VARCHAR(10) NOT NULL,
            quantity DECIMAL(20,8) NOT NULL,
            price DECIMAL(20,8),
            stop_price DECIMAL(20,8),
            status VARCHAR(20) DEFAULT 'pending',
            filled_quantity DECIMAL(20,8) DEFAULT 0,
            average_price DECIMAL(20,8),
            commission DECIMAL(20,8) DEFAULT 0,
            created_at TIMESTAMP DEFAULT NOW(),
            updated_at TIMESTAMP DEFAULT NOW(),
            executed_at TIMESTAMP
        )
        """,
        """
        CREATE TABLE IF NOT EXISTS positions (
            id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
            account_id UUID REFERENCES trading_accounts(id),
            symbol VARCHAR(20) NOT NULL,
            side VARCHAR(10) NOT NULL,
            quantity DECIMAL(20,8) NOT NULL,
            average_price DECIMAL(20,8) NOT NULL,
            current_price DECIMAL(20,8) NOT NULL,
            unrealized_pnl DECIMAL(20,8) DEFAULT 0,
            realized_pnl DECIMAL(20,8) DEFAULT 0,
            commission DECIMAL(20,8) DEFAULT 0,
            opened_at TIMESTAMP DEFAULT NOW(),
            updated_at TIMESTAMP DEFAULT NOW()
        )
        """,
        # High-performance indexes
        "CREATE INDEX IF NOT EXISTS idx_trading_accounts_user_id ON trading_accounts(user_id);",
        "CREATE INDEX IF NOT EXISTS idx_orders_user_id ON orders(user_id);",
        "CREATE INDEX IF NOT EXISTS idx_orders_account_id ON orders(account_id);",
        "CREATE INDEX IF NOT EXISTS idx_orders_symbol ON orders(symbol);",
        "CREATE INDEX IF NOT EXISTS idx_orders_status ON orders(status);",
        "CREATE INDEX IF NOT EXISTS idx_positions_account_id ON positions(account_id);",
        "CREATE INDEX IF NOT EXISTS idx_positions_symbol ON positions(symbol);"
    ]
    
    async with postgresql_manager.get_connection() as conn:
        for query in queries:
            await conn.execute(query)

# API Routes
@app.get("/health")
async def health_check():
    """Health check endpoint"""
    return {"status": "healthy", "service": "high-performance-trading-service"}

# Performance monitoring endpoint
@app.get("/performance")
async def get_performance_stats():
    """Get trading engine performance statistics"""
    return await trading_engine.get_performance_stats()

# Account endpoints
@app.post("/accounts", response_model=TradingAccount)
async def create_account(account_data: CreateTradingAccount, user_id: str):
    """Create trading account"""
    return await account_manager.create_account(user_id, account_data)

@app.get("/accounts", response_model=List[TradingAccount])
async def get_accounts(user_id: str):
    """Get user accounts"""
    return await account_manager.get_user_accounts(user_id)

@app.get("/accounts/{account_id}", response_model=TradingAccount)
async def get_account(account_id: str, user_id: str):
    """Get account by ID"""
    account = await account_manager.get_account(account_id)
    if not account or account.user_id != user_id:
        raise HTTPException(status_code=404, detail="Account not found")
    return account

@app.get("/accounts/{account_id}/portfolio", response_model=Portfolio)
async def get_portfolio(account_id: str, user_id: str):
    """Get portfolio"""
    # Verify account ownership
    account = await account_manager.get_account(account_id)
    if not account or account.user_id != user_id:
        raise HTTPException(status_code=404, detail="Account not found")
    
    return await account_manager.get_portfolio(account_id)

# Order endpoints
@app.post("/orders", response_model=Order)
async def create_order(order_data: CreateOrder, user_id: str):
    """Create high-performance order"""
    return await order_manager.create_order(user_id, order_data)

# Market data endpoints
@app.get("/market-data/{symbol}", response_model=MarketData)
async def get_market_data(symbol: str):
    """Get real-time market data"""
    data = trading_engine.market_data_cache.get(symbol)
    if not data:
        raise HTTPException(status_code=404, detail="Symbol not found")
    
    return MarketData(**data)

@app.get("/market-data", response_model=List[MarketData])
async def get_all_market_data():
    """Get all real-time market data"""
    return [MarketData(**data) for data in trading_engine.market_data_cache.values()]

if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="0.0.0.0", port=8002) 