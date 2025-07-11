from aws_cdk import (
    Stack,
    aws_ec2 as ec2,
    aws_logs as logs,
    CfnOutput,
    Tags
)
from constructs import Construct
from typing import List

class VPCStack(Stack):
    """
    VPC Stack for AI Trading Platform
    
    Creates a production-ready VPC with:
    - Public and private subnets across multiple AZs
    - NAT Gateways for outbound connectivity
    - VPC Endpoints for AWS services
    - Security Groups for different service tiers
    - Flow logs for monitoring
    """
    
    def __init__(self, scope: Construct, construct_id: str, **kwargs) -> None:
        super().__init__(scope, construct_id, **kwargs)
        
        # Create VPC
        self.vpc = ec2.Vpc(
            self, "TradingPlatformVPC",
            ip_addresses=ec2.IpAddresses.cidr("10.0.0.0/16"),
            max_azs=3,
            nat_gateways=3,  # One per AZ for high availability
            subnet_configuration=[
                # Public subnets for load balancers and NAT gateways
                ec2.SubnetConfiguration(
                    name="PublicSubnet",
                    subnet_type=ec2.SubnetType.PUBLIC,
                    cidr_mask=24
                ),
                # Private subnets for application servers
                ec2.SubnetConfiguration(
                    name="PrivateSubnet",
                    subnet_type=ec2.SubnetType.PRIVATE_WITH_EGRESS,
                    cidr_mask=24
                ),
                # Isolated subnets for databases
                ec2.SubnetConfiguration(
                    name="DatabaseSubnet",
                    subnet_type=ec2.SubnetType.PRIVATE_ISOLATED,
                    cidr_mask=24
                )
            ],
            enable_dns_hostnames=True,
            enable_dns_support=True
        )
        
        # Create VPC Flow Logs
        self.flow_logs = ec2.FlowLog(
            self, "VPCFlowLogs",
            resource_type=ec2.FlowLogResourceType.from_vpc(self.vpc),
            destination=ec2.FlowLogDestination.to_cloud_watch_logs(
                logs.LogGroup(
                    self, "VPCFlowLogGroup",
                    log_group_name="/aws/vpc/flowlogs",
                    retention=logs.RetentionDays.ONE_MONTH
                )
            ),
            traffic_type=ec2.FlowLogTrafficType.ALL
        )
        
        # Create VPC Endpoints for AWS services to reduce NAT gateway costs
        self._create_vpc_endpoints()
        
        # Create Security Groups
        self._create_security_groups()
        
        # Add tags
        Tags.of(self).add("Project", "AI-Trading-Platform")
        Tags.of(self).add("Environment", self.node.try_get_context("environment") or "dev")
        
        # Outputs
        CfnOutput(
            self, "VPCId",
            value=self.vpc.vpc_id,
            description="VPC ID"
        )
        
        CfnOutput(
            self, "PublicSubnetIds",
            value=",".join([subnet.subnet_id for subnet in self.vpc.public_subnets]),
            description="Public subnet IDs"
        )
        
        CfnOutput(
            self, "PrivateSubnetIds",
            value=",".join([subnet.subnet_id for subnet in self.vpc.private_subnets]),
            description="Private subnet IDs"
        )
        
        CfnOutput(
            self, "IsolatedSubnetIds",
            value=",".join([subnet.subnet_id for subnet in self.vpc.isolated_subnets]),
            description="Isolated subnet IDs"
        )
    
    def _create_vpc_endpoints(self):
        """Create VPC endpoints for AWS services"""
        
        # S3 Gateway Endpoint
        self.vpc.add_gateway_endpoint(
            "S3Endpoint",
            service=ec2.GatewayVpcEndpointAwsService.S3,
            subnets=[ec2.SubnetSelection(subnet_type=ec2.SubnetType.PRIVATE_WITH_EGRESS)]
        )
        
        # DynamoDB Gateway Endpoint
        self.vpc.add_gateway_endpoint(
            "DynamoDBEndpoint",
            service=ec2.GatewayVpcEndpointAwsService.DYNAMODB,
            subnets=[ec2.SubnetSelection(subnet_type=ec2.SubnetType.PRIVATE_WITH_EGRESS)]
        )
        
        # Interface endpoints for other services
        interface_endpoints = [
            ("ECREndpoint", ec2.InterfaceVpcEndpointAwsService.ECR),
            ("ECRDockerEndpoint", ec2.InterfaceVpcEndpointAwsService.ECR_DOCKER),
            ("LogsEndpoint", ec2.InterfaceVpcEndpointAwsService.CLOUDWATCH_LOGS),
            ("MonitoringEndpoint", ec2.InterfaceVpcEndpointAwsService.CLOUDWATCH),
            ("SecretsManagerEndpoint", ec2.InterfaceVpcEndpointAwsService.SECRETS_MANAGER),
            ("SSMEndpoint", ec2.InterfaceVpcEndpointAwsService.SSM),
            ("SQSEndpoint", ec2.InterfaceVpcEndpointAwsService.SQS),
            ("SNSEndpoint", ec2.InterfaceVpcEndpointAwsService.SNS),
            ("LambdaEndpoint", ec2.InterfaceVpcEndpointAwsService.LAMBDA),
            ("ECSEndpoint", ec2.InterfaceVpcEndpointAwsService.ECS),
            ("ECSAgentEndpoint", ec2.InterfaceVpcEndpointAwsService.ECS_AGENT),
            ("ECSTelemtryEndpoint", ec2.InterfaceVpcEndpointAwsService.ECS_TELEMETRY)
        ]
        
        for endpoint_name, service in interface_endpoints:
            self.vpc.add_interface_endpoint(
                endpoint_name,
                service=service,
                subnets=ec2.SubnetSelection(
                    subnet_type=ec2.SubnetType.PRIVATE_WITH_EGRESS
                ),
                private_dns_enabled=True
            )
    
    def _create_security_groups(self):
        """Create security groups for different service tiers"""
        
        # Application Load Balancer Security Group
        self.alb_security_group = ec2.SecurityGroup(
            self, "ALBSecurityGroup",
            vpc=self.vpc,
            description="Security group for Application Load Balancer",
            allow_all_outbound=True
        )
        
        # Allow HTTP and HTTPS from anywhere
        self.alb_security_group.add_ingress_rule(
            peer=ec2.Peer.any_ipv4(),
            connection=ec2.Port.tcp(80),
            description="Allow HTTP"
        )
        
        self.alb_security_group.add_ingress_rule(
            peer=ec2.Peer.any_ipv4(),
            connection=ec2.Port.tcp(443),
            description="Allow HTTPS"
        )
        
        # ECS Services Security Group
        self.ecs_security_group = ec2.SecurityGroup(
            self, "ECSSecurityGroup",
            vpc=self.vpc,
            description="Security group for ECS services",
            allow_all_outbound=True
        )
        
        # Allow traffic from ALB
        self.ecs_security_group.add_ingress_rule(
            peer=self.alb_security_group,
            connection=ec2.Port.tcp_range(8000, 8010),
            description="Allow traffic from ALB"
        )
        
        # Allow ECS services to communicate with each other
        self.ecs_security_group.add_ingress_rule(
            peer=self.ecs_security_group,
            connection=ec2.Port.all_tcp(),
            description="Allow inter-service communication"
        )
        
        # Database Security Group
        self.database_security_group = ec2.SecurityGroup(
            self, "DatabaseSecurityGroup",
            vpc=self.vpc,
            description="Security group for databases",
            allow_all_outbound=False
        )
        
        # Allow PostgreSQL from ECS services
        self.database_security_group.add_ingress_rule(
            peer=self.ecs_security_group,
            connection=ec2.Port.tcp(5432),
            description="Allow PostgreSQL from ECS"
        )
        
        # Allow MongoDB from ECS services
        self.database_security_group.add_ingress_rule(
            peer=self.ecs_security_group,
            connection=ec2.Port.tcp(27017),
            description="Allow MongoDB from ECS"
        )
        
        # Allow Redis from ECS services
        self.database_security_group.add_ingress_rule(
            peer=self.ecs_security_group,
            connection=ec2.Port.tcp(6379),
            description="Allow Redis from ECS"
        )
        
        # Lambda Security Group
        self.lambda_security_group = ec2.SecurityGroup(
            self, "LambdaSecurityGroup",
            vpc=self.vpc,
            description="Security group for Lambda functions",
            allow_all_outbound=True
        )
        
        # Allow Lambda to access databases
        self.database_security_group.add_ingress_rule(
            peer=self.lambda_security_group,
            connection=ec2.Port.tcp(5432),
            description="Allow Lambda to PostgreSQL"
        )
        
        self.database_security_group.add_ingress_rule(
            peer=self.lambda_security_group,
            connection=ec2.Port.tcp(27017),
            description="Allow Lambda to MongoDB"
        )
        
        # RabbitMQ Security Group
        self.rabbitmq_security_group = ec2.SecurityGroup(
            self, "RabbitMQSecurityGroup",
            vpc=self.vpc,
            description="Security group for RabbitMQ",
            allow_all_outbound=True
        )
        
        # Allow AMQP from ECS services
        self.rabbitmq_security_group.add_ingress_rule(
            peer=self.ecs_security_group,
            connection=ec2.Port.tcp(5672),
            description="Allow AMQP from ECS"
        )
        
        # Allow RabbitMQ Management UI
        self.rabbitmq_security_group.add_ingress_rule(
            peer=self.ecs_security_group,
            connection=ec2.Port.tcp(15672),
            description="Allow RabbitMQ Management"
        )
        
        # Monitoring Security Group
        self.monitoring_security_group = ec2.SecurityGroup(
            self, "MonitoringSecurityGroup",
            vpc=self.vpc,
            description="Security group for monitoring services",
            allow_all_outbound=True
        )
        
        # Allow Prometheus from monitoring services
        self.monitoring_security_group.add_ingress_rule(
            peer=self.monitoring_security_group,
            connection=ec2.Port.tcp(9090),
            description="Allow Prometheus"
        )
        
        # Allow Grafana
        self.monitoring_security_group.add_ingress_rule(
            peer=self.alb_security_group,
            connection=ec2.Port.tcp(3000),
            description="Allow Grafana from ALB"
        )
        
        # Allow ECS services to send metrics
        self.ecs_security_group.add_egress_rule(
            peer=self.monitoring_security_group,
            connection=ec2.Port.tcp(9090),
            description="Allow metrics to Prometheus"
        )
        
        # Bastion Host Security Group (for debugging/maintenance)
        self.bastion_security_group = ec2.SecurityGroup(
            self, "BastionSecurityGroup",
            vpc=self.vpc,
            description="Security group for bastion host",
            allow_all_outbound=True
        )
        
        # Allow SSH from specific IP ranges (should be configured per environment)
        # For demo purposes, allowing from anywhere - in production, restrict this!
        self.bastion_security_group.add_ingress_rule(
            peer=ec2.Peer.any_ipv4(),  # CHANGE THIS IN PRODUCTION!
            connection=ec2.Port.tcp(22),
            description="Allow SSH (RESTRICT IN PRODUCTION!)"
        )
        
        # Allow databases to accept connections from bastion
        self.database_security_group.add_ingress_rule(
            peer=self.bastion_security_group,
            connection=ec2.Port.tcp(5432),
            description="Allow PostgreSQL from bastion"
        )
        
        self.database_security_group.add_ingress_rule(
            peer=self.bastion_security_group,
            connection=ec2.Port.tcp(27017),
            description="Allow MongoDB from bastion"
        )
        
        # Store security groups as properties for use in other stacks
        self.security_groups = {
            "alb": self.alb_security_group,
            "ecs": self.ecs_security_group,
            "database": self.database_security_group,
            "lambda": self.lambda_security_group,
            "rabbitmq": self.rabbitmq_security_group,
            "monitoring": self.monitoring_security_group,
            "bastion": self.bastion_security_group
        }
        
        # Output security group IDs
        for name, sg in self.security_groups.items():
            CfnOutput(
                self, f"{name.upper()}SecurityGroupId",
                value=sg.security_group_id,
                description=f"{name.title()} security group ID"
            )
    
    def get_subnets_by_type(self, subnet_type: str) -> List[ec2.ISubnet]:
        """Get subnets by type"""
        if subnet_type == "public":
            return self.vpc.public_subnets
        elif subnet_type == "private":
            return self.vpc.private_subnets
        elif subnet_type == "isolated":
            return self.vpc.isolated_subnets
        else:
            raise ValueError(f"Invalid subnet type: {subnet_type}")
    
    def get_security_group(self, name: str) -> ec2.SecurityGroup:
        """Get security group by name"""
        if name not in self.security_groups:
            raise ValueError(f"Security group '{name}' not found")
        return self.security_groups[name] 