using GitMultiRepositoryMirror.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitMultiRepositoryMirror.Core
{
    public class RepositoryBackupFinishedEventArgs : EventArgs
    {
        public RepositoryInfo Repository { get; }

        public RepositoryBackupFinishedEventArgs(RepositoryInfo repository)
        {
            Repository = repository;
        }
    }
}
