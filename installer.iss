; Script de Inno Setup Universal
; Se recomienda no modificar las rutas fijas aquí, ya que se pasan por parámetros desde el Generador

#ifndef MyAppName
  #define MyAppName "MiAplicacion"
#endif
#ifndef MyAppVersion
  #define MyAppVersion "1.0"
#endif
#ifndef MyAppPublisher
  #define MyAppPublisher "Developer"
#endif
#ifndef MyAppExeName
  #define MyAppExeName "App.exe"
#endif
#ifndef PublishDir
  #define PublishDir "publish\win-x64-singlefile"
#endif

[Setup]
AppId={{1ECE9444-F1EF-4D07-A52A-EA350C4575AF}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={commonpf64}\{#MyAppName}
DefaultGroupName={#MyAppName}
DisableDirPage=no
DisableProgramGroupPage=no
OutputBaseFilename={#MyAppName}_{#MyAppVersion}_Setup
OutputDir=publish
Compression=lzma
SolidCompression=yes
WizardStyle=modern
ArchitecturesInstallIn64BitMode=x64os
SetupIconFile=img\ico\Installer.ico

[Languages]
Name: "spanish"; MessagesFile: "compiler:Languages\Spanish.isl"

[Tasks]
Name: "desktopicon"; Description: "Crear icono en el escritorio"; GroupDescription: "Tareas adicionales:"; Flags: unchecked

[Files]
; Empaquetar todo el contenido de la carpeta de publicación (incluyendo subcarpetas como img)
Source: "{#PublishDir}\*"; DestDir: "{app}"; Flags: recursesubdirs ignoreversion

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\Desinstalar {#MyAppName}"; Filename: "{uninstallexe}"
Name: "{commondesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Ejecutar {#MyAppName}"; Flags: nowait postinstall skipifsilent

[Code]
procedure InitializeWizard();
var
  AuthorLabel: TLabel;
begin
  { Crear sello de autor en la página de selección de carpeta }
  AuthorLabel := TLabel.Create(WizardForm);
  AuthorLabel.Parent := WizardForm.SelectDirPage;
  AuthorLabel.Caption := 'BY SCORPIO 2026';
  AuthorLabel.Font.Style := [fsBold];
  AuthorLabel.Font.Color := clGray;
  AuthorLabel.Top := WizardForm.DirEdit.Top + WizardForm.DirEdit.Height + 10;
  AuthorLabel.Left := WizardForm.DirEdit.Left;
end;
