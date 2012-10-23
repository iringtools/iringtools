using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using org.iringtools.library;

namespace org.iringtools.nhibernate
{
  public interface IAuthorization
  {
    AccessLevel Authorize(string objectType, ref DataFilter dataFilter);
  }

  public enum AccessLevel
  {
    AccessDenied = 0,
    Read,
    Write,
    Delete,
  }

  public interface ISummary
  {
    IList<Object> GetSummary();
  }
}
