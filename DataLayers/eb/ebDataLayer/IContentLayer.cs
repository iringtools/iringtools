using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using org.iringtools.library;

namespace org.iringtools.adapter.datalayer.eb
{
  public interface IContentLayer : IDataLayer2
  {
    IDictionary<string, string> GetHashValues(string objectType, IList<string> identifiers);
    IDictionary<string, string> GetHashValues(string objectType, DataFilter filter, int pageSize, int startIndex);    
    IList<IContentObject> GetContents(string objectType, IList<string> identifiers);
    IList<IContentObject> GetContents(string objectType, DataFilter filter, int pageSize, int startIndex);
    Response PostContents(IList<IContentObject> contentObjects);
  }
}
