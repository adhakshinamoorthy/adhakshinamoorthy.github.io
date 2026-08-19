param location string = resourceGroup().location
param workflowName string = 'order-approval'
@secure()
param callbackBaseUrl string

var workflow = loadJsonContent('../workflows/OrderApproval/workflow.json')

resource orderApproval 'Microsoft.Logic/workflows@2019-05-01' = {
  name: workflowName
  location: location
  identity: { type: 'SystemAssigned' }
  properties: {
    state: 'Enabled'
    definition: workflow.definition
    parameters: {
      callbackBaseUrl: { value: callbackBaseUrl }
    }
  }
}

output workflowId string = orderApproval.id
output principalId string = orderApproval.identity.principalId
