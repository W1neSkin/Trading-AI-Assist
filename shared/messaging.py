"""
Shared messaging utilities for RabbitMQ, AWS SQS, and AWS SNS
"""
import json
import asyncio
from typing import Dict, Any, Optional, Callable, List
from abc import ABC, abstractmethod
import logging

import pika
import boto3
from aioboto3 import Session

from .config import settings

logger = logging.getLogger(__name__)


class MessageBroker(ABC):
    """Abstract base class for message brokers"""
    
    @abstractmethod
    async def publish(self, topic: str, message: Dict[str, Any]) -> bool:
        """Publish a message to a topic"""
        pass
    
    @abstractmethod
    async def subscribe(self, topic: str, callback: Callable) -> None:
        """Subscribe to a topic with a callback"""
        pass
    
    @abstractmethod
    async def close(self) -> None:
        """Close the connection"""
        pass


class RabbitMQManager(MessageBroker):
    """RabbitMQ message broker implementation"""
    
    def __init__(self):
        self.connection = None
        self.channel = None
        self.callbacks = {}
    
    async def connect(self):
        """Establish RabbitMQ connection"""
        try:
            # Parse RabbitMQ URL
            import pika
            from urllib.parse import urlparse
            
            parsed = urlparse(settings.rabbitmq_url)
            credentials = pika.PlainCredentials(parsed.username, parsed.password)
            parameters = pika.ConnectionParameters(
                host=parsed.hostname,
                port=parsed.port or 5672,
                credentials=credentials,
                heartbeat=600,
                blocked_connection_timeout=300
            )
            
            self.connection = pika.BlockingConnection(parameters)
            self.channel = self.connection.channel()
            
            # Declare exchanges and queues
            await self._setup_exchanges_and_queues()
            
            logger.info("Connected to RabbitMQ")
            
        except Exception as e:
            logger.error(f"Failed to connect to RabbitMQ: {e}")
            raise
    
    async def _setup_exchanges_and_queues(self):
        """Setup RabbitMQ exchanges and queues"""
        # Trading events exchange
        self.channel.exchange_declare(
            exchange='trading.events',
            exchange_type='topic',
            durable=True
        )
        
        # Payment events exchange
        self.channel.exchange_declare(
            exchange='payment.events',
            exchange_type='topic',
            durable=True
        )
        
        # AI processing exchange
        self.channel.exchange_declare(
            exchange='ai.processing',
            exchange_type='topic',
            durable=True
        )
        
        # Notification exchange
        self.channel.exchange_declare(
            exchange='notifications',
            exchange_type='fanout',
            durable=True
        )
        
        # Dead letter exchange
        self.channel.exchange_declare(
            exchange='dead.letter',
            exchange_type='direct',
            durable=True
        )
        
        # Document processing queues
        self._declare_queue_with_dlx('document.processing', 'ai.processing', 'document.process')
        self._declare_queue_with_dlx('document.completed', 'ai.processing', 'document.completed')
        
        # Trading queues
        self._declare_queue_with_dlx('trading.orders', 'trading.events', 'order.*')
        self._declare_queue_with_dlx('trading.positions', 'trading.events', 'position.*')
        
        # Payment queues
        self._declare_queue_with_dlx('payment.webhooks', 'payment.events', 'stripe.*')
        self._declare_queue_with_dlx('payment.billing', 'payment.events', 'billing.*')
        
        # Notification queues
        self._declare_queue_with_dlx('notifications.email', 'notifications', '')
        self._declare_queue_with_dlx('notifications.push', 'notifications', '')
    
    def _declare_queue_with_dlx(self, queue_name: str, exchange: str, routing_key: str):
        """Declare queue with dead letter exchange"""
        # Main queue
        self.channel.queue_declare(
            queue=queue_name,
            durable=True,
            arguments={
                'x-dead-letter-exchange': 'dead.letter',
                'x-dead-letter-routing-key': f'{queue_name}.dead',
                'x-message-ttl': 3600000  # 1 hour TTL
            }
        )
        
        # Dead letter queue
        self.channel.queue_declare(
            queue=f'{queue_name}.dead',
            durable=True
        )
        
        # Bind queues
        self.channel.queue_bind(
            exchange=exchange,
            queue=queue_name,
            routing_key=routing_key
        )
        
        self.channel.queue_bind(
            exchange='dead.letter',
            queue=f'{queue_name}.dead',
            routing_key=f'{queue_name}.dead'
        )
    
    async def publish(self, exchange: str, routing_key: str, message: Dict[str, Any]) -> bool:
        """Publish message to RabbitMQ"""
        try:
            if not self.channel:
                await self.connect()
            
            self.channel.basic_publish(
                exchange=exchange,
                routing_key=routing_key,
                body=json.dumps(message),
                properties=pika.BasicProperties(
                    delivery_mode=2,  # Persistent message
                    content_type='application/json',
                    timestamp=int(asyncio.get_event_loop().time())
                )
            )
            
            logger.debug(f"Published message to {exchange}/{routing_key}")
            return True
            
        except Exception as e:
            logger.error(f"Failed to publish message: {e}")
            return False
    
    async def subscribe(self, queue: str, callback: Callable) -> None:
        """Subscribe to queue with callback"""
        try:
            if not self.channel:
                await self.connect()
            
            def wrapper(ch, method, properties, body):
                try:
                    message = json.loads(body)
                    callback(message)
                    ch.basic_ack(delivery_tag=method.delivery_tag)
                except Exception as e:
                    logger.error(f"Error processing message: {e}")
                    ch.basic_nack(delivery_tag=method.delivery_tag, requeue=False)
            
            self.channel.basic_consume(
                queue=queue,
                on_message_callback=wrapper
            )
            
            self.callbacks[queue] = callback
            
        except Exception as e:
            logger.error(f"Failed to subscribe to queue {queue}: {e}")
            raise
    
    async def close(self):
        """Close RabbitMQ connection"""
        if self.connection and not self.connection.is_closed:
            self.connection.close()


class AWSMessagingManager:
    """AWS SQS and SNS messaging manager"""
    
    def __init__(self):
        self.session = Session()
        self.sqs_client = None
        self.sns_client = None
    
    async def initialize(self):
        """Initialize AWS clients"""
        self.sqs_client = await self.session.client(
            'sqs',
            region_name=settings.aws_region,
            aws_access_key_id=settings.aws_access_key_id,
            aws_secret_access_key=settings.aws_secret_access_key
        ).__aenter__()
        
        self.sns_client = await self.session.client(
            'sns',
            region_name=settings.aws_region,
            aws_access_key_id=settings.aws_access_key_id,
            aws_secret_access_key=settings.aws_secret_access_key
        ).__aenter__()
    
    async def send_sqs_message(self, queue_url: str, message: Dict[str, Any]) -> bool:
        """Send message to SQS queue"""
        try:
            if not self.sqs_client:
                await self.initialize()
            
            response = await self.sqs_client.send_message(
                QueueUrl=queue_url,
                MessageBody=json.dumps(message),
                MessageAttributes={
                    'Source': {
                        'StringValue': 'ai-trading-platform',
                        'DataType': 'String'
                    }
                }
            )
            
            logger.debug(f"Sent SQS message: {response['MessageId']}")
            return True
            
        except Exception as e:
            logger.error(f"Failed to send SQS message: {e}")
            return False
    
    async def receive_sqs_messages(self, queue_url: str, max_messages: int = 10) -> List[Dict]:
        """Receive messages from SQS queue"""
        try:
            if not self.sqs_client:
                await self.initialize()
            
            response = await self.sqs_client.receive_message(
                QueueUrl=queue_url,
                MaxNumberOfMessages=max_messages,
                WaitTimeSeconds=20,  # Long polling
                MessageAttributeNames=['All']
            )
            
            return response.get('Messages', [])
            
        except Exception as e:
            logger.error(f"Failed to receive SQS messages: {e}")
            return []
    
    async def delete_sqs_message(self, queue_url: str, receipt_handle: str) -> bool:
        """Delete message from SQS queue"""
        try:
            if not self.sqs_client:
                await self.initialize()
            
            await self.sqs_client.delete_message(
                QueueUrl=queue_url,
                ReceiptHandle=receipt_handle
            )
            
            return True
            
        except Exception as e:
            logger.error(f"Failed to delete SQS message: {e}")
            return False
    
    async def publish_sns_message(self, topic_arn: str, message: Dict[str, Any], subject: str = None) -> bool:
        """Publish message to SNS topic"""
        try:
            if not self.sns_client:
                await self.initialize()
            
            kwargs = {
                'TopicArn': topic_arn,
                'Message': json.dumps(message),
                'MessageAttributes': {
                    'Source': {
                        'StringValue': 'ai-trading-platform',
                        'DataType': 'String'
                    }
                }
            }
            
            if subject:
                kwargs['Subject'] = subject
            
            response = await self.sns_client.publish(**kwargs)
            
            logger.debug(f"Published SNS message: {response['MessageId']}")
            return True
            
        except Exception as e:
            logger.error(f"Failed to publish SNS message: {e}")
            return False


class HybridMessagingManager:
    """Hybrid messaging manager combining RabbitMQ and AWS services"""
    
    def __init__(self):
        self.rabbitmq = RabbitMQManager()
        self.aws = AWSMessagingManager()
        self.initialized = False
    
    async def initialize(self):
        """Initialize all messaging services"""
        try:
            await asyncio.gather(
                self.rabbitmq.connect(),
                self.aws.initialize(),
                return_exceptions=True
            )
            self.initialized = True
            logger.info("Initialized hybrid messaging manager")
            
        except Exception as e:
            logger.error(f"Failed to initialize messaging manager: {e}")
            raise
    
    async def publish_event(self, event_type: str, data: Dict[str, Any], use_aws: bool = False):
        """Publish event using appropriate messaging system"""
        if use_aws and settings.sns_topic_arn:
            # Use SNS for AWS integrations
            await self.aws.publish_sns_message(
                settings.sns_topic_arn,
                {'event_type': event_type, 'data': data}
            )
        else:
            # Use RabbitMQ for internal communication
            exchange = self._get_exchange_for_event(event_type)
            routing_key = self._get_routing_key_for_event(event_type)
            
            await self.rabbitmq.publish(
                exchange=exchange,
                routing_key=routing_key,
                message={'event_type': event_type, 'data': data}
            )
    
    def _get_exchange_for_event(self, event_type: str) -> str:
        """Get appropriate exchange for event type"""
        event_mapping = {
            'order_created': 'trading.events',
            'order_filled': 'trading.events',
            'payment_received': 'payment.events',
            'document_processed': 'ai.processing',
            'user_notification': 'notifications'
        }
        return event_mapping.get(event_type, 'trading.events')
    
    def _get_routing_key_for_event(self, event_type: str) -> str:
        """Get appropriate routing key for event type"""
        return event_type.replace('_', '.')
    
    async def close(self):
        """Close all connections"""
        await asyncio.gather(
            self.rabbitmq.close(),
            return_exceptions=True
        )


# Global messaging manager
messaging_manager = HybridMessagingManager() 