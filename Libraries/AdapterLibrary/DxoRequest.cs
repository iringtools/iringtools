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
  [DataContract(Namespace = "http://www.iringtools.org/dxfr/request", Name = "dxoRequest")]
  public class DxoRequest
  {
    [DataMember(Name = "manifest", Order = 0)]
    public Manifest Manifest { get; set; }

    [DataMember(Name = "dataTransferIndices", Order = 1)]
    public DataTransferIndices DataTransferIndices { get; set; }
  }
}
