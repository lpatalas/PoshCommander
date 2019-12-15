#Requires -PSEdition Core -Module PSScriptAnalyzer
[CmdletBinding()]
param(
    [ValidateRange('Positive')]
    [Nullable[Int32]] $PreReleaseNumber,

    [ValidateSet('Debug', 'Release')]
    [String] $Configuration = 'Release'
)

$projectName = 'PoshCommander'
$workspaceRoot = Split-Path $PSScriptRoot
$artifactsRoot = Join-Path $workspaceRoot 'artifacts'
$tempArtifactsRoot = Join-Path $artifactsRoot 'temp'
$solutionRoot = Join-Path $workspaceRoot 'src'
$solutionPath = Join-Path $solutionRoot "$projectName.sln"
$projectPath = Join-Path $solutionRoot $projectName "$projectName.fsproj"
$testProjectPath = Join-Path $solutionRoot "$projectName.Tests" "$projectName.Tests.fsproj"

function Main {
    CleanTempArtifacts
    BuildSolution
    RunTests

    $modulePath = PublishProjectToOutputDirectory
    CleanupPublishedFiles $modulePath
    GenerateHelpFiles $modulePath
    UpdatePreReleaseVersion $modulePath
    RunPSScriptAnalyzer $modulePath

    Write-Host 'Build succeeded' -ForegroundColor Green

    Get-Item $modulePath
}

function CleanTempArtifacts {
    if (Test-Path $tempArtifactsRoot) {
        Write-Verbose "Removing temporary artifacts directory: $tempArtifactsRoot"
        Remove-Item `
            -LiteralPath $tempArtifactsRoot `
            -Force `
            -Recurse
    }
}

function BuildSolution {
    dotnet build `
        --configuration $Configuration `
        /p:ModuleVersion="$moduleVersion" `
        /p:PreserveCompilationContext="false" `
        $solutionPath

    if ($LASTEXITCODE -ne 0) {
        throw "dotnet build exited with error code $LASTEXITCODE"
    }
}

function RunTests {
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
        $testProjectPath

    if ($LASTEXITCODE -ne 0) {
        throw "dotnet test exited with error code $LASTEXITCODE"
    }

    $coverageResultsFileName = 'coverage.cobertura.xml'
    $coverageResultsFile = Get-ChildItem `
        -Include $coverageResultsFileName `
        -LiteralPath $testResultsDir `
        -Recurse

    if ($coverageResultsFile.Count -eq 1) {
        Copy-Item $coverageResultsFile $artifactsRoot
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

function PublishProjectToOutputDirectory {
    $publishOutputPath = Join-Path $workspaceRoot 'artifacts' $projectName
    $sourceManifestPath = Join-Path $solutionRoot $projectName "$projectName.psd1"
    $manifest = Import-PowerShellDataFile -Path $sourceManifestPath

    Write-Host "Publishing solution '$projectPath' to '$publishOutputPath'" -ForegroundColor Cyan
    Write-Host "Module Version: $($manifest.ModuleVersion)"

    if (Test-Path $publishOutputPath) {
        Write-Host "Removing existing directory: $publishOutputPath"
        Remove-Item $publishOutputPath -Force -Recurse
    }

    dotnet publish `
        --configuration Release `
        --no-build `
        --no-restore `
        --output "$publishOutputPath" `
        /p:ModuleVersion="$moduleVersion" `
        /p:PreserveCompilationContext="false" `
        "$projectPath" `
        | Out-Host

    if ($LASTEXITCODE -ne 0) {
        throw "dotnet publish exited with error code $LASTEXITCODE"
    }

    return $publishOutputPath
}

function CleanupPublishedFiles($publishDirectory) {
    Write-Host "Cleaning-up directory: $publishDirectory" -ForegroundColor Cyan
    Get-ChildItem (Join-Path $publishDirectory '*.deps.json') `
        | ForEach-Object {
            Write-Host "Removing $_"
            Remove-Item $_.FullName
        }
}

function GenerateHelpFiles($publishDirectory) {
    Write-Host "Generating help files" -ForegroundColor Cyan

    $docsPath = Join-Path $workspaceRoot 'docs'

    New-ExternalHelp -Path $docsPath -OutputPath $publishDirectory -Force `
        | ForEach-Object {
            Write-Host "Generated $($_.FullName)"
        }
}

function UpdatePreReleaseVersion($publishDirectory) {
    if ($PreReleaseNumber) {
        $preReleaseVersion = 'pre{0:000}' -f $PreReleaseNumber
        Write-Host "Setting pre-release version to: $preReleaseVersion" -ForegroundColor Cyan

        $manifestPath = Join-Path $publishDirectory "$projectName.psd1"
        Update-ModuleManifest `
            -Path $manifestPath `
            -Prerelease $preReleaseVersion
    }
    else {
        Write-Host "Pre-release version was not specified" -ForegroundColor Cyan
    }
}

function RunPSScriptAnalyzer($publishDirectory) {
    Write-Host 'Running PSScriptAnalyzer on published project' -ForegroundColor Cyan

    if (-not (Get-Module PSScriptAnalyzer -ErrorAction SilentlyContinue)) {
        Write-Host 'Importing PSScriptAnalyzer module'
        Import-Module PSScriptAnalyzer -ErrorAction Stop
    }

    $allResults = @()

    Get-ChildItem -Path $publishDirectory -Filter '*.ps*1' `
        | ForEach-Object {
            Write-Host "Analyzing $($_.FullName)"
            $results = Invoke-ScriptAnalyzer `
                -Path $_ `
                -Severity Warning `
                -Settings PSGallery

            $allResults += @($results)
        }

    if ($allResults.Count -gt 0) {
        $allResults | Out-Host
        throw 'PSScriptAnalyzer returned some errors'
    }
}

Main
