// Copyright (c) 2009, ids-adi.org /////////////////////////////////////////////
// All rights reserved.
//------------------------------------------------------------------------------
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//     * Redistributions of source code must retain the above copyright
//       notice, this list of conditions and the following disclaimer.
//     * Redistributions in binary form must reproduce the above copyright
//       notice, this list of conditions and the following disclaimer in the
//       documentation and/or other materials provided with the distribution.
//     * Neither the name of the ids-adi.org nor the
//       names of its contributors may be used to endorse or promote products
//       derived from this software without specific prior written permission.
//------------------------------------------------------------------------------
// THIS SOFTWARE IS PROVIDED BY ids-adi.org ''AS IS'' AND ANY
// EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL ids-adi.org BE LIABLE FOR ANY
// DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
// LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
// ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
// SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using System.Text;

namespace org.ids_adi.qmxf
{
  [DataContract(Namespace = "http://ns.ids-adi.org/qxf/model#", Name = "qmxf")]
  [XmlRoot(Namespace = "http://ns.ids-adi.org/qxf/model#", ElementName = "qmxf")]
  public class QMXF
  {
    public QMXF()
    {
      this.classDefinitions = new List<ClassDefinition>();
      this.templateDefinitions = new List<TemplateDefinition>();
      this.templateQualifications = new List<TemplateQualification>();
      this.licenses = new List<License>();
    }

    [DataMember(Name = "class-definition", EmitDefaultValue = false)]
    [XmlElement(ElementName = "class-definition")]
    public List<ClassDefinition> classDefinitions { get; set; }

    [DataMember(Name = "template-definition", EmitDefaultValue = false)]
    [XmlElement(ElementName = "template-definition")]
    public List<TemplateDefinition> templateDefinitions { get; set; }

    [DataMember(Name = "template-qualification", EmitDefaultValue = false)]
    [XmlElement(ElementName = "template-qualification")]
    public List<TemplateQualification> templateQualifications { get; set; }

    [DataMember(Name = "license", EmitDefaultValue = false)]
    [XmlElement(ElementName = "license")]
    public List<License> licenses { get; set; }

    [DataMember(Name = "timestamp", EmitDefaultValue = false)]
    [XmlAttribute(AttributeName = "timestamp")]
    public string timestamp { get; set; }

    [DataMember(Name = "license-ref", EmitDefaultValue = false)]
    [XmlAttribute(AttributeName = "license-ref")]
    public string licenseRef { get; set; }

    [DataMember(Name = "targetRepository", EmitDefaultValue = false)]
    [XmlAttribute(AttributeName = "targetRepository")]
    public string targetRepository { get; set; }

    [DataMember(Name = "sourceRepository", EmitDefaultValue = false)]
    [XmlAttribute(AttributeName = "sourceRepository")]
    public string sourceRepository { get; set; }

  }

  [DataContract(Namespace = "http://ns.ids-adi.org/qxf/model#", Name = "template-definition")]
  public class TemplateDefinition
  {
    public TemplateDefinition()
    {
      this.name = new List<QMXFName>();
      this.description = new List<Description>();
      this.suggestedDesignation = new List<SuggestedDesignation>();
      this.status = new List<QMXFStatus>();
      this.textualDefinition = new List<TextualDefinition>();
      this.roleDefinition = new List<RoleDefinition>();
      this.repositoryName = string.Empty;
      this.specialization = new List<Specialization>();
    }

    [DataMember(Name = "repository", EmitDefaultValue = false)]
    [XmlElement(ElementName = "repository")]
    public string repositoryName { get; set; }

    [DataMember(Name = "suggested-designation", EmitDefaultValue = false)]
    [XmlElement(ElementName = "suggested-designation")]
    public List<SuggestedDesignation> suggestedDesignation { get; set; }

    [DataMember(Name = "designation", EmitDefaultValue = false)]
    [XmlElement(ElementName = "designation")]
    public Designation designation { get; set; }

    [DataMember(Name = "name", EmitDefaultValue = false)]
    [XmlElement(ElementName = "name")]
    public List<QMXFName> name { get; set; }

    [DataMember(Name = "description", EmitDefaultValue = false)]
    [XmlElement(ElementName = "description")]
    public List<Description> description { get; set; }

    [DataMember(Name = "textual-definition", EmitDefaultValue = false)]
    [XmlElement(ElementName = "textual-definition")]
    public List<TextualDefinition> textualDefinition { get; set; }

    [DataMember(Name = "status", EmitDefaultValue = false)]
    [XmlElement(ElementName = "status")]
    public List<QMXFStatus> status { get; set; }

    [DataMember(Name = "id", EmitDefaultValue = false)]
    [XmlAttribute(AttributeName = "id")]
    public string identifier { get; set; }

    [DataMember(Name = "role-definition", EmitDefaultValue = false)]
    [XmlElement(ElementName = "role-definition")]
    public List<RoleDefinition> roleDefinition { get; set; }

    [DataMember(Name = "specialization", EmitDefaultValue = false)]
    [XmlElement(ElementName = "specialization")]
    public List<Specialization> specialization { get; set; }
  }

  [DataContract(Namespace = "http://ns.ids-adi.org/qxf/model#", Name = "class-definition")]
  public class ClassDefinition
  {
    public ClassDefinition()
    {
      this.name = new List<QMXFName>();
      this.description = new List<Description>();
      this.classification = new List<Classification>();
      this.specialization = new List<Specialization>();
      this.status = new List<QMXFStatus>();
      this.suggestedDesignation = new List<SuggestedDesignation>();
      this.textualDefinition = new List<TextualDefinition>();
      this.repositoryName = string.Empty;
    }

    [DataMember(Name = "repository", EmitDefaultValue = false)]
    [XmlElement(ElementName = "repository")]
    public string repositoryName { get; set; }

    [DataMember(Name = "entity-type", EmitDefaultValue = false)]
    [XmlElement(ElementName = "entity-type")]
    public EntityType entityType { get; set; }

    [DataMember(Name = "classification", EmitDefaultValue = false)]
    [XmlElement(ElementName = "classification")]
    public List<Classification> classification { get; set; }

    [DataMember(Name = "specialization", EmitDefaultValue = false)]
    [XmlElement(ElementName = "specialization")]
    public List<Specialization> specialization { get; set; }

    [DataMember(Name = "suggested-designation", EmitDefaultValue = false)]
    [XmlElement(ElementName = "suggested-designation")]
    public List<SuggestedDesignation> suggestedDesignation { get; set; }

    [DataMember(Name = "designation", EmitDefaultValue = false)]
    [XmlElement(ElementName = "designation")]
    public Designation designation { get; set; }

    [DataMember(Name = "name", EmitDefaultValue = false)]
    [XmlElement(ElementName = "name")]
    public List<QMXFName> name { get; set; }

    [DataMember(Name = "description", EmitDefaultValue = false)]
    [XmlElement(ElementName = "description")]
    public List<Description> description { get; set; }

    [DataMember(Name = "textual-definition", EmitDefaultValue = false)]
    [XmlElement(ElementName = "textual-definition")]
    public List<TextualDefinition> textualDefinition { get; set; }

    [DataMember(Name = "status", EmitDefaultValue = false)]
    [XmlElement(ElementName = "status")]
    public List<QMXFStatus> status { get; set; }

    [DataMember(Name = "id", EmitDefaultValue = false)]
    [XmlAttribute(AttributeName = "id")]
    public string identifier { get; set; }

  }

  [DataContract(Namespace = "http://ns.ids-adi.org/qxf/model#", Name = "template-qualification")]
  public class TemplateQualification
  {
    public TemplateQualification()
    {
      this.name = new List<QMXFName>();
      this.description = new List<Description>();
      this.suggestedDesignation = new List<SuggestedDesignation>();
      this.designation = new List<Designation>();
      this.status = new List<QMXFStatus>();
      this.textualDefinition = new List<TextualDefinition>();
      this.roleQualification = new List<RoleQualification>();
      this.specialization = new List<Specialization>();
      this.repositoryName = string.Empty;
    }

    [DataMember(Name = "repository", EmitDefaultValue = false)]
    [XmlElement(ElementName = "repository")]
    public string repositoryName { get; set; }

    [DataMember(Name = "suggested-designation", EmitDefaultValue = false)]
    [XmlElement(ElementName = "suggested-designation")]
    public List<SuggestedDesignation> suggestedDesignation { get; set; }

    [DataMember(Name = "specialization", EmitDefaultValue = false)]
    [XmlElement(ElementName = "specialization")]
    public List<Specialization> specialization { get; set; }

    [DataMember(Name = "designation", EmitDefaultValue = false)]
    [XmlElement(ElementName = "designation")]
    public List<Designation> designation { get; set; }

    [DataMember(Name = "name", EmitDefaultValue = false)]
    [XmlElement(ElementName = "name")]
    public List<QMXFName> name { get; set; }

    [DataMember(Name = "description", EmitDefaultValue = false)]
    [XmlElement(ElementName = "description")]
    public List<Description> description { get; set; }

    [DataMember(Name = "textual-definition", EmitDefaultValue = false)]
    [XmlElement(ElementName = "textual-definition")]
    public List<TextualDefinition> textualDefinition { get; set; }

    [DataMember(Name = "status", EmitDefaultValue = false)]
    [XmlElement(ElementName = "status")]
    public List<QMXFStatus> status { get; set; }

    [DataMember(Name = "id", EmitDefaultValue = false)]
    [XmlAttribute(AttributeName = "id")]
    public string identifier { get; set; }

    [DataMember(Name = "qualifies", EmitDefaultValue = false)]
    [XmlAttribute(AttributeName = "qualifies")]
    public string qualifies { get; set; }

    [DataMember(Name = "role-qualification", EmitDefaultValue = false)]
    [XmlElement(ElementName = "role-qualification")]
    public List<RoleQualification> roleQualification { get; set; }

  }

  [DataContract(Namespace = "http://ns.ids-adi.org/qxf/model#", Name = "license")]
  public class License
  {
    public License()
    {
      this.licenseTerms = new List<LicenseTerms>();
    }

    [DataMember(Name = "license-terms", EmitDefaultValue = false)]
    [XmlElement(ElementName = "license-terms")]
    public List<LicenseTerms> licenseTerms { get; set; }
  }

  [DataContract(Namespace = "http://ns.ids-adi.org/qxf/model#", Name = "license-terms")]
  public class LicenseTerms
  {
    public LicenseTerms()
    {
      this.licenseText = new List<LicenseText>();
    }

    [DataMember(Name = "license-text", EmitDefaultValue = false)]
    [XmlElement(ElementName = "license-text")]
    public List<LicenseText> licenseText { get; set; }

    [DataMember(Name = "id", EmitDefaultValue = false)]
    [XmlAttribute(AttributeName = "id")]
    public string identifier { get; set; }

  }

  [DataContract(Namespace = "http://ns.ids-adi.org/qxf/model#", Name = "license-text")]
  public class LicenseText
  {
    public LicenseText()
    {
      this.formal = "true";
    }

    [DataMember(EmitDefaultValue = false)]
    [XmlAttribute]
    public string lang { get; set; }

    [DataMember(EmitDefaultValue = false)]
    [XmlText]
    public string value { get; set; }

    [DataMember(EmitDefaultValue = false)]
    [XmlAttribute]
    public string formal { get; set; }
  }

  [DataContract(Namespace = "http://ns.ids-adi.org/qxf/model#", Name = "entity-type")]
  public class EntityType
  {
    [DataMember(EmitDefaultValue = false)]
    [XmlAttribute]
    public string reference { get; set; }
  }

  [DataContract(Namespace = "http://ns.ids-adi.org/qxf/model#", Name = "classification")]
  public class Classification
  {
    [DataMember(EmitDefaultValue = false)]
    [XmlAttribute]
    public string reference { get; set; }

    [DataMember(EmitDefaultValue = false)]
    [XmlAttribute]
    public string label { get; set; }

    [DataMember(EmitDefaultValue = false)]
    [XmlAttribute]
    public string lang { get; set; }

    public static IComparer<Classification> sortAscending()
    {
      return (IComparer<Classification>)new sortAscendingHelper();
    }

    private class sortAscendingHelper : IComparer<Classification>
    {
      int IComparer<Classification>.Compare(Classification cd1, Classification cd2)
      {
        return string.Compare(cd1.label, cd2.label);
      }
    }
  }

  [DataContract(Namespace = "http://ns.ids-adi.org/qxf/model#", Name = "specialization")]
  public class Specialization
  {
    [DataMember(EmitDefaultValue = false)]
    [XmlAttribute]
    public string reference { get; set; }

    [DataMember(EmitDefaultValue = false)]
    [XmlAttribute]
    public string rdsuri { get; set; }

    [DataMember(EmitDefaultValue = false)]
    [XmlAttribute]
    public string label { get; set; }

    [DataMember(EmitDefaultValue = false)]
    [XmlAttribute]
    public string lang { get; set; }

    public static IComparer<Specialization> sortAscending()
    {
      return (IComparer<Specialization>)new sortAscendingHelper();
    }

    private class sortAscendingHelper : IComparer<Specialization>
    {
      int IComparer<Specialization>.Compare(Specialization s1, Specialization s2)
      {
        return string.Compare(s1.label, s2.label);
      }
    }
  }

  [DataContract(Namespace = "http://ns.ids-adi.org/qxf/model#", Name = "suggested-designation")]
  public class SuggestedDesignation
  {
    [DataMember(EmitDefaultValue = false)]
    [XmlText]
    public string value { get; set; }
  }

  [DataContract(Namespace = "http://ns.ids-adi.org/qxf/model#", Name = "name")]
  public class QMXFName
  {
    //TODO: Ignore for now
    [DataMember(EmitDefaultValue = false)]
    [XmlAttribute]
    public string lang { get; set; }

    [DataMember(EmitDefaultValue = false)]
    [XmlText]
    public string value { get; set; }
  }

  [DataContract(Namespace = "http://ns.ids-adi.org/qxf/model#", Name = "designation")]
  public class Designation
  {
    [DataMember(EmitDefaultValue = false)]
    [XmlText]
    public string value { get; set; }
  }

  [DataContract(Namespace = "http://ns.ids-adi.org/qxf/model#", Name = "description")]
  public class Description
  {
    //TODO: Ignore for now
    [DataMember(EmitDefaultValue = false)]
    [XmlAttribute]
    public string lang { get; set; }

    //TODO: Ignore for now
    [DataMember(EmitDefaultValue = false)]
    [XmlAttribute]
    public string contentType { get; set; }

    //TODO: Ignore for now
    [DataMember(EmitDefaultValue = false)]
    [XmlAttribute]
    public string parseType { get; set; }

    [DataMember(EmitDefaultValue = false)]
    [XmlText]
    public string value { get; set; }
  }

  [DataContract(Namespace = "http://ns.ids-adi.org/qxf/model#", Name = "textual-definition")]
  public class TextualDefinition
  {
    public TextualDefinition()
    {
      this.lang = "en-GB";
    }

    [DataMember(EmitDefaultValue = false)]
    [XmlAttribute]
    public string lang { get; set; }

    [DataMember(EmitDefaultValue = false)]
    [XmlText]
    public string value { get; set; }
  }

  [DataContract(Namespace = "http://ns.ids-adi.org/qxf/model#", Name = "rule-set")]
  public class RuleSet
  {
    [DataMember(EmitDefaultValue = false)]
    [XmlAttribute]
    public string contentType { get; set; }

    [DataMember(EmitDefaultValue = false)]
    [XmlAttribute]
    public string dlType { get; set; }

    [DataMember(EmitDefaultValue = false)]
    [XmlAttribute]
    public string parseType { get; set; }

    [DataMember(EmitDefaultValue = false)]
    [XmlText]
    public string value { get; set; }
  }

  [DataContract(Namespace = "http://ns.ids-adi.org/qxf/model#", Name = "restriction")]
  public class PropertyRestriction
  {
    [DataMember(Name = "type", EmitDefaultValue = false)]
    [XmlElement(ElementName = "type")]
    public string type { get; set; }

    [DataMember(Name = "value", EmitDefaultValue = false)]
    [XmlElement(ElementName = "value")]
    public string value { get; set; }

    [DataMember(Name = "valuesFrom", EmitDefaultValue = false)]
    [XmlElement(ElementName = "valuesFrom")]
    public string valuesFrom { get; set; }

    [DataMember(Name = "cardinality", EmitDefaultValue = false)]
    [XmlElement(ElementName = "cardinality")]
    public string cardiniality { get; set; }
  }

  [DataContract(Namespace = "http://ns.ids-adi.org/qxf/model#", Name = "role-definition")]
  public class RoleDefinition
  {
    public RoleDefinition()
    {
      this.name = new List<QMXFName>();
      this.suggestedDesignation = new List<SuggestedDesignation>();
      this.restrictions = new List<PropertyRestriction>();
      //this.designation = new Designation();
      this.description = new Description();
      //this.range = "http://www.w3.org/2000/01/rdf-schema#Class";
      //this.minimum = "1";
      //this.maximum = "1";
      //this.inverseMinimum = "0";
      //this.inverseMaximum = "unbounded";
    }

    [DataMember(Name = "restriction", EmitDefaultValue = false)]
    [XmlElement(ElementName = "restriction")]
    public List<PropertyRestriction> restrictions { get; set; }

    [DataMember(Name = "name", EmitDefaultValue = false)]
    [XmlElement(ElementName = "name")]
    public List<QMXFName> name { get; set; }

    [DataMember(Name = "suggested-designation", EmitDefaultValue = false)]
    [XmlElement(ElementName = "suggested-designation")]
    public List<SuggestedDesignation> suggestedDesignation { get; set; }

    [DataMember(Name = "designation", EmitDefaultValue = false)]
    [XmlElement(ElementName = "designation")]
    public Designation designation { get; set; }

    [DataMember(Name = "description", EmitDefaultValue = false)]
    [XmlElement(ElementName = "description")]
    public Description description { get; set; }

    [DataMember(Name = "id", EmitDefaultValue = false)]
    [XmlAttribute(AttributeName = "id")]
    public string identifier { get; set; }

    [DataMember(Name = "range", EmitDefaultValue = false)]
    [XmlAttribute(AttributeName = "range")]
    public string range { get; set; }

    [DataMember(Name = "minimum", EmitDefaultValue = false)]
    [XmlAttribute(AttributeName = "minimum")]
    public string minimum { get; set; }

    [DataMember(Name = "maximum", EmitDefaultValue = false)]
    [XmlAttribute(AttributeName = "maximum")]
    public string maximum { get; set; }

    [DataMember(Name = "inverse-minimum", EmitDefaultValue = false)]
    [XmlAttribute(AttributeName = "inverse-minimum")]
    public string inverseMinimum { get; set; }

    [DataMember(Name = "inverse-maximum", EmitDefaultValue = false)]
    [XmlAttribute(AttributeName = "inverse-maximum")]
    public string inverseMaximum { get; set; }

    public static IComparer<RoleDefinition> sortAscending()
    {
      return (IComparer<RoleDefinition>)new sortAscendingHelper();
    }

    private class sortAscendingHelper : IComparer<RoleDefinition>
    {
      int IComparer<RoleDefinition>.Compare(RoleDefinition rd1, RoleDefinition rd2)
      {
        return string.Compare(rd1.name[0].value, rd2.name[0].value);
      }
    }
  }

  [DataContract(Namespace = "http://ns.ids-adi.org/qxf/model#", Name = "value")]
  public class QMXFValue
  {
    [DataMember(EmitDefaultValue = false)]
    [XmlText]
    public string text { get; set; }

    [DataMember(EmitDefaultValue = false)]
    [XmlAttribute]
    public string lang { get; set; }

    [DataMember(EmitDefaultValue = false)]
    [XmlAttribute]
    public string reference { get; set; }

    [DataMember(Name = "as", EmitDefaultValue = false)]
    [XmlAttribute(AttributeName = "as")]
    public string As { get; set; }
  }

  [DataContract(Namespace = "http://ns.ids-adi.org/qxf/model#", Name = "role-qualification")]
  public class RoleQualification
  {
    public RoleQualification()
    {
      this.name = new List<QMXFName>();
      this.description = new List<Description>();
      //this.value = new Value();
      //this.minimum = "1";
      //this.maximum = "1";
      //this.inverseMinimum = "0";
      //this.inverseMaximum = "unbounded";
    }

    [DataMember(Name = "name", EmitDefaultValue = false)]
    [XmlElement(ElementName = "name")]
    public List<QMXFName> name { get; set; }

    [DataMember(Name = "id", EmitDefaultValue = false)]
    [XmlAttribute(AttributeName = "id")]
    public string identifier { get; set; }

    [DataMember(Name = "value", EmitDefaultValue = false)]
    [XmlElement(ElementName = "value")]
    public QMXFValue value { get; set; }

    [DataMember(Name = "description", EmitDefaultValue = false)]
    [XmlElement(ElementName = "description")]
    public List<Description> description { get; set; }

    [DataMember(Name = "qualifies", EmitDefaultValue = false)]
    [XmlAttribute(AttributeName = "qualifies")]
    public string qualifies { get; set; }

    [DataMember(Name = "range", EmitDefaultValue = false)]
    [XmlAttribute(AttributeName = "range")]
    public string range { get; set; }

    [DataMember(Name = "minimum", EmitDefaultValue = false)]
    [XmlAttribute(AttributeName = "minimum")]
    public string minimum { get; set; }

    [DataMember(Name = "maximum", EmitDefaultValue = false)]
    [XmlAttribute(AttributeName = "maximum")]
    public string maximum { get; set; }

    [DataMember(Name = "inverse-minimum", EmitDefaultValue = false)]
    [XmlAttribute(AttributeName = "inverse-minimum")]
    public string inverseMinimum { get; set; }

    [DataMember(Name = "inverse-maximum", EmitDefaultValue = false)]
    [XmlAttribute(AttributeName = "inverse-maximum")]
    public string inverseMaximum { get; set; }

    public static IComparer<RoleQualification> sortAscending()
    {
      return (IComparer<RoleQualification>)new sortAscendingHelper();
    }

    private class sortAscendingHelper : IComparer<RoleQualification>
    {
      int IComparer<RoleQualification>.Compare(RoleQualification rq1, RoleQualification rq2)
      {
        return string.Compare(rq1.name[0].value, rq2.name[0].value);
      }
    }
  }

  [DataContract(Namespace = "http://ns.ids-adi.org/qxf/model#", Name = "status")]
  public class QMXFStatus
  {
    public QMXFStatus()
    {
      this.authority = "http://rdl.rdlfacade.org/data#R6569332477";
      this.Class = "http://rdl.rdlfacade.org/data#R3732211754";
    }

    [DataMember(Name = "authority", EmitDefaultValue = false)]
    [XmlAttribute(AttributeName = "authority")]
    public string authority { get; set; }

    [DataMember(Name = "class", EmitDefaultValue = false)]
    [XmlAttribute(AttributeName = "class")]
    public string Class { get; set; }

    [DataMember(Name = "from", EmitDefaultValue = false)]
    [XmlAttribute(AttributeName = "from")]
    public string from { get; set; }

    [DataMember(Name = "to", EmitDefaultValue = false)]
    [XmlAttribute(AttributeName = "to")]
    public string to { get; set; }
  }

  [DataContract(Namespace = "http://ns.ids-adi.org/qxf/model#", Name = "type")]
  public enum QMXFType
  {
    [EnumMember(Value = "qualification")]
    [XmlElement(ElementName = "qualification")]
    Qualification,

    [EnumMember(Value = "definition")]
    [XmlElement(ElementName = "definition")]
    Definition
  }
}
