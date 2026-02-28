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
            _config.Proyectos.Clear();
            foreach (var p in Proyectos) _config.Proyectos.Add(p);
            _configService.GuardarConfig(_config);
        }

        private void AddProyecto()
        {
            Proyectos.Add(new ProyectoConfig { Nombre = "Nuevo Proyecto" });
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
    }
}
