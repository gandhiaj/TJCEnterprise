



CREATE PROC dbo.prc_prtl_slc_lgncd_by_rscid
(
	@rsc_id		u_rsc_id

)
as
/**************************************************************************
 Database     	: DBPRC01
 Proc Name    	: prc_prtl_slc_lgncd_by_rscid
 Filename     	: prc_prtl_slc_lgncd_by_rscid.sql
 Author       	: Tom Ridings
 Date         	: 08/12/05
 Purpose      	: Select login id by rsc_id
 Requirement  	: 
 Parameters	    :  rsc_id
				   
                   
 Returns	    :  lgn_cd
 Updates:
 
 Author		                Date		Description
 ----------------------------------------------------------------------
 Tom Ridings					08/12/2005	Original Version
***************************************************************************/


Select 
lgn_cd,
rsc_frs_nm,
rsc_lst_nm,
pro_crd_typ_cd
 FROM DBEMP01.dbo.t_rsc 
WHERE rsc_id = @rsc_id

RETURN