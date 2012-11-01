using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace org.iringtools.utility
{
  [DataContract]
  public class GenericDataTable
  {
    [DataMember]
    public int Count { get; set; }

    [DataMember]
    public List<ColumnDescriptor> Columns { get; set; }

    [DataMember]
    public List<List<string>> Rows { get; set; }
  }

  [DataContract]
  public class ColumnDescriptor
  {
    [DataMember]
    public string ColumnName { get; set; }

    [DataMember]
    public string DataType { get; set; }

    [DataMember]
    public string DisplayName { get; set; }

    [DataMember]
    public int DisplayOrder { get; set; }
  }
}
