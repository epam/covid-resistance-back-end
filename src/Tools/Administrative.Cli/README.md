# Administrative Tool

This is the admin tool `resist-cli` that can perform the following administrative tasks:
- Create admin user

## Installation

In the project folder `Administrative.CLI` execute the following commands:

```bash
# Build and package the application
dotnet pack

# Install the tool
dotnet tool install -g --add-source ./bin/Debug/ Administrative.Cli
```

## Using the application

```bash
# Get the connection string to the identity database
az keyvault secret show --vault-name ${keyvault_name} --name Identity-Connection-String --query value -otsv

# Create a new admin user with specified username and password
resist-cli admin create ${admin_user} ${admin_pwd}
### Paste in the connection string when asked
```

# Generating Health Security Ids

Once we have an admin user we need to make an authenticated call to the `generateHealthSecurityIds` endpoint.
The steps are:
* Log in and retrieve the authentication token
* Call the endpoint by passing the token

A python script is available in the [HealthSecurityIds](../HealthSecurityIds) folder.

```bash
cd ../HealthSecurityIds

# Install requirements before the first usage
pip install -r requirements.txt

# Generate 10 codes of 12 characters (the script will ask for username and password)
SERVICE_URL="https://your-api-management-service-gateway-url/utils/v1"
python generate-hsi.py --service-url ${SERVICE_URL} --number-of-codes 10 --code-length 12 --comment "Test codes"

# See help
python generate-hsi.py -h
```
