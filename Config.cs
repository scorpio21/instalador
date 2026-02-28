using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Instalador
{
    public class ProyectoConfig
    {
        public string Nombre { get; set; } = "";
        public string RutaProyecto { get; set; } = "";
        public string RutaPublicacion { get; set; } = "";
        public string VersionInstalador { get; set; } = "1.0";
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

        public static Config Cargar()
        {
            try
            {
                if (!File.Exists(ArchivoConfig))
                    return new Config();

                string json = File.ReadAllText(ArchivoConfig);
                var config = JsonSerializer.Deserialize<Config>(json) ?? new Config();

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
            File.WriteAllText(ArchivoConfig, json);
        }

        public ProyectoConfig? GetProyectoActual()
        {
            if (string.IsNullOrEmpty(UltimoProyectoSeleccionado))
                return Proyectos.FirstOrDefault();
            
            return Proyectos.FirstOrDefault(p => p.Nombre == UltimoProyectoSeleccionado) ?? Proyectos.FirstOrDefault();
        }
    }
}
