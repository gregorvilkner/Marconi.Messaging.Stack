using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinDir.GraphQLSchema.GraphQLModel
{
    public class QLFolder
    {
        public string dir { get; set; }
        public string name { get; set; }

        public List<QLFile> files { get; set; }

        public List<QLFolder> folders { get; set; }
    }
}
