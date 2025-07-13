# Trading AI Assist - Azure Infrastructure Deployment Script
# This script deploys all the Bicep templates for the Trading AI Assist platform

param(
    [Parameter(Mandatory=$true)]
    [string]$ResourceGroupName,
    
    [Parameter(Mandatory=$true)]
    [string]$Location = "East US",
    
    [Parameter(Mandatory=$false)]
    [string]$Environment = "dev"
)

Write-Host "Starting deployment of Trading AI Assist infrastructure..." -ForegroundColor Green

# Create resource group if it doesn't exist
Write-Host "Creating/verifying resource group: $ResourceGroupName" -ForegroundColor Yellow
New-AzResourceGroup -Name $ResourceGroupName -Location $Location -Force

# Deploy database infrastructure
Write-Host "Deploying database infrastructure..." -ForegroundColor Yellow
New-AzResourceGroupDeployment `
    -ResourceGroupName $ResourceGroupName `
    -TemplateFile "templates/database.bicep" `
    -TemplateParameterFile "templates/database.parameters.json" `
    -Name "database-deployment-$(Get-Date -Format 'yyyyMMdd-HHmmss')"

# Deploy messaging infrastructure
Write-Host "Deploying messaging infrastructure..." -ForegroundColor Yellow
New-AzResourceGroupDeployment `
    -ResourceGroupName $ResourceGroupName `
    -TemplateFile "templates/messaging.bicep" `
    -TemplateParameterFile "templates/messaging.parameters.json" `
    -Name "messaging-deployment-$(Get-Date -Format 'yyyyMMdd-HHmmss')"

# Deploy security infrastructure
Write-Host "Deploying security infrastructure..." -ForegroundColor Yellow
New-AzResourceGroupDeployment `
    -ResourceGroupName $ResourceGroupName `
    -TemplateFile "templates/security.bicep" `
    -TemplateParameterFile "templates/security.parameters.json" `
    -Name "security-deployment-$(Get-Date -Format 'yyyyMMdd-HHmmss')"

# Deploy monitoring infrastructure
Write-Host "Deploying monitoring infrastructure..." -ForegroundColor Yellow
New-AzResourceGroupDeployment `
    -ResourceGroupName $ResourceGroupName `
    -TemplateFile "templates/monitoring.bicep" `
    -TemplateParameterFile "templates/monitoring.parameters.json" `
    -Name "monitoring-deployment-$(Get-Date -Format 'yyyyMMdd-HHmmss')"

# Deploy app services infrastructure
Write-Host "Deploying app services infrastructure..." -ForegroundColor Yellow
New-AzResourceGroupDeployment `
    -ResourceGroupName $ResourceGroupName `
    -TemplateFile "templates/app-services.bicep" `
    -TemplateParameterFile "templates/app-services.parameters.json" `
    -Name "app-services-deployment-$(Get-Date -Format 'yyyyMMdd-HHmmss')"

Write-Host "Infrastructure deployment completed successfully!" -ForegroundColor Green
Write-Host "Resource Group: $ResourceGroupName" -ForegroundColor Cyan
Write-Host "Location: $Location" -ForegroundColor Cyan
Write-Host "Environment: $Environment" -ForegroundColor Cyan 