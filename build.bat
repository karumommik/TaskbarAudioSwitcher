@echo off
echo Building Taskbar Audio Switcher v2.0 (.NET 10.0)...
dotnet --version >nul 2>&1
if %errorlevel% neq 0 (
    echo.
    echo ERROR: .NET SDK is not installed or not in PATH!
    echo Please download and install .NET 10.0 SDK from:
    echo https://dotnet.microsoft.com/download/dotnet/10.0
    echo.
    pause
    exit /b 1
)

echo Restoring and publishing self-contained executable...
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:PublishReadyToRun=true -o .\build
if %errorlevel% neq 0 (
    echo.
    echo Build failed!
    pause
    exit /b %errorlevel%
)

echo.
echo Build succeeded! Executable is located in .\build\TaskbarAudioSwitcher.exe
echo.
pause
