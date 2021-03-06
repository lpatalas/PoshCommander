[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [ValidateSet('Debug', 'Release')]
    [String] $Configuration
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$workspaceRoot = & "$PSScriptRoot\helpers\GetWorkspaceRoot.ps1"
$modulePath = "$workspaceRoot\src\bin\$Configuration\netstandard2.0\PoshCommander.psd1"

& "$PSScriptRoot\helpers\BuildSolution.ps1" -Configuration $Configuration

if (-not (Test-Path $modulePath)) {
    throw "Can't find module manifest at path: $modulePath"
}

Import-Module $modulePath
Invoke-PoshCommander
