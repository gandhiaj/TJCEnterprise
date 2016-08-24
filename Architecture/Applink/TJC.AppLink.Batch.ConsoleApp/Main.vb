Imports System.Configuration
Imports System
Imports System.Data.SqlClient
Imports System.Data
Imports System.Data.Common
Imports Microsoft.Practices.EnterpriseLibrary.Data
Imports Microsoft.Practices.EnterpriseLibrary.Logging
Imports Microsoft.Practices.EnterpriseLibrary.ExceptionHandling
Imports System.DirectoryServices
Imports Microsoft.Practices.EnterpriseLibrary.Common.Configuration

Module Main

    Dim alu As AppLinkUpdate

    Sub Main()

        Try
            DatabaseFactory.SetDatabaseProviderFactory(New DatabaseProviderFactory())
            Dim config As IConfigurationSource = ConfigurationSourceFactory.Create()
            Dim logFactory As LogWriterFactory = New LogWriterFactory(config)
            Logger.SetLogWriter(logFactory.Create())

            Dim factory As ExceptionPolicyFactory = New ExceptionPolicyFactory(config)
            ExceptionPolicy.SetExceptionManager(factory.CreateManager())

            ' make sure the event log has our folder...
            'If Not EventLog.SourceExists("AppLinkBatch") Then
            '    EventLog.CreateEventSource("AppLinkBatch", "AppLinkBatch")
            'End If

            Dim logEntry As LogEntry = New LogEntry()
            logEntry.EventId = 808
            logEntry.Priority = 1
            logEntry.Message = "Processing has started..."
            logEntry.Categories.Add("EventLog") 'write message to the event log.
            Logger.Write(logEntry)

            alu = New AppLinkUpdate
            If Today.DayOfWeek <> DayOfWeek.Sunday Then
                alu.LoadActiveDirectInfo()
            End If

            logEntry.EventId = 809
            logEntry.Priority = 1
            logEntry.Message = "Processing has completed..."
            logEntry.Categories.Add("EventLog") 'write message to the event log.
            Logger.Write(logEntry)

        Catch ex As Exception
            ExceptionPolicy.HandleException(ex, "Critical")
        End Try
    End Sub

    Public Class AppLinkUpdate
        Public sADgrp As String
        Dim sID As String
        Dim sConn As String = "LDAP://JCRIS2"
        Dim sPWD As String
        Dim sUserid As String
        Dim oRoot As DirectoryEntry = New DirectoryEntry(sConn)
        Dim outterSearcher As DirectorySearcher = New DirectorySearcher(oRoot)
        Dim outterResults As SearchResultCollection
        Dim outterResult As SearchResult
        Dim outterProp As ResultPropertyCollection
        Dim outterCnt As Integer
        Dim sData As String = ""
        Dim sFilter As String

        Public Function LoadActiveDirectInfo() As String
            Dim db As Database
            Dim dc As DbCommand
            ' search Active Directory for all users
            Try
                sFilter = "(&(objectCategory=person)(objectClass=user))"
                outterSearcher.SearchScope = SearchScope.Subtree
                outterSearcher.Filter = sFilter
                outterSearcher.PropertiesToLoad.Add("name")
                outterSearcher.PropertiesToLoad.Add("memberof")
                outterSearcher.PropertiesToLoad.Add("userprincipalname")
                outterSearcher.PropertiesToLoad.Add("displayname")
                outterSearcher.PropertiesToLoad.Add("primarygroupid")
                outterSearcher.PropertiesToLoad.Add("objectsid")
                outterSearcher.PropertiesToLoad.Add("adspath")
                outterSearcher.PropertiesToLoad.Add("cn")
                outterResults = outterSearcher.FindAll

                'Call function to remove entries from DBTRK02...t_jc_application_access
                deleteDBTRK02()

                'inner loop through outterresults and find groups users is member
                For Each Me.outterResult In outterResults
                    Try
                        For Me.outterCnt = 0 To outterResult.Properties("name").Count - 1
                            Dim innerSearcher As DirectorySearcher = New DirectorySearcher(oRoot)
                            Dim innerData As String = ""
                            Dim innerResults As SearchResultCollection
                            Dim innerResult As SearchResult
                            Dim innerCnt As Integer
                            Dim innerFilter As String
                            Dim sADgrpString
                            Dim adspath As String

                            sUserid = outterResult.Properties("name")(outterCnt)
                            adspath = outterResult.Properties("adspath")(outterCnt)

                            innerFilter = "( Name=" + sUserid + ")"
                            innerSearcher.SearchScope = SearchScope.Subtree
                            innerSearcher.Filter = innerFilter
                            innerSearcher.PropertiesToLoad.Add("memberof")
                            innerSearcher.PropertiesToLoad.Add("primarygroupid")
                            innerSearcher.PropertiesToLoad.Add("objectsid")
                            innerSearcher.PropertiesToLoad.Add("objectcategory")
                            innerResults = innerSearcher.FindAll

                            Dim istartgrpnm As Integer = 0
                            Dim iendgrpnm As Integer = 0
                            Dim istartaplnm As Integer = 3
                            Dim iendaplnm As Integer = 0

                            Dim PrimaryGroupID As Integer

                            Try

                                PrimaryGroupID = outterResult.Properties("primarygroupid")(outterCnt)

                                'Call function to get Primary Active Directory Group 
                                getPrimaryActiveDirectoryGroup(PrimaryGroupID, outterResult, outterCnt)

                                'Query DBTRK02 to determine if primary group used to gain application access

                                Dim dsSAD1adGrpAccess As DataSet
                                db = DatabaseFactory.CreateDatabase("DBPRC01")
                                dc = db.GetStoredProcCommand("prc_prtl_slc_dbtrk02_app_access_by_ad_grp")
                                db.AddInParameter(dc, "@ad_grp", DbType.String, sADgrp.ToLower)
                                dsSAD1adGrpAccess = db.ExecuteDataSet(dc)
                                dc.Parameters.Clear()

                                '                        p4 = New SqlParameter("@ad_grp", sADgrp.ToLower)
                                '                        dsSAD1adGrpAccess = SqlHelper.ExecuteDataset(sadSQLconn, CommandType.StoredProcedure, "prc_prtl_slc_dbtrk02_app_access_by_ad_grp", p4)

                                If Not (dsSAD1adGrpAccess Is Nothing) And dsSAD1adGrpAccess.Tables.Count > 0 Then
                                    Dim _row As DataRow
                                    For Each _row In dsSAD1adGrpAccess.Tables(0).Rows
                                        dc = db.GetStoredProcCommand("prc_prtl_upd_usr_app_access")
                                        db.AddInParameter(dc, "@usr_lgn_cd", DbType.String, sUserid.ToLower)
                                        db.AddInParameter(dc, "@apl_acrn", DbType.String, _row("apl_cd"))
                                        db.AddInParameter(dc, "@apl_nm", DbType.String, "IntraNet")
                                        db.AddInParameter(dc, "@apl_grp_nm", DbType.String, "SAD")
                                        db.ExecuteNonQuery(dc)
                                        dc.Parameters.Clear()
                                    Next _row
                                End If
                            Catch

                            End Try

                            ' loop through and get groups user is member of and check DBTRK02.dbo.t_usr_apl_secr_grp to see if AD groups provide access to application(s)
                            Try
                                Dim objcat As String
                                For Each innerResult In innerResults
                                    objcat = innerResult.Properties("objectcategory").Item(0)

                                    If innerResult.Properties("memberof").Count > 0 Then

                                        For innerCnt = 0 To innerResult.Properties("memberof").Count - 1
                                            sADgrpString = innerResult.Properties("memberof")(innerCnt)
                                            istartgrpnm = sADgrpString.indexof("=") + 1
                                            iendgrpnm = sADgrpString.indexof(",") - 3
                                            iendaplnm = sADgrpString.indexof(" ") - 2
                                            sADgrp = sADgrpString.substring(istartgrpnm, iendgrpnm)
                                            CheckAppAccessByADgroup(sADgrp)
                                        Next
                                    End If
                                Next

                            Catch ex As Exception

                            Finally
                                innerResult = Nothing
                                innerResults = Nothing
                                innerSearcher = Nothing
                            End Try
                        Next

                    Catch ex As Exception
                        Return False
                        ExceptionPolicy.HandleException(ex, "Critical")
                        'Throw New BaseApplicationException(ex.Message.ToString)
                    End Try

                Next outterResult
                outterResult = Nothing
                outterResults = Nothing
                outterSearcher = Nothing
                oRoot = Nothing

                'Load Intranet application access controlled DBTRK02.dbo.t_usr_apl_secr_grp
                getAccessSAD()

                'Load ExtraNet application access controlled by DBMIND01.dbo.tv_sm_secr_pfl
                getAccessDBMIND01()

                'Load Application access control by Active Directory 
                getAccessAD()
                Return True
            Catch ex As Exception
                ExceptionPolicy.HandleException(ex, "Critical")
            End Try

        End Function
        Sub deleteDBTRK02()

            Dim db As Database
            Dim dc As DbCommand

            Try
                db = DatabaseFactory.CreateDatabase("DBPRC01")

                ' Remove users from dbtrk02.t_jc_application_access
                dc = db.GetStoredProcCommand("prc_prtl_remove_usr_app_access")
                db.AddInParameter(dc, "@apl_grp_nm", DbType.String, "VSNET")
                db.ExecuteNonQuery(dc)
                dc.Parameters.Clear()

                ' Remove user from dbtrk02.t_jc_application_access  (applications controlled by SAD)
                dc = db.GetStoredProcCommand("prc_prtl_remove_usr_app_access")
                db.AddInParameter(dc, "@apl_grp_nm", DbType.String, "SAD")
                db.ExecuteNonQuery(dc)
                dc.Parameters.Clear()

                ' Remove user from dbtrk02.t_jc_application_access  (applications controlled by SiteMinder)
                dc = db.GetStoredProcCommand("prc_prtl_remove_usr_app_access")
                db.AddInParameter(dc, "@apl_grp_nm", DbType.String, "SiteMinder")
                db.ExecuteNonQuery(dc)
                dc.Parameters.Clear()

            Catch ex As Exception
                ExceptionPolicy.HandleException(ex, "Critical")
                Throw
            End Try

        End Sub
        Public Function getPrimaryActiveDirectoryGroup(ByVal PrimaryGroupID As Integer, ByVal outterresult As Object, ByVal outtercnt As Integer)

            Dim istartgrpnm As Integer
            Dim iendgrpnm As Integer
            Dim objectsid As Byte()

            objectsid = outterresult.Properties("objectsid")(outtercnt)
            ' getting object SID
            Dim escapedGroupSid As New System.Text.StringBuilder
            'Copy over everything but the last four bytes(sub-authority/RID)
            'Doing so gives a the prefix SID for objects in the user's domain
            Dim ii As Integer
            For ii = 0 To (objectsid.Length - 4) - 1
                escapedGroupSid.AppendFormat("\{0:x2}", objectsid(ii))
            Next ii
            ' end getting object SID


            'Add the primaryGroupID(RID) to the escape string to build the SID of the primaryGroup()
            For ii = 0 To 3
                escapedGroupSid.AppendFormat("\{0:x2}", PrimaryGroupID And &HFF)
                PrimaryGroupID = PrimaryGroupID / (2 ^ 8) 'Move on to the next byte
            Next ii

            Dim sSIDid As String

            sSIDid = escapedGroupSid.ToString
            Dim searcher As New DirectorySearcher
            searcher.Filter = "(&(objectCategory=Group)(objectSID=" + escapedGroupSid.ToString + "))"
            searcher.PropertiesToLoad.Add("distinguishedName")

            Dim sprimarygroupname As String

            sprimarygroupname = CStr(searcher.FindOne().Properties("distinguishedName")(0))
            istartgrpnm = sprimarygroupname.IndexOf("=") + 1
            iendgrpnm = sprimarygroupname.IndexOf(",") - 3
            sADgrp = sprimarygroupname.Substring(istartgrpnm, iendgrpnm)

            Return sADgrp

        End Function

        Sub CheckAppAccessByADgroup(ByVal sADgrpString As String)

            Dim db As Database
            Dim dc As DbCommand
            Dim dsSAD2adGrpAccess As DataSet
            Dim i As Integer = 0
            Dim row As DataRow

            Try
                db = DatabaseFactory.CreateDatabase("DBPRC01")
                dc = db.GetStoredProcCommand("prc_prtl_slc_dbtrk02_app_access_by_ad_grp")
                db.AddInParameter(dc, "@ad_grp", DbType.String, sADgrpString.ToLower)
                dsSAD2adGrpAccess = db.ExecuteDataSet(dc)
                dc.Parameters.Clear()

                dc = db.GetStoredProcCommand("prc_prtl_upd_usr_app_access")

                For Each row In dsSAD2adGrpAccess.Tables(0).Rows
                    db.AddInParameter(dc, "@usr_lgn_cd", DbType.String, sUserid.ToLower)
                    db.AddInParameter(dc, "@apl_acrn", DbType.String, row("apl_cd"))
                    db.AddInParameter(dc, "@apl_nm", DbType.String, "IntraNet")
                    db.AddInParameter(dc, "@apl_grp_nm", DbType.String, "SAD")
                    db.ExecuteNonQuery(dc)
                    dc.Parameters.Clear()
                Next

            Catch ex As Exception
                ExceptionPolicy.HandleException(ex, "Critical")
            End Try

        End Sub
        Sub getAccessSAD()

            Dim db As Database
            Dim dc As DbCommand
            Dim dsSAD As DataSet
            Dim i As Integer = 0

            Try

                db = DatabaseFactory.CreateDatabase("DBPRC01")
                dc = db.GetStoredProcCommand("prc_prtl_slc_sad_usr_app_access")

                dsSAD = db.ExecuteDataSet(dc)

                If Not (dsSAD Is Nothing) And dsSAD.Tables.Count > 0 Then
                    Dim _row As DataRow
                    dc = db.GetStoredProcCommand("prc_prtl_upd_usr_app_access")
                    For Each _row In dsSAD.Tables(0).Rows

                        db.AddInParameter(dc, "@usr_lgn_cd", DbType.String, _row("usr_lgn_cd".ToLower))
                        db.AddInParameter(dc, "@apl_acrn", DbType.String, _row("apl_cd"))
                        db.AddInParameter(dc, "@apl_nm", DbType.String, "IntraNet")
                        db.AddInParameter(dc, "@apl_grp_nm", DbType.String, "SAD")

                        db.ExecuteNonQuery(dc)
                        dc.Parameters.Clear()

                        i = i + 1
                        If i >= dsSAD.Tables(0).Rows.Count Then
                            Exit Sub
                        End If

                    Next _row
                End If

            Catch ex As Exception
                ExceptionPolicy.HandleException(ex, "Critical")
                Throw
            End Try

        End Sub
        Sub getAccessDBMIND01()

            Dim db As Database
            Dim dc As DbCommand
            Dim dsDBMIND01 As DataSet
            Dim i As Integer = 0
            Dim erlLgnCd As String

            Try
                db = DatabaseFactory.CreateDatabase("DBPRC01")
                dc = db.GetStoredProcCommand("prc_prtl_slc_dbmind01_usr_app_access")
                dsDBMIND01 = db.ExecuteDataSet(dc)
                If Not (dsDBMIND01 Is Nothing) And dsDBMIND01.Tables.Count > 0 Then
                    Dim _row As DataRow
                    dc = db.GetStoredProcCommand("prc_prtl_upd_usr_app_access")
                    For Each _row In dsDBMIND01.Tables(0).Rows ' Select("tevt_typ_id=" & CInt(trkevt))
                        erlLgnCd = _row("erl_lgn_cd")
                        If InStr(erlLgnCd, "@", CompareMethod.Text) Then
                            i = i + 1
                        Else
                            db.AddInParameter(dc, "@usr_lgn_cd", DbType.String, _row("erl_lgn_cd".ToLower))
                            db.AddInParameter(dc, "@apl_acrn", DbType.String, _row("apl_cd"))
                            db.AddInParameter(dc, "@apl_nm", DbType.String, "ExtraNet")
                            db.AddInParameter(dc, "@apl_grp_nm", DbType.String, "SiteMinder")

                            db.ExecuteNonQuery(dc)
                            dc.Parameters.Clear()
                            i = i + 1
                        End If

                        If i >= dsDBMIND01.Tables(0).Rows.Count Then
                            Exit Sub
                        End If

                    Next _row

                End If

            Catch ex As Exception
                ExceptionPolicy.HandleException(ex, "Critical")
            End Try

        End Sub

        Public Sub getAccessAD()
            Dim db As Database
            Dim dc As DbCommand
            Dim user As String
            Dim iStartAplCd As Integer
            Dim iEndAplCd As Integer
            Dim AplCd As String
            Dim sAplCd As String
            Dim dsDBTRK02 As DataSet

            'Get Organization Units Active Directory Groups
            Dim ouConn As String = "LDAP://JCRIS2/OU=AppGroups,DC=jcaho,DC=net"
            Dim ouRoot As DirectoryEntry = New DirectoryEntry(ouConn)
            Dim ouSearcher As DirectorySearcher = New DirectorySearcher(ouRoot)
            Dim ouResults As SearchResultCollection
            Dim ouResult As SearchResult
            Dim ouFilter As String
            Dim GrpMbrCnt As Integer
            Dim _row As DataRow

            ouFilter = "(objectclass=group)"
            ouSearcher.SearchScope = SearchScope.Subtree
            ouSearcher.Filter = ouFilter
            ouSearcher.PropertiesToLoad.Add("distinguishedname")
            ouSearcher.PropertiesToLoad.Add("member")
            ouResults = ouSearcher.FindAll

            Try
                db = DatabaseFactory.CreateDatabase("DBPRC01")

                ' Determine if members of Organization Units Active Directory groups are users or othe AD groups
                For Each ouResult In ouResults
                    If InStr(ouResult.Properties("distinguishedname")(0), "rpt", CompareMethod.Text) Or InStr(ouResult.Properties("distinguishedname")(0), "prtl", CompareMethod.Text) Then
                    Else
                        GrpMbrCnt = 0

                        ' Get Application Code(apl_cd) from parsing distinguished name property
                        sAplCd = ouResult.Properties("distinguishedname")(0)
                        iStartAplCd = 3
                        iEndAplCd = sAplCd.IndexOf(" ") - 3
                        AplCd = sAplCd.Substring(iStartAplCd, iEndAplCd)

                        ' Next three lines added to prevent blank entries from be displayed on AppLink. IF apl_cd not in DBTRK02...t_jc_application_url, do not process AD group.
                        dc = db.GetStoredProcCommand("prc_prtl_slc_vsnet_url")
                        db.AddInParameter(dc, "@apl_cd", DbType.String, AplCd.Trim.ToLower)
                        dsDBTRK02 = db.ExecuteDataSet(dc)
                        dc.Parameters.Clear()
                        For Each _row In dsDBTRK02.Tables(0).Rows

                            Try
                                ' Loop through ouResult containing AD groups in ou=AppGroups and get members
                                Do While GrpMbrCnt < ouResult.Properties("member").Count
                                    Dim MbrConn As String = "LDAP://JCRIS2/" + ouResult.Properties("member")(GrpMbrCnt)
                                    Dim MbrRoot As DirectoryEntry = New DirectoryEntry(MbrConn)
                                    Dim mbrSearcher As DirectorySearcher = New DirectorySearcher(MbrRoot)
                                    Dim mbrResults As SearchResultCollection
                                    Dim mbrResult As SearchResult
                                    Dim mbrFilter As String
                                    Try
                                        mbrFilter = "(|(objectclass=group)(objectclass=person))"
                                        mbrSearcher.SearchScope = SearchScope.Subtree
                                        mbrSearcher.Filter = mbrFilter
                                        mbrSearcher.PropertiesToLoad.Add("objectclass")
                                        mbrSearcher.PropertiesToLoad.Add("member")
                                        mbrSearcher.PropertiesToLoad.Add("cn")
                                        mbrSearcher.PropertiesToLoad.Add("distinguishedname")
                                        mbrResults = mbrSearcher.FindAll

                                    Catch ex As Exception

                                    End Try

                                    ' Determine is AD group member is another AD group or User. If AD group call recursive function
                                    dc = db.GetStoredProcCommand("prc_prtl_upd_usr_app_access")
                                    For Each mbrResult In mbrResults
                                        Dim s As String
                                        s = mbrResult.Properties("objectclass").Item(1)
                                        If mbrResult.Properties("objectclass").Item(1) = "group" Then
                                            Call ADgroupMbrs(mbrResult.Properties("distinguishedname")(0), AplCd)
                                        Else
                                            user = mbrResult.Properties("cn")(0)

                                            db.AddInParameter(dc, "@usr_lgn_cd", DbType.String, user.ToLower)
                                            db.AddInParameter(dc, "@apl_acrn", DbType.String, AplCd.Trim.ToLower)
                                            db.AddInParameter(dc, "@apl_nm", DbType.String, "IntraNet")
                                            db.AddInParameter(dc, "@apl_grp_nm", DbType.String, "VSNET")
                                            db.ExecuteNonQuery(dc)
                                            dc.Parameters.Clear()
                                        End If
                                    Next

                                    GrpMbrCnt = GrpMbrCnt + 1

                                Loop
                            Catch ex As Exception
                                ExceptionPolicy.HandleException(ex, "Critical")
                            End Try
                        Next
                    End If
                Next
            Catch ex As Exception
                ExceptionPolicy.HandleException(ex, "Critical")
            End Try

        End Sub

        Private Sub ADgroupMbrs(ByVal adGrp As Object, ByVal aplcd As String)
            Dim db As Database
            Dim dc As DbCommand
            Dim nestedConn As String = "LDAP://JCRIS2/" + adGrp
            Dim nestedRoot As DirectoryEntry = New DirectoryEntry(nestedConn)
            Dim grpSearcher As DirectorySearcher = New DirectorySearcher(nestedRoot)
            Dim grpResults As SearchResultCollection
            Dim grpResult As SearchResult
            Dim grpFilter As String
            Dim MbrCnt As Integer
            Dim user As String

            ' Query Active Directory for members of AD group passed into function
            grpFilter = "(|(objectclass=group)(objectclass=person))"
            grpSearcher.SearchScope = SearchScope.Subtree
            grpSearcher.Filter = grpFilter
            grpSearcher.PropertiesToLoad.Add("cn")
            grpSearcher.PropertiesToLoad.Add("member")
            grpSearcher.PropertiesToLoad.Add("distinguishedname")
            grpSearcher.PropertiesToLoad.Add("objectclass")
            grpResults = grpSearcher.FindAll

            For Each grpResult In grpResults

                MbrCnt = 0
                Try
                    Do While MbrCnt < grpResult.Properties("member").Count

                        Dim NestedMbrConn As String = "LDAP://JCRIS2/" + grpResult.Properties("member")(MbrCnt)
                        Dim NestedMbrRoot As DirectoryEntry = New DirectoryEntry(NestedMbrConn)
                        Dim NestedMbrSearcher As DirectorySearcher = New DirectorySearcher(NestedMbrRoot)
                        Dim NestedMbrResults As SearchResultCollection
                        Dim NestedMbrResult As SearchResult
                        Dim NestedMbrFilter As String

                        NestedMbrFilter = "(|(objectclass=group)(objectclass=person))"
                        NestedMbrSearcher.SearchScope = SearchScope.Subtree
                        NestedMbrSearcher.Filter = NestedMbrFilter
                        NestedMbrSearcher.PropertiesToLoad.Add("objectclass")
                        NestedMbrSearcher.PropertiesToLoad.Add("member")
                        NestedMbrSearcher.PropertiesToLoad.Add("cn")
                        NestedMbrSearcher.PropertiesToLoad.Add("distinguishedname")
                        NestedMbrResults = NestedMbrSearcher.FindAll

                        db = DatabaseFactory.CreateDatabase("DBPRC01")
                        dc = db.GetStoredProcCommand("prc_prtl_upd_usr_app_access")

                        For Each NestedMbrResult In NestedMbrResults
                            Dim s As String
                            s = NestedMbrResult.Properties("objectclass").Item(1)
                            If NestedMbrResult.Properties("objectclass").Item(1) = "group" Then
                                Call ADgroupMbrs(NestedMbrResult.Properties("distinguishedname")(0), aplcd)
                            Else
                                user = NestedMbrResult.Properties("cn")(0)
                                db.AddInParameter(dc, "@usr_lgn_cd", DbType.String, user.ToLower)
                                db.AddInParameter(dc, "@apl_acrn", DbType.String, aplcd.Trim.ToLower)
                                db.AddInParameter(dc, "@apl_nm", DbType.String, "IntraNet")
                                db.AddInParameter(dc, "@apl_grp_nm", DbType.String, "VSNET")
                                db.ExecuteNonQuery(dc)
                                dc.Parameters.Clear()
                            End If
                        Next
                        MbrCnt = MbrCnt + 1

                    Loop
                Catch ex As Exception
                    ExceptionPolicy.HandleException(ex, "Critical")
                End Try
            Next

        End Sub


        'Public Shared Function GetConnectionStringForEnvironment(ByVal dbName As String) As String
        '    Dim rtn As String = ""
        '    Dim mn As String = Environment.MachineName.ToLower

        '    Try
        '        If mn.StartsWith("jcdev") Then
        '            rtn = (dbName + "_dev")
        '        ElseIf mn.StartsWith("jctqa") Then
        '            rtn = (dbName + "_tqa")
        '        ElseIf mn.StartsWith("jcplt") Then
        '            rtn = (dbName + "_plt")
        '        ElseIf mn.StartsWith("jcprd") Then
        '            rtn = (dbName + "_prd")
        '        Else : rtn = (dbName + "_dev")
        '        End If

        '    Catch ex As Exception
        '        ExceptionPolicy.HandleException(ex, "error")
        '    End Try

        '    Return rtn

        'End Function

    End Class

End Module
