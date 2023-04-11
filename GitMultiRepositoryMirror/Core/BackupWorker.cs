using GitMultiRepositoryMirror.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitMuiltiRepositoryMirror.Core
{
    public static class BackupWorker
    {
        public static void BackupRepositories(BackupInfo config, Action<string> logLine)
        {
            if (!Directory.Exists(config.TargetPath))
                Directory.CreateDirectory(config.TargetPath);
            foreach (var item in config.Repositories)
            {
                BackupRepo($"{config.Server}/{item.RepositorySubPath}", config.TargetPath, item, logLine);
            }
        }

        private static void BackupRepo(string url, string directory, RepositoryInfo repoInfo, Action<string> logLine)
        {
            string workingDirectory = Path.Combine(directory, repoInfo.RepositorySubPath);
            if (Directory.Exists(workingDirectory))
            {
                //REPO INFO exists
                logLine?.Invoke($"Using existing repository {repoInfo.RepositorySubPath}");
            }
            else
            {
                Directory.CreateDirectory(workingDirectory);
                logLine?.Invoke($"Cloning repository {repoInfo.RepositorySubPath} ...");
                ExecuteGitCommand($"clone {url} {workingDirectory}", workingDirectory, logLine);
            }
            logLine?.Invoke("Pulling new data");
            ExecuteGitCommand("fetch --all", workingDirectory, logLine);
            ExecuteGitCommand("branch -r", workingDirectory, logLine);
            foreach (var item in repoInfo.Branches)
            {
                ExecuteGitCommand($"checkout {item}", workingDirectory, logLine);
            }
            ExecuteGitCommand("pull --all", workingDirectory, logLine);
        }

        private static void ExecuteGitCommand(string args, string workingDir, Action<string> logLine)
        {

            ProcessStartInfo psiClone = new ProcessStartInfo("git", args);
            psiClone.WorkingDirectory = workingDir;
            psiClone.UseShellExecute = false;
            psiClone.CreateNoWindow = true;
            psiClone.RedirectStandardOutput = true;
            var pClone = Process.Start(psiClone);
            while (!pClone.StandardOutput.EndOfStream)
                logLine?.Invoke(pClone.StandardOutput.ReadLine());
            pClone.WaitForExit();
        }

        private static string[] ExecuteGitCommandWithResult(string args, string workingDir, Action<string> logLine)
        {

            ProcessStartInfo psiClone = new ProcessStartInfo("git", args);
            psiClone.WorkingDirectory = workingDir;
            psiClone.UseShellExecute = false;
            psiClone.CreateNoWindow = true;
            psiClone.RedirectStandardOutput = true;
            var pClone = Process.Start(psiClone);
            List<string> lines = new List<string>();
            while (!pClone.StandardOutput.EndOfStream)
                lines.Add(pClone.StandardOutput.ReadLine());
            pClone.WaitForExit();
            return lines.ToArray();
        }
    }
}
