using GraphQL;
using GraphQL.Transport;
using GraphQL.Types;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace WinDir.Client.GraphQL
{
    public class GraphQLMessenger
    {

        static string serviceBusEndpoint = "sb://MarconiRelayServiceBus.servicebus.windows.net/";

        static string MarconiKeyName = "ClientChitChatAccessKey";
        string MarconiNr { get; set; }
        string MarconiKey { get; set; }

        ServiceBusConnectionStringBuilder ServiceBusConnectionStringBuilder
        {
            get
            {
                return new ServiceBusConnectionStringBuilder(serviceBusEndpoint, MarconiNr, MarconiKeyName, MarconiKey);
            }
        }

        private IQueueClient queueClient;
        private GraphQLRequest Request { get; set; }

        private string messageId { get; set; }

        public JObject JResponse { get; set; }
        public bool JResponseIsReady = false;


        public GraphQLMessenger(GraphQLRequest _request, string _queueName, string _queueKey)
        {
            Request = _request;
            MarconiNr = _queueName;
            MarconiKey = _queueKey;
        }

        public async Task<bool> ProcessQuery()
        {

            JResponseIsReady = false;

            messageId = Guid.NewGuid().ToString();

            queueClient = new QueueClient(ServiceBusConnectionStringBuilder);


            //var message = new Message(Encoding.UTF8.GetBytes(context.Document.OriginalQuery))
            //var message = new Message(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(Request.Query)))
            var message = new Message(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(Request)))
            {
                ContentType = "application/json",
                Label = "Request",
                MessageId = messageId,
                ReplyTo = MarconiNr,
                TimeToLive = TimeSpan.FromMinutes(5)
            };

            // Register QueueClient's MessageHandler and receive messages in a loop
            RegisterOnMessageHandlerAndReceiveMessages();

            // Send the message to the queue
            await queueClient.SendAsync(message);

            return true;
        }

        public async Task Wait4Response()
        {
            // wait and pray for response
            var startWait = DateTime.UtcNow;
            while (!JResponseIsReady && DateTime.UtcNow - startWait < TimeSpan.FromSeconds(40))
            //while (JResponse == null )
            {
                //Console.WriteLine("Excel is busy");
                await Task.Delay(25);
            }

            await queueClient.CloseAsync();
        }

        private void RegisterOnMessageHandlerAndReceiveMessages()
        {
            // Configure the MessageHandler Options in terms of exception handling, number of concurrent messages to deliver etc.
            var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandler)
            {
                // Maximum number of Concurrent calls to the callback `ProcessMessagesAsync`, set to 1 for simplicity.
                // Set it according to how many messages the application wants to process in parallel.
                MaxConcurrentCalls = 1,

                // Indicates whether MessagePump should automatically complete the messages after returning from User Callback.
                // False below indicates the Complete will be handled by the User Callback as in `ProcessMessagesAsync` below.
                AutoComplete = false
            };

            // Register the function that will process messages
            queueClient.RegisterMessageHandler(ProcessMessagesAsync, messageHandlerOptions);
        }

        private async Task ProcessMessagesAsync(Message message, CancellationToken token)
        {
            // Process the message
            //Console.WriteLine($"Received message: SequenceNumber:{message.SystemProperties.SequenceNumber} Body:{Encoding.UTF8.GetString(message.Body)}");
            try
            {
                if (message.Label == "Response" && message.CorrelationId == messageId)
                {

                    // Complete the message so that it is not received again.
                    // This can be done only if the queueClient is created in ReceiveMode.PeekLock mode (which is default).
                    await queueClient.CompleteAsync(message.SystemProperties.LockToken);

                    var responseJsone = Encoding.UTF8.GetString(message.Body);

                    JResponse = JObject.Parse(responseJsone);

                    JResponseIsReady = true;
                }
                else
                {

                    await queueClient.AbandonAsync(message.SystemProperties.LockToken);
                }
            }
            catch (Exception e)
            {
                var m = e.Message;
            }
            // Note: Use the cancellationToken passed as necessary to determine if the queueClient has already been closed.
            // If queueClient has already been Closed, you may chose to not call CompleteAsync() or AbandonAsync() etc. calls 
            // to avoid unnecessary exceptions.
        }

        private Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            Console.WriteLine($"Message handler encountered an exception {exceptionReceivedEventArgs.Exception}.");
            var context = exceptionReceivedEventArgs.ExceptionReceivedContext;
            Console.WriteLine("Exception context for troubleshooting:");
            Console.WriteLine($"- Endpoint: {context.Endpoint}");
            Console.WriteLine($"- Entity Path: {context.EntityPath}");
            Console.WriteLine($"- Executing Action: {context.Action}");
            return Task.CompletedTask;
        }

    }
}

