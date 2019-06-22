[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

$workspaceRoot = Split-Path $PSScriptRoot
$projectDir = Join-Path $workspaceRoot 'tests'
$runsettingsPath = Join-Path $projectDir 'coverlet.runsettings'
$testProjectPath = Join-Path $projectDir 'PoshCommander.Tests.csproj'
$testResultsDir = Join-Path $projectDir 'TestResults'
$reportOutputDir = Join-Path $testResultsDir 'CoverageReport'

function Main {
    Set-Location $projectDir

    $coverageResultsFile = GenerateCoverageResults

    Write-Host "Generating coverage report from file '$coverageResultsFile'" -ForegroundColor Cyan
    dotnet reportgenerator `
        "-reports:$coverageResultsFile" `
        "-targetdir:$reportOutputDir" `
        "-assemblyfilters:+PoshCommander*" `
        | Out-Host

    if ($LASTEXITCODE -ne 0) {
        throw "dotnet reportgenerator exited with error code $LASTEXITCODE"
    }

    $reportIndexPath = Join-Path $reportOutputDir 'index.htm'
    return Get-Item `
        -LiteralPath $reportIndexPath
}

function GenerateCoverageResults {
    Write-Host "Clearing results directory '$testResultsDir'" -ForegroundColor Cyan
    Remove-Item `
        -Force `
        -Path "$testResultsDir\*" `
        -Recurse

    Write-Host 'Running tests' -ForegroundColor Cyan
    dotnet test `
        --settings:"$runsettingsPath" `
        "$testProjectPath" `
        | Out-Host

    if ($LASTEXITCODE -ne 0) {
        throw "dotnet test exited with error code $LASTEXITCODE"
    }

    $coverageResultsFileName = 'coverage.opencover.xml'
    $coverageResultsFile = Get-ChildItem `
        -Include $coverageResultsFileName `
        -LiteralPath $testResultsDir `
        -Recurse

    if ($coverageResultsFile.Count -eq 1) {
        return $coverageResultsFile
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

$originalLocation = Get-Location
try {
    Main
}
finally {
    Set-Location $originalLocation
}
