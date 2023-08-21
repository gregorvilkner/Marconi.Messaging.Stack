namespace WinDir.GraphQLSchema
{
    public static class GraphQLSchema
    {
        public static readonly string schema = @"
            
            type File {
                name: String
            }

            type Folder {
                name: String
                dir: String
                folders: [Folder]
                files: [File]
            }

            type Query {
                hello: String
                folder(aFolderDir: String): Folder
            }


        ";

    }
}
