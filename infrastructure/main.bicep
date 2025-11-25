targetScope = 'subscription'

@description('The environment for the deployment.')
param environment 'prod' | 'test' | 'dev'

@description('The address prefix for the virtual network. Contact Azure owner for your organization to avoid conflicts.')
param addressPrefix string

//@description('The container image version to deploy to the container app.')
//param imageVersion string

@description('Tags to apply to the resource group for management and organization.')
param tags object = {}

@description('The location for all resources. Should be left to default since it\'s the deployment that decides the location.')
param location string = deployment().location

var workload string = 'salesforce-to-teamtailor'
var owner string = 'patric.jansson@magello.se'

var defaultTags = union(tags, {
  environment: environment
  owner: owner
  slack: '#project-teamtailor-salesforce'
})

resource resourceGroup 'Microsoft.Resources/resourceGroups@2025-04-01' = {
  name: 'rg-${workload}-${environment}'
  location: location
  tags: defaultTags
}

module network 'br:crmagello.azurecr.io/bicep/spoke-vnet:latest' = {
  name: 'DeployNetwork'
  scope: resourceGroup
  params: {
    workload: workload
    environment: environment
    addressPrefixes: [
      addressPrefix
    ]
    subnets: [
      {
        name: 'private-endpoints'
        addressPrefix: cidrSubnet(addressPrefix, 27, 0)
      }
    ]
  }
}

var privateEndpointSubnet = last(filter(
  network.outputs.vnet.subnets,
  subnet => contains(subnet.name, 'private-endpoints')
))!

module kv 'br:crmagello.azurecr.io/bicep/keyvault:latest' = {
  name: 'DeployKeyVault'
  scope: resourceGroup
  params: {
    workload: 'sf-to-tt' // Shortened name due to Key Vault name restrictions
    environment: environment
    privateEndpointSubnetId: privateEndpointSubnet.id
    allowContainerAppEnvironmentAccess: true
  }
}

module storage 'br:crmagello.azurecr.io/bicep/storage:latest' = {
  name: 'DeployStorageAccount'
  scope: resourceGroup
  params: {
    environment: environment
    workload: 'sf-to-tt' // Shortened name due to Key Vault name restrictions
    redundancy: environment == 'prod' ? 'ZRS' : 'LRS'
    privateEndpointSubnetId: privateEndpointSubnet.id
    allowContainerAppEnvironmentAccess: true
    storageServices: [
      'blob'
      'queue'
      'table'
    ]
    keyVaultName: kv.outputs.keyVaultName
    tables: [
      {
        name: 'Applications'
      }
    ]
  }
}
