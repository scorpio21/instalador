using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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

        public string AppVersionDisplay => $"v{Instalador.Views.MainWindow.AppVersion} PRO";

        public ICommand LimpiarCommand { get; }
        public ICommand ActualizarGitCommand { get; }
        public ICommand BuildCommand { get; }
        public ICommand PublishCommand { get; }
        public ICommand ZipPortableCommand { get; }
        public ICommand ZipSingleFileCommand { get; }
        public ICommand InstallerCommand { get; }
        public ICommand RunAllCommand { get; }

        private async Task ActualizarEstadoGit()
        {
            if (ProyectoSeleccionado == null) return;
            var (branch, hasChanges) = await _gitService.GetStatusAsync(ProyectoSeleccionado.RutaProyecto);
            GitBranch = $"Rama: {branch}";
            HasGitChanges = hasChanges;
        }

        private async Task EjecutarLimpiar()
        {
            if (ProyectoSeleccionado == null) return;
            AddLog($"Iniciando limpieza de {ProyectoSeleccionado.Nombre}...");
            if (Directory.Exists(ProyectoSeleccionado.RutaPublicacion))
            {
                try { Directory.Delete(ProyectoSeleccionado.RutaPublicacion, true); AddLog("Carpeta de publicación eliminada."); }
                catch (Exception ex) { AddLog("[WARN] No se pudo borrar carpeta: " + ex.Message); }
            }
            else AddLog("La carpeta ya estaba limpia.");
            IsProjectReady = true;
            await Task.CompletedTask;
        }

        private async Task EjecutarBuild()
        {
            if (ProyectoSeleccionado == null) return;
            AddLog("Compilando proyecto...");
            await _buildService.RunCommandAsync("dotnet", "build", ProyectoSeleccionado.RutaProyecto, AddLog);
        }

        private async Task EjecutarPublish()
        {
            if (ProyectoSeleccionado == null) return;
            AddLog("Publicando proyecto (flags avanzados)...");
            await _buildService.RunPublishAsync(ProyectoSeleccionado, AddLog);
        }

        private async Task CopiarRecursos()
        {
            if (ProyectoSeleccionado == null) return;
            AddLog("Copiando recursos (img, README)...");
            
            await Task.Run(() => 
            {
                string sourceProject = ProyectoSeleccionado.RutaProyecto;
                string destDir = System.IO.Path.Combine(ProyectoSeleccionado.RutaPublicacion, "win-x64-singlefile");
                
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
            if (ProyectoSeleccionado == null) return;
            AddLog("Generando ZIP Portable...");
            string sourceDir = System.IO.Path.Combine(ProyectoSeleccionado.RutaPublicacion, "win-x64-singlefile");
            string zipPortable = System.IO.Path.Combine(ProyectoSeleccionado.RutaPublicacion, $"{ProyectoSeleccionado.Nombre}_{ProyectoSeleccionado.VersionInstalador}_Portable.zip");
            await ComprimirCarpetaAsync(sourceDir, zipPortable);
        }

        private async Task EjecutarZipSingleFile()
        {
            if (ProyectoSeleccionado == null) return;
            AddLog("Generando ZIP Single-File...");
            string sourceDir = System.IO.Path.Combine(ProyectoSeleccionado.RutaPublicacion, "win-x64-singlefile");
            string zipWinX64 = System.IO.Path.Combine(ProyectoSeleccionado.RutaPublicacion, $"{ProyectoSeleccionado.Nombre}_{ProyectoSeleccionado.VersionInstalador}_win-x64_singlefile.zip");
            await ComprimirCarpetaAsync(sourceDir, zipWinX64);
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
            if (ProyectoSeleccionado == null) return;
            AddLog("Preparando instalador de Inno Setup...");
            string issPath = Path.Combine(ProyectoSeleccionado.RutaProyecto, "installer.iss");
            _innoService.GenerarScript(ProyectoSeleccionado, issPath);
            
            if (!string.IsNullOrEmpty(_config.RutaInnoSetup) && File.Exists(_config.RutaInnoSetup))
            {
                string publishDirStr = Path.Combine(ProyectoSeleccionado.RutaPublicacion, "win-x64-singlefile");
                
                // La salida del Setup(.exe) generada por Inno irá a RutaPublicacion (publish/)
                string args = $"/O\"{ProyectoSeleccionado.RutaPublicacion}\" /dMyAppName=\"{ProyectoSeleccionado.Nombre}\" /dMyAppVersion=\"{ProyectoSeleccionado.VersionInstalador}\" /dMyAppExeName=\"{ProyectoSeleccionado.Nombre}.exe\" /dPublishDir=\"{publishDirStr}\" \"{issPath}\"";
                
                AddLog("Compilando con ISCC.exe...");
                bool ok = await _buildService.RunCommandAsync(_config.RutaInnoSetup, args, ProyectoSeleccionado.RutaProyecto, AddLog);
                
                if (ok) AddLog("Instalador .exe generado correctamente en carpeta publish.");
                else AddLog("[ERROR] Fallo al compilar el instalador con Inno Setup.");
            }
            else
            {
                 AddLog("[ERROR] Ruta a Inno Setup (ISCC.exe) no configurada. Ve a Ajustes y dale a Auto-Detectar.");
            }
        }

        private async Task EjecutarTodo()
        {
            await EjecutarLimpiar();
            await EjecutarBuild();
            await EjecutarPublish();
            await CopiarRecursos();
            await EjecutarInstaller();
            await EjecutarZipPortable();
            await EjecutarZipSingleFile();
            _notificationService.Notify("Proceso Completado", "Todas las tareas han finalizado con éxito.");
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
