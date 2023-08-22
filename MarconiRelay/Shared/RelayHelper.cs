using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using Azure;

namespace MarconiRelay.Shared
{
    public class RelayHelper
    {

        string TenantId { get; set; }
        string ClientId { get; set; }
        string ClientSecret { get; set; }

        static string keyVaultName = "MarconiRelayKeyVault";
        static string keyVaultUri = "https://" + keyVaultName + ".vault.azure.net";
        static string[] keyVaultScopes = new string[] { "https://vault.azure.net/.default" };
        SecretClient _keyVaultClient { get; set; }
        SecretClient KeyVaultClient
        {
            get
            {
                if (_keyVaultClient == null)
                {
                    _keyVaultClient = new SecretClient(new Uri(keyVaultUri), new ClientSecretCredential(TenantId, ClientId, ClientSecret));
                }
                return _keyVaultClient;
            }
        }

        async Task<ServiceBusAdministrationClient> GetManagementClientAsync() 
        {
            var aSecret = await KeyVaultClient.GetSecretAsync("queueManager");
            var connectionString = aSecret.Value.Value;
            var aManagementClient = new ServiceBusAdministrationClient(connectionString);
            return aManagementClient;
        }

        async Task<List<QueueProperties>> GetMarconiQueuesAsync()
        {
            var ManagementClient = await GetManagementClientAsync();
            AsyncPageable<QueueProperties> MarconiQueues = ManagementClient.GetQueuesAsync();
            
            List<QueueProperties> queues = new List<QueueProperties>();
            await foreach(var aQueueProperty in MarconiQueues)
            {
                queues.Add(aQueueProperty);
            }

            return queues;
        }
        async Task<List<QueueProperties>> GetMarconiQueuesAsync(ServiceBusAdministrationClient ManagementClient)
        {
            return await GetMarconiQueuesAsync();
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
            if (MarconiQueues.Where(q => q.Name == MarconiNr.ToLower()).Count() > 0)
            {
                var aSecret = await KeyVaultClient.GetSecretAsync("chitChatKey");
                var chitChatKey = aSecret.Value.Value;
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
            return MarconiQueues.Where(x=>x.UserMetadata==userId).Select(x => x.Name);
        }

        public async Task DeleteMarconiNr(string MarconiNr, string userId)
        {
            var ManagementClient = await GetManagementClientAsync();
            var MarconiQueues = await GetMarconiQueuesAsync(ManagementClient);
            if (MarconiQueues.Where(q => q.Name == MarconiNr.ToLower() && q.UserMetadata == userId).Count() > 0)
            {
                await ManagementClient.DeleteQueueAsync(MarconiNr);
            }
        }

        public async Task<string> CreateNewMarconiNr(string userId)
        {
            var ManagementClient = await GetManagementClientAsync();
            var MarconiQueues = await GetMarconiQueuesAsync(ManagementClient);

            string MarconiNr = MarconiNrMaker.CreateNr();
            while (MarconiQueues.Where(q => q.Name == MarconiNr.ToLower()).Count() > 0)
            {
                MarconiNr = MarconiNrMaker.CreateNr();
            }

            var newQueue = await ManagementClient.CreateQueueAsync(new CreateQueueOptions(MarconiNr)
            {
                DefaultMessageTimeToLive=TimeSpan.FromMinutes(1),
                UserMetadata=userId
            });

            return MarconiNr;
        }
    }
}
