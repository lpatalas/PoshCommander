@echo off
set RunScriptPath=%~dp0bin\Debug\netstandard2.0\Run.ps1
pwsh.exe -ExecutionPolicy RemoteSigned -NoLogo -NoProfile -File "%RunScriptPath%" -new_console
