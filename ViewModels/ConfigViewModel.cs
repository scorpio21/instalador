using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Instalador.Helpers;
using Instalador.Models;
using Instalador.Services;

namespace Instalador.ViewModels
{
    public class ConfigViewModel : ViewModelBase
    {
        private readonly IConfigService _configService;
        private Config _config;
        private ProyectoConfig? _proyectoSeleccionado;

        private string _estadoRutaProyecto = "";
        private string _mensajeRutaProyecto = "";
        private string _estadoRutaPublicacion = "";
        private string _mensajeRutaPublicacion = "";
        private string _estadoRutaInnoSetup = "";
        private string _mensajeRutaInnoSetup = "";

        private CancellationTokenSource? _ctsValidacionProyecto;
        private CancellationTokenSource? _ctsValidacionPublicacion;
        private CancellationTokenSource? _ctsValidacionInno;

        public ConfigViewModel(IConfigService configService)
        {
            _configService = configService;
            _config = _configService.CargarConfig();
            Proyectos = new ObservableCollection<ProyectoConfig>(_config.Proyectos);
            
            Console.WriteLine($"[ConfigVM] Inicializado con {Proyectos.Count} proyectos.");
            
            GuardarCommand = new RelayCommand(_ => Guardar());
            AddCommand = new RelayCommand(_ => AddProyecto());
            DeleteCommand = new RelayCommand(_ => DeleteProyecto());
            DetectCommand = new RelayCommand(_ => DetectarInno());

            if (string.IsNullOrEmpty(RutaInnoSetup)) DetectarInno();
        }

        public ObservableCollection<ProyectoConfig> Proyectos { get; }

        public ProyectoConfig? ProyectoSeleccionado
        {
            get => _proyectoSeleccionado;
            set
            {
                if (_proyectoSeleccionado != null)
                {
                    _proyectoSeleccionado.PropertyChanged -= ProyectoSeleccionado_PropertyChanged;
                }

                if (SetProperty(ref _proyectoSeleccionado, value))
                {
                    if (_proyectoSeleccionado != null)
                    {
                        _proyectoSeleccionado.PropertyChanged += ProyectoSeleccionado_PropertyChanged;
                    }

                    _ = ValidarTodoAsync();
                }
            }
        }

        public string RutaInnoSetup
        {
            get => _config.RutaInnoSetup;
            set
            {
                if (_config.RutaInnoSetup != value)
                {
                    _config.RutaInnoSetup = value;
                    OnPropertyChanged();
                    _ = ValidarRutaInnoSetupAsync(value);
                }
            }
        }

        public string EstadoRutaProyecto
        {
            get => _estadoRutaProyecto;
            private set => SetProperty(ref _estadoRutaProyecto, value);
        }

        public string MensajeRutaProyecto
        {
            get => _mensajeRutaProyecto;
            private set => SetProperty(ref _mensajeRutaProyecto, value);
        }

        public string EstadoRutaPublicacion
        {
            get => _estadoRutaPublicacion;
            private set => SetProperty(ref _estadoRutaPublicacion, value);
        }

        public string MensajeRutaPublicacion
        {
            get => _mensajeRutaPublicacion;
            private set => SetProperty(ref _mensajeRutaPublicacion, value);
        }

        public string EstadoRutaInnoSetup
        {
            get => _estadoRutaInnoSetup;
            private set => SetProperty(ref _estadoRutaInnoSetup, value);
        }

        public string MensajeRutaInnoSetup
        {
            get => _mensajeRutaInnoSetup;
            private set => SetProperty(ref _mensajeRutaInnoSetup, value);
        }

        public ICommand GuardarCommand { get; }
        public ICommand AddCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand DetectCommand { get; }

        private void Guardar()
        {
            Console.WriteLine($"[ConfigVM] Guardando {Proyectos.Count} proyectos...");
            _config.Proyectos.Clear();
            foreach (var p in Proyectos) _config.Proyectos.Add(p);
            _configService.GuardarConfig(_config);
        }

        private void AddProyecto()
        {
            Proyectos.Add(new ProyectoConfig { Nombre = "Nuevo Proyecto", VersionInstalador = "1.1.0" });
        }

        private void DeleteProyecto()
        {
            if (ProyectoSeleccionado != null) Proyectos.Remove(ProyectoSeleccionado);
        }

        private void DetectarInno()
        {
            string ruta = _configService.DetectarInnoSetup();
            if (!string.IsNullOrEmpty(ruta)) RutaInnoSetup = ruta;
        }

        private void ProyectoSeleccionado_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ProyectoConfig.RutaProyecto))
            {
                _ = ValidarRutaProyectoAsync(ProyectoSeleccionado?.RutaProyecto ?? "");
            }

            if (e.PropertyName == nameof(ProyectoConfig.RutaPublicacion))
            {
                _ = ValidarRutaPublicacionAsync(ProyectoSeleccionado?.RutaPublicacion ?? "");
            }
        }

        private Task ValidarTodoAsync()
        {
            var tareas = new Task[]
            {
                ValidarRutaProyectoAsync(ProyectoSeleccionado?.RutaProyecto ?? ""),
                ValidarRutaPublicacionAsync(ProyectoSeleccionado?.RutaPublicacion ?? ""),
                ValidarRutaInnoSetupAsync(RutaInnoSetup)
            };

            return Task.WhenAll(tareas);
        }

        private async Task ValidarRutaProyectoAsync(string ruta)
        {
            _ctsValidacionProyecto?.Cancel();
            _ctsValidacionProyecto = new CancellationTokenSource();
            var ct = _ctsValidacionProyecto.Token;

            if (string.IsNullOrWhiteSpace(ruta))
            {
                EstadoRutaProyecto = "";
                MensajeRutaProyecto = "";
                return;
            }

            try
            {
                var resultado = await Task.Run(() =>
                {
                    ct.ThrowIfCancellationRequested();

                    if (!Directory.Exists(ruta)) return (estado: "ERROR", msg: "La carpeta no existe.");

                    ct.ThrowIfCancellationRequested();

                    var csproj = Directory.GetFiles(ruta, "*.csproj", SearchOption.TopDirectoryOnly);
                    if (csproj.Length == 0) return (estado: "WARN", msg: "No se encontró ningún .csproj en la carpeta.");

                    return (estado: "OK", msg: "");
                }, ct);

                if (ct.IsCancellationRequested) return;

                EstadoRutaProyecto = resultado.estado;
                MensajeRutaProyecto = resultado.msg;
            }
            catch (OperationCanceledException)
            {
            }
        }

        private async Task ValidarRutaPublicacionAsync(string ruta)
        {
            _ctsValidacionPublicacion?.Cancel();
            _ctsValidacionPublicacion = new CancellationTokenSource();
            var ct = _ctsValidacionPublicacion.Token;

            if (string.IsNullOrWhiteSpace(ruta))
            {
                EstadoRutaPublicacion = "";
                MensajeRutaPublicacion = "";
                return;
            }

            try
            {
                var resultado = await Task.Run(() =>
                {
                    ct.ThrowIfCancellationRequested();
                    return Directory.Exists(ruta)
                        ? (estado: "OK", msg: "")
                        : (estado: "ERROR", msg: "La carpeta no existe.");
                }, ct);

                if (ct.IsCancellationRequested) return;

                EstadoRutaPublicacion = resultado.estado;
                MensajeRutaPublicacion = resultado.msg;
            }
            catch (OperationCanceledException)
            {
            }
        }

        private async Task ValidarRutaInnoSetupAsync(string ruta)
        {
            _ctsValidacionInno?.Cancel();
            _ctsValidacionInno = new CancellationTokenSource();
            var ct = _ctsValidacionInno.Token;

            if (string.IsNullOrWhiteSpace(ruta))
            {
                EstadoRutaInnoSetup = "";
                MensajeRutaInnoSetup = "";
                return;
            }

            try
            {
                var resultado = await Task.Run(() =>
                {
                    ct.ThrowIfCancellationRequested();
                    return File.Exists(ruta)
                        ? (estado: "OK", msg: "")
                        : (estado: "ERROR", msg: "Archivo no encontrado.");
                }, ct);

                if (ct.IsCancellationRequested) return;

                EstadoRutaInnoSetup = resultado.estado;
                MensajeRutaInnoSetup = resultado.msg;
            }
            catch (OperationCanceledException)
            {
            }
        }

        public void OnRutaProyectoChanged()
        {
            if (ProyectoSeleccionado == null || string.IsNullOrEmpty(ProyectoSeleccionado.RutaProyecto)) return;

            // Si el nombre es el de defecto, intentamos extraer el nombre real de la carpeta
            if (string.IsNullOrEmpty(ProyectoSeleccionado.Nombre) || ProyectoSeleccionado.Nombre == "Nuevo Proyecto")
            {
                try
                {
                    string folderName = System.IO.Path.GetFileName(ProyectoSeleccionado.RutaProyecto.TrimEnd(System.IO.Path.DirectorySeparatorChar));
                    if (!string.IsNullOrEmpty(folderName))
                    {
                        ProyectoSeleccionado.Nombre = folderName;
                    }
                }
                catch { }
            }

            // Intentar extraer versión del .csproj
            string versionDetectada = ExtraerVersionDeCsProj(ProyectoSeleccionado.RutaProyecto);
            if (!string.IsNullOrEmpty(versionDetectada))
            {
                ProyectoSeleccionado.VersionInstalador = versionDetectada;
            }
            else if (string.IsNullOrEmpty(ProyectoSeleccionado.VersionInstalador) || ProyectoSeleccionado.VersionInstalador == "1.0")
            {
                // Solo si no hay versión detectada y es la de defecto, ponemos 1.1.0
                ProyectoSeleccionado.VersionInstalador = "1.1.0";
            }

            // Auto-rellenar carpeta de publicación si está vacía
            if (string.IsNullOrEmpty(ProyectoSeleccionado.RutaPublicacion))
            {
                ProyectoSeleccionado.RutaPublicacion = System.IO.Path.Combine(ProyectoSeleccionado.RutaProyecto, "publish");
            }
        }

        private string ExtraerVersionDeCsProj(string rutaDirectorio)
        {
            try
            {
                if (!System.IO.Directory.Exists(rutaDirectorio)) return "";

                var archivos = System.IO.Directory.GetFiles(rutaDirectorio, "*.csproj");
                if (archivos.Length == 0) return "";

                // Leer el primer .csproj encontrado
                string contenido = System.IO.File.ReadAllText(archivos[0]);

                // Buscar etiquetas comunes de versión
                string[] tags = { "<Version>", "<AssemblyVersion>", "<FileVersion>", "<ApplicationVersion>" };
                foreach (var tag in tags)
                {
                    int start = contenido.IndexOf(tag);
                    if (start != -1)
                    {
                        start += tag.Length;
                        int end = contenido.IndexOf("</", start);
                        if (end != -1)
                        {
                            return contenido.Substring(start, end - start).Trim();
                        }
                    }
                }
            }
            catch { }
            return "";
        }
    }
}
