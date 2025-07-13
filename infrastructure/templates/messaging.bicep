// Azure Service Bus and Event Grid Bicep Template
// Deploys Service Bus namespace with topics/subscriptions and Event Grid topics
// Parameters allow customization for environment

@description('Name of the Service Bus namespace')
param serviceBusNamespaceName string

@description('Name of the Event Grid topic')
param eventGridTopicName string

@description('Azure region for deployment')
param location string = resourceGroup().location

// Service Bus Namespace
resource serviceBusNamespace 'Microsoft.ServiceBus/namespaces@2022-10-01-preview' = {
  name: serviceBusNamespaceName
  location: location
  sku: {
    name: 'Standard'
    tier: 'Standard'
  }
  properties: {
    // Additional properties can be added here
  }
}

// Service Bus Topics
resource userEventsTopic 'Microsoft.ServiceBus/namespaces/topics@2022-10-01-preview' = {
  name: '${serviceBusNamespace.name}/user-events'
  properties: {
    maxSizeInMegabytes: 5120
    defaultMessageTimeToLive: 'P14D'
    enableBatchedOperations: true
  }
  dependsOn: [serviceBusNamespace]
}

resource tradingEventsTopic 'Microsoft.ServiceBus/namespaces/topics@2022-10-01-preview' = {
  name: '${serviceBusNamespace.name}/trading-events'
  properties: {
    maxSizeInMegabytes: 5120
    defaultMessageTimeToLive: 'P14D'
    enableBatchedOperations: true
  }
  dependsOn: [serviceBusNamespace]
}

resource notificationEventsTopic 'Microsoft.ServiceBus/namespaces/topics@2022-10-01-preview' = {
  name: '${serviceBusNamespace.name}/notification-events'
  properties: {
    maxSizeInMegabytes: 5120
    defaultMessageTimeToLive: 'P14D'
    enableBatchedOperations: true
  }
  dependsOn: [serviceBusNamespace]
}

// Service Bus Subscriptions
resource userEventsSubscription 'Microsoft.ServiceBus/namespaces/topics/subscriptions@2022-10-01-preview' = {
  name: '${userEventsTopic.name}/user-events-subscription'
  properties: {
    maxDeliveryCount: 10
    lockDuration: 'PT1M'
    defaultMessageTimeToLive: 'P14D'
    enableBatchedOperations: true
  }
  dependsOn: [userEventsTopic]
}

resource tradingEventsSubscription 'Microsoft.ServiceBus/namespaces/topics/subscriptions@2022-10-01-preview' = {
  name: '${tradingEventsTopic.name}/trading-events-subscription'
  properties: {
    maxDeliveryCount: 10
    lockDuration: 'PT1M'
    defaultMessageTimeToLive: 'P14D'
    enableBatchedOperations: true
  }
  dependsOn: [tradingEventsTopic]
}

resource notificationEventsSubscription 'Microsoft.ServiceBus/namespaces/topics/subscriptions@2022-10-01-preview' = {
  name: '${notificationEventsTopic.name}/notification-events-subscription'
  properties: {
    maxDeliveryCount: 10
    lockDuration: 'PT1M'
    defaultMessageTimeToLive: 'P14D'
    enableBatchedOperations: true
  }
  dependsOn: [notificationEventsTopic]
}

// Event Grid Topic
resource eventGridTopic 'Microsoft.EventGrid/topics@2022-06-15' = {
  name: eventGridTopicName
  location: location
  properties: {
    // Additional properties can be added here
  }
}

// Outputs for use in other templates or applications
output serviceBusConnectionString string = serviceBusNamespace.properties.serviceBusEndpoint
output eventGridTopicEndpoint string = eventGridTopic.properties.endpoint 