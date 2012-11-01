// Copyright (c) 2009, ids-adi.org /////////////////////////////////////////////
// All rights reserved.
//------------------------------------------------------------------------------
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//     * Redistributions of source code must retain the above copyright
//       notice, this list of conditions and the following disclaimer.
//     * Redistributions in binary form must reproduce the above copyright
//       notice, this list of conditions and the following disclaimer in the
//       documentation and/or other materials provided with the distribution.
//     * Neither the name of the ids-adi.org nor the
//       names of its contributors may be used to endorse or promote products
//       derived from this software without specific prior written permission.
//------------------------------------------------------------------------------
// THIS SOFTWARE IS PROVIDED BY ids-adi.org ''AS IS'' AND ANY
// EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL ids-adi.org BE LIABLE FOR ANY
// DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
// LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
// ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
// SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using System.ServiceModel;
using System.Linq;
using org.iringtools.library;

namespace org.iringtools.adapter
{
  [DataContract(Namespace = "http://www.iringtools.org/dxfr/dti", Name = "dataTransferIndices")]
  public class DataTransferIndices  
  {
    public DataTransferIndices()
    {
      DataTransferIndexList = new List<DataTransferIndex>();
    }

    [DataMember(Name = "scopeName", Order = 0, EmitDefaultValue = false)]
    public string ScopeName { get; set; }

    [DataMember(Name = "appName", Order = 1, EmitDefaultValue = false)]
    public string AppName { get; set; }

    [DataMember(Name = "dataTransferIndexList",  Order = 2)]
    public List<DataTransferIndex> DataTransferIndexList { get; set; }

    [DataMember(Name = "sortType", Order = 3, EmitDefaultValue = false)]
    public string SortType { get; set; }

    [DataMember(Name = "sortOrder", Order = 4, EmitDefaultValue = false)]
    public string SortOrder { get; set; }
  }

  [DataContract(Namespace = "http://www.iringtools.org/dxfr/dti", Name = "dataTransferIndex")]
  public class DataTransferIndex
  {
    [DataMember(Name = "identifier", Order = 0, EmitDefaultValue = false)]
    public string Identifier { get; set; }

    [DataMember(Name = "hashValue", Order = 1, EmitDefaultValue = false)]
    public string HashValue { get; set; }

    [DataMember(Name = "transferType", Order = 2, EmitDefaultValue = false)]
    public TransferType TransferType { get; set; }

    [DataMember(Name = "sortIndex", Order = 3, EmitDefaultValue = false)]
    public string SortIndex { get; set; }

    [DataMember(Name = "internalIdentifier", Order = 4, EmitDefaultValue = false)]
    public string InternalIdentifier { get; set; }
  }
}
