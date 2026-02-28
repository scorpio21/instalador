using Instalador.Models;

namespace Instalador.Services
{
    public interface IConfigService
    {
        Config CargarConfig();
        void GuardarConfig(Config config);
        ProyectoConfig? GetProyectoActual(Config config);
        string DetectarInnoSetup();
    }
}
