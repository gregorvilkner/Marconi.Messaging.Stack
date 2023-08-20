using GraphQL;
using GraphQL.Instrumentation;
using WinDir.GraphQLResolver.GraphQL;

namespace WinDir.GraphQLResolver
{
    public class ResolverEntry
    {

        public ResolverEntry()
        {

        }

        public async Task<ExecutionResult> GetResultAsync(GraphQLQuery query)
        {
            var start = DateTime.UtcNow;

            // https://fiyazhasan.me/graphql-with-net-core-part-v-fields-arguments-variables/
            var inputs = query.Variables.ToInputs();

            // https://github.com/graphql-dotnet/examples/blob/master/src/AspNetWebApi/WebApi/Controllers/GraphQLController.cs
            var result = await new DocumentExecuter().ExecuteAsync(_ =>
            {
                _.Schema = new MySchema().GraphQLSchema;
                _.Query = query.Query;
                _.OperationName = query.OperationName;
                _.Inputs = inputs;
                _.EnableMetrics = true;
            });

            result.EnrichWithApolloTracing(start);

            return result;

        }
    }
}
