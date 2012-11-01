using System.Collections.Generic;
using System.Runtime.Serialization;
using org.w3.sparql_results;
using System;

namespace org.iringtools.refdata
{
  [CollectionDataContract(Namespace = "http://www.iringtools.org/refdata/queries", Name = "queries", ItemName = "queryItem")]
  public class Queries : List<QueryItem>
  { }

  [DataContract(Namespace = "http://www.iringtools.org/refdata/queries", Name = "queryItem")]
  public class QueryItem
  {
    [DataMember(Name = "key")]
    public string Key { get; set; }

    [DataMember(Name = "query")]
    public Query Query { get; set; }
  }

  [DataContract(Namespace = "http://www.iringtools.org/refdata/queries", Name = "query")]
  public class Query
  {
    [DataMember(Name="fileName")]
    public string FileName { get; set; }

    [DataMember(Name="bindings")]
    public QueryBindings Bindings { get; set; }
  }

  [CollectionDataContract(Namespace = "http://www.iringtools.org/refdata/queries", Name = "queryBindings", ItemName = "queryBinding")]
  public class QueryBindings : List<QueryBinding>
  { }

  [DataContract(Namespace = "http://www.iringtools.org/refdata/queries", Name = "queryBinding")]
  public class QueryBinding
  {
    [DataMember(Name="name")]
    public string Name { get; set; }

    [DataMember(Name="type")]
    public SPARQLBindingType Type { get; set; }
  }
}
