using System;
using System.Security.Principal;
using System.ServiceModel;
using Ninject;
using System.Collections;
using System.Collections.Generic;
using System.ServiceModel.Web;
using System.Web;
using System.Text;
using log4net;
using System.Net;
using System.Web.Script.Serialization;

namespace org.iringtools.adapter.identity
{
  public class SSOIdentityProvider : IIdentityLayer
  {
    private static readonly ILog _logger = LogManager.GetLogger(typeof(SSOIdentityProvider));
    
    public IDictionary GetKeyRing()
    {
      IDictionary keyRing = new Dictionary<string, string>();

      keyRing["Provider"] = "SSOIdentityProvider";

      if (WebOperationContext.Current != null && WebOperationContext.Current.IncomingRequest.Headers.Count > 0)
      {
        WebHeaderCollection headers = WebOperationContext.Current.IncomingRequest.Headers;

        string userAttrs = headers.Get("X-myPSN-UserAttributes");
        if (!String.IsNullOrEmpty(userAttrs))
        {
          JavaScriptSerializer desSSO = new JavaScriptSerializer();
          keyRing = desSSO.Deserialize<Dictionary<string, string>>(userAttrs);
        }

        string accessToken = headers.Get("X-myPSN-AccessToken");
        _logger.Debug("X-myPSN-AccessToken [" + accessToken + "]");
        if (!String.IsNullOrEmpty(accessToken))
          keyRing["X-myPSN-AccessToken"] = accessToken;
          keyRing["AccessToken"] = accessToken;

        _logger.Debug("X-myPSN-UserAttributes [" + userAttrs + "]");

        string emailAddress = headers.Get("X-myPSN-EmailAddress");
        _logger.Debug("X-myPSN-EmailAddress [" + emailAddress + "]");
        if (!String.IsNullOrEmpty(emailAddress))
          keyRing["X-myPSN-EmailAddress"] = emailAddress;

        string userId = headers.Get("X-myPSN-UserID");
        _logger.Debug("X-myPSN-UserID [" + userId + "]");
        if (!String.IsNullOrEmpty(userId))
          keyRing["X-myPSN-UserID"] = userId;

        string isBechtelEmployee = headers.Get("X-myPSN-IsBechtelEmployee");
        _logger.Debug("X-myPSN-IsBechtelEmployee [" + isBechtelEmployee + "]");

        keyRing["X-myPSN-IsBechtelEmployee"] = isBechtelEmployee;

        if (!String.IsNullOrEmpty(isBechtelEmployee) &&
          (isBechtelEmployee.ToLower() == "true" || isBechtelEmployee.ToLower() == "1"))
        {
          string bechtelUserName = headers.Get("X-myPSN-BechtelUserName");
          _logger.Debug("X-myPSN-BechtelUserName [" + bechtelUserName + "]");
          if (!String.IsNullOrEmpty(bechtelUserName))
          {
            keyRing["X-myPSN-BechtelUserName"] = bechtelUserName;
            keyRing["UserName"] = bechtelUserName;
          }
          
          string bechtelDomain = headers.Get("X-myPSN-BechtelDomain");
          _logger.Debug("X-myPSN-BechtelDomain [" + bechtelDomain + "]");
          if (!String.IsNullOrEmpty(bechtelDomain))
          {
            keyRing["X-myPSN-BechtelDomain"] = bechtelDomain;
            keyRing["DomainName"] = bechtelDomain;
          }

          string bechtelEmployeeNumber = headers.Get("X-myPSN-BechtelEmployeeNumber");
          _logger.Debug("X-myPSN-BechtelEmployeeNumber [" + bechtelEmployeeNumber + "]");
          if (!String.IsNullOrEmpty(bechtelEmployeeNumber))
            keyRing["X-myPSN-BechtelEmployeeNumber"] = bechtelEmployeeNumber;
        }
        else
        {
          keyRing["UserName"] = emailAddress;
        }
      }

      return keyRing;
    }
  }
}
