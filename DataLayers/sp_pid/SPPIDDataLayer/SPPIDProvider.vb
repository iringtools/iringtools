Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports System.IO

Imports org.iringtools.adapter
Imports org.iringtools.utility
Imports org.iringtools.library
Public Class SPPIDProvider

    Implements IDisposable
    Private _settings As AdapterSettings = Nothing
    Private _configurationPath As String = String.Empty
    Private _configuration As SPPIDConfiguration = Nothing

    Public Sub New(configuration As SPPIDConfiguration)
        InitializeProvider(configuration)
    End Sub
    Public Sub New(settings As AdapterSettings)
        _settings = settings
        _configurationPath = Path.Combine(_settings("AppDataPath"), "SPPID-configuration." + _settings("Scope") & ".xml")

        If File.Exists(_configurationPath) Then

            'if (_configuration.Generate)
            '{
            ' _configuration = ProcessConfiguration(_configuration, null);
            '  _configuration.Generate = false;
            '  Utility.Write<SPPIDConfiguration>(_configuration, _configurationPath, true);
            ' }
            InitializeProvider(utility.Utility.Read(Of SPPIDConfiguration)(_configurationPath))
        End If
    End Sub

    Public Sub InitializeProvider(configuration As SPPIDConfiguration)
        If configuration IsNot Nothing Then
            'if (File.Exists(_configuration.Location))
            '{
            ' // if (_stream == null) _stream = OpenStream(_configuration.Location);
            '  if (_document == null) _document = GetDocument(_configuration.Location);
            '  if (_configuration.Generate)
            '  {
            '    _configuration = ProcessConfiguration(_configuration, null);
            '    _configuration.Generate = false;
            '    Utility.Write<SPPIDConfiguration>(_configuration, _configurationPath, true);
            '  }
            '}
            _configuration = configuration
        End If
    End Sub

    Public Overloads Sub Dispose() Implements IDisposable.Dispose
      
    End Sub


End Class

