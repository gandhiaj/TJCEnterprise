CREATE PROCEDURE [dbo].[prc_apl_link_slc_info]
(
	@usr_lgn_cd		u_rsc_lgn_cd
)
AS

/****************************************************************************************
 Database     : DBPRC01
 Proc Name    : [prc_apl_link_slc_info]
 Author       : Ajay Gandhi
 Date         : 11/20/2015
 Purpose      : Used to retrieve the app links for a user
*****************************************************************************************
**		Change History
*****************************************************************************************
 Date			Auhtor				Description
 11/20/2015		Ajay Gandhi			New SP
 11/23/2015		Ajay Gandhi			Added one more column 'apl_typ_nm'
 12/29/2015		Ajay Gandhi			Changed the select clause to get the application type indicator
****************************************************************************************/

BEGIN

DECLARE @tbl TABLE(usr_lgn_cd char(20), apl_cd char(16), apl_nm char(60), apl_url_tx varchar(255), apl_type_ind char(1))

INSERT INTO @tbl
SELECT DISTINCT ltrim(rtrim(a.usr_lgn_cd)),
				ltrim(rtrim(a.apl_cd)),
				'',
				'',
				''
FROM DBTRK02.dbo.t_jc_application_access a
WHERE a.usr_lgn_cd = @usr_lgn_cd

DELETE tbl
FROM @tbl tbl
INNER JOIN DBTRK02.dbo.t_jc_application_url app
ON rtrim(tbl.apl_cd) = rtrim(app.apl_cd)
WHERE app.apl_active_fl = 'N'

UPDATE @tbl 
SET apl_nm = ltrim(rtrim(a.apl_nm)),
	apl_url_tx = ltrim(rtrim(a.apl_url)),
	apl_type_ind = ltrim(rtrim(a.apl_type_ind))
FROM  @tbl tbl
INNER JOIN DBTRK02.dbo.t_jc_application_url a
ON a.apl_cd = tbl.apl_cd

SELECT usr_lgn_cd, apl_cd, apl_nm, apl_url_tx, apl_type_ind
FROM @tbl
ORDER BY apl_nm	

RETURN 

END
