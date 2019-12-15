[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

$workspaceRoot = Split-Path $PSScriptRoot
$solutionRoot = Join-Path $workspaceRoot 'src'
$projectDir = Join-Path $solutionRoot 'PoshCommander.Tests'
$runsettingsPath = Join-Path $projectDir 'coverlet.runsettings'
$testProjectPath = Join-Path $projectDir 'PoshCommander.Tests.fsproj'
$testResultsDir = Join-Path $workspaceRoot 'artifacts' 'TestResults'
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

    Write-Host 'Running tests' -ForegroundColor Cyan
    dotnet test `
        --results-directory:"$testResultsDir" `
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
