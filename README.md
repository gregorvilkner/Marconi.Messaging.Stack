# Marconi.Messaging.Stack
The Marconi Messaging Stack allows accessing of local assets by means of using GraphQL for querying and routing of messages securely through an Azure ServiceBus.

# Creating a Marconi Stack from Scratch

## Azure B2C Active Directory

We use Azure B2C to allow users to securely authenticate. After creation of a new B2C tenant we also organize all resource within this tenant.

## Azure Key Vault

We use Azure Key Vault to store access keys for our messaging service bus. In order to test access to a newly created key vault using a managed principal, we need to register an app in our directory and create a secret. This allows us to use a ClientSecretCredential to authenticate and create a SecretClient.

1. create key vault (MarconiRelayKeyVault)
1. register an app (MarconiKeyVaultClient) and create a secret
1. copy TenantId, ClientId, and ClientSecret to appsettings.json
1. grant access (Key Vault Secrets Officer) to the app using the key vault's Access Control (IAM)
1. run the HelloKeyvault test program
1. to be able to manually verify the created test secret we need to grant the user (ourselves) access (Key Vault Administrator) to the key vault. 

## Azure Service Bus

We use Azure Service Bus to relay messages between local resources and public GraphQL clients.

1. create service bus (MarconiRelayServiceBus)
1. obtain a primary connection string from the service bus' Shared access policies (RootMangeSharedAccessKey) and copy it to the service bus
1. run the HelloServiceBus test program

## Marconi Relay Blazor Web Application

- we are using this walkthrough to create a blazor b2c secured wasm web application: https://learn.microsoft.com/en-us/aspnet/core/blazor/security/webassembly/hosted-with-azure-active-directory-b2c?view=aspnetcore-7.0

1. register api (Marconi.Relay.WebAPI)
1. register client (Marconi.Relay.WebClient)
1. expose api by adding scope on the server
1. select that scope in api permissions on the client
1. create massive cli command to create application