using GraphQL.Types;
using GraphQLParser.AST;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinDir.GraphQLResolver.GraphQL;
using WinDir.GraphQLSchema.GraphQLModel;

namespace WinDir.GraphQLResolver.GraphQLModel
{
    public class QLFolderResolve : QLFolder
    {
        public QLFolderResolve(
            string aFolderDir, GraphQLField foldersField, GraphQLField filesField)
        {
            if (Path.Exists(aFolderDir))
            {
                dir = Path.GetFullPath(aFolderDir);
                name = Path.GetFileName(aFolderDir);

                if (foldersField != null)
                {
                    folders = new List<QLFolder>();
                    foreach (var aChildFolder in Directory.GetDirectories(dir))
                    {
                        var foldersChildrenField = GraphQLHelpers.GetFieldFromFieldOrContext(foldersField, "folders");
                        var filesChildrenField = GraphQLHelpers.GetFieldFromFieldOrContext(foldersField, "files");
                        folders.Add(new QLFolderResolve(aChildFolder, foldersChildrenField, filesChildrenField));
                    }
                }

                if (filesField != null)
                {
                    files = new List<QLFile>();
                    foreach(var aFile in Directory.GetFiles(dir))
                    {
                        files.Add(new QLFile
                        {
                            name = aFile
                        });
                    }
                }
            }

        }
    }
}
