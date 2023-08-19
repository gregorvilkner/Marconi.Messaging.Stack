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

### First to get a B2C Secured Blazor WASM App Going

We are using this walkthrough to create a Blazor B2C secured wasm web application: https://learn.microsoft.com/en-us/aspnet/core/blazor/security/webassembly/hosted-with-azure-active-directory-b2c?view=aspnetcore-7.0

The reason we chose WASM is because it combines a web api and a front end in the same project. The purpose of the Marconi Relay is basically to be an API to control queues in our Service Bus, but having a front end will allow us to build reports and facilitate simple testing.

1. register api (Marconi.Relay.WebAPI)
1. register client (Marconi.Relay.WebClient)
1. expose api by adding scope on the server
1. select that scope in api permissions on the client
1. create massive cli command to create application

```
dotnet new blazorwasm -au IndividualB2C --aad-b2c-instance "https://MarconiStack.b2clogin.com/" --api-client-id "75a3bd6b-14f0-4fe5-970a-9d3fb15991a7" --app-id-uri "75a3bd6b-14f0-4fe5-970a-9d3fb15991a7" --client-id "a26d2087-7db1-4c17-b821-f007f9f015a1" --default-scope "queue.manage" --domain "MarconiStack.onmicrosoft.com" -ho -o MarconiRelay -ssp "B2C_1_SignUpAndSignIn"
```
### Then to Add the Queue Mangement

We add a MarconiNrController to the web api. The methods available on the controller include:

- \[Authorize, Get, "MarconiNr"\] GetActiveMarconiNumbers() - gets a listing of the available currently active Marconi Numbers for the authenticated user.

- \[Get, "MarconiNr/{MarconiNr}"\] ValidateMarconiNumber(string MarconiNr) - validates a Marconi Number. If the number is good the method returns a "chitChatKey", which is a connection string that can be used to post messages to the ServiceBus Queue. The "chitChatKey" can only post and listen.

- \[Authorize, Put, "MarconiNr"\] Put() - Creates and returns a new Marconi Number.

- \[Authorize, Delete, "MarconiNr/{MarconiNr}"] Delete(string MarconiNr) - Deletes a Marconi Number, i.e. deletes the corresponding queue from our Service Bus.

```
We should probably add Swagger...
```

The MarconiNrController requires access to the Key Vault, which is where we store connection strings for our service bus. We configure a service to obtain the required parameters from our configuration in the Program.cs file.

The following helper classes are added to the shared project:

- MarconiNrMaker.cs - can make random phone numbers
- MarconiKeyVaultClient.cs - the contract to serialize a ClientId, ClientSecret, and TenantId
- RelayHelper.cs - a number of methods to create, delete and vaidate Marconi Numbers, i.e. manage queues in our service bus.

We add 2 secrets to our key vault:

1. queueManager - a service bus connection string that can manage queues. We can simply utilize the RootManageSharedAccessKey we used earlier to test the service bus. This secret is only available to the web api.
2. chitChatKey - a service bus connection string that only allows to post and receive messages. We create a SendAndListenSharedAccessKey for this. This secret is returned when a Marconi Number is successfully validated. Messaging participants include a localized resource (server), where a queue is created and requests are answered, as well as a client, which posts requests. 

