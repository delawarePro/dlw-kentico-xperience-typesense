<#
.Synopsis
    Updates local database data and schema to the version of the project's referenced Xperience NuGet package
#>

param (
    [string] $WorkspaceFolder = ".."
)
Import-Module (Join-Path $WorkspaceFolder "scripts/Utilities.psm1")

$connectionString = Get-ConnectionString $WorkspaceFolder

$resultDataSet = Execute-SQL-Data-Query -ConnectionString $connectionString -CommandText "SELECT KeyValue FROM CMS_SettingsKey WHERE KeyName = N'CMSEnableCI'"

$isUsingCD = $resultDataSet.Tables[0].Rows[0][0]

$readyToUpdate = $True

#Since the settings key value is a string and could theoretically be something other than true or false, compare the value rather than treating it as a boolean expression on its own
if($isUsingCD -eq 'True'){
    Write-Host 'Disabling continuous integration'
    $commandResult = Execute-SQL-Command -ConnectionString $connectionString -CommandText "UPDATE CMS_SettingsKey SET KeyValue = N'False' WHERE KeyName = N'CMSEnableCI'"
    $readyToUpdate = $commandResult
}

if($readyToUpdate){
    Write-Host 'Starting Xperience update'
    $projectPath = Get-WebProjectPath $WorkspaceFolder
    $configuration = "Release";
    if (Test-Path (Join-Path $projectPath "bin\Debug"))
    {
        $configuration = "Debug";
    }
    dotnet run --project $projectPath -c $configuration --no-build --no-restore --kxp-update --skip-confirmation
    if ($LASTEXITCODE -ne 0) {
        Handle-Error "Update failed."
    }
}
else{
    Handle-Error 'Unable to disable continuous integration to perform the update.'
}

if($isUsingCD -eq 'True'){
    Write-Host 'Re-enabling continuous integration'

    $commandResult = Execute-SQL-Command -ConnectionString $connectionString -CommandText "UPDATE CMS_SettingsKey SET KeyValue = N'True' WHERE KeyName = N'CMSEnableCI'"

    if(-not $commandResult){
        Handle-Error 'Unable to re-enable continuous integration.'
    }
}
Write-Host "Update Complete"

# & (Join-Path $WorkspaceFolder "scripts/Store-CI.ps1") -WorkspaceFolder $WorkspaceFolder