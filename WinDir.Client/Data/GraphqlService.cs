using GraphQL.Transport;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using WinDir.Client.GraphQL;

namespace WinDir.Client.Data
{
    public class GraphqlService
    {
        public string MarconiNr { get; private set; }
        public string MarconiKey { get; private set; }

        public bool KeyIsValid
        {
            get
            {
                return MarconiKey != null && MarconiKey != "";
            }
        }

        public event Action OnChange;

        public async Task SetMarconiNrAsync(string _marconiNr)
        {
            MarconiNr = _marconiNr;
            await VerifyQueueNameAsync();
            NotifyStateChanged();
        }

        private void NotifyStateChanged() => OnChange?.Invoke();

        public async Task<bool> VerifyQueueNameAsync()
        {

            HttpClient aClient = new HttpClient();
            aClient.BaseAddress = new Uri("https://marconirelay.azurewebsites.net");
            var aResponse = await aClient.GetAsync($"MarconiNr/{MarconiNr}");
            MarconiKey = await aResponse.Content.ReadAsStringAsync();

            return MarconiKey != "";
        }


        public async Task<JObject> ProcessQueryAsync(string queryString)
        {
            if (MarconiNr != "" && MarconiKey == null) await VerifyQueueNameAsync();

            if (MarconiNr == null || MarconiKey == "") return null;

            var start = DateTime.UtcNow;

            GraphQLRequest aRequest = new GraphQLRequest();
            aRequest.Query = $"{queryString}";

            var aMessenger = new GraphQLMessenger(aRequest, MarconiNr, MarconiKey);


            if (!await aMessenger.ProcessQuery())
            {
                return null;
            }
            else
            {
                await aMessenger.Wait4Response();
                if (aMessenger.JResponseIsReady == false)
                {
                    return null;
                }
                else
                {


                    JObject result = aMessenger.JResponse;

                    return result;
                }
            }

        }
        static public List<string> Result2Strings(JObject aResult, string aProp)
        {
            if (aResult == null) return null;
            var returnObject = aResult["data"][aProp].ToObject<List<string>>();
            return returnObject;
        }

    }
}

