using GraphQL;
using GraphQLParser.AST;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinDir.GraphQLResolver.GraphQL
{
    public static class GraphQLHelpers
    {
        public static GraphQLField GetFieldFromFieldOrContext(object aFieldOrContext, string name)
        {
            //https://systemoutofmemory.com/blogs/the-programmer-blog/c-sharp-switch-on-type
            switch (aFieldOrContext)
            {
                case GraphQLField aField:
                    return aField.SelectionSet.Selections.FirstOrDefault(x => (x as GraphQLField).Name == name) as GraphQLField;
                case IResolveFieldContext aContext:
                    return aContext.SubFields.FirstOrDefault(x => x.Key == name).Value.Field as GraphQLField;
                default:
                    return null;
            }
        }
    }
}
