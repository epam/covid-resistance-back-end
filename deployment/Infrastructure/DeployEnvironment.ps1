# https://docs.microsoft.com/en-us/cli/azure/reference-index?view=azure-cli-latest

param ($product, $environment, $settingsKey)
if ($null -eq $product) {
    $product = read-host -Prompt "Please enter the name of the product (e.g. epmcovi)"
}
if ($null -eq $environment) {
    $environment = read-host -Prompt "Please enter the name of the environment (e.g. dev1, tst1, mvp1, ...)"
}
if ($null -eq $settingsKey){
    $settingsKey = "dev"
}

function DeployEnvironment {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [ValidateSet('tst1', 'tst2', 'tst3', 'mvp1', 'dev1')]
        $nc_Environment,
        [Parameter(Mandatory = $true)]
        [ValidateSet('epmcovi', 'homecovi')] # dont use epmcovi on other subsciptions
        $nc_Product,
        [Parameter(Mandatory = $true)]
        $nc_SettingsKey
    )

    $subscriptionId =  $(az account show --query id -otsv)
    Write-Host "Subscription ID: $subscriptionId " -ForegroundColor Green

    $settings = Get-Settings($nc_SettingsKey)
    Validate-Settings($settings)

# Generating names according to 'Naming convention. Azure Resources.docx)

    $nc_Instance = "01" 
    $nc_Instance2 = "02" 
    $nc_Region = $settings.region.key

    $location = $settings.region.name
    $locationPostgreSQL = $settings.region.name
    $locationCosmosDB = $settings.region.name

    $resourceGroupName = "rg-$nc_Region-$nc_Environment-$nc_Product-$nc_Instance"

    $storageAccountName = ("sa" + $nc_Region + $nc_Environment + $nc_Product + $nc_Instance + "app").ToLower()
    $storageAccountWebJobsName = ("sa" + $nc_Region + $nc_Environment + $nc_Product + $nc_Instance2 + "app").ToLower()

    $keyVaultName = "kv-$nc_Region-$nc_Environment-$nc_Product-$nc_Instance"
    $keyVaultCertificateName = "cert-$nc_Region-$nc_Environment-$nc_Product-$nc_Instance"

    $appInsightsName = "ai-$nc_Region-$nc_Environment-$nc_Product-$nc_Instance"

    $serviceBusNamespaceName = "sbus-$nc_Region-$nc_Environment-$nc_Product-$nc_Instance"
    $serviceBusTopicName1 = "contacts"
    $serviceBusTopicName2 = "notifications"

    $appServicePlanName = "asp-$nc_Region-$nc_Environment-$nc_Product-$nc_Instance"
    $webAppInfectionName = "wa-$nc_Region-$nc_Environment-$nc_Product-infection-$nc_Instance"
    $webAppIsName = "wa-$nc_Region-$nc_Environment-$nc_Product-is-$nc_Instance"
    $webAppUserName = "wa-$nc_Region-$nc_Environment-$nc_Product-user-$nc_Instance"

    $cosmosDBaccountName = ("cassandra-$nc_Region-$nc_Environment-$nc_Product-$nc_Instance").ToLower()

    $appConfigName = "config-$nc_Region-$nc_Environment-$nc_Product-$nc_Instance"

    $postgreSQLServerName = "sql-$nc_Region-$nc_Environment-$nc_Product-$nc_Instance"

    $notificationHubsNamespaceName = "nhub-$nc_Region-$nc_Environment-$nc_Product-$nc_Instance"
    $notificationHubName = $settings.notificationHub.name

    $APIManagementServiceName = "api-$nc_Region-$nc_Environment-$nc_Product-$nc_Instance"

    $functionAppInfectionName = "func-$nc_Region-$nc_Environment-$nc_Product-infection-$nc_Instance"
    $functionAppNotificationName = "func-$nc_Region-$nc_Environment-$nc_Product-notification-$nc_Instance"

# Other variables
    $clientId = "ed82e190-d95e-403e-bc8f-ca0d932b02c1"
    $scope = "openid offline_access userApi infectionApi apim"
    $secret = "secret"


################################################################# 
#    Creating Resource Group
#################################################################
    Write-Host "Creating Resource Group:" -ForegroundColor Green

    az group create --location $location --name $resourceGroupName


################################################################# 
#    Deployment App Service Plan - moved to ARM Template Templates/NetCoreApp.json
#################################################################
#    az appservice plan create --name $appServicePlanName --resource-group $resourceGroupName --sku S1
 
  
################################################################# 
#    Creating Storage Account 1  BLOB
#################################################################
    Write-Host "Creating Storage Account 1  BLOB:" -ForegroundColor Green

    az storage account create --name $storageAccountName --resource-group $resourceGroupName --location $location `
        --access-tier Hot `
        --sku Standard_LRS `
        --kind BlobStorage

    # Creating container in storage account
    az storage container create --name meetings --account-name $storageAccountName `
        --public-access off
    az storage container create --name recommendations --account-name $storageAccountName `
        --public-access blob

    $storageAccountConnectionString = $(az storage account show-connection-string --name $storageAccountName --resource-group $resourceGroupName --query "connectionString")
    Write-Host "Storage Account Connection String: $storageAccountConnectionString" -ForegroundColor Cyan

    az storage blob upload --container-name recommendations --account-name $storageAccountName --auth-mode key --connection-string $storageAccountConnectionString `
        --file Configs/recommendations-1.json `
        --name 1 `

    az storage blob upload --container-name recommendations --account-name $storageAccountName --auth-mode key --connection-string $storageAccountConnectionString `
        --file Configs/recommendations-2.json `
        --name 2 `

    az storage blob upload --container-name recommendations --account-name $storageAccountName --auth-mode key --connection-string $storageAccountConnectionString `
        --file Configs/recommendations-3.json `
        --name 3 `

    az storage blob upload --container-name recommendations --account-name $storageAccountName --auth-mode key --connection-string $storageAccountConnectionString `
        --file Configs/recommendations-4.json `
        --name 4 `


################################################################# 
#    Creating Storage Account 2 for Function Apps
#################################################################
    Write-Host "Creating Storage Account 2 for Function Apps:" -ForegroundColor Green

    az storage account create --name $storageAccountWebJobsName --resource-group $resourceGroupName --location $location `
        --access-tier Hot `
        --sku Standard_LRS `
        --kind StorageV2

    $storageAccountWebJobsConnectionString = $(az storage account show-connection-string --name $storageAccountWebJobsName --resource-group $resourceGroupName --query "connectionString")
    Write-Host "Storage Account Connection String: $storageAccountWebJobsConnectionString" -ForegroundColor Cyan


################################################################# 
#    Creating Key Vault
#################################################################
    Write-Host "Creating Key Vault:" -ForegroundColor Green

    az keyvault create --name $keyVaultName --resource-group $resourceGroupName --location $location `
       --enable-soft-delete true # https://github.com/Azure/azure-cli/issues/13247

    # Attach self-signed certificate to key vault. Cert will be regenerated every run
    az keyvault certificate get-default-policy > policy.json
    az keyvault certificate create --name $keyVaultCertificateName --vault-name $keyVaultName --policy `@policy.json


################################################################# 
#    Creating Service Bus Namespace and Topics
#################################################################
    Write-Host "Creating Service Bus Namespace and Topics:" -ForegroundColor Green

    az servicebus namespace create --name $serviceBusNamespaceName --resource-group $resourceGroupName --location $location `
        --sku Standard

    az servicebus topic create --namespace-name $serviceBusNamespaceName --resource-group $resourceGroupName `
        --name $serviceBusTopicName1
    az servicebus topic authorization-rule create --topic-name $serviceBusTopicName1 --namespace-name $serviceBusNamespaceName --resource-group $resourceGroupName `
        --name "Send" `
        --rights Send
    az servicebus topic authorization-rule create --topic-name $serviceBusTopicName1 --namespace-name $serviceBusNamespaceName --resource-group $resourceGroupName `
        --name "Listen" `
        --rights Listen
    az servicebus topic subscription create --topic-name $serviceBusTopicName1 --namespace-name $serviceBusNamespaceName --resource-group $resourceGroupName `
        --name "ExposedContacts"
    az servicebus topic subscription create --topic-name $serviceBusTopicName1 --namespace-name $serviceBusNamespaceName --resource-group $resourceGroupName `
        --name "Log"

    az servicebus topic create --namespace-name $serviceBusNamespaceName --resource-group $resourceGroupName `
        --name $serviceBusTopicName2
    az servicebus topic authorization-rule create --topic-name $serviceBusTopicName2 --namespace-name $serviceBusNamespaceName --resource-group $resourceGroupName `
        --name "Send" `
        --rights Send
    az servicebus topic authorization-rule create --topic-name $serviceBusTopicName2 --namespace-name $serviceBusNamespaceName --resource-group $resourceGroupName `
        --name "Listen" `
        --rights Listen
    az servicebus topic subscription create --topic-name $serviceBusTopicName2 --namespace-name $serviceBusNamespaceName --resource-group $resourceGroupName `
        --name "StatusNotification"

    $contactsTopicListenConnection = $(az servicebus topic authorization-rule keys list --name Listen --topic-name $serviceBusTopicName1 --namespace-name $serviceBusNamespaceName --resource-group $resourceGroupName `
          --query primaryConnectionString) -replace ";EntityPath=contacts", ""

    $contactsTopicSendConnection = $(az servicebus topic authorization-rule keys list --name Send --topic-name $serviceBusTopicName1 --namespace-name $serviceBusNamespaceName --resource-group $resourceGroupName `
          --query primaryConnectionString) -replace ";EntityPath=contacts", ""

    $notificationTopicListenConnection = $(az servicebus topic authorization-rule keys list --name Listen --topic-name $serviceBusTopicName2 --namespace-name $serviceBusNamespaceName --resource-group $resourceGroupName `
          --query primaryConnectionString) -replace ";EntityPath=notifications", ""

    $notificationTopicSendConnection = $(az servicebus topic authorization-rule keys list --name Send --topic-name $serviceBusTopicName2 --namespace-name $serviceBusNamespaceName --resource-group $resourceGroupName `
          --query primaryConnectionString) -replace ";EntityPath=notifications", ""


################################################################# 
#    Creating App Configuration
#################################################################
    Write-Host "Creating App Configuration:" -ForegroundColor Green

    az appconfig create --name $appConfigName --sku Standard --resource-group $resourceGroupName --location $location

    # import backend.json
    az appconfig kv import --yes --name $appConfigName `
        --source file --path Configs/backend.json --format json --separator ":" --prefix "backend:"

    az appconfig kv import --yes --name $appConfigName `
        --source file --path Configs/metadata.json --format json --separator ":" --prefix "metadata:"

    $appConfigConnectionString =  $(az appconfig credential list --name $appConfigName --resource-group $resourceGroupName --query [0]."connectionString")
    $appConfigUrl = "https://" + $appConfigName + ".azconfig.io"
    Write-Host "App Configuration Connection String: $appConfigConnectionString" -ForegroundColor Green


################################################################# 
#    Creating Application Insight
#################################################################
    Write-Host "Creating Application Insight:" -ForegroundColor Green

    az monitor app-insights component create --resource-group $resourceGroupName --location $location `
        --app $appInsightsName `

    $appInsightsKey = az resource show --name $appInsightsName --resource-group $resourceGroupName `
        --resource-type "Microsoft.Insights/components" `
        --query "properties.InstrumentationKey" -o tsv

    az appconfig kv set --name $appConfigName --yes `
        --key ApplicationInsights:InstrumentationKey `
        --value $appInsightsKey


################################################################# 
#    Creating CosmosDB account
#################################################################
    Write-Host "Creating CosmosDB account:" -ForegroundColor Green

    az cosmosdb create --name $cosmosDBaccountName --resource-group $resourceGroupName `
        --locations regionName=$locationCosmosDB failoverPriority=0 isZoneRedundant=False `
        --capabilities EnableCassandra

    # Store password in key vault
    az keyvault secret set `
        --vault-name $keyVaultName `
        --name Cassandra-Credentials-Password `
        --value $(az cosmosdb keys list --name $cosmosDBaccountName --resource-group $resourceGroupName --query "primaryMasterKey")

    # Create keyvault reference in app config
    az appconfig kv set-keyvault --yes `
        --name $appConfigName `
        --key "Cassandra:Credentials:Password" `
        --secret-identifier $(az keyvault secret show --vault-name $keyVaultName --name Cassandra-Credentials-Password --query "id") `

    # Store connection in app config 
    az appconfig kv import --yes --name $appConfigName `
        --source file --path Configs/cassandra-config.json --format json --separator ":"

    az appconfig kv set --name $appConfigName --yes `
        --key Cassandra:ContactPoints:0 `
        --value "$cosmosDBaccountName.cassandra.cosmos.azure.com"

    az appconfig kv set --name $appConfigName --yes `
        --key Cassandra:Credentials:UserName `
        --value $cosmosDBaccountName


################################################################# 
#    Creating PostgreSQL server 
#################################################################
    Write-Host "Creating PostgreSQL server:" -ForegroundColor Green

    $postgreSQLServerAdminPassword = Get-RandomCharacters -length 32 -characters 'ABCDEFGHKLMNOPRSTUVWXYZabcdefghiklmnoprstuvwxyz1234567890-'

    az postgres up --server-name $postgreSQLServerName --resource-group $resourceGroupName --location $locationPostgreSQL `
        --database-name coronaresistance `
        --admin-user $settings.postgres.adminName `
        --admin-password $postgreSQLServerAdminPassword `
        --sku-name $settings.postgres.sku `
        --version 10 `
        --ssl-enforcement Enabled

    $postgreSQLServerConnectionString = "Server=$postgreSQLServerName.postgres.database.azure.com;Database=coronaresistance;Port=5432;User Id=$postgreSQLServerAdminName@$postgreSQLServerName;Password=$postgreSQLServerAdminPassword;Ssl Mode=Require;"

    az keyvault secret set `
        --vault-name $keyVaultName `
        --name Identity-Connection-String `
        --value $postgreSQLServerConnectionString


################################################################# 
#    Creating Net Core Application - Is
#################################################################
    Write-Host "Creating Net Core Application - Is:" -ForegroundColor Green

#    Issue with netcore for Azure CLI:  az webapp create --name $webAppIsName --resource-group $resourceGroupName --plan $appServicePlanName 
    az deployment group create --resource-group $resourceGroupName `
        --name DeployNetCoreApp-Is `
        --template-file Templates/NetCoreApp.json `
        --parameters webAppName=$webAppIsName  AppServicePlanName=$appServicePlanName

    # scope should be restricted (only App Configuration resource)
    az webapp identity assign --name $webAppIsName --resource-group $resourceGroupName `
        --role "App Configuration Data Reader" `
        --scope /subscriptions/$subscriptionId/resourceGroups/$resourceGroupName  

    az keyvault set-policy --name $keyVaultName `
         --object-id $(az webapp identity show --name $webAppIsName --resource-group $resourceGroupName --query principalId) `
         --secret-permissions get

    az webapp config appsettings set --name $webAppIsName --resource-group $resourceGroupName `
        --settings KeyVault:BaseUrl=$(az keyvault show --name $keyVaultName --query properties.vaultUri)

    az webapp config appsettings set --name $webAppIsName --resource-group $resourceGroupName `
        --settings KeyVault:SecretName=$keyVaultCertificateName

    # AppConfig setting
    az webapp config appsettings set --name $webAppIsName --resource-group $resourceGroupName `
        --settings AppConfig:BaseUrl=$appConfigUrl

    az webapp config connection-string set --name $webAppIsName --resource-group $resourceGroupName `
        --connection-string-type PostgreSQL `
        --settings ConnectionStrings:IdentityConnectionString=$postgreSQLServerConnectionString

    az webapp config connection-string set --name $webAppIsName --resource-group $resourceGroupName `
        --connection-string-type PostgreSQL `
        --settings ConnectionStrings:IdentityServerConnectionString=$postgreSQLServerConnectionString

    az webapp config connection-string set --name $webAppIsName --resource-group $resourceGroupName `
        --connection-string-type Custom `
        --settings AppConfig=$appConfigConnectionString

    $webAppIsURL = "https://" + $(az webapp show --name $webAppIsName --resource-group $resourceGroupName --query hostNames[0])
    Write-Host "App Service 'IS' Connection String: $webAppIsURL" -ForegroundColor Cyan


################################################################# 
#    Deployment Net Core Application - Infection
#################################################################
    Write-Host "Creating Net Core Application - Infection:" -ForegroundColor Green

    az deployment group create --resource-group $resourceGroupName `
        --name DeployNetCoreApp-Infection `
        --template-file Templates/NetCoreApp.json `
        --parameters webAppName=$webAppInfectionName  AppServicePlanName=$appServicePlanName

    # scope should be restricted (only App Configuration resource)
    az webapp identity assign --name $webAppInfectionName --resource-group $resourceGroupName `
        --role "App Configuration Data Reader" `
        --scope /subscriptions/$subscriptionId/resourceGroups/$resourceGroupName  

    az keyvault set-policy --name $keyVaultName `
         --object-id $(az webapp identity show --name $webAppInfectionName --resource-group $resourceGroupName --query principalId) `
         --secret-permissions get

    # ApiIdentity:Authority setting
    az webapp config appsettings set --name $webAppInfectionName --resource-group $resourceGroupName `
        --settings ApiIdentity:Authority=$webAppIsURL

    az webapp config appsettings set --name $webAppInfectionName --resource-group $resourceGroupName `
        --settings ApiIdentity:ClientId="infectionApi"

    az webapp config appsettings set --name $webAppInfectionName --resource-group $resourceGroupName `
        --settings ApiIdentity:ClientSecret=$secret

    # AppConfig setting
    az webapp config appsettings set --name $webAppInfectionName --resource-group $resourceGroupName `
        --settings AppConfig=$appConfigConnectionString

    az webapp config appsettings set --name $webAppInfectionName --resource-group $resourceGroupName `
        --settings AppConfig:BaseUrl=$appConfigUrl

    # Blob:ConnectionString setting
    az webapp config appsettings set --name $webAppInfectionName --resource-group $resourceGroupName `
        --settings Blob:ConnectionString=$storageAccountConnectionString

    az webapp config appsettings set --name $webAppInfectionName --resource-group $resourceGroupName `
        --settings Blob:ContainerName="meetings"


################################################################# 
#    Creating Net Core Application - User
#################################################################
    Write-Host "Creating Net Core Application - User:" -ForegroundColor Green

    az deployment group create --resource-group $resourceGroupName `
        --name DeployNetCoreApp-User `
        --template-file Templates/NetCoreApp.json `
        --parameters webAppName=$webAppUserName  AppServicePlanName=$appServicePlanName

    # scope should be restricted (only App Configuration resource)
    az webapp identity assign --name $webAppUserName --resource-group $resourceGroupName `
        --role "App Configuration Data Reader" `
        --scope /subscriptions/$subscriptionId/resourceGroups/$resourceGroupName  

    az keyvault set-policy --name $keyVaultName `
         --object-id $(az webapp identity show --name $webAppUserName --resource-group $resourceGroupName --query principalId) `
         --secret-permissions get

    az webapp config appsettings set --name $webAppUserName --resource-group $resourceGroupName `
        --settings ApiIdentity:Authority=$webAppIsURL

    az webapp config appsettings set --name $webAppUserName --resource-group $resourceGroupName `
        --settings ApiIdentity:ClientId="userApi"

    az webapp config appsettings set --name $webAppUserName --resource-group $resourceGroupName `
        --settings ApiIdentity:ClientSecret=$secret

    az webapp config appsettings set --name $webAppUserName --resource-group $resourceGroupName `
        --settings ClientIdentity:Authority=$webAppIsURL

    az webapp config appsettings set --name $webAppUserName --resource-group $resourceGroupName `
        --settings ClientIdentity:ClientId=$clientId

    az webapp config appsettings set --name $webAppUserName --resource-group $resourceGroupName `
        --settings ClientIdentity:Scope=$scope

    # AppConfig setting
    az webapp config appsettings set --name $webAppUserName --resource-group $resourceGroupName `
        --settings AppConfig:BaseUrl=$appConfigUrl

    az webapp config connection-string set --name $webAppUserName --resource-group $resourceGroupName `
        --connection-string-type Custom `
        --settings AppConfig=$appConfigConnectionString

    az webapp config connection-string set --name $webAppUserName --resource-group $resourceGroupName `
        --connection-string-type PostgreSQL `
        --settings ConnectionStrings:IdentityConnectionString=$postgreSQLServerConnectionString


################################################################# 
#    Creating API Management Service
#################################################################
    Write-Host "Creating API Management Service:" -ForegroundColor Green

    $blobBaseUrl = "https://" + $storageAccountName + ".blob.core.windows.net"
    $identityServerBaseUrl = "https://" + $webAppIsName + ".azurewebsites.net"
    $infectionApiBaseUrl = "https://" + $webAppInfectionName + ".azurewebsites.net"
    $userApiBaseUrl = "https://" + $webAppUserName + ".azurewebsites.net"


    az deployment group create --resource-group $resourceGroupName `
        --name DeployAPIManagementService `
        --template-file Templates/APIManagementService.json `
        --parameters `
             APIManagementServiceName=$APIManagementServiceName `
             publisherEmail=test@epam.com `
             publisherName=EPAM `
             apiClientId="apim" `
             apiClientSecret=$secret `
             blobBaseUrl=$blobBaseUrl `
             clientId=$clientId `
             identityServerBaseUrl=$identityServerBaseUrl `
             infectionApiBaseUrl=$infectionApiBaseUrl  `
             scope=$scope `
             userApiBaseUrl=$userApiBaseUrl 


################################################################# 
#    Creating Notification Hub
#################################################################
    Write-Host "Creating Notification Hub Namespace:" -ForegroundColor Green

    az notification-hub namespace create --resource-group $resourceGroupName --location $location `
        --name $notificationHubsNamespaceName `
        --sku Free

    # Delay to avoid messages 'Notification Hub Namespace is not ready or not found"
    Write-Host "Delay 60 seconds" -ForegroundColor Cyan
    Start-Sleep -Seconds 60
    Write-Host "Creating Notification Hub:" -ForegroundColor Green

    az notification-hub create --namespace-name $notificationHubsNamespaceName --resource-group $resourceGroupName --location $location `
        --name $notificationHubName

    $Host.UI.RawUI.ForegroundColor = 'White'

    $notificationHubConnection =  $(az notification-hub authorization-rule list-keys --namespace-name $notificationHubsNamespaceName --resource-group $resourceGroupName `
        --notification-hub-name $notificationHubName --name DefaultFullSharedAccessSignature --query primaryConnectionString)

    $Host.UI.RawUI.ForegroundColor = 'White'

    # Update Google (GCM/FCM) key
    $googleApiKey = Get-Content -Path $($settings.notificationHub.credentials.googleApiKeyFile)
    az notification-hub credential gcm update --namespace-name $notificationHubsNamespaceName --resource-group $resourceGroupName `
        --notification-hub-name $notificationHubName `
        --google-api-key $googleApiKey

    # Update Apple (APNS) certificate
    # https://docs.microsoft.com/en-us/cli/azure/ext/notification-hub/notification-hub/credential/apns?view=azure-cli-latest#ext-notification-hub-az-notification-hub-credential-apns-update
    $certificateKey = Get-Content -Path $($settings.notificationHub.credentials.appleCertificateKeyFile)
    az notification-hub credential apns update --namespace-name $notificationHubsNamespaceName --resource-group $resourceGroupName `
        --notification-hub-name $notificationHubName `
        --apns-certificate $($settings.notificationHub.credentials.appleCertificateFile) `
        --certificate-key $certificateKey

    $Host.UI.RawUI.ForegroundColor = 'White'

################################################################# 
#    Creating Function App - Infection
#################################################################
    Write-Host "Creating Function App - Infection:" -ForegroundColor Green

    # Checks if function exists. Command 'az functionapp create' is not idempotent
    $funcExist = $false
    $funcList = az functionapp list --resource-group $resourceGroupName --query "[].id" | ConvertFrom-JSON
    foreach ($func in $funcList) { if ($func -like "*/sites/$functionAppInfectionName" ) { $funcExist = $true } }
    
    if (!$funcExist) {
        az functionapp create --resource-group $resourceGroupName  --storage-account $storageAccountWebJobsName `
            --name $functionAppInfectionName `
            --os-type Windows `
            --runtime dotnet `
            --functions-version 3 `
            --plan $appServicePlanName `
            --app-insights $appInsightsName
    }

    az functionapp update --name $functionAppInfectionName --resource-group $resourceGroupName `
        --set httpsOnly=true

    az functionapp identity assign --name $functionAppInfectionName --resource-group $resourceGroupName `
        --role "App Configuration Data Reader" `
        --scope /subscriptions/$subscriptionId/resourceGroups/$resourceGroupName

    az keyvault set-policy --name $keyVaultName `
         --object-id $(az functionapp identity show --name $functionAppInfectionName --resource-group $resourceGroupName --query principalId) `
         --secret-permissions get

    # AppConfig setting
    az functionapp config appsettings set --name $functionAppInfectionName --resource-group $resourceGroupName `
        --settings AppConfig:BaseUrl=$appConfigUrl

    az functionapp config appsettings set --name $functionAppInfectionName --resource-group $resourceGroupName `
        --settings AppConfig=$appConfigConnectionString

    az functionapp config appsettings set --name $functionAppInfectionName --resource-group $resourceGroupName `
        --settings AzureWebJobsStorage=$storageAccountWebJobsConnectionString

    az functionapp config appsettings set --name $functionAppInfectionName --resource-group $resourceGroupName `
        --settings BlobConnection=$storageAccountConnectionString

    az functionapp config appsettings set --name $functionAppInfectionName --resource-group $resourceGroupName `
        --settings BlobPath="meetings"

    az functionapp config appsettings set --name $functionAppInfectionName --resource-group $resourceGroupName `
        --settings ContactsTopicListenConnection=$contactsTopicListenConnection 

    az functionapp config appsettings set --name $functionAppInfectionName --resource-group $resourceGroupName `
        --settings ContactsTopicName="contacts"

    az functionapp config appsettings set --name $functionAppInfectionName --resource-group $resourceGroupName `
        --settings ContactsTopicSendConnection=$contactsTopicSendConnection 

    az functionapp config appsettings set --name $functionAppInfectionName --resource-group $resourceGroupName `
        --settings ExposedContactsSubscription="ExposedContacts"

    az functionapp config appsettings set --name $functionAppInfectionName --resource-group $resourceGroupName `
        --settings NotificationsTopicName="notifications"

    az functionapp config appsettings set --name $functionAppInfectionName --resource-group $resourceGroupName `
        --settings NotificationTopicSendConnection=$notificationTopicSendConnection

    az functionapp config appsettings set --name $functionAppInfectionName --resource-group $resourceGroupName `
        --settings RetryPolicy:MaxRetryCount=3


#################################################################
#    Creating Function App - Notification
#################################################################
    Write-Host "Creating Function App - Notification:" -ForegroundColor Green

    # Checks if function exists. Command 'az functionapp create' is not idempotent
    $funcExist = $false
    $funcList = az functionapp list --resource-group $resourceGroupName --query "[].id" | ConvertFrom-JSON
    foreach ($func in $funcList) { if ($func -like "*/sites/$functionAppNotificationName" ) { $funcExist = $true } }
    
    if (!$funcExist) {
        az functionapp create --resource-group $resourceGroupName  --storage-account $storageAccountWebJobsName `
            --name $functionAppNotificationName `
            --os-type Windows `
            --runtime dotnet `
            --functions-version 3 `
            --plan $appServicePlanName `
            --app-insights $appInsightsName
    }

    az functionapp update --name $functionAppInfectionName --resource-group $resourceGroupName `
        --set httpsOnly=true

    az functionapp identity assign --name $functionAppNotificationName --resource-group $resourceGroupName `
        --role "App Configuration Data Reader" `
        --scope /subscriptions/$subscriptionId/resourceGroups/$resourceGroupName

    az keyvault set-policy --name $keyVaultName `
         --object-id $(az functionapp identity show --name $functionAppNotificationName --resource-group $resourceGroupName --query principalId) `
         --secret-permissions get

    # AppConfig setting
    az functionapp config appsettings set --name $functionAppNotificationName --resource-group $resourceGroupName `
        --settings AppConfig:BaseUrl=$appConfigUrl

    az functionapp config appsettings set --name $functionAppNotificationName --resource-group $resourceGroupName `
        --settings NotificationHub:Connection=$notificationHubConnection

    az functionapp config appsettings set --name $functionAppNotificationName --resource-group $resourceGroupName `
        --settings NotificationHub:Name=$notificationHubName

    az functionapp config appsettings set --name $functionAppNotificationName --resource-group $resourceGroupName `
        --settings NotificationsTopicListenConnection=$notificationTopicListenConnection

    az functionapp config appsettings set --name $functionAppNotificationName --resource-group $resourceGroupName `
        --settings NotificationsTopicName="notifications"

    az functionapp config appsettings set --name $functionAppNotificationName --resource-group $resourceGroupName `
        --settings StatusNotificationSubscription="StatusNotification"

    az functionapp config appsettings set --name $functionAppInfectionName --resource-group $resourceGroupName `
        --settings RetryPolicy:MaxRetryCount=3
}

# Get-RandomCharacters function is used to generate password
function Get-RandomCharacters($length, $characters) { 
    $random = 1..$length | ForEach-Object { Get-Random -Maximum $characters.length } 
    $private:ofs="" 
    return [String]$characters[$random]
}

function Get-Settings($settingsKey) {
    $settingsFile = "./settings/$settingsKey.psd1"
    if (Test-Path $settingsFile) {
        return Import-PowerShellDataFile $settingsFile
    } else {
        throw "File '$settingsFile' not found."
    }
}

function Validate-Settings($settings) {
    $fileMap = @{
        appleCertificateFile = $settings.notificationHub.credentials.appleCertificateFile
        appleCertificateKeyFile = $settings.notificationHub.credentials.appleCertificateKeyFile
        googleApiKeyFile = $settings.notificationHub.credentials.googleApiKeyFile
    }
    foreach ($entry in $fileMap.GetEnumerator())
    {
        if (-Not $entry.Value) {
            throw "$($entry.Name) file is not specified" }
        if (-Not (Test-Path $entry.Value)) {
            throw "$($entry.Name) file '$($entry.Value)' does not exists" }
    }
}

DeployEnvironment -nc_Environment $environment -nc_Product $product -nc_SettingsKey $settingsKey
