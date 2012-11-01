using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using org.iringtools.utility;

namespace org.iringtools.library
{
  [CollectionDataContract(Namespace = "http://www.iringtools.org/library", Name = "relationships", ItemName = "relationship")]
  public class DataRelationships : List<RelationshipType>
  {
    public DataRelationships()
    {
#if !SILVERLIGHT
      foreach (RelationshipType relationship in System.Enum.GetValues(typeof(RelationshipType)))
      {
        this.Add(relationship);
      }
#else
      this.AddRange(Utility.GetEnumValues<RelationshipType>());

#endif
    }
  }
}

