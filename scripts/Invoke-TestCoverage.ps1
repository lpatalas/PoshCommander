[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [ValidateSet('Debug', 'Release')]
    [String] $Configuration
)

$ErrorActionPreference = 'Stop'

$workspaceRoot = & "$PSScriptRoot\Get-WorkspaceRoot.ps1"
$artifactsRoot = Join-Path $workspaceRoot 'artifacts'
$tempArtifactsRoot = Join-Path $artifactsRoot 'temp'
$solutionRoot = Join-Path $workspaceRoot 'src'
$projectDir = Join-Path $solutionRoot 'PoshCommander.Tests'
$runsettingsPath = Join-Path $projectDir 'coverlet.runsettings'
$testProjectPath = Join-Path $projectDir 'PoshCommander.Tests.fsproj'
$testResultsDir = Join-Path $workspaceRoot 'artifacts' 'TestResults'
$reportOutputDir = Join-Path $testResultsDir 'CoverageReport'

function Main {
    $coverageResultsFile = GenerateCoverageResults
    $coverageReportFile = GenerateReport $coverageResultsFile

    $coverageResultsFile
    $coverageReportFile
}

function GenerateCoverageResults {
    $runsettingsPath = Join-Path (Split-Path $testProjectPath) 'coverlet.runsettings'
    $testResultsDir = Join-Path $tempArtifactsRoot 'TestResults'

    if (Test-Path $testResultsDir) {
        Write-Host "Clearing results directory '$testResultsDir'" -ForegroundColor Cyan
        Remove-Item `
            -Force `
            -Path "$testResultsDir\*" `
            -Recurse
    }
    else {
        Write-Host "Creating results directory '$testResultsDir'"
    }

    dotnet test `
        --configuration $Configuration `
        --no-build `
        --no-restore `
        --results-directory:"$testResultsDir" `
        --settings:"$runsettingsPath" `
        $testProjectPath `
        | Out-Host

    if ($LASTEXITCODE -ne 0) {
        throw "dotnet test exited with error code $LASTEXITCODE"
    }

    $coverageResultsFileName = 'coverage.cobertura.xml'
    $coverageResultsFile = Get-ChildItem `
        -Include $coverageResultsFileName `
        -LiteralPath $testResultsDir `
        -Recurse

    if (@($coverageResultsFile).Count -eq 1) {
        Copy-Item $coverageResultsFile $artifactsRoot -PassThru
    }
    elseif (-not $coverageResultsFile) {
        throw "Can't find any '$coverageResultsFileName' in directory '$testResultsDir'"
    }
    elseif ($coverageResultsFile.Count -gt 1) {
        $allFiles = $coverageResultsFile `
            | ForEach-Object { "'$($_.FullName)'" } `
            -join ';'

        throw "Found more than one '$coverageResultsFileName' files: $allFiles"
    }
}

function GenerateReport($coverageResultsFile) {
    Write-Verbose "Generating coverage report from file '$coverageResultsFile'"
    dotnet reportgenerator `
        "-reports:$coverageResultsFile" `
        "-targetdir:$reportOutputDir" `
        "-assemblyfilters:+PoshCommander*" `
        | ForEach-Object {
            Write-Verbose $_
        }

    if ($LASTEXITCODE -ne 0) {
        throw "dotnet reportgenerator exited with error code $LASTEXITCODE"
    }

    $reportIndexPath = Join-Path $reportOutputDir 'index.htm'
    Get-Item -LiteralPath $reportIndexPath
}

Main
