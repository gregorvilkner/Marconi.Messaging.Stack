# Marconi.Messaging.Stack

The Marconi Messaging Stack allows accessing of local assets by means of using GraphQL for querying and routing of messages securely through an Azure ServiceBus.

## How is Marconi Different?

Guglielmo Marconi is credited for the creation of a practical radio wave-based wireless telgraph system. By taking the wired things away and going wireless he increased the range of telegraph communication from houndreds of miles to thousands of miles. In the case of modern IP-based communication the Marconi Communication Stack can be understood to remove the LAN and IP-Sec barrier, and allow communciation accross the web, making local recourses accessible to remote clients.

### Why GraphQL and Service Bus

The core concept of the Marconi Communication Stack is to use GraphQL as a means of request and response: GraphQLRequest and GraphQLResponse. Both, both the GraphQLRequest and the GraphQLResponse are standardized and strongly typed, both are serializable, and both are very convenient means of querying a server. 

Typically the request and response get serviced by the same entity. What the Marconi Communication Stack does differently, is to seperate the entity that services the request (a web-based GraphQL web api endpoint) from the entity that services the response (a GraphQL resolver with access to local resources). Both, the publicly available GraphQL endpoint and the localized GraphQL resolver communicate through a messaging system, Service Bus (but we could use any other means of exchaning json text). 

A queue on this messaging stack can only be created (opened) from the locale participant, which then shares an ID of the queue, like a secret, a Marconi Number (looks like a telephone number), with the remote endpoint. Now we can access or local resource from afar, one request and response message at a time.

### Message Size Limitations

Messages on Service Bus are limited to 1024kB. We are able to extend this by compressing larger messages. The main use of the Marconi Messaging Stack, however, is focused on smaller payloads. Also, implementations that leverage other messengers, or simple text file exchange would allow for much greater payloads. We have not considered breaking requests or responses into batches.

### Working Implementations

We have shown this type of Communication Stack to work well over the last 5+ years in the following scenarios:

- AEC (architecture engineering and construction) - we interoperated remotely with Autodesk Revit and Autodesk AutoCAD
- Aveva PI/AF - we have interogated PI Asset Framework (AF) remotely using the Marconi Stack, this includes both, the asset model and time series data
- SQL - a brief PoC to show messaging of simple SQL statements and responses
- WinDir - the reference Marconi Stack repository uses accesssing of a local PC's disk system as a show case, i.e. browse my c: drive from afar
- OPC UA - we have used a GraphQL resolver for OPC UA to access a OPC UA server remotely

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
2. chitChatKey - a service bus connection string that only allows to post and receive messages. We create a ChitChatAccessKey for this. This secret is returned when a Marconi Number is successfully validated. Messaging participants include a localized resource (server), where a queue is created and requests are answered, as well as a client, which posts requests. 

After publishing these changes, we need to add the key vault access parameter to the web app config.

## Creating a Marconi.Edge Client

The edge side of a Marconi Session utilizes a GraphQL resolver that receives messages from the Marconi Relay containing a GrphQL request. It then utilizes local resources to create a GraphQL response, which goes back into the relay.

### Creating a GraphQL Schema Library

The schema defines the routes and object types available. In this example we only have a hello endpoint, which returns a simple string: "Guglielmo Marconi(4/25/1874 – 7/201937)".

### creating a Resolver

The resolver contains the actual execution code.

### Creating a Test Console App 

This project acts as a test project to understand logic flow and be able to validate functionality of the GraphQL resolver.

### Creating an Edge Application for the Marconi Relay

This application has 3 main responsibilites:

1. authenticate a user against our B2C directory
1. request new Marconi Nr and close existing session (i.e. start a call and hang up)
1. receive and resolve requests

We use a WPF desktop application to achieve this.

## Creating a Cloud-Based Marconi Client

This application hosts the user-facing GraphQL endpoint. We supply the name of a service bus queue (e.g. a Marconi Number) as a GraphQL variable. When queries are posted to the GraphQL controller, we serialize the request and post it into the service bus as a "request". The edge client will pickup the message from the service bus, deserialize the GraphQL request, resolve it, and post a GraphQL result serialized into the service bus as a "response". The response is received by the controller and thus returned to the user. This roundtrip typically takes less than a second and works really well.

1. create a new server based blazor web app
1. activate controllers
    1. add MapControllers() to startup.cs
    1. add Controlers foler
    1. add ValuesController.cs for testing
1. add GraphQL controller and dependencies
    1. add, clean up and resolve Controllers/GraphqlController.cs
    1. add, clean up and resolve Data/GraphqlService.cs
    1. add, clean up and resolve GraphQL/ GraphQLMessenger.cs, MySchema.cs, and Query.cs (optionally add a Mutation.cs file)
    1. add builder.Services.AddScoped<GraphqlService>();
 in program.cs
1. add GraphiQL middleware to program.cs
    1. add app.UseGraphiQl(); (nuget graphiql)
    1. add builder.Services.AddControllers().AddNewtonsoftJson(); (nuget Microsoft.AspNetCore.Mvc.NewtonsoftJson)
1. add menuitems and OnChange methods on Shared/NavMenu.razor
1. add Shared/MarconiNrReader.razor
1. adjust Parges/Index.razor