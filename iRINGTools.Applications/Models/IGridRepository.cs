
using org.iringtools.library;

namespace iRINGTools.Web.Models
{
  public interface IGridRepository
  {
    DataDictionary GetDictionary(string contextName, string endpoint, string baseUrl);

    DataItems GetDataItems(string endpoint, string context, string graph, DataFilter dataFilter, int start, int limit, string baseUrl);

    string DataServiceUri();
  }
}