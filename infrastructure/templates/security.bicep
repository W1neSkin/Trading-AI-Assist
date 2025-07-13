// Azure Key Vault and AD Integration Bicep Template
// Deploys Key Vault with access policies and secrets
// Parameters allow customization for environment

@description('Name of the Key Vault')
param keyVaultName string

@description('Tenant ID for Azure AD')
param tenantId string

@description('Object ID of the service principal for access')
param servicePrincipalObjectId string

@description('Azure region for deployment')
param location string = resourceGroup().location

// Key Vault
resource keyVault 'Microsoft.KeyVault/vaults@2022-07-01' = {
  name: keyVaultName
  location: location
  properties: {
    sku: {
      family: 'A'
      name: 'standard'
    }
    tenantId: tenantId
    enableRbacAuthorization: false
    enableSoftDelete: true
    softDeleteRetentionInDays: 7
    accessPolicies: [
      {
        tenantId: tenantId
        objectId: servicePrincipalObjectId
        permissions: {
          secrets: [
            'get'
            'list'
            'set'
            'delete'
            'recover'
            'backup'
            'restore'
          ]
          keys: [
            'get'
            'list'
            'create'
            'delete'
            'recover'
            'backup'
            'restore'
          ]
          certificates: [
            'get'
            'list'
            'create'
            'delete'
            'recover'
            'backup'
            'restore'
          ]
        }
      }
    ]
  }
}

// Example secrets (you can add more as needed)
resource sqlConnectionSecret 'Microsoft.KeyVault/vaults/secrets@2022-07-01' = {
  name: '${keyVault.name}/sql-connection-string'
  properties: {
    value: 'your-sql-connection-string-here'
  }
  dependsOn: [keyVault]
}

resource serviceBusConnectionSecret 'Microsoft.KeyVault/vaults/secrets@2022-07-01' = {
  name: '${keyVault.name}/servicebus-connection-string'
  properties: {
    value: 'your-servicebus-connection-string-here'
  }
  dependsOn: [keyVault]
}

resource eventGridKeySecret 'Microsoft.KeyVault/vaults/secrets@2022-07-01' = {
  name: '${keyVault.name}/eventgrid-key'
  properties: {
    value: 'your-eventgrid-key-here'
  }
  dependsOn: [keyVault]
}

// Outputs for use in other templates or applications
output keyVaultUri string = keyVault.properties.vaultUri 