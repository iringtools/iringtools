
Imports org.iringtools.adapter
Imports Ninject
Imports System.Collections.Specialized
Imports org.iringtools.library
Imports SD = StaticDust.Configuration
Imports Ninject.Extensions.Xml

Imports System.IO
Imports System.Configuration

Module Module1
  Private _baseDirectory As String = String.Empty
  Private _kernel As IKernel = Nothing
  Private _settings As NameValueCollection
  Private _adapterSettings As AdapterSettings
    Private _sppidDataLayer As IDataLayer2
    Private _queryVariables As Dictionary(Of String, String)
    Private _textReplacements As Dictionary(Of String, String)

  Sub Main()

        'Dim dataLayer As iRINGTools.SDK.SPPIDDataLayer.SPPIDDataLayer = New iRINGTools.SDK.SPPIDDataLayer.SPPIDDataLayer(New AdapterSettings(), Nothing)

        Dim ninjectSettings = New NinjectSettings() With {.LoadExtensions = False}
        Dim spdl As iRINGTools.SDK.SPPIDDataLayer.SPPIDDataLayer

        Dim queryLog As New Dictionary(Of String, String)
        Dim errorLog As New List(Of String)
        Dim tmp As String
        'Dim log As StreamWriter

        _textReplacements = New Dictionary(Of String, String)
        _queryVariables = New Dictionary(Of String, String)

        _kernel = New StandardKernel(ninjectSettings)

        _kernel.Load(New XmlExtensionModule())

        _kernel.Bind(Of AdapterSettings)().ToSelf().InSingletonScope()
        _adapterSettings = _kernel.[Get](Of AdapterSettings)()

        ' Start with some generic settings
        _settings = New NameValueCollection()

        _settings("XmlPath") = ".\12345_000\"
        _settings("ProjectDirName") = "12345_000\"
        _settings("ProjectName") = "12345_000"
        _settings("ApplicationName") = "SPPID"
        _settings("BaseConfigurationPath") = _settings("XmlPath") & _settings("ProjectName")
        _settings("BaseConcatPath") = _settings("ProjectDirName") & _settings("ProjectName")

        _baseDirectory = Directory.GetCurrentDirectory()
        _baseDirectory = _baseDirectory.Substring(0, _baseDirectory.LastIndexOf("\bin"))
        _settings("BaseDirectoryPath") = _baseDirectory
        Directory.SetCurrentDirectory(_baseDirectory)

        ' NOTE - for testing and to separate from other testers, the string '_Adrian' is inserted into the project config path
        ' Please create a new file (replacing 'Adrian' with your name) and customize the connection strings for your own testing
        'tmp = [String].Format("{0}_<YOUR NAME HERE>.{1}.config", _settings("BaseConcatPath"), _settings("ApplicationName"))
        tmp = [String].Format("{0}.Neha.{1}.config", _settings("BaseConcatPath"), _settings("ApplicationName"))
        _settings("ProjectConfigurationPath") = Path.Combine(_baseDirectory, tmp)

        tmp = [String].Format("{0}.StagingConfiguration.{1}.xml", _settings("BaseConcatPath"), _settings("ApplicationName"))
        _settings("StagingConfigurationPath") = Path.Combine(_baseDirectory, tmp)

        tmp = [String].Format("{0}BindingConfiguration.{1}.{2}.xml", _
                      _settings("ProjectDirName"), _settings("ProjectName"), _settings("ApplicationName"))
        _settings("BindingConfigurationPath") = Path.Combine(_baseDirectory, tmp)

        tmp = [String].Format("{0}QueryLog.txt", _settings("BaseConcatPath"))
        _settings("QueryLogPath") = Path.Combine(_baseDirectory, tmp)

        tmp = [String].Format("{0}ErrorLog.txt", _settings("BaseConcatPath"))
        _settings("ErrorLogPath") = Path.Combine(_baseDirectory, tmp)

        _adapterSettings.AppendSettings(_settings)


        'Dim appSettingsPath As String = [String].Format("{0}12345_000.SPPID.config", _adapterSettings("XmlPath"))

        'Testing()

        ' Add our specific settings
        If File.Exists(_settings("ProjectConfigurationPath")) Then
            _adapterSettings.AppendSettings(New SD.AppSettingsReader(_settings("ProjectConfigurationPath")))

        End If

        ' get the log streamwriter
        'log = File.CreateText(_settings("QueryLogPath"))

        ' set up the list of text replacements and query variables. The variable or text replacement key should be in the form
        ' <queryName>.<variableName>. the variableName portion of this for text replacements should always start with '!@~'
        ' if the queryName is set to !All then this replacement will apply to all queries

        ' NOTE: these key-value pairs are hard-coded here but should be built from user choices instead in a production environment
        'textReplacements.Add("!!All.!@~IncludeStockpileJoin", "left")

        ' NOTE: although you can provide a @ProjectDBName in the queryVariables, it will not be used to build the SITE data query and set the schema
        ' for queries in SPPID; this information is instead taken from the ProjectConfiguration file
        'queryVariables.Add("!All.@ProjectDBName", "whatever")

        ' Ninject Extension requires fully qualified path.
        Try

            _kernel.Load(Path.Combine(_baseDirectory, tmp))
            'spdl = _kernel.[Get](Of IDataLayer2)()
            spdl = New iRINGTools.SDK.SPPIDDataLayer.SPPIDDataLayer(_adapterSettings)

        Catch ex As Exception
      ' Intentionally Empty - catching an error here allows the querylog to be processed later
      Throw ex
        End Try

        Testing()

    End Sub


    Private Sub Testing()

        Dim ProjConfigPath As String = ""
        Dim x As Xml.XmlReader
        Dim nvc As New NameValueCollection

        ' *** debug
        ProjConfigPath = _adapterSettings("ProjectConfigurationPath")

        If File.Exists(ProjConfigPath) Then

            x = Xml.XmlReader.Create(ProjConfigPath)
            x.ReadToFollowing("appSettings")
            x.ReadToDescendant("add")

            Do
                nvc.Add(x.GetAttribute("key"), x.GetAttribute("value"))
            Loop While x.ReadToNextSibling("add")

        End If

        _adapterSettings.Add(nvc)





        'If File.Exists(ProjConfigPath) Then
        '    Dim asr As New StaticDust.Configuration.AppSettingsReader(ProjConfigPath)
        '    _adapterSettings.AppendSettings(asr)
        '    Debug.Print(asr.Item(0).ToString)
        'End If


    End Sub

End Module
