using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace org.iringtools.library
{
  [DataContract(Namespace = "http://www.iringtools.org/data", Name = "Picklists")]
  public class Picklists
  {
    public Picklists()
    {
      picklistItems = new List<PicklistItem>();
    }

    [DataMember(IsRequired = true, Order = 0)]
    public string name { get; set; }

    [DataMember(IsRequired = false, Order = 1)]
    public string title { get; set; }

    [DataMember(IsRequired = false, Order = 2)]
    public int valueColumnIndex { get; set; }

    [DataMember(Name = "total", Order = 3)]
    public long total { get; set; }

    [DataMember(Name = "start", Order = 4)]
    public int start { get; set; }

    [DataMember(Name = "limit", Order = 5)]
    public int limit { get; set; }

    [DataMember(IsRequired = true, Order = 6)]
    public List<PicklistItem> picklistItems { get; set; }
  }

  [DataContract(Namespace = "http://www.iringtools.org/data", Name = "PicklistItem")]
  public class PicklistItem
  {
    public PicklistItem()
    {
      picklistColumns = new Dictionary<string, string>();
    }

    [DataMember(IsRequired = true, Order = 0)]
    public Dictionary<string, string> picklistColumns { get; set; }    
  }

  //Not used since the picklistColumns in PicklistItem is using Dictionary
  public class PicklistColumn
  {
    [DataMember(IsRequired = true, Order = 0)]
    public string columnName { get; set; }

    [DataMember(IsRequired = true, Order = 1)]
    public string columnValue { get; set; }
  }
}
