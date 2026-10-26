; Inno Setup script for the Delft3D 4 Community build + launcher.
; Build: ISCC.exe /DPkg="C:\build\package\delft3d4-community-DIMRset_2026.02-win64" installer\setup.iss

#ifndef Pkg
  #define Pkg "C:\build\package\delft3d4-community-DIMRset_2026.02-win64"
#endif
#ifndef OutDir
  #define OutDir "C:\build\release"
#endif
#define AppName "Delft3D 4 Community"
#define AppVer "DIMRset_2026.02"

[Setup]
AppId={{7C3E2A61-5D2B-4E8E-9C1B-D3D4C0FFEE01}
AppName={#AppName} (unofficial)
AppVersion=2026.02-community.1
AppVerName={#AppName} {#AppVer} (unofficial)
AppPublisher=DevAnthonyM (community build, not affiliated with Deltares)
AppPublisherURL=https://delft3d-community.vercel.app
AppSupportURL=https://delft3d-community.vercel.app
DefaultDirName={autopf}\Delft3D4-Community
DefaultGroupName={#AppName}
DisableProgramGroupPage=yes
PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=dialog
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
LicenseFile=..\installer\license-page.txt
OutputDir={#OutDir}
OutputBaseFilename=Delft3D4-Community-Setup-{#AppVer}
SetupIconFile=..\src\app.ico
UninstallDisplayIcon={app}\Delft3D4Launcher.exe
Compression=lzma2/ultra64
SolidCompression=yes
WizardStyle=modern

[Tasks]
Name: "desktopicon"; Description: "Create a desktop shortcut"; GroupDescription: "Shortcuts:"

[Files]
Source: "..\dist\Delft3D4Launcher.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#Pkg}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs; Excludes: "run_example.bat,Delft3D4Launcher.exe"
Source: "{#Pkg}\run_example.bat"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\Delft3D 4 Community Launcher"; Filename: "{app}\Delft3D4Launcher.exe"
Name: "{group}\Read me"; Filename: "{app}\README.txt"
Name: "{group}\Licenses"; Filename: "{app}\licenses"
Name: "{group}\Uninstall Delft3D 4 Community"; Filename: "{uninstallexe}"
Name: "{autodesktop}\Delft3D 4 Community Launcher"; Filename: "{app}\Delft3D4Launcher.exe"; Tasks: desktopicon

[Run]
Filename: "{app}\Delft3D4Launcher.exe"; Description: "Open Delft3D 4 Community Launcher now"; Flags: nowait postinstall skipifsilent
