Option Explicit On
Option Compare Text

Imports System.Runtime.CompilerServices
Imports System.Text
Imports System.Reflection
Imports System.Text.RegularExpressions
'Imports System.Data.OracleClient
Imports Oracle.DataAccess.Client

Imports log4net
Public Module Common

#Region " Constants "

    Public Const nl As String = vbCrLf
    Public Const tb As String = "    "
    Public Const tb2 As String = tb & tb
    Public Const tb3 As String = tb2 & tb
    Public Const nltb As String = nl & tb
    Public Const nltb2 As String = nltb & tb
    Public Const nltb3 As String = nltb2 & tb

#End Region

#Region " Enumerations "

    Public Enum SPSchemaType

        SITE = 0
        SPAPLANT = 1
        DATA_DICTIONARY = 2
        SPPID = 3
        SPPIDDATA_DICTIONARY = 4

    End Enum

    Public Enum SPSchemaTypeforOracle

        SITE = 0
        DATA_DICTIONARY = 1
        SPAPLANT = 2
        SPPIDDATA_DICTIONARY = 3
        SPPID = 4

    End Enum

    ''' <summary>
    ''' Note: the order that these clauses are assigned is important to building a query
    ''' </summary>
    ''' <remarks></remarks>
    Public Enum SQLClause

        TableDef = -2
        StagingName = -1
        QueryName = 0
        [Declare] = 1
        [Set] = 2
        [Select] = 3
        Into = 4
        [From] = 5
        [Where] = 6
        GroupBy = 7
        [Having] = 8
        OrderBy = 9

    End Enum

#End Region

#Region " Structures "

    ''' <summary>
    ''' Represents a uniquely defined table or column name
    ''' </summary>
    ''' <remarks>Also provided functionality for determining whether a column value should be quoted or not</remarks>
    Public Structure SQLUnique

#Region " Variables "
        Dim _type As SqlDbType
        Dim _column As String
        Dim _table As String
        Dim _schema As String
        Private IsInitialized As Boolean

#End Region

#Region " Properties "

        Public ReadOnly Property UniqueName As String
            Get
                Return _schema & "." & _table & IIf(_column = "", "", "." & _column)
            End Get
        End Property

        ''' <summary>
        ''' Returns the data type. 
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks>The data type for tables is 'structured'</remarks>
        Public ReadOnly Property DataType As SqlDbType
            Get
                Return _type
            End Get
        End Property

        Public ReadOnly Property ColumnName As String
            Get
                Return _column
            End Get
        End Property

        Public ReadOnly Property TableName As String
            Get
                Return _table
            End Get
        End Property

        Public ReadOnly Property SchemaName As String
            Get
                Return _schema
            End Get
        End Property

#End Region

#Region " Instantiation "

        Public Sub New(schemaName As String, tableName As String, Optional columnName As String = "", _
                       Optional DataType As SqlDbType = SqlDbType.Structured)

            _schema = schemaName
            _table = tableName
            _column = columnName
            _type = DataType

        End Sub

#End Region

#Region " Public Methods "

        Public Function IsQuotable() As Boolean

            Select Case _type

                Case SqlDbType.Text, SqlDbType.NText, SqlDbType.Char, SqlDbType.Date,
                    SqlDbType.DateTime, SqlDbType.DateTime2, SqlDbType.DateTimeOffset,
                    SqlDbType.DateTimeOffset, SqlDbType.Time, SqlDbType.UniqueIdentifier,
                    SqlDbType.VarChar, SqlDbType.NVarChar, SqlDbType.Xml

                    Return True

                Case Else : Return False

            End Select

        End Function

        ''' <summary>
        ''' determines if a value of a particular type should be quoted or not
        ''' </summary>
        ''' <param name="SQLDataType"></param>
        ''' <returns></returns>
        ''' <remarks>This function is not very robust - should check to verify that the string represents something that
        ''' can actually come from information_schema.columns. If not, an error should be thrown</remarks>
        Public Shared Function IsQuotable(ByVal SQLDataType As String) As Boolean

            Dim i As Integer

            If SQLDataType.Length < 3 Then Return False

            ' the datatype name should appear at the beginning. if it contains any additional information in parens, 
            ' remove this section
            i = InStr(SQLDataType, "(")
            If i > 0 Then SQLDataType = Trim(Mid(SQLDataType, 1, i - 1))

            Select Case SQLDataType

                Case "text", "ntext", "char", "date", "datetime", "datetime2", "datetimeoffset",
                    "time", "uniqueidentifier", "varchar", "nvarchar", "xml"

                    Return True

                Case Else : Return False

            End Select

        End Function

#End Region

    End Structure

#End Region

#Region " Extension Methods "

    ''' <summary>
    ''' Assemble the query according to the order of the sql clauses defined by the SQLClause Enumeration
    ''' Empty clauses and clauses consisting of only the clause start word are skipped
    ''' </summary>
    ''' <param name="Parts"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension()> _
    Public Function BuildQuery(Parts As Dictionary(Of SQLClause, String),
                               ByRef QueryText As String,
                               Optional TextVariables As IEnumerable(Of XElement) = Nothing,
                               Optional ReplacementValues As Dictionary(Of String, String) = Nothing,
                               Optional SelectInto As Boolean = False
                               ) As String

    Dim exists As Boolean
        Dim clause As String = ""
        Dim sb As New StringBuilder
        Dim qParts As Array = [Enum].GetValues(GetType(SQLClause))
        Dim varName As String
        Dim varValue As String = ""
        Dim varKey As String = ""
    Dim pat As String
    Dim _logger As ILog = LogManager.GetLogger(GetType(SPPIDDataLayer))


        For Each part As SQLClause In qParts

            ' skip the query names while building the query
            If part < SQLClause.Declare Then Continue For
            If part = SQLClause.Into AndAlso Not SelectInto Then Continue For

            exists = Parts.TryGetValue(part, clause)
            If exists AndAlso clause.UnpaddedCount > (Len(part.ToString) + 4) Then sb.Append(clause & nl & nl)

        Next

        ' look for any text strings requiring replacement
        QueryText = sb.ToString

        Try

            If TextVariables IsNot Nothing Then

                ' not really sure at this point whether we should indicate an error or warning if substitutions are not provided.
                ' the StagingConfiguration schema supports a "value" property where default values can be provided, so it's not 
                ' necessarily an error

                ' use the textVariables collection to replace only where necessary
                For Each e As XElement In TextVariables

                    varName = e.Attribute("name").Value
                    varKey = Parts(SQLClause.QueryName) & "." & varName
                    exists = ReplacementValues.TryGetValue(varKey, varValue)
                    varKey = "!!All." & varName
                    If Not exists Then exists = ReplacementValues.TryGetValue(varKey, varValue)

                    ' use the default value if there is nothing else
                    If Not exists Then varValue = e.Attribute("value").Value

                    ' this pattern attempts to isolate the varaible to "whole word only" by screening out variable names followed
                    ' by characters legal for use in SQL identifiers.  This is really only a potential
                    ' problem if there are variable names, one of which is a subset of another, and the longer variable is not 
                    ' processed first. Since we are not ordering the replacement, this method should screen out the vast majority
                    ' of cases where this could pose a problem. 
                    pat = varName & "([^a-zA-Z0-9_\-\@\#\$])"
                    QueryText = Regex.Replace(QueryText, pat, varValue & "$1", RegexOptions.IgnoreCase)
                    'sb = sb.Replace(varName, varValue)

                Next

                ' QueryText = sb.ToString

      End If
      Return "Pass"

    Catch ex As Exception
      _logger.Debug("Fail: (BuildQuery) " & ex.Message)
      Return "Fail: (BuildQuery) " & ex.Message
        End Try
  End Function

    <Extension()> _
    Public Function GetDataTypeString(Row As SQLSchemaDS.SchemaColumnsRow) As String

        Dim s As String

        Select Case Row.DataType

            Case "numeric", "decimal"
                s = Row.DataType & "(" & Row.NumericPrecision & ", " & Row.NumericScale & ")"

            Case "varchar", "nvarchar", "char", "nchar", "binary", "varbinary"

                If Row.CharMaxLength > -1 Then
                    s = Row.DataType & "(" & Row.CharMaxLength & ")"
                Else
                    s = Row.DataType & "(MAX)"
                End If

            Case Else
                s = Row.DataType

        End Select

        Return s

    End Function
    <Extension()> _
    Public Function GetDataTypeString(Row As OraSchemaDS.SchemaColumnsRow) As String

        Dim s As String

        Select Case Row.DataType

            Case "numeric", "decimal"
                s = Row.DataType & "(" & Row.NumericPrecision & ", " & Row.NumericScale & ")"

            Case "varchar", "nvarchar", "char", "nchar", "binary", "varbinary"

                If Row.CharMaxLength > -1 Then
                    s = Row.DataType & "(" & Row.CharMaxLength & ")"
                Else
                    s = Row.DataType & "(MAX)"
                End If

            Case Else
                s = Row.DataType

        End Select

        Return s

    End Function

    <Extension()> _
    Public Function SetDeclarationValues(Parts As Dictionary(Of SQLClause, String), Declarations As IEnumerable(Of XElement), _
                                    ValueDictionary As Dictionary(Of String, String), _
                                    _logger As log4net.ILog) As String

        Dim val As String = ""
        Dim s2 As String = ""
        Dim exists As Boolean
        Dim rVal As New StringBuilder
        Dim varName As String
        Dim s As New StringBuilder
        Dim quoteIt As Boolean
        Dim dType As String
        Dim qName As String
        Dim defaultVal As String

        Try

            qName = Parts(SQLClause.QueryName)

            ' use the declarations to determine what values need to be set; the value dictionary may contain
            ' value for variables not used in this query.
            For Each e As XElement In Declarations

                varName = e.Attribute("name").Value
                dType = e.Attribute("datatype").Value

                ' look for a value provided specifically for this query
                exists = ValueDictionary.TryGetValue("!" & qName & "." & varName, val)

                If Not exists Then

                    ' look for a variable provided for any any query
                    exists = ValueDictionary.TryGetValue("!!All." & varName, val)

                    ' use the default value if a value has not been provided. Note that providing a default value in the declaration
                    ' will force the use of any filter (where clause expression) where the optional attribute is set to true;
                    ' THUS, giving a default value overrides the "optional" flag. 
                    If Not exists Then

                        defaultVal = e.Attribute("value").Value

                        If defaultVal = "" Then

                            defaultVal = "!NoValue"
                            _logger.Warn("Variable '" & varName & "' was not provided with a value;")

                        Else
                            _logger.Warn("Variable '" & varName & "' was not provided with a value; the default will be used")
                        End If

                        val = defaultVal

                    End If

                End If

                If val <> "!NoValue" Then

                    ' if no datatype is provided, then quote the value if it's non-numeric
                    ' This softens the StagingConfiguration schema requirement for a datatype but is NOT foolproof. 
                    If dType = "" Then
                        quoteIt = Not IsNumeric(val)
                    Else
                        quoteIt = SQLUnique.IsQuotable(dType)
                    End If

                    s.Append(nltb & "SET " & varName & "=" & IIf(quoteIt, "'", "") & val & IIf(quoteIt, "'", ""))

                End If

            Next

            exists = Parts.TryGetValue(SQLClause.Set, s2)

            If exists Then
                Parts(SQLClause.Set) = s.ToString
            Else
                Parts.Add(SQLClause.Set, s.ToString)
            End If
      Return "Pass"
    Catch ex As Exception
      _logger.Debug("Fail: (" & MethodBase.GetCurrentMethod.Name & "): " & ex.Message)
      Return "Fail: (" & MethodBase.GetCurrentMethod.Name & "): " & ex.Message
        End Try



    End Function

    ''' <summary>
    ''' removes ALL leading and trailing space characters including tab and new line chars
    ''' </summary>
    ''' <param name="CharSequence"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension()> _
    Public Function TrimToChars(CharSequence As String) As String

        Dim sb As New StringBuilder
        Dim ct As Integer = 0
        Dim ct2 As Integer = 0
        Dim s2 As String
        Dim e As IEnumerable(Of Char)

        For Each c As Char In CharSequence

            Select Case c

                Case vbCr, vbCrLf, vbLf, vbTab, " "
                    ct += 1
                Case Else
                    Exit For

            End Select

        Next

        If ct = CharSequence.Length Then Return ""
        s2 = Mid(CharSequence, ct + 1)
        e = s2.Reverse

        For Each c As Char In e

            Select Case c

                Case vbCr, vbCrLf, vbLf, vbTab, " "
                    ct2 += 1
                Case Else
                    Exit For

            End Select

        Next

        If ct2 = s2.Length Then Return ""
        If (ct + ct2) >= CharSequence.Length Then Return ""
        Return Mid(CharSequence, ct + 1, CharSequence.Length - (ct + ct2))


    End Function

    ''' <summary>
    ''' returns the number of ascii characters a-zA-Z-_0-9 and any non-leading spaces
    ''' excludes any leading tabs, spaces, and new line characters
    ''' </summary>
    ''' <param name="CharSequence"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension()> _
    Public Function UnpaddedCount(CharSequence As String) As Integer

        Dim ct As Integer
        Dim uct As Integer = 0
        Dim seqR As IEnumerable(Of Char)

        ct = Len(CharSequence)

        ' count any leading undesirables
        For Each c As Char In CharSequence

            If c = vbCr OrElse c = vbCrLf OrElse c = " " OrElse c = vbTab Then
                uct += 1
            Else
                Exit For
            End If

        Next

        seqR = CharSequence.Reverse

        ' count any trailing undesirables
        For Each c As Char In seqR

            If c = vbCr OrElse c = vbCrLf OrElse c = " " OrElse c = vbTab Then
                uct += 1
            Else
                Exit For
            End If

        Next

        Return ct - uct

    End Function

#End Region

#Region " Common Methods "

    ''' <summary>
    ''' Gave Grants to all Schema so that single Select Query can acces all tables
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub ProvideGrants(ByVal _plantConnOracle As OracleConnection, ByVal _plantDicConnOracle As OracleConnection, ByVal _PIDDicConnOracle As OracleConnection, ByVal _PIDConnOracle As OracleConnection)
        Dim cmdOra As OracleCommand

        ' Gave Select Grants to other Schemas
        cmdOra = New OracleCommand()
        Dim OraDR As OracleDataReader
        If _plantConnOracle.State = ConnectionState.Closed Then _plantConnOracle.Open()
        cmdOra = _plantConnOracle.CreateCommand()
        cmdOra.CommandText = "select sys_context('USERENV', 'CURRENT_SCHEMA') CURRENT_SCHEMA from dual"
        OraDR = cmdOra.ExecuteReader
        Dim str As String = String.Empty
        If OraDR.HasRows Then
            Do While OraDR.Read()
                str = OraDR.Item("CURRENT_SCHEMA")
            Loop
        End If

        cmdOra = New OracleCommand()
        If _plantDicConnOracle.State = ConnectionState.Closed Then _plantDicConnOracle.Open()
        cmdOra = _plantDicConnOracle.CreateCommand()
        Dim strQuery As String = "BEGIN" & _
        " FOR x IN (Select * from tab  where TABTYPE='TABLE') LOOP " & _
        " EXECUTE IMMEDIATE 'GRANT SELECT ON ' || x.Tname || ' TO " & str & "';" & _
        " END LOOP;" & _
        " END; "
        cmdOra.CommandText = strQuery
        cmdOra.ExecuteNonQuery()

        cmdOra = New OracleCommand()
        If _PIDConnOracle.State = ConnectionState.Closed Then _PIDConnOracle.Open()
        cmdOra = _PIDConnOracle.CreateCommand()
        strQuery = "BEGIN" & _
        " FOR x IN (Select * from tab  where TABTYPE='TABLE') LOOP " & _
        " EXECUTE IMMEDIATE 'GRANT SELECT ON ' || x.Tname || ' TO " & str & "';" & _
        " END LOOP;" & _
        " END; "
        cmdOra.CommandText = strQuery
        cmdOra.ExecuteNonQuery()

        cmdOra = New OracleCommand()
        If _PIDDicConnOracle.State = ConnectionState.Closed Then _PIDDicConnOracle.Open()
        cmdOra = _PIDDicConnOracle.CreateCommand()
        strQuery = "BEGIN" & _
        " FOR x IN (Select * from tab  where TABTYPE='TABLE') LOOP " & _
        " EXECUTE IMMEDIATE 'GRANT SELECT ON ' || x.Tname || ' TO " & str & "';" & _
        " END LOOP;" & _
        " END; "
        cmdOra.CommandText = strQuery
        cmdOra.ExecuteNonQuery()

    End Sub


    ''' <summary>
    ''' Revoke Grants from all Schemas 
    ''' </summary>
    Public Sub RevokeGrants(ByVal _plantConnOracle As OracleConnection, ByVal _plantDicConnOracle As OracleConnection, ByVal _PIDDicConnOracle As OracleConnection, ByVal _PIDConnOracle As OracleConnection)
        Dim cmdOra As OracleCommand

        ' Gave Select Grants to other Schemas
        cmdOra = New OracleCommand()
        Dim OraDR As OracleDataReader
        If _plantConnOracle.State = ConnectionState.Closed Then _plantConnOracle.Open()
        cmdOra = _plantConnOracle.CreateCommand()
        cmdOra.CommandText = "select sys_context('USERENV', 'CURRENT_SCHEMA') CURRENT_SCHEMA from dual"
        OraDR = cmdOra.ExecuteReader
        Dim str As String = String.Empty
        If OraDR.HasRows Then
            Do While OraDR.Read()
                str = OraDR.Item("CURRENT_SCHEMA")
            Loop
        End If

        cmdOra = New OracleCommand()
        If _plantDicConnOracle.State = ConnectionState.Closed Then _plantDicConnOracle.Open()
        cmdOra = _plantDicConnOracle.CreateCommand()
        Dim strQuery As String = "BEGIN" & _
        " FOR x IN (Select * from tab  where TABTYPE='TABLE') LOOP " & _
        " EXECUTE IMMEDIATE 'REVOKE SELECT ON ' || x.Tname || ' FROM " & str & "';" & _
        " END LOOP;" & _
        " END; "
        cmdOra.CommandText = strQuery
        cmdOra.ExecuteNonQuery()

        cmdOra = New OracleCommand()
        If _PIDConnOracle.State = ConnectionState.Closed Then _PIDConnOracle.Open()
        cmdOra = _PIDConnOracle.CreateCommand()
        strQuery = "BEGIN" & _
         " FOR x IN (Select * from tab  where TABTYPE='TABLE') LOOP " & _
         " EXECUTE IMMEDIATE 'REVOKE SELECT ON ' || x.Tname || ' FROM " & str & "';" & _
         " END LOOP;" & _
         " END; "
        cmdOra.CommandText = strQuery
        cmdOra.ExecuteNonQuery()

        cmdOra = New OracleCommand()
        If _PIDDicConnOracle.State = ConnectionState.Closed Then _PIDDicConnOracle.Open()
        cmdOra = _PIDDicConnOracle.CreateCommand()
        strQuery = "BEGIN" & _
         " FOR x IN (Select * from tab  where TABTYPE='TABLE') LOOP " & _
         " EXECUTE IMMEDIATE 'REVOKE SELECT ON ' || x.Tname || ' FROM " & str & "';" & _
         " END LOOP;" & _
         " END; "
        cmdOra.CommandText = strQuery
        cmdOra.ExecuteNonQuery()

    End Sub
    Public Function GetQueryParts(ByVal QueryNode As XElement,
                                    ByVal ColumnsDV As DataView,
                                    ByVal TablesDV As DataView,
                                    ByVal SchemaSubstitutions As Dictionary(Of String, String),
                                    ByRef QueryParts As Dictionary(Of SQLClause, String),
                                    ByRef Replacements As IEnumerable(Of XElement),
                                    ByRef Declarations As IEnumerable(Of XElement),
                                    Optional ByRef DeclarationValues As Dictionary(Of String, String) = Nothing,
                                    Optional ByVal CommonServerName As String = "", Optional ByVal SiteDatabaseName As String = "") As String

        Dim dec, s, f, w, g, h, o, i, t As New StringBuilder
        Dim tabWidthAlias As Integer = 50
        Dim l As Integer = 50
        Dim queryTables As IQueryable(Of XElement)
        Dim tablesX As IEnumerable(Of XElement)
        Dim relations As IEnumerable(Of XElement)
        Dim filters As IEnumerable(Of XElement)
        Dim sorts As IEnumerable(Of XElement)
        Dim sourceAliasMap As New Dictionary(Of String, SQLUnique)
        Dim fieldAliasMap As New Dictionary(Of String, SQLUnique)
        Dim d As String = "."
        Dim q As New StringBuilder
        Dim tmpX As XElement = Nothing
        Dim tmpStr As String
        Dim source As String
        Dim colName As String
        Dim stagingFieldName As String
        Dim exists As Boolean
        Dim sourceAlias As String = ""
        Dim sourceUnique As SQLUnique = Nothing
        Dim quoteTheValue As Boolean = False
        Dim subs As Dictionary(Of String, String) = SchemaSubstitutions
        Dim conj As String = ""
        Dim svNm As String = ""
        Dim col As SQLSchemaDS.SchemaColumnsRow
        Dim lastSource As String = ""
        Dim dataType As String
        Dim colNull As String
        Dim isExpression As Boolean
        Dim vals As Dictionary(Of String, String) = DeclarationValues
        Dim lastChecked As String = ""
    Dim includeFilter As Boolean = False
    Dim _logger As ILog = LogManager.GetLogger(GetType(SPPIDDataLayer))

        ' init
        If CommonServerName <> "" Then svNm = "[" & CommonServerName & "]."

        Try

            ' fetch the SQL Schema information
            TablesDV.Sort = ""

            ' initialize the query part strings
            s.Append("SELECT ") : f.Append("FROM ") : t.Append("CREATE TABLE ")
            tmpX = QueryNode.Element("variables")

            ' Query Names ************************************************************************************************
            QueryParts.Add(SQLClause.QueryName, QueryNode.Attribute("name").Value)
            QueryParts.Add(SQLClause.StagingName, QueryNode.Attribute("stagingDestinationName").Value)
            t.Append(QueryNode.Attribute("stagingDestinationName").Value & " (" & nltb)

            ' DELCARE clause ************************************************************************************************

            ' fetch declarations
            Declarations = _
                From var In tmpX...<declaration> _
                Select var

            For Each de As XElement In Declarations

                If de IsNot Declarations.First Then dec.Append(",")
                dec.Append(de.Attribute("name").Value & tb2)
                dec.Append(de.Attribute("datatype").Value)

                If de Is Declarations.Last Then
                    dec.Append(";")
                Else
                    dec.Append(nltb)
                End If

            Next

            If dec.Length > 0 Then dec.Insert(0, "DECLARE " & nltb)
            QueryParts.Add(SQLClause.Declare, dec.ToString)

            ' Fetch textReplacements  ***************************************************************************************

            ' fetch text replacements
            Replacements = _
                From var In tmpX...<textReplacement> _
                Select var

            ' INTO clause ***************************************************************************************************

            i.Append(QueryNode.Attribute("stagingDestinationName").Value)
            If i.Length > 0 Then i.Insert(0, "INTO ")
            QueryParts.Add(SQLClause.Into, i.ToString)


            ' FROM clause ***************************************************************************************************

            ' fetch all tables
            tablesX = _
                From tb In QueryNode...<source> _
                Select tb
            queryTables = tablesX.AsQueryable

            For Each e As XElement In tablesX

                ' look for an alias mapping for this table
                exists = sourceAliasMap.TryGetValue(e.Attribute("alias").Value, sourceUnique)

                ' create one and add it to the dictionary if it doesn't exist
                If Not exists Then

                    sourceAlias = e.Attribute("alias").Value

                    If sourceAlias = "" Then
                        sourceAlias = e.Attribute("schema").Value & d & e.Attribute("name").Value
                    End If

                    ' skip deriving the schema; it will be a textReplacement
                    If subs Is Nothing _
                        OrElse e.Attribute("schema").Value.StartsWith("!@~") _
                        OrElse e.Attribute("schema").Value.StartsWith("@") Then
                        sourceAliasMap.Add(sourceAlias, New SQLUnique(e.Attribute("schema").Value, e.Attribute("name").Value))
                    Else

                        ' derive the correct schema for the table from schema substitutions dictionary 
                        'exists = [Enum].TryParse(e.Attribute("schema").Value, True, schemaType)

                        If e.Attribute("alias").Value = "" AndAlso subs IsNot Nothing Then
                            sourceAlias = subs(e.Attribute("schema").Value) & d & e.Attribute("name").Value
                        End If

                        sourceAliasMap.Add(sourceAlias, New SQLUnique(subs(e.Attribute("schema").Value), e.Attribute("name").Value))

                    End If

                End If

                f.Append(nltb)

                If e Is tablesX.First Then
                    f.Append(tb3)
                Else
                    If tablesX.Count > 1 Then f.Append(LCase(e.Attribute("joinType").Value) & " join ")
                End If

                If (SiteDatabaseName <> "") Then
                    source = svNm & SiteDatabaseName & "." & sourceAliasMap(sourceAlias).UniqueName
                Else
                    source = svNm & sourceAliasMap(sourceAlias).UniqueName
                End If

                l = IIf((Len(source) + Len(tb)) > (tabWidthAlias + 1), Len(source) + Len(tb2), tabWidthAlias)
                f.Append(LSet(source, l))
                If e.Attribute("alias").Value <> "" Then f.Append("as " & sourceAlias)

                If e IsNot tablesX.First Then

                    f.Append(nltb2 & " on " & tb)

                    relations = _
                        From r In e...<relation>
                        Select r

                    If relations IsNot Nothing Then

                        For Each r As XElement In relations

                            conj = r.Attribute("conjunction").Value
                            If conj <> "" Then f.Append(nltb3 & conj & " ")
                            f.Append(r.Attribute("leftSource").Value & d & _
                                     r.Attribute("leftField").Value & _
                                     r.Attribute("operator").Value)

                            tmpStr = r.Attribute("rightField").Value

                            If r.Attribute("joinToText") = "True" Then

                                tmpStr = tmpStr.Replace("'", "''")
                                f.Append("'" & tmpStr & "'")

                            Else

                                ' JoinToText should be FALSE if the join value is a number or variable
                                'If r.Attribute("rightSource").Value = "" Then
                                '    f.Append(r.Attribute("rightField").Value & " ")
                                'Else
                                f.Append(r.Attribute("rightSource").Value & "." & r.Attribute("rightField").Value & " ")
                                'End If

                            End If

                        Next

                    End If

                End If

            Next

            QueryParts.Add(SQLClause.From, f.ToString)

            ' SELECT and TableDef clauses ***********************************************************************************

            ' fetch selection modifiers
            Dim sels As IEnumerable(Of XElement) = QueryNode.Descendants("selection")

            ' update the selection string with any selection restrictions
            For Each e As XElement In sels : s.Append(e.Attribute("value").Value & " ") : Next
            s.Append(nltb)

            ' fetch all fields
            Dim fields As IEnumerable(Of XElement) = _
                QueryNode.Element("fields").Descendants("field")

            ' add fields to the selection string and ddl
            For Each e As XElement In fields

                source = e.Attribute("source").Value
                colName = e.Attribute("name").Value
                stagingFieldName = e.Attribute("alias").Value
                isExpression = Not (e.Attribute("expression") Is Nothing OrElse e.Attribute("expression").Value = "")

                If source <> lastSource Then

                    lastSource = source
                    If e IsNot fields.First Then s.Append(nl)

                End If

                ' update the select clause
                If isExpression Then : tmpStr = e.Attribute("expression").Value
                Else : tmpStr = source & d & "[" & colName & "]"
                End If

                If e IsNot fields.First Then tmpStr = "," & tmpStr

                l = IIf((Len(tmpStr) + Len(tb)) > (tabWidthAlias + 1), Len(tmpStr) + Len(tb2), tabWidthAlias)
                s.Append(nltb & LSet(tmpStr, l))

                If stagingFieldName = "" Then
                    stagingFieldName = colName
                Else
                    s.Append("as " & stagingFieldName)
                End If

                If isExpression Then

                    ' expressions may provide a datatype hint; if not, use nvarchar(max)
                    If e.Attribute("datatype").Value = "" Then
                        dataType = "nvarchar(MAX)"
                    Else
                        ' ToDo - it would be wise to verify this is a valid datatype here to catch typos
                        dataType = e.Attribute("datatype").Value
                    End If

                    colNull = " null"

                Else

                    ' look up the data type of the column if this is not an expression
                    exists = sourceAliasMap.TryGetValue(source, sourceUnique)

                    If exists Then

                        ColumnsDV.RowFilter = "TableSchema='" & sourceUnique.SchemaName & "' " & _
                        "and TableName='" & sourceUnique.TableName & "' " & _
                        "and ColumnName='" & colName & "'"

                        If ColumnsDV.Count <> 1 Then
                            Throw New InvalidExpressionException("The column '" & _
                                sourceUnique.UniqueName & d & colName & "' is not a " & _
                                "valid uniquely identified column; the query cannot be built")
                        End If

                        col = ColumnsDV(0).Row
                        dataType = col.GetDataTypeString

                        ' I would generally prefer to pay attention to the nullability of the source field, but left joins against the source table
                        ' make it possible for these values to be null even if the source table does not allow it. It's far safer to 
                        ' simply set the nullability to 'yes'
                        'colNull = IIf(col.IsNullable = "Yes", " null", " not null")
                        colNull = " null"

                    Else : Throw New InvalidExpressionException("The source value '" & source & _
                        "' is not a valid uniquely identified table or table reference; the query cannot be built")

                    End If

                End If

                If e IsNot fields.First Then t.Append(nltb & ",")
                t.Append(stagingFieldName & " " & dataType & colNull)

            Next

            t.Append(")")
            QueryParts.Add(SQLClause.TableDef, t.ToString)
            QueryParts.Add(SQLClause.Select, s.ToString)

            ' WHERE clause ***************************************************************************************************
            filters = QueryNode.Element("filters").Descendants

            For Each fil As XElement In filters

                tmpStr = fil.Attribute("filterValue").Value
                ' deal with optional filters
                ' check to see if the filter value contains variables
                If tmpStr.Contains("@") Then

                    includeFilter = FilterValuesProvided(QueryParts(SQLClause.QueryName), tmpStr, Declarations, DeclarationValues, lastChecked)

                    ' if no value is provided and the 'Optional' flag is not set to true then the query cannot be built
                    If Not includeFilter AndAlso (fil.Attribute("optional") Is Nothing OrElse fil.Attribute("optional").Value <> "true") Then

                        Throw New InvalidExpressionException("No value was provided for the declaration '" & lastChecked & _
                            "' without this value, a valid SQL data query cannot be built from query '" & QueryParts(SQLClause.QueryName))

                    End If

                End If

                ' add conjunction. Since it's possible for the first filter to be optional with no value provided, if this is the first valid
                ' filter, then the conjunction is ignored
                If fil.Attribute("conjunction").Value <> "" AndAlso w.ToString.TrimToChars.Length > 5 AndAlso includeFilter Then
                    w.Append(tb & fil.Attribute("conjunction").Value & tb)
                End If

                ' add begining parentheses; these get added regardless of whether the filter is included or not
                If fil.Attribute("preParenCount").Value <> "" AndAlso fil.Attribute("preParenCount").Value <> "0" Then
                    w.Append(StrDup(CInt(fil.Attribute("preParenCount").Value), "("))
                End If

                If includeFilter Then

                    ' decide whether to quote the value or not by checking to see if it is a variable or
                    ' if the field's data type makes it necessary. Variables are never quoted; however string substitutions may still be quoted
                    If tmpStr.Contains("@") Then
                        quoteTheValue = False
                    Else

                        ' look up the data type of the column
                        exists = sourceAliasMap.TryGetValue(fil.Attribute("source").Value, sourceUnique)

                        If exists Then

                            ColumnsDV.RowFilter = "TableSchema='" & sourceUnique.SchemaName & "' " & _
                            "and TableName='" & sourceUnique.TableName & "' " & _
                            "and ColumnName='" & fil.Attribute("fieldName").Value & "'"

                            If ColumnsDV.Count <> 1 Then
                                Throw New InvalidExpressionException("The column '" & _
                                    sourceUnique.UniqueName & d & fil.Attribute("fieldName").Value & "' is not a " & _
                                    "valid uniquely identified column; the query cannot be built")
                            End If

                            col = ColumnsDV(0).Row
                            quoteTheValue = SQLUnique.IsQuotable(col.DataType)

                        Else : Throw New InvalidExpressionException("The source value '" & fil.Attribute("source").Value & _
                            "' is not a valid uniquely identified table or table reference; the query cannot be built")

                        End If

                    End If

                    ' add filter clause
                    tmpStr = tmpStr.Replace("'", "''")
                    w.Append(fil.Attribute("source").Value & d & fil.Attribute("fieldName").Value)
                    w.Append(" " & fil.Attribute("operator").Value & IIf(quoteTheValue, " '", " "))
                    w.Append(tmpStr & IIf(quoteTheValue, "'", ""))

                End If

                ' add ending parentheses
                If fil.Attribute("postParenCount").Value <> "" AndAlso fil.Attribute("postParenCount").Value <> "0" Then
                    w.Append(StrDup(CInt(fil.Attribute("postParenCount").Value), ")"))
                End If

                w = w.Replace("()", "")
                w.Append(nltb)

            Next

            w = w.Replace("()", "")
            If w.ToString.TrimToChars.Length > 3 Then w.Insert(0, "WHERE " & nltb)
            QueryParts.Add(SQLClause.Where, w.ToString)

            ' HAVING and GROUP BY clause ****************************************************************************************
            ' NOTE: this section is not yet implemented - its unclear when or if these clauses will ever be used to
            ' extract data from SPPID

            ' ORDER BY clause ***************************************************************************************************
            sorts = QueryNode.Element("sorts").Descendants

            For Each sort As XElement In sorts

                If sort IsNot sorts.First Then o.Append(tb & ",")
                o.Append(IIf(sort.Attribute("source").Value = "", "", sort.Attribute("source").Value))
                o.Append(sort.Attribute("fieldName").Value)
                o.Append(IIf(sort.Attribute("sortDirection").Value = "", "", sort.Attribute("sortDirection").Value))

            Next

            If o.Length > 0 Then o.Insert(0, "ORDER BY " & nltb)
            QueryParts.Add(SQLClause.OrderBy, o.ToString)
      Return "Pass"
    Catch ex As Exception
      _logger.Debug(ex.Message)
      Return "Fail: " & ex.Message
    End Try



    End Function


    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="QueryNode"></param>
    ''' <param name="ColumnsDV"></param>
    ''' <param name="TablesDV"></param>
    ''' <param name="SchemaSubstitutions"></param>
    ''' <param name="QueryParts"></param>
    ''' <param name="Replacements"></param>
    ''' <param name="Declarations"></param>
    ''' <param name="CommonServerName">If not blank, this is name is prepended to each schema.table source in the FROM clause</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function GetQueryPartsforOracle(ByVal QueryNode As XElement,
                                    ByVal ColumnsDV As DataView,
                                    ByVal TablesDV As DataView,
                                    ByVal SchemaSubstitutions As Dictionary(Of String, String),
                                    ByRef QueryParts As Dictionary(Of SQLClause, String),
                                    ByRef Replacements As IEnumerable(Of XElement),
                                    ByRef Declarations As IEnumerable(Of XElement),
                                    Optional ByRef DeclarationValues As Dictionary(Of String, String) = Nothing,
                                    Optional ByVal CommonServerName As String = "", Optional ByVal SiteDatabaseName As String = "") As String

        Dim dec, s, f, w, g, h, o, i, t As New StringBuilder
        Dim tabWidthAlias As Integer = 50
        Dim l As Integer = 50
        Dim queryTables As IQueryable(Of XElement)
        Dim tablesX As IEnumerable(Of XElement)
        Dim relations As IEnumerable(Of XElement)
        Dim filters As IEnumerable(Of XElement)
        Dim sorts As IEnumerable(Of XElement)
        Dim sourceAliasMap As New Dictionary(Of String, SQLUnique)
        Dim fieldAliasMap As New Dictionary(Of String, SQLUnique)
        Dim d As String = "."
        Dim q As New StringBuilder
        Dim tmpX As XElement = Nothing
        Dim tmpStr As String
        Dim source As String
        Dim colName As String
        Dim stagingFieldName As String
        Dim exists As Boolean
        Dim sourceAlias As String = ""
        Dim sourceUnique As SQLUnique = Nothing
        Dim quoteTheValue As Boolean = False
        Dim subs As Dictionary(Of String, String) = SchemaSubstitutions
        Dim conj As String = ""
        Dim svNm As String = ""
        Dim col As OraSchemaDS.SchemaColumnsRow
        Dim lastSource As String = ""
        Dim dataType As String
        Dim colNull As String
        Dim isExpression As Boolean
        Dim vals As Dictionary(Of String, String) = DeclarationValues
        Dim lastChecked As String = ""
    Dim includeFilter As Boolean = False
    Dim _logger As ILog = LogManager.GetLogger(GetType(SPPIDDataLayer))

        ' init
        If CommonServerName <> "" Then svNm = "[" & CommonServerName & "]."

        Try

            ' fetch the SQL Schema information
            TablesDV.Sort = ""

            ' initialize the query part strings
            s.Append("SELECT ") : f.Append("FROM ") : t.Append("CREATE TABLE ")
            tmpX = QueryNode.Element("variables")

            ' Query Names ************************************************************************************************
            QueryParts.Add(SQLClause.QueryName, QueryNode.Attribute("name").Value)
            QueryParts.Add(SQLClause.StagingName, QueryNode.Attribute("stagingDestinationName").Value)
            t.Append(QueryNode.Attribute("stagingDestinationName").Value & " (" & nltb)

            ' DELCARE clause ************************************************************************************************

            ' fetch declarations
            Declarations = _
                From var In tmpX...<declaration> _
                Select var

            For Each de As XElement In Declarations

                If de IsNot Declarations.First Then dec.Append(",")
                dec.Append(de.Attribute("name").Value & tb2)
                dec.Append(de.Attribute("datatype").Value)

                If de Is Declarations.Last Then
                    dec.Append(";")
                Else
                    dec.Append(nltb)
                End If

            Next

            If dec.Length > 0 Then dec.Insert(0, "DECLARE " & nltb)
            ' QueryParts.Add(SQLClause.Declare, dec.ToString)

            ' Fetch textReplacements  ***************************************************************************************

            ' fetch text replacements
            Replacements = _
                From var In tmpX...<textReplacement> _
                Select var

            ' INTO clause ***************************************************************************************************

            i.Append(QueryNode.Attribute("stagingDestinationName").Value)
            If i.Length > 0 Then i.Insert(0, "INTO ")
            QueryParts.Add(SQLClause.Into, i.ToString)


            ' FROM clause ***************************************************************************************************

            ' fetch all tables
            tablesX = _
                From tb In QueryNode...<source> _
                Select tb
            queryTables = tablesX.AsQueryable

            For Each e As XElement In tablesX

                ' look for an alias mapping for this table
                exists = sourceAliasMap.TryGetValue(e.Attribute("alias").Value, sourceUnique)

                ' create one and add it to the dictionary if it doesn't exist
                If Not exists Then

                    sourceAlias = e.Attribute("alias").Value

                    If sourceAlias = "" Then
                        sourceAlias = e.Attribute("schema").Value & d & e.Attribute("name").Value
                    End If

                    ' skip deriving the schema; it will be a textReplacement
                    If subs Is Nothing _
                        OrElse e.Attribute("schema").Value.StartsWith("!@~") _
                        OrElse e.Attribute("schema").Value.StartsWith("@") Then
                        sourceAliasMap.Add(sourceAlias, New SQLUnique(e.Attribute("schema").Value, e.Attribute("name").Value))
                    Else

                        ' derive the correct schema for the table from schema substitutions dictionary 
                        'exists = [Enum].TryParse(e.Attribute("schema").Value, True, schemaType)

                        If e.Attribute("alias").Value = "" AndAlso subs IsNot Nothing Then
                            sourceAlias = subs(e.Attribute("schema").Value) & d & e.Attribute("name").Value
                        End If

                        sourceAliasMap.Add(sourceAlias, New SQLUnique(subs(e.Attribute("schema").Value), e.Attribute("name").Value))

                    End If

                End If

                f.Append(nltb)

                If e Is tablesX.First Then
                    f.Append(tb3)
                Else
                    If tablesX.Count > 1 Then f.Append(LCase(e.Attribute("joinType").Value) & " join ")
                End If

                If (SiteDatabaseName <> "") Then
                    source = svNm & SiteDatabaseName & "." & sourceAliasMap(sourceAlias).UniqueName
                Else
                    source = svNm & sourceAliasMap(sourceAlias).UniqueName
                End If

                l = IIf((Len(source) + Len(tb)) > (tabWidthAlias + 1), Len(source) + Len(tb2), tabWidthAlias)
                f.Append(LSet(source, l))
                If e.Attribute("alias").Value <> "" Then f.Append("  " & sourceAlias)

                If e IsNot tablesX.First Then

                    f.Append(nltb2 & " on " & tb)

                    relations = _
                        From r In e...<relation>
                        Select r

                    If relations IsNot Nothing Then

                        For Each r As XElement In relations

                            conj = r.Attribute("conjunction").Value
                            If conj <> "" Then f.Append(nltb3 & conj & " ")
                            f.Append(r.Attribute("leftSource").Value & d & _
                                     r.Attribute("leftField").Value & _
                                     r.Attribute("operator").Value)

                            tmpStr = r.Attribute("rightField").Value

                            If r.Attribute("joinToText") = "True" Then

                                tmpStr = tmpStr.Replace("'", "''")
                                f.Append("'" & tmpStr & "'")

                            Else

                                ' JoinToText should be FALSE if the join value is a number or variable
                                'If r.Attribute("rightSource").Value = "" Then
                                '    f.Append(r.Attribute("rightField").Value & " ")
                                'Else
                                f.Append(r.Attribute("rightSource").Value & "." & r.Attribute("rightField").Value & " ")
                                'End If

                            End If

                        Next

                    End If

                End If

            Next

            QueryParts.Add(SQLClause.From, f.ToString)

            ' SELECT and TableDef clauses ***********************************************************************************

            ' fetch selection modifiers
            Dim sels As IEnumerable(Of XElement) = QueryNode.Descendants("selection")

            ' update the selection string with any selection restrictions
            For Each e As XElement In sels : s.Append(e.Attribute("value").Value & " ") : Next
            s.Append(nltb)

            ' fetch all fields
            Dim fields As IEnumerable(Of XElement) = _
                QueryNode.Element("fields").Descendants("field")

            ' add fields to the selection string and ddl
            For Each e As XElement In fields

                source = e.Attribute("source").Value
                colName = e.Attribute("name").Value
                stagingFieldName = e.Attribute("alias").Value
                isExpression = Not (e.Attribute("expression") Is Nothing OrElse e.Attribute("expression").Value = "")

                If source <> lastSource Then

                    lastSource = source
                    If e IsNot fields.First Then s.Append(nl)

                End If

                ' update the select clause
                If isExpression Then : tmpStr = e.Attribute("expression").Value
                    '  Else : tmpStr = source & d & "" & colName & ""
                    'decode(DB.Data_base, null,' ',DB.Data_base)  Data_base        
                Else : tmpStr = "decode(" & source & d & "" & colName & ", null,''," & source & d & "" & colName & ")" & " " & colName
                End If

                If e IsNot fields.First Then tmpStr = "," & tmpStr

                l = IIf((Len(tmpStr) + Len(tb)) > (tabWidthAlias + 1), Len(tmpStr) + Len(tb2), tabWidthAlias)
                If stagingFieldName = "" Then
                    s.Append(nltb & LSet(tmpStr, l))
                Else
                    If (colName <> "") Then
                        tmpStr = tmpStr.Replace(tmpStr.Substring(tmpStr.Substring(0, tmpStr.IndexOf(" " + colName)).LastIndexOf(")") + 1, colName.Length + 1), " ")
                    End If
                    s.Append(nltb & LSet(tmpStr, l))
                End If

                If stagingFieldName = "" Then
                    stagingFieldName = colName
                Else
                    s.Append(" " & stagingFieldName)
                End If

                If isExpression Then

                    ' expressions may provide a datatype hint; if not, use nvarchar(max)
                    If e.Attribute("datatype").Value = "" Then
                        dataType = "nvarchar(MAX)"
                    Else
                        ' ToDo - it would be wise to verify this is a valid datatype here to catch typos
                        dataType = e.Attribute("datatype").Value
                    End If

                    colNull = " null"

                Else

                    ' look up the data type of the column if this is not an expression
                    exists = sourceAliasMap.TryGetValue(source, sourceUnique)

                    If exists Then

                        ColumnsDV.RowFilter = "TableSchema='" & sourceUnique.SchemaName & "' " & _
                        "and TableName='" & sourceUnique.TableName.ToUpper() & "' " & _
                        "and ColumnName='" & colName & "'"

                        If ColumnsDV.Count <> 1 Then
                            Throw New InvalidExpressionException("The column '" & _
                                sourceUnique.UniqueName & d & colName & "' is not a " & _
                                "valid uniquely identified column; the query cannot be built")
                        End If

                        col = ColumnsDV(0).Row
                        dataType = col.GetDataTypeString

                        ' I would generally prefer to pay attention to the nullability of the source field, but left joins against the source table
                        ' make it possible for these values to be null even if the source table does not allow it. It's far safer to 
                        ' simply set the nullability to 'yes'
                        'colNull = IIf(col.IsNullable = "Yes", " null", " not null")
                        colNull = " null"

                    Else : Throw New InvalidExpressionException("The source value '" & source & _
                        "' is not a valid uniquely identified table or table reference; the query cannot be built")

                    End If

                End If

                If e IsNot fields.First Then t.Append(nltb & ",")
                t.Append(stagingFieldName & " " & dataType & colNull)

            Next

            t.Append(")")
            QueryParts.Add(SQLClause.TableDef, t.ToString)
            QueryParts.Add(SQLClause.Select, s.ToString)

            ' WHERE clause ***************************************************************************************************
            filters = QueryNode.Element("filters").Descendants

            For Each fil As XElement In filters

                tmpStr = fil.Attribute("filterValue").Value
                ' deal with optional filters
                ' check to see if the filter value contains variables
                If tmpStr.Contains("@") Then

                    includeFilter = FilterValuesProvided(QueryParts(SQLClause.QueryName), tmpStr, Declarations, DeclarationValues, lastChecked)

                    ' if no value is provided and the 'Optional' flag is not set to true then the query cannot be built
                    If Not includeFilter AndAlso (fil.Attribute("optional") Is Nothing OrElse fil.Attribute("optional").Value <> "true") Then

                        Throw New InvalidExpressionException("No value was provided for the declaration '" & lastChecked & _
                            "' without this value, a valid SQL data query cannot be built from query '" & QueryParts(SQLClause.QueryName))

                    End If

                End If

                ' add conjunction. Since it's possible for the first filter to be optional with no value provided, if this is the first valid
                ' filter, then the conjunction is ignored
                If fil.Attribute("conjunction").Value <> "" AndAlso w.ToString.TrimToChars.Length > 5 AndAlso includeFilter Then
                    w.Append(tb & fil.Attribute("conjunction").Value & tb)
                End If

                ' add begining parentheses; these get added regardless of whether the filter is included or not
                If fil.Attribute("preParenCount").Value <> "" AndAlso fil.Attribute("preParenCount").Value <> "0" Then
                    w.Append(StrDup(CInt(fil.Attribute("preParenCount").Value), "("))
                End If

                If includeFilter Then

                    ' decide whether to quote the value or not by checking to see if it is a variable or
                    ' if the field's data type makes it necessary. Variables are never quoted; however string substitutions may still be quoted
                    If tmpStr.Contains("@") Then
                        quoteTheValue = False
                    Else

                        ' look up the data type of the column
                        exists = sourceAliasMap.TryGetValue(fil.Attribute("source").Value, sourceUnique)

                        If exists Then

                            ColumnsDV.RowFilter = "TableSchema='" & sourceUnique.SchemaName & "' " & _
                            "and TableName='" & sourceUnique.TableName & "' " & _
                            "and ColumnName='" & fil.Attribute("fieldName").Value & "'"

                            If ColumnsDV.Count <> 1 Then
                                Throw New InvalidExpressionException("The column '" & _
                                    sourceUnique.UniqueName & d & fil.Attribute("fieldName").Value & "' is not a " & _
                                    "valid uniquely identified column; the query cannot be built")
                            End If

                            col = ColumnsDV(0).Row
                            quoteTheValue = SQLUnique.IsQuotable(col.DataType)

                        Else : Throw New InvalidExpressionException("The source value '" & fil.Attribute("source").Value & _
                            "' is not a valid uniquely identified table or table reference; the query cannot be built")

                        End If

                    End If

                    ' add filter clause
                    tmpStr = tmpStr.Replace("'", "''")
                    w.Append(fil.Attribute("source").Value & d & fil.Attribute("fieldName").Value)
                    w.Append(" " & fil.Attribute("operator").Value & IIf(quoteTheValue, " '", " "))
                    w.Append(tmpStr & IIf(quoteTheValue, "'", ""))

                End If

                ' add ending parentheses
                If fil.Attribute("postParenCount").Value <> "" AndAlso fil.Attribute("postParenCount").Value <> "0" Then
                    w.Append(StrDup(CInt(fil.Attribute("postParenCount").Value), ")"))
                End If

                w = w.Replace("()", "")
                w.Append(nltb)

            Next

            w = w.Replace("()", "")
            If w.ToString.TrimToChars.Length > 3 Then w.Insert(0, "WHERE " & nltb)
            QueryParts.Add(SQLClause.Where, w.ToString)

            ' HAVING and GROUP BY clause ****************************************************************************************
            ' NOTE: this section is not yet implemented - its unclear when or if these clauses will ever be used to
            ' extract data from SPPID

            ' ORDER BY clause ***************************************************************************************************
            sorts = QueryNode.Element("sorts").Descendants

            For Each sort As XElement In sorts

                If sort IsNot sorts.First Then o.Append(tb & ",")
                o.Append(IIf(sort.Attribute("source").Value = "", "", sort.Attribute("source").Value))
                o.Append(sort.Attribute("fieldName").Value)
                o.Append(IIf(sort.Attribute("sortDirection").Value = "", "", sort.Attribute("sortDirection").Value))

            Next

            If o.Length > 0 Then o.Insert(0, "ORDER BY " & nltb)
            QueryParts.Add(SQLClause.OrderBy, o.ToString)
      Return "Pass"
    Catch ex As Exception
      _logger.Debug(ex.Message)
      Return "Fail: " & ex.Message
        End Try



    End Function


    ''' <summary>
    ''' Searches the values provided and detects any default values to determine if a valid query can be made using the input filter string
    ''' </summary>
    ''' <param name="QueryName"></param>
    ''' <param name="FilterVal"></param>
    ''' <param name="Declarations"></param>
    ''' <param name="vars"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function FilterValuesProvided(ByVal QueryName As String, _
                                         ByVal FilterVal As String, _
                                         ByVal Declarations As IEnumerable(Of XElement), _
                                         ByVal vars As Dictionary(Of String, String), _
                                         ByRef lastChecked As String) As Boolean

        Dim pat As String
        Dim matches As MatchCollection
        Dim found, provided As Boolean
        Dim tmp As String = ""

        provided = False
        pat = "(@[a-zA-Z0-9_\-\#\$]+)\w"
        matches = Regex.Matches(FilterVal, pat, RegexOptions.IgnoreCase)

        ' for each variable found
        For Each m As System.Text.RegularExpressions.Match In matches

            lastChecked = m.Value

            ' look to see if a value is provided
            If vars Is Nothing Then : found = False
            Else

                ' check first to see if a query-specific value is provided
                found = vars.TryGetValue("!" & QueryName & "." & RTrim(m.Value), tmp)

                If found Then

                    ' an explicit !NoValue overrides any generic value that may be present, allowing
                    ' for query-specific defaults to be used, or the removal of specific filters on
                    ' a query by query basis
                    If tmp = "!NoValue" Then found = False

                    ' if not found, look for a generic value that is used for all queries
                Else : found = vars.TryGetValue("!!All." & RTrim(m.Value), tmp)

                End If

                ' the above order of processing allows us to provide a single (generic) value for all queries to be processed that
                ' can still be overridden by a query-specific value. The check for !NoValue allows us to override a generic value by
                ' not providing any value at all for this specific query; this is useful for removing optional filters on specific queries
                ' and also allows us to make use of a default value in specific instances and avoids forcing an eclipse of all query-specific
                ' default values for a given variable.

            End If

            provided = found

            ' NOTE; if it were common to have queries with > 4 variable filters, then it might be worth
            ' our while to implement an iQueryProvider or dump the Declarations into a dictionary. Since
            ' this is rare, we'll stick with the iEnumerable until it actually creates a verifiable 
            ' performance problem that can be overcome with the additional overhead of the dictionary

            ' if no value is provided for one of the variables, look to see if there is a default
            If Not found Then

                For Each x As XElement In Declarations

                    ' look for a default; if found, then a value is considered to be provided. Note that this
                    ' behavior constitutes an override of the OPTIONAL flag for the declaration - the default value
                    ' will always be used if provided when an explicit value has not been provided, regardless of whether
                    ' OPTIONAL is set or not
                    If x.Attribute("name").Value = m.Value AndAlso x.Attribute("value").Value <> "" Then
                        provided = True
                        Exit For
                    End If

                Next

                ' if the value is still not provided then exit for
                If Not provided Then Exit For

            End If

        Next

        ' if even one variable is not provided, then the provided flag will be false.
        ' if a value for all variables is provided, then the provided flag will be true
        ' return the provided flag
        Return provided

    End Function

#End Region


End Module
