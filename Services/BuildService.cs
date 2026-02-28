using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Instalador.Models;

namespace Instalador.Services
{
    public interface IBuildService
    {
        Task<bool> RunCommandAsync(string fileName, string args, string workingDir, Action<string> onLineReceived);
        Task RunPublishAsync(ProyectoConfig proyecto, Action<string> onLog);
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

        public async Task RunPublishAsync(ProyectoConfig p, Action<string> onLog)
        {
            string outputDir = p.RutaPublicacion;
            string flags = "-r win-x64 --self-contained true /p:PublishSingleFile=true";
            
            if (p.ReadyToRun) flags += " /p:PublishReadyToRun=true";
            if (p.Trimmed) flags += " /p:PublishTrimmed=true";
            if (p.Compressed) flags += " /p:EnableCompressionInSingleFile=true";

            string args = $"publish -c Release -o \"{outputDir}\" {flags}";
            await RunCommandAsync("dotnet", args, p.RutaProyecto, onLog);
        }
    }
}
