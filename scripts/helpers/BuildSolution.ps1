#Requires -PSEdition Core
[CmdletBinding()]
param(
    [ValidateSet('Debug', 'Release')]
    [String] $Configuration
)

$workspaceRoot = & "$PSScriptRoot\GetWorkspaceRoot.ps1"
$solutionPath = Join-Path $workspaceRoot 'src' "PoshCommander.sln"

dotnet build `
    --configuration $Configuration `
    --verbosity (& "$PSScriptRoot\GetMSBuildVerbosity.ps1") `
    $solutionPath

if ($LASTEXITCODE -ne 0) {
    throw "dotnet build exited with error code $LASTEXITCODE"
}
