param
(
	[String] $AppFolderName, 	# eg. AppLinkADBatch
	[String] $TaskName,		# eg. AppLinkADBatch 
	[String] $NetName, 		# eg. TJC or JCIA
	[String] $EnvName,		# eg. DEV/UAT/PRD
	[String] $Receiver		# eg. agandhi@jointcommission.org, kent@jointcommission.org
)

Try
{
	[String] $AppFolderPath = "C:\$NetName\ScheduledTasks\$AppFolderName"
	[String] $AppBackupFolderName = "L:\Backups\$AppFolderName"	
	[int] $counter = 0
	
	Write-Output "********** Release: START **********"
	
	# Create the application directory
	if (!(Test-Path $AppFolderPath -ErrorAction Stop))
	{
		New-Item $AppFolderPath -type directory  | Out-Null -ErrorAction Stop
		$counter++
		Write-Output ("#" + $counter + ". Create the application directory")
	}
	
	# Create the application backup directory
	if (!(Test-Path $AppBackupFolderName -ErrorAction Stop))
	{
		New-Item $AppBackupFolderName -type directory | Out-Null -ErrorAction Stop
		$counter++
		Write-Output ("#" + $counter + ". Create the application backup directory")
	}

	# Take a backup
	Copy-Item $AppFolderPath "$AppBackupFolderName\Temp" -Recurse -ErrorAction Stop
	$counter++
	Write-Output ("#" + $counter + ". Take a backup")
	
	# Stop the task
	Stop-ScheduledTask -TaskName $NetName"\"$TaskName -ErrorAction Stop
	$counter++
	Write-Output ("#" + $counter + ". Stop the task")
	
	# Delete the application folder
	Remove-Item -Force -Recurse "$AppFolderPath\*" -ErrorAction Stop
	$counter++
	Write-Output ("#" + $counter + ". Delete the application folder")
	
	# Copy from build server to the application server
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
	$counter++
	Write-Output ("#" + $counter + ". Copy from build server to the application server")
	
	# Delete the local config file
	Remove-Item -Force -Recurse "$AppFolderPath\*.config" -Exclude "*_*.config" -ErrorAction Stop
	$counter++
	Write-Output ("#" + $counter + ". Delete the local config file")
	
	# Rename *_$EnvName.config to 'app_name' + .config
	$FileName = get-childitem $AppFolderPath -ErrorAction Stop | where {$_.extension -eq ".exe"}
	get-childitem -Path $AppFolderPath | where-object { $_.Name -like ("*_" + $EnvName + ".config") } | rename-item -NewName { if ($_.name.StartsWith("App","CurrentCultureIgnoreCase")) { $_.name -replace "App_$EnvName", $FileName } else { $_.name -replace "_$EnvName", "" } } -ErrorAction Stop
	$counter++
	Write-Output ("#" + $counter + ". Rename *_$EnvName.config to 'app_name' + .config")
	
	# Delete the other config files
	Remove-Item -Force -Recurse "$AppFolderPath\*_DEV.config" -ErrorAction Stop
	Remove-Item -Force -Recurse "$AppFolderPath\*_UAT.config" -ErrorAction Stop
	Remove-Item -Force -Recurse "$AppFolderPath\*_PRD.config" -ErrorAction Stop
	$counter++
	Write-Output ("#" + $counter + ". Delete the other config files")
	
	# Send success email
	Send-MailMessage -from "ReleaseManagement.Notification@microsoft.com" -to $Receiver -subject ($AppFolderName + " promotion has been completed successfully in " + $EnvName + " !!!") -body "Thank You." -smtpServer jcsendmail.jcaho.net -ErrorAction Stop
	$counter++
	Write-Output ("#" + $counter + ". Send success email")
	
	# Timestamp the backup folder using format 'AppName_MMDDYYYY_HHMMSS'
	if ($AppFolderName.Contains("\"))
	{		
		Rename-Item "$AppBackupFolderName\Temp" ($AppBackupFolderName + "\" + ($AppFolderName.Split("\")[1]) + "_" + [DateTime]::Now.ToString('MMddyyyy_HHmmss')) -ErrorAction Stop
	}
	else
	{
		Rename-Item "$AppBackupFolderName\Temp" ($AppBackupFolderName + "\" + $AppFolderName + "_" + [DateTime]::Now.ToString('MMddyyyy_HHmmss')) -ErrorAction Stop
	}
	$counter++
	Write-Output ("#" + $counter + ". Timestamp the backup folder using format 'AppName_MMDDYYYY_HHMMSS'")
	
	Write-Output "********** Release: END **********"
}
Catch [system.exception]
{
	[String] $ErrorMessage = $_.Exception.Message
	
	# Log error back to RM
	Write-Error $_.Exception.Message
		
	Try
	{
		Write-Output "********** Rollback: START **********"
		$counter = 0
		
		# Stop the task
		Stop-ScheduledTask -TaskName $NetName"\"$TaskName
		$counter++
		Write-Output ("#" + $counter + ". Stop the task")

		# Delete the application folder
		Remove-Item -Force -Recurse "$AppFolderPath\*" 
		$counter++
		Write-Output ("#" + $counter + ". Delete the application folder")
		
		# Copy from backup to application folder
		Copy-Item "$AppBackupFolderName\Temp\*" $AppFolderPath -recurse
		$counter++
		Write-Output ("#" + $counter + ". Copy from backup to application folder")
		
		# Delete the 'Temp' backup folder
		Remove-Item -Force -Recurse "$AppBackupFolderName\Temp"
		$counter++
		Write-Output ("#" + $counter + ". Delete the 'Temp' backup folder")
		
		# Send failure email
		Send-MailMessage -from "ReleaseManagement.Notification@microsoft.com" -to $Receiver -subject ($AppFolderName + " promotion failed in " + $EnvName + " !!!") -body "Promotion has been rolled back.`n`nError Details:`n$ErrorMessage" -smtpServer jcsendmail.jcaho.net
		$counter++
		Write-Output ("#" + $counter + ". Send failure email")		
				
		# Send an failure code to RM
		exit 1
		
		Write-Output "********** Rollback: END **********"
	}
	Catch [system.exception]
	{
		# Log error back to RM
		Write-Error $_.Exception.Message
		
		Write-Output "********** Rollback Failed: Further Steps **********"
		$counter = 0
		
		$ErrorMessage = $_.Exception.Message
		
		# Send failure email
		Send-MailMessage -from "ReleaseManagement.Notification@microsoft.com" -to $Receiver -subject ($AppFolderName + " promotion failed in " + $EnvName + " during rollback !!!") -body "Error Details:`n$ErrorMessage`n`nPlease contact helpdesk to rollback your application." -smtpServer jcsendmail.jcaho.net
		$counter++
		Write-Output ("#" + $counter + ". Send failure email")			    
				
		# Send an failure code to RM
		exit 1
	}
	
}
