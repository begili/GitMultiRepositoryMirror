using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitMultiRepositoryMirror.Data
{
    public class RepositoryInfo
    {
        public string RepositorySubPath { get; set; } = "sub/path.git";

        public string DirectoryName { get; set; }

        public List<string> Branches { get; }

        public RepositoryInfo()
        {
            Branches = new List<string>();
        }
    }
}
