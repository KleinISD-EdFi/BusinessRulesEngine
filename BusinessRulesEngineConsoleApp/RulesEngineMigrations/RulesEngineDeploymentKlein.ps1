[System.Reflection.Assembly]::LoadWithPartialName('Microsoft.SqlServer.SMO')

$odsDatabaseRegEx = $OctopusParameters['Ods.DatabaseNameRegex']
$dataSource = $OctopusParameters['DatabaseServer.Name']
$sqlScript = $OctopusParameters['EngineMigrationsDirectory']
$odsList = @()

$Server = New-Object Microsoft.SqlServer.Management.Smo.Server("$dataSource")

foreach($server in $Server.Databases){
    if($server.Name -match $odsDatabaseRegEx){
        $odsList += $server.Name
    }       
}

foreach($ods in $odsList){
    $connectionString = "Data Source=$dataSource;Initial Catalog=$ods;Integrated Security=true;"
    Write-Host "Running Update-Ods-For-Rules-Engine.sql on $ods"
    Invoke-Sqlcmd -ConnectionString $connectionString -InputFile $sqlScript
}