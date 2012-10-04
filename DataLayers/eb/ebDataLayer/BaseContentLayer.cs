using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using org.iringtools.adapter;
using org.iringtools.library;

namespace org.iringtools.adapter.datalayer.eb
{
  public abstract class BaseContentLayer : BaseDataLayer, IContentLayer
  {
    public BaseContentLayer(AdapterSettings settings) : base (settings) {}

    public virtual IDictionary<string, string> GetHashValues(string objectType, IList<string> identifiers)
    {
      throw new NotImplementedException();
    }

    public virtual IDictionary<string, string> GetHashValues(string objectType, library.DataFilter filter, int pageSize, int startIndex)
    {
      throw new NotImplementedException();
    }

    public virtual IList<library.IContentObject> GetContents(string objectType, IList<string> identifiers)
    {
      throw new NotImplementedException();
    }

    public virtual IList<library.IContentObject> GetContents(string objectType, library.DataFilter filter, int pageSize, int startIndex)
    {
      throw new NotImplementedException();
    }

    public virtual library.Response PostContents(IList<library.IContentObject> contentObjects)
    {
      throw new NotImplementedException();
    }
  }
}
