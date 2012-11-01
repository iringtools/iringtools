using System;
using System.Linq;
using System.Text;
using org.iringtools.utility;
using System.Collections;

namespace org.iringtools.adapter.security
{
  public interface IAuthorizationLayer
  {
    void Init(Properties properties);
    bool IsAuthorized(IDictionary claims);
  }
}
