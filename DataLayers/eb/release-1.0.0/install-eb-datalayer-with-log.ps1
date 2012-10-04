# Import-Module WebAdministration

param($context, $app, $community)

$base_path = Split-Path -Path $MyInvocation.MyCommand.Path -Parent
& $base_path\install-eb-datalayer.ps1 $context $app $community > $env:temp\install-eb-datalayer.log 2>&1