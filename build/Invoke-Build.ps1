#Requires -PSEdition Core -Module PSScriptAnalyzer
[CmdletBinding()]
param(
    [ValidateRange('Positive')]
    [Nullable[Int32]] $PreReleaseNumber,

    [String] $ProjectName = 'PoshCommander'
)

$workspaceRoot = Split-Path $PSScriptRoot
$projectPath = Join-Path $workspaceRoot 'src' "$ProjectName.fsproj"
$testProjectPath = Join-Path $workspaceRoot 'tests' "$ProjectName.Tests.fsproj"

function Main {
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


function BuildSolution {
    $solutionPath = Join-Path $workspaceRoot "$ProjectName.sln"

    dotnet build `
        --configuration Release `
        /p:ModuleVersion="$moduleVersion" `
        /p:PreserveCompilationContext="false" `
        "$solutionPath"

    if ($LASTEXITCODE -ne 0) {
        throw "dotnet build exited with error code $LASTEXITCODE"
    }
}

function RunTests {
    dotnet test `
        --configuration Release `
        --no-build `
        --no-restore `
        "$testProjectPath"

    if ($LASTEXITCODE -ne 0) {
        throw "dotnet test exited with error code $LASTEXITCODE"
    }
}

function PublishProjectToOutputDirectory {
    $publishOutputPath = Join-Path $workspaceRoot 'artifacts' $ProjectName
    $sourceManifestPath = Join-Path $workspaceRoot 'src' "$ProjectName.psd1"
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

        $manifestPath = Join-Path $publishDirectory "$ProjectName.psd1"
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
