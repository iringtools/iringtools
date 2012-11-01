// Copyright (c) 2009, ids-adi.org /////////////////////////////////////////////
// All rights reserved.
//------------------------------------------------------------------------------
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//     * Redistributions of source code must retain the above copyright
//       notice, this list of conditions and the following disclaimer.
//     * Redistributions in binary form must reproduce the above copyright
//       notice, this list of conditions and the following disclaimer in the
//       documentation and/or other materials provided with the distribution.
//     * Neither the name of the ids-adi.org nor the
//       names of its contributors may be used to endorse or promote products
//       derived from this software without specific prior written permission.
//------------------------------------------------------------------------------
// THIS SOFTWARE IS PROVIDED BY ids-adi.org ''AS IS'' AND ANY
// EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL ids-adi.org BE LIABLE FOR ANY
// DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
// LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
// ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
// SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
////////////////////////////////////////////////////////////////////////////////

using System;
using System.Runtime.Serialization;
using System.Net;

namespace org.iringtools.utility
{
  [DataContract]
  public class WebCredentials
  {
    protected static string _delimiter = "{`|";
    protected static string[] _delimiterArray = { _delimiter };

    public WebCredentials()
    {
      isEncrypted = false;
    }

    public WebCredentials(string encryptedCredentials)
    {
      if (encryptedCredentials != String.Empty && encryptedCredentials != null)
      {
        encryptedToken = encryptedCredentials;
        isEncrypted = true;
      }
      else
      {
        isEncrypted = false;
      }
    }

    public virtual void Encrypt()
    {
      string credentials = userName + _delimiter + password + _delimiter + domain;
      encryptedToken = Encryption.EncryptString(credentials);
      userName = null;
      password = null;
      domain = null;
      isEncrypted = true;
    }

    public virtual void Decrypt()
    {
      string credentials = Encryption.DecryptString(encryptedToken);
      string[] credentialsArray = credentials.Split(_delimiterArray, StringSplitOptions.None);

      userName = credentialsArray[0];
      password = credentialsArray[1];
      domain = credentialsArray[2];
      encryptedToken = null;
      isEncrypted = false;
    }

    public NetworkCredential GetNetworkCredential()
    {
      NetworkCredential credentials = null;

      if (this.userName != string.Empty && this.userName != null)
      {
        credentials = new NetworkCredential(userName, password, domain);
      }
      else
      {
        credentials = CredentialCache.DefaultNetworkCredentials;
      }

      return credentials;
    }

    [DataMember(EmitDefaultValue = false)]
    public bool isEncrypted { get; set; }

    [DataMember(EmitDefaultValue = false)]
    public string userName { get; set; }

    [DataMember(EmitDefaultValue = false)]
    public string password { get; set; }

    [DataMember(EmitDefaultValue = false)]
    public string domain { get; set; }

    [DataMember(EmitDefaultValue = false)]
    public string encryptedToken { get; set; }
  }

  [DataContract]
  public class WebProxyCredentials
    : WebCredentials
  {
    public WebProxyCredentials()
      : base()
    {
      this.proxyHost = string.Empty;
      this.proxyPort = 0;
    }

    public WebProxyCredentials(string encryptedCredentials, string hostName, int port)
      : base(encryptedCredentials)
    {
      Decrypt();
      this.proxyHost = hostName;
      this.proxyPort = port;
    }

    public WebProxyCredentials(string encryptedCredentials, string hostName, int port, string bypassOnLocal, string bypassList)
      : base(encryptedCredentials)
    {
      Decrypt();
      this.proxyHost = hostName;
      this.proxyPort = port;
      this.proxyBypassOnLocal = bypassOnLocal;
      this.proxyBypassList = bypassList;
    }

    [DataMember(EmitDefaultValue = false)]
    public string proxyHost { get; set; }

    [DataMember(EmitDefaultValue = false)]
    public int proxyPort { get; set; }

    [DataMember(EmitDefaultValue = false)]
    public string proxyBypassList { get; set; }

    [DataMember(EmitDefaultValue = false)]
    public string proxyBypassOnLocal { get; set; }

    public IWebProxy GetWebProxy()
    {
      IWebProxy webProxy = null;

      if (!string.IsNullOrEmpty(proxyHost))
      {
        // proxyBypassList is a list of regular expressions separated by ";" used to match against URL's that should bypass the proxy
        string[] bypassList = string.IsNullOrEmpty(proxyBypassList) ? null : proxyBypassList.Split(';');
        Boolean bypassOnLocal = string.IsNullOrEmpty(proxyBypassOnLocal) ? true : (proxyBypassOnLocal.ToLower() == "true");
        string proxyAddress = proxyHost + ":" + proxyPort.ToString();
        webProxy = new WebProxy(proxyAddress, true, bypassList); // NB" 2nd Parameter = "true" indicates to bypass proxy for local addresses
      }
      else
        webProxy = WebRequest.GetSystemWebProxy();

      webProxy.Credentials = this.GetNetworkCredential();

      return webProxy;
    }

  }
}
