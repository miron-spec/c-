@echo off
setlocal
cd /d "%~dp0"

echo.
echo ========================================
echo          LoveMatch - OFFLINE MODE
echo ========================================
echo.

where dotnet >nul 2>nul
if errorlevel 1 (
  echo [ERROR] .NET 8 SDK is not installed.
  echo Install .NET 8 SDK once, then LoveMatch can run locally without Internet.
  echo.
  echo If you already have Visual Studio 2022/2026 with ASP.NET Core installed,
  echo open LoveMatch.sln and run MatchApp.Backend with Ctrl+F5.
  pause
  exit /b 1
)

if not exist "%~dp0wwwroot\index.html" (
  echo [ERROR] LoveMatch files are incomplete.
  pause
  exit /b 1
)

if exist "%~dp0obj\project.assets.json" (
  echo [LoveMatch] Local NuGet packages are already restored.
  echo [LoveMatch] Starting without Internet and without package restore...
  start "LoveMatch Server" /D "%~dp0" cmd /k dotnet run --no-restore --no-launch-profile --urls http://localhost:5187
) else (
  echo [LoveMatch] First run detected.
  echo [LoveMatch] No Internet is required after NuGet packages have been restored once.
  echo.
  echo This computer does not have the local NuGet restore cache for this project.
  echo If you have Internet, run this file once to restore packages.
  echo After that, future launches will work completely offline.
  echo.
  choice /C YN /N /M "Restore NuGet packages now? [Y/N]: "
  if errorlevel 2 (
    echo.
    echo Cancelled. To run fully offline, restore this project once on this PC.
    pause
    exit /b 0
  )
  echo.
  dotnet restore --ignore-failed-sources
  if errorlevel 1 (
    echo.
    echo [ERROR] NuGet packages could not be restored.
    echo After a successful restore, LoveMatch can run without Internet.
    pause
    exit /b 1
  )
  echo [LoveMatch] Starting local server...
  start "LoveMatch Server" /D "%~dp0" cmd /k dotnet run --no-restore --no-launch-profile --urls http://localhost:5187
)

powershell -NoProfile -ExecutionPolicy Bypass -Command "$url='http://localhost:5187'; for($i=0;$i-lt90;$i++){try{Invoke-WebRequest -UseBasicParsing -Uri $url -TimeoutSec 1 | Out-Null; Start-Process $url; exit 0}catch{}; Start-Sleep -Seconds 1}; Start-Process $url"
endlocal
