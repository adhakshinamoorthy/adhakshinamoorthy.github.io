param location string = resourceGroup().location
param namePrefix string = 'microorders'

resource logs 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {
  name: '${namePrefix}-logs'
  location: location
  properties: { retentionInDays: 30 }
}

resource bus 'Microsoft.ServiceBus/namespaces@2024-01-01' = {
  name: '${namePrefix}-${uniqueString(resourceGroup().id)}'
  location: location
  sku: { name: 'Standard', tier: 'Standard' }
  properties: { disableLocalAuth: true }
}

resource orders 'Microsoft.ServiceBus/namespaces/queues@2024-01-01' = {
  parent: bus
  name: 'order-placed'
  properties: {
    deadLetteringOnMessageExpiration: true
    maxDeliveryCount: 10
  }
}

output namespaceName string = bus.name
output logWorkspaceId string = logs.id
