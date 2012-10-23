using System;
using System.IO;
using System.Configuration;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Net;
using Ninject;

using org.iringtools.adapter;
using org.iringtools.library;
using org.iringtools.utility;
using System.Xml.Linq;
using DocumentFormat.OpenXml.Packaging;
using log4net;

namespace org.iringtools.adapter.datalayer 
{
  public interface ISpreadsheetRepository  
  {
    SpreadsheetConfiguration GetConfiguration(string context, string endpoint, string baseurl);
    SpreadsheetConfiguration ProcessConfiguration(SpreadsheetConfiguration configuration, Stream inputFile, string baseUrl);
    List<WorksheetPart> GetWorksheets(SpreadsheetConfiguration configuration);
    List<SpreadsheetColumn> GetColumns(SpreadsheetConfiguration configuration, string worksheetName);
    void Configure(string context, string endpoint, string datalayer, SpreadsheetConfiguration configuration, Stream inputFile, string baseurl);
    byte[] getExcelFile(string context, string endpoint, string baseurl);
  }

  public class SpreadsheetRepository : ISpreadsheetRepository
  {
    private SpreadsheetProvider _provider { get; set; }
    private WebHttpClient _adapterServiceClient { get; set; }
    private static readonly ILog _logger = LogManager.GetLogger(typeof(SpreadsheetRepository));   
    private string proxyHost = "";
    private string proxyPort = "";
    private WebProxy webProxy = null;
    private string adapterServiceUri = "";

    [Inject]
    public SpreadsheetRepository()
    {      
      NameValueCollection settings = ConfigurationManager.AppSettings;      
      AdapterSettings _settings = new AdapterSettings();
      _settings.AppendSettings(settings);

      #region initialize webHttpClient for converting old mapping
      proxyHost = _settings["ProxyHost"];
      proxyPort = _settings["ProxyPort"];
      adapterServiceUri = _settings["AdapterServiceUri"];

      if (!String.IsNullOrEmpty(proxyHost) && !String.IsNullOrEmpty(proxyPort))
      {
        webProxy = _settings.GetWebProxyCredentials().GetWebProxy() as WebProxy;
        _adapterServiceClient = new WebHttpClient(adapterServiceUri, null, webProxy);        
      }
      else
      {        
        _adapterServiceClient = new WebHttpClient(adapterServiceUri);       
      }
      #endregion
    }

    public WebHttpClient getServiceClient(string uri, string serviceName)
    {
      WebHttpClient _newServiceClient = null;
      string serviceUri = uri + "/" + serviceName;

      if (!String.IsNullOrEmpty(proxyHost) && !String.IsNullOrEmpty(proxyPort))
      {
        _newServiceClient = new WebHttpClient(serviceUri, null, webProxy);
      }
      else
      {
        _newServiceClient = new WebHttpClient(serviceUri);
      }
      return _newServiceClient;
    }    

    public List<WorksheetPart> GetWorksheets(SpreadsheetConfiguration configuration)
    {
      List<WorksheetPart> wp = new List<WorksheetPart>();
      using (InitializeProvider(configuration))
      {
        foreach(SpreadsheetTable st in configuration.Tables)
        {
          wp.Add(_provider.GetWorksheetPart(st));
        }
        return wp;
      }
    }

    public byte[] getExcelFile(string context, string endpoint, string baseurl)
    {
      try
      {
        WebHttpClient _newServiceClient = PrepareServiceClient(baseurl, "adapter");
        var resourceDataUri = String.Format("/{0}/{1}/resourcedata", context, endpoint);
        DocumentBytes pathObject = _newServiceClient.Get<DocumentBytes>(resourceDataUri, true);

        return pathObject.Content;
      }
      catch (Exception ioEx)
      {
        _logger.Error(ioEx.Message);
        throw ioEx;
      }
    }

    public List<SpreadsheetColumn> GetColumns(SpreadsheetConfiguration configuration, string worksheetName)
    {
      using (InitializeProvider(configuration))
      {
      SpreadsheetTable table = configuration.Tables.Find(c => c.Name == worksheetName);
      if (table != null)
        return _provider.GetColumns(table);
      else
        return new List<SpreadsheetColumn>();
      }
    }

    public SpreadsheetConfiguration ProcessConfiguration(SpreadsheetConfiguration configuration, Stream inputFile, string baseUrl)
    {
      using (InitializeProvider(configuration))
      {
        return _provider.ProcessConfiguration(configuration, inputFile);
      }
    }

    public void Configure(string context, string endpoint, string datalayer, SpreadsheetConfiguration configuration, Stream inputFile, string baseurl)
    {
      try
      {
        WebHttpClient _newServiceClient = PrepareServiceClient(baseurl, "adapter");

        using (InitializeProvider(configuration))
        {
          List<MultiPartMessage> requestMessages = new List<MultiPartMessage>();

          if (datalayer != null)
          {
            requestMessages.Add(new MultiPartMessage
            {
              name = "DataLayer",
              message = datalayer,
              type = MultipartMessageType.FormData
            });

            requestMessages.Add(new MultiPartMessage
            {
              name = "Configuration",
              message = Utility.Serialize<XElement>(Utility.SerializeToXElement(configuration), true),
              type = MultipartMessageType.FormData
            });
            if (inputFile != null)
            {
              inputFile.Position = 0;
              requestMessages.Add(new MultiPartMessage
              {
                name = "SourceFile",
                fileName = configuration.Location,
                message = inputFile,
                //mimeType = "application/zip",
                mimeType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                type = MultipartMessageType.File
              });
              inputFile.Flush();
            }
            _newServiceClient.PostMultipartMessage(string.Format("/{0}/{1}/configure", context, endpoint), requestMessages);
          }
        }
      }
      catch (Exception ex)
      {
        PrepareErrorResponse(ex);
      }
    }

    public SpreadsheetConfiguration GetConfiguration(string context, string endpoint, string baseurl)
    {
      SpreadsheetConfiguration obj = null;
      WebHttpClient _newServiceClient = PrepareServiceClient(baseurl, "adapter");

      try
      {
        XElement element = _newServiceClient.Get<XElement>(string.Format("/{0}/{1}/configuration", context, endpoint));
        if (!element.IsEmpty)
        {
          obj = Utility.DeserializeFromXElement<SpreadsheetConfiguration>(element);
        }
      }
      catch (Exception ex)
      {
        PrepareErrorResponse(ex);
      }

      return obj;
    }

    #region Private methods for Spreadsheet

    private WebHttpClient PrepareServiceClient(string baseUrl, string serviceName)
    {
      if (baseUrl == "" || baseUrl == null)
        return _adapterServiceClient;

      string baseUri = CleanBaseUrl(baseUrl.ToLower(), '/');
      string adapterBaseUri = CleanBaseUrl(adapterServiceUri.ToLower(), '/');

      if (!baseUri.Equals(adapterBaseUri))
        return getServiceClient(baseUrl, serviceName);
      else
        return _adapterServiceClient;
    }

    private string CleanBaseUrl(string url, char con)
    {
      try
      {
        System.Uri uri = new System.Uri(url);
        return uri.Scheme + ":" + con + con + uri.Host + ":" + uri.Port;
      }
      catch (Exception) { }
      return null;
    }

    private SpreadsheetProvider InitializeProvider(SpreadsheetConfiguration configuration)
    {
      try
      {
        if (_provider == null)
        {
          _provider = new SpreadsheetProvider(configuration);
        }
      }
      catch (Exception ex)
      {
        PrepareErrorResponse(ex);
      }
      return _provider;
    }

    private Response PrepareErrorResponse(Exception ex)
    {
      Response response = new Response
      {
        Level = StatusLevel.Error,
        Messages = new Messages
          {
            ex.Message
          }
      };

      return response;
    }
  }
  #endregion   
}
