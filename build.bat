@echo off
rem Builds Delft3D4Launcher.exe with the C# compiler that ships with Windows (.NET Framework 4.8),
rem then (if Inno Setup 6 and the engine package are available) the installer.
rem Usage: build.bat [path\to\engine-package] [output-folder]
setlocal
cd /d "%~dp0"
set CSC=%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\csc.exe
set PKG=%~1
if "%PKG%"=="" set PKG=C:\build\package\delft3d4-community-DIMRset_2026.02-win64
set OUT=%~2
if "%OUT%"=="" set OUT=C:\build\release

if not exist dist mkdir dist
powershell -NoProfile -ExecutionPolicy Bypass -File src\make-icon.ps1 -Out src\app.ico || exit /b 1

"%CSC%" /nologo /target:winexe /optimize+ /platform:x64 /win32icon:src\app.ico /win32manifest:src\app.manifest ^
  /r:System.dll /r:System.Drawing.dll /r:System.Windows.Forms.dll /r:System.Core.dll ^
  /out:dist\Delft3D4Launcher.exe src\Launcher.cs || exit /b 1
echo Built dist\Delft3D4Launcher.exe

set ISCC=%LOCALAPPDATA%\Programs\Inno Setup 6\ISCC.exe
if not exist "%ISCC%" set ISCC=%ProgramFiles(x86)%\Inno Setup 6\ISCC.exe
if not exist "%ISCC%" set ISCC=%ProgramFiles%\Inno Setup 6\ISCC.exe
if not exist "%ISCC%" (echo Inno Setup 6 not found - skipping installer. & exit /b 0)
if not exist "%PKG%\bin\d_hydro.exe" (echo Engine package not found at "%PKG%" - skipping installer. & exit /b 0)
if not exist "%OUT%" mkdir "%OUT%"
"%ISCC%" /Q /DPkg="%PKG%" /DOutDir="%OUT%" installer\setup.iss || exit /b 1
echo Built installer in %OUT%
