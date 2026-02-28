using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Instalador.Services
{
    public interface IBuildService
    {
        Task<bool> RunCommandAsync(string fileName, string args, string workingDir, Action<string> onLineReceived);
    }

    public class BuildService : IBuildService
    {
        public async Task<bool> RunCommandAsync(string fileName, string args, string workingDir, Action<string> onLineReceived)
        {
            return await Task.Run(() =>
            {
                try
                {
                    var psi = new ProcessStartInfo
                    {
                        FileName = fileName,
                        Arguments = args,
                        WorkingDirectory = workingDir,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    using var process = new Process { StartInfo = psi };
                    process.OutputDataReceived += (s, e) => { if (e.Data != null) onLineReceived(e.Data); };
                    process.ErrorDataReceived += (s, e) => { if (e.Data != null) onLineReceived("ERROR: " + e.Data); };

                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();
                    process.WaitForExit();

                    return process.ExitCode == 0;
                }
                catch (Exception ex)
                {
                    onLineReceived("EXCEPCIÓN: " + ex.Message);
                    return false;
                }
            });
        }
    }
}
