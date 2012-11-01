using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Collections.Specialized;
using System.Net;
using System.Configuration;
using System.IO;
using System.Web.Script.Serialization;
using System.Collections;
using org.iringtools.utility;
using log4net;

namespace org.iringtools.adapter.security
{
  public class OAuthProvider : IAuthenticationLayer
  {

    private static readonly ILog _logger = LogManager.GetLogger(typeof(OAuthProvider));
    const string LOGON_COOKIE_NAME = "Auth";

    public string Authenticate(ref System.Collections.IDictionary allClaims, ref string OAuthToken)
    {
      string authenticatedUserName = "";

      //three use cases
      // Case 1: the user has already logged in and the application has already processed the SSO event
      // Case 2: the user needs to login
      // Case 3: the user has logged in but the application needs to process the SSO event

      // Case 1: the user has already logged in and the application has already processed the SSO event
      if (HttpContext.Current.Request.Cookies[LOGON_COOKIE_NAME] != null)
      {
        allClaims = new Dictionary<String, String>();

        HttpCookie authCookie = HttpContext.Current.Request.Cookies[LOGON_COOKIE_NAME];

        NameValueCollection col = new NameValueCollection(authCookie.Values);
        String[] keyNames = col.AllKeys;
        foreach (string key in keyNames)
        {
          if (key != null)
          {
            allClaims.Add(key, HttpUtility.UrlDecode(col.GetValues(key)[0]));
          }
        }

        authenticatedUserName = authCookie["subject"].ToString();
      }
      else
      {
        // Case 3: the user has logged in but the application needs to process the SSO event

        //the REF key will be different each time even for the same user
        //it's only good for a few seconds, use it quick
        if (HttpContext.Current.Request["REF"] != null)
        {
          //call ping federate to get the attributes of the authenticated user
          string referenceID = HttpContext.Current.Request.QueryString["REF"].ToString();

          //this information is unique to the application 
          WebRequest req = WebRequest.Create(ConfigurationManager.AppSettings["AuthenticationWebServiceAddress"].ToString() + referenceID);
          req.Headers.Add("ping.uname", ConfigurationManager.AppSettings["AuthWebServiceUserName"].ToString());
          req.Headers.Add("ping.pwd", ConfigurationManager.AppSettings["AuthWebServicePassword"].ToString());
          req.Headers.Add("ping.instanceId", ConfigurationManager.AppSettings["AuthWebServiceInstanceID"].ToString());

          //if you need to use a proxy to get there, then this is that
          string proxyCreds = ConfigurationManager.AppSettings["ProxyCredentialToken"];
          if (!String.IsNullOrEmpty(proxyCreds))
          {
            string host = ConfigurationManager.AppSettings["ProxyHost"];
            int port = int.Parse(ConfigurationManager.AppSettings["ProxyPort"]);
            string bypassOnLocal = ConfigurationManager.AppSettings["ProxyBypassOnLocal"];
            string bypassList = ConfigurationManager.AppSettings["ProxyBypassList"];
            WebProxyCredentials webCreds = new WebProxyCredentials(proxyCreds, host, port, bypassOnLocal, bypassList);

            req.Proxy = webCreds.GetWebProxy();  // Note that the Proxy returned by GetWebProxy has already had it's .Credentials set
          }

          //get the response from the service 
          WebResponse resp = req.GetResponse();
          StreamReader stream = new StreamReader(resp.GetResponseStream());
          string response = stream.ReadToEnd();

          _logger.Debug("PingResponse: " + response);

          //response from both pingfederate is json - deserialize that into dictionaries for easier operation
          JavaScriptSerializer desSSO = new JavaScriptSerializer();
          IDictionary userInfo = desSSO.Deserialize<Dictionary<string, string>>(response);

          //ping federate interaction complete
          // --------------------------------------------------------------------------------------------------------------------------



          //the following code is not necessary unless you are calling APIs through apigee



          //call apigee to get the oauth headers for calling services
          //if (!String.IsNullOrEmpty(ConfigurationManager.AppSettings["applicationKey"]))
          //{
          //  _logger.Debug("ApplicationKey: " + ConfigurationManager.AppSettings["applicationKey"]);

          //  string tokenServerAddress = ConfigurationManager.AppSettings["tokenServiceAddress"].ToString() + ConfigurationManager.AppSettings["applicationKey"].ToString();
          //  WebRequest reqApigee = WebRequest.Create(tokenServerAddress);
          //  reqApigee.Method = "POST";

          //  if (!String.IsNullOrEmpty(proxyCreds))
          //  {
          //    string host = ConfigurationManager.AppSettings["ProxyHost"];
          //    int port = int.Parse(ConfigurationManager.AppSettings["ProxyPort"]);
          //    WebProxyCredentials webCreds = new WebProxyCredentials(proxyCreds, host, port);

          //    reqApigee.Proxy = webCreds.GetWebProxy();
          //    reqApigee.Proxy.Credentials = webCreds.GetNetworkCredential();
          //  }

          //  //post the json response from ping federate to the apigee url
          //  ASCIIEncoding encoding = new ASCIIEncoding();
          //  byte[] data = encoding.GetBytes(response);

          //  reqApigee.ContentType = "application/xml";
          //  reqApigee.ContentLength = data.Length;
          //  Stream newStream = reqApigee.GetRequestStream();

          //  // Send the data.
          //  newStream.Write(data, 0, data.Length);
          //  newStream.Close();

          //  //get back the response from apigee
          //  WebResponse respApigee = reqApigee.GetResponse();
          //  StreamReader streamApigee = new StreamReader(respApigee.GetResponseStream());
          //  string responseApigee = streamApigee.ReadToEnd();
          //  string accessToken = responseApigee.Replace("{\"accesstoken\":", "");

          //  //response from apigee is json - deserialize that into dictionaries for the purposes of display
          //  JavaScriptSerializer desApigee = new JavaScriptSerializer();
          //  IDictionary apigeeInfo = desApigee.Deserialize<Dictionary<string, string>>(accessToken.Replace("}}", "}"));

            //foreach (DictionaryEntry entry in userInfo)
            //{
            //  userInfo.Add("OAuth " + entry.Key.ToString(), entry.Value);

            //  if (entry.Key.ToString() == "token")
            //  {

          OAuthToken = userInfo["OAuthToken"].ToString();
          _logger.Debug("OAuthToken: " + OAuthToken);
                
          HttpCookie authorizationCookie = new HttpCookie("Authorization");
          authorizationCookie.Value = OAuthToken;
          HttpContext.Current.Response.Cookies.Add(authorizationCookie);

          HttpCookie appKeyCookie = new HttpCookie("X-myPSN-AppKey");
          appKeyCookie.Value = ConfigurationManager.AppSettings["applicationKey"];
          HttpContext.Current.Response.Cookies.Add(appKeyCookie);
          //    }
          //  }
          //}
          //end of apigee interaction
          //---------------------------------------------



          //process dictionaries and respond

          HttpCookie authCookie = new HttpCookie(LOGON_COOKIE_NAME);

          foreach (DictionaryEntry entry in userInfo)
          {
            switch (entry.Key.ToString())
            {
              case "not-before":
                break;
              case "authnContext":
                break;
              case "not-on-or-after":
                break;
              case "renew-until":
                break;
              default:
                try
                {
                  authCookie.Values[entry.Key.ToString()] = HttpUtility.UrlEncode(entry.Value.ToString());
                }
                catch { }
                break;
            }
          }

          authenticatedUserName = userInfo["subject"].ToString();

          _logger.Debug("authenticatedUserName: " + authenticatedUserName);

          HttpContext.Current.Response.Cookies.Add(authCookie);
          allClaims = userInfo;
        }
        else
        {
          // Case 2: the user needs to login

          string port = HttpContext.Current.Request.ServerVariables["SERVER_PORT"];
          if (port == null || port == "80" || port == "443")
            port = "";
          else
            port = ":" + port;

          string returnPath = HttpContext.Current.Request.Url.Scheme + ":" + "//" +
              HttpContext.Current.Request.Url.Host + port +
              HttpContext.Current.Request.Url.PathAndQuery;

          returnPath = HttpContext.Current.Server.UrlEncode(returnPath);

          string FederationServerAddress = ConfigurationManager.AppSettings["FederationServerAddress"];
          string PartnerIdpId = ConfigurationManager.AppSettings["PartnerIdpId"];
          string SPFederationEndPoint = ConfigurationManager.AppSettings["SPFederationEndPoint"];

          string url = FederationServerAddress + SPFederationEndPoint + "?PartnerIdpId=" + PartnerIdpId +
              "&TargetResource=" + returnPath;

          //This does not end the call in WCF, unlike ASP.NET.
          HttpContext.Current.Response.Redirect(url);
          return null;
        }
      }
      return authenticatedUserName;
    }
  }
}
