using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using org.iringtools.utility;

namespace org.iringtools.library
{
  [CollectionDataContract(Namespace = "http://www.iringtools.org/library", Name = "providers", ItemName = "provider")]
  public class DataProviders : List<Provider>
  {
    public DataProviders()
    {
      foreach (Provider provider in System.Enum.GetValues(typeof(Provider)))
      {
        this.Add(provider);
      }
    }
  }
}
