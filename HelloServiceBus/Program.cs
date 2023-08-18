using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using System;
using System.Linq;

namespace HelloServiceBus
{
    class Program
    {
        static async System.Threading.Tasks.Task Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            var queueName = "testqueue";

            var serviceBusConnectionString_ = ConfigurationRoot.Get().GetSection("ServiceBus_").GetSection("ConnectionString").Value;
            var serviceBusConnectionString = ConfigurationRoot.Get().GetSection("ServiceBus").GetSection("ConnectionString").Value;

            //https://github.com/Azure/azure-sdk-for-net/blob/main/sdk/servicebus/Azure.Messaging.ServiceBus/samples/Sample00_AuthenticateClient.md
            await using var MessagingClient = new ServiceBusClient(serviceBusConnectionString);


            //https://github.com/Azure/azure-sdk-for-net/blob/main/sdk/servicebus/Azure.Messaging.ServiceBus/samples/Sample07_CrudOperations.md
            var AdminClient = new ServiceBusAdministrationClient(serviceBusConnectionString);

            var allQueues = AdminClient.GetQueuesAsync();
            await foreach (var item in allQueues)
            {
                Console.WriteLine(item.Name);
                if (item.Name == queueName)
                {
                    await AdminClient.DeleteQueueAsync(item.Name);
                }
            }

            var newQueue = await AdminClient.CreateQueueAsync(queueName);


        }
    }
}
