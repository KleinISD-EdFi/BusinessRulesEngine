$SQLServer = "(local)"
$SQLDBName = "EdFi_Ods_Minimal_Template"
$location = Get-Location
$SubDirectory = "\RulesEngineMigrations\"

try{
    Write-Host "Starting Engine Migrations"

    Set-Location -Path  "$location$SubDirectory"
    foreach ($f in Get-ChildItem -path "$location$SubDirectory" -Filter *.sql)
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