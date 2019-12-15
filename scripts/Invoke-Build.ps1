#Requires -PSEdition Core
[CmdletBinding()]
param(
    [ValidateSet('Debug', 'Release')]
    [String] $Configuration = 'Release'
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$workspaceRoot = & "$PSScriptRoot\Get-WorkspaceRoot.ps1"
$artifactsDir = Join-Path $workspaceRoot 'artifacts'

function Main {
    RunStep "Build script analysis" {
        & "$PSScriptRoot\Test-ScriptsInDirectory.ps1" -Path $PSScriptRoot
    }

    RemoveExistingArtifacts

    RunStep "Compilation" {
        BuildSolution
    }

    $coverageResults, $coverageReport = RunStep "Tests and code coverage" {
        & "$PSScriptRoot\Invoke-TestCoverage.ps1" `
            -Configuration $Configuration
    }

    $modulePath = RunStep "Publish module to artifacts directory" {
        & "$PSScriptRoot\Invoke-ModulePublish.ps1" `
            -Configuration $Configuration
    }

    RemoveTemporaryArtifacts
    ShowSummary

    [PSCustomObject]@{
        PublishedModule = $modulePath
        TestCoverageReport = $coverageReport
        TestCoverageResults = $coverageResults
    }
}

function RemoveExistingArtifacts {
    if (Test-Path $artifactsDir) {
        Write-Verbose "Removing artifacts directory: $artifactsDir"
        Remove-Item `
            -LiteralPath $artifactsDir `
            -Force `
            -Recurse
    }
}

function RemoveTemporaryArtifacts {
    $tempArtifactsDir = Join-Path $artifactsDir 'temp'
    if (Test-Path $tempArtifactsDir) {
        Write-Verbose "Removing temporary artifacts directory: $tempArtifactsDir"
        Remove-Item `
            -LiteralPath $tempArtifactsDir `
            -Force `
            -Recurse
    }
}

function BuildSolution {
    $solutionPath = Join-Path 'src' "PoshCommander.sln"

    dotnet build `
        --configuration $Configuration `
        --verbosity (& "$PSScriptRoot\Get-MSBuildVerbosity.ps1") `
        $solutionPath

    if ($LASTEXITCODE -ne 0) {
        throw "dotnet build exited with error code $LASTEXITCODE"
    }
}

function WriteHeader($text) {
    Write-Host ('-' * ($text.Length + 8)) -ForegroundColor Cyan
    Write-Host "--- $text ---" -ForegroundColor Cyan
    Write-Host ('-' * ($text.Length + 8)) -ForegroundColor Cyan
}

$executedSteps = New-Object System.Collections.ArrayList
$totalTimeStopwatch = New-Object System.Diagnostics.Stopwatch

function RunStep {
    param(
        [String] $Name,
        [scriptblock] $Action
    )

    $sw = New-Object System.Diagnostics.Stopwatch
    $sw.Start()

    WriteHeader $Name
    Invoke-Command -ScriptBlock $Action
    Write-Host

    $elapsedTime = $sw.Elapsed
    $executedSteps.Add([PSCustomObject]@{
        Duration = $elapsedTime
        Name = $Name
    }) | Out-Null
}

function ShowSummary {
    WriteHeader 'Summary'
    $executedSteps `
        | Format-Table -Property Name, Duration -HideTableHeaders

    Write-Host "Total time: $($totalTimeStopwatch.Elapsed)" -ForegroundColor Green
}

$totalTimeStopwatch.Start()
Main
