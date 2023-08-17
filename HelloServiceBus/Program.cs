using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.ServiceBus.Fluent;
using Microsoft.Azure.ServiceBus.Management;
using System;
using System.Linq;

namespace HelloServiceBus
{
    class Program
    {
        static async System.Threading.Tasks.Task Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            string serviceBusConnectionString = "Endpoint=sb://marconirelay.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=0Sj+R0v7xdUiqVPavaVymH0igrrsq9gd7+ASbE7pTn4=";

            var managementClient = new ManagementClient(serviceBusConnectionString);

            var allQueues = await managementClient.GetQueuesAsync();

            if(allQueues.Where(x=>x.Path=="testqueue").Count() > 0)
            {
                // delete the existing test queue
                await managementClient.DeleteQueueAsync("testqueue");
            }

            var newQueue = await managementClient.CreateQueueAsync("testqueue");

            allQueues = await managementClient.GetQueuesAsync();



        }
    }
}
