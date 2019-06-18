Import-Module "$PSScriptRoot\PoshCommander.dll"

#1..10 | %{ Write-Host "Line $_" }

Invoke-PoshCommander .
