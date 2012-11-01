using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using org.iringtools.library;
using org.iringtools.dxfr.manifest;
using org.iringtools.mapping;
using org.iringtools.utility;
using System.Collections.Specialized;
using Ninject;
using Ninject.Extensions.Xml;
using System.IO;
using log4net;
using Microsoft.ServiceModel.Web;
using StaticDust.Configuration;
using org.iringtools.adapter;
using VDS.RDF.Query;
using System.Net;
using org.iringtools.adapter.identity;
using System.Collections;

namespace org.iringtools.exchange
{
  public class ExchangeProvider
  {
    private static readonly XNamespace DTO_NS = "http://iringtools.org/adapter/library/dto";
    private static readonly XNamespace RDL_NS = "http://rdl.rdlfacade.org/data#";

    private static readonly ILog _logger = LogManager.GetLogger(typeof(AdapterProvider));

    private Response _response = null;
    private IKernel _kernel = null;
    private AdapterSettings _settings = null;
    private Resource _scopes = null;
    private IIdentityLayer _identityLayer = null;
    private IDictionary _keyRing = null;
    private IDataLayer _dataLayer = null;
    private IProjectionLayer _projectionEngine = null;
    private Mapping _mapping = null;
    private GraphMap _graphMap = null;

    private IList<IDataObject> _dataObjects = new List<IDataObject>();
    private Dictionary<string, List<string>> _classIdentifiers = new Dictionary<string, List<string>>();

    private bool _isScopeInitialized = false;
    private bool _isDataLayerInitialized = false;

    [Inject]
    public ExchangeProvider(NameValueCollection settings)
    {
      var ninjectSettings = new NinjectSettings { LoadExtensions = false };
      _kernel = new StandardKernel(ninjectSettings, new AdapterModule());

      _kernel.Load(new XmlExtensionModule());
      _settings = _kernel.Get<AdapterSettings>();
      _settings.AppendSettings(settings);

      Directory.SetCurrentDirectory(_settings["BaseDirectoryPath"]);

      string scopesPath = String.Format("{0}Scopes.xml", _settings["XmlPath"]);
      _settings["ScopesPath"] = scopesPath;

      if (File.Exists(scopesPath))
      {
        _scopes = Utility.Read<Resource>(scopesPath);
      }
      else
      {
        _scopes = new Resource();
        Utility.Write<Resource>(_scopes, scopesPath);
      }

      _response = new Response();
      _response.StatusList = new List<Status>();
      _kernel.Bind<Response>().ToConstant(_response);
      string relativePath = String.Format("{0}BindingConfiguration.Adapter.xml",
            _settings["XmlPath"]
          );
      string bindingConfigurationPath = Path.Combine(
        _settings["BaseDirectoryPath"],
        relativePath
      );
      _kernel.Load(bindingConfigurationPath);
      InitializeIdentity();
    }

    private void InitializeIdentity()
    {
      try
      {
        _identityLayer = _kernel.Get<IIdentityLayer>("IdentityLayer");
        _keyRing = _identityLayer.GetKeyRing();
        _kernel.Bind<IDictionary>().ToConstant(_keyRing).Named("KeyRing");

        _settings.AppendKeyRing(_keyRing);
      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Error initializing identity: {0}", ex));
        throw new Exception(string.Format("Error initializing identity: {0})", ex));
      }
    }
    public Response PullDTO(string projectName, string applicationName, string graphName, Request request)
    {
      String targetUri = String.Empty;
      String targetCredentialsXML = String.Empty;
      String filter = String.Empty;
      String projectNameForPull = String.Empty;
      String applicationNameForPull = String.Empty;
      String graphNameForPull = String.Empty;
      String dataObjectsString = String.Empty;
      Status status = new Status();
      status.Messages = new Messages();

      try
      {
        status.Identifier = String.Format("{0}.{1}", projectName, applicationName);

        InitializeScope(projectName, applicationName);
        InitializeDataLayer();

        _projectionEngine = _kernel.Get<IProjectionLayer>("dto");

        targetUri = request["targetUri"];
        targetCredentialsXML = request["targetCredentials"];
        graphNameForPull = request["targetGraphName"];
        filter = request["filter"];
        projectNameForPull = request["projectName"];
        applicationNameForPull = request["applicationName"];

        WebCredentials targetCredentials = Utility.Deserialize<WebCredentials>(targetCredentialsXML, true);
        if (targetCredentials.isEncrypted) targetCredentials.Decrypt();

        WebHttpClient httpClient = new WebHttpClient(targetUri);
        if (filter != String.Empty)
        {
          dataObjectsString = httpClient.GetMessage(@"/" + projectNameForPull + "/" + applicationNameForPull + "/" + graphNameForPull + "/" + filter + "?format=dto");
        }
        else
        {
          dataObjectsString = httpClient.GetMessage(@"/" + projectNameForPull + "/" + applicationNameForPull + "/" + graphNameForPull + "?format=dto");
        }
        XDocument xDocument = XDocument.Parse(dataObjectsString);

        IList<IDataObject> dataObjects = _projectionEngine.ToDataObjects(graphName, ref xDocument);

        _response.Append(_dataLayer.Post(dataObjects));
        status.Messages.Add(String.Format("Pull is successful from " + targetUri + "for Graph " + graphName));
      }
      catch (Exception ex)
      {
        _logger.Error("Error in PullDTO: " + ex);

        status.Level = StatusLevel.Error;
        status.Messages.Add("Error while pulling " + graphName + " data from " + targetUri + " as " + targetUri + " data with filter " + filter + ".\r\n");
        status.Messages.Add(ex.ToString());
      }

      _response.Append(status);
      return _response;
    }

    public Response Pull(string projectName, string applicationName, string graphName, Request request)
    {
      Status status = new Status();
      status.Messages = new Messages();

      try
      {
        status.Identifier = String.Format("{0}.{1}", projectName, applicationName);

        InitializeScope(projectName, applicationName);
        InitializeDataLayer();

        DateTime startTime = DateTime.Now;

       if (!request.ContainsKey("targetEndpointUri"))
          throw new Exception("Target Endpoint Uri is required");

        string targetEndpointUri = request["targetEndpointUri"];

        if (!request.ContainsKey("targetGraphBaseUri"))
          throw new Exception("Target graph uri is required");

        string targetGraphBaseUri = request["targetGraphBaseUri"];
        _settings["TargetGraphBaseUri"] = targetGraphBaseUri;

        SparqlRemoteEndpoint endpoint = new SparqlRemoteEndpoint(new Uri(targetEndpointUri), targetGraphBaseUri);

        if (request.ContainsKey("targetCredentials"))
        {
          string targetCredentialsXML = request["targetCredentials"];
          WebCredentials targetCredentials = Utility.Deserialize<WebCredentials>(targetCredentialsXML, true);

          if (targetCredentials.isEncrypted)
            targetCredentials.Decrypt();

          endpoint.SetCredentials(
              targetCredentials.GetNetworkCredential().UserName, 
              targetCredentials.GetNetworkCredential().Password, 
              targetCredentials.GetNetworkCredential().Domain);
        }

        string proxyHost = _settings["ProxyHost"];
        string proxyPort = _settings["ProxyPort"];
        if (!String.IsNullOrEmpty(proxyHost) && !String.IsNullOrEmpty(proxyPort))
        {
          WebProxyCredentials proxyCrendentials = _settings.GetWebProxyCredentials();
          endpoint.Proxy = proxyCrendentials.GetWebProxy() as WebProxy;
          endpoint.ProxyCredentials = proxyCrendentials.GetNetworkCredential();
        }

        String query = "CONSTRUCT {?s ?p ?o} WHERE {?s ?p ?o}";
        VDS.RDF.IGraph graph = endpoint.QueryWithResultGraph(query);

        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        TextWriter textWriter = new StringWriter(sb);
        VDS.RDF.Writing.RdfXmlWriter rdfWriter = new VDS.RDF.Writing.RdfXmlWriter();
        rdfWriter.Save(graph, textWriter);
        XDocument xDocument = XDocument.Parse(sb.ToString());

        // call RdfProjectionEngine to fill data objects from a given graph
        _projectionEngine = _kernel.Get<IProjectionLayer>("rdf");
        _dataObjects = _projectionEngine.ToDataObjects(graphName, ref xDocument);

        // post data objects to data layer
        _dataLayer.Post(_dataObjects);

        DateTime endTime = DateTime.Now;
        TimeSpan duration = endTime.Subtract(startTime);

        status.Messages.Add(string.Format("Graph [{0}] has been posted to legacy system successfully.", graphName));

        status.Messages.Add(String.Format("Execution time [{0}:{1}.{2}] minutes.",
          duration.Minutes, duration.Seconds, duration.Milliseconds));
      }
      catch (Exception ex)
      {
        _logger.Error("Error in Pull(): ", ex);

        status.Level = StatusLevel.Error;
        status.Messages.Add(string.Format("Error pulling graph: {0}", ex));
      }

      _response.Append(status);
      return _response;
    }

    //Gets from datalayer and send it to another endpoint
    public Response Push(string projectName, string applicationName, string graphName, PushRequest request)
    {
      Status status = new Status();
      status.Messages = new Messages();

      try
      {
        String targetUri = String.Empty;
        String targetCredentialsXML = String.Empty;
        String filter = String.Empty;
        String projectNameForPush = String.Empty;
        String applicationNameForPush = String.Empty;
        String graphNameForPush = String.Empty;
        String format = String.Empty;
        targetUri = request["targetUri"];
        targetCredentialsXML = request["targetCredentials"];
        filter = request["filter"];
        projectNameForPush = request["targetProjectName"];
        applicationNameForPush = request["targetApplicationName"];
        graphNameForPush = request["targetGraphName"];
        format = request["format"];

        WebHttpClient httpClient = new WebHttpClient(targetUri);

        InitializeScope(projectName, applicationName);
        InitializeDataLayer();

        _graphMap = _mapping.FindGraphMap(graphName);

        _projectionEngine = _kernel.Get<IProjectionLayer>(format);
        IList<IDataObject> dataObjectList;
        if (filter != String.Empty)
        {
          IList<string> identifiers = new List<string>();
          identifiers.Add(filter);
          dataObjectList = _dataLayer.Get(_graphMap.dataObjectName, identifiers);
        }
        else
        {
          dataObjectList = _dataLayer.Get(_graphMap.dataObjectName, null);
        }

        XDocument xDocument = _projectionEngine.ToXml(graphName, ref dataObjectList);

        _isDataLayerInitialized = false;
        _isScopeInitialized = false;
        Response localResponse = httpClient.Post<XDocument, Response>(@"/" + projectNameForPush + "/" + applicationNameForPush + "/" + graphNameForPush + "?format=" + format, xDocument, true);

        _response.Append(localResponse);

        if (request.ExpectedResults != null)
        {
          foreach (Status responseStatus in localResponse.StatusList)
          {
            string dataObjectName = request.ExpectedResults.DataObjectName;

            IList<IDataObject> dataObjects = _dataLayer.Get(
              dataObjectName, new List<string> { responseStatus.Identifier });

            foreach (var resultMap in request.ExpectedResults)
            {
              string propertyValue = responseStatus.Results[resultMap.Key];
              string dataPropertyName = resultMap.Value;

              dataObjects[0].SetPropertyValue(dataPropertyName, propertyValue);
            }

            _response.Append(_dataLayer.Post(dataObjects));
          }
        }
      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Error in pushing data", ex));

        status.Level = StatusLevel.Error;
        status.Messages.Add(string.Format("Error in pushing data: {0}", ex));
      }

      return _response;
    }

    #region helper methods

    private void getResource()
    {
      WebHttpClient _javaCoreClient = new WebHttpClient(_settings["JavaCoreUri"]);
      System.Uri uri = new System.Uri(_settings["GraphBaseUri"]);
      string baseUrl = uri.Scheme + ":.." + uri.Host + ":" + uri.Port;
      _scopes = _javaCoreClient.PostMessage<Resource>("/directory/resource", baseUrl, true);
    }

    private void InitializeScope(string projectName, string applicationName)
    {
      try
      {
        if (_scopes.Locators == null)
          getResource();

        if (!_isScopeInitialized)
        {
          bool isScopeValid = false;

          foreach (Locator project in _scopes.Locators)
          {
            if (project.Context.ToUpper() == projectName.ToUpper())
            {
              foreach (EndpointApplication application in project.Applications)
              {
                if (application.Endpoint.ToUpper() == applicationName.ToUpper())
                  isScopeValid = true;
              }
            }
          }

          string scope = String.Format("{0}.{1}", projectName, applicationName);

          if (!isScopeValid) throw new Exception(String.Format("Invalid scope [{0}].", scope));

          _settings["ProjectName"] =  projectName;
          _settings["ApplicationName"] = applicationName;
          _settings["Scope"] = scope;

          string appSettingsPath = String.Format("{0}{1}.config",
            _settings["XmlPath"],
            scope
          );

          if (File.Exists(appSettingsPath))
          {
            AppSettingsReader appSettings = new AppSettingsReader(appSettingsPath);
            _settings.AppendSettings(appSettings);
          }
          string relativePath = String.Format("{0}BindingConfiguration.{1}.xml",
            _settings["XmlPath"],
            scope
          );

          //Ninject Extension requires fully qualified path.
          string bindingConfigurationPath = Path.Combine(
            _settings["BaseDirectoryPath"],
            relativePath
          );

          _settings["BindingConfigurationPath"] = bindingConfigurationPath;

          if (!File.Exists(bindingConfigurationPath))
          {
            XElement binding = new XElement("module",
              new XAttribute("name", _settings["Scope"]),
              new XElement("bind",
                new XAttribute("name", "DataLayer"),
                new XAttribute("service", "org.iringtools.library.IDataLayer, iRINGLibrary"),
                new XAttribute("to", "org.iringtools.adapter.datalayer.NHibernateDataLayer, NHibernateDataLayer")
              )
            );

            binding.Save(bindingConfigurationPath);
          }

          _kernel.Load(bindingConfigurationPath);

          string mappingPath = String.Format("{0}Mapping.{1}.xml",
            _settings["XmlPath"],
            scope
          );

          if (File.Exists(mappingPath))
          {
            _mapping = Utility.Read<Mapping>(mappingPath);
          }
          else
          {
            _mapping = new Mapping();
            Utility.Write<Mapping>(_mapping, mappingPath);
          }
          _kernel.Bind<Mapping>().ToConstant(_mapping);

          _isScopeInitialized = true;
        }
      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Error initializing application: {0}", ex));
        throw new Exception(string.Format("Error initializing application: {0})", ex));
      }
    }

    private void InitializeDataLayer()
    {
      try
      {
        if (!_isDataLayerInitialized)
        {
          _dataLayer = _kernel.Get<IDataLayer>("DataLayer");

          //_dataDictionary = _dataLayer.GetDictionary();
          //_kernel.Bind<DataDictionary>().ToConstant(_dataDictionary);

          _isDataLayerInitialized = true;
        }
      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Error initializing application: {0}", ex));
        throw new Exception(string.Format("Error initializing application: {0})", ex));
      }
    }

    private void PopulateClassIdentifiers(List<string> identifiers)
    {
      _classIdentifiers.Clear();

      foreach (ClassTemplateMap classTemplateMap in _graphMap.classTemplateMaps)
      {
        ClassMap classMap = classTemplateMap.classMap;

        List<string> classIdentifiers = new List<string>();

        foreach (string identifier in classMap.identifiers)
        {
          // identifier is a fixed value
          if (identifier.StartsWith("#") && identifier.EndsWith("#"))
          {
            string value = identifier.Substring(1, identifier.Length - 2);

            for (int i = 0; i < _dataObjects.Count; i++)
            {
              if (classIdentifiers.Count == i)
              {
                classIdentifiers.Add(value);
              }
              else
              {
                classIdentifiers[i] += classMap.identifierDelimiter + value;
              }
            }
          }
          else  // identifier comes from a property
          {
            string[] property = identifier.Split('.');
            string objectName = property[0].Trim();
            string propertyName = property[1].Trim();

            if (_dataObjects != null)
            {
              for (int i = 0; i < _dataObjects.Count; i++)
              {
                string value = Convert.ToString(_dataObjects[i].GetPropertyValue(propertyName));

                if (classIdentifiers.Count == i)
                {
                  classIdentifiers.Add(value);
                }
                else
                {
                  classIdentifiers[i] += classMap.identifierDelimiter + value;
                }
              }
            }
          }
        }

        _classIdentifiers[classMap.id] = classIdentifiers;
      }
    }

    #endregion
  }
}
