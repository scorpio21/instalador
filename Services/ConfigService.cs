using Instalador.Models;

namespace Instalador.Services
{
    public class ConfigService : IConfigService
    {
        public Config CargarConfig() => Config.Cargar();
        public void GuardarConfig(Config config) => config.Guardar();
        public ProyectoConfig? GetProyectoActual(Config config) => config.GetProyectoActual();
    }
}
