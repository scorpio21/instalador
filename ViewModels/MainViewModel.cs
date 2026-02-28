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
            ZipCommand = new RelayCommand(async _ => await EjecutarZip());
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
        public ICommand ZipCommand { get; }
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

        private async Task EjecutarZip()
        {
            if (ProyectoSeleccionado == null) return;
            AddLog("Generando ZIP (Simulado para MVVM proof)...");
            await Task.Delay(1000);
            AddLog("ZIP Generado con éxito.");
        }

        private async Task EjecutarInstaller()
        {
            if (ProyectoSeleccionado == null) return;
            AddLog("Generando instalador...");
            _innoService.GenerarScript(ProyectoSeleccionado, Path.Combine(ProyectoSeleccionado.RutaProyecto, "installer.iss"));
            AddLog("Script .iss generado. Inicie ISCC.exe para finalizar.");
            _notificationService.Notify("Instalador Generado", $"El script para {ProyectoSeleccionado.Nombre} está listo.");
            await Task.CompletedTask;
        }

        private async Task EjecutarTodo()
        {
            await EjecutarLimpiar();
            await EjecutarBuild();
            await EjecutarPublish();
            await EjecutarInstaller();
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
