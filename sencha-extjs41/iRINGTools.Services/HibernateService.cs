using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Web.Configuration;
using log4net;
using org.iringtools.nhibernate;
using org.iringtools.library;
using System.ComponentModel;
using org.iringtools.utility;
using System.Web;
using System.Net;

namespace org.iringtools.services
{
  [ServiceContract]
  [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
  public class HibernateService
  {
    private static readonly ILog _logger = LogManager.GetLogger(typeof(HibernateService));
    private NHibernateProvider _NHibernateProvider = null;

    public HibernateService()
    {
      _NHibernateProvider = new NHibernateProvider(WebConfigurationManager.AppSettings);
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
	  
	  VersionInfo version = new  VersionInfo();
      
      Type type = typeof(NHibernateProvider);
      version.Major = type.Assembly.GetName().Version.Major;
      version.Minor = type.Assembly.GetName().Version.Minor;
      version.Build = type.Assembly.GetName().Version.Build;
      version.Revision = type.Assembly.GetName().Version.Revision;

      return version;
    }
    #endregion

    [WebInvoke(Method = "POST", UriTemplate = "/{scope}/{application}/dictionary")]
    public Response PostDictionary(string scope, string application, DatabaseDictionary dictionary)
    {
      OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;
      context.ContentType = "application/xml";
      return _NHibernateProvider.PostDictionary(scope, application, dictionary);
    }

    [WebGet(UriTemplate = "/{scope}/{application}/generate")]
    public Response Generate(string scope, string application)
    {
      OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;
      context.ContentType = "application/xml";
      return _NHibernateProvider.Generate(scope, application);
    }

    [WebGet(UriTemplate = "/relationships")]
    public DataRelationships GetRelationships()
    {
      OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;
      context.ContentType = "application/xml";
      return _NHibernateProvider.GetRelationships();
    }

    [WebGet(UriTemplate = "/{scope}/{application}/objects")]
    public DataObjects GetSchemaObjects(string scope, string application)
    {
      OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;
      context.ContentType = "application/xml";
      return _NHibernateProvider.GetSchemaObjects(scope, application);
    }

    [WebGet(UriTemplate = "/{scope}/{application}/objects/{objectName}")]
    public DataObject GetSchemaObjectSchema(string scope, string application, string objectName)
    {
      OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;
      context.ContentType = "application/xml";
      return _NHibernateProvider.GetSchemaObjectSchema(scope, application, objectName);
    }
    
    #region NHibernate Config support
    [WebGet(UriTemplate = "/providers")]
    public DataProviders GetProviders()
    {
      OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;
      context.ContentType = "application/xml";
      return _NHibernateProvider.GetProviders();
    }

    [WebGet(UriTemplate = "/{scope}/{application}/dictionary")]
    public DatabaseDictionary GetDictionary(string scope, string application)
    {
      OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;
      context.ContentType = "application/xml";
      return _NHibernateProvider.GetDictionary(scope, application);
    }

    [WebInvoke(Method = "POST", UriTemplate = "/{scope}/{application}/tables")]
		public List<string> GetTableNames(string scope, string application, Request request)
    {
      OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;
      context.ContentType = "application/xml";

			return _NHibernateProvider.GetTableNames(scope, application, request["dbProvider"], request["dbServer"], request["portNumber"], request["dbInstance"],
				request["dbName"], request["dbSchema"], request["dbUserName"], request["dbPassword"], request["serName"]);
    }

    [WebInvoke(Method = "POST", UriTemplate = "/{scope}/{application}/objects")]
    public List<DataObject> GetDBObjects(string scope, string application, Request request)
    {
      OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;
      context.ContentType = "application/xml";

			return _NHibernateProvider.GetDBObjects(scope, application, request["dbProvider"], request["dbServer"], request["portNumber"], request["dbInstance"],
				request["dbName"], request["dbSchema"], request["dbUserName"], request["dbPassword"], request["tableNames"], request["serName"]);
    }
    #endregion
  }
}
