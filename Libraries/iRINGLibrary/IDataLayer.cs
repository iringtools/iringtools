// Copyright (c) 2011, iringug.org /////////////////////////////////////////////
// All rights reserved.
//------------------------------------------------------------------------------
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//     * Redistributions of source code must retain the above copyright
//       notice, this list of conditions and the following disclaimer.
//     * Redistributions in binary form must reproduce the above copyright
//       notice, this list of conditions and the following disclaimer in the
//       documentation and/or other materials provided with the distribution.
//     * Neither the name of the iringug.org nor the
//       names of its contributors may be used to endorse or promote products
//       derived from this software without specific prior written permission.
//------------------------------------------------------------------------------
// THIS SOFTWARE IS PROVIDED BY iringug.org ''AS IS'' AND ANY
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
using System.Linq;
using System.Xml.Linq;
using System.Text;
using org.ids_adi.qxf;
using org.w3.sparql_results;
using System.Runtime.Serialization;
using System.IO;

namespace org.iringtools.library
{
  public interface IDataObject
  {    
    object GetPropertyValue(string propertyName);

    void SetPropertyValue(string propertyName, object value);
  }

  public interface IContentObject : IDataObject
  {
    string identifier { get; set; }
    string contentType { get; set; }
    Stream content { get; set; }
    string hashType { get; set; }
    string hash { get; set; }
    string url { get; set; }
  }

  public interface IDataLayer
  {
    IList<IDataObject> Create(string objectType, IList<string> identifiers);

    long GetCount(string objectType, DataFilter filter);

    IList<string> GetIdentifiers(string objectType, DataFilter filter);

    IList<IDataObject> Get(string objectType, IList<string> identifiers);

    IList<IDataObject> Get(string objectType, DataFilter filter, int pageSize, int startIndex);

    Response Post(IList<IDataObject> dataObjects);

    Response Delete(string objectType, IList<string> identifiers);

    Response Delete(string objectType, DataFilter filter);

    DataDictionary GetDictionary();

    IList<IDataObject> GetRelatedObjects(IDataObject dataObject, string relatedObjectType);

    long GetRelatedCount(IDataObject dataObject, string relatedObjectType);

    IList<IDataObject> GetRelatedObjects(IDataObject dataObject, string relatedObjectType, int pageSize, int startIndex);
  }

  public interface IDataLayer2 : IDataLayer
  {
    Response Configure(XElement configuration);

    IList<IDataObject> Search(string objectType, string query, int pageSize, int startIndex);

    IList<IDataObject> Search(string objectType, string query, DataFilter filter, int pageSize, int startIndex);

    long GetSearchCount(string objectType, string query, DataFilter filter);

    long GetSearchCount(string objectType, string query);

    XElement GetConfiguration();

    DocumentBytes GetResourceData();

    Response RefreshAll();

    Response Refresh(string objectType);

    IList<Object> GetSummary();

    Picklists GetPicklist(string name, int start, int limit);
  }

  [CollectionDataContract(Name = "authorizedUsers", Namespace = "http://www.iringtools.org/library", ItemName = "userId")]
  public class AuthorizedUsers : List<string> { }
}
