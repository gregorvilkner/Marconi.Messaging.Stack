using Microsoft.Azure.KeyVault;
using Microsoft.Identity.Client;
using System;
using System.Linq;

namespace HelloKeyvault
{
    class Program
    {
        static IConfidentialClientApplication app;
        static async System.Threading.Tasks.Task Main(string[] args)
        {
            // mdesk
            string tenantId = "14c2fac0-aa1f-4bd3-beb8-9e58dfb9b18a";
            string ClientSecret = "~938Q~maRFLoarelt83lumyEnSL4CV4TaHPf~bhZ";
            string ClientId = "fbc3619f-d3b9-4fef-85a2-a8599029a232";
            string authority = $"https://login.microsoftonline.com/{tenantId}/oauth2v2.0/token";

            app = ConfidentialClientApplicationBuilder.Create(ClientId)
                .WithAuthority(authority)
                .WithClientSecret(ClientSecret)
                .Build();

            KeyVaultClient client = new KeyVaultClient(async (authority, resource, scope) =>
            {
                string[] scopes = new string[] { "https://vault.azure.net/.default" };
                AuthenticationResult authenticationResult = await app.AcquireTokenForClient(scopes)
                    .ExecuteAsync();

                return authenticationResult.AccessToken;

            });


            string keyVaultName = "MarconiKeyVaultClient";
            var kvUri = "https://" + keyVaultName + ".vault.azure.net";
            var newse = await client.SetSecretAsync(kvUri, "newSecret", Guid.NewGuid().ToString());
            var secret = (await client.GetSecretsAsync(kvUri)).First();
            var v = await client.GetSecretAsync(secret.Identifier.Identifier);
            var v2 = await client.GetSecretVersionsAsync(kvUri, secret.Identifier.Name);
            foreach(var aSecret in v2)
            {
                Console.WriteLine($"{aSecret.Identifier.Version} - {aSecret.Attributes.Created} - {aSecret.Attributes.Enabled} - {aSecret.Attributes.Updated} ");
            }
        }


    }
}
