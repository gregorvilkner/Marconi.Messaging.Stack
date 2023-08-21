using GraphQL;
using GraphQL.Types;
using GraphQLParser.AST;
using WinDir.GraphQLResolver.GraphQLModel;
using WinDir.GraphQLSchema.GraphQLModel;
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

        [GraphQLMetadata("folder")]
        public QLFolder GetFolder(IResolveFieldContext context, string aFolderDir)
        {
            var foldersField = GraphQLHelpers.GetFieldFromFieldOrContext(context, "folders");
            var filesField = GraphQLHelpers.GetFieldFromFieldOrContext(context, "files");
            return new QLFolderResolve(aFolderDir, foldersField, filesField);
        }

    }
}
