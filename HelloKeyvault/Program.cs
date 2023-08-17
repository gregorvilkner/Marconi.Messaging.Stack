using Azure.Core;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
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

            string TenantId = ConfigurationRoot.Get().GetSection("KeyVault").GetSection("TenantId").Value;
            string ClientSecret = ConfigurationRoot.Get().GetSection("KeyVault").GetSection("ClientSecret").Value;
            string ClientId = ConfigurationRoot.Get().GetSection("KeyVault").GetSection("ClientId").Value;

            string keyVaultName = "MarconiKeyVaultClient";
            var kvUri = "https://" + keyVaultName + ".vault.azure.net";

            var client = new SecretClient(new Uri(kvUri), new ClientSecretCredential(TenantId, ClientId, ClientSecret));

            var setNewSecret = await client.SetSecretAsync("newSecret", Guid.NewGuid().ToString());
            var getNewSecret = await client.GetSecretAsync("newSecret");

            var getNewSecretVersionProperties = client.GetPropertiesOfSecretVersions("newSecret");
            foreach (var aSecretVersionProperty in getNewSecretVersionProperties)
            {
                var aSecretVersion = await client.GetSecretAsync("newSecret", aSecretVersionProperty.Version);
                Console.WriteLine($"{aSecretVersionProperty.Version} - {aSecretVersionProperty.CreatedOn} - {aSecretVersionProperty.UpdatedOn} - {aSecretVersion.Value.Value}");
            }
        }


    }
}
