Option Explicit On
Option Compare Text
Option Strict Off

Imports System.Collections.Generic
Imports System.Text

Public Enum SPQueryName

    SiteData = 1
    Equipment = 2
    Instruments = 3
    PipeRuns = 4
    Pipesections = 5
    ControlValves = 6
    Panels = 7

End Enum

Public Class SmartPlantDBQueries

    ' private declarations
    Private s, f, w, h, o As String
    Private queries As Dictionary(Of SPQueryName, Dictionary(Of SQLClause, String))
    Private IncludeStockpile As Boolean = True
    Dim sections As Dictionary(Of SQLClause, String)

#Region " Raw Query Instantiation "

    Public Sub New()

        ' init
        queries = New Dictionary(Of SPQueryName, Dictionary(Of SQLClause, String))

        GetQuerySiteData() : AddQuerySections(SPQueryName.SiteData, s, f, w, h, o)

        ' For all data queries, the placeholder for table schema is in the form [!schXX] where X represents the schema type required
        ' DT = SPAPLANT  (DataBaseTableName)     DD = DATA_DICTIONARY   PT = SPPID (PidTableName)   PD = SPPID_DICTIONARY (PidDataDictionaryTableName)

        GetQueryEquipment() : AddQuerySections(SPQueryName.Equipment, s, f, w, h, o)
        GetQueryInstruments() : AddQuerySections(SPQueryName.Instruments, s, f, w, h, o)
        GetQueryPanels() : AddQuerySections(SPQueryName.Panels, s, f, w, h, o)

    End Sub

#End Region

#Region " Private Functions "

    ''' <summary>
    ''' Add query clauses independently under the appropriate query name to the queries collection
    ''' </summary>
    ''' <param name="Name"></param>
    ''' <param name="Select"></param>
    ''' <param name="From"></param>
    ''' <param name="Where"></param>
    ''' <param name="Having"></param>
    ''' <param name="OrderBy"></param>
    ''' <remarks></remarks>
    Private Sub AddQuerySections(Name As SPQueryName, [Select] As String, [From] As String, [Where] As String, [Having] As String, OrderBy As String)

        sections = New Dictionary(Of SQLClause, String)
        sections.Add(SQLClause.Select, [Select])
        sections.Add(SQLClause.From, [From])
        sections.Add(SQLClause.Where, [Where])
        sections.Add(SQLClause.Having, [Having])
        sections.Add(SQLClause.OrderBy, OrderBy)
        queries.Add(Name, sections)

    End Sub

    ''' <summary>
    ''' GetQueryTemplate()
    ''' Fetch site data from the Site database, including projects and SP schema mapping
    ''' </summary>
    ''' <remarks>Source Function: SmartPlantInstrumentDataQuery </remarks>
    Private Sub GetQueryTemplate()

        s = "" : f = "" : w = "" : h = "" : o = ""


    End Sub

    ''' <summary>
    ''' GetQuerySiteData()
    ''' Fetch site data from the Site database, including projects and SP schema mapping
    ''' </summary>
    ''' <remarks>There is no default WHERE clause
    ''' Source Function: SmartPlantSiteDbDataQuery </remarks>
    Private Sub GetQuerySiteData()

        s = "" : f = "" : w = "" : h = "" : o = ""
        s = _
            "select distinct                                                                    " & _
            "   DB.Data_base    ,DB.DBServer        ,RI.Path                ,DB.SP_Schema_Type  " & _
            "   ,DB.Username    ,RI.Description     ,RI.PlantGroupTypeName  ,RI.DateCreated     " & _
            "   ,DB.SP_ID       ,DB.SP_RootItemID                                               "
        f = _
            "from [!sch].T_DB_Data                      DB                                      " & _
            "   inner join [!sch].T_RootItem            RI                                      " & _
            "       on DB.SP_RootItemID = RI.SP_ID                                              "
        w = _
            "where DB.Data_Base = '[!dbname]'                                                   "

        o = "order by DB.Data_base, DB.Username                                                 "


    End Sub

    ''' <summary>
    ''' GetQueryPanels()
    ''' Fetch site data from the Site database, including projects and SP schema mapping
    ''' </summary>
    ''' <remarks>There is no default WHERE clause
    ''' Source Function: SmartPlantPanelDataQuery </remarks>
    Private Sub GetQueryPanels()

        s = "" : f = "" : w = "" : h = "" : o = ""

        s = _
            "SELECT distinct                                                                            " & _
            "   PlI.ItemTag                     ,PlI.SupplyBy                                           " & _
            "   ,PlI.SP_ID                      ,PlI.SP_PlantGroupID                                    " & _
            "   ,PlI.SP_PartOfId                                                                        " & _
            "   ,PlI.Name                                                   T_PlantItemName             " & _
            "                                                                                           " & _
            "   ,PG.Name                                                    PidUnitName                 " & _
            "   ,PG.Description                                             PidUnitDescription          " & _
            "                                                                                           " & _
            "   ,Dr.DrawingNumber                                                                       " & _
            "   ,Dr.Name                                                    DrawingName                 " & _
            "   ,REPLACE(REPLACE(REPLACE(Dr.Title,CHAR(9),' '),CHAR(10),' '),CHAR(13),' ')              " & _
            "                                                               Title                       " & _
            "   ,Dr.Description                                             DrawingDescription          " & _
            "                                                                                           " & _
            "   ,Inst.TagSequenceNo             ,Inst.TagSuffix                                         " & _
            "   ,Inst.LoopTagSuffix             ,Inst.InstrumentClass                                   " & _
            "   ,Inst.PipingMaterialsClass      ,Inst.InstrumentType                                    " & _
            "   ,Inst.SP_PipeRunID              ,Inst.SP_SignalRundID                                   " & _
            "   ,Inst.MeasuredVariableCode                                  UnitProcessNo               " & _
            "   ,Inst.InstrumentTypeModifier                                TagPrefix                   " & _
            "                                                                                           " & _
            "   ,Cl.codeList_number             ,Cl.codelist_text                                       " & _
            "   ,Cl.codelist_index              ,Cl.codelist_constraint                                 " & _
            "                                                                                           " & _
            "   ,IC.NominalDiameter             ,IC.ActuatorType                                        " & _
            "   ,En.Name                                                    EnumerationsName            " & _
            "   ,IFM.FailureAction                                                                      " & _
            "   ,Rep.InStockPile                                                                        " & _
            "   ,U.UnitCode                                                 PidUnitCode                 "
        f =
            "FROM [!schDT].T_PlantGroup                                     PG                          " & _
            "   inner join [!schPT].T_Drawing                               Dr                          " & _
            "       on PG.SP_ID = Dr.SP_PlantGroupID                                                    " & _
            "   inner join [!schPT].T_Representation                        Rep                         " & _
            "       on Rep.SP_DrawingID = Dr.SP_ID                                                      " & _
            "   @!~StockpileJoin join [!schDT].T_Unit                       U                           " & _
            "      -- on PG.SP_ID = U.SP_ID                                                               " & _
            "   inner join [!schPT].T_PlantItem                             PlI                         " & _
            "        --  PlI.SP_PlantGroupID = U.SP_ID                                               " & _
            "           on  PlI.SP_ID = Rep.SP_ModelItemID                                              " & _
            "           and PlI.SP_PlantGroupID = PG.SP_ID                                              " & _
            "   inner join [!schPT].T_Instrument                            Inst                        " & _
            "       on Inst.SP_ID = PlI.SP_ID                                                           " & _
            "   inner join [!schPT].T_InlineComp                            IC                          " & _
            "       on IC.SP_InstrumentID = Inst.SP_ID                                                  " & _
            "   inner join [!schPT].T_InstrFailMode                         IFM                         " & _
            "       on IFM.SP_InstrumentID = Inst.SP_ID                                                 " & _
            "   inner join [!schPD].Codelists                               Cl                          " & _
            "       on Cl.codelist_index = Inst.InstrumentType                                          " & _
            "   inner join [!schPD].Enumerations                            En                          " & _
            "       on  En.Id = Cl.codelist_number                                                      "


    End Sub

    ''' <summary>
    ''' GetQueryInstruments()
    ''' Fetch site data from the Site database, including projects and SP schema mapping
    ''' </summary>
    ''' <remarks>Source Function: SmartPlantInstrumentDataQuery </remarks>
    Private Sub GetQueryInstruments()

        s = "" : f = "" : w = "" : h = "" : o = ""

        s = _
            "SELECT distinct                                                                            " & _
            "   PlI.ItemTag                     ,PlI.SupplyBy                                           " & _
            "   ,PlI.SP_ID                      ,PlI.SP_PlantGroupID                                    " & _
            "   ,PlI.SP_PartOfId                                                                        " & _
            "   ,PlI.Name                                                   PlantItemName               " & _
            "                                                                                           " & _
            "                                                                                           " & _
            "   ,PG.Name                                                    PidUnitName                 " & _
            "   ,PG.Description                                             PidUnitDescription          " & _
            "                                                                                           " & _
            "   ,Dr.DrawingNumber                                                                       " & _
            "   ,Dr.Name                                                    DrawingName                 " & _
            "   ,REPLACE(REPLACE(REPLACE(Dr.Title,CHAR(9),' '),CHAR(10),' '),CHAR(13),' ')              " & _
            "                                                               DrawingTitle                " & _
            "   ,Dr.Description                                             DrawingDescription          " & _
            "                                                                                           " & _
            "   ,Inst.TagSequenceNo             ,Inst.TagSuffix                                         " & _
            "   ,Inst.LoopTagSuffix             ,Inst.InstrumentClass                                   " & _
            "   ,Inst.PipingMaterialsClass      ,Inst.InstrumentType                                    " & _
            "   ,Inst.SP_PipeRunID              ,Inst.SP_SignalRundID                                   " & _
            "   ,Inst.IsInline                                                                          " & _
            "   ,Inst.MeasuredVariableCode                                  UnitProcessNo               " & _
            "   ,Inst.InstrumentTypeModifier                                TagPrefix                   " & _
            "                                                                                           " & _
            "   ,En.Name                                                    EnumerationsName            " & _
            "   ,En.Description                                             EnumerationsDescription     " & _
            "                                                                                           " & _
            "   ,Cl.codeList_number             ,Cl.codelist_text                                       " & _
            "   ,Cl.codelist_constraint                                                                 " & _
            "                                                                                           " & _
            "   ,IC.NominalDiameter                                                                     " & _
            "   ,PR.OperFluidCode                                                                       " & _
            "   ,Rep.InStockPile                                                                        " & _
            "   ,U.UnitCode                                                 PidUnitCode                 "

        f =
            "FROM [!schDT].T_PlantGroup                                     PG                          " & _
            "   inner join [!schPT].T_Drawing                               Dr                          " & _
            "       on Dr.SP_PlantGroupID = PG.SP_ID                                                    " & _
            "   inner join [!schPT].T_Representation                        Rep                         " & _
            "       on Rep.SP_DrawingID = Dr.SP_ID                                                      " & _
            "   @!~StockpileJoin join [!schDT].T_Unit                       U                           " & _
            "       on U.SP_ID = PG.SP_ID                                                               " & _
            "   inner join [!schPT].T_PlantItem                             PlI                         " & _
            "       --      PlI.SP_PlantGroupID = U.SP_ID                                               " & _
            "          on PlI.SP_ID = Rep.SP_ModelItemID                                              " & _
            "           and PlI.SP_PlantGroupID = PG.SP_ID                                              " & _
            "   inner join [!schPT].T_Instrument                            Inst                        " & _
            "       on Inst.SP_ID = PlI.SP_ID                                                           " & _
            "   inner join [!schPD].Codelists                               Cl                          " & _
            "       on Cl.codelist_index = Eq.Class                                                     " & _
            "   inner join [!schPD].Enumerations                            En                          " & _
            "       on  En.Id = Cl.codelist_number                                                      " & _
            "   left join [!schPT].T_InlineComp                             IC                          " & _
            "       on IC.SP_InstrumentID = Inst.SP_ID                                                  " & _
            "   inner join [!schPT].T_PipeRun                               PR                          " & _
            "       on PR.SP_ID = IC.SP_PipeRunID                                                       "
        w =
            "WHERE  En.name = @EquipmentClass                                                           " & _
            "   and PlI.SP_PlantGroupID = @PlantGroupID                                                 "

        ' Other criteria can be appended, such as:
        ' U.UnitCode in (unit code list)
        ' Dr.DrawingNumber in (drawing number list) 
        ' Dr.SP_ID in (drawing ID list)Project/Unit/Drawing
        ' Originally, this was ORDER BY U.UnitCode, Dr.DrawingNumber, PlI.ItemTag


    End Sub

    ''' <summary>
    ''' GetQueryEquipment()
    ''' Fetch site data from the Site database, including projects and SP schema mapping
    ''' </summary>
    ''' <remarks>Source Function: SmartPlantInstrumentDataQuery </remarks>
    Private Sub GetQueryEquipment()

        s = "" : f = "" : w = "" : h = "" : o = ""

        s = _
            "SELECT distinct                                                                            " & _
            "   PlI.ItemTag                     ,PlI.SupplyBy                                           " & _
            "   ,PlI.SP_ID                      ,PlI.SP_PlantGroupID                                    " & _
            "   ,PlI.SP_PartOfId                                                                        " & _
            "   ,PlI.Name                                                   PlantItemName               " & _
            "                                                                                           " & _
            "                                                                                           " & _
            "   ,PG.Name                                                    PidUnitName                 " & _
            "   ,PG.Description                                             PidUnitDescription          " & _
            "                                                                                           " & _
            "   ,Dr.DrawingNumber                                                                       " & _
            "   ,Dr.Name                                                    DrawingName                 " & _
            "   ,REPLACE(REPLACE(REPLACE(Dr.Title,CHAR(9),' '),CHAR(10),' '),CHAR(13),' ')              " & _
            "                                                               DrawingTitle                " & _
            "   ,Dr.Description                                             DrawingDescription          " & _
            "                                                                                           " & _
            "   ,Eq.TagPrefix                   ,Eq.TagSequenceNo                                       " & _
            "   ,Eq.TagSuffix                   ,Eq.Class                                               " & _
            "   ,Eq.EquipmentSubclass           ,Eq.EquipmentType                                       " & _
            "                                                                                           " & _
            "   ,En.Name                                                    EnumerationsName            " & _
            "   ,En.Description                                             EnumerationsDescription     " & _
            "                                                                                           " & _
            "   ,Cl.codeList_number             ,Cl.codelist_text                                       " & _
            "   ,Cl.codelist_constraint                                                                 " & _
            "                                                                                           " & _
            "   ,Rep.InStockPile                                                                        " & _
            "   ,U.UnitCode                                                 PidUnitCode                 "

        f =
            "FROM [!schDT].T_PlantGroup                                     PG                          " & _
            "   inner join [!schPT].T_Drawing                               Dr                          " & _
            "       on Dr.SP_PlantGroupID = PG.SP_ID                                                    " & _
            "   inner join [!schPT].T_Representation                        Rep                         " & _
            "       on Rep.SP_DrawingID = Dr.SP_ID                                                      " & _
            "   @!~StockpileJoin join [!schDT].T_Unit                       U                           " & _
            "       on U.SP_ID = PG.SP_ID                                                               " & _
            "   inner join [!schPT].T_PlantItem                             PlI                         " & _
            "       on      PlI.SP_PlantGroupID = U.SP_ID                                               " & _
            "           and PlI.SP_ID = Rep.SP_ModelItemID                                              " & _
            "           and PlI.SP_PlantGroupID = PG.SP_ID                                              " & _
            "   inner join [!schPT].T_Equipment                             Eq                          " & _
            "       on Eq.SP_ID = PlI.SP_ID                                                             " & _
            "   inner join [!schPD].Codelists                               Cl                          " & _
            "       on Cl.codelist_index = Eq.Class                                                     " & _
            "   inner join [!schPD].Enumerations                            En                          " & _
            "       on  En.Id = Cl.codelist_number                                                      "
        w =
            "WHERE  En.name = @EquipmentClass                                                           " & _
            "   and PlI.SP_PlantGroupID = @PlantGroupID                                                 "

        ' Other criteria can be appended, such as:
        ' U.UnitCode in (unit code list)
        ' Dr.DrawingNumber in (drawing number list) 
        ' Dr.SP_ID in (drawing ID list)Project/Unit/Drawing
        ' Originally, this was ORDER BY U.UnitCode, Dr.DrawingNumber, PlI.ItemTag

    End Sub

#End Region

#Region " Public Functions "

    ''' <summary>
    ''' Concatenates all of the sections of a query into a single string
    ''' </summary>
    ''' <param name="name"></param>
    ''' <param name="s">Select</param>
    ''' <param name="f">From</param>
    ''' <param name="w">Where</param>
    ''' <param name="h">Having</param>
    ''' <param name="o">Order By</param>
    ''' <returns></returns>
    ''' <remarks>Clauses of the query can be replaced by strings provided in the optional arguments
    ''' In general, clauses should start with the appropriate key word; however, if they do not, 
    ''' the key word will automatically be added</remarks>
    Public Function ConcatQuery(name As SPQueryName, Optional s As String = "", Optional f As String = "", _
                                Optional w As String = "", Optional h As String = "", Optional o As String = "") _
                                As String

        Dim q As Dictionary(Of SQLClause, String) = queries(name)
        Dim sb As New StringBuilder

        If s = "" Then
            sb.Append(q(SQLClause.Select))
        Else

            If Left(s, 6) <> "select" Then sb.Append("SELECT ")
            sb.Append(s)

        End If

        sb.Append(nl)

        If f = "" Then
            sb.Append(q(SQLClause.From))
        Else

            If Left(f, 4) <> "select" Then sb.Append("FROM ")
            sb.Append(f)

        End If

        If w <> "" OrElse q(SQLClause.Where) <> "" Then sb.Append(nl)

        If w = "" Then

            If Left(w, 5) <> "where" Then sb.Append("WHERE ")
            sb.Append(q(SQLClause.Where))

        Else
            sb.Append(w)
        End If

        If h <> "" OrElse q(SQLClause.Having) <> "" Then sb.Append(nl)

        If h = "" Then

            If Left(h, 6) <> "having" Then sb.Append("HAVING ")
            sb.Append(q(SQLClause.Having))
        Else
            sb.Append(h)
        End If

        If o <> "" OrElse q(SQLClause.OrderBy) <> "" Then sb.Append(nl)

        If o = "" Then

            If Left(s, 8) <> "order by" Then sb.Append("ORDER BY ")
            sb.Append(q(SQLClause.OrderBy))
        Else
            sb.Append(o)
        End If

        Return sb.ToString

    End Function


    Public Function GetQuerySections(Name As SPQueryName) As Dictionary(Of SQLClause, String)
        Return queries(Name)
    End Function

    ' ''' <summary>
    ' ''' Fetches a query by name, updated with the correct schema and filter
    ' ''' </summary>
    ' ''' <param name="Name"></param>
    ' ''' <param name="QueryText"></param>
    ' ''' <param name="SiteSchema"></param>
    ' ''' <param name="DBSchema"></param>
    ' ''' <param name="FilterText"></param>
    ' ''' <returns></returns>
    ' ''' <remarks>The raw query is returned if the schema and/or FilterText are not provided</remarks>
    'Public Function GetQuery(Name As SPQueryName, ByRef QueryText As String, Optional SiteSchema As String = "", _
    '                         Optional DBSchema As String = "", Optional FilterText As String = "") _
    '                                                                                        As String

    '    Dim q As String
    '    Dim found As Boolean
    '    Dim rVal As String = ""

    '    Try

    '        found = queries.TryGetValue(Name, q)

    '        If Not found Then

    '            QueryText = ""
    '            Return "Fail: A query named '" & Name.ToString & "' cannot be found"

    '        End If

    '        ' replace the site schema placeholder
    '        If DBSchema <> "" AndAlso InStr(q, "[!sch]") < 1 Then
    '            rVal = "Warn: Schema placeholder not found; schema will not be applied to the query"
    '        Else

    '            If DBSchema <> "" Then
    '                q = q.Replace("[!sch]", "[" & DBSchema & "]")
    '            End If

    '        End If

    '        ' replace the schema placeholder
    '        If DBSchema <> "" AndAlso InStr(q, "[!sch]") < 1 Then
    '            rVal = "Warn: Schema placeholder not found; schema will not be applied to the query"
    '        Else

    '            If DBSchema <> "" Then
    '                q = q.Replace("[!sch]", "[" & DBSchema & "]")
    '            End If

    '        End If

    '        ' replace the filter placeholder
    '        If DBSchema <> "" AndAlso InStr(q, "[!filter]") < 1 Then
    '            rVal = "; Warn: Filter placeholder not found; schema will not be applied to the query"
    '        Else

    '            If DBSchema <> "" Then
    '                q = q.Replace("[!filter]", "[" & DBSchema & "]")
    '            End If

    '        End If

    '        QueryText = q
    '        If rVal.StartsWith(";") Then rVal.Remove(1, 2)
    '        rVal = IIf(rVal = "", "Pass", rVal)

    '    Catch ex As Exception
    '        rVal = "Fail: could not fetch query text due to error: " & ex.Message & IIf(rVal = "", "", "; " & rVal)
    '    End Try

    '    Return rVal

    'End Function

#End Region


End Class
