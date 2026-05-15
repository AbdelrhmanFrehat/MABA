# Maba Control Center Installer

This folder contains the Windows installer definition for the desktop app.

## What it provides

- Stable install location under `Program Files`
- Start Menu shortcut
- Optional desktop shortcut
- Uninstall entry through Windows Apps & Features
- In-place upgrades that preserve the same app identity

## Build the installer

Use:

```powershell
powershell -ExecutionPolicy Bypass -File C:\Users\PC\Desktop\maba\scripts\package-maba-desktop-installer.ps1
```

## One-command desktop release

To build the desktop app, updater package, installer, manifests, and sign artifacts automatically when a certificate is configured:

```powershell
powershell -ExecutionPolicy Bypass -File C:\Users\PC\Desktop\maba\scripts\release-desktop.ps1
```

To build and sign in one step once you have a code-signing certificate configured:

```powershell
powershell -ExecutionPolicy Bypass -File C:\Users\PC\Desktop\maba\scripts\package-maba-desktop-installer.ps1 -SignArtifacts
```

If Inno Setup 6 is installed, this produces:

- `artifacts/desktop-installer/stable/MabaControlCenter-<version>-Setup.exe`

If Inno Setup is not installed, the script still prepares the publish output and tells you what is missing.

The combined release script produces:

- `artifacts/desktop-updates/stable/MabaControlCenter-<version>-win-x64.zip`
- `artifacts/desktop-updates/stable/manifest.json`
- `artifacts/desktop-installer/stable/MabaControlCenter-<version>-Setup.exe`
- `artifacts/desktop-installer/stable/manifest.json`

## Signing configuration

The signing helper supports either:

- `MABA_CODESIGN_CERT_PATH` + `MABA_CODESIGN_CERT_PASSWORD`
- or `MABA_CODESIGN_CERT_THUMBPRINT`

Artifacts signed by the pipeline:

- `MabaControlCenter.exe`
- `Updater\MabaUpdater.exe`
- `MabaControlCenter-<version>-Setup.exe`
