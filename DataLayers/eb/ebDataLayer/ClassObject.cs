using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using eB.Common.Enum;

namespace org.iringtools.adapter.datalayer.eb
{
  public class ClassObject
  {
    public string Name { get; set; }
    public ObjectType ObjectType { get; set; }
    public int GroupId { get; set; }
    public List<int> Ids { get; set; }
  }

  public enum GroupType
  {
    Tag = 17,
    Document = 1
  }
}
