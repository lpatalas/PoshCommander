#Requires -PSEdition Core
[CmdletBinding()]
param(
    [switch] $OnlyTemporary
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$workspaceRoot = & "$PSScriptRoot/GetWorkspaceRoot.ps1"
$artifactsDir = if ($OnlyTemporary) {
    Join-Path $workspaceRoot 'artifacts' 'temp'
}
else {
    Join-Path $workspaceRoot 'artifacts'
}

if (Test-Path $artifactsDir) {
    Write-Verbose "Removing artifacts directory: $artifactsDir (only temporary: $OnlyTemporary)"
    Remove-Item `
        -LiteralPath $artifactsDir `
        -Force `
        -Recurse
}
