﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace GitMultiRepositoryMirror.Data
{
    public class RepositoryInfo
    {
        public string RepositorySubPath { get; set; } = "sub/path.git";

        public string DirectoryName { get; set; }

        public List<string> Branches { get; }

        public bool ConfigureAsNonInteractive { get; set; }

        public bool IsSubPathAbsolute { get; set; }

        [XmlIgnore]
        public DateTime BackupStarted { get; set; }

        [XmlIgnore]
        public DateTime BackupFinished { get; set; }

        public RepositoryInfo()
        {
            Branches = new List<string>();
        }
    }
}
