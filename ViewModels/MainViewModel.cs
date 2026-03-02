using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Windows.Input;
using Instalador.Helpers;
using Instalador.Models;
using Instalador.Services;

namespace Instalador.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly IConfigService _configService;
        private readonly IGitService _gitService;
        private readonly IBuildService _buildService;
        private readonly IInnoSetupService _innoService;
        private readonly INotificationService _notificationService;

        private Config _config;
        private ProyectoConfig? _proyectoSeleccionado;
        private string _gitBranch = "Git: Desconocido";
        private bool _hasGitChanges;
        private bool _isProjectReady;
        private double _progresoTotal;
        private string _tiempoTranscurrido = "00:00";
        private bool _estaProcesando;
        private string _buildConfiguration = "Release";
        private readonly Stopwatch _cronometro = new Stopwatch();
        private readonly DispatcherTimer _timerTiempo;

        public MainViewModel(IConfigService configService, IGitService gitService, IBuildService buildService, IInnoSetupService innoService, INotificationService notificationService)
        {
            _configService = configService;
            _gitService = gitService;
            _buildService = buildService;
            _innoService = innoService;
            _notificationService = notificationService;

            _config = _configService.CargarConfig();
            _proyectoSeleccionado = _configService.GetProyectoActual(_config);

            LogEntries = new ObservableCollection<LogEntry>();
            Proyectos = new ObservableCollection<ProyectoConfig>(_config.Proyectos);

            LimpiarCommand = new RelayCommand(async _ => await EjecutarLimpiar());
            ActualizarGitCommand = new RelayCommand(async _ => await ActualizarEstadoGit());
            BuildCommand = new RelayCommand(async _ => await EjecutarBuild());
            PublishCommand = new RelayCommand(async _ => await EjecutarPublish());
            ZipPortableCommand = new RelayCommand(async _ => await EjecutarZipPortable());
            ZipSingleFileCommand = new RelayCommand(async _ => await EjecutarZipSingleFile());
            InstallerCommand = new RelayCommand(async _ => await EjecutarInstaller());
            RunAllCommand = new RelayCommand(async _ => await EjecutarTodo());
            
            _timerTiempo = new DispatcherTimer { Interval = System.TimeSpan.FromSeconds(1) };
            _timerTiempo.Tick += (s, e) => TiempoTranscurrido = _cronometro.Elapsed.ToString(@"mm\:ss");

            _ = ActualizarEstadoGit();
        }

        public ObservableCollection<ProyectoConfig> Proyectos { get; }
        public ObservableCollection<LogEntry> LogEntries { get; }

        public ProyectoConfig? ProyectoSeleccionado
        {
            get => _proyectoSeleccionado;
            set
            {
                if (SetProperty(ref _proyectoSeleccionado, value))
                {
                    if (value != null)
                    {
                        _config.UltimoProyectoSeleccionado = value.Nombre;
                        _configService.GuardarConfig(_config);
                        _ = ActualizarEstadoGit();
                    }
                }
            }
        }

        public string GitBranch
        {
            get => _gitBranch;
            set => SetProperty(ref _gitBranch, value);
        }

        public bool HasGitChanges
        {
            get => _hasGitChanges;
            set => SetProperty(ref _hasGitChanges, value);
        }

        public bool IsProjectReady
        {
            get => _isProjectReady;
            set => SetProperty(ref _isProjectReady, value);
        }

        public double ProgresoTotal
        {
            get => _progresoTotal;
            set
            {
                if (SetProperty(ref _progresoTotal, value))
                {
                    OnPropertyChanged(nameof(ProgresoIndeterminado));
                }
            }
        }

        public string TiempoTranscurrido
        {
            get => _tiempoTranscurrido;
            set
            {
                if (SetProperty(ref _tiempoTranscurrido, value))
                {
                    OnPropertyChanged(nameof(TiempoDisplay));
                }
            }
        }

        public bool EstaProcesando
        {
            get => _estaProcesando;
            set
            {
                if (SetProperty(ref _estaProcesando, value))
                {
                    OnPropertyChanged(nameof(TiempoDisplay));
                    OnPropertyChanged(nameof(ProgresoIndeterminado));
                }
            }
        }

        public bool ProgresoIndeterminado => EstaProcesando && ProgresoTotal <= 0;

        public string TiempoDisplay => $"{TiempoTranscurrido} {(EstaProcesando ? "Procesando" : "Terminado")}";

        public string BuildConfiguration
        {
            get => _buildConfiguration;
            set => SetProperty(ref _buildConfiguration, value);
        }

        public string AppVersionDisplay => $"v{Instalador.Views.MainWindow.AppVersion} PRO";

        public ICommand LimpiarCommand { get; }
        public ICommand ActualizarGitCommand { get; }
        public ICommand BuildCommand { get; }
        public ICommand PublishCommand { get; }
        public ICommand ZipPortableCommand { get; }
        public ICommand ZipSingleFileCommand { get; }
        public ICommand InstallerCommand { get; }
        public ICommand RunAllCommand { get; }

        private bool IniciarCronometroSiNecesario()
        {
            if (_cronometro.IsRunning) return false;

            ProgresoTotal = 0;
            TiempoTranscurrido = "00:00";
            EstaProcesando = true;
            _cronometro.Restart();
            _timerTiempo.Start();
            return true;
        }

        private void DetenerCronometroSiCorresponde(bool iniciadoPorEstaAccion)
        {
            if (!iniciadoPorEstaAccion) return;

            _timerTiempo.Stop();
            _cronometro.Stop();
            TiempoTranscurrido = _cronometro.Elapsed.ToString(@"mm\:ss");
            ProgresoTotal = 100;
            EstaProcesando = false;
        }

        private async Task ActualizarEstadoGit()
        {
            if (ProyectoSeleccionado == null) return;
            var (branch, hasChanges) = await _gitService.GetStatusAsync(ProyectoSeleccionado.RutaProyecto);
            GitBranch = $"Rama: {branch}";
            HasGitChanges = hasChanges;
        }

        private async Task EjecutarLimpiar()
        {
            bool iniciar = IniciarCronometroSiNecesario();
            if (ProyectoSeleccionado == null) return;
            string publishDir = GetAbsolutePublishDir();
            AddLog($"Iniciando limpieza de {ProyectoSeleccionado.Nombre}...");
            AddLog($"- [DEBUG] Ruta evaluada a limpiar: {publishDir}");
            
            if (Directory.Exists(publishDir))
            {
                try 
                { 
                    Directory.Delete(publishDir, true); 
                    AddLog("Carpeta de publicación eliminada completa."); 
                }
                catch (Exception ex) 
                { 
                    AddLog("[WARN] No se pudo borrar carpeta: " + ex.Message); 
                }
            }
            else 
            {
                AddLog("La carpeta ya estaba limpia (no se detectó directorio).");
            }
            IsProjectReady = true;
            DetenerCronometroSiCorresponde(iniciar);
            await Task.CompletedTask;
        }

        private async Task EjecutarBuild()
        {
            bool iniciar = IniciarCronometroSiNecesario();
            if (ProyectoSeleccionado == null) return;
            AddLog("Compilando proyecto...");
            await _buildService.RunCommandAsync("dotnet", "build", ProyectoSeleccionado.RutaProyecto, AddLog);
            DetenerCronometroSiCorresponde(iniciar);
        }

        private async Task EjecutarPublish()
        {
            bool iniciar = IniciarCronometroSiNecesario();
            if (ProyectoSeleccionado == null) return;
            AddLog($"Publicando proyecto ({BuildConfiguration})...");
            await _buildService.RunPublishAsync(ProyectoSeleccionado, AddLog, BuildConfiguration);
            await CopiarRecursos();
            DetenerCronometroSiCorresponde(iniciar);
        }

        private async Task CopiarRecursos()
        {
            if (ProyectoSeleccionado == null) return;
            AddLog("Copiando recursos (img, README)...");
            
            await Task.Run(() => 
            {
                string sourceProject = ProyectoSeleccionado.RutaProyecto;
                string destDir = System.IO.Path.Combine(GetAbsolutePublishDir(), "win-x64-singlefile");
                
                try 
                {
                    // Copiar carpeta img/
                    string sourceImg = Path.Combine(sourceProject, "img");
                    if (Directory.Exists(sourceImg))
                    {
                        string destImg = Path.Combine(destDir, "img");
                        CopyDirectory(sourceImg, destImg);
                    }
                    
                    // Copiar README.md
                    string sourceReadme = Path.Combine(sourceProject, "README.md");
                    if (File.Exists(sourceReadme))
                    {
                        File.Copy(sourceReadme, Path.Combine(destDir, "README.md"), true);
                    }
                } 
                catch (Exception ex)
                {
                    AddLog("[WARN] Copiando recursos: " + ex.Message);
                }
            });
        }

        private void CopyDirectory(string sourceDir, string destinationDir)
        {
            var dir = new DirectoryInfo(sourceDir);
            if (!dir.Exists) throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");

            DirectoryInfo[] dirs = dir.GetDirectories();
            Directory.CreateDirectory(destinationDir);

            foreach (FileInfo file in dir.GetFiles())
            {
                string targetFilePath = Path.Combine(destinationDir, file.Name);
                file.CopyTo(targetFilePath, true);
            }

            foreach (DirectoryInfo subDir in dirs)
            {
                string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                CopyDirectory(subDir.FullName, newDestinationDir);
            }
        }

        private async Task EjecutarZipPortable()
        {
            bool iniciar = IniciarCronometroSiNecesario();
            if (ProyectoSeleccionado == null) return;
            AddLog("Generando ZIP Portable...");
            await CopiarRecursos();
            string absolutePublish = GetAbsolutePublishDir();
            string sourceDir = System.IO.Path.Combine(absolutePublish, "win-x64-singlefile");
            string zipPortable = System.IO.Path.Combine(absolutePublish, $"{ProyectoSeleccionado.Nombre}_{ProyectoSeleccionado.VersionInstalador}_Portable.zip");
            await ComprimirCarpetaAsync(sourceDir, zipPortable);
            DetenerCronometroSiCorresponde(iniciar);
        }

        private async Task EjecutarZipSingleFile()
        {
            bool iniciar = IniciarCronometroSiNecesario();
            if (ProyectoSeleccionado == null) return;
            AddLog("Generando ZIP Single-File...");
            await CopiarRecursos();
            string absolutePublish = GetAbsolutePublishDir();
            string sourceDir = System.IO.Path.Combine(absolutePublish, "win-x64-singlefile");
            string zipWinX64 = System.IO.Path.Combine(absolutePublish, $"{ProyectoSeleccionado.Nombre}_{ProyectoSeleccionado.VersionInstalador}_win-x64_singlefile.zip");
            await ComprimirCarpetaAsync(sourceDir, zipWinX64);
            DetenerCronometroSiCorresponde(iniciar);
        }

        private string DetectarExePublicado(string publishDir)
        {
            try
            {
                if (!Directory.Exists(publishDir)) return "";

                var exeFiles = Directory.GetFiles(publishDir, "*.exe", SearchOption.TopDirectoryOnly);
                if (exeFiles.Length == 0) return "";

                if (ProyectoSeleccionado != null)
                {
                    string esperado = Path.Combine(publishDir, $"{ProyectoSeleccionado.Nombre}.exe");
                    if (File.Exists(esperado)) return Path.GetFileName(esperado);
                }

                if (exeFiles.Length == 1) return Path.GetFileName(exeFiles[0]);

                var candidato = exeFiles.FirstOrDefault(f =>
                    !string.Equals(Path.GetFileName(f), "vshost.exe", System.StringComparison.OrdinalIgnoreCase) &&
                    !Path.GetFileName(f).Contains("WebView2", System.StringComparison.OrdinalIgnoreCase));

                return candidato != null ? Path.GetFileName(candidato) : Path.GetFileName(exeFiles[0]);
            }
            catch
            {
                return "";
            }
        }

        private async Task ComprimirCarpetaAsync(string sourceDir, string destZip)
        {
            await Task.Run(() => 
            {
                try 
                {
                    if (File.Exists(destZip)) File.Delete(destZip);
                    if (Directory.Exists(sourceDir))
                    {
                        System.IO.Compression.ZipFile.CreateFromDirectory(sourceDir, destZip);
                        AddLog($"ZIP generado: {System.IO.Path.GetFileName(destZip)}");
                    }
                    else 
                    {
                        AddLog($"[ERROR] Carpeta {sourceDir} no encontrada para zipear.");
                    }
                } 
                catch(Exception e) 
                {
                    AddLog("[ERROR ZIP] " + e.Message);
                }
            });
        }

        private async Task EjecutarInstaller()
        {
            bool iniciar = IniciarCronometroSiNecesario();
            if (ProyectoSeleccionado == null) return;
            AddLog("Preparando instalador de Inno Setup...");
            string issPath = Path.Combine(ProyectoSeleccionado.RutaProyecto, "installer.iss");
            _innoService.GenerarScript(ProyectoSeleccionado, issPath);
            
            if (!string.IsNullOrEmpty(_config.RutaInnoSetup) && File.Exists(_config.RutaInnoSetup))
            {
                string absolutePublish = GetAbsolutePublishDir();
                string publishDirStr = Path.Combine(absolutePublish, "win-x64-singlefile");

                await CopiarRecursos();

                string exePublicado = DetectarExePublicado(publishDirStr);
                if (string.IsNullOrWhiteSpace(exePublicado))
                {
                    AddLog($"[ERROR] No se detectó ningún .exe en: {publishDirStr}. Ejecuta 'Publicar single-file' primero.");
                    return;
                }
                
                // La salida del Setup(.exe) generada por Inno irá a RutaPublicacion (publish/)
                string args = $"/O\"{absolutePublish}\" /dMyAppName=\"{ProyectoSeleccionado.Nombre}\" /dMyAppVersion=\"{ProyectoSeleccionado.VersionInstalador}\" /dMyAppExeName=\"{exePublicado}\" /dPublishDir=\"{publishDirStr}\" \"{issPath}\"";
                
                AddLog("Compilando con ISCC.exe...");
                bool ok = await _buildService.RunCommandAsync(_config.RutaInnoSetup, args, ProyectoSeleccionado.RutaProyecto, AddLog);
                
                if (ok) AddLog("Instalador .exe generado correctamente en carpeta publish.");
                else AddLog("[ERROR] Fallo al compilar el instalador con Inno Setup.");
            }
            else
            {
                 AddLog("[ERROR] Ruta a Inno Setup (ISCC.exe) no configurada. Ve a Ajustes y dale a Auto-Detectar.");
            }

            DetenerCronometroSiCorresponde(iniciar);
        }

        private async Task EjecutarTodo()
        {
            ProgresoTotal = 0;
            TiempoTranscurrido = "00:00";
            EstaProcesando = true;
            _cronometro.Restart();
            _timerTiempo.Start();

            try
            {
                await EjecutarLimpiar();
                ProgresoTotal = 10;

                await EjecutarBuild();
                ProgresoTotal = 25;

                await EjecutarPublish();
                ProgresoTotal = 55;

                await EjecutarInstaller();
                ProgresoTotal = 80;

                await EjecutarZipPortable();
                ProgresoTotal = 90;

                await EjecutarZipSingleFile();
                ProgresoTotal = 100;

                _notificationService.Notify("Proceso Completado", "Todas las tareas han finalizado con éxito.");
            }
            finally
            {
                _timerTiempo.Stop();
                _cronometro.Stop();
                TiempoTranscurrido = _cronometro.Elapsed.ToString(@"mm\:ss");
                EstaProcesando = false;
            }
        }

        private string GetAbsolutePublishDir()
        {
            if (ProyectoSeleccionado == null) return "";
            if (string.IsNullOrWhiteSpace(ProyectoSeleccionado.RutaPublicacion)) return ProyectoSeleccionado.RutaProyecto;
            return Path.IsPathRooted(ProyectoSeleccionado.RutaPublicacion) 
                ? ProyectoSeleccionado.RutaPublicacion 
                : Path.Combine(ProyectoSeleccionado.RutaProyecto, ProyectoSeleccionado.RutaPublicacion);
        }

        public void AddLog(string mensaje)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                LogEntries.Insert(0, new LogEntry { Hora = DateTime.Now.ToString("HH:mm:ss"), Mensaje = mensaje });
            });
        }

        public void ActualizarConfig(Config config)
        {
            _config = config;
            // No reseteamos Proyectos aquí porque ya se hace en MainWindow.xaml.cs
            Console.WriteLine($"[MainVM] Configuración sincronizada. Proyectos: {_config.Proyectos.Count}");
        }
    }

    public class LogEntry
    {
        public string Hora { get; set; } = "";
        public string Mensaje { get; set; } = "";
    }
}
