using GraphQL.Types;

namespace WinDir.Client.GraphQL
{
    public class MySchema
    {
        private ISchema _schema { get; set; }
        public ISchema GraphQLSchema
        {
            get
            {
                return this._schema;
            }
        }

        public MySchema()
        {
            this._schema = Schema.For(WinDir.GraphQLSchema.GraphQLSchema.schema, _ =>
            {
                _.Types.Include<Query>();
            });
        }


    }


}

