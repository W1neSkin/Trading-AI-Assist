// Azure App Service Plans and App Services Bicep Template
// Deploys App Service Plans and App Services for microservices
// Parameters allow customization for environment

@description('Name of the App Service Plan')
param appServicePlanName string

@description('Name of the API Gateway App Service')
param apiGatewayAppName string

@description('Name of the User Service App Service')
param userServiceAppName string

@description('Name of the AI Service App Service')
param aiServiceAppName string

@description('Name of the Trading Service App Service')
param tradingServiceAppName string

@description('Name of the Payment Service App Service')
param paymentServiceAppName string

@description('Name of the Notification Service App Service')
param notificationServiceAppName string

@description('Name of the Document Service App Service')
param documentServiceAppName string

@description('SKU for the App Service Plan')
param appServicePlanSku string = 'B1'

@description('Azure region for deployment')
param location string = resourceGroup().location

// App Service Plan
resource appServicePlan 'Microsoft.Web/serverfarms@2022-03-01' = {
  name: appServicePlanName
  location: location
  sku: {
    name: appServicePlanSku
    tier: 'Basic'
  }
  properties: {
    // Additional properties can be added here
  }
}

// API Gateway App Service
resource apiGatewayApp 'Microsoft.Web/sites@2022-03-01' = {
  name: apiGatewayAppName
  location: location
  properties: {
    serverFarmId: appServicePlan.id
    siteConfig: {
      // Additional configuration can be added here
    }
  }
  dependsOn: [appServicePlan]
}

// User Service App Service
resource userServiceApp 'Microsoft.Web/sites@2022-03-01' = {
  name: userServiceAppName
  location: location
  properties: {
    serverFarmId: appServicePlan.id
    siteConfig: {
      // Additional configuration can be added here
    }
  }
  dependsOn: [appServicePlan]
}

// AI Service App Service
resource aiServiceApp 'Microsoft.Web/sites@2022-03-01' = {
  name: aiServiceAppName
  location: location
  properties: {
    serverFarmId: appServicePlan.id
    siteConfig: {
      // Additional configuration can be added here
    }
  }
  dependsOn: [appServicePlan]
}

// Trading Service App Service
resource tradingServiceApp 'Microsoft.Web/sites@2022-03-01' = {
  name: tradingServiceAppName
  location: location
  properties: {
    serverFarmId: appServicePlan.id
    siteConfig: {
      // Additional configuration can be added here
    }
  }
  dependsOn: [appServicePlan]
}

// Payment Service App Service
resource paymentServiceApp 'Microsoft.Web/sites@2022-03-01' = {
  name: paymentServiceAppName
  location: location
  properties: {
    serverFarmId: appServicePlan.id
    siteConfig: {
      // Additional configuration can be added here
    }
  }
  dependsOn: [appServicePlan]
}

// Notification Service App Service
resource notificationServiceApp 'Microsoft.Web/sites@2022-03-01' = {
  name: notificationServiceAppName
  location: location
  properties: {
    serverFarmId: appServicePlan.id
    siteConfig: {
      // Additional configuration can be added here
    }
  }
  dependsOn: [appServicePlan]
}

// Document Service App Service
resource documentServiceApp 'Microsoft.Web/sites@2022-03-01' = {
  name: documentServiceAppName
  location: location
  properties: {
    serverFarmId: appServicePlan.id
    siteConfig: {
      // Additional configuration can be added here
    }
  }
  dependsOn: [appServicePlan]
}

// Outputs for use in other templates or applications
output apiGatewayUrl string = 'https://${apiGatewayApp.properties.defaultHostName}'
output userServiceUrl string = 'https://${userServiceApp.properties.defaultHostName}'
output aiServiceUrl string = 'https://${aiServiceApp.properties.defaultHostName}'
output tradingServiceUrl string = 'https://${tradingServiceApp.properties.defaultHostName}'
output paymentServiceUrl string = 'https://${paymentServiceApp.properties.defaultHostName}'
output notificationServiceUrl string = 'https://${notificationServiceApp.properties.defaultHostName}'
output documentServiceUrl string = 'https://${documentServiceApp.properties.defaultHostName}' 