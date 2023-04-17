using GitMultiRepositoryMirror.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GitMultiRepositoryMirror.Core
{
    public class BackupWorker
    {
        private static readonly Regex s_rxURLTokenReplace = new Regex(@"(https?:\/\/)(.*)");

        public event EventHandler<RepositoryBackupFinishedEventArgs> RepositoryBackupFinished;
        public delegate void LogMessage(string message, bool isError = false);

        public void BackupRepositories(BackupInfo config, LogMessage logLine)
        {
            if (!Directory.Exists(config.TargetPath))
                Directory.CreateDirectory(config.TargetPath);
            foreach (var item in config.Repositories)
            {
                BackupRepo(item.IsSubPathAbsolute ? item.RepositorySubPath : $"{config.Server}/{item.RepositorySubPath}", config.TargetPath, item, config.AuthToken, logLine);
            }
        }

        private void BackupRepo(string url, string directory, RepositoryInfo repoInfo, string authToken, LogMessage logLine)
        {
            repoInfo.BackupStarted = DateTime.Now;
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
                if (!ExecuteGitCommand($"clone {authUrl} {workingDirectory}", workingDirectory, logLine))
                    return;
            }
            logLine?.Invoke("Evaluating local branches");
            string[] defaultOutput, errorOutput;
            var branchSuccessful = ExecuteGitCommandWithResult("branch", workingDirectory, logLine, out defaultOutput, out errorOutput);
            bool errorOccuredIntermediate = false;
            if (branchSuccessful)
            {
                HashSet<string> existingBranches = new HashSet<string>();
                foreach (var item in defaultOutput)
                {
                    string branch = item;
                    if (branch.StartsWith("*"))
                        branch = branch.Substring(1);
                    branch = branch.Trim();
                    existingBranches.Add(branch);
                }
                logLine?.Invoke("Pulling new data");
                ExecuteGitCommand("fetch --all", workingDirectory, logLine);
                ExecuteGitCommand("branch -v -a", workingDirectory, logLine);
                foreach (var item in repoInfo.Branches)
                {
                    if (!existingBranches.Contains(item))
                        //ExecuteGitCommand($"switch -c {item} origin/{item}", workingDirectory, logLine);
                        if (!ExecuteGitCommand($"switch -c {item} origin/{item}", workingDirectory, logLine))
                            return;
                }
                ExecuteGitCommand("pull --all", workingDirectory, logLine);
            }
            ExecuteGitCommand($"remote set-url origin {url}", workingDirectory, logLine);
            repoInfo.BackupFinished = DateTime.Now;
            RepositoryBackupFinished?.Invoke(this, new RepositoryBackupFinishedEventArgs(repoInfo));
        }

        /// <summary>
        /// Executes the git command with the specified parameters
        /// </summary>
        /// <param name="args"></param>
        /// <param name="workingDir"></param>
        /// <param name="logLine">Logging method</param>
        /// <returns>true, if the command ran without any errors on StandardError. false otherwise</returns>
        private static bool ExecuteGitCommand(string args, string workingDir, LogMessage logLine)
        {

            ProcessStartInfo psiClone = new ProcessStartInfo("git", args);
            psiClone.WorkingDirectory = workingDir;
            psiClone.UseShellExecute = false;
            psiClone.CreateNoWindow = true;
            psiClone.RedirectStandardOutput = true;
            psiClone.RedirectStandardError = true;
            var pClone = Process.Start(psiClone);
            List<Tuple<string, bool>> logLinesWithError = new List<Tuple<string, bool>>();
            while (!pClone.StandardOutput.EndOfStream)
                logLinesWithError.Add(new Tuple<string, bool>(pClone.StandardOutput.ReadLine(), false));
            while (!pClone.StandardError.EndOfStream)
                logLinesWithError.Add(new Tuple<string, bool>(pClone.StandardError.ReadLine(), true));
            pClone.WaitForExit();
            var eCode = pClone.ExitCode;
            //Workaround as invoke of git command leads to normal messages being written to stderr
            foreach (var item in logLinesWithError)
                logLine?.Invoke(item.Item1, item.Item2 && eCode != 0);
            return eCode == 0;
        }

        private static bool ExecuteGitCommandWithResult(string args, string workingDir, LogMessage logLine, out string[] defaultOutput, out string[] errorOutput)
        {

            ProcessStartInfo psiClone = new ProcessStartInfo("git", args);
            psiClone.WorkingDirectory = workingDir;
            psiClone.UseShellExecute = false;
            psiClone.CreateNoWindow = true;
            psiClone.RedirectStandardOutput = true;
            psiClone.RedirectStandardError = true;
            var pClone = Process.Start(psiClone);
            List<string> lines = new List<string>();
            List<string> errorLines = new List<string>();
            while (!pClone.StandardOutput.EndOfStream)
                lines.Add(pClone.StandardOutput.ReadLine());
            while (!pClone.StandardError.EndOfStream)
                errorLines.Add(pClone.StandardError.ReadLine());
            pClone.WaitForExit();
            defaultOutput = lines.ToArray();
            errorOutput = errorLines.ToArray();
            return pClone.ExitCode == 0;
        }
    }
}
