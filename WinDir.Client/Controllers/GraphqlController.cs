using GraphQL;
using GraphQL.Instrumentation;
using GraphQL.SystemTextJson;
using GraphQL.Transport;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;
using WinDir.Client.Data;
using WinDir.Client.GraphQL;

namespace WinDir.Client.Controllers
{
    [Route("graphql")]
    [ApiController]
    public class GraphqlController : ControllerBase
    {
        private readonly GraphqlService _graphqlService;

        public GraphqlController(GraphqlService graphqlService)
        {
            _graphqlService = graphqlService;
        }

        [HttpPost]
        public async Task<IActionResult> Post(GraphQLRequest aRequest)
        {

            if (
                aRequest.OperationName == "IntrospectionQuery" 
                || aRequest.Query.Replace("\n", "").Replace(" ", "").Contains("IntrospectionQuery")
                || aRequest.Query.Replace("\n", "").Replace(" ", "").Contains("{hello}")
                )
            {

                var start = DateTime.UtcNow;

                ExecutionResult json = await new DocumentExecuter().ExecuteAsync(_ =>
                {
                    _.Schema = new MySchema().GraphQLSchema;
                    _.Query = aRequest.Query;
                    _.OperationName = aRequest.OperationName;
                    _.Variables = aRequest.Variables;
                    _.EnableMetrics = true;
                });

                json.EnrichWithApolloTracing(start);

                // https://www.lucanatali.it/c-from-string-to-stream-and-from-stream-to-string/
                var ms = new MemoryStream();
                var sw = new StreamWriter(ms);

                await new GraphQLSerializer().WriteAsync(ms, json);

                string resultJson = Encoding.ASCII.GetString(ms.ToArray());


                return Ok(JObject.Parse(resultJson));

            }
            else
            {

                var start = DateTime.UtcNow;

                var schema = new MySchema();

                if (aRequest.Variables.ContainsKey("MarconiNr"))
                {
                    await _graphqlService.SetMarconiNrAsync(aRequest.Variables["MarconiNr"].ToString());
                }
                ExecutionResult json = new ExecutionResult();
                json.Errors = new ExecutionErrors();

                if (_graphqlService.MarconiKey == "" || _graphqlService.MarconiKey == null)
                {
                    if(_graphqlService.MarconiKey == null)
                    {
                        json.Errors.Add(new ExecutionError("Missing MarconiNr."));
                    } else
                    {
                        json.Errors.Add(new ExecutionError("Invalid MarconiNr."));
                    }

                    var ms = new MemoryStream();
                    var sw = new StreamWriter(ms);

                    await new GraphQLSerializer().WriteAsync(ms, json);

                    string resultJson = Encoding.ASCII.GetString(ms.ToArray());

                    return Ok(JObject.Parse(resultJson));

                }
                else
                {
                    var aMessenger = new GraphQLMessenger(aRequest, _graphqlService.MarconiNr, _graphqlService.MarconiKey);
                    await aMessenger.ProcessQuery();
                    await aMessenger.Wait4Response();
                    if (aMessenger.JResponseIsReady == false)
                    {
                        json.Errors.Add(new ExecutionError("Marconi call timed out (40sec)..."));

                        var ms = new MemoryStream();
                        var sw = new StreamWriter(ms);

                        await new GraphQLSerializer().WriteAsync(ms, json);

                        string resultJson = Encoding.ASCII.GetString(ms.ToArray());

                        return Ok(JObject.Parse(resultJson));

                    }
                    else
                    {
                        return Ok(aMessenger.JResponse);
                    }
                }
                return Ok(JObject.Parse("{}"));

            }

        }

    }
}
