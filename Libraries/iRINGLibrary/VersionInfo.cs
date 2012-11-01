using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace org.iringtools.library
{
  [DataContract(Namespace = "http://www.iringtools.org/library", Name = "version")]
  public class VersionInfo
  {
    [DataMember(Name = "major", Order = 0)]
    public int Major { get; set; }

    [DataMember(Name = "minor", Order = 1)]
    public int Minor { get; set; }

    [DataMember(Name = "build", Order = 2)]
    public int Build { get; set; }

    [DataMember(Name = "revision", Order = 3)]
    public int Revision { get; set; }
  }
}
