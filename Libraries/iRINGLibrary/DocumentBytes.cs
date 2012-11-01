using System.Collections.Generic;
using System;
using System.Runtime.Serialization;
using System.ComponentModel;

namespace org.iringtools.library
{
	[DataContract(Name = "documentbytes", Namespace = "http://www.iringtools.org/library")]
	public class DocumentBytes
	{
		[DataMember(Name = "content", IsRequired = true, Order = 0)]
		public byte[] Content { get; set; }

    [DataMember(Name = "path", IsRequired = true, Order = 1)]
    public string DocumentPath { get; set; }
	}

}