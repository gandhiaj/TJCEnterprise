


CREATE PROC dbo.prc_prtl_upd_dbtrk02
(
	@usr_lgn_cd		char(100),
	@apl_cd		u_apl_cd,
	@apl_secr_grp_id u_id
	
)
as
/**************************************************************************
 Database     	: DBPRC01
 Proc Name    	: prc_prtl_upd_dbtrk02
 Filename     	: prc_prtl_upd_dbtrk02
 Author       	: Tom Ridings
 Date         	: 08/12/05
 Purpose      	: Update AppLink application access
 Requirement  	: 
 Parameters	    :  usr_lgn_cd,
				   app_acrn,
                   
 Returns	    :  Nothing.
 Updates:
 
 Author		                Date		Description
 ----------------------------------------------------------------------
 Tom Ridings					08/12/2005	Original Version
***************************************************************************/
	INSERT INTO DBTRK02..t_usr_apl_secr_grp
			(
		    usr_lgn_cd,
			apl_cd,
			apl_secr_grp_id
			)
	VALUES
			(
			@usr_lgn_cd,
			@apl_cd,
			@apl_secr_grp_id
			)
		

RETURN