using GraphQL;
using GraphQL.Instrumentation;
using GraphQL.Transport;
using WinDir.GraphQLResolver.GraphQL;

namespace WinDir.GraphQLResolver
{
    public class ResolverEntry
    {

        public ResolverEntry()
        {

        }

        public async Task<ExecutionResult> GetResultAsync(GraphQLRequest aRequest)
        {
            var start = DateTime.UtcNow;

            // https://github.com/graphql-dotnet/examples/blob/master/src/AspNetWebApi/WebApi/Controllers/GraphQLController.cs
            ExecutionResult json = await new DocumentExecuter().ExecuteAsync(_ =>
            {
                _.Schema = new MySchema().GraphQLSchema;
                _.Query = aRequest.Query;
                _.OperationName = aRequest.OperationName;
                _.Variables = aRequest.Variables;
                _.EnableMetrics = true;
            });

            json.EnrichWithApolloTracing(start);

            return json;

        }
    }
}
