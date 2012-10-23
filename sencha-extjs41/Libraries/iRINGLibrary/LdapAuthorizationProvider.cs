using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using System.DirectoryServices.Protocols;
using System.Net;
using org.iringtools.utility;
using System.Collections;

namespace org.iringtools.adapter.security
{
  public class LdapAuthorizationProvider : IAuthorizationLayer
  {
    private static readonly ILog _logger = LogManager.GetLogger(typeof(LdapAuthorizationProvider));
    private const string BASE_DN = "o=iringtools,dc=iringug,dc=org";    

    private LdapConnection ldapConnection;
    private string authorizedGroup;

    public void Init(Properties properties)
    {
      string server = properties["server"];
      int portNumber = Int32.Parse(properties["portNumber"]);
      string userName = properties["userName"];
      string password = EncryptionUtility.Decrypt(properties["password"]);

      LdapDirectoryIdentifier ldapIdentifier = new LdapDirectoryIdentifier(server, portNumber, true, false);
      ldapConnection = new LdapConnection(ldapIdentifier);
      ldapConnection.Credential = new NetworkCredential(userName, password);
      ldapConnection.AuthType = AuthType.Basic;
      LdapSessionOptions options = ldapConnection.SessionOptions;
      options.ProtocolVersion = 3;
      ldapConnection.Bind();

      authorizedGroup = properties["authorizedGroup"];
    }

    public bool IsAuthorized(IDictionary claims)
    {
      string userId = GetUserId((IDictionary<string, string>)claims);

      if (userId != null && ldapConnection != null)
      {
        string groupDN = "cn=" + authorizedGroup + ",ou=groups," + BASE_DN;
        string qualUserId = "uid=" + userId + ",ou=users," + BASE_DN;
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
      if (claims.ContainsKey("BechtelUserName"))
        return claims["BechtelUserName"];

      return claims["EMailAddress"];
    }
  }
}
