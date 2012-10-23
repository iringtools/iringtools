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
using System.ComponentModel;

namespace org.iringtools.library
{
  [DataContract(Name = "databaseDictionary", Namespace = "http://www.iringtools.org/library")]
  public class DatabaseDictionary : DataDictionary
  {
    [DataMember(Name = "provider", IsRequired = true, Order = 0)]
    public string Provider { get; set; }

    [DataMember(Name = "connectionString", IsRequired = true, Order = 1)]
    public string ConnectionString { get; set; }

    [DataMember(Name = "schemaName", IsRequired = true, Order = 2)]
    public string SchemaName { get; set; }

    [DataMember(EmitDefaultValue = false, Order = 3)]
    public IdentityConfiguration IdentityConfiguration { get; set; }
  }

  [CollectionDataContract(Namespace = "http://www.iringtools.org/library", ItemName = "objectConfiguration",
    KeyName = "objectName", ValueName = "identityProperties")]
  public class IdentityConfiguration : Dictionary<string, IdentityProperties>
  {}
  [DataContract(Name = "identityProperties", Namespace = "http://www.iringtools.org/library")]
  public class IdentityProperties
  {
    [DataMember(Name = "useIdentityFilter", IsRequired = true, Order = 0)]
    public bool UseIdentityFilter { get; set; }

    [DataMember(Name = "identityProperty", IsRequired = true, Order = 1)]
    public string IdentityProperty { get; set; }

    [DataMember(Name = "keyRingProperty", IsRequired = true, Order = 2)]
    public string KeyRingProperty { get; set; }

    [DataMember(Name = "isCaseSensitive", Order = 3, EmitDefaultValue = false)]
    public bool IsCaseSensitive { get; set; }
  }
  [DataContract(Namespace = "http://www.iringtools.org/library")]
  public enum Provider
  {
    [EnumMember]
    MsSql2000,
    [EnumMember]
    MsSql2005,
    [EnumMember]
    MsSql2008,
    [EnumMember]
    MySql3,
    [EnumMember]
    MySql4,
    [EnumMember]
    MySql5,
    [EnumMember]
    Oracle8i,
    [EnumMember]
    Oracle9i,
    [EnumMember]
    Oracle10g,
    [EnumMember]
    OracleLite,
    [EnumMember]
    PostgresSql81,
    [EnumMember]
    PostgresSql82,
    [EnumMember]
    SqLite
  }
}