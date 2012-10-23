using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace org.iringtools.library
{
  [DataContract(Namespace = "http://www.iringtools.org/data", Name = "dataItems")]
  public class DataItems
  {
    [DataMember(Name = "type", Order = 0)]
    public string type { get; set; }

    [DataMember(Name = "version", Order = 1, EmitDefaultValue = false)]
    public string version { get; set; }

    [DataMember(Name = "total", Order = 2)]
    public long total { get; set; }

    [DataMember(Name = "start", Order = 3)]
    public int start { get; set; }

    [DataMember(Name = "limit", Order = 4)]
    public int limit { get; set; }

    [DataMember(Name = "items", Order = 5, EmitDefaultValue = false)]
    public List<DataItem> items { get; set; }
  }

  [DataContract(Namespace = "http://www.iringtools.org/data", Name = "item")]
  public class DataItem
  {
    [DataMember(Name = "id", Order = 0, EmitDefaultValue = false)]
    public string id { get; set; }

    [DataMember(Name = "properties", Order = 1, EmitDefaultValue = false)]
    public Dictionary<string, string> properties { get; set; }
    
    [DataMember(Name = "links", Order = 2, EmitDefaultValue = false)]
    public List<Link> links { get; set; }
  }

  [DataContract(Namespace = "http://www.iringtools.org/data", Name = "link")]
  public class Link
  {
    [DataMember(Name = "href", Order = 0)]
    public string href { get; set; }

    [DataMember(Name = "rel", Order = 1)]
    public string rel { get; set; }
  }
}
