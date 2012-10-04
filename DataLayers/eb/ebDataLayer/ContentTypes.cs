using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace org.iringtools.adapter.datalayer.eb
{
  [CollectionDataContract(Namespace = "http://www.iringtools.org/library", Name = "contentTypes")]
  public class ContentTypes : List<ContentType> {}

  [DataContract(Namespace = "http://www.iringtools.org/library", Name = "contentType")]
  public class ContentType
  {
    [DataMember(Name = "extension")]
    public string Extension { get; set; }

    [DataMember(Name = "mimeType")]
    public string MimeType { get; set; }
  }
}
