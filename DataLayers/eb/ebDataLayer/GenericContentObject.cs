using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using org.iringtools.library;
using System.IO;
using System.Runtime.Serialization;

namespace org.iringtools.adapter.datalayer.eb
{
  public class GenericContentObject : GenericDataObject, IContentObject
  {
    [IgnoreDataMember]
    public Stream content { get; set; }
    public string contentType { get; set; }
    public string hash { get; set; }
    public string hashType { get; set; }
    public string identifier { get; set; }
    public string url { get; set; }
    public string name { get; set; }
    public string revision { get; set; }
  }
}
