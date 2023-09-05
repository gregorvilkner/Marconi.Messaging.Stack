using GraphQL;
using System.Collections.Generic;
using WinDir.GraphQLSchema.GraphQLModel;

namespace WinDir.GraphQLSchema.IGraphQl
{
    public interface IQuery
    {
        [GraphQLMetadata("hello")]
        string GetHello();

        [GraphQLMetadata("folder")]
        QLFolder GetFolder(IResolveFieldContext context, string aFolderDir);

        [GraphQLMetadata("folders")]
        List<QLFolder> GetFolders(IResolveFieldContext context, List<string> aFolderDirList);

    }
}