using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Instalador.Services
{
    public interface IGitService
    {
        Task<(string Branch, bool HasChanges)> GetStatusAsync(string repoPath);
    }

    public class GitService : IGitService
    {
        public async Task<(string Branch, bool HasChanges)> GetStatusAsync(string repoPath)
        {
            if (string.IsNullOrEmpty(repoPath) || !Directory.Exists(Path.Combine(repoPath, ".git")))
                return ("No es un repo Git", false);

            try
            {
                string branch = await Task.Run(() => RunSilentProcess("git", "branch --show-current", repoPath).Trim());
                string status = await Task.Run(() => RunSilentProcess("git", "status --porcelain", repoPath).Trim());
                return (branch, !string.IsNullOrWhiteSpace(status));
            }
            catch { return ("Error Git", false); }
        }

        private string RunSilentProcess(string fileName, string args, string workingDir)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = args,
                    WorkingDirectory = workingDir,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                using var process = Process.Start(psi);
                return process?.StandardOutput.ReadToEnd() ?? "";
            }
            catch { return ""; }
        }
    }
}
