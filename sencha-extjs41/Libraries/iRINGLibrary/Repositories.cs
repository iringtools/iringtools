using System.Collections.Generic;
using System.Runtime.Serialization;
using org.w3.sparql_results;
using System;

namespace org.iringtools.library
{
  [CollectionDataContract(Namespace = "http://www.iringtools.org/library", Name = "repositories", ItemName = "repository")]
  public class Repositories : List<Repository>
  {
  }

  [DataContract(Namespace = "http://www.iringtools.org/library", Name = "repository")]
  public class Repository
  {
    [DataMember(Name="name")]
    public string Name { get; set; }

    [DataMember(Name="uri")]
    public string Uri { get; set; }

    [DataMember(Name="updateUri", EmitDefaultValue=false)]
    public string UpdateUri { get; set; }

    [DataMember(Name = "description")]
    public string Description { get; set; }

    [DataMember(Name = "encryptedCredentials", EmitDefaultValue = false)]
    public string EncryptedCredentials { get; set; }

    [DataMember(Name = "isReadOnly")]
    public bool IsReadOnly { get; set; }

    [DataMember(Name = "repositoryType")]
    public RepositoryType RepositoryType { get; set; }

  }

  [DataContract(Namespace = "http://www.iringtools.org/library", Name = "repositoryType")]
  public enum RepositoryType
  {
    [EnumMember]
    RDSWIP,
    [EnumMember]
    Camelot,
    [EnumMember]
    Part8,
  }
}
