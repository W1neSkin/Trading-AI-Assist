@description('The name of the virtual network')
param virtualNetworkName string = 'trading-ai-assist-vnet'

@description('The address space for the virtual network')
param virtualNetworkAddressSpace array = [
  '10.0.0.0/16'
]

@description('The name of the first subnet')
param subnet1Name string = 'app-subnet'

@description('The address space for the first subnet')
param subnet1AddressSpace string = '10.0.1.0/24'

@description('The name of the second subnet')
param subnet2Name string = 'database-subnet'

@description('The address space for the second subnet')
param subnet2AddressSpace string = '10.0.2.0/24'

@description('The name of the third subnet')
param subnet3Name string = 'management-subnet'

@description('The address space for the third subnet')
param subnet3AddressSpace string = '10.0.3.0/24'

@description('The location for all resources')
param location string = resourceGroup().location

@description('Tags to apply to all resources')
param tags object = {
  project: 'trading-ai-assist'
  environment: 'production'
  managedBy: 'bicep'
}

// Virtual Network
resource virtualNetwork 'Microsoft.Network/virtualNetworks@2023-09-01' = {
  name: virtualNetworkName
  location: location
  tags: tags
  properties: {
    addressSpace: {
      addressPrefixes: virtualNetworkAddressSpace
    }
    subnets: [
      {
        name: subnet1Name
        properties: {
          addressPrefix: subnet1AddressSpace
          networkSecurityGroup: {
            id: appSubnetNSG.id
          }
        }
      }
      {
        name: subnet2Name
        properties: {
          addressPrefix: subnet2AddressSpace
          networkSecurityGroup: {
            id: databaseSubnetNSG.id
          }
        }
      }
      {
        name: subnet3Name
        properties: {
          addressPrefix: subnet3AddressSpace
          networkSecurityGroup: {
            id: managementSubnetNSG.id
          }
        }
      }
    ]
  }
}

// Network Security Group for App Subnet
resource appSubnetNSG 'Microsoft.Network/networkSecurityGroups@2023-09-01' = {
  name: '${virtualNetworkName}-app-nsg'
  location: location
  tags: tags
  properties: {
    securityRules: [
      {
        name: 'AllowHTTP'
        properties: {
          priority: 1000
          protocol: 'Tcp'
          access: 'Allow'
          direction: 'Inbound'
          sourceAddressPrefix: 'Internet'
          sourcePortRange: '*'
          destinationAddressPrefix: '*'
          destinationPortRange: '80'
        }
      }
      {
        name: 'AllowHTTPS'
        properties: {
          priority: 1001
          protocol: 'Tcp'
          access: 'Allow'
          direction: 'Inbound'
          sourceAddressPrefix: 'Internet'
          sourcePortRange: '*'
          destinationAddressPrefix: '*'
          destinationPortRange: '443'
        }
      }
      {
        name: 'AllowInternal'
        properties: {
          priority: 1002
          protocol: '*'
          access: 'Allow'
          direction: 'Inbound'
          sourceAddressPrefix: 'VirtualNetwork'
          sourcePortRange: '*'
          destinationAddressPrefix: '*'
          destinationPortRange: '*'
        }
      }
    ]
  }
}

// Network Security Group for Database Subnet
resource databaseSubnetNSG 'Microsoft.Network/networkSecurityGroups@2023-09-01' = {
  name: '${virtualNetworkName}-database-nsg'
  location: location
  tags: tags
  properties: {
    securityRules: [
      {
        name: 'AllowAppSubnet'
        properties: {
          priority: 1000
          protocol: 'Tcp'
          access: 'Allow'
          direction: 'Inbound'
          sourceAddressPrefix: subnet1AddressSpace
          sourcePortRange: '*'
          destinationAddressPrefix: '*'
          destinationPortRange: '1433'
        }
      }
      {
        name: 'AllowRedis'
        properties: {
          priority: 1001
          protocol: 'Tcp'
          access: 'Allow'
          direction: 'Inbound'
          sourceAddressPrefix: subnet1AddressSpace
          sourcePortRange: '*'
          destinationAddressPrefix: '*'
          destinationPortRange: '6379'
        }
      }
      {
        name: 'DenyInternet'
        properties: {
          priority: 4096
          protocol: '*'
          access: 'Deny'
          direction: 'Inbound'
          sourceAddressPrefix: 'Internet'
          sourcePortRange: '*'
          destinationAddressPrefix: '*'
          destinationPortRange: '*'
        }
      }
    ]
  }
}

// Network Security Group for Management Subnet
resource managementSubnetNSG 'Microsoft.Network/networkSecurityGroups@2023-09-01' = {
  name: '${virtualNetworkName}-management-nsg'
  location: location
  tags: tags
  properties: {
    securityRules: [
      {
        name: 'AllowSSH'
        properties: {
          priority: 1000
          protocol: 'Tcp'
          access: 'Allow'
          direction: 'Inbound'
          sourceAddressPrefix: 'Internet'
          sourcePortRange: '*'
          destinationAddressPrefix: '*'
          destinationPortRange: '22'
        }
      }
      {
        name: 'AllowRDP'
        properties: {
          priority: 1001
          protocol: 'Tcp'
          access: 'Allow'
          direction: 'Inbound'
          sourceAddressPrefix: 'Internet'
          sourcePortRange: '*'
          destinationAddressPrefix: '*'
          destinationPortRange: '3389'
        }
      }
    ]
  }
}

// Application Gateway
resource applicationGateway 'Microsoft.Network/applicationGateways@2023-09-01' = {
  name: '${virtualNetworkName}-appgw'
  location: location
  tags: tags
  properties: {
    sku: {
      name: 'Standard_v2'
      tier: 'Standard_v2'
      capacity: 2
    }
    gatewayIPConfigurations: [
      {
        name: 'appGatewayIpConfig'
        properties: {
          subnet: {
            id: virtualNetwork.properties.subnets[0].id
          }
        }
      }
    ]
    frontendIPConfigurations: [
      {
        name: 'appGwFrontendIP'
        properties: {
          publicIPAddress: {
            id: appGatewayPublicIP.id
          }
        }
      }
    ]
    frontendPorts: [
      {
        name: 'appGwFrontendPort'
        properties: {
          port: 80
        }
      }
      {
        name: 'appGwFrontendPortSSL'
        properties: {
          port: 443
        }
      }
    ]
    backendAddressPools: [
      {
        name: 'appGatewayBackendPool'
        properties: {
          backendAddresses: []
        }
      }
    ]
    backendHttpSettingsCollection: [
      {
        name: 'appGatewayBackendHttpSettings'
        properties: {
          port: 80
          protocol: 'Http'
          cookieBasedAffinity: 'Disabled'
          requestTimeout: 30
        }
      }
    ]
    httpListeners: [
      {
        name: 'appGatewayHttpListener'
        properties: {
          frontendIPConfiguration: {
            id: applicationGateway.properties.frontendIPConfigurations[0].id
          }
          frontendPort: {
            id: applicationGateway.properties.frontendPorts[0].id
          }
          protocol: 'Http'
        }
      }
    ]
    requestRoutingRules: [
      {
        name: 'appGatewayRoutingRule'
        properties: {
          ruleType: 'Basic'
          httpListener: {
            id: applicationGateway.properties.httpListeners[0].id
          }
          backendAddressPool: {
            id: applicationGateway.properties.backendAddressPools[0].id
          }
          backendHttpSettings: {
            id: applicationGateway.properties.backendHttpSettingsCollection[0].id
          }
        }
      }
    ]
  }
}

// Public IP for Application Gateway
resource appGatewayPublicIP 'Microsoft.Network/publicIPAddresses@2023-09-01' = {
  name: '${virtualNetworkName}-appgw-pip'
  location: location
  tags: tags
  properties: {
    publicIPAllocationMethod: 'Static'
    sku: {
      name: 'Standard'
    }
  }
}

// Load Balancer
resource loadBalancer 'Microsoft.Network/loadBalancers@2023-09-01' = {
  name: '${virtualNetworkName}-lb'
  location: location
  tags: tags
  properties: {
    frontendIPConfigurations: [
      {
        name: 'loadBalancerFrontEnd'
        properties: {
          publicIPAddress: {
            id: loadBalancerPublicIP.id
          }
        }
      }
    ]
    backendAddressPools: [
      {
        name: 'loadBalancerBackEndPool'
        properties: {}
      }
    ]
    loadBalancingRules: [
      {
        name: 'LBRule'
        properties: {
          frontendIPConfiguration: {
            id: loadBalancer.properties.frontendIPConfigurations[0].id
          }
          backendAddressPool: {
            id: loadBalancer.properties.backendAddressPools[0].id
          }
          frontendPort: 80
          backendPort: 80
          enableFloatingIP: false
          idleTimeoutInMinutes: 5
          protocol: 'Tcp'
          probe: {
            id: loadBalancer.properties.probes[0].id
          }
        }
      }
    ]
    probes: [
      {
        name: 'healthProbe'
        properties: {
          protocol: 'Http'
          port: 80
          intervalInSeconds: 15
          numberOfProbes: 2
          requestPath: '/health'
        }
      }
    ]
  }
}

// Public IP for Load Balancer
resource loadBalancerPublicIP 'Microsoft.Network/publicIPAddresses@2023-09-01' = {
  name: '${virtualNetworkName}-lb-pip'
  location: location
  tags: tags
  properties: {
    publicIPAllocationMethod: 'Static'
    sku: {
      name: 'Standard'
    }
  }
}

// Outputs
output virtualNetworkName string = virtualNetwork.name
output virtualNetworkId string = virtualNetwork.id
output appSubnetId string = virtualNetwork.properties.subnets[0].id
output databaseSubnetId string = virtualNetwork.properties.subnets[1].id
output managementSubnetId string = virtualNetwork.properties.subnets[2].id
output applicationGatewayName string = applicationGateway.name
output applicationGatewayPublicIP string = appGatewayPublicIP.properties.ipAddress
output loadBalancerName string = loadBalancer.name
output loadBalancerPublicIP string = loadBalancerPublicIP.properties.ipAddress 