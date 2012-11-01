using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using net.java.dev.wadl;

namespace com.apigee.api.wadl
{
  /// <remarks/>
  [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.0.30319.225")]
  [System.Diagnostics.DebuggerStepThroughAttribute()]
  [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://api.apigee.com/wadl/2010/07/")]
  [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://api.apigee.com/wadl/2010/07/", IsNullable = false)]
  public partial class ApigeeChoice
  {

    private WADLParameter[] paramField;

    private bool requiredField;

    private bool requiredFieldSpecified;

    private byte countMaxField;

    private bool countMaxFieldSpecified;

    private byte countMinField;

    private bool countMinFieldSpecified;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("param", Namespace = "http://wadl.dev.java.net/2009/02")]
    public WADLParameter[] param
    {
      get
      {
        return this.paramField;
      }
      set
      {
        this.paramField = value;
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
    [System.Xml.Serialization.XmlIgnoreAttribute()]
    public bool requiredSpecified
    {
      get
      {
        return this.requiredFieldSpecified;
      }
      set
      {
        this.requiredFieldSpecified = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public byte countMax
    {
      get
      {
        return this.countMaxField;
      }
      set
      {
        this.countMaxField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlIgnoreAttribute()]
    public bool countMaxSpecified
    {
      get
      {
        return this.countMaxFieldSpecified;
      }
      set
      {
        this.countMaxFieldSpecified = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public byte countMin
    {
      get
      {
        return this.countMinField;
      }
      set
      {
        this.countMinField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlIgnoreAttribute()]
    public bool countMinSpecified
    {
      get
      {
        return this.countMinFieldSpecified;
      }
      set
      {
        this.countMinFieldSpecified = value;
      }
    }
  }
  /// <remarks/>
  [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.0.30319.225")]
  [System.Diagnostics.DebuggerStepThroughAttribute()]
  [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://api.apigee.com/wadl/2010/07/")]
  [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://api.apigee.com/wadl/2010/07/", IsNullable = false)]
  public partial class ApigeeTags
  {

    private ApigeeTag[] tagField;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("tag")]
    public ApigeeTag[] Items
    {
      get
      {
        return this.tagField;
      }
      set
      {
        this.tagField = value;
      }
    }
  }
  /// <remarks/>
  [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.0.30319.225")]
  [System.Diagnostics.DebuggerStepThroughAttribute()]
  [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://api.apigee.com/wadl/2010/07/")]
  public partial class ApigeeTag
  {

    private bool primaryField;

    private bool primaryFieldSpecified;

    private string valueField;

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public bool primary
    {
      get
      {
        return this.primaryField;
      }
      set
      {
        this.primaryField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlIgnoreAttribute()]
    public bool primarySpecified
    {
      get
      {
        return this.primaryFieldSpecified;
      }
      set
      {
        this.primaryFieldSpecified = value;
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
  [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://api.apigee.com/wadl/2010/07/")]
  [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://api.apigee.com/wadl/2010/07/", IsNullable = false)]
  public partial class ApigeeAuthentication
  {

    private bool requiredField;

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
  }
  /// <remarks/>
  [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.0.30319.225")]
  [System.Diagnostics.DebuggerStepThroughAttribute()]
  [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://api.apigee.com/wadl/2010/07/")]
  [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://api.apigee.com/wadl/2010/07/", IsNullable = false)]
  public partial class ApigeeExample
  {

    private string urlField;

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string url
    {
      get
      {
        return this.urlField;
      }
      set
      {
        this.urlField = value;
      }
    }
  }
}
