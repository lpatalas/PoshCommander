@echo off

pwsh.exe -Version 2>NUL >NUL || (
    echo "pwsh.exe not found. Install PowerShell Core from https://github.com/PowerShell/PowerShell"
    exit /B
)

pwsh.exe -NoProfile -ExecutionPolicy RemoteSigned -File "%~dp0scripts\Invoke-Build.ps1"
