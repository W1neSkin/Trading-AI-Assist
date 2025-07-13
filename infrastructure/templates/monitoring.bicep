// Azure Monitor and Log Analytics Bicep Template
// Deploys Log Analytics workspace and Application Insights
// Parameters allow customization for environment

@description('Name of the Log Analytics workspace')
param logAnalyticsWorkspaceName string

@description('Name of the Application Insights resource')
param applicationInsightsName string

@description('Retention period in days for Log Analytics')
param retentionInDays int = 30

@description('Azure region for deployment')
param location string = resourceGroup().location

// Log Analytics Workspace
resource logAnalyticsWorkspace 'Microsoft.OperationalInsights/workspaces@2022-10-01' = {
  name: logAnalyticsWorkspaceName
  location: location
  properties: {
    sku: {
      name: 'PerGB2018'
    }
    retentionInDays: retentionInDays
    features: {
      enableLogAccessUsingOnlyResourcePermissions: true
    }
  }
}

// Application Insights
resource applicationInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: applicationInsightsName
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: logAnalyticsWorkspace.id
  }
  dependsOn: [logAnalyticsWorkspace]
}

// Action Group for alerts
resource actionGroup 'Microsoft.Insights/actionGroups@2022-06-01' = {
  name: '${applicationInsightsName}-action-group'
  location: 'Global'
  properties: {
    groupShortName: 'TradingAI'
    enabled: true
    emailReceivers: [
      {
        name: 'admin-email'
        emailAddress: 'admin@tradingaiassist.com'
        useCommonAlertSchema: true
      }
    ]
  }
}

// Alert rule for high CPU usage
resource cpuAlertRule 'Microsoft.Insights/metricAlerts@2018-03-01' = {
  name: '${applicationInsightsName}-cpu-alert'
  location: 'Global'
  properties: {
    description: 'Alert when CPU usage is high'
    severity: 2
    enabled: true
    scopes: [
      applicationInsights.id
    ]
    evaluationFrequency: 'PT5M'
    windowSize: 'PT15M'
    criteria: {
      'odata.type': 'Microsoft.Azure.Monitor.SingleResourceMultipleMetricCriteria'
      allOf: [
        {
          name: 'HighCPU'
          metricName: 'cpu_percentage'
          operator: 'GreaterThan'
          threshold: 80
          timeAggregation: 'Average'
        }
      ]
    }
    actions: [
      {
        actionGroupId: actionGroup.id
      }
    ]
  }
  dependsOn: [applicationInsights, actionGroup]
}

// Outputs for use in other templates or applications
output logAnalyticsWorkspaceId string = logAnalyticsWorkspace.id
output applicationInsightsInstrumentationKey string = applicationInsights.properties.InstrumentationKey 