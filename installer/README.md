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

If Inno Setup 6 is installed, this produces:

- `artifacts/desktop-installer/stable/MabaControlCenter-<version>-Setup.exe`

If Inno Setup is not installed, the script still prepares the publish output and tells you what is missing.

