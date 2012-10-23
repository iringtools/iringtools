using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Xml.Linq;

namespace org.iringtools.library
{
  [DataContract(Name = "datalayerconfig", Namespace = "http://www.iringtools.org/library")]
  public class DataLayerConfig
  {
    [DataMember(Name = "datalayername", Order = 0)]
    public string DataLayerName { get; set; }

    [DataMember(Name = "datalayerconfiguration", Order = 1)]
    public XElement DataLayerConfiguration { get; set; }
  }
}
