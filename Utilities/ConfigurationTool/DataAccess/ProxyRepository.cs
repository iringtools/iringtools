using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace ConfigurationTool.DataAccess
{
  class ProxyRepository
  {
    public ProxyRepository()
    {
      //Get proxy host and port
      Refresh();
    }

    public void Refresh()
    {
      //Refresh proxy host and port
      _proxyHost = "";
      _proxyPort = "";
    }

    string _proxyHost;
    public string ProxyHost
    {
      get
      {
        return _proxyHost;
      }
      set
      {
        _proxyHost = value;
      }
    }

    string _proxyPort;
    public string ProxyPort
    {
      get
      {
        return _proxyPort;
      }
      set
      {
        _proxyPort = value;
      }
    }
  }
}
