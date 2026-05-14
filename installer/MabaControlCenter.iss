#define MyAppName "Maba Control Center"
#define MyAppPublisher "MABA"
#define MyAppPublisherUrl "https://mabasol.com"
#define MyAppSupportUrl "https://mabasol.com"
#define MyAppUpdatesUrl "https://api.mabasol.com/desktop-updates/stable/manifest.json"
#define MyAppExeName "MabaControlCenter.exe"
#define MyAppAssocName MyAppName + " Desktop App"
#define MyAppId "{{8D225A92-59B6-4627-A099-9A0C5F0E8D69}}"
#ifndef MyAppVersion
  #define MyAppVersion "0.0.0"
#endif
#ifndef SourcePublishDir
  #define SourcePublishDir "C:\MabaControlCenter\publish"
#endif
#ifndef OutputInstallerDir
  #define OutputInstallerDir "C:\MabaControlCenter\installer-output"
#endif
#ifndef InstallerIconFile
  #define InstallerIconFile "C:\Users\PC\Desktop\maba\MabaControlCenter\Assets\maba.ico"
#endif

[Setup]
AppId={#MyAppId}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppPublisherUrl}
AppSupportURL={#MyAppSupportUrl}
AppUpdatesURL={#MyAppUpdatesUrl}
VersionInfoCompany={#MyAppPublisher}
VersionInfoProductName={#MyAppName}
VersionInfoProductVersion={#MyAppVersion}
VersionInfoDescription={#MyAppName} Installer
VersionInfoTextVersion={#MyAppVersion}
DefaultDirName={localappdata}\Programs\Maba Control Center
DefaultGroupName=Maba Control Center
AllowNoIcons=yes
DisableProgramGroupPage=yes
OutputDir={#OutputInstallerDir}
OutputBaseFilename=MabaControlCenter-{#MyAppVersion}-Setup
SetupIconFile={#InstallerIconFile}
UninstallDisplayIcon={app}\MabaControlCenter.exe
Compression=lzma
SolidCompression=yes
WizardStyle=modern
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=dialog
CloseApplications=yes
CloseApplicationsFilter={#MyAppExeName}
RestartApplications=yes
UsedUserAreasWarning=no
SetupLogging=yes

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "Create a desktop shortcut"; GroupDescription: "Additional shortcuts:"; Flags: unchecked

[Files]
Source: "{#SourcePublishDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{autoprograms}\Maba Control Center"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\Maba Control Center"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Launch Maba Control Center"; Flags: nowait postinstall skipifsilent
