#!/usr/bin/env bash

PWSH_CMD=''

if command -v pwsh-preview >/dev/null; then
    PWSH_CMD='pwsh-preview'
elif command -v pwsh >/dev/null; then
    PWSH_CMD='pwsh'
else
    echo "pwsh command not found. Install PowerShell Core from https://github.com/PowerShell/PowerShell"
    exit 1
fi

WORKSPACEROOT="$(dirname $0)"
$PWSH_CMD -NoProfile -ExecutionPolicy RemoteSigned -File "$WORKSPACEROOT/scripts/Invoke-Build.ps1"
