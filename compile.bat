@echo off
echo Compiling Taskbar Audio Switcher...
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe /target:winexe /out:TaskbarAudioSwitcher.exe Program.cs
if %errorlevel% neq 0 (
    echo Compilation failed!
    pause
    exit /b %errorlevel%
)
echo Compiled successfully! Starting...
start TaskbarAudioSwitcher.exe
exit
