param location string = resourceGroup().location
param namespaceName string
param eventHubName string = 'telemetry'
param storageAccountName string

resource namespace 'Microsoft.EventHub/namespaces@2024-01-01' = {
  name: namespaceName
  location: location
  sku: { name: 'Standard', tier: 'Standard', capacity: 1 }
  properties: {
    disableLocalAuth: true
    isAutoInflateEnabled: true
    maximumThroughputUnits: 4
    publicNetworkAccess: 'Enabled'
    minimumTlsVersion: '1.2'
  }
}

resource hub 'Microsoft.EventHub/namespaces/eventhubs@2024-01-01' = {
  parent: namespace
  name: eventHubName
  properties: { partitionCount: 4, messageRetentionInDays: 7 }
}

resource consumerGroup 'Microsoft.EventHub/namespaces/eventhubs/consumergroups@2024-01-01' = {
  parent: hub
  name: 'telemetry-processor'
  properties: { userMetadata: 'Independent checkpoint scope for the sample processor' }
}

resource storage 'Microsoft.Storage/storageAccounts@2023-05-01' = {
  name: storageAccountName
  location: location
  kind: 'StorageV2'
  sku: { name: 'Standard_LRS' }
  properties: { allowBlobPublicAccess: false, minimumTlsVersion: 'TLS1_2', supportsHttpsTrafficOnly: true }
}

output fullyQualifiedNamespace string = '${namespace.name}.servicebus.windows.net'
output eventHub string = hub.name
output checkpointStorageId string = storage.id
