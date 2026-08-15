param location string = resourceGroup().location
param publisherEmail string
param publisherName string
param serviceName string
param backendUrl string

resource apim 'Microsoft.ApiManagement/service@2024-05-01' = {
  name: serviceName
  location: location
  sku: { name: 'Developer', capacity: 1 }
  properties: { publisherEmail: publisherEmail, publisherName: publisherName }
}

resource api 'Microsoft.ApiManagement/service/apis@2024-05-01' = {
  parent: apim
  name: 'orders'
  properties: {
    displayName: 'Orders API'
    path: 'orders'
    protocols: [ 'https' ]
    serviceUrl: backendUrl
    subscriptionRequired: false
    type: 'http'
  }
}

output gatewayUrl string = apim.properties.gatewayUrl
output apiName string = api.name
