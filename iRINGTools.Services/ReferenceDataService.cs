// Copyright (c) 2010, ids-adi.org /////////////////////////////////////////////
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

using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Configuration;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using log4net;
using org.ids_adi.qmxf;
using org.iringtools.library;
using org.iringtools.refdata;
using org.iringtools.refdata.federation;

namespace org.iringtools.services
{
  [ServiceContract]
  [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
  public class ReferenceDataService
  {
    private static readonly ILog log = LogManager.GetLogger(typeof(ReferenceDataService));
    private ReferenceDataProvider _referenceDataProvider = null;

    public ReferenceDataService()
    {
      NameValueCollection settings = ConfigurationManager.AppSettings;
      _referenceDataProvider = new ReferenceDataProvider(settings);            
    }

    #region GetVersion
    /// <summary>
    /// Gets the version of the service.
    /// </summary>
    /// <returns>Returns the version as a string.</returns>
    [Description("Gets the version of the service.")]
    [WebGet(UriTemplate = "/version")]
    public VersionInfo GetVersion()
    {
      OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;
      context.ContentType = "application/xml";

      return _referenceDataProvider.GetVersion();
    }
    #endregion

    /// <summary>
    /// Finds a specific item by label and returns a list of Entity objects.
    /// </summary>
    [Description("Finds a specific item by label and returns a list of Entity objects.")]
    [WebGet(UriTemplate = "/find/{query}")]
    public List<Entity> Find(string query)
    {
      OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;
      context.ContentType = "application/xml";

      return _referenceDataProvider.Find(query);
    }

    /// <summary>
    /// Gets configured repositories.
    /// </summary>
    [Description("Gets configured repositories.")]
    [WebGet(UriTemplate = "/repositories")]
    public List<Repository> GetRepositories()
    {
      OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;
      context.ContentType = "application/xml";

      return _referenceDataProvider.GetRepositories();
    }

    /// <summary>
    /// Gets configured repositories.
    /// </summary>
    [Description("Gets Federation.")]
    [WebGet(UriTemplate = "/federation")]
    public Federation GetFederation()
    {
      OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;
      context.ContentType = "application/xml";

      return _referenceDataProvider.GetFederation();
    }

    /// <summary>
    /// Does a fuzzy search by label and returns a list of Entity objects.
    /// </summary>
    [Description("Does a fuzzy search by label and returns a list of Entity objects.")]
    [WebGet(UriTemplate = "/search/{query}")]
    public RefDataEntities Search(string query)
    {
      OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;
      context.ContentType = "application/xml";

      return _referenceDataProvider.Search(query);
    }

    /// <summary>
    /// do a search and return a specific page
    /// </summary>
    [Description("do a search and return a specific page")]
    [WebGet(UriTemplate = "/search/{query}/{start}/{limit}")]
    public RefDataEntities SearchPage(string query, string start, string limit)
    {
      int startIdx = 0;
      int pageLimit = 0;
      int.TryParse(start, out startIdx);
      int.TryParse(limit, out pageLimit);

      OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;
      context.ContentType = "application/xml";

      return _referenceDataProvider.SearchPage(query, startIdx, pageLimit);
    }

    /// <summary>
    /// do a search, ignoring cached results
    /// </summary>
    [Description("do a search, ignoring cached results")]
    [WebGet(UriTemplate = "/search/{query}/reset")]
    public RefDataEntities SearchReset(string query)
    {
      OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;
      context.ContentType = "application/xml";

      return _referenceDataProvider.SearchReset(query);
    }

    /// <summary>
    /// do a search, ignoring cached results, and return a specific page
    /// </summary>
    [Description("do a search, ignoring cached results, and return a specific page")]
    [WebGet(UriTemplate = "/search/{query}/{start}/{limit}/reset")]
    public RefDataEntities SearchPageReset(string query, string start, string limit)
    {
      int startIdx = 0;
      int pageLimit = 0;
      int.TryParse(start, out startIdx);
      int.TryParse(limit, out pageLimit);

      OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;
      context.ContentType = "application/xml";

      return _referenceDataProvider.SearchPageReset(query, startIdx, pageLimit);
    }

    /// <summary>
    /// return class label
    /// </summary>
    [Description("return class label")]
    [WebGet(UriTemplate = "/classes/{id}/label")]
    public Entity GetClassLabel(string id)
    {
      OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;
      context.ContentType = "application/xml";

      return _referenceDataProvider.GetClassLabel(id);
    }

    /// <summary>
    /// return class label
    /// </summary>
    [Description("return entity types")]
    [WebGet(UriTemplate = "/entitytypes")]
    public Entities GetEntityTypes()
    {
      OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;
      context.ContentType = "application/xml";

      return _referenceDataProvider.GetEntityTypes();
    }

       /// <summary>
    /// return class details
    /// </summary>
    //[XmlSerializerFormat]
    [Description("return class details")]
    [WebGet(UriTemplate = "/classes/{id}?namespace={namespace}")]
    public QMXF GetClass(string id, string @namespace)
    {
      OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;
      context.ContentType = "application/xml";

      return _referenceDataProvider.GetClass(id, @namespace, null);
    }
    /// <summary>
    /// return immediate superclasses of specified class
    /// </summary>
    [Description("return immediate superclasses of specified class")]
    [WebGet(UriTemplate = "/classes/{id}/superclasses")]
    public Entities GetSuperClasses(string id)
    {
      OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;
      context.ContentType = "application/xml";

      return _referenceDataProvider.GetSuperClasses(id, null);
    }

    /// <summary>
    /// return all superclasses of specified class
    /// </summary>
    [Description("return all superclasses of specified class")]
    [WebGet(UriTemplate = "/classes/{id}/allsuperclasses")]
    public Entities GetAllSuperClasses(string id)
    {
      OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;
      context.ContentType = "application/xml";

      return _referenceDataProvider.GetAllSuperClasses(id);
    }

    /// <summary>
    /// return immediate subclasses of specified class
    /// </summary>
    [Description("return immediate subclasses of specified class")]
    [WebGet(UriTemplate = "/classes/{id}/subclasses")]
    public Entities GetSubClasses(string id)
    {
      OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;
      context.ContentType = "application/xml";

 return _referenceDataProvider.GetSubClasses(id, null);
    }

    /// <summary>
    /// return immediate subclasses of specified class
    /// </summary>
    [Description("return immediate subclasses of specified class from repository")]
    [WebInvoke(UriTemplate = "/classes/{id}/subclasses")]
    public Entities GetSubClassesFromRepository(string id, Repository repository)
    {
      OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;
      context.ContentType = "application/xml";

      return _referenceDataProvider.GetSubClasses(id, repository);
    }

    /// <summary>
    /// return immediate subclasses count of specified class
    /// </summary>
    [Description("return immediate subclasses of specified class")]
    [WebGet(UriTemplate = "/classes/{id}/subclasses/count")]
    public Entities GetSubClassesCount(string id)
    {
      OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;
      context.ContentType = "application/xml";

      return _referenceDataProvider.GetSubClassesCount(id);
    }

    /// <summary>
    /// templates on a class
    /// </summary>
    [Description("templates on a class")]
    [WebGet(UriTemplate = "/classes/{id}/templates")]
    public Entities GetClassTemplates(string id)
    {
      return _referenceDataProvider.GetClassTemplates(id);
    }

    /// <summary>
    /// Count templates on a class
    /// </summary>
    [Description("Count templates on a class")]
    [WebGet(UriTemplate = "/classes/{id}/templates/count")]
    public Entities GetClassTemplatesCount(string id)
    {
      OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;
      context.ContentType = "application/xml";

      return _referenceDataProvider.GetClassTemplatesCount(id);
    }

    ///<summary>
    /// get template details
    ///</summary>
    //[XmlSerializerFormat]
    [Description("get template details")]
    [WebGet(UriTemplate = "/templates/{id}")]
    public QMXF GetTemplate(string id)
    {
      OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;
      context.ContentType = "application/xml";

      return _referenceDataProvider.GetTemplate(id);
    }

    /// <summary>
    /// Updates a Template in a repository
    /// </summary>
    //[XmlSerializerFormat]
    [Description("Updates a Template in a repository")]
    [WebInvoke(UriTemplate = "/templates")]
    public Response PostTemplate(QMXF qmxf)
    {
      OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;
      context.ContentType = "application/xml";

      return _referenceDataProvider.PostTemplate(qmxf);
    }

    ///<summary>
    /// Updates a Class in a repository
    ///</summary>
    //[XmlSerializerFormat]
    [Description("Updates a Class in a repository")]
    [WebInvoke(UriTemplate = "/classes")]
    public Response PostClass(QMXF qmxf)
    {
      OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;
      context.ContentType = "application/xml";

      return _referenceDataProvider.PostClass(qmxf);
    }

    /// <summary>
    /// members on a class
    /// </summary>
    [Description("members on a class from repository")]
    [WebInvoke(UriTemplate = "/classes/{id}/members")]
    public Entities GetClassMembersFromRepository(string id, Repository repository)
    {
      OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;
      context.ContentType = "application/xml";

      return _referenceDataProvider.GetClassMembers(id, repository);
    }

    /// <summary>
    /// members on a class
    /// </summary>
    [Description("members on a class")]
    [WebGet(UriTemplate = "/classes/{id}/members")]
    public Entities GetClassMembers(string id)
    {
      OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;
      context.ContentType = "application/xml";

      return _referenceDataProvider.GetClassMembers(id, null);
    }
  }
}
