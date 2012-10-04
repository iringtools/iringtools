
#Region " Imports "

Imports System.Collections
Imports System.Collections.Generic
Imports System.Collections.Specialized
Imports System.IO
Imports System.Linq
Imports System.Linq.Expressions
Imports System.Xml.Linq
Imports Ninject
Imports org.iringtools.adapter
Imports org.iringtools.library
Imports org.iringtools.utility
Imports System.Diagnostics
Imports log4net
Imports log4net.Config

'Imports Llama
'Imports ISPClientData3
Imports System.Data.SqlClient
Imports System.Text
Imports System.Text.RegularExpressions
'Imports System.Data.OracleClient
Imports Oracle.DataAccess.Client

#End Region

Public Class SPPIDDataLayer : Inherits BaseSQLDataLayer
    Implements IDataLayer2
#Region " Variables "

    'Private _projDatasource As Llama.LMADataSource = Nothing ' SPPID DataSource
    'Private _lmFilters As Llama.LMAFilter = Nothing
    'Private _lmCriterion As Llama.LMACriterion = Nothing
    Private m_skipInternalAttributes As Boolean  ' ignore internal attributes
    Private m_skipNoDisplayAttributes As Boolean  ' ignore non-displayed attributes
    'Protected _configuration As XElement
    Private AppSettings As AdapterSettings
    Private ProjConfig As Xml.XmlDocument
    Private SPWorkSet As SPPIDWorkingSet
    Private Shared ReadOnly _logger As ILog = LogManager.GetLogger(GetType(SPPIDDataLayer))
    Private _kernel As IKernel = Nothing

    Private _dataDictionary As DataDictionary

    Private _projConn As SqlConnection
    Private _stageConn As SqlConnection
    Private _siteConn As SqlConnection


    'Private _stageConnOracle As OracleConnection
    Private _siteConnOracle As OracleConnection
    Private _plantConnOracle As OracleConnection
    Private _plantDicConnOracle As OracleConnection
    Private _PIDConnOracle As OracleConnection
    Private _PIDDicConnOracle As OracleConnection

#End Region

#Region " Instantiation "


    <Inject()>
    Public Sub New(ByVal settings As AdapterSettings)

        MyBase.New(settings)


        Dim configPath As String
        Dim StagingConfigurationPath As String = ""

        ' configures the logger based on the configuration information in the app.config file
        XmlConfigurator.Configure(New System.IO.FileInfo(".\app.config"))

        AppSettings = settings

        Try
            Dim tmp As String = String.Empty

            tmp = String.Format("{0}{1}.{2}.StagingConfiguration.xml", _settings("AppDataPath"), _settings("ProjectName"), _settings("ApplicationName"))
            settings("StagingConfigurationPath") = Path.Combine(_settings("BaseDirectoryPath"), tmp)

            configPath = [String].Format("{0}{1}.{2}.config", _settings("AppDataPath"), _settings("ProjectName"), _settings("ApplicationName"))
            settings("ProjectConfigurationPath") = Path.Combine(_settings("BaseDirectoryPath"), configPath)

            AppSettings.AppendSettings(settings)

            If (File.Exists(_settings("ProjectConfigurationPath"))) Then
                AppSettings.AppendSettings(New StaticDust.Configuration.AppSettingsReader(_settings("ProjectConfigurationPath")))
            End If

            DecryptConntionInfo(AppSettings("iRingStagingConnectionString"), AppSettings("SPPIDSiteConnectionString"), AppSettings("SPPIDPLantConnectionString"), AppSettings("PIDDataDicConnectionString"), AppSettings("PIDConnectionString"), AppSettings("PlantDataDicConnectionString"))

            ''Set Connection strings----------------
            If AppSettings("SPPIDPLantConnectionString").Contains("PROTOCOL") = False Then
                _projConn = New SqlConnection(AppSettings("SPPIDPLantConnectionString"))

                _siteConn = New SqlConnection(AppSettings("SPPIDSiteConnectionString"))
            Else
                _plantConnOracle = New OracleConnection(AppSettings("SPPIDPLantConnectionString"))
                _siteConnOracle = New OracleConnection(AppSettings("SPPIDSiteConnectionString"))
                _plantDicConnOracle = New OracleConnection(AppSettings("PlantDataDicConnectionString"))
                _PIDConnOracle = New OracleConnection(AppSettings("PIDConnectionString"))
                _PIDDicConnOracle = New OracleConnection(AppSettings("PIDDataDicConnectionString"))

                ''Set Oracle Stagging Files-------------------------
                tmp = String.Format("{0}{1}.{2}.StagingConfiguration.{3}.xml", _settings("AppDataPath"), _settings("ProjectName"), _settings("ApplicationName"), "Oracle")
                settings("StagingConfigurationPath") = Path.Combine(_settings("BaseDirectoryPath"), tmp)
                AppSettings("StagingConfigurationPath") = Path.Combine(_settings("BaseDirectoryPath"), tmp)
            End If

            _stageConn = New SqlConnection(AppSettings("iRingStagingConnectionString"))

        Catch ex As Exception
            _logger.Error("SPPIDDataLayer could not be instantiated due to error: " + ex.Message)
            Throw New Exception(ex.Message)
            'MsgBox("Fail: SPPIDDataLayer could not be instantiated due to error: " & ex.Message, MsgBoxStyle.Critical)
            ' this will likely only be loaded in this way while testing, so ignore the error
        End Try

    End Sub

#End Region

#Region " Overridden Public Methods "

    Public Overrides Function GetDatabaseDictionary() As DatabaseDictionary

        Dim databaseDictionary As New DatabaseDictionary()
        Try
            Dim path As String = [String].Format("{0}{1}DatabaseDictionary.{2}.{3}.xml", _settings("BaseDirectoryPath"), _settings("XmlPath"), _settings("ProjectName"), _settings("ApplicationName"))
            If (File.Exists(path)) Then
                databaseDictionary = utility.Utility.Read(Of DatabaseDictionary)(path)
            End If
            Return databaseDictionary
        Catch ex As Exception
            _logger.Debug("SPPIDDataLayer:- Error while fetch Database Dictionary: " + ex.Message)
            Throw New Exception("SPPIDDataLayer:- Error while fetch Database Dictionary: ", ex)
        End Try


    End Function

    Public Overrides Function GetDictionary() As DataDictionary

        Try
            Dim path As String = [String].Format("{0}{1}DataDictionary.{2}.{3}.xml", _settings("BaseDirectoryPath"), _settings("XmlPath"), _settings("ProjectName"), _settings("ApplicationName"))

            If (File.Exists(path)) Then

                Dim DataDictionary = utility.Utility.Read(Of DataDictionary)(path)

                _dataObjectDefinition = DataDictionary.dataObjects.Find(Function(o) o.objectName.ToUpper() = "EQUIPMENT")

                _dataDictionary = utility.Utility.Read(Of DataDictionary)(path)
                Return _dataDictionary

            Else
                ''Call Configure
                Dim _xElement As System.Xml.Linq.XElement = Nothing
                Dim _response As Response = Configure(_xElement)
                If _response.Level = StatusLevel.Success Then
                    Dim tablename As List(Of String) = LoadDataTable(_stageConn.ConnectionString)
                    _dataDictionary = LoadDataObjects(tablename, _stageConn.ConnectionString)
                    Dim _databaseDictionary As New DatabaseDictionary()
                    _databaseDictionary.dataObjects = _dataDictionary.dataObjects
                    _databaseDictionary.ConnectionString = EncryptionUtility.Encrypt(_stageConn.ConnectionString)
                    _databaseDictionary.Provider = "MSSQL2008"
                    _databaseDictionary.SchemaName = "dbo"


                    utility.Utility.Write(Of DatabaseDictionary)(_databaseDictionary, [String].Format("{0}{1}DataBaseDictionary.{2}.{3}.xml", _settings("BaseDirectoryPath"), _settings("XmlPath"), _settings("ProjectName"), _settings("ApplicationName")))
                    utility.Utility.Write(Of DataDictionary)(_dataDictionary, [String].Format("{0}{1}DataDictionary.{2}.{3}.xml", _settings("BaseDirectoryPath"), _settings("XmlPath"), _settings("ProjectName"), _settings("ApplicationName")))

                    ' _dataObjectDefinition = _dataDictionary.dataObjects.Find(Function(o) o.objectName.ToUpper() = "EQUIPMENT")
                    Return _dataDictionary
                Else
                    ' _logger.Debug("Error while configuring SP P&ID data layer:  '" & _response.Messages(0))
                    Throw New Exception("Error while configuring SP P&ID data layer:  '" & _response.Messages(0))
                End If
            End If
        Catch ex As Exception
            _logger.Debug("GetDictionary has error: " + ex.Message)
            Throw New Exception("GetDictionary has error: '", ex)
        End Try

    End Function

    Public Overrides Function GetDataTable(ByVal tableName As String, ByVal identifiers As IList(Of String)) As System.Data.DataTable
        Try

            Dim filter As DataFilter = FormMultipleKeysFilter(identifiers)
            _projConn = New SqlConnection(AppSettings("iRingStagingConnectionString"))
            'TODO: Is the whereClauseAlias always set?
            Dim whereClause As String = filter.ToSqlWhereClause(_dbDictionary, tableName, _whereClauseAlias)

            'TODO: Does the where clause include the word WHERE?
            Dim query As String = "SELECT * FROM " & tableName & whereClause

            Dim adapter As New SqlDataAdapter()
            adapter.SelectCommand = New SqlCommand(query, _projConn)

            Dim command As New SqlCommandBuilder(adapter)

            Dim dataSet As New DataSet()
            adapter.Fill(dataSet, tableName)

            Dim myTable As DataTable = dataSet.Tables(tableName)

            Return dataSet.Tables(tableName)
        Catch ex As Exception
            _logger.Error("Error in GetDataTable of SP P&ID data layer:  '" & ex.Message)
            Throw New Exception("Error in GetDataTable of SP P&ID data layer:  '", ex)
        End Try

    End Function

    Public Overrides Function GetDataTable(ByVal tableName As String, ByVal whereClause As String, ByVal start As Long, ByVal limit As Long) As System.Data.DataTable
        Try

            Dim query As String = "SELECT * FROM " & tableName & " " & whereClause
            _projConn = _stageConn
            Dim adapter As New SqlDataAdapter()
            adapter.SelectCommand = New SqlCommand(query, _projConn) ' need to set connection string.

            Dim command As New SqlCommandBuilder(adapter)

            Dim dataSet As New DataSet()
            ' adapter.Fill(dataSet, tableName)

            adapter.Fill(dataSet, start, limit, tableName)

            Return (dataSet.Tables(tableName))
        Catch ex As Exception
            _logger.Error("Error in GetDataTable which paging of SP P&ID data layer:  '" & ex.Message)
            Throw New Exception("Error in GetDataTable which paging of SP P&ID data layer:  '", ex)
        End Try
    End Function

    Public Overrides Function GetCount(ByVal tableName As String, ByVal whereClause As String) As Long

        Dim dataObjects As DataTable = GetDataTable(tableName, whereClause, 0, 0)

        Return dataObjects.Rows.Count()
    End Function

    Public Overrides Function GetIdentifiers(ByVal tableName As String, ByVal whereClause As String) As IList(Of String)

        Try
            Dim identifiers As New List(Of String)()


            Dim dataTable As DataTable = GetDataTable(tableName, whereClause, 0, 0)

            Dim dataObjects As IList(Of IDataObject) = ToDataObjects(dataTable, tableName)
            Dim _list As List(Of org.iringtools.library.KeyProperty)
            For value As Integer = 0 To _dataDictionary.dataObjects.Count()
                If (_dataDictionary.dataObjects(value).objectName.ToUpper() = tableName.ToUpper()) Then
                    _list = _dataDictionary.dataObjects(value).keyProperties
                    Exit For
                End If

            Next

            For Each dataObject As IDataObject In dataObjects
                For value As Integer = 0 To _list.Count() - 1
                    identifiers.Add(DirectCast(dataObject.GetPropertyValue(_list(value).keyPropertyName), String))
                Next
            Next

            Return identifiers
        Catch ex As Exception
            _logger.Debug("Error while getting a list of identifiers of type [" + tableName + "].")
            Throw New Exception("Error while getting a list of identifiers of type [" & tableName & "].", ex)
        End Try

    End Function

    Public Overrides Function PostDataTables(ByVal dataTables As IList(Of System.Data.DataTable)) As Response
        Dim response As New Response()
        Try
            Dim tableName As String = dataTables.First().TableName
            Dim query As String = "SELECT * FROM " & tableName

            Dim adapter As New SqlDataAdapter()
            adapter.SelectCommand = New SqlCommand(query, _projConn)

            Dim command As New SqlCommandBuilder(adapter)
            adapter.UpdateCommand = command.GetUpdateCommand()

            Dim dataSet As New DataSet()
            For Each dataTable As DataTable In dataTables
                dataSet.Tables.Add(dataTable)
            Next

            adapter.Update(dataSet, tableName)

            Dim status As New Status
            status.Level = StatusLevel.Success
            status.Messages = New Messages()
            status.Messages.Add("success")
            response.Level = StatusLevel.Success
            response.StatusList.Add(status)


        Catch ex As Exception
            Dim errorMessage As String
            errorMessage = "Error Posting Tables: " & ex.Message
            _logger.Error(errorMessage)
            Dim status As New Status
            status.Level = StatusLevel.Error
            status.Messages = New Messages()
            status.Messages.Add(errorMessage)
            response.Level = StatusLevel.Error
            response.StatusList.Add(status)
        End Try
        Return response

    End Function

    Dim _sppidconfiguration As SPPIDConfiguration
    Public Overrides Function GetConfiguration() As System.Xml.Linq.XElement

        Dim xelement As XElement
        Dim provider As String = ""

        Dim path As String = _settings("ProjectConfigurationPath")
        Dim oFile As System.IO.File
        Dim oRead As System.IO.StreamReader
        oRead = oFile.OpenText(path)


        Dim LineIn As String
        Dim value As String = ""
        While oRead.Peek <> -1
            LineIn = oRead.ReadLine()
            If LineIn.Contains("Provider") = True Then
                value = LineIn.Substring(LineIn.IndexOf("value=") + 7, LineIn.LastIndexOf("""") - (LineIn.IndexOf("value=") + 7))
            End If

        End While
        oRead.Close()

        If _sppidconfiguration Is Nothing Then
            If AppSettings("SPPIDPlantConnectionString") = Nothing Then
                AppSettings("SPPIDPlantConnectionString") = String.Empty
            End If
            If AppSettings("SPPIDSiteConnectionString") = Nothing Then
                AppSettings("SPPIDSiteConnectionString") = String.Empty
            End If
            If AppSettings("iRingStagingConnectionString") = Nothing Then
                AppSettings("iRingStagingConnectionString") = String.Empty
            End If

            _sppidconfiguration = New SPPIDConfiguration() With { _
              .PlantConnectionString = AppSettings("SPPIDPlantConnectionString"),
              .SiteConnectionString = AppSettings("SPPIDSiteConnectionString"),
            .StagingConnectionString = AppSettings("iRingStagingConnectionString"),
              .PIDConnectionString = AppSettings("PIDConnectionString"),
              .PIDDataDicConnectionString = AppSettings("PIDDataDicConnectionString"),
              .PlantDataDicConnectionString = AppSettings("PlantDataDicConnectionString"),
          .Provider = value
            }
        End If

        xelement = utility.Utility.SerializeToXElement(_sppidconfiguration)

        Return xelement

    End Function

    Public Overrides Function Configure(ByVal configuration As System.Xml.Linq.XElement) As Response

        Dim response As New Response
        Try
            If configuration IsNot Nothing Then
                Dim Config As SPPIDConfiguration = utility.Utility.DeserializeFromXElement(Of SPPIDConfiguration)(configuration)

                '' Create Config File ----------------------
                Dim configfile As New XElement("configuration", _
                            New XElement("appSettings", _
                            New XElement("add", New XAttribute("key", "Provider"), _
                            New XAttribute("value", Config.Provider)), _
                            New XElement("add", New XAttribute("key", "SPPIDSiteConnectionString"), _
                            New XAttribute("value", EncryptionUtility.Encrypt(Config.SiteConnectionString))), _
                            New XElement("add", New XAttribute("key", "SPPIDPlantConnectionString"), _
                            New XAttribute("value", EncryptionUtility.Encrypt(Config.PlantConnectionString))), _
                            New XElement("add", New XAttribute("key", "PlantDataDicConnectionString"), _
                            New XAttribute("value", EncryptionUtility.Encrypt(Config.PlantDataDicConnectionString))), _
                            New XElement("add", New XAttribute("key", "PIDConnectionString"), _
                            New XAttribute("value", EncryptionUtility.Encrypt(Config.PIDConnectionString))), _
                            New XElement("add", New XAttribute("key", "PIDDataDicConnectionString"), _
                            New XAttribute("value", EncryptionUtility.Encrypt(Config.PIDDataDicConnectionString))), _
                            New XElement("add", New XAttribute("key", "iRingStagingConnectionString"), _
                            New XAttribute("value", EncryptionUtility.Encrypt(Config.StagingConnectionString)))))


                If (File.Exists(_settings("ProjectConfigurationPath"))) Then
                    File.Delete(_settings("ProjectConfigurationPath"))
                End If

                configfile.Save(_settings("ProjectConfigurationPath"))

                If (File.Exists(_settings("ProjectConfigurationPath"))) Then
                    AppSettings.AppendSettings(New StaticDust.Configuration.AppSettingsReader(_settings("ProjectConfigurationPath")))
                End If


                ''Set Connection strings----------------
                If (AppSettings("SPPIDPlantConnectionString").ToString().Contains("PROTOCOL=TCP")) Then
                    _siteConnOracle = New OracleConnection(AppSettings("SPPIDSiteConnectionString"))

                    _plantConnOracle = New OracleConnection(AppSettings("SPPIDPlantConnectionString"))

                    _PIDConnOracle = New OracleConnection(AppSettings("PIDConnectionString"))
                    _PIDDicConnOracle = New OracleConnection(AppSettings("PIDDataDicConnectionString"))


                    _plantDicConnOracle = New OracleConnection(AppSettings("PlantDataDicConnectionString"))

                Else
                    _projConn = New SqlConnection(AppSettings("SPPIDPlantConnectionString"))
                    _siteConn = New SqlConnection(AppSettings("SPPIDSiteConnectionString"))
                End If


                _stageConn = New SqlConnection(AppSettings("iRingStagingConnectionString"))
            End If
            Dim success As String = UpdateConfigurations()
            If success.Contains("Pass") = False Then
                response.Level = StatusLevel.Error
            End If
            response.Messages.Add(success)
        Catch ex As Exception
            Dim errorMessage As String
            errorMessage = "Error Happened in Configuring SPPID Datalayer: " & ex.Message
            _logger.Error(errorMessage)
            Dim status As New Status
            status.Level = StatusLevel.Error
            status.Messages = New Messages()
            status.Messages.Add(errorMessage)
            response.Level = StatusLevel.Error
            response.StatusList.Add(status)
        End Try
        Return response
    End Function

    Public Overrides Function CreateDataTable(ByVal tableName As String, ByVal identifiers As IList(Of String)) As System.Data.DataTable
        Throw New NotImplementedException()
    End Function

    Public Overrides Function DeleteDataTable(ByVal tableName As String, ByVal identifiers As IList(Of String)) As Response
        Dim response As New Response
        Dim errorMessage As String
        errorMessage = "The function of DeleteDataTable is not implemented "
        _logger.Error(errorMessage)
        Dim status As New Status
        status.Level = StatusLevel.Error
        status.Messages = New Messages()
        status.Messages.Add(errorMessage)
        response.Level = StatusLevel.Error
        response.StatusList.Add(status)
        Return response
        'Throw New NotImplementedException()
    End Function

    Public Overrides Function DeleteDataTable(ByVal tableName As String, ByVal whereClause As String) As Response
        Dim response As New Response
        Dim errorMessage As String
        errorMessage = "The function of DeleteDataTable is not implemented "
        _logger.Error(errorMessage)
        Dim status As New Status
        status.Level = StatusLevel.Error
        status.Messages = New Messages()
        status.Messages.Add(errorMessage)
        response.Level = StatusLevel.Error
        response.StatusList.Add(status)
        Return response
        'Throw New NotImplementedException()
    End Function

    Public Overrides Function GetRelatedDataTable(ByVal dataRow As System.Data.DataRow, ByVal relatedTableName As String) As System.Data.DataTable
        Throw New NotImplementedException()
    End Function
    Public Overrides Function GetRelatedDataTable(ByVal dataRow As System.Data.DataRow, ByVal relatedTableName As String, ByVal start As Long, ByVal limit As Long) As System.Data.DataTable
        Throw New NotImplementedException()
    End Function
    Public Overrides Function GetRelatedCount(ByVal dataRow As System.Data.DataRow, ByVal relatedTableName As String) As Long
        Throw New NotImplementedException()
    End Function

    Public Overrides Function Refresh(ByVal tableName As String) As Response
        Dim response As New Response
        Try
            Dim success = UpdateConfigurations(tableName)
            Dim status As New Status
            status.Level = StatusLevel.Success
            status.Messages = New Messages()
            status.Messages.Add(success)
            response.Level = StatusLevel.Success
            response.StatusList.Add(status)
        Catch ex As Exception
            Dim errorMessage As String
            errorMessage = "Error refreshing the table of " & tableName & ": " & ex.Message
            _logger.Error(errorMessage)
            Dim status As New Status
            status.Level = StatusLevel.Error
            status.Messages = New Messages()
            status.Messages.Add(errorMessage)
            response.Level = StatusLevel.Error
            response.StatusList.Add(status)
        End Try
        Return response
    End Function

    Public Overrides Function RefreshAll() As Response

        Dim response As New Response
        Try
            Dim success = UpdateConfigurations()
            Dim status As New Status
            status.Level = StatusLevel.Success
            status.Messages = New Messages()
            status.Messages.Add(success)
            response.Level = StatusLevel.Success
            response.StatusList.Add(status)
        Catch ex As Exception
            Dim errorMessage As String
            errorMessage = "Error refreshing dictionary: " & ex.Message
            _logger.Error(errorMessage)
            Dim status As New Status
            status.Level = StatusLevel.Error
            status.Messages = New Messages()
            status.Messages.Add(errorMessage)
            response.Level = StatusLevel.Error
            response.StatusList.Add(status)
            'MsgBox("Fail: SPPIDDataLayer could not be instantiated due to error: " & ex.Message, MsgBoxStyle.Critical)
            ' this will likely only be loaded in this way while testing, so ignore the error
        End Try
        Return response

    End Function

    Public Overrides Function RefreshDataTable(ByVal tablename As String) As Response
        Dim response As New Response
        Dim errorMessage As String
        errorMessage = "The function of RefreshDataTable is not implemented "
        _logger.Error(errorMessage)
        Dim status As New Status
        status.Level = StatusLevel.Error
        status.Messages = New Messages()
        status.Messages.Add(errorMessage)
        response.Level = StatusLevel.Error
        response.StatusList.Add(status)
        Return response
        'Throw New NotImplementedException()
    End Function



#End Region

    Public Function LoadDataTable(ByVal _stageConStr As String) As List(Of String)
        Dim _dataTables As New List(Of String)
        Dim _stage As SqlConnection = _stageConn
        ' Dim _stage As New SqlConnection(_stageConStr)
        If _stage.State = ConnectionState.Closed Then _stage.Open()

        Dim _selectSql As SqlCommand = _stage.CreateCommand
        _selectSql.CommandText = "SELECT TABLE_NAME FROM information_schema.tables"
        Dim _selectSqlDR As SqlDataReader = _selectSql.ExecuteReader()
        If _selectSqlDR.HasRows Then
            Do While _selectSqlDR.Read()
                _dataTables.Add(_selectSqlDR.Item("TABLE_NAME"))
            Loop
        End If
        _selectSqlDR.Close()
        Return _dataTables

    End Function

#Region " Staging Methods "

    Public Function MigrateSPPIDToStaging(Optional ByVal tablename As String = "") As String

        Dim replacements As IEnumerable(Of XElement) = Nothing
        Dim declarations As IEnumerable(Of XElement) = Nothing
        Dim queryParts As New Dictionary(Of SQLClause, String)
        Dim queryText As String = ""
        Dim stgCfgQueries As IEnumerable(Of XElement) = Nothing
        Dim siteDataQuery As XElement = Nothing
        Dim tmpStr As String = ""
        Dim cmd As SqlCommand
        Dim DT As DataTable
        Dim DS As DataSet
        Dim DA As SqlDataAdapter
        Dim allQueryText As String
        Dim sbc As New SqlBulkCopy(_stageConn)
        Dim rVal As String

        Try

            GetStagingQueries(stgCfgQueries, tablename)

            For Each q As XElement In stgCfgQueries

                queryParts.Clear()
                rVal = GetQueryParts(q, SPWorkSet.ColumnsView, SPWorkSet.TablesView, SPWorkSet.SchemaSubstitutions,
                              queryParts, replacements, declarations, SPWorkSet.QueryVariableMap, SPWorkSet.CommonServerName, SPWorkSet.ProjectDBName)
                SetDeclarationValues(queryParts, declarations, SPWorkSet.QueryVariableMap, _logger)

                ' commbine the query parts and perform any necessary replacements. 
                ' NOTE - although it is possible to make use of an INTO clause to create a selection query that will 
                ' also automatically create the destination table, this has limitations, the most serious of which is
                ' it is not safe to assume that the Source DB and Staging DB have the same security requirements. Instead,
                ' we will always assume that security is separate for these two databases and that the connection strings for the 
                ' Source and Staging connections provide this information for each individual location. We also cannot assume that
                ' the specified credentials have the power to create a Linked Server connection or that both SQL Server instances
                ' allow ad hoc (OpenDataSource) queries. Instead, the provided credentials are used to copy the data to the 
                ' local machine and then bulk copied out to the staging server, bypassing the need for a more sophisticated security
                ' check/edit)

                If Mid(rVal, 1, 4) = "Warn" Then _logger.Warn(Mid(rVal, 7))
                If Mid(rVal, 1, 4) = "Fail" Then : _logger.Error("Query '" & queryParts(SQLClause.QueryName) & "' could not be built due to error: " & Mid(rVal, 7))
                Else

                    queryParts.BuildQuery(queryText, replacements, SPWorkSet.TextReplacementMap, False)
                    allQueryText = nl & "--************ Table Definition ***************" & nl & queryParts(SQLClause.TableDef) & nl &
                        "--**************** End Table Definition  ***************" & nl & nl & queryText

                    _logger.Info("")
                    _logger.Info("--" & StrDup(18, "*") & LSet("  Start Query '" & queryParts(SQLClause.QueryName) & "'", 60) & StrDup(20, "*"))
                    _logger.Info(allQueryText)
                    _logger.Info("--" & StrDup(18, "*") & LSet("  End Query '" & queryParts(SQLClause.QueryName) & "'", 60) & StrDup(20, "*"))
                    _logger.Info("")

                    ' delete any existing table in the Staging location by the destination name
                    cmd = _stageConn.CreateCommand
                    cmd.CommandText = _
                        "IF  EXISTS (" &
                        "SELECT object_id " &
                        "FROM sys.objects " &
                        "WHERE object_id = OBJECT_ID(N'dbo.[" & queryParts(SQLClause.StagingName) & "]')  AND type in (N'U'))" &
                        "   DROP TABLE dbo.[" & queryParts(SQLClause.StagingName) & "]"

                    If _stageConn.State = ConnectionState.Closed Then _stageConn.Open()
                    cmd.ExecuteNonQuery()

                    ' create a new table to hold the data
                    cmd = _stageConn.CreateCommand()
                    cmd.CommandText = queryParts(SQLClause.TableDef)
                    cmd.ExecuteNonQuery()



                    ' fetch the data
                    cmd = _projConn.CreateCommand()
                    cmd.CommandText = queryText

                    DS = New DataSet
                    DA = New SqlDataAdapter(cmd)
                    'DA.MissingSchemaAction = MissingSchemaAction.AddWithKey
                    DA.Fill(DS, queryParts(SQLClause.StagingName))
                    DT = DS.Tables(queryParts(SQLClause.StagingName))

                    ' set the destination location and bulk copy the data to the new table
                    sbc.DestinationTableName = queryParts(SQLClause.StagingName)
                    sbc.WriteToServer(DT)
                    If _stageConn.State = ConnectionState.Open Then _stageConn.Close()

                End If

            Next
            Return "Pass"
        Catch ex As Exception
            _logger.Debug(ex.Message)
            Debug.Print("got here")
            Return "Fail: " + ex.Message
        End Try
    End Function

    Public Function MigrateSPPIDToStagingfromOracle(Optional ByVal tablename As String = "") As String

        Dim replacements As IEnumerable(Of XElement) = Nothing
        Dim declarations As IEnumerable(Of XElement) = Nothing
        Dim queryParts As New Dictionary(Of SQLClause, String)
        Dim queryText As String = ""
        Dim stgCfgQueries As IEnumerable(Of XElement) = Nothing
        Dim siteDataQuery As XElement = Nothing
        Dim tmpStr As String = ""
        Dim cmd As SqlCommand
        Dim DT As DataTable
        Dim DS As DataSet
        Dim DA As SqlDataAdapter
        Dim allQueryText As String
        Dim sbc As New SqlBulkCopy(_stageConn)
        Dim rVal As String
        Dim cmdOra As OracleCommand
        Dim OraDR As OracleDataReader

        Try

            GetStagingQueries(stgCfgQueries, tablename)

            For Each q As XElement In stgCfgQueries

                queryParts.Clear()
                rVal = GetQueryPartsforOracle(q, SPWorkSet.OraColumnsView, SPWorkSet.TablesView, SPWorkSet.SchemaSubstitutions,
                              queryParts, replacements, declarations, SPWorkSet.QueryVariableMap, SPWorkSet.CommonServerName, SPWorkSet.ProjectDBName)
                SetDeclarationValues(queryParts, declarations, SPWorkSet.QueryVariableMap, _logger)

                ' commbine the query parts and perform any necessary replacements. 
                ' NOTE - although it is possible to make use of an INTO clause to create a selection query that will 
                ' also automatically create the destination table, this has limitations, the most serious of which is
                ' it is not safe to assume that the Source DB and Staging DB have the same security requirements. Instead,
                ' we will always assume that security is separate for these two databases and that the connection strings for the 
                ' Source and Staging connections provide this information for each individual location. We also cannot assume that
                ' the specified credentials have the power to create a Linked Server connection or that both SQL Server instances
                ' allow ad hoc (OpenDataSource) queries. Instead, the provided credentials are used to copy the data to the 
                ' local machine and then bulk copied out to the staging server, bypassing the need for a more sophisticated security
                ' check/edit)

                If Mid(rVal, 1, 4) = "Warn" Then _logger.Warn(Mid(rVal, 7))
                If Mid(rVal, 1, 4) = "Fail" Then : _logger.Error("Query '" & queryParts(SQLClause.QueryName) & "' could not be built due to error: " & Mid(rVal, 7))
                Else

                    queryParts.BuildQuery(queryText, replacements, SPWorkSet.TextReplacementMap, False)
                    allQueryText = nl & "--************ Table Definition ***************" & nl & queryParts(SQLClause.TableDef) & nl &
                        "--**************** End Table Definition  ***************" & nl & nl & queryText

                    _logger.Info("")
                    _logger.Info("--" & StrDup(18, "*") & LSet("  Start Query '" & queryParts(SQLClause.QueryName) & "'", 60) & StrDup(20, "*"))
                    _logger.Info(allQueryText)
                    _logger.Info("--" & StrDup(18, "*") & LSet("  End Query '" & queryParts(SQLClause.QueryName) & "'", 60) & StrDup(20, "*"))
                    _logger.Info("")

                    ' delete any existing table in the Staging location by the destination name
                    cmd = _stageConn.CreateCommand
                    cmd.CommandText = _
                        "IF  EXISTS (" &
                        "SELECT object_id " &
                        "FROM sys.objects " &
                        "WHERE object_id = OBJECT_ID(N'dbo.[" & queryParts(SQLClause.StagingName) & "]')  AND type in (N'U'))" &
                        "   DROP TABLE dbo.[" & queryParts(SQLClause.StagingName) & "]"



                    If _stageConn.State = ConnectionState.Closed Then _stageConn.Open()
                    cmd.ExecuteNonQuery()

                    ' create a new table to hold the data-----------------------------
                    cmd = _stageConn.CreateCommand()
                    Dim _queryTableDef As String = queryParts(SQLClause.TableDef).ToString()
                    _queryTableDef = Regex.Replace(_queryTableDef, "NVARCHAR2", "nvarchar(255)", RegexOptions.IgnoreCase)
                    _queryTableDef = Regex.Replace(_queryTableDef, "NUMBER", "int", RegexOptions.IgnoreCase)
                    cmd.CommandText = _queryTableDef
                    cmd.ExecuteNonQuery()

                    ' Gave Select Grants to other Schemas-----------------------------
                    ProvideGrants(_plantConnOracle, _plantDicConnOracle, _PIDDicConnOracle, _PIDConnOracle)

                    ' fetch the data-----------------------------
                    cmdOra = New OracleCommand()
                    cmdOra = _plantConnOracle.CreateCommand()
                    cmdOra.CommandText = queryText

                    OraDR = cmdOra.ExecuteReader


                    Dim dra As DataUtils.DataReaderAdapter = New DataUtils.DataReaderAdapter
                    Dim _dt As DataTable = New DataTable
                    dra.FillFromReader(_dt, OraDR)


                    ' set the destination location and bulk copy the data to the new table-----------------------------
                    sbc.DestinationTableName = queryParts(SQLClause.StagingName)
                    sbc.WriteToServer(_dt)

                    ' Revoke Select Grants from other Schemas-----------------------------
                    RevokeGrants(_plantConnOracle, _plantDicConnOracle, _PIDDicConnOracle, _PIDConnOracle)

                    If _stageConn.State = ConnectionState.Open Then _stageConn.Close()

                    'Close SPPID Database connections-----------------------------
                    If _PIDConnOracle.State = ConnectionState.Open Then _PIDConnOracle.Close()
                    If _PIDDicConnOracle.State = ConnectionState.Open Then _PIDDicConnOracle.Close()
                    If _plantConnOracle.State = ConnectionState.Open Then _plantConnOracle.Close()
                    If _plantConnOracle.State = ConnectionState.Open Then _plantConnOracle.Close()

                End If

            Next
            Return "Pass"
        Catch ex As Exception
            _logger.Debug(ex.Message)
            Debug.Print("got here")
            Return "Fail: " + ex.Message
        End Try
    End Function




#End Region

#Region "Private Functions"

    Public Shared Sub SaveDatabaseDictionary(ByVal dbDictionary As DatabaseDictionary, ByVal path As String)
        Dim connStr As String = dbDictionary.ConnectionString

        If connStr IsNot Nothing Then
            If connStr.ToUpper().Contains("DATA SOURCE") Then
                ' connection string is not encrypted, encrypt and write it back
                dbDictionary.ConnectionString = EncryptionUtility.Encrypt(connStr)
            End If
        End If

        utility.Utility.Write(Of DatabaseDictionary)(dbDictionary, path)
    End Sub


    Private Sub InitializeScope(ByVal projectName As String, ByVal applicationName As String)
        Try
            Dim scope As String = String.Format("{0}.{1}", projectName, applicationName)

            _settings("ProjectName") = projectName
            _settings("ApplicationName") = applicationName
            _settings("Scope") = scope
            _settings("DBDictionaryPath") = [String].Format("{0}DatabaseDictionary.{1}.xml", "D:\Project\iRing-Branch\2.3.x\iRINGTools.Services\App_Data\", scope)
        Catch ex As Exception
            _logger.Debug("Error initializing application: " & ex.Message)
            Throw New Exception(String.Format("Error initializing application: {0})", ex))
        End Try
    End Sub

    Public Shared Function LoadDatabaseDictionary(ByVal path As String) As DatabaseDictionary
        Dim dbDictionary As DatabaseDictionary = utility.Utility.Read(Of DatabaseDictionary)(path)
        Dim connStr As String = dbDictionary.ConnectionString

        If connStr IsNot Nothing Then
            If connStr.ToUpper().Contains("DATA SOURCE") Then
                ' connection string is not encrypted, encrypt and write it back
                dbDictionary.ConnectionString = EncryptionUtility.Encrypt(connStr)
                utility.Utility.Write(Of DatabaseDictionary)(dbDictionary, path)

                dbDictionary.ConnectionString = connStr
            Else
                dbDictionary.ConnectionString = EncryptionUtility.Decrypt(connStr)
            End If
        End If

        Return dbDictionary
    End Function




    Private Function ValidateDatabaseDictionary(ByVal dbDictionary As DatabaseDictionary) As Boolean
        ' Validate table key
        For Each dataObject As DataObject In dbDictionary.dataObjects
            If dataObject.keyProperties Is Nothing OrElse dataObject.keyProperties.Count = 0 Then
                Throw New Exception(String.Format("Table ""{0}"" has no key. Must select keys before saving.", dataObject.tableName))
            End If
        Next

        Return True
    End Function

    ''' <summary>
    ''' Fetch the queries from the staging configuration XDocument for this project
    ''' </summary>
    ''' <param name="StagingConfigQueries"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function GetStagingQueries(ByRef StagingConfigQueries As IEnumerable(Of XElement), Optional ByVal stagingDestinationName As String = "") As String

        Dim doc As XDocument

        Try

            doc = XDocument.Load(AppSettings("StagingConfigurationPath"))

            If (stagingDestinationName <> "") Then
                ' fetch all of the queries except any existing templates
                StagingConfigQueries = _
                    From el In doc...<query>
                    Select el
                    Where el.Attribute("name").Value <> "!Template" AndAlso el.Attribute("name").Value <> "!SiteData" AndAlso el.Attribute("stagingDestinationName").Value.ToUpper() = stagingDestinationName.ToUpper()
            Else
                ' fetch all of the queries except any existing templates
                StagingConfigQueries = _
                    From el In doc...<query>
                    Select el
                    Where el.Attribute("name").Value <> "!Template" AndAlso el.Attribute("name").Value <> "!SiteData"
            End If
            Return "Pass"
        Catch ex As Exception
            _logger.Debug(ex.Message)
            Return "Fail: " & ex.Message
        End Try
    End Function

    ''' <summary>
    '''Reset or update Data Objects
    ''' </summary>
    ''' <param name="tablename"></param>
    ''' <returns>Success or Failure while Migrating Tables and Data </returns>
    ''' <remarks></remarks>
    Private Function UpdateConfigurations(Optional ByVal tablename As String = "") As String


        Dim configPath As String = AppSettings("ProjectConfigurationPath")
        Dim StagingConfigurationPath As String = AppSettings("StagingConfigurationPath")
        Dim success As String = String.Empty
        AddProjConfigSettings(configPath)

        If (_plantConnOracle Is Nothing) Then
            SPWorkSet = New SPPIDWorkingSet(_projConn, _siteConn, _stageConn, StagingConfigurationPath, _logger)
            success = MigrateSPPIDToStaging(tablename)
        Else
            SPWorkSet = New SPPIDWorkingSet(_plantConnOracle, _siteConnOracle, _plantDicConnOracle, _PIDConnOracle, _PIDDicConnOracle, _stageConn, StagingConfigurationPath, _logger)
            success = MigrateSPPIDToStagingfromOracle(tablename)
        End If


        Return success

    End Function


    ''' <summary>
    ''' Allows additional configuration settings to be added to the appSettings collection
    ''' </summary>
    ''' <param name="Path"></param>
    ''' <returns></returns>
    ''' <remarks>This allows, for instance, for test settings under a non-standard naming convention to be loaded</remarks>
    Private Function AddProjConfigSettings(ByVal [Path] As String) As String

        Dim ProjConfigPath As String = [Path]
        Dim x As Xml.XmlReader
        Dim nvc As New NameValueCollection

        Try

            If File.Exists(ProjConfigPath) Then

                x = Xml.XmlReader.Create(ProjConfigPath)
                x.ReadToFollowing("appSettings")
                x.ReadToDescendant("add")

                Do
                    nvc.Add(x.GetAttribute("key"), x.GetAttribute("value"))
                Loop While x.ReadToNextSibling("add")

            Else : Return "Fail: Configuration file '" & ProjConfigPath & "' cannot be found"
            End If

            Return "Pass"

        Catch ex As Exception
            _logger.Debug(ex.Message)
            Return "Fail: " & ex.Message
        End Try
    End Function


    Private Sub DecryptConntionInfo(ByRef siteConn As String, ByRef stageConn As String, ByRef plantconn As String, ByRef pidconn As String, ByRef pidDicconn As String, ByRef plantDicconn As String)

        If (stageConn.ToUpper().Contains("DATA SOURCE") = False) Then  '' Check Site Conntion String
            siteConn = EncryptionUtility.Decrypt(siteConn)
        End If

        If (stageConn.ToUpper().Contains("DATA SOURCE") = False) Then  '' Check Site Conntion String
            '' connection string is not encrypted, encrypt and write it back

            stageConn = EncryptionUtility.Decrypt(stageConn)
        End If

        If (plantconn.ToUpper().Contains("DATA SOURCE") = False) Then  '' Check Site Conntion String
            '' connection string is not encrypted, encrypt and write it back
            plantconn = EncryptionUtility.Decrypt(plantconn)
        End If
        If (plantDicconn.ToUpper().Contains("DATA SOURCE") = False) Then  '' Check Site Conntion String
            '' connection string is not encrypted, encrypt and write it back
            plantDicconn = EncryptionUtility.Decrypt(plantDicconn)
        End If
        If (pidconn.ToUpper().Contains("DATA SOURCE") = False) Then  '' Check Site Conntion String
            '' connection string is not encrypted, encrypt and write it back
            pidconn = EncryptionUtility.Decrypt(pidconn)
        End If
        If (pidDicconn.ToUpper().Contains("DATA SOURCE") = False) Then  '' Check Site Conntion String
            '' connection string is not encrypted, encrypt and write it back
            pidDicconn = EncryptionUtility.Decrypt(pidDicconn)
        End If
    End Sub


    Private Function LoadDataObjects(ByVal objectType As List(Of String), ByVal connectionString As String) As DataDictionary
        Try

            '            Dim dataObjects As New List(Of DataObjects)()
            Dim _dataObject As New DataObject()
            Dim _keyproperties As New KeyProperty()
            Dim _dataproperties As New DataProperty()
            Dim _stage As SqlConnection = _stageConn
            Dim _dataDictionary As New DataDictionary()

            'Dim _stage As New SqlConnection(connectionString)
            If _stage.State = ConnectionState.Closed Then _stage.Open()

            Dim _dataTables As New List(Of String)

            For Each item As String In objectType



                Dim _selectSql As SqlCommand = _stage.CreateCommand
                _selectSql.CommandText = "SELECT COLUMN_NAME,DATA_TYPE, " & _
                                        " DataLenght = " & _
                                                   " CASE DATA_TYPE " & _
                                                   " WHEN 'int' THEN NUMERIC_PRECISION " & _
                                                   " ELSE CHARACTER_MAXIMUM_LENGTH " & _
                                                " END, IS_NULLABLE =  CASE IS_NULLABLE " & _
                                                    " when 'YES' then 'True' " & _
                                                    " else 'False' " & _
                                                "    end " & _
                                                  " FROM information_schema.COLUMNS " & _
                                            " where TABLE_NAME='" & item & "'"


                Dim _selectSqlDR As SqlDataReader = _selectSql.ExecuteReader()
                If _selectSqlDR.HasRows Then
                    _dataObject = New DataObject()
                    _dataObject.objectName = item
                    _dataObject.tableName = item
                    _dataObject.objectNamespace = "com.example"
                    _dataObject.keyDelimeter = "_"
                    Do While _selectSqlDR.Read()
                        '' _dataTables.Add(_selectSqlDR.Item("TABLE_NAME"))
                        If _selectSqlDR.Item("COLUMN_NAME").ToString().ToUpper().EndsWith("TAG") Then
                            _keyproperties = New KeyProperty()
                            _keyproperties.keyPropertyName = _selectSqlDR.Item("COLUMN_NAME")
                            _dataObject.keyProperties.Add(_keyproperties)
                        End If
                        _dataproperties = New DataProperty()
                        _dataproperties.columnName = _selectSqlDR.Item("COLUMN_NAME")
                        _dataproperties.dataLength = _selectSqlDR.Item("DataLenght")
                        _dataproperties.isNullable = Convert.ToBoolean(_selectSqlDR.Item("IS_NULLABLE"))
                        _dataproperties.keyType = KeyType.unassigned
                        _dataproperties.propertyName = _selectSqlDR.Item("COLUMN_NAME")

                        Select Case _selectSqlDR.Item("DATA_TYPE")
                            Case "bool"
                                _dataproperties.dataType = DataType.Char
                            Case "int"
                                _dataproperties.dataType = DataType.Int32
                            Case "datetime"
                                _dataproperties.dataType = DataType.DateTime
                            Case Else
                                _dataproperties.dataType = DataType.Char
                                Exit Select
                        End Select

                        _dataObject.dataProperties.Add(_dataproperties)


                        '_dataObject.dataProperties.Clear()
                        '_dataObject.keyProperties.Clear()
                    Loop
                End If

                _dataDictionary.dataObjects.Add(_dataObject)
                _selectSqlDR.Close()
            Next
            Return _dataDictionary
        Catch ex As Exception
            _logger.Debug("Error in LoadDataObjects: " & ex.Message)
            '_logger.[Error]("Error in LoadDataObjects: " & ex.ToString())
            Throw New Exception("Error while loading data objects " & ex.Message)
        End Try
        Return Nothing
    End Function

    Private Function SaveDataObjects(ByVal objectType As String, ByVal dataObjects As IList(Of IDataObject)) As Response
        Dim response As New Response
        Dim errorMessage As String
        errorMessage = "The function of SaveDataObjects is not implemented "
        _logger.Error(errorMessage)
        Dim status As New Status
        status.Level = StatusLevel.Error
        status.Messages = New Messages()
        status.Messages.Add(errorMessage)
        response.Level = StatusLevel.Error
        response.StatusList.Add(status)
        Return response
    End Function

    'Private Function skipDwg( _
    '      ByRef rep As LMRepresentation, _
    '      ByRef errMsgs As String) As Boolean

    '    Dim dwg As LMDrawing
    '    Dim dwgNo As String
    '    Dim filespec As String
    '    Dim m_plantPath As String = getPlantPath()

    '    skipDwg = False

    '    ' Get the drawing filename. If no drawing it's in the project stockpile.
    '    dwg = rep.DrawingObject
    '    If Not dwg Is Nothing Then
    '        filespec = m_plantPath & dwg.Attributes("Path").Name

    '        ' See if file is open
    '        If isFileLocked(filespec) Then
    '            dwgNo = dwg.Attributes("DrawingNumber").Value
    '            'errMsgs.add("Drawing " & dwgNo & " is open")
    '            skipDwg = True
    '        End If
    '    End If
    'End Function

    Private Sub LoadConfiguration()
        If _configuration Is Nothing Then
            Try


                Dim uri As String = [String].Format("{0}Configuration.{1}.xml", _settings("XmlPath"), _settings("ApplicationName"))

                Dim configDocument As XDocument = XDocument.Load(uri)
                _configuration = configDocument.Element("configuration")
            Catch ex As Exception
                _logger.Debug(ex.Message)

            End Try
        End If
    End Sub

    Private Function GetCommodityConfig(ByVal objectType As String) As XElement
        If _configuration Is Nothing Then
            LoadConfiguration()
        End If
        objectType = "Equipment"
        Dim commodityConfig As XElement = _configuration.Elements("commodities").Elements("commodity").Where(Function(o) o.FirstAttribute.Value = objectType).First()

        Return commodityConfig
    End Function

    'Private Function fetchEquipment(objEquipment As LMEquipment, DataObject As IDataObject, objectType As String) As Boolean



    '    Dim fetchEquioment As Boolean
    '    Dim rep As LMRepresentation
    '    Dim drawing As LMDrawing
    '    Dim attr As LMAAttribute
    '    Dim inStockpile As Boolean
    '    Dim dwgId As String
    '    Dim spId As String
    '    Dim CantPossiblyBeARealName As String = "toastandjam"

    '    fetchEquioment = True

    '    ' Skip if no Representation
    '    If objEquipment.Representations.Count = 0 Then
    '        fetchEquipment = False
    '        Exit Function
    '    End If

    '    rep = objEquipment.Representations.Nth(1)
    '    drawing = rep.DrawingObject

    '    ' See if it's in the project or drawing stockpile.
    '    attr = rep.Attributes("InStockpile")
    '    inStockpile = attr.Value = "True"

    '    If inStockpile Then
    '        If Not drawing Is Nothing Then
    '            'If Not m_exposeDwgStockpile("Equipment") Then
    '            '    fetchEquipment = False
    '            '    Exit Function
    '            'End If
    '        End If
    '    End If

    '    ' Drawing attributes
    '    dwgId = rep.DrawingID

    '    ' Skip this component if querying by dwg and it's not on the first dwg.
    '    Dim m_queriedDrawingId = getDrawingID(dwgId)
    '    Dim _attr = objEquipment.Attributes("toastandjam")
    '    'If m_queriedByDrawing And dwgId <> m_queriedDrawingId Then
    '    '    fetchEquipment = False
    '    '    Exit Function
    '    'End If

    '    ' Representation
    '    For Each attr In rep.Attributes
    '        addAttrSP(DataObject, attr, , "Representation", , objectType)
    '    Next attr

    '    ' Commodity-specific attributes
    '    ' First find the subclass of this equipment
    '    Dim equipType As String
    '    equipType = objEquipment.Attributes("ItemTypeName").Value

    '    spId = objEquipment.Id
    '    ' Don't think you can expand the case attributes for just the base equipment
    '    Select Case equipType
    '        Case "Exchanger"
    '            Dim equipExchanger As LMExchanger
    '            equipExchanger = _projDatasource.GetExchanger(spId)

    '            ' Expand Attributes collection to include all Case properties
    '            attr = equipExchanger.Attributes("toastandjam")

    '            For Each attr In equipExchanger.Attributes
    '                addAttrSP(DataObject, attr, , equipType, , objectType)
    '            Next attr

    '            equipExchanger = Nothing
    '        Case "Mechanical"
    '            Dim equipMechanical As LMMechanical
    '            equipMechanical = _projDatasource.GetMechanical(spId)

    '            ' Expand Attributes collection to include all Case properties
    '            attr = equipMechanical.Attributes(CantPossiblyBeARealName)

    '            For Each attr In equipMechanical.Attributes
    '                addAttrSP(DataObject, attr, , equipType, , objectType)
    '            Next attr

    '            equipMechanical = Nothing
    '        Case "Vessel"
    '            Dim equipVessel As LMVessel
    '            equipVessel = _projDatasource.GetVessel(spId)

    '            ' Expand Attributes collection to include all Case properties
    '            attr = equipVessel.Attributes(CantPossiblyBeARealName)

    '            For Each attr In equipVessel.Attributes
    '                addAttrSP(DataObject, attr, , equipType, , objectType)
    '            Next attr

    '            equipVessel = Nothing
    '        Case "EquipmentOther"
    '            Dim equipOther As LMEquipmentOther
    '            equipOther = _projDatasource.GetEquipmentOther(spId)

    '            ' Expand Attributes collection to include all Case properties
    '            attr = equipOther.Attributes(CantPossiblyBeARealName)

    '            For Each attr In equipOther.Attributes
    '                addAttrSP(DataObject, attr, , equipType, , objectType)
    '            Next attr

    '            equipOther = Nothing
    '        Case "EquipComponent"
    '            'If m_skipEquipComponents Then
    '            '    fetchEquipment = False
    '            '    Exit Function
    '            'Else
    '            ' Expand Attributes collection to include all Case properties

    '            attr = objEquipment.Attributes(CantPossiblyBeARealName)

    '            For Each attr In objEquipment.Attributes
    '                addAttrSP(DataObject, attr, , equipType, , objectType)
    '            Next attr
    '            'End If
    '        Case Else   ' shouldn't be anything else
    '            fetchEquipment = False
    '            Exit Function
    '    End Select

    '    ' Get the drawing attributes. If no drawing it's in the project stockpile.
    '    If drawing Is Nothing Then
    '        ' Fake the drawing number
    '        'addAttr(xmlDoc, DrawingNumberTag, StockpileTag, , TagDrawing)
    '        'addAttr(xmlDoc, NameTag, StockpileTag, , TagDrawing)
    '        'addAttr(xmlDoc, DescriptionTag, StockpileTag, , TagDrawing)
    '        'addAttr(xmlDoc, TitleTag, StockpileTag, , TagDrawing)
    '    Else
    '        For Each attr In drawing.Attributes
    '            addAttrSP(DataObject, attr, , "Drawing", True, objectType)
    '        Next attr
    '    End If

    '    ' Symbol
    '    Dim symbol = _projDatasource.GetSymbol(rep.Id)
    '    For Each attr In symbol.Attributes
    '        addAttrSP(DataObject, attr, , "Symbol", True, objectType)
    '    Next attr
    '    symbol = Nothing

    '    rep = Nothing

    '    ' Nozzle
    '    If objEquipment.Nozzles.Count > 0 Then
    '        Dim nozzle As LMNozzle
    '        nozzle = objEquipment.Nozzles.Nth(1)
    '        For Each attr In nozzle.Attributes
    '            addAttrSP(DataObject, attr, , "Nozzle", , objectType)
    '        Next attr
    '        nozzle = Nothing
    '    End If

    '    ' Parent Tag
    '    Dim parentTag As String
    '    If Not objEquipment.PartOfPlantItemObject Is Nothing Then
    '        If Not IsDBNull(objEquipment.PartOfPlantItemObject.Attributes("ItemTag").Value) Then
    '            parentTag = objEquipment.PartOfPlantItemObject.Attributes("ItemTag").Value
    '            ' addAttrSP(DataObject, "Parent", parentTag, , "Adapter")
    '            addAttrSP(DataObject, attr, , "Adapter", , objectType)
    '        End If
    '    End If


    '    Return fetchEquioment
    'End Function

    'Sub addAttrSP(dataObject As IDataObject, attr As LMAAttribute, Optional subclass As String = "", Optional src As String = "", _
    '    Optional ByVal displayedOnly As Boolean = False, Optional objectType As String = "")


    '    Dim useAltValue As Boolean
    '    Dim enumAttrs As ISPEnumeratedAttributes
    '    Dim attrValue As Object
    '    Dim intCount As Integer
    '    Dim value As String

    '    Dim commodityElement As XElement = GetCommodityConfig(objectType)
    '    Dim attributeElements As IEnumerable(Of XElement) = commodityElement.Element("attributes").Elements("attribute")

    '    attrValue = attr.Value

    '    Debug.WriteLine(src & "--->" & attr.Name)
    '    ' Skip hidden attributes
    '    If Not skipAttribute(attr, displayedOnly) Then
    '        '  If isAttrRequested(attr.Name, subclass, src, useAltValue) Then
    '        If useAltValue Then
    '            ' See if attribute has a select list.
    '            enumAttrs = attr.ISPAttribute.Attribution.ISPEnumAtts
    '            If Not enumAttrs Is Nothing Then
    '                ' .Name is long value, .Description is short value
    '                attrValue = enumAttrs.Item(CStr(attr.Index)).Description    ' Bin Lin 11/10/2008
    '            End If
    '        End If

    '        'If Not IsDBNull(attrValue) Then
    '        '    dataObject.SetPropertyValue(attr.Name, attrValue)
    '        'End If
    '        'End If

    '        '---------------
    '        '' Get Equipment Attributes------------------
    '        For Each attributeElement In attributeElements  'xml
    '            intCount = 0
    '            If (attributeElement.Attribute("name").Value = attr.Name Or attributeElement.Attribute("nativeName").Value = attr.Name) Then
    '                intCount = 1
    '                If Not IsDBNull(attr.Value) Then
    '                    value = attr.Value
    '                Else
    '                    value = "Null"
    '                End If
    '                dataObject.SetPropertyValue(attributeElement.Attribute("name").Value, value)
    '                Exit Sub
    '            End If

    '            'If (intCount = 0) Then
    '            '    dataObject.SetPropertyValue(attributeElement.Attribute("name").Value, "Null")
    '            'End If
    '        Next
    '    End If
    '    'If Not IsDBNull(DataObjects) Then
    '    '    DataObjects.Add(DataObjects)
    '    'End If
    '    '---------------

    'End Sub

    'Private Function skipAttribute( _
    '    ByRef attr As LMAAttribute, _
    '    Optional ByVal displayedOnly As Boolean = False) As Boolean

    '    skipAttribute = False

    '    Select Case attr.ISPAttribute.Attribution.Displayable.ToString()
    '        Case "spInternalAtt"
    '            skipAttribute = displayedOnly Or m_skipInternalAttributes
    '        Case "spNoDisplayAtt"
    '            skipAttribute = displayedOnly Or m_skipNoDisplayAttributes
    '    End Select
    'End Function

    'Private Function getDrawingID( _
    '    ByVal dwgNo As String)

    '    Const funcName As String = "getDrawingID"

    '    Dim dwgFilter As New LMAFilter
    '    Dim criteriaName As String

    '    dwgFilter.ItemType = "Drawing"

    '    criteriaName = "dwg"
    '    dwgFilter.Criteria.AddNew(criteriaName)
    '    dwgFilter.Criteria.Item(criteriaName).SourceAttributeName = "SP_ID"
    '    dwgFilter.Criteria.Item(criteriaName).ValueAttribute = dwgNo
    '    dwgFilter.Criteria.Item(criteriaName).Operator = "="
    '    dwgFilter.Criteria.Item(criteriaName).Conjunctive = True

    '    Dim drawings As New LMDrawings
    '    drawings.Collect(_projDatasource, Filter:=dwgFilter)
    '    If drawings.Count <> 1 Then
    '        Err.Raise(vbObjectError + 1, funcName, "Drawing " & dwgNo & " not found")
    '    End If

    '    getDrawingID = drawings.Nth(1).Id

    '    dwgFilter = Nothing
    '    drawings = Nothing
    'End Function

    'Private Function isFileLocked( _
    '    ByRef filespec As String) As Boolean

    '    ' If the file is already opened by another process and the specified type of access
    '    ' is not allowed the Open operation fails and an error occurs.
    '    On Error Resume Next
    '    isFileLocked = False

    '    Dim f As Integer
    '    f = FreeFile()

    '    '  Open filespec For Binary Access Read Lock Read Write As #f

    '    ' Check for "Permission Denied"
    '    If Err.Number = 70 Then
    '        isFileLocked = True
    '    End If

    '    ' Close #f
    'End Function

    'Private Function getPlantPath()
    '    ' Get "Plant Path" from PlantSettings


    '    Dim pathFilter As New LMAFilter
    '    Dim criterion As New LMACriterion

    '    criterion.SourceAttributeName = "Name"
    '    criterion.ValueAttribute = "Plant Path"
    '    criterion.Operator = "="

    '    pathFilter.ItemType = "PlantSetting"
    '    pathFilter.Criteria.Add(criterion)

    '    Dim plantSettings As New LMPlantSettings
    '    Dim plantSetting As LMPlantSetting
    '    plantSettings.Collect(_projDatasource, Filter:=pathFilter)

    '    plantSetting = plantSettings.Nth(1)

    '    getPlantPath = plantSetting.Attributes("Value")

    '    pathFilter = Nothing
    '    criterion = Nothing
    '    plantSetting = Nothing
    '    plantSettings = Nothing


    '    Exit Function

    'End Function

#End Region




End Class


