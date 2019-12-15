#Requires -PSEdition Core -Module PSScriptAnalyzer
[CmdletBinding()]
param(
    [ValidateSet('Debug', 'Release')]
    [String] $Configuration = 'Release'
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$workspaceRoot = & "$PSScriptRoot\Get-WorkspaceRoot.ps1"
$artifactsDir = Join-Path $workspaceRoot 'artifacts'
$solutionPath = Join-Path 'src' "PoshCommander.sln"

function Main {
    RemoveExistingArtifacts
    BuildSolution

    $coverageResults, $coverageReport = & "$PSScriptRoot\Invoke-TestCoverage.ps1" `
        -Configuration $Configuration

    $modulePath = & "$PSScriptRoot\Publish-Module.ps1" `
        -Configuration $Configuration

    RemoveTemporaryArtifacts
    Write-Host 'Build succeeded' -ForegroundColor Green

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
    dotnet build `
        --configuration $Configuration `
        $solutionPath

    if ($LASTEXITCODE -ne 0) {
        throw "dotnet build exited with error code $LASTEXITCODE"
    }
}

Main
