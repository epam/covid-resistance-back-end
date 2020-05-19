@{
    region = @{
        key = "usw"
        name = "westus2" # (Full list: Get-AzLocation | ft)
    }
    postgres = @{
        sku = "B_Gen5_1"
        adminName = "epmcovi"
    }
    notificationHub = @{
        name = "epm-covi"
        # Needed for iOS and Android notifications, make sure to supply a valid certificate and keys
        credentials = @{
            appleCertificateFile = "~/.covi/aps_prod.p12"
            appleCertificateKeyFile = "~/.covi/aps_prod_pass.txt"
            googleApiKeyFile = "~/.covi/google_api_key_file.txt"
        }
    }
}