using GitMultiRepositoryMirror.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GitMuiltiRepositoryMirror.Core
{
    public static class BackupWorker
    {
        private static readonly Regex s_rxURLTokenReplace = new Regex(@"(https?:\/\/)(.*)");

        public static void BackupRepositories(BackupInfo config, Action<string> logLine)
        {
            if (!Directory.Exists(config.TargetPath))
                Directory.CreateDirectory(config.TargetPath);
            foreach (var item in config.Repositories)
            {
                BackupRepo(item.IsSubPathAbsolute ? item.RepositorySubPath : $"{config.Server}/{item.RepositorySubPath}", config.TargetPath, item, config.AuthToken, logLine);
            }
        }

        private static void BackupRepo(string url, string directory, RepositoryInfo repoInfo, string authToken, Action<string> logLine)
        {
            string workingDirectory = Path.Combine(directory, !string.IsNullOrEmpty(repoInfo.DirectoryName) ? repoInfo.DirectoryName : repoInfo.RepositorySubPath);
            string authUrl = url;
            bool useAuthToken = false;
            if (!string.IsNullOrEmpty(authToken) && s_rxURLTokenReplace.IsMatch(authUrl))
            {
                var match = s_rxURLTokenReplace.Match(authUrl);
                authUrl = $"{match.Groups[1]}{authToken.Trim()}@{match.Groups[2]}";
                useAuthToken = true;
            }
            if (Directory.Exists(workingDirectory))
            {
                //REPO INFO exists
                logLine?.Invoke($"Using existing repository {repoInfo.RepositorySubPath}");
                if (useAuthToken)
                    ExecuteGitCommand($"remote set-url origin {authUrl}", workingDirectory, logLine);
            }
            else
            {
                Directory.CreateDirectory(workingDirectory);
                logLine?.Invoke($"Cloning repository {repoInfo.RepositorySubPath} ...");
                ExecuteGitCommand($"clone {authUrl} {workingDirectory}", workingDirectory, logLine);
            }
            logLine?.Invoke("Pulling new data");
            ExecuteGitCommand("fetch --all", workingDirectory, logLine);
            ExecuteGitCommand("branch -v -a", workingDirectory, logLine);
            foreach (var item in repoInfo.Branches)
            {
                ExecuteGitCommand($"switch -c {item} origin/{item}", workingDirectory, logLine);
            }
            ExecuteGitCommand("pull --all", workingDirectory, logLine);
            ExecuteGitCommand($"remote set-url origin {url}", workingDirectory, logLine);
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
