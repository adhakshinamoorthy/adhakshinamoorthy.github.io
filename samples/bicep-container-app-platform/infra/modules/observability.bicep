param location string
param prefix string
param tags object

resource workspace 'Microsoft.OperationalInsights/workspaces@2022-10-01' = {
  name: 'log-${prefix}'
  location: location
  tags: tags
  properties: {
    retentionInDays: 30
    features: { enableLogAccessUsingOnlyResourcePermissions: true }
  }
}

resource insights 'Microsoft.Insights/components@2020-02-02' = {
  name: 'appi-${prefix}'
  location: location
  kind: 'web'
  tags: tags
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: workspace.id
  }
}

output workspaceName string = workspace.name
output applicationInsightsConnectionString string = insights.properties.ConnectionString
