using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace org.iringtools.nhibernate.ext
{
  [CollectionDataContract(Namespace = "http://www.iringtools.org/adapter/datalayer/ext", Name = "config")]
  public class SummaryConfig : List<SummaryItem> {}

  [DataContract(Namespace = "http://www.iringtools.org/adapter/datalayer/ext", Name = "item")]
  public class SummaryItem
  {
    [DataMember(Name = "headers", Order = 0)]
    public Headers Headers { get; set; }

    [DataMember(Name = "query", Order = 0)]
    public string Query { get; set; }
  }

  [CollectionDataContract(Namespace = "http://www.iringtools.org/adapter/datalayer/ext", Name = "headers", ItemName = "header")]
  public class Headers : List<string> { }
}
