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

namespace org.w3.sparql_results
{
  [XmlRoot(ElementName = "sparql", Namespace = "http://www.w3.org/2005/sparql-results#")]
  public class SPARQLResults
  {
    public SPARQLResults()
    {
      this.head = new Head();

      this.resultsElement = new Results();
    }

    [XmlElement(ElementName = "head")]
    public Head head { get; set; }

    [XmlElement(ElementName = "results")]
    public Results resultsElement { get; set; }
  }

  public class Head
  {
    public Head()
    {
      this.variables = new List<Variable>();
    }

    [XmlElement(ElementName = "variable")]
    public List<Variable> variables;
  }

  public class Variable  {
    [XmlAttribute]
    public string name { get; set; }
  }

  public class Results
  { 
    public Results()
    {
      this.results = new List<SPARQLResult>();
    }

    [XmlElement(ElementName = "result")]
    public List<SPARQLResult> results;
  }


  public class SPARQLResult
  {
    public SPARQLResult()
    {
      this.bindings = new List<SPARQLBinding>();
    }

    [XmlElement(ElementName = "binding")]
    public List<SPARQLBinding> bindings;
  }

  public class SPARQLBinding
  {
    [XmlAttribute]
    public string name { get; set; }

    [XmlElement]
    public string bnode { get; set; }

    [XmlElement]
    public string uri { get; set; }

    [XmlElement(ElementName = "literal")]
    public SPARQLLiteral literal { get; set; }
  }

  [DataContract]
  public enum SPARQLBindingType
  {
    [EnumMember]
    Uri,
    [EnumMember]
    Literal,
  }

  public class SPARQLLiteral
  {
    [XmlAttribute(Namespace = "http://www.w3.org/XML/1998/namespace")]
    public string lang { get; set; }

    [XmlAttribute]
    public string datatype { get; set; }

    [XmlText]
    public string value { get; set; }
  }
}
