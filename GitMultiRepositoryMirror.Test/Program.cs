using GitMultiRepositoryMirror.Core;
using GitMultiRepositoryMirror.Data;
using System;
using System.Diagnostics;
using System.IO.Compression;
using System.Reflection;
using System.Xml.Serialization;

namespace GitMultiRepositoryMirror.Test
{
    public class Program
    {
        public static int Main(string[] args)
        {
            string configPath = Path.Combine(new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName, "backup.config");
            if (!File.Exists(configPath))
            {
                BackupInfo.GenerateEmptyConfig(configPath);
                return Unconfigured();
            }
            var config = BackupInfo.FromConfigFile(configPath);
            if (config == null || config.Server.Contains("<") || config.Repositories.Select(it => it.RepositorySubPath).Contains("<PATH_TO_EXAMPLE_REPO>"))
                return Unconfigured();
            (new BackupWorker()).BackupRepositories(config, (msg, error) => Console.WriteLine($"{(error ? "ERROR: " : "")}{msg}"));
            Console.WriteLine("Done");
            return 0;
        }

        private static int Unconfigured()
        {
            Console.WriteLine("Configuration invalid");
            return 1;
        }
    }
}
