using Microsoft.Azure.KeyVault;
using Microsoft.Azure.ServiceBus.Management;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarconiRelay.Shared
{
    public class RelayHelper
    {

        string TenantId { get; set; }
        string ClientId { get; set; }
        string ClientSecret { get; set; }
        string authority
        {
            get
            {
                var aAuthority = $"https://login.microsoftonline.com/{TenantId}/oauth2v2.0/token";
                return aAuthority;
            }
        }
        IConfidentialClientApplication _app { get; set; }
        IConfidentialClientApplication app 
        {
            get
            {
                if (_app == null)
                {
                    _app = ConfidentialClientApplicationBuilder.Create(ClientId)
                    .WithAuthority(authority)
                    .WithClientSecret(ClientSecret)
                    .Build();
                }
                return _app;
            }
        }

        static string keyVaultName = "MarconiRelayKeyVault";
        static string keyVaultUri = "https://" + keyVaultName + ".vault.azure.net";
        static string[] keyVaultScopes = new string[] { "https://vault.azure.net/.default" };
        KeyVaultClient _keyVaultClient { get; set; }
        KeyVaultClient KeyVaultClient
        {
            get
            {
                if (_keyVaultClient == null)
                {
                    _keyVaultClient = new KeyVaultClient(async (authority, resource, scope) =>
                    {
                        AuthenticationResult authenticationResult = await app.AcquireTokenForClient(keyVaultScopes).ExecuteAsync();
                        return authenticationResult.AccessToken;
                    });
                }
                return _keyVaultClient;
            }
        }


        async Task<ManagementClient> GetManagementClientAsync() 
        {
            var aSecret = await KeyVaultClient.GetSecretAsync(keyVaultUri, "queueManager");
            var connectionString = aSecret.Value;
            var aManagementClient = new ManagementClient(connectionString);
            return aManagementClient;
        }

        async Task<IList<QueueDescription>> GetMarconiQueuesAsync()
        {
            var ManagementClient = await GetManagementClientAsync();
            var MarconiQueues = await ManagementClient.GetQueuesAsync();
            return MarconiQueues;
        }
        async Task<IList<QueueDescription>> GetMarconiQueuesAsync(ManagementClient ManagementClient)
        {
            var MarconiQueues = await ManagementClient.GetQueuesAsync();
            return MarconiQueues;
        }

        public RelayHelper(string _clientId, string _clientSecret, string _tenantId)
        {
            ClientId = _clientId;
            ClientSecret = _clientSecret;
            TenantId = _tenantId;
        }

        public async Task<string> ValidateMarconiNr(string MarconiNr)
        {
            var MarconiQueues = await GetMarconiQueuesAsync();
            if (MarconiQueues.Where(q => q.Path == MarconiNr.ToLower()).Count() > 0)
            {
                var aSecret = await KeyVaultClient.GetSecretAsync(keyVaultUri, "chitChatKey");
                var chitChatKey = aSecret.Value;
                return chitChatKey;
            }
            return null;
        }

        //public async Task<IEnumerable<string>> GetAllMarconiNrsAsync()
        //{
        //    var MarconiQueues = await GetMarconiQueuesAsync();
        //    return MarconiQueues.Select(x => x.Path);
        //}
        public async Task<IEnumerable<string>> GetAllMarconiNrsAsync(string userId)
        {
            var MarconiQueues = await GetMarconiQueuesAsync();
            return MarconiQueues.Where(x=>x.UserMetadata==userId).Select(x => x.Path);
        }

        public async Task DeleteMarconiNr(string MarconiNr, string userId)
        {
            var ManagementClient = await GetManagementClientAsync();
            var MarconiQueues = await GetMarconiQueuesAsync(ManagementClient);
            if (MarconiQueues.Where(q => q.Path == MarconiNr.ToLower() && q.UserMetadata == userId).Count() > 0)
            {
                await ManagementClient.DeleteQueueAsync(MarconiNr);
            }
        }

        public async Task<string> CreateNewMarconiNr(string userId)
        {
            var ManagementClient = await GetManagementClientAsync();
            var MarconiQueues = await GetMarconiQueuesAsync(ManagementClient);

            string MarconiNr = MarconiNrMaker.CreateNr();
            while (MarconiQueues.Where(q => q.Path == MarconiNr.ToLower()).Count() > 0)
            {
                MarconiNr = MarconiNrMaker.CreateNr();
            }

            QueueDescription aQueueDescription = new QueueDescription(MarconiNr)
            {
                //AutoDeleteOnIdle = TimeSpan.FromMinutes(30),
                DefaultMessageTimeToLive = TimeSpan.FromMinutes(1),
                UserMetadata=userId
            };

            var newQueue = await ManagementClient.CreateQueueAsync(aQueueDescription);

            return MarconiNr;
        }
    }
}
