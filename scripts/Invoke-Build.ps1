#Requires -PSEdition Core
[CmdletBinding()]
param(
    [ValidateSet('Debug', 'Release')]
    [String] $Configuration = 'Release'
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$helpersDir = Join-Path $PSScriptRoot 'helpers'

function Main {
    RunStep "Build script analysis" {
        & "$helpersDir\RunScriptAnalysis.ps1" -Path $PSScriptRoot
    }

    RunStep "Restoring tools" {
        dotnet tool restore
        if ($LASTEXITCODE -ne 0) {
            throw "dotnet tool restore exited with error code $LASTEXITCODE"
        }
    }

    & "$helpersDir\ClearArtifacts.ps1"

    RunStep "Compilation" {
        & "$helpersDir\BuildSolution.ps1" `
            -Configuration $Configuration
    }

    $coverageResults, $coverageReport = RunStep "Tests and code coverage" {
        & "$helpersDir\RunTests.ps1" `
            -Configuration $Configuration
    }

    $modulePath = RunStep "Publish module to artifacts directory" {
        & "$helpersDir\PublishModule.ps1" `
            -Configuration $Configuration
    }

    & "$helpersDir\ClearArtifacts.ps1" -OnlyTemporary

    ShowSummary

    [PSCustomObject]@{
        PublishedModule = $modulePath
        TestCoverageReport = $coverageReport
        TestCoverageResults = $coverageResults
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
