using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinDir.GraphQLSchema.GraphQLModel;

namespace WinDir.GraphQLResolver.GraphQLModel
{
    public class QLFileResolve : QLFile
    {
        public QLFileResolve(string aFileName) {
            name = aFileName;
        }
    }
}
