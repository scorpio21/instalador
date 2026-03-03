using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace Instalador.Models
{
    public class ProyectoConfig : INotifyPropertyChanged
    {
        private string _nombre = "";
        private string _rutaProyecto = "";
        private string _rutaPublicacion = "";
        private string _versionInstalador = "1.1.0";
        private bool _readyToRun = true;
        private bool _trimmed = false;
        private bool _compressed = true;

        public string Nombre { get => _nombre; set => SetField(ref _nombre, value); }
        public string RutaProyecto { get => _rutaProyecto; set => SetField(ref _rutaProyecto, value); }
        public string RutaPublicacion { get => _rutaPublicacion; set => SetField(ref _rutaPublicacion, value); }
        public string VersionInstalador { get => _versionInstalador; set => SetField(ref _versionInstalador, value); }
        public bool ReadyToRun { get => _readyToRun; set => SetField(ref _readyToRun, value); }
        public bool Trimmed { get => _trimmed; set => SetField(ref _trimmed, value); }
        public bool Compressed { get => _compressed; set => SetField(ref _compressed, value); }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? name = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(name!);
            return true;
        }
    }

    public class Config
    {
        public List<ProyectoConfig> Proyectos { get; set; } = new List<ProyectoConfig>();
        public string UltimoProyectoSeleccionado { get; set; } = "";
        public string RutaInnoSetup { get; set; } = "";

        // Propiedades de compatibilidad (para migración de versiones anteriores)
        public string? RutaProyecto { get; set; }
        public string? RutaPublicacion { get; set; }
        public string? VersionInstalador { get; set; }
        public string? NombreProyecto { get; set; }

        public static string ArchivoConfig => "config.json";

        private static string GetRutaConfigAbsoluta()
        {
            string baseDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string carpetaApp = Path.Combine(baseDir, "Instalador");
            return Path.Combine(carpetaApp, ArchivoConfig);
        }

        private static string GetRutaConfigLegacy()
        {
            string currentDir = AppContext.BaseDirectory;
            return Path.Combine(currentDir, ArchivoConfig);
        }

        private static string? BuscarConfigLocalSubiendoDirectorios()
        {
            try
            {
                var dir = new DirectoryInfo(AppContext.BaseDirectory);
                for (int i = 0; i < 6 && dir != null; i++)
                {
                    var candidate = Path.Combine(dir.FullName, ArchivoConfig);
                    if (File.Exists(candidate))
                    {
                        return candidate;
                    }

                    dir = dir.Parent;
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        private static bool EsArchivoMasNuevo(string origen, string destino)
        {
            try
            {
                if (!File.Exists(origen)) return false;
                if (!File.Exists(destino)) return true;

                var fOrigen = File.GetLastWriteTimeUtc(origen);
                var fDestino = File.GetLastWriteTimeUtc(destino);
                return fOrigen > fDestino;
            }
            catch
            {
                return false;
            }
        }

        private static void AsegurarCarpetaConfigExiste(string fullPath)
        {
            string? dir = Path.GetDirectoryName(fullPath);
            if (string.IsNullOrWhiteSpace(dir)) return;
            Directory.CreateDirectory(dir);
        }

        private static void MigrarConfigLegacySiExiste(string destino)
        {
            try
            {
                // Primero intentar config junto al exe; si no existe, buscar subiendo carpetas (p.ej. raíz del repo)
                string? legacy = BuscarConfigLocalSubiendoDirectorios();
                if (string.IsNullOrWhiteSpace(legacy) || !File.Exists(legacy)) return;

                // Si el legacy es más nuevo, sustituir el de AppData.
                if (EsArchivoMasNuevo(legacy, destino))
                {
                    AsegurarCarpetaConfigExiste(destino);
                    File.Copy(legacy, destino, overwrite: true);
                }
            }
            catch
            {
                // Ignorar migración si falla por permisos o archivos bloqueados
            }
        }

        public static Config Cargar()
        {
            try
            {
                string fullPath = GetRutaConfigAbsoluta();
                MigrarConfigLegacySiExiste(fullPath);
                Console.WriteLine($"[CONFIG] Cargando desde: {fullPath}");

                if (!File.Exists(fullPath))
                {
                    Console.WriteLine("[CONFIG] Archivo no existe, creando nuevo.");
                    return new Config();
                }

                string json = File.ReadAllText(fullPath);
                var config = JsonSerializer.Deserialize<Config>(json) ?? new Config();
                
                Console.WriteLine($"[CONFIG] Proyectos cargados: {config.Proyectos.Count}");
                foreach(var p in config.Proyectos) Console.WriteLine($" - {p.Nombre}");

                // Lógica de migración si venimos de v1.0.3 o inferior
                if (config.Proyectos.Count == 0 && !string.IsNullOrEmpty(config.NombreProyecto))
                {
                    var p = new ProyectoConfig
                    {
                        Nombre = config.NombreProyecto,
                        RutaProyecto = config.RutaProyecto ?? "",
                        RutaPublicacion = config.RutaPublicacion ?? "",
                        VersionInstalador = config.VersionInstalador ?? "1.0"
                    };
                    config.Proyectos.Add(p);
                    config.UltimoProyectoSeleccionado = p.Nombre;
                    
                    // Limpiar propiedades antiguas
                    config.NombreProyecto = null;
                    config.RutaProyecto = null;
                    config.RutaPublicacion = null;
                    config.VersionInstalador = null;
                    
                    config.Guardar(); // Guardar el nuevo formato
                }

                return config;
            }
            catch
            {
                return new Config();
            }
        }

        public void Guardar()
        {
            string json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
            string fullPath = GetRutaConfigAbsoluta();
            AsegurarCarpetaConfigExiste(fullPath);
            File.WriteAllText(fullPath, json);
            Console.WriteLine($"[CONFIG] Guardado exitoso. Total proyectos: {Proyectos.Count}");
        }

        public ProyectoConfig? GetProyectoActual()
        {
            if (string.IsNullOrEmpty(UltimoProyectoSeleccionado))
                return Proyectos.FirstOrDefault();
            
            return Proyectos.FirstOrDefault(p => p.Nombre == UltimoProyectoSeleccionado) ?? Proyectos.FirstOrDefault();
        }
    }
}
