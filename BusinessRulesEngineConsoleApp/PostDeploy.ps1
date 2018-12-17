$SQLServer = "#{Ods.DatabaseServer.Name}"
$SQLDBName = "#{Ods.Database.Name}"
$MigrationsDirectory = "#{EngineMigrationsDirectory}"

Write-Host "Starting Engine Migrations"
$Error.Clear()

Set-Location -Path  "$MigrationsDirectory"
foreach ($f in Get-ChildItem -path "$MigrationsDirectory" -Filter *.sql){
    Write-Host "$f"
    Invoke-Sqlcmd -InputFile "$f" -ServerInstance "$SQLServer" -Database "$SQLDBName" -Verbose
}

if($Error.Count -gt 0){
    Write-Host $Error.Count
    throw "Error running migration scripts, please resolve errors."
}