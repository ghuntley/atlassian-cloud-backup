# Atlassian Cloud Backup

Atlassian take point in time backups every 24 hours of your data for the purposes of recovery of application failure. Atlassian claim that these backups are kept seperate from the data centre in which the instance runs and that using these snapshots cannot be used to roll back changes to application data. Thus is tool is born, it integrates in with the Atlassian Cloud and will export your data from their cloud onto your local computer.

# Getting Started


	> AtlassianCloudBackup.exe -h
	Copyright (C) 2014 Geoffrey Huntley <ghuntley@ghuntley.com>
	
	Usage: AtlassianCloudBackup --destination C:\backups\ --sleep 600 --instance https://yourinstance.atlassian.net --username admin --password password
	
	  -d, --destination    Required. Destination directory where backups will be written to.
	  -s, --sleep          Required. Amount of time in seconds to sleep after reqeusting a backup before download is started. Increase if your instance is large.
	  -i, --instance       Required. Atlassian Cloud instance url.
	  -u, --username       Required. Atlassian Cloud username with administrative privledges.
	  -p, --password       Required. Password for administrative account.
	  --help               Display this help screen.

# Usage

	> AtlassianCloudBackup.exe --destination C:\backups --sleep 10 --instance https://removed.atlassian.net --username removed --password removed
	2014-12-27 20:13:59 [Information] Started backup.
	2014-12-27 20:13:59 [Information] Signing in as "removed" at "removed"
	2014-12-27 20:14:01 [Information] "removed" has successfully authenticated.
	2014-12-27 20:14:01 [Information] Requesting backup of JIRA.
	2014-12-27 20:14:02 [Information] Backup request successful.
	2014-12-27 20:14:02 [Information] Requesting backup of Confluence.
	2014-12-27 20:14:03 [Warning] Backup frequency is limited. You can not make another backup right now. Approximate time till next allowed backup: 45h 0m
	2014-12-27 20:14:03 [Information] Sleeping for 10 seconds before starting download.
	2014-12-27 20:14:13 [Information] Beginning operation "Download": "https://removed.atlassian.net/webdav/backupmanager/JIRA-backup-20141227.zip"
	2014-12-27 20:14:15 [Information] Completed operation "Download": "https://removed.atlassian.net/webdav/backupmanager/JIRA-backup-20141227.zip" in 00:00:01.4421289 (1442 ms)
	2014-12-27 20:14:15 [Information] Beginning operation "Download": "https://removed.atlassian.net/webdav/backupmanager/Confluence-backup-20141227.zip"
	2014-12-27 20:14:29 [Information] Completed operation "Download": "https://removed.atlassian.net/webdav/backupmanager/Confluence-backup-20141227.zip" in 00:00:14.7953145 (14795 ms)
	2014-12-27 20:14:29 [Information] Finished backup.
