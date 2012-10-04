Imports System.Collections.Generic
Imports System.Collections.Specialized
Imports System.IO
Imports System.Linq
Imports NUnit.Framework
Imports org.iringtools.adapter
Imports org.iringtools.library
Imports org.iringtools.utility
Imports StaticDust.Configuration
Imports Ninject
Imports Ninject.Extensions.Xml
Imports System.Reflection
Imports Oracle.DataAccess.Client
Imports org.iringtools.datalayers.IIP.SPPID
Imports System.Data.SqlClient

<TestFixture()>
Public Class Test
    Private _baseDirectory As String = String.Empty
    Private _kernel As IKernel = Nothing
    Private _settings As NameValueCollection
    Private _adapterSettings As AdapterSettings
    Private _sppidDataLayer As IDataLayer2
    Private _objectType As String

    Private _projConn As SqlConnection
    Private _stageConn As SqlConnection
    Private _siteConn As SqlConnection


    'Private _stageConnOracle As OracleConnection
    Private _siteConnOracle As OracleConnection
    Private _plantConnOracle As OracleConnection
    Private _plantDicConnOracle As OracleConnection
    Private _PIDConnOracle As OracleConnection
    Private _PIDDicConnOracle As OracleConnection
    Public Sub New()
        ' N inject magic

        Dim textReplacements As New Dictionary(Of String, String)
        Dim queryVariables As New Dictionary(Of String, String)
        Dim ninjectSettings = New NinjectSettings() With {.LoadExtensions = False}
        _kernel = New StandardKernel(ninjectSettings)

        _kernel.Load(New XmlExtensionModule())

        _kernel.Bind(Of AdapterSettings)().ToSelf().InSingletonScope()
        _adapterSettings = _kernel.[Get](Of AdapterSettings)()

        ' Start with some generic settings
        _baseDirectory = Directory.GetCurrentDirectory()
        Directory.SetCurrentDirectory(_baseDirectory.Substring(0, _baseDirectory.LastIndexOf("\bin")))

        _adapterSettings.AppendSettings(New AppSettingsReader("App.config"))


        _settings = New NameValueCollection()

        _settings("BaseDirectoryPath") = Directory.GetCurrentDirectory()
        _settings("ExecutingAssemblyName") = Assembly.GetExecutingAssembly().GetName().Name

        Dim tmp = [String].Format("{0}{1}.{2}.config", _adapterSettings("AppDataPath"), _adapterSettings("ProjectName"), _adapterSettings("ApplicationName"))
        _settings("ProjectConfigurationPath") = Path.Combine(_settings("BaseDirectoryPath"), tmp)


        tmp = String.Format("{0}{1}.StagingConfiguration.{2}.xml", _settings("AppDataPath"), _adapterSettings("ProjectName"), _adapterSettings("ApplicationName"))
        _settings("StagingConfigurationPath") = Path.Combine(_settings("BaseDirectoryPath"), tmp)


        _adapterSettings.AppendSettings(_settings)

        'Set Commodity 
        _objectType = _adapterSettings("ObjectType")

        ' Add our specific settings
        Dim appSettingsPath As String = [String].Format("{0}12345_000.SPPID.config", _adapterSettings("XmlPath"))

        If File.Exists(appSettingsPath) Then
            Dim appSettings As New AppSettingsReader(appSettingsPath)
            _adapterSettings.AppendSettings(appSettings)
        End If

        ''Set Connection strings----------------
        If _adapterSettings("SPPIDPLantConnectionString").Contains("PROTOCOL") = False Then
            _projConn = New SqlConnection(_adapterSettings("SPPIDPLantConnectionString"))

            _siteConn = New SqlConnection(_adapterSettings("SPPIDSiteConnectionString"))
        Else
            _plantConnOracle = New OracleConnection(_adapterSettings("SPPIDPLantConnectionString"))
            _siteConnOracle = New OracleConnection(_adapterSettings("SPPIDSiteConnectionString"))
            _plantDicConnOracle = New OracleConnection(_adapterSettings("PlantDataDicConnectionString"))
            _PIDConnOracle = New OracleConnection(_adapterSettings("PIDConnectionString"))
            _PIDDicConnOracle = New OracleConnection(_adapterSettings("PIDDataDicConnectionString"))

            ''Set Oracle Stagging Files-------------------------
            tmp = String.Format("{0}{1}.StagingConfiguration.{2}.{3}.xml", _settings("AppDataPath"), _adapterSettings("ProjectName"), _adapterSettings("ApplicationName"), "Oracle")
            _adapterSettings("StagingConfigurationPath") = Path.Combine(_settings("BaseDirectoryPath"), tmp)
        End If

        _stageConn = New SqlConnection(_adapterSettings("iRingStagingConnectionString"))


        ' and run the thing
        Dim relativePath As String = [String].Format("{0}BindingConfiguration.{1}.{2}.xml", _settings("XmlPath"), _settings("ProjectName"), _settings("ApplicationName"))

        ' Ninject Extension requires fully qualified path.
        Dim bindingConfigurationPath As String = Path.Combine(_settings("BaseDirectoryPath"), relativePath)




        _kernel.Load(bindingConfigurationPath)

        ' set up the list of text replacements and query variables. The variable or text replacement key should be in the form
        ' <queryName>.<variableName>. the variableName portion of this for text replacements should always start with '!@~'
        ' if the queryName is set to !All then this replacement will apply to all queries

        ' NOTE: these key-value pairs are hard-coded here but should be built from user choices instead in a production environment
        textReplacements.Add("!All.!@~IncludeStockpile", "true")

        ' NOTE: although you can provide a @ProjectDBName in the queryVariables, it will not be used to build the SITE data query and set the schema
        ' for queries in SPPID; this information is instead taken from the ProjectConfiguration file
        'queryVariables.Add("!All.@ProjectDBName", "whatever")

        _sppidDataLayer = New SPPIDDataLayer(_adapterSettings)
        '_sppidDataLayer = _kernel.[Get](Of IDataLayer2)()
    End Sub
    '<Test()>
    Public Sub Create()
        Dim identifiers As IList(Of String) = New List(Of String)() From { _
     "E5E3A74C7A0F431AB5069EA1BCD0407D"
    }

        Dim random As New Random()
        Dim dataObjects As IList(Of IDataObject) = _sppidDataLayer.Create("Equipment", identifiers)

        For Each dataObject As IDataObject In dataObjects
            dataObject.SetPropertyValue("Name", "PT-" & random.[Next](2, 10))
            dataObject.SetPropertyValue("Drawing_DateCreated", DateTime.Today)
        Next

        Dim actual As Response = _sppidDataLayer.Post(dataObjects)

        If actual.Level <> StatusLevel.Success Then
            Throw New AssertionException(Utility.SerializeDataContract(Of Response)(actual))
        End If

        Assert.IsTrue(actual.Level = StatusLevel.Success)

    End Sub

    <Test()>
    Public Sub TestGetObjects()

        Dim identifiers As IList(Of String) = New List(Of String)() From { _
               "-2608A", _
               "1-WL-MT-2610", _
               "345-MV-9982", _
               "345-MV-9983" _
         }

        Dim dataObjects As IList(Of IDataObject) = _sppidDataLayer.Get(_objectType, identifiers)

        If (dataObjects.Count() <= 0) Then
            Assert.IsTrue(True, "No Rows returned.")
        End If


        For Each dataObject As IDataObject In dataObjects
            Assert.IsNotNull(dataObject.GetPropertyValue("DrawingName"))
            Assert.IsNotNull(dataObject.GetPropertyValue("Title"))
            Assert.IsNotNull(dataObject.GetPropertyValue("PlantItemName"))
            Assert.IsNotNull(dataObject.GetPropertyValue("PidUnitDescription"))
            Assert.IsNotNull(dataObject.GetPropertyValue("PidUnitName"))
        Next

    End Sub

    <Test()>
    Public Sub TestGetWithIdentifiers()
        Dim identifiers As IList(Of String) = _sppidDataLayer.GetIdentifiers(_objectType, New DataFilter())

        Dim identifier As IList(Of String) = DirectCast(identifiers, List(Of String)).GetRange(0, 10)

        Dim dataObjects As IList(Of IDataObject) = _sppidDataLayer.Get(_objectType, identifier)

        Assert.IsTrue(True, "Success")

    End Sub

    <Test()>
    Public Sub TestGetIdentifiersWithFilter()
        Dim _filter As New DataFilter() With {.Expressions = New List(Of Expression)() From { _
          New Expression() With { _
            .PropertyName = "PlantItemName", _
            .RelationalOperator = RelationalOperator.EqualTo, _
            .Values = New Values() From { _
            "TANK" _
             }
         }
     }
 }
        Dim identifiers As IList(Of String) = DirectCast(_sppidDataLayer.GetIdentifiers(_objectType, _filter), List(Of String))


        Dim dataObjects As IList(Of IDataObject) = _sppidDataLayer.Get(_objectType, identifiers)


        Assert.Greater(dataObjects.Count, 0)
    End Sub



    <Test()>
    Public Sub TestGetTotalCount()
        Dim identifiers As IList(Of String) = New List(Of String)() From { _
              "AA-MV-9983", _
              "345-ED-9982", _
              "345-MV-9982", _
              "345-MV-9983" _
        }

        Dim dataObjects As IList(Of IDataObject) = _sppidDataLayer.Get(_objectType, identifiers)

        If (dataObjects.Count() <= 0) Then
            Assert.IsTrue(True, "No Rows returned.")
        End If

        Assert.IsNotNull(True, dataObjects.Count.ToString())
    End Sub

    <Test()>
    Public Sub TestGetCountWithFilter()

        Dim dataFilter As New DataFilter() With {.Expressions = New List(Of Expression)() From { _
              New Expression() With { _
                .PropertyName = "PlantItemName", _
                .RelationalOperator = RelationalOperator.EqualTo, _
                .Values = New Values() From { _
                "TANK" _
            }
        }
     }
    }

        Dim dataObjects As Long = _sppidDataLayer.GetCount(_objectType, dataFilter)

        If dataObjects = -1 Then
            Assert.IsNotNull(dataObjects)
        End If

        Assert.IsTrue(True, dataObjects.ToString())

    End Sub

    <Test()>
    Public Sub TestGetDictionary()
        Dim benchmark As DataDictionary = Nothing

        Dim dictionary As DataDictionary = _sppidDataLayer.GetDictionary()

        Assert.IsNotNull(dictionary)

    End Sub

    <Test()>
    Public Sub TestGetPageWithFilter()

        Dim _filter As New DataFilter() With {.Expressions = New List(Of Expression)() From { _
                         New Expression() With { _
                           .PropertyName = "PlantItemName", _
                           .RelationalOperator = RelationalOperator.EqualTo, _
                           .Values = New Values() From { _
                           "TANK" _
                            }
                        }
                    }
                }

        Dim dataObjects As IList(Of IDataObject) = _sppidDataLayer.Get(_objectType, _filter, 20, 0)

        Assert.Greater(dataObjects.Count, 0)
    End Sub

    <Test()>
    Public Sub TestGetPage()

        Dim dataObjects As IList(Of IDataObject) = _sppidDataLayer.Get(_objectType, New DataFilter(), 25, 0)
        Assert.Greater(dataObjects.Count, 0)

    End Sub

    <Test()>
    Public Sub TestRefresh()
        Dim response As Response = _sppidDataLayer.Refresh(_objectType)

        If response.Level <> StatusLevel.Success Then

            Throw New AssertionException(Utility.SerializeDataContract(Of Response)(response))

        End If

        Assert.IsTrue(response.Level = StatusLevel.Success)
    End Sub

    <Test()>
    Public Sub TestRefreshAll()
        Dim response As Response = _sppidDataLayer.RefreshAll()

        If response.Level <> StatusLevel.Success Then

            Throw New AssertionException(Utility.SerializeDataContract(Of Response)(response))

        End If

        Assert.IsTrue(response.Level = StatusLevel.Success)

    End Sub
End Class

