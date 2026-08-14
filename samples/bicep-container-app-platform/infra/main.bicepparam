using './main.bicep'

param environment = 'dev'
param workloadName = 'catalog'
param containerImage = 'mcr.microsoft.com/dotnet/samples:aspnetapp'
param externalApiKey = readEnvironmentVariable('EXTERNAL_API_KEY')
param tags = {
  owner: 'platform-team'
  costCenter: 'learning'
}
