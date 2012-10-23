using System;
using System.Collections.Specialized;
using System.Configuration;
using Ninject;
using log4net;
using org.iringtools.library;
using org.iringtools.utility;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Web.Script.Serialization;
using org.iringtools.adapter;
using System.Net;

namespace iRINGTools.Web.Models
{
  public class GridRepository : IGridRepository
  {
    private WebHttpClient _dataServiceClient = null;
    private static readonly ILog _logger = LogManager.GetLogger(typeof(AdapterRepository));   
    ServiceSettings _settings = null;
    string proxyHost = "";
    string proxyPort = "";
    string dataServiceUri = null;

    [Inject]
    public GridRepository()
    {
      NameValueCollection settings = ConfigurationManager.AppSettings;
      _settings = new ServiceSettings();
      _settings.AppendSettings(settings);
      proxyHost = _settings["ProxyHost"];
      proxyPort = _settings["ProxyPort"];

      #region initialize webHttpClient for converting old mapping      
      
      WebProxy webProxy = null;

      if (_settings["DataServiceURI"] != null)
      {
        dataServiceUri = _settings["DataServiceURI"];      
     
        if (!String.IsNullOrEmpty(proxyHost) && !String.IsNullOrEmpty(proxyPort))
        {
          webProxy = _settings.GetWebProxyCredentials().GetWebProxy() as WebProxy;
          _dataServiceClient = new WebHttpClient(dataServiceUri, null, webProxy);
        }
        else
        {
          _dataServiceClient = new WebHttpClient(dataServiceUri);

        }
      }
      #endregion

    }   

    public string DataServiceUri()
    {
      getSetting();
      string dataServiceUri = _settings["DataServiceURI"];
      string response = "";     

      if (string.IsNullOrEmpty(dataServiceUri))        
      {
        response = "DataServiceURI is not configured.";
        _logger.Error(response);
      }

      return response;
    }

    //public DataDictionary GetDictionary(string relUri, string baseUrl)
    //{
    //  WebHttpClient _newServiceClient = PrepareServiceClient(baseUrl, "adapter");
    //  string relativeUrl = string.Format("/{0}/dictionary?format=xml", relUri);
    //  return _newServiceClient.Get<DataDictionary>(relativeUrl, true);
    //}

    public DataDictionary GetDictionary(string contextName, string endpoint, string baseUrl)
    {
      DataDictionary obj = null;

      try
      {
        WebHttpClient _newServiceClient = PrepareServiceClient(baseUrl, "data");
        obj = _newServiceClient.Get<DataDictionary>(String.Format("/{0}/{1}/dictionary?format=xml", endpoint, contextName), true);
      }
      catch (Exception ex)
      {
        _logger.Error(ex.ToString());
      }

      return obj;
    }

    public DataItems GetDataItems(string endpoint, string context, string graph, DataFilter dataFilter, int start, int limit, string baseUrl)
    {
      WebHttpClient _newServiceClient = PrepareServiceClient(baseUrl, "data");      
      string fmt = "json";
      string relUrl = string.Format("/{0}/{1}/{2}/filter?format={3}&start={4}&limit={5}", endpoint, context, graph, fmt, start, limit);
      string json = _newServiceClient.Post<DataFilter, string>(relUrl, dataFilter, fmt, true);
      
      DataItemSerializer serializer = new DataItemSerializer();
      DataItems dataItems = serializer.Deserialize<DataItems>(json, false); 
      return dataItems;
    }

    private void getSetting()
    {
      if (_settings == null)
        _settings = new ServiceSettings();     
    }

    private void getAllSetting()
    {
      if (_settings == null)
        _settings = new ServiceSettings();
      getProxy();
    }

    private void getProxy()
    {     
      proxyHost = _settings["ProxyHost"];
      proxyPort = _settings["ProxyPort"];
    }

    private WebHttpClient PrepareServiceClient(string baseUrl, string serviceName)
    {
      getSetting();
      if (baseUrl == "" || baseUrl == null)
        return _dataServiceClient;

      string baseUri = CleanBaseUrl(baseUrl.ToLower(), '/');
      string adapterBaseUri = CleanBaseUrl(dataServiceUri.ToLower(), '/');

      if (!baseUri.Equals(adapterBaseUri))
        return getServiceClient(baseUrl, serviceName);
      else
        return _dataServiceClient;
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

    private WebHttpClient getServiceClient(string uri, string serviceName)
    {
      getProxy();
      WebHttpClient _newServiceClient = null;
      WebProxy webProxy = null;
      string serviceUri = uri + "/" + serviceName;

      if (!String.IsNullOrEmpty(proxyHost) && !String.IsNullOrEmpty(proxyPort))
      {
        webProxy = _settings.GetWebProxyCredentials().GetWebProxy() as WebProxy;
        _newServiceClient = new WebHttpClient(serviceUri, null, webProxy);
      }
      else
      {
        _newServiceClient = new WebHttpClient(serviceUri);
      }
      return _newServiceClient;
    }
  }
}