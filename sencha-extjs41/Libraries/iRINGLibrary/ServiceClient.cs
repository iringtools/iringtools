using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using org.iringtools.utility;
using System.Net;

namespace org.iringtools.library
{
  public class ServiceClient
  {
    protected WebHttpClient _webHttpClient = null;

    public ServiceClient(ServiceSettings settings, string baseUri)
    {
      string proxyHost = settings["ProxyHost"];
      string proxyPort = settings["ProxyPort"];

      if (!String.IsNullOrEmpty(proxyHost) && !String.IsNullOrEmpty(proxyPort))
      {
        WebProxy webProxy = settings.GetWebProxyCredentials().GetWebProxy() as WebProxy;
        _webHttpClient = new WebHttpClient(baseUri, null, webProxy);
      }
      else
      {
        _webHttpClient = new WebHttpClient(baseUri);
      }
    }
  }
}
