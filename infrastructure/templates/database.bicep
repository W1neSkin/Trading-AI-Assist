// Azure SQL Database Bicep Template
// Deploys a SQL Server and a single database
// Parameters allow customization for environment

@description('Name of the SQL Server')
param sqlServerName string

@description('SQL Server admin username')
param sqlAdminUsername string

@secure()
@description('SQL Server admin password')
param sqlAdminPassword string

@description('Name of the SQL Database')
param sqlDatabaseName string

@description('SKU for the SQL Database (e.g., S0, S1, GP_S_Gen5_2)')
param sqlSkuName string = 'S0'

@description('Azure region for deployment')
param location string = resourceGroup().location

resource sqlServer 'Microsoft.Sql/servers@2022-02-01-preview' = {
  name: sqlServerName
  location: location
  properties: {
    administratorLogin: sqlAdminUsername
    administratorLoginPassword: sqlAdminPassword
    version: '12.0'
  }
}

resource sqlDatabase 'Microsoft.Sql/servers/databases@2022-02-01-preview' = {
  name: '${sqlServer.name}/${sqlDatabaseName}'
  location: location
  properties: {
    // Additional properties can be added here
  }
  sku: {
    name: sqlSkuName
  }
  dependsOn: [sqlServer]
}

// Output connection string for use in apps
output sqlConnectionString string = 'Server=tcp:${sqlServer.name}.database.windows.net,1433;Initial Catalog=${sqlDatabaseName};Persist Security Info=False;User ID=${sqlAdminUsername};Password=<password>;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;' 