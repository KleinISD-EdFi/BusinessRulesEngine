$odsDatabaseRegEx = 'EdFi[_]Ods[_]\d{4}$'
$dataSource = "localhost"
$odsList = @()
$sqlScript = "C:\Users\a.talpur\Desktop\Klein\Klein rules engine sql\Update-Ods-For-Rules-Engine.sql"

$Server = New-Object Microsoft.SqlServer.Management.Smo.Server("$dataSource")


foreach($server in $Server.Databases){
    if($server.Name -match $odsDatabaseRegEx){
        $odsList += $server.Name
    }       
}

Write-Host "OdsList = $odsList"

foreach($ods in $odsList){
    $connectionString = "Data Source=$dataSource;Initial Catalog=$ods;Integrated Security=true;"
    Write-Host "Running Update-Ods-For-Rules-Engine.sql on $ods"
    Invoke-Sqlcmd -ConnectionString $connectionString -InputFile $sqlScript
}