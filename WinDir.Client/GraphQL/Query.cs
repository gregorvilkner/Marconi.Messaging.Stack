using GraphQL;
using GraphQL.Types;
using System.Collections.Generic;
using System.Threading.Tasks;
using WinDir.GraphQLSchema.GraphQLModel;
using WinDir.GraphQLSchema.IGraphQl;

namespace WinDir.Client.GraphQL
{
    public class Query : ObjectGraphType, IQuery
    {

        [GraphQLMetadata("hello")]
        public string GetHello()
        {
            return "Guglielmo Marconi (4/25/1874 – 7/20/1937)";
        }

        [GraphQLMetadata("folder")]
        public QLFolder GetFolder(IResolveFieldContext context, string aFolderDir)
        {
            throw new NotImplementedException();
        }
        

    }
}
