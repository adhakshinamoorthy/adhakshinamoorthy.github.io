param location string = resourceGroup().location
param storeName string
param workloadPrincipalId string

resource store 'Microsoft.AppConfiguration/configurationStores@2024-05-01' = {
  name: storeName
  location: location
  sku: { name: 'standard' }
  properties: { disableLocalAuth: true, publicNetworkAccess: 'Disabled', softDeleteRetentionInDays: 7, enablePurgeProtection: true }
}

resource dataReaderRole 'Microsoft.Authorization/roleDefinitions@2022-04-01' existing = {
  scope: subscription()
  name: '516239f1-63e1-4d78-a4de-a74fb236a071'
}
resource workloadAccess 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: store
  name: guid(store.id, workloadPrincipalId, dataReaderRole.id)
  properties: { principalId: workloadPrincipalId, principalType: 'ServicePrincipal', roleDefinitionId: dataReaderRole.id }
}

output endpoint string = store.properties.endpoint
