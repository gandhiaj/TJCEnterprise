param
(
	[String] $PackageLocation, 	# eg. \\jcprdtfsbld1\d$\Builds\BD_AppLinkADBatch_Main\BD_AppLinkADBatch_Main_20151223.2\
	[String] $AppFolderName, 	# eg. AppLinkADBatch
	[String] $TaskName,		# eg. AppLinkADBatch 
	[String] $NetName, 		# eg. TJC or JCIA
	[String] $EnvName,		# eg. DEV/UAT/PRD
	[String[]] $Receiver		# eg. "agandhi@jointcommission.org","kent@jointcommission.org"
)

Try
{
	[String] $AppFolderPath = "C:\$NetName\ScheduledTasks\$AppFolderName"
	[String] $AppBackupFolderName = "L:\Backups\$AppFolderName"	

	# Create the application backup directory
	if (!(Test-Path $AppBackupFolderName -ErrorAction Stop))
	{
		#New-Item $AppBackupFolderName -type directory -ErrorAction Stop
	}

	# Take a backup
	#Copy-Item $AppFolderPath "$AppBackupFolderName\Temp" -Recurse -ErrorAction Stop

	# Stop the task
	#Stop-ScheduledTask -TaskName $NetName"\"$TaskName -ErrorAction Stop

	# Delete the application folder
	Remove-Item -Force -Recurse "$AppFolderPath\*" -ErrorAction Stop

	# Copy from most recent build to the application server
	[Int] $maxDepth = 1

	function TraverseFolders($folder, $remainingDepth) {
		Get-ChildItem $folder | ForEach-Object {
			if ($remainingDepth -gt 1) 
			{
				TraverseFolders $_.FullName ($remainingDepth - 1)
			}
			else
			{
				Copy-Item $_.FullName $AppFolderPath -Recurse -ErrorAction Stop
			}
		}
	}

	TraverseFolders $PackageLocation $maxDepth
	
	# Delete the local config file
	Remove-Item -Force -Recurse "$AppFolderPath\*.config" -Exclude "*_*.config" -ErrorAction Stop
	
	# Rename *_$EnvName.config to 'app_name' + .config
	$FileName = get-childitem $AppFolderPath -recurse -ErrorAction Stop | where {$_.extension -eq ".exe"}
	get-childitem -Path $AppFolderPath -ErrorAction Stop | where-object { $_.Name -like ("*_" + $EnvName + ".config") } | rename-item -NewName { if ($_.name.StartsWith("App","CurrentCultureIgnoreCase")) { $_.name -replace "App_$EnvName", $FileName } else { $_.name -replace "_$EnvName", "" } } -ErrorAction Stop
	
	# Delete the other config files
	Remove-Item -Force -Recurse "$AppFolderPath\*_*.config" -ErrorAction Stop

	# Send success email
	Send-MailMessage -from "ReleaseManagement.Notification@microsoft.com" -to $Receiver -subject ($AppFolderName + " promotion has been completed successfully in " + $EnvName + " !!!") -body "Thank You." -smtpServer jcsendmail.jcaho.net -ErrorAction Stop
	
	# Timestamp the backup folder using format 'AppName_MMDDYYYY_HHMMSS'
	#Rename-Item "$AppBackupFolderName\Temp" ($AppBackupFolderName + "\" + $AppFolderName + "_" + [DateTime]::Now.ToString('MMddyyyy_HHmmss')) -ErrorAction Stop
	
}
Catch [system.exception]
{
	[String] $ErrorMessage = $_.Exception.Message
	
	Try
	{
		# Stop the task
		#Stop-ScheduledTask -TaskName $NetName"\"$TaskName

		# Delete the application folder
		Remove-Item -Force -Recurse "$AppFolderPath\*" 

		# Copy from backup to application folder
		#Copy-Item "$AppBackupFolderName\Temp\*" $AppFolderPath -recurse

		# Delete the 'Temp' backup folder
		#Remove-Item -Force -Recurse "$AppBackupFolderName\Temp"
	
		# Send failure email
		Send-MailMessage -from "ReleaseManagement.Notification@microsoft.com" -to $Receiver -subject ($AppFolderName + " promotion failed in " + $EnvName + " !!!") -body "Promotion has been rolled back.`n`nError Details:`n$ErrorMessage" -smtpServer jcsendmail.jcaho.net
		
		# Log error back to RM
		Write-Error $_.Exception.Message    
				
		# Send an failure code to RM
		exit 1
	}
	Catch [system.exception]
	{
		$ErrorMessage = $_.Exception.Message
		
		# Send failure email
		Send-MailMessage -from "ReleaseManagement.Notification@microsoft.com" -to $Receiver -subject ($AppFolderName + " promotion failed in " + $EnvName + " during rollback !!!") -body "Error Details:`n$ErrorMessage`n`nPlease contact helpdesk to rollback your application." -smtpServer jcsendmail.jcaho.net
		
		# Log error back to RM
		Write-Error $_.Exception.Message    
				
		# Send an failure code to RM
		exit 1
	}
	
}
