using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using System.DirectoryServices.Protocols;
using System.Net;

namespace org.iringtools.adapter.security
{
  public class LdapAuthorizationProvider : IAuthorizationLayer
  {
    private static readonly ILog _logger = LogManager.GetLogger(typeof(LdapAuthorizationProvider));

    private const string BASE_DN = "ou=iringtools,dc=iringug,dc=org";
    private const string USERID_KEY = "EmailAddress";
    
    private LdapConnection ldapConnection;
    private string authorizedGroup;

    public void Init(IDictionary<string, string> settings)
    {
      string server = settings["server"];
      int portNumber = Int32.Parse(settings["portNumber"]);
      string userName = settings["userName"];
      string password = settings["password"];

      LdapDirectoryIdentifier ldapIdentifier = new LdapDirectoryIdentifier(server, portNumber, true, false);
      ldapConnection = new LdapConnection(ldapIdentifier);
      ldapConnection.Credential = new NetworkCredential(userName, password);
      ldapConnection.AuthType = AuthType.Basic;
      ldapConnection.Bind();

      authorizedGroup = settings["authorizedGroup"];
    }

    public bool IsAuthorized(IDictionary<string, string> claims)
    {
      string userId = GetUserId(claims);

      if (userId != null && ldapConnection != null)
      {
        string groupDN = "cn=" + authorizedGroup + ",cn=groups," + BASE_DN;
        string qualUserId = "uid=" + userId + ",cn=users," + BASE_DN;
        string filter = "(member=" + qualUserId + ")";

        SearchRequest request = new SearchRequest
        {
          DistinguishedName = groupDN,
          Filter = filter,
          Scope = System.DirectoryServices.Protocols.SearchScope.Subtree,
        };

        SearchResponse response = (SearchResponse)ldapConnection.SendRequest(request);
        UTF8Encoding utf8 = new UTF8Encoding(false, true);

        return (response.Entries.Count > 0);
      }

      return false;
    }

    private string GetUserId(IDictionary<string, string> claims)
    {
      foreach (var pair in claims)
      {
        if (pair.Key.ToLower() == USERID_KEY)
        {
          return pair.Value;
        }
      }

      return null;
    }
  }
}
