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

using System.Runtime.Serialization;

namespace org.iringtools.dxfr.manifest
{
  [System.Runtime.Serialization.DataContractAttribute(Name = "manifest", Namespace = "http://www.iringtools.org/dxfr/manifest")]
  public partial class Manifest
  {
    private org.iringtools.dxfr.manifest.Graphs graphsField;

    private string versionField;

    private org.iringtools.mapping.ValueListMaps valueListMapsField;

    public Manifest()
    {
      graphs = new Graphs();
      valueListMaps = new org.iringtools.mapping.ValueListMaps();
    }

    [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue = false, Order = 0)]
    public org.iringtools.dxfr.manifest.Graphs graphs
    {
      get
      {
        return this.graphsField;
      }
      set
      {
        this.graphsField = value;
      }
    }

    [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue = false, Order = 1)]
    public string version
    {
      get
      {
        return this.versionField;
      }
      set
      {
        this.versionField = value;
      }
    }

    [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue = false, Order = 2)]
    public org.iringtools.mapping.ValueListMaps valueListMaps
    {
      get
      {
        return this.valueListMapsField;
      }
      set
      {
        this.valueListMapsField = value;
      }
    }
  }

  [System.Runtime.Serialization.CollectionDataContractAttribute(Name = "graphs", Namespace = "http://www.iringtools.org/dxfr/manifest", ItemName = "graph")]
  public class Graphs : System.Collections.Generic.List<org.iringtools.dxfr.manifest.Graph>
  {
  }

  [System.Runtime.Serialization.DataContractAttribute(Name = "graph", Namespace = "http://www.iringtools.org/dxfr/manifest")]
  public partial class Graph
  {
    private string nameField;

    private org.iringtools.dxfr.manifest.ClassTemplatesList classTemplatesListField;

    public Graph()
    {
      classTemplatesList = new ClassTemplatesList();
    }

    [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue = false, Order = 0)]
    public string name
    {
      get
      {
        return this.nameField;
      }
      set
      {
        this.nameField = value;
      }
    }

    [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue = false, Order = 1)]
    public org.iringtools.dxfr.manifest.ClassTemplatesList classTemplatesList
    {
      get
      {
        return this.classTemplatesListField;
      }
      set
      {
        this.classTemplatesListField = value;
      }
    }
  }

  [System.Runtime.Serialization.CollectionDataContractAttribute(Name = "classTemplatesList", Namespace = "http://www.iringtools.org/dxfr/manifest", ItemName = "classTemplates")]
  public class ClassTemplatesList : System.Collections.Generic.List<org.iringtools.dxfr.manifest.ClassTemplates>
  {
  }

  [System.Runtime.Serialization.DataContractAttribute(Name = "classTemplates", Namespace = "http://www.iringtools.org/dxfr/manifest")]
  public partial class ClassTemplates
  {
    private org.iringtools.dxfr.manifest.Class classField;

    private org.iringtools.dxfr.manifest.Templates templatesField;

    public ClassTemplates()
    {
      templates = new Templates();
    }

    [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue = false, Order = 0)]
    public org.iringtools.dxfr.manifest.Class @class
    {
      get
      {
        return this.classField;
      }
      set
      {
        this.classField = value;
      }
    }

    [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue = false, Order = 1)]
    public org.iringtools.dxfr.manifest.Templates templates
    {
      get
      {
        return this.templatesField;
      }
      set
      {
        this.templatesField = value;
      }
    }
  }

  [System.Runtime.Serialization.DataContractAttribute(Name = "class", Namespace = "http://www.iringtools.org/dxfr/manifest")]
  public partial class Class
  {
    private string idField;

    private string nameField;

    [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue = false, Order = 0)]
    public string id
    {
      get
      {
        return this.idField;
      }
      set
      {
        this.idField = value;
      }
    }

    [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue = false, Order = 1)]
    public string name
    {
      get
      {
        return this.nameField;
      }
      set
      {
        this.nameField = value;
      }
    }
  }

  [System.Runtime.Serialization.CollectionDataContractAttribute(Name = "templates", Namespace = "http://www.iringtools.org/dxfr/manifest", ItemName = "template")]
  public class Templates : System.Collections.Generic.List<org.iringtools.dxfr.manifest.Template>
  {
  }

  [System.Runtime.Serialization.DataContractAttribute(Name = "template", Namespace = "http://www.iringtools.org/dxfr/manifest")]
  public partial class Template
  {
    private string idField;

    private string nameField;

    private org.iringtools.dxfr.manifest.Roles rolesField;

    private TransferOption transferOptionField;

    public Template()
    {
      roles = new Roles();
    }

    [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue = false, Order = 0)]
    public string id
    {
      get
      {
        return this.idField;
      }
      set
      {
        this.idField = value;
      }
    }

    [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue = false, Order = 1)]
    public string name
    {
      get
      {
        return this.nameField;
      }
      set
      {
        this.nameField = value;
      }
    }

    [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue = false, Order = 2)]
    public org.iringtools.dxfr.manifest.Roles roles
    {
      get
      {
        return this.rolesField;
      }
      set
      {
        this.rolesField = value;
      }
    }

    [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue = false, Order = 3)]
    public TransferOption transferOption
    {
      get
      {
        return this.transferOptionField;
      }
      set
      {
        this.transferOptionField = value;
      }
    }
  }

  [System.Runtime.Serialization.CollectionDataContractAttribute(Name = "roles", Namespace = "http://www.iringtools.org/dxfr/manifest", ItemName = "role")]
  public class Roles : System.Collections.Generic.List<org.iringtools.dxfr.manifest.Role>
  {
  }

  [System.Runtime.Serialization.DataContractAttribute(Name = "role", Namespace = "http://www.iringtools.org/dxfr/manifest")]
  public partial class Role
  {
    private org.iringtools.mapping.RoleType typeField;

    private string idField;

    private string nameField;

    private string dataTypeField;

    private int dataLengthField;

    private string valueField;

    private org.iringtools.dxfr.manifest.Class classField;

    private Cardinality cardinalityField;

    [System.Runtime.Serialization.DataMemberAttribute(Order = 0)]
    public org.iringtools.mapping.RoleType type
    {
      get
      {
        return this.typeField;
      }
      set
      {
        this.typeField = value;
      }
    }

    [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue = false, Order = 1)]
    public string id
    {
      get
      {
        return this.idField;
      }
      set
      {
        this.idField = value;
      }
    }

    [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue = false, Order = 2)]
    public string name
    {
      get
      {
        return this.nameField;
      }
      set
      {
        this.nameField = value;
      }
    }

    [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue = false, Order = 3)]
    public string dataType
    {
      get
      {
        return this.dataTypeField;
      }
      set
      {
        this.dataTypeField = value;
      }
    }

    [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue = false, Order = 4)]
    public string value
    {
      get
      {
        return this.valueField;
      }
      set
      {
        this.valueField = value;
      }
    }

    [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue = false, Order = 5)]
    public org.iringtools.dxfr.manifest.Class @class
    {
      get
      {
        return this.classField;
      }
      set
      {
        this.classField = value;
      }
    }

    [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue = false, Order = 6)]
    public Cardinality cardinality
    {
      get
      {
        return this.cardinalityField;
      }
      set
      {
        this.cardinalityField = value;
      }
    }

    [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue = false, Order = 7)]
    public int dataLength
    {
      get
      {
        return this.dataLengthField;
      }
      set
      {
        this.dataLengthField = value;
      }
    }
  }

  [System.Runtime.Serialization.DataContractAttribute(Name = "transferOption", Namespace = "http://www.iringtools.org/dxfr/manifest")]
  public enum TransferOption
  {

    [System.Runtime.Serialization.EnumMemberAttribute()]
    Unknown,

    [System.Runtime.Serialization.EnumMemberAttribute()]
    Desired,

    [System.Runtime.Serialization.EnumMemberAttribute()]
    Required,
  }

  [System.Runtime.Serialization.DataContractAttribute(Name = "cardinality", Namespace = "http://www.iringtools.org/dxfr/manifest")]
  public enum Cardinality
  {

    [System.Runtime.Serialization.EnumMemberAttribute()]
    Unknown,

    [System.Runtime.Serialization.EnumMemberAttribute()]
    Self,

    [System.Runtime.Serialization.EnumMemberAttribute()]
    OneToOne,

    [System.Runtime.Serialization.EnumMemberAttribute()]
    OneToMany,
  }
}

namespace org.iringtools.mapping
{
  using System.Runtime.Serialization;
  using System.Collections.Generic;
  using org.iringtools.library;

  [System.Runtime.Serialization.DataContractAttribute(Name = "roleType", Namespace = "http://www.iringtools.org/mapping")]
  public enum RoleType
  {
    [System.Runtime.Serialization.EnumMemberAttribute()]
    Unknown,

    [System.Runtime.Serialization.EnumMemberAttribute()]
    Property,

    [System.Runtime.Serialization.EnumMemberAttribute()]
    Possessor,

    [System.Runtime.Serialization.EnumMemberAttribute()]
    Reference,

    [System.Runtime.Serialization.EnumMemberAttribute()]
    FixedValue,

    [System.Runtime.Serialization.EnumMemberAttribute()]
    DataProperty,

    [System.Runtime.Serialization.EnumMemberAttribute()]
    ObjectProperty,
  }

  [System.Runtime.Serialization.DataContractAttribute(Name = "mapping", Namespace = "http://www.iringtools.org/mapping")]
  public partial class Mapping
  {
    private org.iringtools.mapping.GraphMaps graphMapsField;

    private org.iringtools.mapping.ValueListMaps valueListMapsField;

    private string versionField;    

    public Mapping()
    {
      graphMaps = new GraphMaps();
      valueListMaps = new ValueListMaps();
    }

    [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue = false, Order = 0)]
    public org.iringtools.mapping.GraphMaps graphMaps
    {
      get
      {
        return this.graphMapsField;
      }
      set
      {
        this.graphMapsField = value;
      }
    }

    [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue = false, Order = 1)]
    public org.iringtools.mapping.ValueListMaps valueListMaps
    {
      get
      {
        return this.valueListMapsField;
      }
      set
      {
        this.valueListMapsField = value;
      }
    }

    [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue = false, Order = 2)]
    public string version
    {
      get
      {
        return this.versionField;
      }
      set
      {
        this.versionField = value;
      }
    }

    public org.iringtools.mapping.GraphMap FindGraphMap(string name)
    {
      org.iringtools.mapping.GraphMap graph = null;
      foreach (org.iringtools.mapping.GraphMap item in this.graphMaps)
      {
        if (item.name.ToLower() == name.ToLower())
        {
          graph = item;
          break;
        }
      }
      return graph;
    }
  }  

  [System.Runtime.Serialization.CollectionDataContractAttribute(Name = "graphMaps", Namespace = "http://www.iringtools.org/mapping", ItemName = "graphMap")]
  public class GraphMaps : System.Collections.Generic.List<org.iringtools.mapping.GraphMap>
  {    
  }

  [System.Runtime.Serialization.CollectionDataContractAttribute(Name = "valueListMaps", Namespace = "http://www.iringtools.org/mapping", ItemName = "valueListMap")]
  public class ValueListMaps : System.Collections.Generic.List<org.iringtools.mapping.ValueListMap>
  {
  }

  [System.Runtime.Serialization.DataContractAttribute(Name = "graphMap", Namespace = "http://www.iringtools.org/mapping")]
  public partial class GraphMap
  {
    private string nameField;

    private org.iringtools.mapping.ClassTemplateMaps classTemplateMapsField;

    private string dataObjectNameField;

    private DataFilter dataFilterField;

    public GraphMap()
    {
      classTemplateMaps = new ClassTemplateMaps();
    }

    [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue = false, Order = 0)]
    public string name
    {
      get
      {
        return this.nameField;
      }
      set
      {
        this.nameField = value;
      }
    }

    [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue = false, Order = 1)]
    public org.iringtools.mapping.ClassTemplateMaps classTemplateMaps
    {
      get
      {
        return this.classTemplateMapsField;
      }
      set
      {
        this.classTemplateMapsField = value;
      }
    }

    [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue = false, Order = 2)]
    public string dataObjectName
    {
      get
      {
        return this.dataObjectNameField;
      }
      set
      {
        this.dataObjectNameField = value;
      }
    }

    [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue = false, Order = 3)]
    public DataFilter dataFilter
    {
      get
      {
        return this.dataFilterField;
      }
      set
      {
        this.dataFilterField = value;
      }
    }
  }

  [System.Runtime.Serialization.CollectionDataContractAttribute(Name = "classTemplateMaps", Namespace = "http://www.iringtools.org/mapping", ItemName = "classTemplateMap")]
  public class ClassTemplateMaps : System.Collections.Generic.List<org.iringtools.mapping.ClassTemplateMap>
  {
  }

  [System.Runtime.Serialization.DataContractAttribute(Name = "classTemplateMap", Namespace = "http://www.iringtools.org/mapping")]
  public partial class ClassTemplateMap
  {
    private org.iringtools.mapping.ClassMap classMapField;

    private org.iringtools.mapping.TemplateMaps templateMapsField;

    public ClassTemplateMap()
    {
      templateMaps = new TemplateMaps();
    }

    [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue = false, Order = 0)]
    public org.iringtools.mapping.ClassMap classMap
    {
      get
      {
        return this.classMapField;
      }
      set
      {
        this.classMapField = value;
      }
    }

    [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue = false, Order = 2)]
    public org.iringtools.mapping.TemplateMaps templateMaps
    {
      get
      {
        return this.templateMapsField;
      }
      set
      {
        this.templateMapsField = value;
      }
    }
  }

  [System.Runtime.Serialization.DataContractAttribute(Name = "classMap", Namespace = "http://www.iringtools.org/mapping")]
  public partial class ClassMap
  {
    private string nameField;

    private string idField;

    private string identifierDelimiterField;

    private org.iringtools.mapping.Identifiers identifiersField;

    private string identifierValueField;

    public ClassMap()
    {
      identifiers = new Identifiers();
    }

    [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue = false, Order = 0)]
    public string id
    {
      get
      {
        return this.idField;
      }
      set
      {
        this.idField = value;
      }
    }

    [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue = false, Order = 1)]
    public string name
    {
      get
      {
        return this.nameField;
      }
      set
      {
        this.nameField = value;
      }
    }

    [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue = false, Order = 2)]
    public string identifierDelimiter
    {
      get
      {
        return this.identifierDelimiterField;
      }
      set
      {
        this.identifierDelimiterField = value;
      }
    }

    [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue = false, Order = 3)]
    public org.iringtools.mapping.Identifiers identifiers
    {
      get
      {
        return this.identifiersField;
      }
      set
      {
        this.identifiersField = value;
      }
    }

    [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue = false, Order = 4)]
    public string identifierValue
    {
      get
      {
        return this.identifierValueField;
      }
      set
      {
        this.identifierValueField = value;
      }
    }
  }

  [System.Runtime.Serialization.CollectionDataContractAttribute(Name = "templateMaps", Namespace = "http://www.iringtools.org/mapping", ItemName = "templateMap")]
  public class TemplateMaps : System.Collections.Generic.List<org.iringtools.mapping.TemplateMap>
  {
  }

  [System.Runtime.Serialization.CollectionDataContractAttribute(Name = "identifiers", Namespace = "http://www.iringtools.org/mapping", ItemName = "identifier")]
  public class Identifiers : System.Collections.Generic.List<string>
  {
  }

  [System.Runtime.Serialization.DataContractAttribute(Name = "templateMap", Namespace = "http://www.iringtools.org/mapping")]
  public partial class TemplateMap
  {
    private string idField;

    private org.iringtools.mapping.TemplateType typeField;

    private string nameField;

    private org.iringtools.mapping.RoleMaps roleMapsField;

    public TemplateMap()
    {
      roleMaps = new RoleMaps();
    }

    [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue = false, Order = 0)]
    public org.iringtools.mapping.TemplateType type
    {
      get
      {
        return this.typeField;
      }
      set
      {
        this.typeField = value;
      }
    }

    [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue = false, Order = 1)]
    public string id
    {
      get
      {
        return this.idField;
      }
      set
      {
        this.idField = value;
      }
    }

    [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue = false, Order = 2)]
    public string name
    {
      get
      {
        return this.nameField;
      }
      set
      {
        this.nameField = value;
      }
    }

    [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue = false, Order = 3)]
    public org.iringtools.mapping.RoleMaps roleMaps
    {
      get
      {
        return this.roleMapsField;
      }
      set
      {
        this.roleMapsField = value;
      }
    }
  }

  [System.Runtime.Serialization.DataContractAttribute(Name = "TemplateType", Namespace = "http://www.iringtools.org/mapping")]
  public enum TemplateType
  {

    [System.Runtime.Serialization.EnumMemberAttribute()]
    Qualification,

    [System.Runtime.Serialization.EnumMemberAttribute()]
    Definition,
  }

  [System.Runtime.Serialization.CollectionDataContractAttribute(Name = "roleMaps", Namespace = "http://www.iringtools.org/mapping", ItemName = "roleMap")]
  public class RoleMaps : System.Collections.Generic.List<org.iringtools.mapping.RoleMap>
  {
  }

  [System.Runtime.Serialization.DataContractAttribute(Name = "roleMap", Namespace = "http://www.iringtools.org/mapping")]
  public partial class RoleMap
  {
    private org.iringtools.mapping.RoleType typeField;

    private string idField;

    private string nameField;

    private string dataTypeField;

    private int dataLengthField;

    private string valueField;

    private string propertyNameField;

    private string valueListNameField;

    private org.iringtools.mapping.ClassMap classMapField;

    [System.Runtime.Serialization.DataMemberAttribute(Order = 0)]
    public org.iringtools.mapping.RoleType type
    {
      get
      {
        return this.typeField;
      }
      set
      {
        this.typeField = value;
      }
    }

    [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue = false, Order = 1)]
    public string id
    {
      get
      {
        return this.idField;
      }
      set
      {
        this.idField = value;
      }
    }

    [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue = false, Order = 2)]
    public string name
    {
      get
      {
        return this.nameField;
      }
      set
      {
        this.nameField = value;
      }
    }

    [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue = false, Order = 3)]
    public string dataType
    {
      get
      {
        return this.dataTypeField;
      }
      set
      {
        this.dataTypeField = value;
      }
    }

    [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue = false, Order = 4)]
    public string value
    {
      get
      {
        return this.valueField;
      }
      set
      {
        this.valueField = value;
      }
    }

    [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue = false, Order = 5)]
    public string propertyName
    {
      get
      {
        return this.propertyNameField;
      }
      set
      {
        this.propertyNameField = value;
      }
    }

    [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue = false, Order = 6)]
    public string valueListName
    {
      get
      {
        return this.valueListNameField;
      }
      set
      {
        this.valueListNameField = value;
      }
    }

    [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue = false, Order = 7)]
    public org.iringtools.mapping.ClassMap classMap
    {
      get
      {
        return this.classMapField;
      }
      set
      {
        this.classMapField = value;
      }
    }

    public int dataLength
    {
      get
      {
        return this.dataLengthField;
      }
      set
      {
        this.dataLengthField = value;
      }
    }
  }

  [System.Runtime.Serialization.DataContractAttribute(Name = "valueListMap", Namespace = "http://www.iringtools.org/mapping")]
  public partial class ValueListMap
  {
    private string nameField;

    private org.iringtools.mapping.ValueMaps valueMapsField;

    public ValueListMap()
    {
      valueMaps = new ValueMaps();
    }

    [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue = false, Order = 0)]
    public string name
    {
      get
      {
        return this.nameField;
      }
      set
      {
        this.nameField = value;
      }
    }

    [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue = false, Order = 1)]
    public org.iringtools.mapping.ValueMaps valueMaps
    {
      get
      {
        return this.valueMapsField;
      }
      set
      {
        this.valueMapsField = value;
      }
    }
  }

  [System.Runtime.Serialization.CollectionDataContractAttribute(Name = "valueMaps", Namespace = "http://www.iringtools.org/mapping", ItemName = "valueMap")]
  public class ValueMaps : System.Collections.Generic.List<org.iringtools.mapping.ValueMap>
  {
  }

  [System.Runtime.Serialization.DataContractAttribute(Name = "valueMap", Namespace = "http://www.iringtools.org/mapping")]
  public partial class ValueMap
  {
    private string internalValueField;

    private string uriField;

    private string labelField;

    [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue = false, Order = 0)]
    public string internalValue
    {
      get
      {
        return this.internalValueField;
      }
      set
      {
        this.internalValueField = value;
      }
    }

    [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue = false, Order = 1)]
    public string uri
    {
      get
      {
        return this.uriField;
      }
      set
      {
        this.uriField = value;
      }
    }

    [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue = false, Order = 2)]
    public string label
    {
      get
      {
        return this.labelField;
      }
      set
      {
        this.labelField = value;
      }
    }
  }

  [CollectionDataContract(ItemName = "id")]
  public class TemplateIds : List<string> { }

  [DataContract(Name = "classificationTemplate", Namespace = "http://www.iringtools.org/mapping")]
  public class ClassificationTemplate
  {
    [DataMember(Name = "templateIds", EmitDefaultValue = false, Order = 0)]
    public TemplateIds TemplateIds { get; set; }

    [DataMember(Name = "templateMap", EmitDefaultValue = false, Order = 1)]
    public TemplateMap TemplateMap { get; set; }
  }

  [DataContract(Name = "classificationStyle", Namespace = "http://www.iringtools.org/mapping")]
  public enum ClassificationStyle
  {
    [EnumMember]
    Type,

    [EnumMember]
    Template,

    [EnumMember]
    Both
  }
}
