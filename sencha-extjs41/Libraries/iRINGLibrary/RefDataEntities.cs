using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace org.iringtools.library
{
  [DataContract(Namespace = "http://www.iringtools.org/refdata/response", Name = "response")]
  public class RefDataEntities
  {
    public RefDataEntities()
    {
      Entities = new SortedEntities();
    }

    [DataMember(Name = "entities", Order = 0)]
    public SortedEntities Entities { get; set; }

    [DataMember(Name = "total", Order = 1)]
    public int Total { get; set; }
  }

  [CollectionDataContract(Namespace = "http://www.iringtools.org/refdata/response", Name = "entities", ItemName = "entity", KeyName = "key", ValueName = "value")]
  public class SortedEntities : SortedList<string, Entity>
  {
  }

  [CollectionDataContract(Namespace = "http://www.iringtools.org/refdata/response", Name = "entities", ItemName = "entity")]
  public class Entities : List<Entity>
  {
  }

  [DataContract(Namespace = "http://www.iringtools.org/refdata/response", Name = "entity")]
  public class Entity
  {
    [DataMember(Name = "uri")]
    public string Uri { get; set; }

    [DataMember(Name = "rdsuri")]
    public string RDSUri { get; set; }

    [DataMember(Name = "label")]
    public string Label { get; set; }

    [DataMember(Name = "lang")]
        public string Lang { get; set; }

    [DataMember(Name = "repository")]
    public string Repository { get; set; }

    public static IComparer<Entity> sortAscending()
    {
      return (IComparer<Entity>)new sortAscendingHelper();
    }

    private class sortAscendingHelper : IComparer<Entity>
    {
      int IComparer<Entity>.Compare(Entity e1, Entity e2)
      {
        return string.Compare(e1.Label, e2.Label);
      }
    }
  }
}
