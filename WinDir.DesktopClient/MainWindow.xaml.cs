using GraphQL;
using GraphQL.SystemTextJson;
using Microsoft.Azure.ServiceBus;
using WinDir.DesktopClient.Helpers;
using WinDir.GraphQLResolver;
using WinDir.GraphQLResolver.GraphQL;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WinDir.DesktopClient.Helpers;
using Message = Microsoft.Azure.ServiceBus.Message;
using GraphQL.Transport;
using System.IO;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace WinDir.DesktopClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MsalAuthHelper msalAuthHelper;

        public MainWindow()
        {

            InitializeComponent();

            Closed += MainWindow_Closed;

            InitializeComponent();
            msalAuthHelper = new MsalAuthHelper();
            RefreshSignInStatus();

        }


        private string MarconiNr = "";

        private bool _marconiIsOpen = false;

        private bool _marconiIsBusy = false;

        private bool MarconiIsOpen
        {
            get
            {
                return _marconiIsOpen;
            }
            set
            {
                _marconiIsOpen = value;
                if (value)
                {
                    Marconi_Open.IsEnabled = false;
                    Marconi_Close.IsEnabled = true;
                }
                else
                {
                    Marconi_Open.IsEnabled = true;
                    Marconi_Close.IsEnabled = false;
                }
            }
        }


        const string SignInString = "Sign In";
        const string ClearCacheString = "Sign Out";
        const string UserNotSignedIn = "Signed Out";

        private IQueueClient queueClient;
        //static string serviceBusEndpoint = "sb://pisystemrelay.servicebus.windows.net/";
        static string serviceBusEndpoint = "sb://MarconiRelayServiceBus.servicebus.windows.net/";

        static string MarconiKeyName = "ClientChitChatAccessKey";
        string MarconiKey { get; set; }

        ServiceBusConnectionStringBuilder ServiceBusConnectionStringBuilder
        {
            get
            {
                return new ServiceBusConnectionStringBuilder(serviceBusEndpoint, MarconiNr, MarconiKeyName, MarconiKey);
            }
        }


        private void MainWindow_Closed(object sender, EventArgs e)
        {

            if (MarconiIsOpen) Marconi_Close_Click(null, null);

            Close();
        }

        private async void SignInButton_Click(object sender, RoutedEventArgs e)
        {
            if (msalAuthHelper.IsSignedIn())
            {
                await msalAuthHelper.SignOutAsync();
            }
            else
            {
                await msalAuthHelper.SignInAsync();
            }
            RefreshSignInStatus();
        }

        public async void RefreshSignInStatus()
        {
            if (msalAuthHelper.IsSignedIn())
            {
                SignInButton.Content = ClearCacheString;
                SignInLabel.Content = await msalAuthHelper.GetNameOfActiveAccountAsync();
                if (!MarconiIsOpen)
                {
                    Marconi_Open.IsEnabled = true;
                    Marconi_Close.IsEnabled = false;
                }
                else
                {
                    Marconi_Open.IsEnabled = false;
                    Marconi_Close.IsEnabled = true;
                }
            }
            else
            {
                if (MarconiIsOpen) Marconi_Close_Click(null, null);

                SignInButton.Content = SignInString;
                SignInLabel.Content = UserNotSignedIn;

                Marconi_Open.IsEnabled = false;
                Marconi_Close.IsEnabled = false;
            }

        }

        private async void Marconi_Open_Click(object sender, RoutedEventArgs e)
        {

            if (!MarconiIsOpen && (await EnforceSignInAsync()))
            {

                HttpClient aClient = new HttpClient();
                //aClient.BaseAddress = new Uri("https://localhost:7167");
                aClient.BaseAddress = new Uri("https://marconirelay.azurewebsites.net");
                aClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", await msalAuthHelper.GetTokenAsync());
                MarconiNr = await (await aClient.PutAsync("MarconiNr", null)).Content.ReadAsStringAsync();
                MarconiKey = await (await aClient.GetAsync($"MarconiNr/{MarconiNr}")).Content.ReadAsStringAsync();

                queueClient = new QueueClient(ServiceBusConnectionStringBuilder);

                MarconiIdText.Text = $"{MarconiNr}";

                // Register QueueClient's MessageHandler and receive messages in a loop
                RegisterOnMessageHandlerAndReceiveMessages();

                MarconiIsOpen = true;
            }
        }

        public async Task<bool> EnforceSignInAsync()
        {
            if (!msalAuthHelper.IsSignedIn())
            {
                await msalAuthHelper.SignInAsync();
                RefreshSignInStatus();
            }

            if (msalAuthHelper.IsSignedIn())
            {
                return true;
            }
            else
            {
                return false;
            }
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
            try
            {
                queueClient.RegisterMessageHandler(ProcessMessagesAsync, messageHandlerOptions);
            }
            catch (Exception e)
            {
                var m = e.Message;
            }
        }

        private async Task ProcessMessagesAsync(Message message, CancellationToken token)
        {
            // Process the message
            Console.WriteLine($"Received message: SequenceNumber:{message.SystemProperties.SequenceNumber} Body:{Encoding.UTF8.GetString(message.Body)}");

            if (message.Label == "Request")
            {

                // Complete the message so that it is not received again.
                // This can be done only if the queueClient is created in ReceiveMode.PeekLock mode (which is default).
                await queueClient.CompleteAsync(message.SystemProperties.LockToken);

                ExecutionResult result = null;

                if (_marconiIsBusy)
                {

                }
                else
                {
                    _marconiIsBusy = true;

                    try
                    {

                        var start = DateTime.UtcNow;

                        string queryJsonString = Encoding.UTF8.GetString(message.Body);

                        ResolverEntry aEntry = new ResolverEntry();
                        //var request = new GraphQLSerializer().Deserialize<GraphQLRequest>(queryJsonString);

                        var aJObject = JsonConvert.DeserializeObject<JObject>(queryJsonString);
                        aJObject["operationName"] = aJObject.Properties().First(x => x.Name == "OperationName").Value;
                        aJObject["query"] = aJObject.Properties().First(x => x.Name == "Query").Value;
                        aJObject["variables"] = aJObject.Properties().First(x => x.Name == "Variables").Value;
                        aJObject["extensions"] = aJObject.Properties().First(x => x.Name == "Extensions").Value;
                        var request = new GraphQLSerializer().Deserialize<GraphQLRequest>(aJObject.ToString());

                        result = await aEntry.GetResultAsync(request);

                    }
                    catch (Exception e)
                    {
                        var m = e.Message;
                    }

                    _marconiIsBusy = false;
                }

                try
                {
                    // https://www.lucanatali.it/c-from-string-to-stream-and-from-stream-to-string/
                    var ms = new MemoryStream();
                    var sw = new StreamWriter(ms);

                    await new GraphQLSerializer().WriteAsync(ms, result);

                    string resultJson = Encoding.ASCII.GetString(ms.ToArray());

                    //var responseJson = await new GraphQL.SystemTextJson.DocumentWriter(true).WriteToStringAsync(result);
                    var responseMessage = new Message(Encoding.UTF8.GetBytes(resultJson))
                    {
                        ContentType = "application/json",
                        Label = "Response",
                        CorrelationId = message.MessageId,
                        MessageId = Guid.NewGuid().ToString(),
                        TimeToLive = TimeSpan.FromMinutes(5)
                    };

                    // Send the message to the queue
                    IQueueClient queueResponseClient = new QueueClient(ServiceBusConnectionStringBuilder);
                    await queueClient.SendAsync(responseMessage);
                }
                catch (Exception e)
                {
                    var m = e.Message;
                }


            }
            else
            {

                await queueClient.AbandonAsync(message.SystemProperties.LockToken);
            }
            // Note: Use the cancellationToken passed as necessary to determine if the queueClient has already been closed.
            // If queueClient has already been Closed, you may chose to not call CompleteAsync() or AbandonAsync() etc. calls 
            // to avoid unnecessary exceptions.
        }


        // Use this Handler to look at the exceptions received on the MessagePump
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

        private async void Marconi_Close_Click(object sender, RoutedEventArgs e)
        {
            if (MarconiIsOpen)
            {

                await queueClient.CloseAsync();

                HttpClient aClient = new HttpClient();
                //aClient.BaseAddress = new Uri("https://localhost:7167");
                aClient.BaseAddress = new Uri("https://marconirelay.azurewebsites.net");
                aClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", await msalAuthHelper.GetTokenAsync());
                await aClient.DeleteAsync($"MarconiNr/{MarconiNr}");

                MarconiIdText.Text = $"---";

                MarconiIsOpen = false;
            }
        }

    }
}
