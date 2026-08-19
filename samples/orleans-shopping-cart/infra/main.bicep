param location string = resourceGroup().location
@minLength(3)
@maxLength(12)
param namePrefix string = 'orleanscart'

resource storage 'Microsoft.Storage/storageAccounts@2023-05-01' = {
  name: take('orl${replace(namePrefix, '-', '')}${uniqueString(resourceGroup().id)}', 24)
  location: location
  kind: 'StorageV2'
  sku: { name: 'Standard_LRS' }
  properties: {
    allowBlobPublicAccess: false
    allowSharedKeyAccess: false
    minimumTlsVersion: 'TLS1_2'
    supportsHttpsTrafficOnly: true
  }
}

resource logs 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {
  name: '${namePrefix}-logs'
  location: location
  properties: { retentionInDays: 30 }
}

output storageAccountName string = storage.name
output logWorkspaceId string = logs.id
