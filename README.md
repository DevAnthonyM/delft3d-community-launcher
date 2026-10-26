# Delft3D 4 Community Launcher

[![build](https://github.com/DevAnthonyM/delft3d-community-launcher/actions/workflows/build.yml/badge.svg)](https://github.com/DevAnthonyM/delft3d-community-launcher/actions/workflows/build.yml)

A small Windows app that runs Delft3D 4 (Delft3D-FLOW) models with the community-compiled engines, without command lines.
**Unofficial. Not affiliated with or endorsed by Deltares.** "Delft3D" and "Deltares" are trademarks of Stichting Deltares.

## What it does
- Choose a model folder → it finds the `.mdf` (dropdown if several).
- Missing `config_d_hydro.xml`? **Create it for me** writes one (an existing one is backed up as `.bak`).
- **Run model**: runs `bin\d_hydro.exe config_d_hydro.xml` in the model folder with `PATH=bin;share;lib`, exactly like Deltares' `bin\run_dflow2d3d.bat`. No console window, live progress, Stop button.
- Shows **Finished: 0 errors** or the actual errors from the `tri-diag` file.
- **Try it: run the example model** copies Deltares' f34 example to `Documents\Delft3D4-example` and runs it.

Limits: runs single-domain Delft3D-FLOW models (`.mdf`). It does not build models (grids, depths). For that, use Deltares' free GUI: https://download.deltares.nl/en/delft3d-4-gui-open-source

## Build
Requirements: Windows 10/11 (uses the built-in .NET Framework 4.8 C# compiler) and, for the installer, Inno Setup 6.
```
build.bat [path\to\engine-package]
```
Produces `dist\Delft3D4Launcher.exe` and `C:\build\release\Delft3D4-Community-Setup-DIMRset_2026.02.exe`.

Files: `src\Launcher.cs` (the app), `src\make-icon.ps1` (draws the original icon), `src\app.manifest` (DPI-aware, no admin), `installer\setup.iss` (per-user install by default, Start-menu + optional desktop shortcut, uninstaller).

Every push is built by GitHub Actions ([build.yml](.github/workflows/build.yml)) from this source; the run summary lists the SHA-256 of each file.

## Privacy
The launcher collects nothing and makes no network connections. It only reads and writes files in the model folder you choose and remembers the last folder in `%APPDATA%\Delft3D4CommunityLauncher`.

## Code signing policy
See [CODE_SIGNING_POLICY.md](CODE_SIGNING_POLICY.md).

## License
GPL-3.0-or-later. The Delft3D engines are GPL-3.0 / LGPL-2.1 / AGPL-3.0 (source: https://github.com/Deltares/Delft3D, tag DIMRset_2026.02).
