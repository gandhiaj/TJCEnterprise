



CREATE proc dbo.prc_prtl_slc_dbmind01_usr_app_access
as
/**************************************************************************    
 Database      : DBPRC01    
 Proc Name     : prc_prtl_slc_dbmind01_usr_app_access    
 Filename      : prc_prtl_slc_dbmind01_usr_app_access.sql    
 Author        : Tom Ridings    
 Date          : 08/11/05
 Purpose       : select applications users have access to in SAD
 Requirement   :     
 Parameters :     
 Author  Date  Description    
----------------------------------------------------------------------    
Tom Ridings  08/11/05 Original version.    
D. Bhavsar   08/08/07 Use new view that uses ESA tables
D. Gropopi	02-20-2008 changed user_lgn_id to user_lgn_cd
Uday Danapal 06/12/13  modified DBMIND01 to DBMIND02 database.
***************************************************************************/    


SELECT 	a.user_lgn_cd AS 'erl_lgn_cd',
		a.apl_cd
		from  DBMIND02.dbo.tv_esa_sm_secr_pfl a  join DBMIND02.dbo.t_apl_web_lnk b on a.apl_cd = b.apl_cd 
order by  a.user_lgn_cd


return