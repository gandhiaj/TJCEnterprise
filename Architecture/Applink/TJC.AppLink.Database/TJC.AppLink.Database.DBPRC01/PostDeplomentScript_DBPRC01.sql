/*
Post-Deployment Script Template							
--------------------------------------------------------------------------------------
 This file contains SQL statements that will be appended to the build script.		
 Use SQLCMD syntax to include a file in the post-deployment script.			
 Example:      :r .\myfile.sql								
 Use SQLCMD syntax to reference a variable in the post-deployment script.		
 Example:      :setvar TableName MyTable							
               SELECT * FROM [$(TableName)]					
--------------------------------------------------------------------------------------
*/

GRANT EXEC ON prc_apl_link_slc_info TO PROC_ROLE 
GO
GRANT EXEC ON dbo.prc_prtl_remove_usr_app_access TO PROC_ROLE
GO
GRANT EXEC ON dbo.prc_prtl_slc_dbmind01_usr_app_access TO PROC_ROLE
GO
GRANT EXEC ON dbo.prc_prtl_slc_dbtrk02_app_access_by_ad_grp TO PROC_ROLE
GO
GRANT EXEC ON dbo.prc_prtl_slc_lgncd_by_rscid TO PROC_ROLE
GO
GRANT EXEC ON dbo.prc_prtl_slc_sad_usr_app_access TO PROC_ROLE
GO
GRANT EXEC ON dbo.prc_prtl_upd_dbtrk02 TO PROC_ROLE
GO
GRANT EXEC ON dbo.prc_prtl_upd_dbtrk02_t_apl_secr_grp TO PROC_ROLE
GO