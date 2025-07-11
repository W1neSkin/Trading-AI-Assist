# Trading AI Assist - Architecture Overview

## Table of Contents

1. [System Overview](#system-overview)
2. [Microservices Architecture](#microservices-architecture)
3. [Hybrid LLM Architecture](#hybrid-llm-architecture)
4. [Data Architecture](#data-architecture)
5. [Messaging Architecture](#messaging-architecture)
6. [Infrastructure Architecture](#infrastructure-architecture)
7. [Security Architecture](#security-architecture)
8. [Deployment Architecture](#deployment-architecture)

## System Overview

The Trading AI Assist platform is an enterprise-grade, cloud-native application designed to enhance user interaction and automate complex workflows in the trading and cryptocurrency domain. The system enables intelligent data retrieval and conversational access to financial and transactional information through natural language processing.

### Key Features

- **Hybrid LLM System**: Seamless switching between local Ollama and cloud OpenRoute LLMs
- **Advanced Payment Infrastructure**: Secure transactions with Stripe integration
- **High-Performance Data Pipelines**: Nanosecond-scale event stream processing
- **Natural Language Interface**: SQL generation and conversational AI
- **Document Processing**: Multi-format support (PDF, DOCX, TXT, HTML)
- **KYC/Identity Verification**: Automated onboarding workflows
- **Desktop Admin Application**: Comprehensive administration with Azure AD integration

## Microservices Architecture

### Service Overview

The platform implements a microservices architecture with clear separation of concerns:

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   API Gateway   │────│   User Service  │────│ Trading Service │
│   Port: 8000    │    │   Port: 8001    │    │   Port: 8002    │
└─────────────────┘    └─────────────────┘    └─────────────────┘
         │                       │                       │
         ├───────────────────────┼───────────────────────┤
         │                       │                       │
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   AI Service    │    │ Payment Service │    │Notification Svc │
│   Port: 8003    │    │   Port: 8004    │    │   Port: 8005    │
└─────────────────┘    └─────────────────┘    └─────────────────┘
         │                       │                       │
         └───────────────────────┼───────────────────────┘
                                 │
                    ┌─────────────────┐
                    │Document Service │
                    │   Port: 8006    │
                    └─────────────────┘
```

### Service Responsibilities

#### API Gateway (Port 8000)
**Primary Role**: Central entry point and request routing

**Responsibilities**:
- Request routing and load balancing
- Authentication and authorization via Azure AD
- Rate limiting and throttling
- API versioning and documentation
- Metrics collection and monitoring
- CORS and security headers management

**Technologies**: FastAPI, Azure Redis Cache (caching), Azure Monitor (metrics)

#### User Service (Port 8001)
**Primary Role**: User management and authentication

**Responsibilities**:
- User registration and profile management
- Azure AD integration and token validation
- Role-based access control (RBAC)
- Session management
- User preferences and settings
- Account verification and password reset

**Technologies**: FastAPI, Azure SQL Database, Azure Redis Cache, Azure AD

#### Trading Service (Port 8002)
**Primary Role**: Trading operations and market data

**Responsibilities**:
- Order management and execution
- Portfolio tracking and analytics
- Market data processing
- Risk management and compliance
- Trading strategy implementation
- Real-time position updates

**Technologies**: FastAPI, Azure SQL Database, Azure Redis Cache, WebSockets

#### AI Service (Port 8003)
**Primary Role**: Artificial Intelligence and machine learning

**Responsibilities**:
- **Hybrid LLM Management**: Switch between Ollama (local) and OpenRoute (cloud)
- **RAG System**: Retrieval-Augmented Generation with vector databases
- **Natural Language to SQL**: Convert user queries to SQL
- **Document Processing**: Extract and index document content
- **Performance Monitoring**: Track ML model performance
- **Context Management**: Maintain conversation context

**Technologies**: FastAPI, LangChain, Ollama, OpenRoute API, FAISS, MongoDB, TensorFlow

#### Payment Service (Port 8004)
**Primary Role**: Payment processing and billing

**Responsibilities**:
- **Stripe Integration**: Payment processing and webhooks
- **Subscription Management**: Billing cycles and proration
- **KYC/Identity Verification**: Automated compliance workflows
- **Invoice Generation**: Automated billing and dunning
- **Payment Method Management**: Secure card storage
- **Fraud Detection**: Transaction monitoring

**Technologies**: FastAPI, Stripe API, Azure SQL Database, Webhooks

#### Notification Service (Port 8005)
**Primary Role**: Communication and alerts

**Responsibilities**:
- **Multi-channel Notifications**: Email, SMS, push notifications
- **Azure Integration**: Service Bus message processing, Event Grid publishing
- **Event-driven Alerts**: Trading alerts and system notifications
- **Template Management**: Dynamic notification templates
- **Delivery Tracking**: Notification status and analytics

**Technologies**: FastAPI, Azure Service Bus, Azure Event Grid, Email/SMS APIs

#### Document Service (Port 8006)
**Primary Role**: Document processing and management

**Responsibilities**:
- **Multi-format Processing**: PDF, DOCX, TXT, HTML support
- **Content Extraction**: Text and metadata extraction
- **Document Storage**: Secure file management with Azure Blob Storage
- **OCR Processing**: Image-to-text conversion
- **Version Control**: Document versioning and history
- **Search and Indexing**: Full-text search capabilities

**Technologies**: FastAPI, MongoDB, Azure Blob Storage, OCR libraries

## Hybrid LLM Architecture

### Overview

The platform implements a sophisticated hybrid LLM system that allows dynamic switching between local and cloud-based language models based on requirements, cost, and performance considerations.

```
┌─────────────────────────────────────────────────────────────┐
│                    AI Service Manager                      │
├─────────────────────────────────────────────────────────────┤
│  ┌─────────────────┐              ┌───────────────────────┐ │
│  │  Local LLM      │              │    Cloud LLM         │ │
│  │   (Ollama)      │              │  (OpenRoute API)     │ │
│  │                 │              │                      │ │
│  │ • Fast response │              │ • Advanced models    │ │
│  │ • Data privacy  │              │ • Scalable compute   │ │
│  │ • Cost effective│              │ • Latest features    │ │
│  │ • Offline ready │              │ • High accuracy      │ │
│  └─────────────────┘              └───────────────────────┘ │
└─────────────────────────────────────────────────────────────┘
                              │
                    ┌─────────────────┐
                    │   RAG System    │
                    │                 │
                    │ • Vector Store  │
                    │ • Embeddings    │
                    │ • Retrieval     │
                    │ • Context Mgmt  │
                    └─────────────────┘
```

### Component Details

#### Local LLM (Ollama)
- **Use Cases**: 
  - SQL generation (deterministic tasks)
  - Document analysis
  - Privacy-sensitive queries
  - Development and testing
- **Models**: Llama2, CodeLlama, Mistral
- **Benefits**: Low latency, data privacy, cost control

#### Cloud LLM (OpenRoute)
- **Use Cases**:
  - Complex reasoning tasks
  - Multi-modal processing
  - Latest model capabilities
  - High-scale operations
- **Models**: GPT-4, Claude, Gemini, Mistral
- **Benefits**: Advanced capabilities, automatic scaling

#### RAG System
- **Vector Database**: FAISS for similarity search
- **Embeddings**: HuggingFace sentence-transformers
- **Document Storage**: MongoDB for persistence
- **Retrieval**: Context-aware document retrieval

### Selection Strategy

```python
def select_llm_provider(query_type, sensitivity, complexity):
    """
    Dynamic LLM provider selection based on:
    - Query type (SQL, chat, analysis)
    - Data sensitivity (public, internal, confidential)
    - Complexity level (simple, moderate, complex)
    """
    if sensitivity == "confidential" or query_type == "sql":
        return "ollama"
    elif complexity == "complex" or query_type == "reasoning":
        return "openroute"
    else:
        return "ollama"  # Default to local for cost efficiency
```

## Data Architecture

### Database Design

The platform uses a polyglot persistence approach with different databases optimized for specific use cases:

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│ Azure SQL       │    │    MongoDB      │    │ Azure Redis     │
│ Database        │    │                 │    │ Cache           │
│                 │    │                 │    │                 │
│ • User data     │    │ • Documents     │    │ • Sessions      │
│ • Transactions  │    │ • Embeddings    │    │ • Cache         │
│ • Orders        │    │ • Logs          │    │ • Rate limits   │
│ • Payments      │    │ • Metadata      │    │ • Temp data     │
│ • Analytics     │    │ • Unstructured  │    │ • Pub/Sub       │
└─────────────────┘    └─────────────────┘    └─────────────────┘
```

#### Azure SQL Database Schema Design

**Core Tables**:
- `users`: User accounts and profiles
- `trading_accounts`: Trading account information
- `orders`: Trading orders and execution data
- `transactions`: Financial transactions
- `payments`: Payment records and status
- `subscriptions`: Billing and subscription data
- `kyc_verifications`: Identity verification records

**Performance Optimizations**:
- **Partitioning**: Time-based partitioning for high-volume tables
- **Indexing**: Strategic indexes for common query patterns
- **Connection Pooling**: Optimized connection management
- **Read Replicas**: Scale read operations across multiple instances

## Messaging Architecture

### Azure Service Bus Integration

The platform uses Azure Service Bus as the primary messaging infrastructure:

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   Publisher     │────│  Service Bus    │────│   Subscriber    │
│   Services      │    │   Namespace     │    │   Services      │
└─────────────────┘    └─────────────────┘    └─────────────────┘
                                │
                    ┌─────────────────┐
                    │   Dead Letter   │
                    │     Queue       │
                    └─────────────────┘
```

**Message Types**:
- **Trading Events**: Order execution, price updates, portfolio changes
- **User Events**: Registration, profile updates, authentication events
- **Payment Events**: Transaction processing, subscription changes
- **AI Events**: Model usage, cost tracking, performance metrics
- **System Events**: Health checks, maintenance notifications

### Azure Event Grid

For event-driven architecture and real-time notifications:

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   Event Source  │────│   Event Grid    │────│   Event Handler │
│                 │    │                 │    │                 │
│ • Azure Storage │    │ • Topic         │    │ • Azure Function│
│ • Service Bus   │    │ • Subscription  │    │ • Webhook       │
│ • Custom Events │    │ • Routing       │    │ • Logic App     │
└─────────────────┘    └─────────────────┘    └─────────────────┘
```

## Infrastructure Architecture

### Azure Resource Organization

```
┌─────────────────────────────────────────────────────────────┐
│                    Resource Group                          │
├─────────────────────────────────────────────────────────────┤
│  ┌─────────────────┐  ┌─────────────────┐  ┌───────────────┐ │
│  │   Networking    │  │   Compute       │  │   Storage     │ │
│  │                 │  │                 │  │               │ │
│  │ • Virtual       │  │ • App Service   │  │ • SQL Database│ │
│  │   Network       │  │ • Functions     │  │ • Blob Storage│ │
│  │ • Load Balancer │  │ • Container     │  │ • Redis Cache │ │
│  │ • API Gateway   │  │   Registry      │  │ • File Share  │ │
│  └─────────────────┘  └─────────────────┘  └───────────────┘ │
├─────────────────────────────────────────────────────────────┤
│  ┌─────────────────┐  ┌─────────────────┐  ┌───────────────┐ │
│  │   Messaging     │  │   Security      │  │   Monitoring  │ │
│  │                 │  │                 │  │               │ │
│  │ • Service Bus   │  │ • Key Vault     │  │ • Monitor     │ │
│  │ • Event Grid    │  │ • AD B2C        │  │ • Log Analytics│ │
│  │ • Event Hubs    │  │ • Application   │  │ • Application │ │
│  │                 │  │   Gateway       │  │   Insights    │ │
│  └─────────────────┘  └─────────────────┘  └───────────────┘ │
└─────────────────────────────────────────────────────────────┘
```

### Azure App Service

**Service Deployment**:
- **App Service Plans**: Dedicated plans for each service tier
- **Container Deployment**: Docker containers for consistent deployment
- **Auto-scaling**: Based on CPU, memory, and custom metrics
- **Health Monitoring**: Built-in health checks and monitoring

**Configuration**:
```json
{
  "appServicePlan": {
    "sku": "P1v3",
    "capacity": 2,
    "autoScale": {
      "minCapacity": 2,
      "maxCapacity": 10,
      "rules": [
        {
          "metricTrigger": {
            "metricName": "CpuPercentage",
            "threshold": 70
          },
          "scaleAction": {
            "direction": "Increase",
            "type": "ChangeCount",
            "value": 1
          }
        }
      ]
    }
  }
}
```

### Azure Functions

**Serverless Processing**:
- **Event-driven**: Triggered by Service Bus, Event Grid, HTTP
- **Scalable**: Automatic scaling based on demand
- **Cost-effective**: Pay-per-execution model
- **Integration**: Seamless integration with Azure services

**Use Cases**:
- **Data Processing**: Transform and enrich data streams
- **Scheduled Tasks**: Periodic maintenance and cleanup
- **Webhook Handlers**: Process external service notifications
- **Image Processing**: Document OCR and image analysis

## Security Architecture

### Azure Active Directory Integration

**Authentication Flow**:
```
┌─────────────┐    ┌─────────────┐    ┌─────────────┐    ┌─────────────┐
│   Client    │────│   API       │────│   Azure AD  │────│   Resource  │
│   App       │    │   Gateway   │    │             │    │   Service   │
└─────────────┘    └─────────────┘    └─────────────┘    └─────────────┘
```

**Authorization Model**:
- **Application Permissions**: Service-to-service authentication
- **Delegated Permissions**: User context for admin operations
- **Role-based Access Control**: Fine-grained permission management
- **Conditional Access**: Location, device, and risk-based policies

### Azure Key Vault

**Secret Management**:
- **API Keys**: External service credentials
- **Connection Strings**: Database and service connections
- **Certificates**: SSL/TLS certificates for secure communication
- **Configuration**: Application settings and environment variables

**Access Control**:
- **Managed Identities**: Automatic authentication for Azure resources
- **Access Policies**: Granular permissions for different services
- **Audit Logging**: Complete audit trail for all access
- **Rotation**: Automatic secret rotation and management

## Deployment Architecture

### Azure DevOps Pipeline

**CI/CD Pipeline**:
```yaml
trigger:
  branches:
    include:
    - main
    - develop

stages:
- stage: Build
  jobs:
  - job: BuildAndTest
    steps:
    - task: DotNetCoreCLI@2
      inputs:
        command: 'build'
        projects: '**/*.csproj'
    - task: DotNetCoreCLI@2
      inputs:
        command: 'test'
        projects: '**/*Tests/*.csproj'

- stage: Deploy
  jobs:
  - job: DeployToStaging
    steps:
    - task: AzureResourceManagerTemplateDeployment@3
      inputs:
        deploymentScope: 'Resource Group'
        azureResourceManagerConnection: 'Azure Connection'
        subscriptionId: '$(subscriptionId)'
        action: 'Create Or Update Resource Group'
        resourceGroupName: '$(resourceGroupName)'
        location: '$(location)'
        templateLocation: 'Linked artifact'
        csmFile: '$(System.DefaultWorkingDirectory)/infrastructure/main.bicep'
        csmParametersFile: '$(System.DefaultWorkingDirectory)/infrastructure/parameters.json'
        deploymentMode: 'Incremental'
```

### ARM Templates

**Infrastructure as Code**:
- **Bicep Templates**: Declarative infrastructure definition
- **Parameter Files**: Environment-specific configuration
- **Modular Design**: Reusable components and templates
- **Version Control**: Infrastructure changes tracked in Git

**Deployment Environments**:
- **Development**: Lightweight resources for development
- **Staging**: Production-like environment for testing
- **Production**: Full-scale deployment with monitoring

This architecture provides a robust, scalable, and secure foundation for the Trading AI Assist platform, leveraging Azure's comprehensive cloud services for optimal performance and cost efficiency.