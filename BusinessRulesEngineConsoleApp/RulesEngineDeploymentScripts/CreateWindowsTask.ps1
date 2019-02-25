$businessRulesExeLocation = $OctopusParameters['Octopus.Action[Deploy Console Application].Output.Package.InstallationDirectoryPath'] + "BusinessRulesEngineConsoleApp.exe"
$businessRulesTaskName = $OctopusParameters['RulesEngine.TaskName']
$runAtTime = $OctopusParameters['RulesEngine.RunAtTime']
$frequency = $OctopusParameters['RulesEngine.Frequence']

$taskExists = Get-ScheduledTask | Where-Object {$_.TaskName -like $businessRulesTaskName }

if($taskExists) {
    Write-Host "Task: $businessRulesTaskName exists, updating task to point to new path."
    Write-Host "Path: $businessRulesExeLocation"
    
    Get-ScheduledTaskInfo -TaskName $businessRulesTaskName 

    $A = New-ScheduledTaskAction -Execute $businessRulesExeLocation
    $P = New-ScheduledTaskPrincipal "sys_doubleline"
    $S = New-ScheduledTaskSettingsSet
    Set-ScheduledTask -TaskName $businessRulesTaskName -Action $A -Settings $S -Principal $P
}
else {
    Write-Host "Task: $businessRulesTaskName does not exist, creating new task."
    Write-Host "Path: $businessRulesExeLocation"
	
    $A = New-ScheduledTaskAction -Execute $businessRulesExeLocation
    $T = New-ScheduledTaskTrigger -At $runAtTime -$frequency
    $P = New-ScheduledTaskPrincipal "sys_doubleline"
    $S = New-ScheduledTaskSettingsSet
    $D = New-ScheduledTask -Action $A -Principal $P -Trigger $T -Settings $S
    Register-ScheduledTask $businessRulesTaskName -InputObject $D
}
