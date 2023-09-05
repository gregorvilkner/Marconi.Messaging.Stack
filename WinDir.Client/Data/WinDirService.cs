using GraphQL.NewtonsoftJson;
using GraphQL.Transport;
using GraphQL;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using WinDir.Client.Controllers;
using WinDir.GraphQLSchema.GraphQLModel;

namespace WinDir.Client.Data
{
    public class WinDirService
    {
        static GraphqlController aEntry { get; set; }
        public WinDirService(GraphqlService graphqlService)
        {
            aEntry = new GraphqlController(graphqlService);
        }
        public async Task<ExecutionResult> GetGraphQLResult(GraphQLRequest request)
        {

            var r = JObject.FromObject(request);

            r["Variables"] = JObject.Parse($"{{\"MarconiNr\": \"{aEntry._graphqlService.MarconiNr}\"}}");

            request = r.ToObject<GraphQLRequest>();

            var resultConnect2 = await aEntry.Post(request);

            var sw2 = new StringWriter();
            new GraphQLSerializer().Write(sw2, resultConnect2);

            var resultJson = JsonConvert.DeserializeObject<JObject>(sw2.ToString())["Value"].ToObject<ExecutionResult>();

            return resultJson;
        }

        public async Task<QLFolder> GetBrowseFolderAsync(GraphQLRequest request)
        {
            var qlResult = await GetGraphQLResult(request);
            var data = (JObject)qlResult.Data;
            var browseNode = data["folder"];
            var aResultList = browseNode.ToObject<QLFolder>();
            return aResultList;
        }
        public async Task<List<QLFolder>> GetBrowseFoldersAsync(GraphQLRequest request)
        {
            var qlResult = await GetGraphQLResult(request);
            var data = (JObject)qlResult.Data;
            var browseNode = data["folders"];
            var aResultList = browseNode.ToObject<List<QLFolder>>();
            return aResultList;
        }
    }
}
