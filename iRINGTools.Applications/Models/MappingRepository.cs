using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Xml.Linq;
using System.Web;
using Ninject;
using log4net;
using System.Net;

using org.iringtools.library;
using org.iringtools.utility;
using org.iringtools.mapping;

namespace iRINGTools.Web.Models
{
  public class MappingRepository : IMappingRepository
  {
    private static readonly ILog _logger = LogManager.GetLogger(typeof(MappingRepository));
    private string _refDataServiceURI = string.Empty;
    private string proxyHost = "";
    private string proxyPort = "";
    private WebProxy webProxy = null;
    private string adapterServiceUri = "";   
    private WebHttpClient _adapterServiceClient = null;

    [Inject]
    public MappingRepository()
    {
      //_settings = ConfigurationManager.AppSettings;
      //_client = new WebHttpClient(_settings["AdapterServiceUri"]);

      NameValueCollection settings = ConfigurationManager.AppSettings;

      ServiceSettings _settings = new ServiceSettings();
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

    public Mapping GetMapping(string context, string endpoint, string baseUrl)
    {
      Mapping obj = null;

      try
      {
        WebHttpClient _newServiceClient = getAdapterServiceClient(baseUrl);
        obj = _newServiceClient.Get<Mapping>(String.Format("/{0}/{1}/mapping", context, endpoint), true);
      }
      catch (Exception ex)
      {
        _logger.Error(ex.ToString());
      }

      return obj;
    }

    public void UpdateMapping(Mapping mapping, string context, string endpoint, string baseUrl)
    {
      XElement mappingXml = XElement.Parse(Utility.SerializeDataContract<Mapping>(mapping));
      try
      {
        WebHttpClient _newServiceClient = getAdapterServiceClient(baseUrl);
        _newServiceClient.Post<XElement>(String.Format("/{0}/{1}/mapping", context, endpoint), mappingXml, true);
      }
      catch (Exception ex)
      {
        _logger.Error(ex.ToString());
      }
    }

    private WebHttpClient getAdapterServiceClient(string baseUrl)
    {
      string baseUri = CleanBaseUrl(baseUrl.ToLower(), '/');
      string adapterBaseUri = CleanBaseUrl(adapterServiceUri.ToLower(), '/');

      if (!baseUri.Equals(adapterBaseUri))
        return getServiceClinet(baseUrl, "adapter");
      else
        return _adapterServiceClient;
    }

    public WebHttpClient getServiceClinet(string uri, string serviceName)
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

    private string CleanBaseUrl(string url, char con)
    {
      System.Uri uri = new System.Uri(url);
      return uri.Scheme + ":" + con + con + uri.Host + ":" + uri.Port;
    }
  }
}