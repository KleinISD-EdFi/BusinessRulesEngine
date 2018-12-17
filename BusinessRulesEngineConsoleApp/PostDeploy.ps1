$SQLServer = "#{Ods.DatabaseServer.Name}"
$SQLDBName = "#{Ods.Database.Name}"
$MigrationsDirectory = "#{EngineMigrationsDirectory}"

try{
    Write-Host "Starting Engine Migrations"

    Set-Location -Path  "$MigrationsDirectory"
    foreach ($f in Get-ChildItem -path "$MigrationsDirectory" -Filter *.sql)
    {
        Write-Host "$f"
        Invoke-Sqlcmd -InputFile "$f" -ServerInstance "$SQLServer" -Database "$SQLDBName" -Verbose
    }
}
catch{
    $ErrorMessage = $_.Exception.Message
    $FailedItem = $_.Exception.ItemName
    Write-Error "$ErrorMessage"
}