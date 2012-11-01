using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using org.iringtools.library;
using System.Runtime.Serialization;

namespace org.iringtools.library
{
  [CollectionDataContract]
  public class PushRequest : Request
  {
    [DataMember]
    public ExpectedResults ExpectedResults { get; set; }
  }

  [CollectionDataContract]
  public class ExpectedResults : Dictionary<string, string>
  {
    [DataMember]
    public string DataObjectName { get; set; }
  }
}