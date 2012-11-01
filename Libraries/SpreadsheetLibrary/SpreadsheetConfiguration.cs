using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using System.Linq;
using System.Text;

using org.iringtools.library;
using org.iringtools.adapter.datalayer;

namespace org.iringtools.adapter.datalayer
{

  [DataContract(Name = "document")]
  public class SpreadsheetConfiguration
  {
    [DataMember(Name = "location", Order = 0, IsRequired = true)]
    public string Location { get; set; }

    [DataMember(Name = "generate", Order = 1)]
    public bool Generate { get; set; }

    [DataMember(Name = "tables", Order = 2)]
    public List<SpreadsheetTable> Tables { get; set; }
  }

  [DataContract(Name = "tableType")]
  public enum TableType
  {
    [EnumMemberAttribute]
    DefinedName = 0,

    [EnumMemberAttribute]
    Worksheet = 1
  }

  [DataContract(Name = "table")]
  public class SpreadsheetTable
  {    
    [DataMember(Name = "type", Order = 0)]
    public TableType TableType { get; set; }

    [DataMember(Name = "name", Order = 1)]
    public string Name { get; set; }

    [DataMember(Name = "label", Order = 2)]
    public string Label { get; set; }

    [DataMember(Name = "reference", Order = 3)]
    public string Reference { get; set; }

    [DataMember(Name = "header", Order = 4)]
    public int HeaderRow { get; set; }

    [DataMember(Name = "identifier", Order = 5)]
    public string Identifier { get; set; }
    
    [DataMember(Name = "columns", Order = 6)]
    public List<SpreadsheetColumn> Columns { get; set; }

    public SpreadsheetReference GetReference()
    {
      return new SpreadsheetReference(this.Reference);
    }
  }

  [DataContract(Name = "column")]
  public class SpreadsheetColumn
  {
    [DataMember(Name = "name", Order = 0)]
    public string Name { get; set; }

    [DataMember(Name = "label", Order = 1)]
    public string Label { get; set; }

    [DataMember(Name = "datatype", Order = 2)]
    public DataType DataType { get; set; }

    [DataMember(Name = "columnIdx", Order = 3)]
    public string ColumnIdx { get; set; }

    [DataMember(Name = "datalength", Order = 4)]
    public int DataLength { get; set; }
  }

}


