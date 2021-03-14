#Requires -PSEdition Core -Module platyPS, PSScriptAnalyzer
[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [ValidateSet('Debug', 'Release')]
    [String] $Configuration
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$workspaceRoot = & "$PSScriptRoot\GetWorkspaceRoot.ps1"
$artifactsDir = Join-Path $workspaceRoot 'artifacts'

function Main {
    $modulePath = PublishProjectToOutputDirectory
    CleanupPublishedFiles $modulePath
    GenerateHelpFiles $modulePath
    RunPSScriptAnalyzer $modulePath

    $modulePath
}

function PublishProjectToOutputDirectory {
    $publishOutputPath = Join-Path $artifactsDir 'PoshCommander'
    $projectPath = Join-Path $workspaceRoot 'src' 'PoshCommander.fsproj'
    $sourceManifestPath = Join-Path $workspaceRoot 'src' "PoshCommander.psd1"
    $manifest = Import-PowerShellDataFile -Path $sourceManifestPath

    Write-Verbose "Publishing project '$projectPath' to '$publishOutputPath'"
    Write-Verbose "Module Version: $($manifest.ModuleVersion)"

    if (Test-Path $publishOutputPath) {
        Write-Verbose "Removing existing directory: $publishOutputPath"
        Remove-Item $publishOutputPath -Force -Recurse
    }

    dotnet publish `
        --configuration $Configuration `
        --no-build `
        --no-restore `
        --output "$publishOutputPath" `
        --verbosity (& "$PSScriptRoot\GetMSBuildVerbosity.ps1") `
        /p:ModuleVersion="$($manifest.ModuleVersion)" `
        /p:PreserveCompilationContext="false" `
        "$projectPath" `
        | Out-Host

    if ($LASTEXITCODE -ne 0) {
        throw "dotnet publish exited with error code $LASTEXITCODE"
    }

    return $publishOutputPath
}

function CleanupPublishedFiles($publishDirectory) {
    Write-Verbose "Cleaning-up directory: $publishDirectory"
    Get-ChildItem (Join-Path $publishDirectory '*.deps.json') `
        | ForEach-Object {
            Write-Verbose "Removing $_"
            Remove-Item $_.FullName
        }
}

function GenerateHelpFiles($publishDirectory) {
    Write-Verbose "Generating help files"

    $docsPath = Join-Path $workspaceRoot 'docs'

    New-ExternalHelp -Path $docsPath -OutputPath $publishDirectory -Force `
        | ForEach-Object {
            Write-Verbose "Generated $($_.FullName)"
        }
}

function RunPSScriptAnalyzer($publishDirectory) {
    & "$PSScriptRoot\RunScriptAnalysis.ps1" -Path $publishDirectory
}

Main
