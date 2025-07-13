#!/bin/bash

# Trading AI Assist - Environment Setup Script
# This script helps set up environment variables for different deployment scenarios

set -e

echo "Trading AI Assist - Environment Setup"
echo "====================================="

# Function to display usage
show_usage() {
    echo "Usage: $0 [environment]"
    echo ""
    echo "Environments:"
    echo "  local     - Set up for local development"
    echo "  staging   - Set up for Azure staging deployment"
    echo "  production - Set up for Azure production deployment"
    echo ""
    echo "Examples:"
    echo "  $0 local"
    echo "  $0 staging"
    echo "  $0 production"
}

# Function to setup local environment
setup_local() {
    echo "Setting up local development environment..."
    
    if [ -f ".env" ]; then
        echo "Warning: .env file already exists. Creating backup..."
        cp .env .env.backup.$(date +%Y%m%d_%H%M%S)
    fi
    
    cp env.example .env
    echo "✅ Local environment file created: .env"
    echo "📝 Please edit .env with your local configuration values"
}

# Function to setup staging environment
setup_staging() {
    echo "Setting up Azure staging environment..."
    
    if [ -f ".env" ]; then
        echo "Warning: .env file already exists. Creating backup..."
        cp .env .env.backup.$(date +%Y%m%d_%H%M%S)
    fi
    
    cp env.azure.staging .env
    echo "✅ Staging environment file created: .env"
    echo "📝 Please edit .env with your Azure staging configuration values"
    echo "🔧 Remember to update Azure service connection strings and keys"
}

# Function to setup production environment
setup_production() {
    echo "Setting up Azure production environment..."
    
    if [ -f ".env" ]; then
        echo "Warning: .env file already exists. Creating backup..."
        cp .env .env.backup.$(date +%Y%m%d_%H%M%S)
    fi
    
    cp env.azure .env
    echo "✅ Production environment file created: .env"
    echo "📝 Please edit .env with your Azure production configuration values"
    echo "🔧 Remember to update Azure service connection strings and keys"
    echo "⚠️  Make sure to use production keys and endpoints"
}

# Function to validate environment file
validate_env() {
    echo "Validating environment file..."
    
    if [ ! -f ".env" ]; then
        echo "❌ Error: .env file not found"
        exit 1
    fi
    
    # Check for required variables
    required_vars=(
        "APP_NAME"
        "APP_VERSION"
        "ENVIRONMENT"
    )
    
    missing_vars=()
    
    for var in "${required_vars[@]}"; do
        if ! grep -q "^${var}=" .env; then
            missing_vars+=("$var")
        fi
    done
    
    if [ ${#missing_vars[@]} -eq 0 ]; then
        echo "✅ Environment file validation passed"
    else
        echo "❌ Missing required environment variables:"
        for var in "${missing_vars[@]}"; do
            echo "   - $var"
        done
        exit 1
    fi
}

# Function to show environment info
show_env_info() {
    echo ""
    echo "Environment Information:"
    echo "======================="
    
    if [ -f ".env" ]; then
        echo "📁 Current .env file:"
        grep -E "^(APP_NAME|APP_VERSION|ENVIRONMENT|DEBUG|LOG_LEVEL)=" .env | sort
    else
        echo "❌ No .env file found"
    fi
}

# Main script logic
case "${1:-}" in
    "local")
        setup_local
        validate_env
        show_env_info
        ;;
    "staging")
        setup_staging
        validate_env
        show_env_info
        ;;
    "production")
        setup_production
        validate_env
        show_env_info
        ;;
    "validate")
        validate_env
        show_env_info
        ;;
    "info")
        show_env_info
        ;;
    *)
        show_usage
        exit 1
        ;;
esac

echo ""
echo "Setup complete! 🎉"
echo ""
echo "Next steps:"
echo "1. Edit the .env file with your configuration values"
echo "2. For Azure deployment, ensure all Azure service connection strings are set"
echo "3. Test your configuration with: $0 validate" 