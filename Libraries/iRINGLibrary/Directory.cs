using System.Collections.Generic;
using System.Runtime.Serialization;
using System;
using System.Collections.ObjectModel;
using System.Xml.Serialization;
using System.Text;

namespace org.iringtools.library
{
  [CollectionDataContract(Namespace = "http://www.iringtools.org/directory", Name = "directory")]
  public class Directories : List<Folder>
  {    
  }

  [CollectionDataContract(Namespace = "http://www.iringtools.org/directory", Name = "folders")]
  public class Folders : List<Folder>
  {
  }

  [CollectionDataContract(Namespace = "http://www.iringtools.org/directory", Name = "endpoints")]
  public class Endpoints : List<Endpoint>
  {
  }

  [DataContract(Namespace = "http://www.iringtools.org/directory", Name = "folder")]
  public class Folder
  {
    [DataMember(Name = "endpoints", Order = 0, EmitDefaultValue = false)]
    public Endpoints Endpoints { get; set; }

    [DataMember(Name = "folders", Order = 1, EmitDefaultValue = false)]
    public Folders Folders { get; set; }

    [DataMember(Name = "name", Order = 2)]
    public string Name { get; set; }

    [DataMember(Name = "type", Order = 3)]
    public string Type { get; set; }

    [DataMember(Name = "description", Order = 4, EmitDefaultValue = false)]
    public string Description { get; set; }

    [DataMember(Name = "context", Order = 5, EmitDefaultValue = false)]
    public string Context { get; set; }

    [DataMember(Name = "securityRole", Order = 6, EmitDefaultValue = false)]
    public string SecurityRole { get; set; }

    [DataMember(Name = "user", Order = 7, EmitDefaultValue = false)]
    public string User { get; set; }
  }

  [DataContract(Namespace = "http://www.iringtools.org/directory", Name = "endpoint")]
  public class Endpoint
  {
    [DataMember(Name = "name", Order = 0)]
    public string Name { get; set; }   

    [DataMember(Name = "type", Order = 1, EmitDefaultValue = false)]
    public string Type { get; set; }

    [DataMember(Name = "description", Order = 2, EmitDefaultValue = false)]
    public string Description { get; set; }

    [DataMember(Name = "context", Order = 3, EmitDefaultValue = false)]
    public string Context { get; set; }

    [DataMember(Name = "baseUrl", Order = 4, EmitDefaultValue = false)]
    public string BaseUrl { get; set; }

    [DataMember(Name = "securityRole", Order = 5, EmitDefaultValue = false)]
    public string SecurityRole { get; set; }

    [DataMember(Name = "user", Order = 6, EmitDefaultValue = false)]
    public string User { get; set; }
  }

  [DataContract(Namespace = "http://www.iringtools.org/directory")]
  public enum NodeIconCls
  {
    [EnumMember]
    @folder,
    [EnumMember]
    @project,
    [EnumMember]
    @application,
    [EnumMember]
    @resource,
    [EnumMember]
    @scope,
    [EnumMember]
    @key,
    [EnumMember]
    @property,
    [EnumMember]
    @relation
  }

  [CollectionDataContract(Namespace = "http://www.iringtools.org/directory", Name = "locators")]
  public class Locators : List<Locator>
  {
  }

  [CollectionDataContract(Namespace = "http://www.iringtools.org/directory", Name = "applications")]
  public class EndpointApplications : List<EndpointApplication>
  {
  }

  [CollectionDataContract(Name = "resources", Namespace = "http://www.iringtools.org/directory", ItemName = "resource")]
  public class Resources : List<Resource>
  {
  }  
  
  [DataContract(Namespace = "http://www.iringtools.org/directory", Name = "resource")]
  public class Resource
  {
    [DataMember(Name = "baseUrl", Order = 0)]
    public string BaseUrl { get; set; }

    [DataMember(Name = "locators", Order = 1, EmitDefaultValue = false)]
    public Locators Locators { get; set; }
  }

  [DataContract(Namespace = "http://www.iringtools.org/directory", Name = "locator")]
  public class Locator
  {
    [DataMember(Name = "context", Order = 0)]
    public string Context { get; set; }

    [DataMember(Name = "description", Order = 1, EmitDefaultValue = false)]
    public string Description { get; set; }

    [DataMember(Name = "applications", Order = 2, EmitDefaultValue = false)]
    public EndpointApplications Applications { get; set; }
  }

  [DataContract(Namespace = "http://www.iringtools.org/directory", Name = "application")]
  public class EndpointApplication
  {
    [DataMember(Name = "endpoint", Order = 0)]
    public string Endpoint { get; set; }

    [DataMember(Name = "description", Order = 1, EmitDefaultValue = false)]
    public string Description { get; set; }

    [DataMember(Name = "assembly", Order = 2, EmitDefaultValue = false)]
    public string Assembly { get; set; }

    [DataMember(Name = "lpath", Order = 3, EmitDefaultValue = false)]
    public string Path { get; set; }  
  }

  [DataContract(Namespace = "http://www.iringtools.org/directory", Name = "context")]
  public class ContextName
  {
    [DataMember(Name = "context", Order = 1, EmitDefaultValue = false)]
    public string Context { get; set; }
  }

  [CollectionDataContract(Name = "contexts", Namespace = "http://www.iringtools.org/directory", ItemName = "context")]
  public class ContextNames : List<ContextName>
  {
  }

  [DataContract(Namespace = "http://www.iringtools.org/directory", Name = "baseUrl")]
  public class Url
  {    
    [DataMember(Name = "url", Order = 1, EmitDefaultValue = false)]
    public string Urlocator { get; set; }
  }

  [CollectionDataContract(Name = "baseUrls", Namespace = "http://www.iringtools.org/directory", ItemName = "baseUrl")]
  public class Urls : List<Url>
  {
  }
}
