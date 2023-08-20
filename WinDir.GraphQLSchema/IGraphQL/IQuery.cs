using GraphQL;
using System.Collections.Generic;

namespace WinDir.GraphQLSchema.IGraphQl
{
    public interface IQuery
    {
        [GraphQLMetadata("hello")]
        string GetHello();


    }
}