using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Instalador.Models;

namespace Instalador.Services
{
    public interface IBuildService
    {
        Task<bool> RunCommandAsync(string fileName, string args, string workingDir, Action<string> onLineReceived);
        Task RunBuildAsync(ProyectoConfig proyecto, Action<string> onLog, string configuration = "Release");
        Task RunPublishAsync(ProyectoConfig proyecto, Action<string> onLog, string configuration = "Release");
    }

    public class BuildService : IBuildService
    {
        private static string? ResolverRutaProyectoParaDotnet(string rutaProyecto, Action<string> onLog)
        {
            if (string.IsNullOrWhiteSpace(rutaProyecto))
            {
                return null;
            }

            // Permitir que RutaProyecto sea directamente un .sln o .csproj
            if (File.Exists(rutaProyecto) && (rutaProyecto.EndsWith(".sln", StringComparison.OrdinalIgnoreCase) || rutaProyecto.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase)))
            {
                return rutaProyecto;
            }

            if (!Directory.Exists(rutaProyecto))
            {
                onLog($"ERROR: RutaProyecto no existe: {rutaProyecto}");
                return null;
            }

            // Preferir solución si hay una única
            var soluciones = Directory.GetFiles(rutaProyecto, "*.sln", SearchOption.TopDirectoryOnly);
            if (soluciones.Length == 1)
            {
                return soluciones[0];
            }

            // Si no hay solución única, intentar con un único csproj
            var csprojs = Directory.GetFiles(rutaProyecto, "*.csproj", SearchOption.TopDirectoryOnly);
            if (csprojs.Length == 1)
            {
                return csprojs[0];
            }

            if (soluciones.Length > 1)
            {
                onLog("ERROR: Hay más de un .sln en RutaProyecto. Indica un archivo .sln específico en RutaProyecto.");
                foreach (var s in soluciones)
                {
                    onLog($" - {s}");
                }
                return null;
            }

            if (csprojs.Length > 1)
            {
                onLog("ERROR: Hay más de un .csproj en RutaProyecto. Indica un archivo .csproj específico en RutaProyecto.");
                foreach (var c in csprojs)
                {
                    onLog($" - {c}");
                }
                return null;
            }

            onLog("ERROR: No se encontró ningún .sln ni .csproj en RutaProyecto.");
            return null;
        }

        public async Task RunBuildAsync(ProyectoConfig p, Action<string> onLog, string configuration = "Release")
        {
            var rutaTarget = ResolverRutaProyectoParaDotnet(p.RutaProyecto, onLog);
            if (string.IsNullOrWhiteSpace(rutaTarget))
            {
                onLog("ERROR: No se pudo resolver RutaProyecto. Configura RutaProyecto con un .sln o .csproj.");
                return;
            }

            var workingDir = Directory.Exists(p.RutaProyecto)
                ? p.RutaProyecto
                : (Path.GetDirectoryName(rutaTarget) ?? p.RutaProyecto);

            string args = $"build \"{rutaTarget}\" -c {configuration}";
            await RunCommandAsync("dotnet", args, workingDir, onLog);
        }

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
                        StandardOutputEncoding = Encoding.UTF8,
                        StandardErrorEncoding = Encoding.UTF8,
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

        public async Task RunPublishAsync(ProyectoConfig p, Action<string> onLog, string configuration = "Release")
        {
            string outputDir = System.IO.Path.Combine(p.RutaPublicacion, "win-x64-singlefile");
            string flags = "-r win-x64 --self-contained true /p:PublishSingleFile=true";
            
            if (p.ReadyToRun) flags += " /p:PublishReadyToRun=true";
            if (p.Trimmed) flags += " /p:PublishTrimmed=true";
            if (p.Compressed) flags += " /p:EnableCompressionInSingleFile=true";

            var rutaTarget = ResolverRutaProyectoParaDotnet(p.RutaProyecto, onLog);
            if (string.IsNullOrWhiteSpace(rutaTarget))
            {
                onLog("ERROR: No se pudo resolver RutaProyecto. Configura RutaProyecto con un .sln o .csproj.");
                return;
            }

            var workingDir = Directory.Exists(p.RutaProyecto)
                ? p.RutaProyecto
                : (Path.GetDirectoryName(rutaTarget) ?? p.RutaProyecto);

            string args = $"publish \"{rutaTarget}\" -c {configuration} -o \"{outputDir}\" {flags}";
            await RunCommandAsync("dotnet", args, workingDir, onLog);
        }
    }
}
