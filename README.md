# Trading AI Assist

An enterprise-grade AI-powered platform designed to enhance user interaction and automate complex workflows in the trading and cryptocurrency domain. The system enables intelligent data retrieval and conversational access to financial and transactional information through natural language processing.

## 🏗️ Architecture Overview

This platform implements a **microservices architecture** with the following key components:

- **Hybrid LLM System**: Switch between local Ollama and cloud OpenRoute LLMs
- **Messaging Layer**: Azure Service Bus as primary message broker with Event Grid integration
- **Database Layer**: Azure SQL Database for relational data, MongoDB for document storage
- **Payment Processing**: Stripe integration for secure transactions and billing
- **AI/ML Pipeline**: LangChain with RAG workflows for intelligent data access
- **Infrastructure**: Azure with ARM templates for Infrastructure as Code

## 🛠️ Technology Stack

### Backend & APIs
- **FastAPI**: High-performance Python web framework
- **Python 3.11+**: Primary development language
- **Pydantic**: Data validation and serialization

### Databases
- **Azure SQL Database**: Relational database for transactional data
- **MongoDB**: Document database for flexible schema requirements
- **Azure Redis Cache**: Caching and session management

### Messaging & Queues
- **Azure Service Bus**: Primary message broker for microservices communication
- **Azure Event Grid**: Event service for Azure-native integrations
- **Azure Event Hubs**: Stream processing for high-volume events

### AI/ML Components
- **Ollama**: Local LLM deployment
- **OpenRoute**: Cloud LLM service
- **LangChain**: LLM orchestration and RAG workflows
- **TensorFlow**: ML model deployment and monitoring

### Payment Processing
- **Stripe**: Payment gateway and billing management
- **Webhook handling**: Real-time payment notifications

### Infrastructure
- **Azure ARM Templates**: Infrastructure as Code
- **Azure Functions**: Serverless computing
- **Azure Monitor**: Monitoring and logging
- **Docker**: Containerization
- **Azure DevOps**: Automated deployment pipelines

## 📁 Project Structure

```
├── services/                    # Microservices
│   ├── api-gateway/            # API Gateway service
│   ├── user-service/           # User management and authentication
│   ├── trading-service/        # Trading logic and operations
│   ├── ai-service/             # AI/ML models and processing
│   ├── payment-service/        # Payment processing and billing
│   ├── notification-service/   # Notifications and messaging
│   └── document-service/       # Document processing pipeline
├── infrastructure/             # Azure ARM infrastructure code
├── shared/                     # Shared libraries and utilities
├── admin-app/                  # Desktop Admin Application (.NET WPF)
├── scripts/                    # Deployment and utility scripts
├── docs/                       # Comprehensive documentation
├── tests/                      # Test suites
└── docker-compose.yml          # Local development environment
```

## 🚀 Key Features

### 1. Hybrid LLM Architecture
- **Local Processing**: Ollama integration for on-premises AI
- **Cloud Flexibility**: OpenRoute API for scalable cloud processing
- **Dynamic Switching**: Runtime selection between local and cloud models

### 2. Advanced Payment Infrastructure
- **Secure Transactions**: One-time and recurring payments
- **Flexible Billing**: Subscription management with metered billing
- **Partner Onboarding**: KYC and identity verification workflows

### 3. High-Performance Data Pipelines
- **Nanosecond Scaling**: Event stream processing
- **Real-time Analytics**: Live trading data analysis
- **Multi-format Support**: PDF, DOCX, TXT, HTML document processing

### 4. Natural Language Interface
- **SQL Generation**: Natural language to SQL conversion
- **Conversational AI**: Interactive financial data queries
- **RAG Integration**: Context-aware responses with database integration

### 5. Desktop Admin Application
- **Azure AD Integration**: Secure authentication and authorization
- **User Management**: Comprehensive user administration dashboard
- **AI Analytics**: Real-time cost tracking and usage analytics
- **System Monitoring**: Platform health and performance monitoring

## 📋 Implementation Responsibilities

This project implements all specified technical requirements:

✅ **Architecture & Design**
- Microservices architecture with service separation
- Database schema design (Azure SQL Database & MongoDB)
- High-performance data pipeline architecture

✅ **AI/ML Integration**
- Hybrid LLM architecture (Ollama + OpenRoute)
- LangChain with RAG workflows
- TensorFlow integration for ML monitoring
- Natural language to SQL conversion

✅ **Infrastructure & DevOps**
- Azure ARM templates for Infrastructure as Code
- Azure DevOps CI/CD pipelines
- Docker containerization
- Azure Functions serverless functions

✅ **Data Management**
- Azure SQL Database with backup/restore/replication
- MongoDB flexible schema design
- Data consistency and integrity

✅ **Payment Processing**
- Stripe Payments integration
- Subscription billing systems
- Invoice automation and dunning management

✅ **Messaging & Communication**
- Azure Service Bus messaging patterns
- Azure Event Grid event processing
- Azure Event Hubs stream processing

✅ **Desktop Application Development**
- .NET WPF desktop application
- Azure AD authentication and authorization
- Real-time analytics and monitoring
- User management and administration

## 🔧 Getting Started

### Prerequisites
- Python 3.11+
- Docker & Docker Compose
- Azure CLI configured
- .NET 8.0 SDK (for Admin App)
- Visual Studio 2022 or VS Code

### Quick Start
```bash
# Clone the repository
git clone <repository-url>
cd trading-ai-assist

# Set up virtual environment
python -m venv venv
source venv/bin/activate  # On Windows: venv\Scripts\activate

# Install dependencies
pip install -r requirements.txt

# Start local development environment
docker-compose up -d

# Run database migrations
python scripts/migrate.py

# Start the API gateway
cd services/api-gateway
uvicorn main:app --reload --port 8000

# Build and run Admin App
cd admin-app
dotnet build
dotnet run
```

## 📚 Documentation

Comprehensive documentation is available in the `/docs` directory:

- [Architecture Overview](docs/architecture.md)
- [API Documentation](docs/api.md)
- [Deployment Guide](docs/deployment.md)
- [Development Guide](docs/development.md)
- [Security Guidelines](docs/security.md)
- [Admin App Guide](docs/admin-app.md)

## 🧪 Testing

```bash
# Run all tests
pytest

# Run tests with coverage
pytest --cov=services --cov-report=html

# Run integration tests
pytest tests/integration/

# Run Admin App tests
cd admin-app
dotnet test
```

## 🚀 Deployment

The platform supports multiple deployment environments:

- **Development**: Local Docker Compose
- **Staging**: Azure App Service with ARM templates
- **Production**: Azure App Service with full monitoring and scaling

See [Deployment Guide](docs/deployment.md) for detailed instructions.

## 📈 Monitoring & Observability

- **Application Metrics**: Custom FastAPI metrics
- **Infrastructure Monitoring**: Azure Monitor
- **Log Aggregation**: Centralized logging with structured logs
- **Performance Monitoring**: ML model performance tracking

## 🔒 Security

- **Authentication**: Azure AD with JWT tokens
- **Authorization**: Role-based access control (RBAC)
- **Data Encryption**: At rest and in transit with Azure Key Vault
- **Compliance**: SOC2 and PCI DSS considerations

## 🤝 Contributing

Please read our [Contributing Guide](docs/contributing.md) for details on our code of conduct and the process for submitting pull requests.

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details. 