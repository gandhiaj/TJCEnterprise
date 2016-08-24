





CREATE proc dbo.prc_prtl_slc_sad_usr_app_access
as
/**************************************************************************    
 Database      : DBPRC01    
 Proc Name     : prc_prtl_slc_sad_usr_app_access    
 Filename      : prc_prtl_slc_sad_usr_app_access.sql    
 Author        : Tom Ridings    
 Date          : 08/11/05
 Purpose       : select applications users have access to in SAD
 Requirement   :     
 Parameters :     
 Author  Date  Description    
----------------------------------------------------------------------    
Tom Ridings  08/11/05 Original version.    
***************************************************************************/    


SELECT 	a.usr_lgn_cd,
		a.apl_cd
		from  DBTRK02.dbo.t_usr_apl_secr_grp a  join DBTRK02.dbo.t_apl b on a.apl_cd = b.apl_cd 
order by  a.usr_lgn_cd

return