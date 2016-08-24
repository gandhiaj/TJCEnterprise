


CREATE PROC dbo.prc_prtl_remove_usr_app_access
(
	@apl_grp_nm		char(10)

)
as
/**************************************************************************
 Database     	: DBPRC01
 Proc Name    	: prc_prtl_slc_remove_app_access
 Filename     	: prc_prtl_slc_remove_app_access.sql
 Author       	: Tom Ridings
 Date         	: 08/12/05
 Purpose      	: Remove application access
 Requirement  	: 
 Parameters	    :  usr_lgn_cd,
				   
                   
 Returns	    :  DataSet.
 Updates:
 
 Author		                Date		Description
 ----------------------------------------------------------------------
 Tom Ridings					08/12/2005	Original Version
***************************************************************************/


DELETE FROM DBTRK02.dbo.t_jc_application_access 
WHERE apl_grp_nm = @apl_grp_nm

RETURN