using Azure.Identity;
using Microsoft.Identity.Client;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Nodes;

var client = new HttpClient();
client.BaseAddress = new Uri("https://marconirelayserver.azurewebsites.net/");

// get local nodesets
string TenantId = ConfigurationRoot.Get().GetSection("MarconiRelayDesktopClient").GetSection("TenantId").Value;
string ClientSecret = ConfigurationRoot.Get().GetSection("MarconiRelayDesktopClient").GetSection("ClientSecret").Value;
string ClientId = ConfigurationRoot.Get().GetSection("MarconiRelayDesktopClient").GetSection("ClientId").Value;

var cred = new ClientSecretCredential(TenantId, ClientId, ClientSecret);
string SpListScope1 = $"https://MarconiStack.onmicrosoft.com/75a3bd6b-14f0-4fe5-970a-9d3fb15991a7/.default";
string SpListScope2 = $"https://MarconiStack.onmicrosoft.com/75a3bd6b-14f0-4fe5-970a-9d3fb15991a7/queue.manage";
string[] Scopes = { SpListScope1 };

var token = await cred.GetTokenAsync(new Azure.Core.TokenRequestContext(Scopes));

IConfidentialClientApplication confidentialClientApplication = ConfidentialClientApplicationBuilder
    .Create(ClientId)
    .WithTenantId(TenantId)
    .WithClientSecret(ClientSecret)
    .Build();
ClientCredentialProvider authProvider = new ClientCredentialProvider(confidentialClientApplication);


client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);
var weather = await client.GetAsync($"WeatherForecast");

Console.WriteLine( weather );
