using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace Instalador
{
    public partial class MainWindow : Window
    {
        private Config config = null!;
        private Stopwatch cronometro = new Stopwatch();
        private DispatcherTimer timerHora = new DispatcherTimer();
        private DispatcherTimer timerReloj = new DispatcherTimer();

        public ObservableCollection<LogEntry> LogEntries { get; set; } = new ObservableCollection<LogEntry>();

        public MainWindow()
        {
            InitializeComponent();
            LvLog.ItemsSource = LogEntries;
            CargarConfig();

            timerHora.Interval = TimeSpan.FromSeconds(1);
            timerHora.Tick += (s, e) => TxtHora.Text = DateTime.Now.ToString("HH:mm:ss");
            timerHora.Start();

            timerReloj.Interval = TimeSpan.FromMilliseconds(500);
            timerReloj.Tick += (s, e) => TxtTiempo.Text = "Tiempo: " + cronometro.Elapsed.ToString(@"mm\:ss");

            ComboConfig.SelectedIndex = 0; // Release
        }

        private void CargarConfig()
        {
            config = Config.Cargar();
            
            // Llenar combo de proyectos
            ComboProyectos.SelectionChanged -= ComboProyectos_SelectionChanged;
            ComboProyectos.ItemsSource = config.Proyectos;
            
            var actual = config.GetProyectoActual();
            if (actual != null)
            {
                ComboProyectos.SelectedItem = actual;
            }
            ComboProyectos.SelectionChanged += ComboProyectos_SelectionChanged;

            Log("Configuración cargada.");
        }

        private void ComboProyectos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboProyectos.SelectedItem is ProyectoConfig p)
            {
                config.UltimoProyectoSeleccionado = p.Nombre;
                config.Guardar();
                Log($"Cambiado al proyecto: {p.Nombre}");
            }
        }

        private void Log(string mensaje)
        {
            Dispatcher.Invoke(() =>
            {
                string hora = DateTime.Now.ToString("HH:mm:ss");
                LogEntries.Add(new LogEntry { Hora = hora, Mensaje = mensaje });
                if (LogEntries.Count > 0)
                    LvLog.ScrollIntoView(LogEntries[LogEntries.Count - 1]);

                // Guardar en archivo de log persistente
                try
                {
                    string logDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
                    if (!Directory.Exists(logDir)) Directory.CreateDirectory(logDir);
                    
                    string logFile = Path.Combine(logDir, $"log_{DateTime.Now:yyyyMMdd}.txt");
                    File.AppendAllLines(logFile, new[] { $"[{hora}] {mensaje}" });
                }
                catch { /* Ignorar errores de escritura de log para no bloquear la UI */ }
            });
        }

        private void SetProgress(int value)
        {
            Dispatcher.Invoke(() => ProgressTotal.Value = Math.Max(0, Math.Min(100, value)));
        }

        private void StepProgress(int step)
        {
            Dispatcher.Invoke(() => SetProgress((int)ProgressTotal.Value + step));
        }

        private void IniciarCronometro()
        {
            cronometro.Reset();
            cronometro.Start();
            timerReloj.Start();
        }

        private void DetenerCronometro()
        {
            cronometro.Stop();
            timerReloj.Stop();
        }

        private string SelectedConfig => (ComboConfig.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Release";

        private int RunProcess(string fileName, string arguments, string workingDirectory)
        {
            Log($"> {fileName} {arguments}");

            var psi = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                WorkingDirectory = workingDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                StandardOutputEncoding = System.Text.Encoding.UTF8,
                StandardErrorEncoding = System.Text.Encoding.UTF8
            };

            using var proc = new Process();
            proc.StartInfo = psi;

            proc.OutputDataReceived += (s, e) => { if (!string.IsNullOrWhiteSpace(e.Data)) Log(e.Data); };
            proc.ErrorDataReceived += (s, e) => { if (!string.IsNullOrWhiteSpace(e.Data)) Log("[ERR] " + e.Data); };

            proc.Start();
            proc.BeginOutputReadLine();
            proc.BeginErrorReadLine();
            proc.WaitForExit();

            Log($"Proceso terminado con código {proc.ExitCode}");
            return proc.ExitCode;
        }

        private void CopiarRecursos()
        {
            var p = config.GetProyectoActual();
            if (p == null) return;

            try
            {
                string destino = Path.Combine(p.RutaPublicacion, "win-x64-singlefile");
                if (!Directory.Exists(destino)) Directory.CreateDirectory(destino);

                string origenImg = Path.Combine(p.RutaProyecto, "img");
                string destinoImg = Path.Combine(destino, "img");

                if (Directory.Exists(origenImg))
                {
                    if (Directory.Exists(destinoImg)) Directory.Delete(destinoImg, true);
                    Directory.CreateDirectory(destinoImg);
                    foreach (var file in Directory.GetFiles(origenImg, "*.*", SearchOption.AllDirectories))
                    {
                        string relative = file.Substring(origenImg.Length + 1);
                        string destFile = Path.Combine(destinoImg, relative);
                        Directory.CreateDirectory(Path.GetDirectoryName(destFile)!);
                        File.Copy(file, destFile, true);
                    }
                    Log("Carpeta img copiada.");
                }

                string origenReadme = Path.Combine(p.RutaProyecto, "README.md");
                string destinoReadme = Path.Combine(destino, "README.md");
                if (File.Exists(origenReadme))
                {
                    File.Copy(origenReadme, destinoReadme, true);
                    Log("README.md copiado.");
                }
            }
            catch (Exception ex) { Log("[ERR] Error copiando recursos: " + ex.Message); }
        }

        private void CrearZipConProgreso(string origen, string destino)
        {
            if (!Directory.Exists(origen)) throw new DirectoryNotFoundException($"Origen no existe: {origen}");
            var archivos = Directory.GetFiles(origen, "*", SearchOption.AllDirectories);
            int total = archivos.Length;
            int contador = 0;

            if (File.Exists(destino)) File.Delete(destino);

            using (FileStream zipToOpen = new FileStream(destino, FileMode.Create))
            using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Create))
            {
                foreach (var file in archivos)
                {
                    string relative = file.Substring(origen.Length + 1);
                    archive.CreateEntryFromFile(file, relative);
                    contador++;
                    int progreso = (int)((contador / (double)total) * 100);
                    Log("Añadiendo: " + relative);
                    SetProgress(progreso);
                }
            }
        }

        // --- Manejadores de Eventos ---

        private void BtnSalir_Click(object sender, RoutedEventArgs e) => Close();

        private async void BtnLimpiar_Click(object sender, RoutedEventArgs e)
        {
            var p = config.GetProyectoActual();
            if (p == null) { Log("[ERR] No hay proyecto seleccionado."); return; }

            LogEntries.Clear();
            Log($"=== LIMPIEZA COMPLETA: {p.Nombre} ===");
            SetProgress(0);
            IniciarCronometro();

            await Task.Run(() =>
            {
                if (Directory.Exists(p.RutaPublicacion))
                {
                    try { Directory.Delete(p.RutaPublicacion, true); Log("Directorio de publicación eliminado."); }
                    catch (Exception ex) { Log("[WARN] No se pudo borrar la carpeta de publicación: " + ex.Message); }
                }
                RunProcess("dotnet", "clean", p.RutaProyecto);
                RunProcess("dotnet", "restore", p.RutaProyecto);
            });

            SetProgress(100);
            Log("Limpieza completada.");
            DetenerCronometro();
            
            // Habilitar Utilidades tras la limpieza
            MenuUtilidades.IsEnabled = true;
        }

        private async void BtnCompilar_Click(object sender, RoutedEventArgs e)
        {
            var p = config.GetProyectoActual();
            if (p == null) return;

            LogEntries.Clear();
            Log($"=== COMPILAR PROYECTO: {p.Nombre} ({SelectedConfig}) ===");
            SetProgress(0);
            IniciarCronometro();

            int exitCode = -1;
            await Task.Run(() => exitCode = RunProcess("dotnet", $"build -c {SelectedConfig}", p.RutaProyecto));

            if (exitCode == 0) { Log("Compilación exitosa."); SetProgress(100); }
            else { Log("[ERR] Compilación fallida."); SetProgress(0); }

            DetenerCronometro();
        }

        private async void BtnPublicar_Click(object sender, RoutedEventArgs e)
        {
            var p = config.GetProyectoActual();
            if (p == null) return;

            LogEntries.Clear();
            Log($"=== PUBLICAR SINGLE-FILE: {p.Nombre} ({SelectedConfig}) ===");
            SetProgress(0);
            IniciarCronometro();

            string? csprojPath = ObtenerCsproj();
            if (csprojPath == null) return;

            string outputDir = Path.Combine(p.RutaPublicacion, "win-x64-singlefile");
            if (!Directory.Exists(p.RutaPublicacion)) Directory.CreateDirectory(p.RutaPublicacion);

            string args = $"publish \"{csprojPath}\" -c {SelectedConfig} -r win-x64 --self-contained true /p:PublishSingleFile=true /p:PublishTrimmed=false -o \"{outputDir}\"";

            int exitCode = -1;
            await Task.Run(() => exitCode = RunProcess("dotnet", args, p.RutaProyecto));

            if (exitCode == 0 && Directory.Exists(outputDir) && Directory.GetFiles(outputDir).Length > 0)
            {
                CopiarRecursos();
                Log("Publicación exitosa.");
                SetProgress(100);
            }
            else { Log("[ERR] Falló la publicación."); SetProgress(0); }

            DetenerCronometro();
        }

        private async void BtnZipPortable_Click(object sender, RoutedEventArgs e)
        {
            var p = config.GetProyectoActual();
            if (p == null) return;

            Log("=== CREAR ZIP PORTABLE ===");
            IniciarCronometro();
            string outputDir = Path.Combine(p.RutaPublicacion, "win-x64-singlefile");
            string zipPath = Path.Combine(p.RutaPublicacion, $"{p.Nombre}_Portable.zip");
            
            await Task.Run(() => CrearZipConProgreso(outputDir, zipPath));
            
            Log("ZIP Portable creado exitosamente.");
            DetenerCronometro();
        }

        private async void BtnZipSingleFile_Click(object sender, RoutedEventArgs e)
        {
            var p = config.GetProyectoActual();
            if (p == null) return;

            Log("=== CREAR ZIP SINGLE-FILE ===");
            IniciarCronometro();
            string outputDir = Path.Combine(p.RutaPublicacion, "win-x64-singlefile");
            string zipPath = Path.Combine(p.RutaPublicacion, $"{p.Nombre}_SingleFile.zip");

            await Task.Run(() => CrearZipConProgreso(outputDir, zipPath));

            Log("ZIP SingleFile creado exitosamente.");
            DetenerCronometro();
        }

        private string? ObtenerCsproj()
        {
            var p = config.GetProyectoActual();
            if (p == null) return null;

            string path = Path.Combine(p.RutaProyecto, $"{p.Nombre}.csproj");
            if (File.Exists(path)) return path;

            var archivos = Directory.GetFiles(p.RutaProyecto, "*.csproj");
            if (archivos.Length > 0)
            {
                Log($"[INFO] Usando: {Path.GetFileName(archivos[0])}");
                return archivos[0];
            }
            Log("[ERR] No se encontró .csproj");
            DetenerCronometro();
            return null;
        }

        private async void BtnInstalador_Click(object sender, RoutedEventArgs e)
        {
            var p = config.GetProyectoActual();
            if (p == null) return;

            LogEntries.Clear();
            Log($"=== GENERAR INSTALADOR: {p.Nombre} ===");
            SetProgress(0);
            IniciarCronometro();

            string iss = config.RutaInnoSetup;
            if (!File.Exists(iss)) { Log("[ERR] No se encontró ISCC.exe"); DetenerCronometro(); return; }

            string issScript = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "installer.iss");
            if (!File.Exists(issScript))
                issScript = Path.Combine(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)!.Parent!.Parent!.FullName, "installer.iss");

            string args = $"/Sspawn=0 /V=1 /DMyAppName=\"{p.Nombre}\" /DMyAppVersion=\"{p.VersionInstalador}\" /DMyAppExeName=\"{p.Nombre}.exe\" /DPublishDir=\"{Path.Combine(p.RutaPublicacion, "win-x64-singlefile")}\" /O\"{p.RutaPublicacion}\" \"{issScript}\"";

            await Task.Run(() => RunProcess(iss, args, p.RutaProyecto));
            Log("Proceso de instalador finalizado.");
            SetProgress(100);
            DetenerCronometro();
        }

        private async void BtnEjecutarTodo_Click(object sender, RoutedEventArgs e)
        {
            var p = config.GetProyectoActual();
            if (p == null) return;

            LogEntries.Clear();
            Log($"=== EJECUTAR TODO: {p.Nombre} ({SelectedConfig}) ===");
            SetProgress(0);
            IniciarCronometro();

            string? csproj = ObtenerCsproj();
            if (csproj == null) return;

            Log("Iniciando flujo completo...");
            await Task.Run(() => {
                // Build
                if (RunProcess("dotnet", $"build -c {SelectedConfig}", p.RutaProyecto) != 0) return;
                SetProgress(20);

                // Publish
                string outputDir = Path.Combine(p.RutaPublicacion, "win-x64-singlefile");
                string args = $"publish \"{csproj}\" -c {SelectedConfig} -r win-x64 --self-contained true /p:PublishSingleFile=true /p:PublishTrimmed=false -o \"{outputDir}\"";
                if (RunProcess("dotnet", args, p.RutaProyecto) != 0) return;
                CopiarRecursos();
                SetProgress(40);

                // ZIPs (Portable)
                string zipPath = Path.Combine(p.RutaPublicacion, $"{p.Nombre}_Portable.zip");
                CrearZipConProgreso(outputDir, zipPath);
                SetProgress(60);

                // Instalador (Llamada síncrona dentro del Task.Run)
                string iss = config.RutaInnoSetup;
                if (File.Exists(iss))
                {
                    string issScript = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "installer.iss");
                    if (!File.Exists(issScript))
                        issScript = Path.Combine(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)!.Parent!.Parent!.FullName, "installer.iss");

                    string iArgs = $"/Sspawn=0 /V=1 /DMyAppName=\"{p.Nombre}\" /DMyAppVersion=\"{p.VersionInstalador}\" /DMyAppExeName=\"{p.Nombre}.exe\" /DPublishDir=\"{outputDir}\" /O\"{p.RutaPublicacion}\" \"{issScript}\"";
                    RunProcess(iss, iArgs, p.RutaProyecto);
                }
                
                SetProgress(100);
            });

            Log("Flujo completo terminado.");
            DetenerCronometro();
            
            // Volver a estado fantasma de Utilidades
            MenuUtilidades.IsEnabled = false;
        }

        private void BtnConfig_Click(object sender, RoutedEventArgs e)
        {
            var cfg = new ConfigWindow();
            cfg.Owner = this;
            if (cfg.ShowDialog() == true)
            {
                CargarConfig();
            }
        }
    }

    public class LogEntry
    {
        public string Hora { get; set; } = "";
        public string Mensaje { get; set; } = "";
    }
}
