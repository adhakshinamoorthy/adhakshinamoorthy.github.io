targetScope = 'resourceGroup'

@description('Azure region for every resource.')
param location string = resourceGroup().location

@allowed(['dev', 'test', 'prod'])
@description('Deployment environment used in naming and policy.')
param environment string

@description('Short lowercase workload name.')
@minLength(3)
@maxLength(18)
param workloadName string = 'catalog'

@description('Immutable container image tag or digest.')
param containerImage string

@secure()
@description('Example runtime secret. Prefer Key Vault references in production.')
param externalApiKey string

@description('Governance tags merged with required values.')
param tags object = {}

var prefix = '${workloadName}-${environment}'
var requiredTags = {
  environment: environment
  managedBy: 'bicep'
  workload: workloadName
}
var allTags = union(tags, requiredTags)

module observability 'modules/observability.bicep' = {
  name: 'observability-${uniqueString(resourceGroup().id, prefix)}'
  params: {
    location: location
    prefix: prefix
    tags: allTags
  }
}

module environmentModule 'modules/environment.bicep' = {
  name: 'environment-${uniqueString(resourceGroup().id, prefix)}'
  params: {
    location: location
    prefix: prefix
    logAnalyticsWorkspaceName: observability.outputs.workspaceName
    tags: allTags
  }
}

module application 'modules/container-app.bicep' = {
  name: 'application-${uniqueString(resourceGroup().id, prefix)}'
  params: {
    location: location
    prefix: prefix
    environmentId: environmentModule.outputs.environmentId
    containerImage: containerImage
    externalApiKey: externalApiKey
    applicationInsightsConnectionString: observability.outputs.applicationInsightsConnectionString
    minReplicas: environment == 'prod' ? 2 : 1
    tags: allTags
  }
}

output containerAppName string = application.outputs.containerAppName
output containerAppFqdn string = application.outputs.fqdn
output principalId string = application.outputs.principalId
