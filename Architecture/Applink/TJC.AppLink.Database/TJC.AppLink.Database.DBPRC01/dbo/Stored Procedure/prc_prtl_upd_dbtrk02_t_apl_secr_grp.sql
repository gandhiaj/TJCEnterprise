


CREATE PROC dbo.prc_prtl_upd_dbtrk02_t_apl_secr_grp
(
	@apl_cd			u_apl_cd,
    @apl_secr_grp_id u_id,
	@apl_secr_grp_ds u_ds,
	@apl_secr_grp_nm u_nm
	
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
	INSERT INTO DBTRK02..t_apl_secr_grp
			(
			apl_cd,
		    apl_secr_grp_id,
		    apl_secr_grp_ds,
		    apl_secr_grp_nm
		    )
	VALUES
			(
			@apl_cd,
		    @apl_secr_grp_id,
		    @apl_secr_grp_ds,
		    @apl_secr_grp_nm
			)
		

RETURN