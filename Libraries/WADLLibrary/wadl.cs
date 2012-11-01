using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace net.java.dev.wadl
{
  /// <remarks/>
  [System.Xml.Serialization.XmlRootAttribute(ElementName = "application", Namespace = "http://wadl.dev.java.net/2009/02", IsNullable = false)]
  public partial class WADLApplication
  {
    private List<object> itemsField = new List<object>();

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("doc", typeof(WADLDocumentation))]
    [System.Xml.Serialization.XmlElementAttribute("resources", typeof(WADLResources))]
    [System.Xml.Serialization.XmlElementAttribute("method", typeof(WADLMethod))]
    [System.Xml.Serialization.XmlElementAttribute("param", typeof(WADLParameter))]
    public List<object> Items
    {
      get
      {
        return this.itemsField;
      }
      set
      {
        this.itemsField = value;
      }
    }
  }

  /// <remarks/>
  [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.0.30319.225")]
  [System.Diagnostics.DebuggerStepThroughAttribute()]
  [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://wadl.dev.java.net/2009/02")]
  public partial class WADLDocumentation
  {
    private string xmlLangField;

    private string titleField;

    private string valueField;

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string title
    {
      get
      {
        return this.titleField;
      }
      set
      {
        this.titleField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute("xml:lang")]
    public string lang
    {
      get
      {
        return this.xmlLangField;
      }
      set
      {
        this.xmlLangField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlTextAttribute()]
    public string Value
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
  }

  /// <remarks/>
  [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.0.30319.225")]
  [System.Diagnostics.DebuggerStepThroughAttribute()]
  [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://wadl.dev.java.net/2009/02")]
  public partial class WADLHeaderDocumentation
  {
    private string xmlLangField;

    private string titleField;


    private string cDataField;

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string title
    {
      get
      {
        return this.titleField;
      }
      set
      {
        this.titleField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute("xml:lang")]
    public string lang
    {
      get
      {
        return this.xmlLangField;
      }
      set
      {
        this.xmlLangField = value;
      }
    }

    [System.Xml.Serialization.XmlTextAttribute()]
    public System.Xml.XmlNode[] CData
    {
      get
      {
        System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
        return new System.Xml.XmlNode[] { doc.CreateCDataSection(cDataField) };
      }
      set
      {
        if (value == null)
        {
          cDataField = null;
          return;
        }

        if (value.Length != 1)
        {
          throw new InvalidOperationException(
              String.Format(
                  "Invalid array length {0}", value.Length));
        }

        var node0 = value[0];
        var cdata = node0 as System.Xml.XmlCDataSection;
        if (cdata == null)
        {
          throw new InvalidOperationException(
              String.Format(
                  "Invalid node type {0}", node0.NodeType));
        }

        cDataField = cdata.Data;
      }
    }
  }

  /// <remarks/>
  [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.0.30319.225")]
  [System.Diagnostics.DebuggerStepThroughAttribute()]
  [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://wadl.dev.java.net/2009/02")]
  public partial class WADLParameter
  {
    private List<object> itemsField = new List<object>();

    private string nameField;

    private string styleField;

    private string typeField;

    private string defaultField;

    private bool requiredField;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("doc", typeof(WADLDocumentation))]
    [System.Xml.Serialization.XmlElementAttribute("option", typeof(WADLOption))]
    public List<object> Items
    {
      get
      {
        return this.itemsField;
      }
      set
      {
        this.itemsField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
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

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string type
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

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string style
    {
      get
      {
        return this.styleField;
      }
      set
      {
        this.styleField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public bool required
    {
      get
      {
        return this.requiredField;
      }
      set
      {
        this.requiredField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string @default
    {
      get
      {
        return this.defaultField;
      }
      set
      {
        this.defaultField = value;
      }
    }
  }
  /// <remarks/>
  [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.0.30319.225")]
  [System.Diagnostics.DebuggerStepThroughAttribute()]
  [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://wadl.dev.java.net/2009/02")]
  public partial class WADLOption
  {

    private string valueField;

    private string mediaTypeField;

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
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

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string mediaType
    {
      get
      {
        return this.mediaTypeField;
      }
      set
      {
        this.mediaTypeField = value;
      }
    }
  }
  /// <remarks/>
  [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.0.30319.225")]
  [System.Diagnostics.DebuggerStepThroughAttribute()]
  [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://wadl.dev.java.net/2009/02")]
  public partial class WADLResources
  {

    private List<object> resourceField = new List<object>();

    private string baseField;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("doc", typeof(WADLHeaderDocumentation))]
    [System.Xml.Serialization.XmlElementAttribute("resource", typeof(WADLResource))]
    public List<object> Items
    {
      get
      {
        return this.resourceField;
      }
      set
      {
        this.resourceField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string @base
    {
      get
      {
        return this.baseField;
      }
      set
      {
        this.baseField = value;
      }
    }
  }
  /// <remarks/>
  [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.0.30319.225")]
  [System.Diagnostics.DebuggerStepThroughAttribute()]
  [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://wadl.dev.java.net/2009/02")]
  public partial class WADLResource
  {
    private string pathField;

    private List<object> itemsField = new List<object>();

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("doc", typeof(WADLDocumentation))]
    [System.Xml.Serialization.XmlElementAttribute("param", typeof(WADLParameter))]
    [System.Xml.Serialization.XmlElementAttribute("method", typeof(WADLMethod))]
    [System.Xml.Serialization.XmlElementAttribute("resource", typeof(WADLResource))]
    public List<object> Items
    {
      get
      {
        return this.itemsField;
      }
      set
      {
        this.itemsField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string path
    {
      get
      {
        return this.pathField;
      }
      set
      {
        this.pathField = value;
      }
    }
  }
  /// <remarks/>
  [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.0.30319.225")]
  [System.Diagnostics.DebuggerStepThroughAttribute()]
  [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://wadl.dev.java.net/2009/02")]
  public partial class WADLMethod
  {
    private string nameField;

    private string idField;

    private List<object> itemsField = new List<object>();

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
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

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
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

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("doc", typeof(WADLDocumentation))]
    [System.Xml.Serialization.XmlElementAttribute("method", typeof(WADLMethod))]
    [System.Xml.Serialization.XmlElementAttribute("request", typeof(WADLRequest))]
    public List<object> Items
    {
      get
      {
        return this.itemsField;
      }
      set
      {
        this.itemsField = value;
      }
    }
  }

  /// <remarks/>
  [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.0.30319.225")]
  [System.Diagnostics.DebuggerStepThroughAttribute()]
  [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://wadl.dev.java.net/2009/02")]
  public partial class WADLRequest
  {
    private List<object> itemsField = new List<object>();

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("doc", typeof(WADLDocumentation))]
    [System.Xml.Serialization.XmlElementAttribute("param", typeof(WADLParameter))]
    public List<object> Items
    {
      get
      {
        return this.itemsField;
      }
      set
      {
        this.itemsField = value;
      }
    }
  }
}
