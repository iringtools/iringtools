using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConfigurationTool.Model
{
  class ProxyParams
  {
    #region Properties

    public string AppName { get; set; }
    public string RegAppName { get; set; }
    public string ServicesWebConfig { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public string RegUsername { get; set; }
    public string RegPassword { get; set; }
    public string Domain { get; set; }
    public string ProxyCredentialToken { get; set; }
    public string RegistryCredentialToken { get; set; }
    public string ProxyHost { get; set; }
    public string ProxyPort { get; set; }
    public bool IsEnable { get; set; }

    #endregion //Properties
  }
}
