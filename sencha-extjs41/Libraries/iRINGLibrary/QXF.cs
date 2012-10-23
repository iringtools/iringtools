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
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using System.Text;

namespace org.ids_adi.qxf
{
  [XmlRoot(ElementName="qxf", Namespace="http://ns.ids-adi.org/qxf/schema#")]
  public class QXF
  {
      public QXF()
    {
      this.Relationships = new List<Relationship>();
    }

    [XmlElement(ElementName = "relationship")]
    public List<Relationship> Relationships { get; set; }
  }

  public class Relationship
  {
    public Relationship()
    {
      this.Properties = new List<Property>();
    }

    [XmlElement(ElementName = "property")]
    public List<Property> Properties { get; set; }

    [XmlAttribute(AttributeName = "id")]
    public string identifier { get; set; }

    [XmlAttribute(AttributeName="instance-of")]
    public string instanceOf { get; set; }
  }
  
  public class Property
  {
    [XmlAttribute(AttributeName = "instance-of")]
    public string instanceOf { get; set; }

    [XmlAttribute]
    public string reference { get; set; }

    [XmlAttribute]
    public string lang { get; set; }

    [XmlAttribute(AttributeName="as")]
    public string asType { get; set; }

    [XmlText]
    public string value { get; set; }
  }
}
