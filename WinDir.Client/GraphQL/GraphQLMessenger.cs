using Azure.Messaging.ServiceBus;
using GraphQL.Transport;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WinDir.Client.GraphQL
{
    public class GraphQLMessenger
    {

        static string serviceBusEndpoint = "sb://MarconiRelayServiceBus.servicebus.windows.net/";

        static string MarconiKeyName = "ClientChitChatAccessKey";
        string MarconiNr { get; set; }
        string MarconiKey { get; set; }

        private ServiceBusClient serviceBusClient;
        private ServiceBusSender serviceBusSender;
        private ServiceBusProcessor serviceBusProcessor;

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

            serviceBusClient = new ServiceBusClient(MarconiKey);
            serviceBusSender = serviceBusClient.CreateSender(MarconiNr);

            var message = new ServiceBusMessage(JsonConvert.SerializeObject(Request))
            {
                ContentType = "application/json",
                Subject = "Request",
                MessageId = messageId,
                ReplyTo = MarconiNr,
                TimeToLive = TimeSpan.FromMinutes(5)
            };

            // Register the function that will process messages
            try
            {
                serviceBusProcessor = serviceBusClient.CreateProcessor(MarconiNr);

                // add handler to process messages
                serviceBusProcessor.ProcessMessageAsync += MessageHandler;

                // add handler to process any errors
                serviceBusProcessor.ProcessErrorAsync += ErrorHandler;

                // start processing 
                await serviceBusProcessor.StartProcessingAsync();
            }
            catch (Exception ex)
            {
                var m = ex.Message;
            }

            // Send the message to the queue
            await serviceBusSender.SendMessageAsync(message);

            return true;
        }

        // handle any errors when receiving messages
        Task ErrorHandler(ProcessErrorEventArgs args)
        {
            Console.WriteLine(args.Exception.ToString());
            return Task.CompletedTask;
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

            await serviceBusProcessor.CloseAsync();
        }

        async Task MessageHandler(ProcessMessageEventArgs args)

        {
            var message = args.Message;
            // Process the message
            //Console.WriteLine($"Received message: SequenceNumber:{message.SystemProperties.SequenceNumber} Body:{Encoding.UTF8.GetString(message.Body)}");
            try
            {
                if (message.Subject == "Response" && message.CorrelationId == messageId)
                {

                    await args.CompleteMessageAsync(args.Message);

                    var responseJson = message.Body.ToString();

                    JResponse = JObject.Parse(responseJson);

                    JResponseIsReady = true;
                }
                else
                {

                    await args.AbandonMessageAsync(message);
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


    }
}

