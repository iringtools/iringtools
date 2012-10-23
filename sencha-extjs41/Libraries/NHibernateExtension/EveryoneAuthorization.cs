using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace org.iringtools.nhibernate.ext
{
  public class EveryoneAuthorization : IAuthorization
  {
    public AccessLevel Authorize(string objectType, ref library.DataFilter dataFilter)
    {
      return AccessLevel.Delete;
    }
  }
}
