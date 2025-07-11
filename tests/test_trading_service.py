import pytest
import asyncio
import json
from decimal import Decimal
from datetime import datetime
import asyncpg
import aioredis
from unittest.mock import AsyncMock, patch, MagicMock
import time

# Import the trading service components
import sys
import os
sys.path.append(os.path.dirname(os.path.dirname(__file__)))

from services.trading_service.main import (
    HighPerformanceTradingEngine, AccountManager, OrderManager,
    CreateOrder, CreateTradingAccount, OrderType, OrderSide, AccountType
)
from shared.config import settings
from shared.database import postgresql_manager, redis_manager

class TestHighPerformanceTradingEngine:
    """Test suite for the high-performance trading engine"""
    
    @pytest.fixture
    async def trading_engine(self):
        """Create trading engine instance for testing"""
        engine = HighPerformanceTradingEngine()
        # Mock the database and Redis connections
        with patch('shared.database.postgresql_manager.get_connection'), \
             patch('shared.database.redis_manager.set'), \
             patch('shared.messaging.hybrid_messaging_manager.publish_message'):
            await engine.initialize()
        return engine
    
    @pytest.mark.asyncio
    async def test_trading_engine_initialization(self, trading_engine):
        """Test trading engine initializes correctly"""
        assert trading_engine.active_orders == {}
        assert trading_engine.market_data_cache == {}
        assert trading_engine.processing_stats["orders_processed"] == 0
        assert isinstance(trading_engine.event_queue, asyncio.Queue)
    
    @pytest.mark.asyncio
    async def test_market_data_simulation(self, trading_engine):
        """Test market data simulation generates realistic data"""
        # Start market data simulation
        task = asyncio.create_task(trading_engine.market_data_simulator())
        
        # Wait a bit for data generation
        await asyncio.sleep(0.1)
        task.cancel()
        
        # Check that market data was generated
        assert len(trading_engine.market_data_cache) > 0
        
        # Verify data structure
        for symbol, data in trading_engine.market_data_cache.items():
            assert 'symbol' in data
            assert 'bid' in data
            assert 'ask' in data
            assert 'last' in data
            assert data['ask'] >= data['bid']  # Spread validation
    
    @pytest.mark.asyncio
    async def test_event_processing_performance(self, trading_engine):
        """Test event processing meets nanosecond precision requirements"""
        # Add test events
        test_events = []
        for i in range(100):
            test_events.append({
                "type": "market_data_update",
                "symbol": "EURUSD",
                "market_data": {
                    "symbol": "EURUSD",
                    "bid": Decimal("1.1000"),
                    "ask": Decimal("1.1002"),
                    "last": Decimal("1.1001"),
                    "volume": Decimal("1000"),
                    "high": Decimal("1.1005"),
                    "low": Decimal("1.0995"),
                    "change": Decimal("0.0001"),
                    "change_percent": Decimal("0.01"),
                    "timestamp": datetime.utcnow()
                },
                "timestamp_ns": time.perf_counter_ns()
            })
        
        # Process events and measure performance
        start_time = time.perf_counter_ns()
        
        for event in test_events:
            await trading_engine.event_queue.put(event)
        
        # Start event processor
        processor_task = asyncio.create_task(trading_engine.process_events())
        
        # Wait for processing
        await asyncio.sleep(0.1)
        processor_task.cancel()
        
        end_time = time.perf_counter_ns()
        total_time_ns = end_time - start_time
        
        # Verify performance requirements
        avg_time_per_event = total_time_ns / len(test_events)
        assert avg_time_per_event < 1_000_000  # Less than 1ms per event
        
        # Check processing stats
        stats = await trading_engine.get_performance_stats()
        assert stats["orders_processed"] > 0
        assert stats["avg_processing_time_ns"] > 0
    
    @pytest.mark.asyncio
    async def test_order_execution_latency(self, trading_engine):
        """Test order execution meets low-latency requirements"""
        # Mock order
        from services.trading_service.main import Order, OrderStatus
        
        order = Order(
            id="test-order-1",
            user_id="test-user",
            account_id="test-account",
            symbol="EURUSD",
            order_type=OrderType.MARKET,
            side=OrderSide.BUY,
            quantity=Decimal("1000"),
            price=None,
            stop_price=None,
            status=OrderStatus.PENDING,
            created_at=datetime.utcnow(),
            updated_at=datetime.utcnow(),
            executed_at=None
        )
        
        # Add to active orders
        trading_engine.active_orders[order.id] = order
        
        # Mock database operations
        with patch('shared.database.postgresql_manager.get_connection') as mock_conn:
            mock_conn.return_value.__aenter__.return_value.fetchrow.return_value = {
                'id': order.id,
                'status': 'filled'
            }
            
            # Test order execution
            execution_event = {
                "type": "order_execution",
                "order": order,
                "execution_price": Decimal("1.1001"),
                "timestamp_ns": time.perf_counter_ns()
            }
            
            start_time = time.perf_counter_ns()
            await trading_engine.handle_order_execution(execution_event)
            end_time = time.perf_counter_ns()
            
            execution_latency = end_time - start_time
            
            # Verify latency is under 1ms
            assert execution_latency < 1_000_000  # Less than 1ms
    
    @pytest.mark.asyncio
    async def test_high_frequency_data_processing(self, trading_engine):
        """Test system can handle high-frequency data updates"""
        # Simulate high-frequency updates (1000 updates/second)
        updates_per_second = 1000
        test_duration = 0.1  # 100ms test
        expected_updates = int(updates_per_second * test_duration)
        
        # Generate market data updates
        updates_processed = 0
        
        async def update_generator():
            nonlocal updates_processed
            for i in range(expected_updates):
                await trading_engine.event_queue.put({
                    "type": "market_data_update",
                    "symbol": "EURUSD",
                    "market_data": {
                        "symbol": "EURUSD",
                        "last": Decimal("1.1001") + Decimal(str(i * 0.0001)),
                        "timestamp": datetime.utcnow()
                    },
                    "timestamp_ns": time.perf_counter_ns()
                })
                updates_processed += 1
                await asyncio.sleep(test_duration / expected_updates)
        
        # Run test
        generator_task = asyncio.create_task(update_generator())
        processor_task = asyncio.create_task(trading_engine.process_events())
        
        await generator_task
        await asyncio.sleep(0.05)  # Allow processing to complete
        
        processor_task.cancel()
        
        # Verify all updates were processed
        assert updates_processed == expected_updates
        assert trading_engine.event_queue.qsize() < expected_updates * 0.1  # Most events processed

class TestAccountManager:
    """Test suite for account management"""
    
    @pytest.fixture
    async def account_manager(self):
        """Create account manager instance"""
        return AccountManager()
    
    @pytest.mark.asyncio
    async def test_create_trading_account(self, account_manager):
        """Test trading account creation"""
        account_data = CreateTradingAccount(
            account_type=AccountType.DEMO,
            initial_balance=Decimal("10000"),
            currency="USD",
            leverage=100,
            risk_level="medium"
        )
        
        # Mock database
        with patch('shared.database.postgresql_manager.get_connection') as mock_conn:
            mock_result = {
                'id': 'test-account-id',
                'user_id': 'test-user',
                'account_type': 'demo',
                'balance': Decimal('10000'),
                'available_balance': Decimal('10000'),
                'equity': Decimal('10000'),
                'margin': Decimal('0'),
                'free_margin': Decimal('10000'),
                'margin_level': Decimal('100'),
                'risk_level': 'medium',
                'currency': 'USD',
                'leverage': 100,
                'is_active': True,
                'created_at': datetime.utcnow(),
                'updated_at': datetime.utcnow()
            }
            
            mock_conn.return_value.__aenter__.return_value.fetchrow.return_value = mock_result
            
            result = await account_manager.create_account("test-user", account_data)
            
            assert result.id == 'test-account-id'
            assert result.account_type == AccountType.DEMO
            assert result.balance == Decimal('10000')
            assert result.leverage == 100
    
    @pytest.mark.asyncio
    async def test_portfolio_calculation(self, account_manager):
        """Test portfolio value calculation"""
        # Mock account and positions data
        with patch('shared.database.postgresql_manager.get_connection') as mock_conn:
            account_data = {
                'id': 'test-account',
                'user_id': 'test-user',
                'balance': Decimal('10000'),
                'available_balance': Decimal('8000'),
                'equity': Decimal('10500'),
                'margin': Decimal('2000'),
                'free_margin': Decimal('8500'),
                'margin_level': Decimal('525'),
                'risk_level': 'medium',
                'currency': 'USD',
                'leverage': 100,
                'is_active': True,
                'account_type': 'demo',
                'created_at': datetime.utcnow(),
                'updated_at': datetime.utcnow()
            }
            
            positions_data = [
                {
                    'id': 'pos-1',
                    'account_id': 'test-account',
                    'symbol': 'EURUSD',
                    'side': 'buy',
                    'quantity': Decimal('10000'),
                    'average_price': Decimal('1.1000'),
                    'current_price': Decimal('1.1050'),
                    'unrealized_pnl': Decimal('50'),
                    'realized_pnl': Decimal('0'),
                    'commission': Decimal('10'),
                    'opened_at': datetime.utcnow(),
                    'updated_at': datetime.utcnow()
                }
            ]
            
            mock_conn.return_value.__aenter__.return_value.fetchrow.return_value = account_data
            mock_conn.return_value.__aenter__.return_value.fetch.return_value = positions_data
            
            # Mock trading engine market data
            with patch('services.trading_service.main.trading_engine') as mock_engine:
                mock_engine.market_data_cache = {
                    'EURUSD': {'last': Decimal('1.1050')}
                }
                
                portfolio = await account_manager.get_portfolio('test-account')
                
                assert portfolio.account_id == 'test-account'
                assert portfolio.cash_balance == Decimal('8000')
                assert len(portfolio.positions) == 1
                assert portfolio.positions[0].unrealized_pnl == Decimal('50')

class TestOrderManager:
    """Test suite for order management"""
    
    @pytest.fixture
    async def order_manager(self):
        """Create order manager instance"""
        return OrderManager()
    
    @pytest.mark.asyncio
    async def test_create_market_order(self, order_manager):
        """Test market order creation"""
        order_data = CreateOrder(
            account_id="test-account",
            symbol="EURUSD",
            order_type=OrderType.MARKET,
            side=OrderSide.BUY,
            quantity=Decimal("1000")
        )
        
        # Mock database operations
        with patch('shared.database.postgresql_manager.get_connection') as mock_conn:
            # Mock account validation
            account_data = {
                'id': 'test-account',
                'user_id': 'test-user',
                'available_balance': Decimal('10000')
            }
            
            # Mock order creation
            order_result = {
                'id': 'test-order-id',
                'user_id': 'test-user',
                'account_id': 'test-account',
                'symbol': 'EURUSD',
                'order_type': 'market',
                'side': 'buy',
                'quantity': Decimal('1000'),
                'price': None,
                'stop_price': None,
                'status': 'pending',
                'filled_quantity': Decimal('0'),
                'average_price': None,
                'commission': Decimal('0'),
                'created_at': datetime.utcnow(),
                'updated_at': datetime.utcnow(),
                'executed_at': None
            }
            
            mock_conn.return_value.__aenter__.return_value.fetchrow.side_effect = [
                account_data,  # Account validation
                order_result   # Order creation
            ]
            
            # Mock trading engine
            with patch('services.trading_service.main.trading_engine') as mock_engine:
                mock_engine.active_orders = {}
                
                result = await order_manager.create_order("test-user", order_data)
                
                assert result.id == 'test-order-id'
                assert result.symbol == 'EURUSD'
                assert result.order_type == OrderType.MARKET
                assert result.side == OrderSide.BUY
                assert result.quantity == Decimal('1000')
    
    @pytest.mark.asyncio
    async def test_order_validation(self, order_manager):
        """Test order validation logic"""
        # Test insufficient balance
        order_data = CreateOrder(
            account_id="test-account",
            symbol="EURUSD",
            order_type=OrderType.MARKET,
            side=OrderSide.BUY,
            quantity=Decimal("100000")  # Large quantity
        )
        
        with patch('shared.database.postgresql_manager.get_connection') as mock_conn:
            account_data = {
                'id': 'test-account',
                'user_id': 'test-user',
                'available_balance': Decimal('1000')  # Insufficient balance
            }
            
            mock_conn.return_value.__aenter__.return_value.fetchrow.return_value = account_data
            
            # Should raise HTTPException for insufficient balance
            with pytest.raises(Exception):  # HTTPException
                await order_manager.create_order("test-user", order_data)

class TestPerformanceBenchmarks:
    """Performance benchmark tests"""
    
    @pytest.mark.asyncio
    async def test_order_creation_performance(self):
        """Benchmark order creation performance"""
        order_manager = OrderManager()
        
        # Create multiple orders and measure time
        num_orders = 1000
        orders_data = []
        
        for i in range(num_orders):
            orders_data.append(CreateOrder(
                account_id=f"account-{i}",
                symbol="EURUSD",
                order_type=OrderType.MARKET,
                side=OrderSide.BUY,
                quantity=Decimal("1000")
            ))
        
        # Mock database operations for performance test
        with patch('shared.database.postgresql_manager.get_connection') as mock_conn:
            mock_conn.return_value.__aenter__.return_value.fetchrow.side_effect = [
                # Account validation responses
                {'id': f'account-{i}', 'user_id': 'test-user', 'available_balance': Decimal('10000')}
                for i in range(num_orders)
            ] + [
                # Order creation responses
                {
                    'id': f'order-{i}',
                    'user_id': 'test-user',
                    'account_id': f'account-{i}',
                    'symbol': 'EURUSD',
                    'order_type': 'market',
                    'side': 'buy',
                    'quantity': Decimal('1000'),
                    'status': 'pending',
                    'created_at': datetime.utcnow(),
                    'updated_at': datetime.utcnow()
                }
                for i in range(num_orders)
            ]
            
            with patch('services.trading_service.main.trading_engine') as mock_engine:
                mock_engine.active_orders = {}
                
                start_time = time.perf_counter()
                
                # Create orders
                tasks = []
                for i, order_data in enumerate(orders_data):
                    tasks.append(order_manager.create_order("test-user", order_data))
                
                results = await asyncio.gather(*tasks)
                
                end_time = time.perf_counter()
                total_time = end_time - start_time
                
                # Performance assertions
                orders_per_second = num_orders / total_time
                avg_time_per_order = total_time / num_orders
                
                print(f"Created {num_orders} orders in {total_time:.3f}s")
                print(f"Orders per second: {orders_per_second:.1f}")
                print(f"Average time per order: {avg_time_per_order*1000:.2f}ms")
                
                # Assert performance requirements
                assert orders_per_second > 100  # At least 100 orders per second
                assert avg_time_per_order < 0.1  # Less than 100ms per order
                assert len(results) == num_orders
    
    @pytest.mark.asyncio
    async def test_market_data_processing_throughput(self):
        """Test market data processing throughput"""
        trading_engine = HighPerformanceTradingEngine()
        
        # Test processing 10,000 market data updates
        num_updates = 10000
        
        with patch('shared.database.postgresql_manager.get_connection'), \
             patch('shared.database.redis_manager.set'), \
             patch('shared.messaging.hybrid_messaging_manager.publish_message'):
            
            await trading_engine.initialize()
            
            # Generate market data updates
            start_time = time.perf_counter()
            
            for i in range(num_updates):
                await trading_engine.event_queue.put({
                    "type": "market_data_update",
                    "symbol": f"PAIR{i % 10}",  # 10 different symbols
                    "market_data": {
                        "symbol": f"PAIR{i % 10}",
                        "last": Decimal("1.0000") + Decimal(str(i * 0.0001)),
                        "bid": Decimal("0.9999") + Decimal(str(i * 0.0001)),
                        "ask": Decimal("1.0001") + Decimal(str(i * 0.0001)),
                        "timestamp": datetime.utcnow()
                    },
                    "timestamp_ns": time.perf_counter_ns()
                })
            
            # Process events
            processor_task = asyncio.create_task(trading_engine.process_events())
            
            # Wait for queue to empty
            while trading_engine.event_queue.qsize() > 0:
                await asyncio.sleep(0.001)
            
            end_time = time.perf_counter()
            processor_task.cancel()
            
            total_time = end_time - start_time
            updates_per_second = num_updates / total_time
            
            print(f"Processed {num_updates} market data updates in {total_time:.3f}s")
            print(f"Updates per second: {updates_per_second:.1f}")
            
            # Assert performance requirements
            assert updates_per_second > 1000  # At least 1000 updates per second

# Integration Tests
class TestIntegration:
    """Integration tests for complete trading workflows"""
    
    @pytest.mark.asyncio
    async def test_complete_trading_workflow(self):
        """Test complete trading workflow from account creation to order execution"""
        # This would test the entire flow in a real environment
        # For now, we'll test the component interactions
        
        account_manager = AccountManager()
        order_manager = OrderManager()
        trading_engine = HighPerformanceTradingEngine()
        
        # Mock all database operations
        with patch('shared.database.postgresql_manager.get_connection') as mock_conn, \
             patch('shared.database.redis_manager.set'), \
             patch('shared.messaging.hybrid_messaging_manager.publish_message'):
            
            # Mock account creation
            account_data = {
                'id': 'test-account',
                'user_id': 'test-user',
                'account_type': 'demo',
                'balance': Decimal('10000'),
                'available_balance': Decimal('10000'),
                'created_at': datetime.utcnow(),
                'updated_at': datetime.utcnow()
            }
            
            mock_conn.return_value.__aenter__.return_value.fetchrow.return_value = account_data
            
            # Initialize trading engine
            await trading_engine.initialize()
            
            # Create account
            create_account_data = CreateTradingAccount(
                account_type=AccountType.DEMO,
                initial_balance=Decimal("10000")
            )
            
            account = await account_manager.create_account("test-user", create_account_data)
            assert account.id == 'test-account'
            
            # Create order
            order_data = CreateOrder(
                account_id=account.id,
                symbol="EURUSD",
                order_type=OrderType.MARKET,
                side=OrderSide.BUY,
                quantity=Decimal("1000")
            )
            
            # Mock order creation response
            order_result = {
                'id': 'test-order',
                'user_id': 'test-user',
                'account_id': account.id,
                'symbol': 'EURUSD',
                'order_type': 'market',
                'side': 'buy',
                'quantity': Decimal('1000'),
                'status': 'pending',
                'created_at': datetime.utcnow(),
                'updated_at': datetime.utcnow()
            }
            
            mock_conn.return_value.__aenter__.return_value.fetchrow.side_effect = [
                account_data,  # Account validation
                order_result   # Order creation
            ]
            
            order = await order_manager.create_order("test-user", order_data)
            assert order.symbol == "EURUSD"
            assert order.quantity == Decimal("1000")
            
            # Verify order is in trading engine
            assert order.id in trading_engine.active_orders

if __name__ == "__main__":
    # Run specific performance tests
    pytest.main([__file__ + "::TestPerformanceBenchmarks", "-v", "-s"]) 