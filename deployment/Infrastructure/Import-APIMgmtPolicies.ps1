
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

function Get-Settings($settingsKey) {
    $settingsFile = "./settings/$settingsKey.psd1"
    if (Test-Path $settingsFile) {
        return Import-PowerShellDataFile $settingsFile
    } else {
        throw "File '$settingsFile' not found."
    }
}

$settings = Get-Settings($settingsKey)

$resourceGroupName = "rg-$($settings.region.key)-$environment-$product-01"
Write-Host "Resource group name: $resourceGroupName"

function DeployAPIManagement {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        $resourceGroupName,
        [Parameter(Mandatory = $true)]
        [ValidateSet('Public endpoints', 'Maintenance endpoints')]
        $apiVersionSetName
    )

    $parentFolderPath =  (Get-Item (Get-Location).ToString()).Parent.FullName
    switch ($apiVersionSetName)  {
        'Public endpoints' {
            $apiSpecificationPath = $parentFolderPath + "/open-api/public_endpoints.yaml"
            $apiAllOperationsPolicyPath = $parentFolderPath + "/apim-policies/All operations.xml"
            $apiPoliciesPath = $parentFolderPath + "/apim-policies"
            break;
         }
         'Maintenance endpoints' {
            $apiSpecificationPath = $parentFolderPath + "/open-api/maintenance_endpoints.yaml"
            $apiAllOperationsPolicyPath = $parentFolderPath + "/apim-policies/All operations.xml"
            $apiPoliciesPath = $parentFolderPath + "/apim-policies"
            break;
         }
    }

    Write-Host  ""
    Write-Host "Specification path: $apiSpecificationPath"
    Write-Host "All operations policy path: $apiAllOperationsPolicyPath"
    Write-Host "Policies path: $apiPoliciesPath"
    Write-Host  ""

    #will work only if one API Management service in Resource group
    $apiServiceName = (Get-AzApiManagement -ResourceGroupName $resourceGroupName).Name
    $ApiMgmtContext = New-AzApiManagementContext -ResourceGroupName $resourceGroupName -ServiceName $apiServiceName

    ####################################################################
    # STEP 1. Creating an API Version Sets "Public endpoints"
    ####################################################################

    $apiVersionSet = Get-AzApiManagementApiVersionSet -Context $ApiMgmtContext | where { $_.DisplayName -eq $apiVersionSetName }
    if ($null -eq $apiVersionSet) {
        Write-Host "Creating an API Version Sets: $apiVersionSetName"
        $apiVersionSet = New-AzApiManagementApiVersionSet -Context $ApiMgmtContext -Name $apiVersionSetName -Scheme Segment
    }
    else {
        Write-Host "API Version Set '$apiVersionSetName' has already exist."
        Write-Host "If you want to update it, please remove manually and rerun the script."
    }

    ####################################################################
    # STEP 2. Importing an API specification from OpenApi
    ####################################################################

    $apiVersionSetId = $apiVersionSet.ApiVersionSetId
    $apiManagementAPI = Get-AzApiManagementApi -Context $ApiMgmtContext -Name $apiVersionSetName

    if ($null -eq $apiManagementAPI) {
        Write-Host "Importing an API specification from $apiSpecificationPath ..." -Foreground Green

        switch ($apiVersionSetName)  {
          'Public endpoints' {
               Import-AzApiManagementApi -Context $ApiMgmtContext `
               -SpecificationFormat "OpenApi" `
               -SpecificationPath $apiSpecificationPath `
               -Path "" `
               -ApiVersion "v1" `
               -ApiVersionSetId $apiVersionSetId
            break;
           }
           'Maintenance endpoints' {
              Import-AzApiManagementApi -Context $ApiMgmtContext `
                -SpecificationFormat "OpenApi" `
                -SpecificationPath $apiSpecificationPath `
                -Path "utils" `
                -ApiVersion "v1" `
                -ApiVersionSetId $apiVersionSetId
             break;
           }
        }
    }
    else {
        Write-Host "API specification has alredy exist."
        Write-Host "If you want to update it, please remove manually and rerun the script."
    }


    ####################################################################
    # STEP 3. Importing an API policy for 'All operations'
    ####################################################################

    $apiManagementAPI = Get-AzApiManagementApi -Context $ApiMgmtContext -Name $apiVersionSetName
    Write-Host "Importing an API policy for 'All operations' from $apiAllOperationsPolicyPath"
    Set-AzApiManagementPolicy -Context $ApiMgmtContext -ApiId $apiManagementAPI.ApiId -PolicyFilePath $apiAllOperationsPolicyPath `
                -Format "application/vnd.ms-azure-apim.policy.raw+xml"


    ####################################################################
    # STEP 4. Importing policies for API Management
    ####################################################################

    $operations = Get-AzApiManagementOperation -Context $ApiMgmtContext -ApiId $apiManagementAPI.ApiId

    foreach($op in $operations) {
        $fileName = "$apiPoliciesPath/" + $op.Name + "xml"
        if (Test-Path $fileName -PathType leaf)  {
            Write-Host "Importing policy $fileName"
            $oId = $op.OperationId
            Set-AzApiManagementPolicy -Context $ApiMgmtContext -ApiId $apiManagementAPI.ApiId -OperationId $oId -PolicyFilePath $fileName `
                -Format "application/vnd.ms-azure-apim.policy.raw+xml"
        }
        else {
            Write-Host "Policy '$fileName' does not exist"  -Foreground Yellow
        }
    }


    ####################################################################
    # STEP 5. Remove 'Subscription required' parameter
    ####################################################################

    if ($apiManagementAPI.SubscriptionRequired -eq $true) {
        Write-Host "Removing 'Subscription required' parameter ..."
        $apiManagementAPI.SubscriptionRequired = $false
        Set-AzApiManagementApi -InputObject $apiManagementAPI -Name $apiManagementAPI.Name -ServiceUrl $apiManagementAPI.ServiceUrl -Protocols $apiManagementAPI.Protocols
    }

}

DeployAPIManagement -resourceGroupName $resourceGroupName -apiVersionSetName "Public endpoints"
DeployAPIManagement -resourceGroupName $resourceGroupName -apiVersionSetName "Maintenance endpoints"
