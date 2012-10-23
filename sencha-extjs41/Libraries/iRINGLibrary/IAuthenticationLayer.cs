using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace org.iringtools.adapter.security
{
  public interface IAuthenticationLayer
  {
    string Authenticate(ref IDictionary allClaims, ref string OAuthToken);
  }
}
