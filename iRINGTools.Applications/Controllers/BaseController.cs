using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using org.iringtools.adapter.security;
using System.Collections;
using log4net;
using System.Configuration;
using org.iringtools.utility;

namespace org.iringtools.web.controllers
{
  public abstract class BaseController : Controller
  {
    protected IAuthenticationLayer _authenticationLayer = new OAuthProvider();
    protected IDictionary _allClaims = new Dictionary<string, string>();
    protected string _oAuthToken = String.Empty;
    protected string adapter_PREFIX = "adpmgr-";
    protected IAuthorizationLayer _authorizationLayer = new LdapAuthorizationProvider();
    private static readonly ILog _logger = LogManager.GetLogger(typeof(BaseController));
    private const string USERID_KEY = "emailaddress";

    public BaseController()
    {
      try
      {
        string enableOAuth = ConfigurationManager.AppSettings["EnableOAuth"];

        if (!String.IsNullOrEmpty(enableOAuth) && enableOAuth.ToUpper() == "TRUE")
        {
          _authenticationLayer.Authenticate(ref _allClaims, ref _oAuthToken);

          if (System.Web.HttpContext.Current.Response.IsRequestBeingRedirected)
              return;

          string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
          string ldapConfigFilePath = baseDirectory + @"App_Data\ldap.conf";

          if (System.IO.File.Exists(ldapConfigFilePath))
          {
            Properties ldapConfig = new Properties();
            ldapConfig.Load(ldapConfigFilePath);
            ldapConfig["authorizedGroup"] = "adapterAdmins";
            _authorizationLayer.Init(ldapConfig);

            if (!_authorizationLayer.IsAuthorized(_allClaims))
            {
              throw new UnauthorizedAccessException("User not authorized to access AdapterManager.");
            }
          }
          else
          {
            _logger.Warn("LDAP Configuration is missing!");
          }
        }
      }
      catch (Exception e)
      {
        _logger.Error(e.ToString());
        throw e;
      }
    }

    protected string GetUserId(IDictionary<string, string> claims)
    {
      foreach (var pair in claims)
      {
        if (pair.Key.ToLower() == USERID_KEY)
        {
          return pair.Value;
        }
      }

      return "guest";
    }
  }
}
