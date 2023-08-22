using Azure.Messaging.ServiceBus;
using GraphQL;
using GraphQL.SystemTextJson;
using GraphQL.Transport;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WinDir.DesktopClient.Helpers;
using WinDir.GraphQLResolver;

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

        private ServiceBusClient serviceBusClient;
        private ServiceBusSender serviceBusSender;
        private ServiceBusProcessor serviceBusProcessor;
        //static string serviceBusEndpoint = "sb://pisystemrelay.servicebus.windows.net/";
        static string serviceBusEndpoint = "sb://MarconiRelayServiceBus.servicebus.windows.net/";

        static string MarconiKeyName = "ClientChitChatAccessKey";
        string MarconiKey { get; set; }

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

        private async void Marconi_Open_Click(object sender, RoutedEventArgs e)
        {

            if (!MarconiIsOpen && (await EnforceSignInAsync()))
            {

                HttpClient aClient = new HttpClient();
                //aClient.BaseAddress = new Uri("https://localhost:7167");
                aClient.BaseAddress = new Uri("https://marconirelay.azurewebsites.net");
                aClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", await msalAuthHelper.GetTokenAsync());
                MarconiNr = await (await aClient.PutAsync("MarconiNr", null)).Content.ReadAsStringAsync();
                MarconiIdText.Text = $"{MarconiNr}";

                MarconiKey = await (await aClient.GetAsync($"MarconiNr/{MarconiNr}")).Content.ReadAsStringAsync();

                serviceBusClient = new ServiceBusClient(MarconiKey);
                serviceBusSender = serviceBusClient.CreateSender(MarconiNr);

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


                MarconiIsOpen = true;
            }
        }


        // handle any errors when receiving messages
        Task ErrorHandler(ProcessErrorEventArgs args)
        {
            Console.WriteLine(args.Exception.ToString());
            return Task.CompletedTask;
        }
        async Task MessageHandler(ProcessMessageEventArgs args)

        {
            var message = args.Message;
            // Process the message
            Console.WriteLine($"Received message: SequenceNumber:{message.SessionId} Body:{Encoding.UTF8.GetString(message.Body)}");

            if (message.Subject == "Request")
            {

                // complete the message. messages is deleted from the subscription. 
                await args.CompleteMessageAsync(args.Message);

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

                        string queryJsonString = message.Body.ToString();

                        ResolverEntry aEntry = new ResolverEntry();
                        //var request = new GraphQLSerializer().Deserialize<GraphQLRequest>(queryJsonString);

                        // very strange - would like to get this fixed
                        // GraphQLSerializer().Deserialize<GraphQLRequest> wants the json keys in lower case
                        // but the GraphQLRequest class has them in upper case
                        // probably something with which serializer we use and what options
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
                    var responseMessage = new ServiceBusMessage(resultJson)
                    {
                        ContentType = "application/json",
                        Subject = "Response",
                        CorrelationId = message.MessageId,
                        MessageId = Guid.NewGuid().ToString(),
                        TimeToLive = TimeSpan.FromMinutes(5)
                    };

                    // Send the message to the queue
                    await serviceBusSender.SendMessageAsync(responseMessage);
                }
                catch (Exception e)
                {
                    var m = e.Message;
                }


            }
            else
            {
                await args.AbandonMessageAsync(args.Message);

            }

        }



        private async void Marconi_Close_Click(object sender, RoutedEventArgs e)
        {
            if (MarconiIsOpen)
            {

                await serviceBusProcessor.CloseAsync();

                HttpClient aClient = new HttpClient();
                //aClient.BaseAddress = new Uri("https://localhost:7167");
                aClient.BaseAddress = new Uri("https://marconirelay.azurewebsites.net");
                aClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", await msalAuthHelper.GetTokenAsync());
                var aResponse = await aClient.DeleteAsync($"MarconiNr/{MarconiNr}");

                MarconiIdText.Text = $"---";

                MarconiIsOpen = false;
            }
        }

    }
}
