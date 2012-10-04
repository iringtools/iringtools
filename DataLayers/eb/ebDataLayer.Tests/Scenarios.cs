using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Runtime.Serialization;

namespace org.iringtools.test
{
  [CollectionDataContract(Namespace = "http://www.iringtools.org/test", Name = "senarios")]
  public class Scenarios : List<Scenario> { }

  [DataContract(Namespace = "http://www.iringtools.org/test", Name = "scenario")]
  public class Scenario
  {
    [DataMember(Name = "name", Order = 0)]
    public string Name { get; set; }

    [DataMember(Name = "objectType", Order = 1)]
    public string ObjectType { get; set; }

    [DataMember(Name = "identifierPadding", Order = 2)]
    public string IdentifierPadding { get; set; }

    [DataMember(Name = "properties", Order = 3)]
    public Properties Properties { get; set; }

    [DataMember(Name = "dataFilter", Order = 4)]
    public string DataFilter { get; set; }

    [DataMember(Name = "postable", Order = 5)]
    public bool Postable { get; set; }
  }

  [CollectionDataContract(Namespace = "http://www.iringtools.org/test", Name = "properties", ItemName = "property")]
  public class Properties : List<Property> { }

  [DataContract(Namespace = "http://www.iringtools.org/test", Name = "property")]
  public class Property
  {
    [DataMember(Name = "name", Order = 0)]
    public string Name { get; set; }

    [DataMember(Name = "value", Order = 1)]
    public string Value { get; set; }
  }
}
