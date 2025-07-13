# Trading AI Assist - Environment Setup Script (PowerShell)
# This script helps set up environment variables for different deployment scenarios

param(
    [Parameter(Mandatory=$true)]
    [ValidateSet("local", "staging", "production", "validate", "info")]
    [string]$Environment
)

Write-Host "Trading AI Assist - Environment Setup" -ForegroundColor Green
Write-Host "=====================================" -ForegroundColor Green

# Function to display usage
function Show-Usage {
    Write-Host "Usage: .\setup-env.ps1 [environment]" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Environments:" -ForegroundColor Yellow
    Write-Host "  local     - Set up for local development" -ForegroundColor White
    Write-Host "  staging   - Set up for Azure staging deployment" -ForegroundColor White
    Write-Host "  production - Set up for Azure production deployment" -ForegroundColor White
    Write-Host "  validate  - Validate current .env file" -ForegroundColor White
    Write-Host "  info      - Show current environment information" -ForegroundColor White
    Write-Host ""
    Write-Host "Examples:" -ForegroundColor Yellow
    Write-Host "  .\setup-env.ps1 local" -ForegroundColor White
    Write-Host "  .\setup-env.ps1 staging" -ForegroundColor White
    Write-Host "  .\setup-env.ps1 production" -ForegroundColor White
}

# Function to setup local environment
function Setup-Local {
    Write-Host "Setting up local development environment..." -ForegroundColor Yellow
    
    if (Test-Path ".env") {
        Write-Host "Warning: .env file already exists. Creating backup..." -ForegroundColor Yellow
        $timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
        Copy-Item ".env" ".env.backup.$timestamp"
    }
    
    Copy-Item "env.example" ".env"
    Write-Host "✅ Local environment file created: .env" -ForegroundColor Green
    Write-Host "📝 Please edit .env with your local configuration values" -ForegroundColor Cyan
}

# Function to setup staging environment
function Setup-Staging {
    Write-Host "Setting up Azure staging environment..." -ForegroundColor Yellow
    
    if (Test-Path ".env") {
        Write-Host "Warning: .env file already exists. Creating backup..." -ForegroundColor Yellow
        $timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
        Copy-Item ".env" ".env.backup.$timestamp"
    }
    
    Copy-Item "env.azure.staging" ".env"
    Write-Host "✅ Staging environment file created: .env" -ForegroundColor Green
    Write-Host "📝 Please edit .env with your Azure staging configuration values" -ForegroundColor Cyan
    Write-Host "🔧 Remember to update Azure service connection strings and keys" -ForegroundColor Cyan
}

# Function to setup production environment
function Setup-Production {
    Write-Host "Setting up Azure production environment..." -ForegroundColor Yellow
    
    if (Test-Path ".env") {
        Write-Host "Warning: .env file already exists. Creating backup..." -ForegroundColor Yellow
        $timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
        Copy-Item ".env" ".env.backup.$timestamp"
    }
    
    Copy-Item "env.azure" ".env"
    Write-Host "✅ Production environment file created: .env" -ForegroundColor Green
    Write-Host "📝 Please edit .env with your Azure production configuration values" -ForegroundColor Cyan
    Write-Host "🔧 Remember to update Azure service connection strings and keys" -ForegroundColor Cyan
    Write-Host "⚠️  Make sure to use production keys and endpoints" -ForegroundColor Red
}

# Function to validate environment file
function Test-EnvironmentFile {
    Write-Host "Validating environment file..." -ForegroundColor Yellow
    
    if (-not (Test-Path ".env")) {
        Write-Host "❌ Error: .env file not found" -ForegroundColor Red
        exit 1
    }
    
    # Check for required variables
    $requiredVars = @(
        "APP_NAME",
        "APP_VERSION",
        "ENVIRONMENT"
    )
    
    $missingVars = @()
    $envContent = Get-Content ".env"
    
    foreach ($var in $requiredVars) {
        if (-not ($envContent -match "^${var}=")) {
            $missingVars += $var
        }
    }
    
    if ($missingVars.Count -eq 0) {
        Write-Host "✅ Environment file validation passed" -ForegroundColor Green
    } else {
        Write-Host "❌ Missing required environment variables:" -ForegroundColor Red
        foreach ($var in $missingVars) {
            Write-Host "   - $var" -ForegroundColor Red
        }
        exit 1
    }
}

# Function to show environment info
function Show-EnvironmentInfo {
    Write-Host ""
    Write-Host "Environment Information:" -ForegroundColor Green
    Write-Host "=======================" -ForegroundColor Green
    
    if (Test-Path ".env") {
        Write-Host "📁 Current .env file:" -ForegroundColor Yellow
        $envContent = Get-Content ".env"
        $envContent | Where-Object { $_ -match "^(APP_NAME|APP_VERSION|ENVIRONMENT|DEBUG|LOG_LEVEL)=" } | Sort-Object
    } else {
        Write-Host "❌ No .env file found" -ForegroundColor Red
    }
}

# Main script logic
switch ($Environment) {
    "local" {
        Setup-Local
        Test-EnvironmentFile
        Show-EnvironmentInfo
    }
    "staging" {
        Setup-Staging
        Test-EnvironmentFile
        Show-EnvironmentInfo
    }
    "production" {
        Setup-Production
        Test-EnvironmentFile
        Show-EnvironmentInfo
    }
    "validate" {
        Test-EnvironmentFile
        Show-EnvironmentInfo
    }
    "info" {
        Show-EnvironmentInfo
    }
    default {
        Show-Usage
        exit 1
    }
}

Write-Host ""
Write-Host "Setup complete! 🎉" -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "1. Edit the .env file with your configuration values" -ForegroundColor White
Write-Host "2. For Azure deployment, ensure all Azure service connection strings are set" -ForegroundColor White
Write-Host "3. Test your configuration with: .\setup-env.ps1 validate" -ForegroundColor White 