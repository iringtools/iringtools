# Import-Module WebAdministration

param($context, $app, $community)

# =============================================================================
# Get deployment package
# =============================================================================
$base_path = Split-Path -Path $MyInvocation.MyCommand.Path -Parent
$zip_file = Get-Item $base_path\eBDataLayer*.zip

if ($zip_file -is [System.Array]) {
  $zip_file_name = $zip_file[$zip_file.Count - 1].name
}
else {
  $zip_file_name = $zip_file.name
}

if (!$zip_file_name) {
  Write-Output "Deployment package not found."
  Exit 1
}
Write-Output "Deployment package: $zip_file_name."

# =============================================================================
# Find iRINGTools services installation
# =============================================================================
$irt_path = "\inetpub\iringtools"

if ((Test-Path -path e:) -eq $True) { 
  $irt_path = "e:" + $irt_path
}
elseif ((Test-Path -path d:) -eq $True) { 
  $irt_path = "d:" + $irt_path
}
else { 
  $irt_path = "c:" + $irt_path
}

Write-Output "iRINGTools path: $irt_path."

if ((Test-Path -path "$irt_path") -eq $False) {
  Write-Output "iRINGTools services not found."
  Exit 1
}

# =============================================================================
# Deploy binaries to bin folder and configurations to App_Data folder
# =============================================================================
$shell_app = New-Object -com shell.application
  
if ($context -eq $Null -or $app -eq $Null) {
  # context & app not provided, deploy binaries
  $zip_bin = "$base_path\$zip_file_name\bin"
  $irt_bin = "$irt_path\services\bin"

  $zip_bin_folder = $shell_app.namespace($zip_bin)
  $irt_bin_folder = $shell_app.namespace($irt_bin)
  $irt_bin_folder.CopyHere($zip_bin_folder.items(), 0x14)
  
  Write-Output "Binaries deployed successfully."
}
else {
  # context & app provided, deploy configurations
  $gen_dir = [system.guid]::newguid().guid.tostring()
  $temp_dir = "$env:temp\$gen_dir"  
  new-item $temp_dir -type directory -force
  
  $zip_conf = "$base_path\$zip_file_name\conf"
  $zip_conf_folder = $shell_app.namespace($zip_conf)
  $temp_folder = $shell_app.NameSpace($temp_dir)
  $temp_folder.CopyHere($zip_conf_folder.items())
  
  $temp_folder.items() | foreach {
    $new_file_name = $_.name
    $new_file_name = $new_file_name -replace "{context}", $context
    $new_file_name = $new_file_name -replace "{app}", $app
    $new_file_name = $new_file_name -replace "{community}", $community
    Rename-Item $_.path $new_file_name
  }

  $irt_data = "$irt_path\services\app_data"
  
  Copy-Item $temp_dir\* $irt_data
  Remove-Item $temp_dir -recurse
  
  Write-Output "Configurations deployed successfully."
}
