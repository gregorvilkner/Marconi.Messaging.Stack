using GraphQL;
using GraphQL.Types;
using WinDir.GraphQLSchema.IGraphQl;

namespace WinDir.GraphQLResolver.GraphQL
{
    public class Query : ObjectGraphType, IQuery
    {

        [GraphQLMetadata("hello")]
        public string GetHello()
        {
            return "Guglielmo Marconi (4/25/1874 – 7/20/1937)";
        }
    }
}
