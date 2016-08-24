


CREATE PROC dbo.prc_prtl_slc_dbtrk02_app_access_by_ad_grp
(
	@ad_grp		char(20)
)
as
/**************************************************************************
 Database     	: DBPRC01
 Proc Name    	: prc_prtl_slc_dbtrk02_app_access_by_ad_grp
 Filename     	: prc_prtl_slc_dbtrk02_app_access_by_ad_grp.sql
 Author       	: Tom Ridings
 Date         	: 08/12/05
 Purpose      	: Select application access from DBTRK02 by AD group
 Requirement  	: 
 Parameters	    :  ad_grp,
                   
 Returns	    :  DataSet.
 Updates:
 
 Author		                Date		Description
 ----------------------------------------------------------------------
 Tom Ridings					08/12/2005	Original Version
***************************************************************************/


SELECT 	a.usr_lgn_cd,
		a.apl_cd
		from  DBTRK02.dbo.t_usr_apl_secr_grp a
		where a.usr_lgn_cd = @ad_grp
order by  a.usr_lgn_cd



RETURN