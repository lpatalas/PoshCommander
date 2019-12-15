#Requires -PSEdition Core -Module PSScriptAnalyzer
[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

Write-Verbose 'Running PSScriptAnalyzer on build scripts'

$results = Invoke-ScriptAnalyzer `
    -Path $PSScriptRoot `
    -Severity Warning `
    -Settings PSGallery

if (@($results).Count -gt 0) {
    $results | Out-Host
    throw 'PSScriptAnalyzer returned some errors'
}
else {
    Write-Host "PSScriptAnalyzer did not find any errors"
}
