using System.Collections.ObjectModel;
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
            set => SetProperty(ref _proyectoSeleccionado, value);
        }

        public string RutaInnoSetup
        {
            get => _config.RutaInnoSetup;
            set { _config.RutaInnoSetup = value; OnPropertyChanged(); }
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
