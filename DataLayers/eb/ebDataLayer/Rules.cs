using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace org.iringtools.adapter.datalayer.eb
{
  [XmlType("rules")]
  public class Rules : List<Rule> {}

  [XmlType("rule")]
  public class Rule
  {
    [XmlAttribute("id")]
    public int Id { get; set; }

    [XmlElement("name")]
    public string Name { get; set; }

    [XmlElement("relationshiptemplate")]
    public string RelationshipTemplate { get; set; }

    [XmlElement("relatedobjecttype")]
    public int RelatedObjectType { get; set; }

    [XmlElement("eql")]
    public string Eql { get; set; }

    [XmlArray("parameters")]
    public Parameter[] Parameters { get; set; }

    [XmlArray("selfchecks", IsNullable = true)]
    public SelfCheck[] SelfChecks { get; set; }

    [XmlElement("create")]
    public bool Create { get; set; }

    [XmlElement("createtemplate", IsNullable = true)]
    public string CreateTemplate { get; set; }
  }

  [XmlType("parameter")]
  public class Parameter
  {
    [XmlAttribute("position")]
    public int Position { get; set; }

    [XmlAttribute("placeholder")]
    public int Placeholder { get; set; }

    [XmlAttribute("seperator")]
    public string Seperator { get; set; }

    [XmlText]
    public string Value { get; set; }
  }

  [XmlType("check")]
  public class SelfCheck
  {
    [XmlAttribute("column")]
    public string Column { get; set; }

    [XmlAttribute("value")]
    public string Value { get; set; }

    [XmlAttribute("operator")]
    public string Operator { get; set; }
  }
}
