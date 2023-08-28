using GraphQL;
using GraphQL.Instrumentation;
using GraphQL.NewtonsoftJson;
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

                ExecutionResult result = await new DocumentExecuter().ExecuteAsync(_ =>
                {
                    _.Schema = new MySchema().GraphQLSchema;
                    _.Query = aRequest.Query;
                    _.OperationName = aRequest.OperationName;
                    _.Variables = aRequest.Variables;
                    _.EnableMetrics = true;
                });

                result.EnrichWithApolloTracing(start);

                var stringWriter = new StringWriter();
                new GraphQLSerializer().Write(stringWriter, result);
                JObject returnObject = JObject.Parse(stringWriter.ToString());

                return Ok();

            }
            else
            {

                var start = DateTime.UtcNow;

                var schema = new MySchema();

                if (aRequest.Variables.ContainsKey("MarconiNr"))
                {
                    await _graphqlService.SetMarconiNrAsync(aRequest.Variables["MarconiNr"].ToString());
                }
                ExecutionResult result = new ExecutionResult();
                result.Errors = new ExecutionErrors();

                if (_graphqlService.MarconiKey == "" || _graphqlService.MarconiKey == null)
                {
                    if (_graphqlService.MarconiKey == null)
                    {
                        result.Errors.Add(new ExecutionError("Missing MarconiNr."));
                    }
                    else
                    {
                        result.Errors.Add(new ExecutionError("Invalid MarconiNr."));
                    }

                    var stringWriter = new StringWriter();
                    new GraphQLSerializer().Write(stringWriter, result);
                    JObject returnObject = JObject.Parse(stringWriter.ToString());

                    return Ok(returnObject);

                }
                else
                {
                    var aMessenger = new GraphQLMessenger(aRequest, _graphqlService.MarconiNr, _graphqlService.MarconiKey);
                    await aMessenger.ProcessQuery();
                    await aMessenger.Wait4Response();
                    if (aMessenger.JResponseIsReady == false)
                    {
                        result.Errors.Add(new ExecutionError("Marconi call timed out (40sec)..."));

                        var stringWriter = new StringWriter();
                        new GraphQLSerializer().Write(stringWriter, result);
                        JObject returnObject = JObject.Parse(stringWriter.ToString());

                        return Ok(returnObject);

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
