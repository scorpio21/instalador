using System.IO;
using System.Text.Json;

namespace Instalador
{
    public class Config
    {
        public string RutaProyecto { get; set; } = "";
        public string RutaPublicacion { get; set; } = "";
        public string RutaInnoSetup { get; set; } = "";
        public string VersionInstalador { get; set; } = "1.0";
        public string NombreProyecto { get; set; } = "AOUpdate";

        public static string ArchivoConfig => "config.json";

        public static Config Cargar()
        {
            try
            {
                if (!File.Exists(ArchivoConfig))
                    return new Config();

                string json = File.ReadAllText(ArchivoConfig);
                return JsonSerializer.Deserialize<Config>(json) ?? new Config();
            }
            catch
            {
                // Si el JSON está mal, devolvemos una config vacía para no romper el inicio
                return new Config();
            }
        }

        public void Guardar()
        {
            string json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(ArchivoConfig, json);
        }
    }
}
