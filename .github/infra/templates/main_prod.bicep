@description('Name of the web app')
param webAppName string

@description('Location of all resources')
param location string = resourceGroup().location

@description('SQL admin username')
param sqlAdminUser string = 'ticketifyne-admin'

@description('SQL admin password - pass via parameter (secret)')
@secure()
param sqlAdminPassword string

/*
@description('ApplicationInsights name')
param appInsightsName string
*/

@description('App Service Plan SKU')
param skuName string = 'P1v2'

// --- SQL server
resource sqlServer 'Microsoft.Sql/servers@2022-02-01-preview' = {
    name: '${webAppName}-sql'
    location: location
    properties: {
        administratorLogin: sqlAdminUser
        administratorLoginPassword: sqlAdminPassword
        version: '12.0'
    }
    sku: {
    name: 'GP_Gen5_2' // managed instance SKU;
    }
}  

// SQL Firewall rule - Allow Azure services
resource sqlFirewallRule 'Microsoft.Sql/servers/firewallRules@2022-02-01-preview' = {
  parent: sqlServer
  name: 'AllowAzureServices'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

// SQL database
resource sqlDatabase 'Microsoft.Sql/servers/databases@2021-05-01-preview' = {
  parent: sqlServer
  location: location
  name: 'TicketifyDb'
  properties: {
    sku: {
      name: 'Basic'
      tier: 'Basic'
    }
    maxSizeBytes: 2147483648
    collation: 'SQL_Latin1_General_CP1_CI_AS'
  }
  sku: {
    name: 'Basic'
  }
}

// --- App Service Plan
resource appServicePlan 'Microsoft.Web/serverfarms@2024-11-01' = {
  name: '${webAppName}-plan'
  location: location
  sku: {
    name: 'B1'
    tier: 'basic'
    size: 'B1'
    family: 'B'
    capacity: 1
  }
  kind: 'linux'
  properties: {
    reserved: true
  }
}

// --- Web App (Linux, container/no container - we'll deploy code)
resource webApp 'Microsoft.Web/sites@2024-11-01' = {
  name: webAppName
  location: location
  kind: 'app,linux'
  properties: {
    serverFarmId: appServicePlan.id
    httpsOnly: true
    siteConfig: {
        linuxFxVersion: 'DOTNETCORE|9.0'
        alwaysOn: false
        ftpsState: 'Disabled'
        minTlsVersion: '1.2'
        appSettings: [
            {
                name: 'ASPNETCORE_ENVIRONMENT'
                value: 'Production'
            }
            {
                name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
                value: '' // replaced by update below
            }
        ]
        
    }
  }
  dependsOn: [
    appServicePlan
  ]
}

// --- Compose SQL connection string
var sqlServerFullyQualified = '${sqlServer.name}.database.windows.net'
var sqlConnectionString = 'Server=tcp:${sqlServerFullyQualified},1433;Initial Catalog=${sqlDatabase.name};User ID=${sqlAdminUser};Password=${sqlAdminPassword};Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;'

// set web app settings using a separate resource to reference outputs
resource webAppConfig 'Microsoft.Web/sites/config@2021-02-01' = {
  name: '${webApp.name}/appsettings'
  properties: {
    'ConnectionStrings__DefaultConnection': sqlConnectionString
    //"ApplicationInsights__ConnectionString": appInsights.properties.ConnectionString
    'ASPNETCORE_ENVIRONMENT': 'Production'
  }
  dependsOn: [
    webApp
    //appInsights
    sqlDatabase
  ]
}

/*
In Bicep (and ARM), the output keyword lets you export values from your deployment — values that can then be:
viewed in the Azure Portal after deployment,
referenced in other Bicep files (if you do modular deployments),
or even used in your GitHub Actions or Azure DevOps pipelines.
*/
// --- Outputs
output webAppDefaultHostName string = webApp.properties.defaultHostName
//output appInsightsConnectionString string = appInsights.properties.ConnectionString
output sqlServerName string = sqlServer.name
output dbName string = sqlDatabase.name
output sqlConnectionStringOutput string = sqlConnectionString


// --- Application Insights
/*
resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: appInsightsName
  location: location 
  kind: 'web'
  properties: {
    Application_Type: 'web'
    publicNetworkAccessForIngestion: 'Enabled'
    publicNetworkAccessForQuery: 'Enabled'
  }
}
*/


