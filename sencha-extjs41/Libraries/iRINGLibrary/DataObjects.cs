using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace org.iringtools.library
{
  [CollectionDataContract(Namespace = "http://www.iringtools.org/library", Name = "objects", ItemName = "object")]
  public class DataObjects : List<String>
  {
  }
}
