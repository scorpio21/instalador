using System;
using System.IO;
using Instalador.Models;

namespace Instalador.Services
{
    public interface IInnoSetupService
    {
        void GenerarScript(ProyectoConfig p, string setupFile);
    }

    public class InnoSetupService : IInnoSetupService
    {
        public void GenerarScript(ProyectoConfig p, string setupFile)
        {
            string contenido = $@"
[Setup]
AppName={p.Nombre}
AppVersion={p.VersionInstalador}
DefaultDirName={{autopf}}\{p.Nombre}
DefaultGroupName={p.Nombre}
OutputDir={p.RutaPublicacion}
OutputBaseFilename=Instalador_{p.Nombre}_v{p.VersionInstalador}
Compression=lzma
SolidCompression=yes

[Files]
Source: ""{p.RutaPublicacion}\*"" ; DestDir: ""{{app}}""; Flags: recursesubdirs createallsubdirs

[Icons]
Name: ""{{group}}\{p.Nombre}""; Filename: ""{{app}}\{p.Nombre}.exe""
";
            File.WriteAllText(setupFile, contenido);
        }
    }
}
