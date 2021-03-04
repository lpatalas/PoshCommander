#Requires -PSEdition Core -Module PSScriptAnalyzer
[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [String] $Path
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

Write-Verbose "Running PSScriptAnalyzer on directory: $Path"

$results = Invoke-ScriptAnalyzer `
    -Path $Path `
    -Severity Warning `
    -Settings PSGallery

if (@($results).Count -gt 0) {
    $results | Out-Host
    throw "PSScriptAnalyzer returned errors for directory: $Path"
}
else {
    Write-Host "PSScriptAnalyzer did not find any errors"
}
