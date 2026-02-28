using System.IO;
using Instalador.Models;

namespace Instalador.Services
{
    public class ConfigService : IConfigService
    {
        public Config CargarConfig() => Config.Cargar();
        public void GuardarConfig(Config config) => config.Guardar();
        public ProyectoConfig? GetProyectoActual(Config config) => config.GetProyectoActual();

        public string DetectarInnoSetup()
        {
            string[] commonPaths = {
                @"C:\Program Files (x86)\Inno Setup 6\ISCC.exe",
                @"C:\Program Files\Inno Setup 6\ISCC.exe",
                @"C:\Program Files (x86)\Inno Setup 5\ISCC.exe"
            };

            foreach (var path in commonPaths)
            {
                if (File.Exists(path)) return path;
            }

            return "";
        }
    }
}
