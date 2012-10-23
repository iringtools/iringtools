using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using org.iringtools.library;
using org.iringtools.utility;
using System.Collections.Specialized;
using Ninject;
using System.Configuration;
using log4net;

namespace org.iringtools.web.Models
{
  public class FacadeRepository : IFacadeRepository
  {
    private static readonly ILog _logger = LogManager.GetLogger(typeof(FacadeRepository));
    private WebHttpClient _facadeServiceClient = null;        
    private string _facadeServiceURI = string.Empty;
    private string relativeUri = string.Empty;
    private string proxyHost = "";
    private string proxyPort = "";
    private WebProxy webProxy = null;
    private string facadeServiceUri = "";

    [Inject]
    public FacadeRepository()
    {
      NameValueCollection settings = ConfigurationManager.AppSettings;

      ServiceSettings _settings = new ServiceSettings();
      _settings.AppendSettings(settings);

      #region initialize webHttpClient for converting old mapping
      proxyHost = _settings["ProxyHost"];
      proxyPort = _settings["ProxyPort"];
      facadeServiceUri = _settings["FacadeServiceUri"];

      if (!String.IsNullOrEmpty(proxyHost) && !String.IsNullOrEmpty(proxyPort))
      {
        webProxy = _settings.GetWebProxyCredentials().GetWebProxy() as WebProxy; 
        _facadeServiceClient = new WebHttpClient(facadeServiceUri, null, webProxy);
      }
      else
      {
        _facadeServiceClient = new WebHttpClient(facadeServiceUri);
      }
      #endregion
    }

    public Response RefreshGraph(string scope, string app, string graph, string baseUrl)
    {
      Response resp = null;
      try
      {
        WebHttpClient _newServiceClient = GetFacadeServiceClient(baseUrl);
        relativeUri = string.Format("/{0}/{1}/{2}/refresh", scope, app, graph);
        resp = _newServiceClient.Get<Response>(relativeUri);
      }
      catch (Exception ex)
      {
        _logger.Error(ex.ToString());
      }
      return resp;
    }

    private WebHttpClient GetFacadeServiceClient(string baseUrl)
    {
      string baseUri = CleanBaseUrl(baseUrl.ToLower(), '/');
      string facadeBaseUri = CleanBaseUrl(facadeServiceUri.ToLower(), '/');

      if (!baseUri.Equals(facadeBaseUri))
        return GetServiceClinet(baseUrl, "facade/svc");
      else
        return _facadeServiceClient;
    }

    public WebHttpClient GetServiceClinet(string uri, string serviceName)
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