using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using org.iringtools.library;
using org.iringtools.adapter;
using org.iringtools.dxfr.manifest;

namespace org.iringtools.adapter
{
  [DataContract(Namespace = "http://www.iringtools.org/dxfr/request", Name = "dxiRequest")]
  public class DxiRequest
  {
    [DataMember(Name = "manifest", Order = 0)]
    public Manifest Manifest { get; set; }

    [DataMember(Name = "dataFilter", Order = 1)]
    public DataFilter DataFilter { get; set; }
  }
}
