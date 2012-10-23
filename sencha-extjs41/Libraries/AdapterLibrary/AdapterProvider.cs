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
// DIRECT, INDIRECT, INCIDENTAL, SPECIAL + exEMPLARY, OR CONSEQUENTIAL DAMAGES
// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
// LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
// ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
// SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.ServiceModel.Web;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using log4net;
using net.java.dev.wadl;
using Ninject;
using Ninject.Extensions.Xml;
using org.iringtools.adapter.datalayer;
using org.iringtools.adapter.identity;
using org.iringtools.adapter.projection;
using org.iringtools.library;
using org.iringtools.mapping;
using org.iringtools.nhibernate;
using org.iringtools.utility;
using StaticDust.Configuration;
using System.Text;

namespace org.iringtools.adapter
{
  public class AdapterProvider : BaseProvider
  {
    private static readonly ILog _logger = LogManager.GetLogger(typeof(AdapterProvider));
    private static readonly int DEFAULT_PAGE_SIZE = 25;

    private IKernel _kernel = null;
    private AdapterSettings _settings = null;
    private Resource _scopes = null;
    private EndpointApplication _application = null;
    private IDataLayer2 _dataLayer = null;
    private IIdentityLayer _identityLayer = null;
    private IDictionary _keyRing = null;
    private ISemanticLayer _semanticEngine = null;
    private IProjectionLayer _projectionEngine = null;
    private DataDictionary _dataDictionary = null;
    private mapping.Mapping _mapping = null;
    private mapping.GraphMap _graphMap = null;
    private DataObject _dataObjDef = null;
    private bool _isResourceGraph = false;
    private bool _isProjectionPart7 = false;
    private bool _isFormatExpected = true;

    //Projection specific stuff
    private IList<IDataObject> _dataObjects = new List<IDataObject>(); // dictionary of object names and list of data objects
    private Dictionary<string, List<string>> _classIdentifiers = new Dictionary<string, List<string>>(); // dictionary of class ids and list of identifiers

    private bool _isScopeInitialized = false;
    private bool _isDataLayerInitialized = false;
    private string _dataLayersBindingConfiguration = string.Empty;

    [Inject]
    public AdapterProvider(NameValueCollection settings)
    {
      AppDomain currentDomain = AppDomain.CurrentDomain;
      currentDomain.AssemblyResolve += new ResolveEventHandler(DataLayerAssemblyResolveEventHandler);

      var ninjectSettings = new NinjectSettings { LoadExtensions = false, UseReflectionBasedInjection = true };
      _kernel = new StandardKernel(ninjectSettings, new AdapterModule());

      _kernel.Load(new XmlExtensionModule());
      _settings = _kernel.Get<AdapterSettings>();
      _settings.AppendSettings(settings);

      Directory.SetCurrentDirectory(_settings["BaseDirectoryPath"]);

      #region initialize webHttpClient for converting old mapping
      string proxyHost = _settings["ProxyHost"];
      string proxyPort = _settings["ProxyPort"];
      string rdsUri = _settings["ReferenceDataServiceUri"];

      if (!String.IsNullOrEmpty(proxyHost) && !String.IsNullOrEmpty(proxyPort))
      {
        WebProxy webProxy = _settings.GetWebProxyCredentials().GetWebProxy() as WebProxy;
        _webHttpClient = new WebHttpClient(rdsUri, null, webProxy);
      }
      else
      {
        _webHttpClient = new WebHttpClient(rdsUri);
      }
      #endregion

      string scopesPath = String.Format("{0}Scopes.xml", _settings["AppDataPath"]);
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

      _dataLayersBindingConfiguration = string.Format("{0}DataLayersBindingConfiguration.xml", _settings["DataLayersPath"]);
      if (!Directory.Exists(_settings["DataLayersPath"]))
      {
        Directory.CreateDirectory(_settings["DataLayersPath"]);
      }

      string identityBindingRelativePath = String.Format("{0}BindingConfiguration.Adapter.xml", _settings["AppDataPath"]);

      // NInject requires full qualified path
      string identityBindingPath = Path.Combine(
        _settings["BaseDirectoryPath"],
        identityBindingRelativePath
      );

      _kernel.Load(identityBindingPath);

      InitializeIdentity();
    }

    #region application methods
    public Resource GetScopes()
    {
      try
      {
        return _scopes;
      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Error in GetScopes: {0}", ex));
        throw new Exception(string.Format("Error getting the list of scopes: {0}", ex));
      }
    }

    public VersionInfo GetVersion()
    {
      Version version = this.GetType().Assembly.GetName().Version;

      return new VersionInfo()
      {
        Major = version.Major,
        Minor = version.Minor,
        Build = version.Build,
        Revision = version.Revision
      };
    }

    //public Response AddScope(ScopeProject scope)
    //{
    //  Response response = new Response();
    //  Status status = new Status();

    //  response.StatusList.Add(status);

    //  try
    //  {
    //    Resource sc = _scopes.Find(x => x.Name.ToLower() == scope.Name.ToLower());

    //    if (sc == null)
    //    {
    //      _scopes.Add(scope);
    //      Utility.Write<Resource>(_scopes, _settings["ScopesPath"], true);
    //      status.Messages.Add(String.Format("Scope [{0}] updated successfully.", scope.Name));
    //    }
    //    else
    //    {
    //      status.Level = StatusLevel.Error;
    //      status.Messages.Add(String.Format("Scope [{0}] already exists.", scope.Name));
    //    }
    //  }
    //  catch (Exception ex)
    //  {
    //    _logger.Error(String.Format("Error updating scope [{0}]: {1}", scope.Name, ex));

    //    status.Level = StatusLevel.Error;
    //    status.Messages.Add(String.Format("Error updating scope [{0}]: {1}", scope.Name, ex));
    //  }

    //  return response;
    //}

    public Response UpdateScope(string newScope, Locator oldScope)
    {
      Response response = new Response();
      Status status = new Status();
      response.StatusList.Add(status);

      try
      {
        if (oldScope == null)
        {
          status.Level = StatusLevel.Error;
          status.Messages.Add(String.Format("Scope [{0}] does not exist.", oldScope));
        }
        else
        {
          //
          // add new scope and move applications in the existing scope to the new one
          //
          //AddScope(scope);

          if (oldScope.Applications != null)
          {
            foreach (EndpointApplication app in oldScope.Applications)
            {
              //
              // copy database dictionary
              //
              string path = _settings["AppDataPath"];
              string currDictionaryPath = String.Format("{0}DatabaseDictionary.{1}.{2}.xml", path, oldScope.Context, app.Endpoint);

              if (File.Exists(currDictionaryPath))
              {
                string updatedDictionaryPath = String.Format("{0}DatabaseDictionary.{1}.{2}.xml", path, newScope, app.Endpoint);
                File.Copy(currDictionaryPath, updatedDictionaryPath);
              }

              AddApplication(newScope, app);
              DeleteApplicationArtifacts(oldScope.Context, app.Endpoint);
            }
          }

          // delete old scope
          //DeleteScope(oldScope.Context);          
        }
      }
      catch (Exception ex)
      {
        _logger.Error(String.Format("Error updating scope [{0}]: {1}", oldScope.Context, ex));

        status.Level = StatusLevel.Error;
        status.Messages.Add(String.Format("Error updating scope [{0}]: {1}", oldScope.Context, ex));
      }

      return response;
    }

    public Response DeleteScope(string scopeName)
    {
      Response response = new Response();
      Status status = new Status();

      response.StatusList.Add(status);

      try
      {
        if (_scopes.Locators == null)
          GetResource();

        Locator sc = _scopes.Locators.Find(x => x.Context.ToLower() == scopeName.ToLower());

        if (sc == null)
        {
          status.Level = StatusLevel.Error;
          status.Messages.Add(String.Format("Scope [{0}] not found.", scopeName));
        }
        else
        {
          //
          // delete all applications under scope
          //
          if (sc.Applications != null)
          {
            for (int i = 0; i < sc.Applications.Count; i++)
            {
              EndpointApplication app = sc.Applications[i];
              DeleteApplicationArtifacts(sc.Context, app.Endpoint);
              sc.Applications.RemoveAt(i--);
            }
          }

          // remove scope from scope list
          //_scopes.Remove(sc);

          //Utility.Write<ScopeProjects>(_scopes, _settings["ScopesPath"], true);
          status.Messages.Add(String.Format("Scope [{0}] deleted successfully.", scopeName));
        }
      }
      catch (Exception ex)
      {
        _logger.Error(String.Format("Error deleting scope [{0}]: {1}", scopeName, ex));

        status.Level = StatusLevel.Error;
        status.Messages.Add(String.Format("Error deleting scope [{0}]: {1}", scopeName, ex));
      }

      return response;
    }

    public Response AddApplication(string scopeName, EndpointApplication application)
    {
      Response response = new Response();
      Status status = new Status();

      response.StatusList.Add(status);

      try
      {
        //
        // update binding configurations
        //
        string adapterBindingConfigPath = String.Format("{0}BindingConfiguration.Adapter.xml",
          _settings["AppDataPath"]);

        if (File.Exists(adapterBindingConfigPath))
        {
          XElement adapterBindingConfig = XElement.Load(adapterBindingConfigPath);

          //
          // update authorization binding
          //
          foreach (XElement bindElement in adapterBindingConfig.Elements("bind"))
          {
            if (bindElement.Attribute("name").Value == "IdentityLayer")
            {
              XAttribute toAttribute = bindElement.Attribute("to");
              XElement authorizationBinding = null;

              if (toAttribute.Value.ToString().Contains(typeof(AnonymousIdentityProvider).FullName))
              {
                authorizationBinding = new XElement("module",
                  new XAttribute("name", "AuthorizationBindingConfiguration" + "." + scopeName + "." + application.Endpoint),
                  new XElement("bind",
                    new XAttribute("name", "AuthorizationBinding"),
                    new XAttribute("service", "org.iringtools.nhibernate.IAuthorization, NHibernateLibrary"),
                    new XAttribute("to", "org.iringtools.nhibernate.ext.EveryoneAuthorization, NHibernateExtension")
                  )
                );
              }
              else  // default to NHibernate Authorization
              {
                authorizationBinding = new XElement("module",
                  new XAttribute("name", "AuthorizationBinding" + "." + scopeName + "." + application.Endpoint),
                  new XElement("bind",
                    new XAttribute("name", "AuthorizationBinding"),
                    new XAttribute("service", "org.iringtools.nhibernate.IAuthorization, NHibernateLibrary"),
                    new XAttribute("to", "org.iringtools.nhibernate.ext.NHibernateAuthorization, NHibernateExtension")
                  )
                );
              }

              authorizationBinding.Save(String.Format("{0}AuthorizationBindingConfiguration.{1}.{2}.xml",
                  _settings["AppDataPath"], scopeName, application.Endpoint));
            }

            break;
          }

          //
          // update summary binding
          //
          XElement summaryBinding = new XElement("module",
            new XAttribute("name", "SummaryBinding" + "." + scopeName + "." + application.Endpoint),
            new XElement("bind",
              new XAttribute("name", "SummaryBinding"),
              new XAttribute("service", "org.iringtools.nhibernate.ISummary, NHibernateLibrary"),
              new XAttribute("to", "org.iringtools.nhibernate.ext.NHibernateSummary, NHibernateExtension")
            )
          );

          summaryBinding.Save(String.Format("{0}SummaryBindingConfiguration.{1}.{2}.xml",
              _settings["AppDataPath"], scopeName, application.Endpoint));

          //
          // update data layer binding
          //
          if (!String.IsNullOrEmpty(application.Assembly))
          {
            XElement dataLayerBinding = new XElement("module",
              new XAttribute("name", "DataLayerBinding" + "." + scopeName + "." + application.Endpoint),
              new XElement("bind",
                new XAttribute("name", "DataLayer"),
                new XAttribute("service", "org.iringtools.library.IDataLayer, iRINGLibrary"),
                new XAttribute("to", application.Assembly)
              )
            );

            dataLayerBinding.Save(String.Format("{0}BindingConfiguration.{1}.{2}.xml",
                _settings["AppDataPath"], scopeName, application.Endpoint));
          }
        }
        else
        {
          throw new Exception("Adapter binding configuration not found.");
        }

        //
        // now add scope to scopes.xml
        //
        //if (scope.Applications == null)
        //{
        //  scope.Applications = new ScopeApplications();
        //}

        //scope.Applications.Add(application);
        //Utility.Write<ScopeProjects>(_scopes, _settings["ScopesPath"], true);

        response.Append(Generate(scopeName, application.Endpoint));
        status.Messages.Add("Application [{0}.{1}] updated successfully.");
      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Error adding application [{0}.{1}]: {2}", scopeName, application.Endpoint, ex));

        status.Level = StatusLevel.Error;
        status.Messages.Add(string.Format("Error adding application [{0}.{1}]: {2}", scopeName, application.Endpoint, ex));
      }

      return response;
    }

    public Response UpdateApplication(string scopeName, string appName, EndpointApplication updatedApp)
    {
      Response response = new Response();
      Status status = new Status();

      response.StatusList.Add(status);

      try
      {
        // check if this scope exists in the current scope list

        if (_scopes.Locators == null)
          GetResource();

        Locator scope = _scopes.Locators.FirstOrDefault<Locator>(o => o.Context.ToLower() == scopeName.ToLower());

        if (scope == null)
        {
          throw new Exception(String.Format("Scope [{0}] not found.", scopeName));
        }

        EndpointApplication application = scope.Applications.FirstOrDefault<EndpointApplication>(o => o.Endpoint.ToLower() == appName.ToLower());

        if (application != null)  // application exists, delete and re-create it
        {
          //
          // copy database dictionary
          //
          string path = _settings["AppDataPath"];
          string currDictionaryPath = String.Format("{0}DatabaseDictionary.{1}.{2}.xml", path, scopeName, updatedApp.Endpoint);

          if (File.Exists(currDictionaryPath))
          {
            string updatedDictionaryPath = String.Format("{0}DatabaseDictionary.{1}.{2}.xml", path, scopeName, updatedApp.Endpoint);
            if (currDictionaryPath.ToLower() != updatedDictionaryPath.ToLower())
              File.Copy(currDictionaryPath, updatedDictionaryPath);
          }

          DeleteApplication(scopeName, updatedApp);
          AddApplication(scopeName, application);
        }
        else  // application does not exist, stop processing
        {
          throw new Exception(String.Format("Application [{0}.{1}] not found.", scopeName, appName));
        }

        //Utility.Write<ScopeProjects>(_scopes, _settings["ScopesPath"], true);
        status.Messages.Add(String.Format("Application [{0}.{1}] updated successfully.", scopeName, appName));
      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Error updating application [{0}.{1}]: {2}", scopeName, updatedApp.Endpoint, ex));

        status.Level = StatusLevel.Error;
        status.Messages.Add(string.Format("Error updating application [{0}.{1}]: {2}", scopeName, updatedApp.Endpoint, ex));
      }

      return response;
    }

    public Response DeleteApplication(string scopeName, EndpointApplication oldApp)
    {
      Response response = new Response();
      Status status = new Status();

      response.StatusList.Add(status);

      try
      {
        DeleteApplicationArtifacts(scopeName, oldApp.Endpoint);
        status.Messages.Add(String.Format("Application [{0}.{1}] deleted successfully.", scopeName, oldApp));
      }
      catch (Exception ex)
      {
        _logger.Error(String.Format("Error deleting application [{0}.{1}]: {2}", scopeName, oldApp, ex));

        status.Level = StatusLevel.Error;
        status.Messages.Add(String.Format("Error deleting application [{0}.{1}]: {2}", scopeName, oldApp, ex));
      }

      return response;
    }

    // delete all application artifacts except for its mapping
    private void DeleteApplicationArtifacts(string scopeName, string appName)
    {
      string path = _settings["AppDataPath"];
      string context = scopeName + "." + appName;

      string authorizationPath = String.Format("{0}Authorization.{1}.xml", path, context);
      if (File.Exists(authorizationPath))
      {
        File.Delete(authorizationPath);
      }

      string authorizationBindingPath = String.Format("{0}AuthorizationBindingConfiguration.{1}.xml", path, context);
      if (File.Exists(authorizationBindingPath))
      {
        File.Delete(authorizationBindingPath);
      }

      string bindingConfigurationPath = String.Format("{0}BindingConfiguration.{1}.xml", path, context);
      if (File.Exists(bindingConfigurationPath))
      {
        File.Delete(bindingConfigurationPath);
      }

      string databaseDictionaryPath = String.Format("{0}DatabaseDictionary.{1}.xml", path, context);
      if (File.Exists(databaseDictionaryPath))
      {
        File.Delete(databaseDictionaryPath);
      }

      string dataDictionaryPath = String.Format("{0}DataDictionary.{1}.xml", path, context);
      if (File.Exists(dataDictionaryPath))
      {
        File.Delete(dataDictionaryPath);
      }

      string nhConfigPath = String.Format("{0}nh-configuration.{1}.xml", path, context);
      if (File.Exists(nhConfigPath))
      {
        File.Delete(nhConfigPath);
      }

      string nhMappingPath = String.Format("{0}nh-mapping.{1}.xml", path, context);
      if (File.Exists(nhMappingPath))
      {
        File.Delete(nhMappingPath);
      }

      string summaryBindingConfigurationPath = String.Format("{0}SummaryBindingConfiguration.{1}.xml", path, context);
      if (File.Exists(summaryBindingConfigurationPath))
      {
        File.Delete(summaryBindingConfigurationPath);
      }

      string summaryConfigPath = String.Format("{0}SummaryConfig.{1}.xml", path, context);
      if (File.Exists(summaryConfigPath))
      {
        File.Delete(summaryConfigPath);
      }

      string appCodePath = String.Format("{0}Model.{1}.cs", _settings["AppCodePath"], context);
      if (File.Exists(appCodePath))
      {
        File.Delete(appCodePath);
      }

      string SpreadSheetConfigPath = String.Format("{0}spreadsheet-configuration.{1}.xml", path, context);
      if (File.Exists(SpreadSheetConfigPath))
      {
        File.Delete(SpreadSheetConfigPath);
      }

      string SpreadSheetDataPath = String.Format("{0}SpreadsheetData.{1}.xlsx", path, context);
      if (File.Exists(SpreadSheetDataPath))
      {
        File.Delete(SpreadSheetDataPath);
      }
    }

    #region Generate methods
    public Response Generate()
    {
      Response response = new Response();
      Status status = new Status()
      {
        Identifier = "Contexts"
      };

      response.StatusList.Add(status);

      try
      {
        if (_scopes.Locators == null)
          GetResource();

        foreach (Locator scope in _scopes.Locators)
        {
          response.Append(Generate(scope));
        }

        status.Messages.Add("Artifacts are generated successfully.");
      }
      catch (Exception ex)
      {
        string error = String.Format("Error generating application artifacts, {0}", ex);
        _logger.Error(error);

        status.Level = StatusLevel.Error;
        status.Messages.Add(error);
      }

      return response;
    }

    private Response Generate(Locator scope)
    {
      Response response = new Response();
      Status status = new Status()
      {
        Identifier = scope.Context
      };

      response.StatusList.Add(status);

      try
      {
        foreach (EndpointApplication app in scope.Applications)
        {
          response.Append(Generate(scope.Context, app.Endpoint));
        }

        status.Messages.Add("Artifacts are generated successfully.");
      }
      catch (Exception ex)
      {
        string error = String.Format("Error generating application artifacts, {0}", ex);
        _logger.Error(error);

        status.Level = StatusLevel.Error;
        status.Messages.Add(error);
      }

      return response;
    }

    public Response Generate(string scope)
    {
      foreach (Locator sc in _scopes.Locators)
      {
        if (sc.Context.ToLower() == scope.ToLower())
        {
          return Generate(sc);
        }
      }

      Response response = new Response()
      {
        Level = StatusLevel.Warning,
        Messages = new Messages()
        {
          "Scope [" + scope + "] not found."
        }
      };

      return response;
    }

    public Response Generate(string scopeName, string newAppName)
    {
      Response response = new Response();
      Locator scope = null;
      EndpointApplication application = null;

      Status status = new Status()
      {
        Identifier = scopeName + "." + newAppName
      };

      response.StatusList.Add(status);

      try
      {
        InitializeScope(scopeName, newAppName);

        scope = _scopes.Locators.FirstOrDefault<Locator>(o => o.Context.ToLower() == scopeName.ToLower());

        if (scope == null)
        {
          throw new Exception(String.Format("Scope [{0}] not found.", scopeName));
        }

        if (scope.Applications == null)
        {
          throw new Exception(String.Format("No applications found in scope [{0}].", scopeName));
        }

        application = scope.Applications.Find(o => o.Endpoint.ToLower() == newAppName.ToLower());

        if (application == null)
        {
          throw new Exception(String.Format("Application [{0}] is not found in scope [{1}].", newAppName, scopeName));
        }

        string path = _settings["AppDataPath"];
        string context = scope.Context + "." + newAppName;
        string bindingPath = String.Format("{0}BindingConfiguration.{1}.xml", path, context);
        XElement binding = XElement.Load(bindingPath);

        if (binding.Element("bind").Attribute("to").Value.Contains(typeof(NHibernateDataLayer).Name))
        {
          string dbDictionaryPath = String.Format("{0}DatabaseDictionary.{1}.xml", path, context);
          DatabaseDictionary dbDictionary = null;

          if (File.Exists(dbDictionaryPath))
          {
            dbDictionary = NHibernateUtility.LoadDatabaseDictionary(dbDictionaryPath);
          }

          if (dbDictionary != null && dbDictionary.dataObjects != null)
          {
            EntityGenerator generator = _kernel.Get<EntityGenerator>();

            string compilerVersion = "v4.0";
            if (!String.IsNullOrEmpty(_settings["CompilerVersion"]))
            {
              compilerVersion = _settings["CompilerVersion"];
            }

            response.Append(generator.Generate(compilerVersion, dbDictionary, scope.Context, application.Endpoint));
          }
          else
          {
            status.Level = StatusLevel.Warning;
            status.Messages.Add(string.Format("Database dictionary [{0}.{1}] does not exist.", scopeName, application.Endpoint));
          }
        }

      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Error adding application [{0}.{1}]: {2}", scopeName, newAppName, ex));

        status.Level = StatusLevel.Error;
        status.Messages.Add(string.Format("Error adding application [{0}.{1}]: {2}", scopeName, newAppName, ex));
      }

      return response;
    }
    #endregion Generate methods

    public XElement GetBinding(string projectName, string applicationName)
    {
      XElement binding = null;

      try
      {
        InitializeScope(projectName, applicationName);

        binding = XElement.Load(_settings["BindingConfigurationPath"]);
      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Error in UpdateBindingConfiguration: {0}", ex));
        //throw ex;
      }
      return binding;
    }

    public Response UpdateBinding(string projectName, string applicationName, XElement binding)
    {
      Response response = new Response();
      Status status = new Status();

      response.StatusList.Add(status);

      try
      {
        status.Identifier = String.Format("{0}.{1}", projectName, applicationName);

        InitializeScope(projectName, applicationName);

        XDocument bindingConfiguration = new XDocument();
        bindingConfiguration.Add(binding);

        bindingConfiguration.Save(_settings["BindingConfigurationPath"]);

        status.Messages.Add("BindingConfiguration of [" + projectName + "." + applicationName + "] updated successfully.");
      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Error in UpdateBindingConfiguration: {0}", ex));

        status.Level = StatusLevel.Error;
        status.Messages.Add(string.Format("Error updating the binding configuration: {0}", ex));
      }

      return response;
    }
    #endregion

    #region adapter methods
    public DataDictionary GetDictionary(string projectName, string applicationName)
    {
      try
      {
        InitializeScope(projectName, applicationName);
        InitializeDataLayer();

        return _kernel.TryGet<DataDictionary>();

      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Error in GetDictionary: {0}", ex));
        throw new Exception(string.Format("Error getting data dictionary: {0}", ex));
      }
    }

    public Contexts GetContexts(string applicationName)
    {
      try
      {
        Contexts contexts = new Contexts();

        foreach (Locator scope in _scopes.Locators)
        {
          if (scope.Context.ToLower() != "all")
          {
            var app = scope.Applications.Find(a => a.Endpoint.ToUpper() == applicationName.ToUpper());

            if (app != null)
            {
              Context context = new Context
              {
                Name = scope.Context,
                Description = "",
              };

              contexts.Add(context);
            }
          }
        }

        return contexts;
      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Error in GetContexts for {0}: {1}", applicationName, ex));
        throw new Exception(string.Format("Error in GetContexts for {0}: {1}", applicationName, ex));
      }
    }

    public WADLApplication GetWADL(string projectName, string applicationName)
    {
      WADLApplication wadl = new WADLApplication();

      try
      {
        bool isAll = projectName == "all";
        bool isApp = projectName == "app";
        if (isApp)
        {
          //get thie first context and initialize everything
          Context context = GetContexts(applicationName).FirstOrDefault();

          if (context == null)
            throw new WebFaultException(HttpStatusCode.NotFound);

          projectName = context.Name;
        }
        InitializeScope(projectName, applicationName);
        InitializeDataLayer();

        bool isReadOnly = (_settings["ReadOnlyDataLayer"] != null && _settings["ReadOnlyDataLayer"].ToString().ToLower() == "true");

        // load uri maps config
        Properties _uriMaps = new Properties();

        string uriMapsFilePath = _settings["AppDataPath"] + "UriMaps.conf";

        if (File.Exists(uriMapsFilePath))
        {
          try
          {
            _uriMaps.Load(uriMapsFilePath);
          }
          catch (Exception e)
          {
            _logger.Info("Error loading [UriMaps.config]: " + e);
          }
        }

        string baseUri = _settings["GraphBaseUri"];
        if (isAll)
          baseUri += "all/";

        string appBaseUri = Utility.FormAppBaseURI(_uriMaps, baseUri, applicationName);
        string baseResource = String.Empty;

        if (!isApp && !isAll)
        {
          appBaseUri = appBaseUri + "/" + projectName;
        }
        else if (!isAll)
        {
          baseResource = "/{contextName}";
        }

        WADLResources resources = new WADLResources
        {
          @base = appBaseUri,
        };

        string title = _application.Endpoint;
        if (title == String.Empty)
          title = applicationName;

        string appDescription = "This is an iRINGTools endpoint.";
        if (_application.Description != null && _application.Description != String.Empty)
          appDescription = _application.Description;

        string header = "<div id=\"wadlDescription\">" +
            "  <p class=\"wadlDescText\">" +
            "    " + appDescription +
            "  </p>" +
            "  <ul class=\"wadlList\">" +
            "    <li>API access is restricted to Authorized myPSN Users only.</li>" +
            "    <li>The attributes available for each context may be different.</li>" +
            "  </ul>" +
            "</div>";

        XmlDocument dummy = new XmlDocument();
        XmlNode[] headerDocText = new XmlNode[] { dummy.CreateCDataSection(header) };

        WADLHeaderDocumentation doc = new WADLHeaderDocumentation
        {
          title = title,
          CData = headerDocText,
        };

        resources.Items.Add(doc);

        if (isApp)
        {
          #region Build Contexts Resource
          WADLResource contexts = new WADLResource
          {
            path = "/contexts",
            Items = new List<object>
            {
              new WADLMethod
              {
                name = "GET",
                Items = new List<object>
                {
                  new WADLDocumentation
                  {
                    Value = "Gets the list of contexts. A context could be a Bechtel project, GBU, or other name that identifies a set of data."
                  },
                  new WADLRequest
                  {
                    Items = new List<object>
                    {
                      new WADLParameter
                      {
                        name = "start",
                        type = "int",
                        style = "query",
                        required = false,
                        @default = "0",
                        Items = new List<object>
                        {
                          new WADLDocumentation
                          {
                            Value = "The API pages results by default.  This parameter indicates which item to start with for the current page.  Defaults to 0 or start with the first item."
                          }
                        }
                      },
                      new WADLParameter
                      {
                        name = "limit",
                        type = "xsd:int",
                        style = "query",
                        required = false,
                        @default = "25",
                        Items = new List<object>
                        {
                          new WADLDocumentation
                          {
                            Value = "The API pages results by default.  This parameter indicates how many items to include in the resulting page.  Defaults to 25 items per page."
                          }
                        }
                      },
                      new WADLParameter
                      {
                        name = "format",
                        type = "xsd:string",
                        style = "query",
                        required = false,
                        @default = "json",
                        Items = new List<object>
                        {
                          new WADLDocumentation
                          {
                            Value = "API response format supplied as a query string.  Valid choices for this parameter are: JSON, HTML &amp; XML (defaults to JSON)"
                          },
                          new WADLOption
                          {
                            value = "xml",
                            mediaType = "application/xml",
                          },
                          new WADLOption
                          {
                            value = "json",
                            mediaType = "application/json",
                          },
                          new WADLOption
                          {
                            value = "html",
                            mediaType = "application/html",
                          }
                        }
                      }
                    }
                  }
                }
              }
            }
          };

          resources.Items.Add(contexts);
          #endregion
        }

        if (_dataDictionary.enableSummary)
        {
          #region Build Summary Resource
          WADLResource summary = new WADLResource
          {
            path = baseResource + "/summary",
            Items = new List<object>
            {
              new WADLMethod
              {
                name = "GET",
                Items = new List<object>
                {
                  new WADLDocumentation
                  {
                    Value = "Gets a customizable summary of the data on the endpoint. Only JSON is returned at this time."
                  },
                  new WADLRequest
                  {
                    Items = new List<object>
                    {
                      new WADLParameter
                      {
                        name = "contextName",
                        type = "string",
                        style = "template",
                        required = true,
                        Items = new List<object>
                        {
                          new WADLDocumentation
                          {
                            Value = "The name of the context.  A context can be a Bechtel project, or GBU.  Each context could refer to one or more repositories."
                          }
                        }
                      }
                    }
                  }
                }
              }
            }
          };

          resources.Items.Add(summary);
          #endregion
        }

        foreach (DataObject dataObject in _dataDictionary.dataObjects)
        {
          if (!dataObject.isRelatedOnly)
          {
            #region Build DataObject List Resource
            WADLResource list = new WADLResource
            {
              path = baseResource + "/" + dataObject.objectName.ToLower(),
              Items = new List<object>
            {
              #region Build GetList Method
              new WADLMethod
              {
                name = "GET",
                Items = new List<object>
                {
                  new WADLDocumentation
                  {
                    Value = String.Format(
                      "Gets a list of {0} data. {1} Data is returned according to the context specific configuration.  In addition to paging and sorting, results can be filtered by using property names as query paramters in the form: ?{{propertyName}}={{value}}.", 
                       CultureInfo.CurrentCulture.TextInfo.ToTitleCase(dataObject.objectName.ToLower()),
                      dataObject.description
                    )
                  },
                  new WADLRequest
                  {
                    Items = new List<object>
                    {
                      new WADLParameter
                      {
                        name = "contextName",
                        type = "string",
                        style = "template",
                        required = true,
                        Items = new List<object>
                        {
                          new WADLDocumentation
                          {
                            Value = "The name of the context.  A context can be a Bechtel project, or GBU.  Each context could refer to one or more repositories."
                          }
                        }
                      },                      
                      new WADLParameter
                      {
                        name = "start",
                        type = "int",
                        style = "query",
                        required = false,
                        @default = "0",
                        Items = new List<object>
                        {
                          new WADLDocumentation
                          {
                            Value = "The API pages results by default.  This parameter indicates which item to start with for the current page.  Defaults to 0 or start with the first item."
                          }
                        }
                      },
                      new WADLParameter
                      {
                        name = "limit",
                        type = "xsd:int",
                        style = "query",
                        required = false,
                        @default = "25",
                        Items = new List<object>
                        {
                          new WADLDocumentation
                          {
                            Value = "The API pages results by default.  This parameter indicates how many items to include in the resulting page.  Defaults to 25 items per page."
                          }
                        }
                      },
                      new WADLParameter
                      {
                        name = "format",
                        type = "xsd:string",
                        style = "query",
                        required = false,
                        @default = "json",
                        Items = new List<object>
                        {
                          new WADLDocumentation
                          {
                            Value = "API response format supplied as a query string.  Valid choices for this parameter are: JSON, HTML &amp; XML (defaults to JSON)"
                          },
                          new WADLOption
                          {
                            value = "xml",
                            mediaType = "application/xml",
                          },
                          new WADLOption
                          {
                            value = "json",
                            mediaType = "application/json",
                          },
                          new WADLOption
                          {
                            value = "html",
                            mediaType = "application/html",
                          }
                        }
                      }
                    }
                  }
                }
              },
              #endregion
            }
            };

            if (!dataObject.isReadOnly && !isReadOnly)
            {
              #region Build PutList Method
              WADLMethod put = new WADLMethod
              {
                name = "PUT",
                Items = new List<object>
              {
                new WADLDocumentation
                {
                  Value = String.Format(
                    "Updates a list of {0} data in the specified context. {1}. The response returned provides information about how each item was proccessed, and any issues that were encountered.", 
                      CultureInfo.CurrentCulture.TextInfo.ToTitleCase(dataObject.objectName.ToLower()),
                    "This is a dynamic data object"
                  )
                },
                new WADLRequest
                {
                  Items = new List<object>
                  {
                    new WADLParameter
                    {
                      name = "contextName",
                      type = "string",
                      style = "template",
                      required = true,
                      Items = new List<object>
                      {
                        new WADLDocumentation
                        {
                          Value = "The name of the context.  A context can be a Bechtel project, or GBU.  Each context could refer to one or more repositories."
                        }
                      }
                    },
                    new WADLParameter
                    {
                      name = "format",
                      type = "xsd:string",
                      style = "query",
                      required = false,
                      @default = "json",
                      Items = new List<object>
                      {
                        new WADLDocumentation
                        {
                          Value = "API response format supplied as a query string.  Valid choices for this parameter are: JSON &amp; XML (defaults to JSON)"
                        },
                        new WADLOption
                        {
                          value = "xml",
                          mediaType = "application/xml",
                        },
                        new WADLOption
                        {
                          value = "json",
                          mediaType = "application/json",
                        }
                      }
                    }
                  }
                }
              }
              };
              #endregion

              list.Items.Add(put);

              #region Build PostList Method
              WADLMethod post = new WADLMethod
              {
                name = "POST",
                Items = new List<object>
              {
                new WADLDocumentation
                {
                  Value = String.Format(
                    "Creates a single {0} item in the specified context. {1}. The response returned provides information about how each item was proccessed, and any issues that were encountered.", 
                      CultureInfo.CurrentCulture.TextInfo.ToTitleCase(dataObject.objectName.ToLower()),
                    "This is a dynamic data object"
                  )
                },
                new WADLRequest
                {
                  Items = new List<object>
                  {
                    new WADLParameter
                    {
                      name = "contextName",
                      type = "string",
                      style = "template",
                      required = true,
                      Items = new List<object>
                      {
                        new WADLDocumentation
                        {
                          Value = "The name of the context.  A context can be a Bechtel project, or GBU.  Each context could refer to one or more repositories."
                        }
                      }
                    },
                    new WADLParameter
                    {
                      name = "format",
                      type = "xsd:string",
                      style = "query",
                      required = false,
                      @default = "json",
                      Items = new List<object>
                      {
                        new WADLDocumentation
                        {
                          Value = "API response format supplied as a query string.  Valid choices for this parameter are: JSON &amp; XML (defaults to JSON)"
                        },
                        new WADLOption
                        {
                          value = "xml",
                          mediaType = "application/xml",
                        },
                        new WADLOption
                        {
                          value = "json",
                          mediaType = "application/json",
                        }
                      }
                    }
                  }
                }
              }
              };
              #endregion

              list.Items.Add(post);
            }

            resources.Items.Add(list);
            #endregion

            if (_dataDictionary.enableSearch)
            {
              #region Build DataObject Search Resource
              WADLResource search = new WADLResource
              {
                path = baseResource + "/" + dataObject.objectName.ToLower() + "/search?q={query}",
                Items = new List<object>
            {
              #region Build GetList Method
              new WADLMethod
              {
                name = "GET",
                Items = new List<object>
                {
                  new WADLDocumentation
                  {
                    Value = String.Format(
                      "Searches the  {0} data for the specified context.  The specific properties searched, and whether content is searched, will depend on the context configuration.  In addition to paging and sorting, results can be filtered by using property names as query paramters in the form: ?{{propertyName}}={{value}}.", 
                       CultureInfo.CurrentCulture.TextInfo.ToTitleCase(dataObject.objectName.ToLower())
                    )
                  },
                  new WADLRequest
                  {
                    Items = new List<object>
                    {
                      new WADLParameter
                      {
                        name = "contextName",
                        type = "string",
                        style = "template",
                        required = true,
                        Items = new List<object>
                        {
                          new WADLDocumentation
                          {
                            Value = "The name of the context.  A context can be a Bechtel project, or GBU.  Each context could refer to one or more repositories."
                          }
                        }
                      },
                      new WADLParameter
                      {
                        name = "q",
                        type = "string",
                        style = "query",
                        required = true,
                        Items = new List<object>
                        {
                          new WADLDocumentation
                          {
                            Value = "Enter full or partial text to search for (minimum 2 characters). The specific properties searched, and whether content is searched, will depend on the repository configuration."
                          }
                        }
                      },
                      new WADLParameter
                      {
                        name = "start",
                        type = "int",
                        style = "query",
                        required = false,
                        @default = "0",
                        Items = new List<object>
                        {
                          new WADLDocumentation
                          {
                            Value = "The API pages results by default.  This parameter indicates which item to start with for the current page.  Defaults to 0 or start with the first item."
                          }
                        }
                      },
                      new WADLParameter
                      {
                        name = "limit",
                        type = "int",
                        style = "query",
                        required = false,
                        @default = "25",
                        Items = new List<object>
                        {
                          new WADLDocumentation
                          {
                            Value = "The API pages results by default.  This parameter indicates how many items to include in the resulting page.  Defaults to 25 items per page."
                          }
                        }
                      },
                      new WADLParameter
                      {
                        name = "format",
                        type = "string",
                        style = "query",
                        required = false,
                        @default = "json",
                        Items = new List<object>
                        {
                          new WADLDocumentation
                          {
                            Value = "API response format supplied as a query string.  Valid choices for this parameter are: JSON, HTML &amp; XML (defaults to JSON)"
                          },
                          new WADLOption
                          {
                            value = "xml",
                            mediaType = "application/xml",
                          },
                          new WADLOption
                          {
                            value = "json",
                            mediaType = "application/json",
                          },
                          new WADLOption
                          {
                            value = "html",
                            mediaType = "application/html",
                          }
                        }
                      }
                    }
                  }
                }
              },
              #endregion
            }
              };

              resources.Items.Add(search);
              #endregion
            }

            if (!dataObject.isListOnly)
            {
              #region Build DataObject Item Resource
              WADLResource item = new WADLResource
              {
                path = baseResource + "/" + dataObject.objectName.ToLower() + "/{identifier}",
                Items = new List<object>
              {
                #region Build GetItem Method
                new WADLMethod
                {
                  name = "GET",
                  Items = new List<object>
                  {
                    new WADLDocumentation
                    {
                      Value = String.Format(
                        "Gets a list containing the specified {0} data. {1}. Data is returned according to the context specific configuration.  In addition to paging and sorting, results can be filtered by using property names as query paramters in the form: ?{{propertyName}}={{value}}.", 
                         CultureInfo.CurrentCulture.TextInfo.ToTitleCase(dataObject.objectName.ToLower()),
                        "This is a dynamic data object"
                      )
                    },
                    new WADLRequest
                    {
                      Items = new List<object>
                      {
                        new WADLParameter
                        {
                          name = "contextName",
                          type = "string",
                          style = "template",
                          required = true,
                          Items = new List<object>
                          {
                            new WADLDocumentation
                            {
                              Value = "The name of the context.  A context can be a Bechtel project, or GBU.  Each context could refer to one or more repositories."
                            }
                          }
                        },
                        new WADLParameter
                        {
                          name = "identifier",
                          type = "string",
                          style = "template",
                          required = true,
                          Items = new List<object>
                          {
                            new WADLDocumentation
                            {
                              Value = String.Format(
                                "The identifier of the {0} that you would like to fetch.", 
                                 CultureInfo.CurrentCulture.TextInfo.ToTitleCase(dataObject.objectName.ToLower())
                              )
                            }
                          }
                        },
                        new WADLParameter
                        {
                          name = "start",
                          type = "integer",
                          style = "query",
                          required = false,
                          @default = "0",
                          Items = new List<object>
                          {
                            new WADLDocumentation
                            {
                              Value = "The API pages results by default.  This parameter indicates which item to start with for the current page.  Defaults to 0 or start with the first item."
                            }
                          }
                        },
                        new WADLParameter
                        {
                          name = "limit",
                          type = "xsd:int",
                          style = "query",
                          required = false,
                          @default = "25",
                          Items = new List<object>
                          {
                            new WADLDocumentation
                            {
                              Value = "The API pages results by default.  This parameter indicates how many items to include in the resulting page.  Defaults to 25 items per page."
                            }
                          }
                        },
                        new WADLParameter
                        {
                          name = "format",
                          type = "xsd:string",
                          style = "query",
                          required = false,
                          @default = "json",
                          Items = new List<object>
                          {
                            new WADLDocumentation
                            {
                              Value = "API response format supplied as a query string.  Valid choices for this parameter are: JSON, HTML &amp; XML (defaults to JSON)"
                            },
                            new WADLOption
                            {
                              value = "xml",
                              mediaType = "application/xml",
                            },
                            new WADLOption
                            {
                              value = "json",
                              mediaType = "application/json",
                            },
                            new WADLOption
                            {
                              value = "html",
                              mediaType = "application/html",
                            }
                          }
                        }
                      }
                    }
                  }
                },
                #endregion
              }
              };

              if (!dataObject.isReadOnly && !isReadOnly)
              {
                #region Build PutItem Method
                WADLMethod put = new WADLMethod
                {
                  name = "PUT",
                  Items = new List<object>
              {
                new WADLDocumentation
                {
                  Value = String.Format(
                    "Updates the specified {0} in the specified context. {1}. The response returned provides information about how each item was proccessed, and any issues that were encountered.", 
                      CultureInfo.CurrentCulture.TextInfo.ToTitleCase(dataObject.objectName.ToLower()),
                    "This is a dynamic data object"
                  )
                },
                new WADLRequest
                {
                  Items = new List<object>
                  {
                    new WADLParameter
                    {
                      name = "contextName",
                      type = "string",
                      style = "template",
                      required = true,
                      Items = new List<object>
                      {
                        new WADLDocumentation
                        {
                          Value = "The name of the context.  A context can be a Bechtel project, or GBU.  Each context could refer to one or more repositories."
                        }
                      }
                    },
                    new WADLParameter
                        {
                          name = "identifier",
                          type = "string",
                          style = "template",
                          required = true,
                          Items = new List<object>
                          {
                            new WADLDocumentation
                            {
                              Value = String.Format(
                                "The identifier of the {0} that you would like to fetch.", 
                                 CultureInfo.CurrentCulture.TextInfo.ToTitleCase(dataObject.objectName.ToLower())
                              )
                            }
                          }
                        },
                    new WADLParameter
                    {
                      name = "format",
                      type = "xsd:string",
                      style = "query",
                      required = false,
                      @default = "json",
                      Items = new List<object>
                      {
                        new WADLDocumentation
                        {
                          Value = "API response format supplied as a query string.  Valid choices for this parameter are: JSON &amp; XML (defaults to JSON)"
                        },
                        new WADLOption
                        {
                          value = "xml",
                          mediaType = "application/xml",
                        },
                        new WADLOption
                        {
                          value = "json",
                          mediaType = "application/json",
                        }
                      }
                    }
                  }
                }
              }
                };
                #endregion

                item.Items.Add(put);

                #region Build DeleteItem Method
                WADLMethod delete = new WADLMethod
                {
                  name = "DELETE",
                  Items = new List<object>
              {
                new WADLDocumentation
                {
                  Value = String.Format(
                    "Deletes the specified {0} item in the specified context. {1}. The response returned provides information about how each item was proccessed, and any issues that were encountered.", 
                      CultureInfo.CurrentCulture.TextInfo.ToTitleCase(dataObject.objectName.ToLower()),
                    "This is a dynamic data object"
                  )
                },
                new WADLRequest
                {
                  Items = new List<object>
                  {
                    new WADLParameter
                    {
                      name = "contextName",
                      type = "string",
                      style = "template",
                      required = true,
                      Items = new List<object>
                      {
                        new WADLDocumentation
                        {
                          Value = "The name of the context.  A context can be a Bechtel project, or GBU.  Each context could refer to one or more repositories."
                        }
                      }
                    },
                    new WADLParameter
                        {
                          name = "identifier",
                          type = "string",
                          style = "template",
                          required = true,
                          Items = new List<object>
                          {
                            new WADLDocumentation
                            {
                              Value = String.Format(
                                "The identifier of the {0} that you would like to fetch.", 
                                 CultureInfo.CurrentCulture.TextInfo.ToTitleCase(dataObject.objectName.ToLower())
                              )
                            }
                          }
                        },
                    new WADLParameter
                    {
                      name = "format",
                      type = "xsd:string",
                      style = "query",
                      required = false,
                      @default = "json",
                      Items = new List<object>
                      {
                        new WADLDocumentation
                        {
                          Value = "API response format supplied as a query string.  Valid choices for this parameter are: JSON &amp; XML (defaults to JSON)"
                        },
                        new WADLOption
                        {
                          value = "xml",
                          mediaType = "application/xml",
                        },
                        new WADLOption
                        {
                          value = "json",
                          mediaType = "application/json",
                        }
                      }
                    }
                  }
                }
              }
                };
                #endregion

                item.Items.Add(delete);
              }

              resources.Items.Add(item);
              #endregion
            }

            foreach (DataRelationship relationship in dataObject.dataRelationships)
            {
              #region Build DataObject List Resource
              WADLResource relatedList = new WADLResource
              {
                path = baseResource + "/" + dataObject.objectName.ToLower() + "/{identifier}/" + relationship.relationshipName.ToLower(),
                Items = new List<object>
            {
              #region Build GetList Method
              new WADLMethod
              {
                name = "GET",
                Items = new List<object>
                {
                  new WADLDocumentation
                  {
                    Value = String.Format(
                      "Gets a list containing the {0} data related to the specified {1}. Data is returned according to the context specific configuration.  In addition to paging and sorting, results can be filtered by using property names as query paramters in the form: ?{{propertyName}}={{value}}.", 
                       CultureInfo.CurrentCulture.TextInfo.ToTitleCase(relationship.relationshipName.ToLower()),
                       CultureInfo.CurrentCulture.TextInfo.ToTitleCase(dataObject.objectName.ToLower())  
                    )
                  },
                  new WADLRequest
                  {
                    Items = new List<object>
                    {
                      new WADLParameter
                      {
                        name = "contextName",
                        type = "string",
                        style = "template",
                        required = true,
                        Items = new List<object>
                        {
                          new WADLDocumentation
                          {
                            Value = "The name of the context.  A context can be a Bechtel project, or GBU.  Each context could refer to one or more repositories."
                          }
                        }
                      },                      
                      new WADLParameter
                        {
                          name = "identifier",
                          type = "string",
                          style = "template",
                          required = true,
                          Items = new List<object>
                          {
                            new WADLDocumentation
                            {
                              Value = String.Format(
                                "The identifier of the {0} that you would like to fetch related items.", 
                                CultureInfo.CurrentCulture.TextInfo.ToTitleCase(dataObject.objectName.ToLower())
                              )
                            }
                          }
                        },
                        new WADLParameter
                      {
                        name = "start",
                        type = "int",
                        style = "query",
                        required = false,
                        @default = "0",
                        Items = new List<object>
                        {
                          new WADLDocumentation
                          {
                            Value = "The API pages results by default.  This parameter indicates which item to start with for the current page.  Defaults to 0 or start with the first item."
                          }
                        }
                      },
                      new WADLParameter
                      {
                        name = "limit",
                        type = "xsd:int",
                        style = "query",
                        required = false,
                        @default = "25",
                        Items = new List<object>
                        {
                          new WADLDocumentation
                          {
                            Value = "The API pages results by default.  This parameter indicates how many items to include in the resulting page.  Defaults to 25 items per page."
                          }
                        }
                      },
                      new WADLParameter
                      {
                        name = "format",
                        type = "xsd:string",
                        style = "query",
                        required = false,
                        @default = "json",
                        Items = new List<object>
                        {
                          new WADLDocumentation
                          {
                            Value = "API response format supplied as a query string.  Valid choices for this parameter are: JSON, HTML &amp; XML (defaults to JSON)"
                          },
                          new WADLOption
                          {
                            value = "xml",
                            mediaType = "application/xml",
                          },
                          new WADLOption
                          {
                            value = "json",
                            mediaType = "application/json",
                          },
                          new WADLOption
                          {
                            value = "html",
                            mediaType = "application/html",
                          }
                        }
                      }
                    }
                  }
                }
              },
              #endregion
            }
              };

              resources.Items.Add(relatedList);
              #endregion

              if (relationship.relationshipType == RelationshipType.OneToMany)
              {
                #region Build DataObject Item Resource
                WADLResource relatedItem = new WADLResource
                {
                  path = baseResource + "/" + dataObject.objectName.ToLower() + "/{identifier}/" + relationship.relationshipName.ToLower() + "/{relatedIdentifier}",
                  Items = new List<object>
                {
                  #region Build GetItem Method
                  new WADLMethod
                  {
                    name = "GET",
                    Items = new List<object>
                    {
                      new WADLDocumentation
                      {
                        Value = String.Format(
                          "Gets a list containing the specified {0} data related to the specified {1}. Data is returned according to the context specific configuration.  In addition to paging and sorting, results can be filtered by using property names as query paramters in the form: ?{{propertyName}}={{value}}.", 
                          CultureInfo.CurrentCulture.TextInfo.ToTitleCase(relationship.relationshipName.ToLower()),
                          CultureInfo.CurrentCulture.TextInfo.ToTitleCase(dataObject.objectName.ToLower())
                        )
                      },
                      new WADLRequest
                      {
                        Items = new List<object>
                        {
                          new WADLParameter
                          {
                            name = "contextName",
                            type = "string",
                            style = "template",
                            required = true,
                            Items = new List<object>
                            {
                              new WADLDocumentation
                              {
                                Value = "The name of the context.  A context can be a Bechtel project, or GBU.  Each context could refer to one or more repositories."
                              }
                            }
                          },
                          new WADLParameter
                          {
                            name = "identifier",
                            type = "string",
                            style = "template",
                            required = true,
                            Items = new List<object>
                            {
                              new WADLDocumentation
                              {
                                Value = String.Format(
                                  "The identifier of the {0} that you would like to fetch related items.",  
                                   CultureInfo.CurrentCulture.TextInfo.ToTitleCase(dataObject.objectName.ToLower())
                                )
                              }
                            }
                          },
                          new WADLParameter
                          {
                            name = "relatedIdentifier",
                            type = "string",
                            style = "template",
                            required = true,
                            Items = new List<object>
                            {
                              new WADLDocumentation
                              {
                                Value = String.Format(
                                  "The identifier of the {0} that you would like to fetch.", 
                                   CultureInfo.CurrentCulture.TextInfo.ToTitleCase(relationship.relationshipName.ToLower())
                                )
                              }
                            }
                          },
                          new WADLParameter
                          {
                            name = "start",
                            type = "integer",
                            style = "query",
                            required = false,
                            @default = "0",
                            Items = new List<object>
                            {
                              new WADLDocumentation
                              {
                                Value = "The API pages results by default.  This parameter indicates which item to start with for the current page.  Defaults to 0 or start with the first item."
                              }
                            }
                          },
                          new WADLParameter
                          {
                            name = "limit",
                            type = "xsd:int",
                            style = "query",
                            required = false,
                            @default = "25",
                            Items = new List<object>
                            {
                              new WADLDocumentation
                              {
                                Value = "The API pages results by default.  This parameter indicates how many items to include in the resulting page.  Defaults to 25 items per page."
                              }
                            }
                          },
                          new WADLParameter
                          {
                            name = "format",
                            type = "xsd:string",
                            style = "query",
                            required = false,
                            @default = "json",
                            Items = new List<object>
                            {
                              new WADLDocumentation
                              {
                                Value = "API response format supplied as a query string.  Valid choices for this parameter are: JSON, HTML &amp; XML (defaults to JSON)"
                              },
                              new WADLOption
                              {
                                value = "xml",
                                mediaType = "application/xml",
                              },
                              new WADLOption
                              {
                                value = "json",
                                mediaType = "application/json",
                              },
                              new WADLOption
                              {
                                value = "html",
                                mediaType = "application/html",
                              }
                            }
                          }
                        }
                      }
                    }
                  },
                  #endregion
                }
                };

                resources.Items.Add(relatedItem);
                #endregion
              }
            }
          }
        }

        wadl.Items.Add(resources);
      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Error in GetDictionary: {0}", ex));
        throw new Exception(string.Format("Error getting data dictionary: {0}", ex));
      }

      return wadl;
    }

    public mapping.Mapping GetMapping(string projectName, string applicationName)
    {
      try
      {
        InitializeScope(projectName, applicationName);

        return _mapping;
      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Error in GetMapping: {0}", ex));
        throw new Exception(string.Format("Error getting mapping: {0}", ex));
      }
    }

    public Response UpdateMapping(string projectName, string applicationName, XElement mappingXml)
    {
      Response response = new Response();
      Status status = new Status();

      response.StatusList.Add(status);

      string path = string.Format("{0}Mapping.{1}.{2}.xml", _settings["AppDataPath"], projectName, applicationName);

      try
      {
        status.Identifier = String.Format("{0}.{1}", projectName, applicationName);

        mapping.Mapping mapping = LoadMapping(path, mappingXml, ref status);

        Utility.Write<mapping.Mapping>(mapping, path, true);

        status.Messages.Add("Mapping of [" + projectName + "." + applicationName + "] updated successfully.");
      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Error in UpdateMapping: {0}", ex));

        status.Level = StatusLevel.Error;
        status.Messages.Add(string.Format("Error saving mapping file to path [{0}]: {1}", path, ex));
      }

      return response;
    }

    public Response Refresh(string projectName, string applicationName, string graphName)
    {
      Response response = new Response();
      Status status = new Status();

      response.StatusList.Add(status);

      try
      {
        status.Identifier = String.Format("{0}.{1}", projectName, applicationName);

        InitializeScope(projectName, applicationName);
        InitializeDataLayer();

        response.Append(Refresh(graphName));
      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Error in Refresh: {0}", ex));

        status.Level = StatusLevel.Error;
        status.Messages.Add(string.Format("Error refreshing graph [{0}]: {1}", graphName, ex));
      }

      return response;
    }

    //DataFilter List
    public XDocument GetDataProjection(
      string projectName, string applicationName, string resourceName,
        DataFilter filter, ref string format, int start, int limit, bool fullIndex)
    {
      try
      {
        DataDictionary dictionary = GetDictionary(projectName, applicationName);
        DataObject dataObject = dictionary.GetDataObject(resourceName);
        filter.AppendFilter(dataObject.dataFilter);
        _logger.DebugFormat("Initializing Scope: {0}.{1}", projectName, applicationName);
        InitializeScope(projectName, applicationName);
        _logger.Debug("Initializing DataLayer.");
        InitializeDataLayer();
        _logger.DebugFormat("Initializing Projection: {0} as {1}", resourceName, format);
        InitializeProjection(resourceName, ref format, false);

        _projectionEngine.Start = start;
        _projectionEngine.Limit = limit;

        IList<string> index = new List<string>();

        if (limit == 0)
        {
          limit = (_settings["DefaultPageSize"] != null) ? int.Parse(_settings["DefaultPageSize"]) : DEFAULT_PAGE_SIZE;
        }

        _logger.DebugFormat("Getting DataObjects Page: {0} {1}", start, limit);
        _dataObjects = _dataLayer.Get(_dataObjDef.objectName, filter, limit, start);
        _projectionEngine.Count = _dataLayer.GetCount(_dataObjDef.objectName, filter);
        _logger.DebugFormat("DataObjects Total Count: {0}", _projectionEngine.Count);
        _projectionEngine.FullIndex = fullIndex;

        if (_isProjectionPart7)
        {
          return _projectionEngine.ToXml(_graphMap.name, ref _dataObjects);
        }
        else
        {
          return _projectionEngine.ToXml(_dataObjDef.objectName, ref _dataObjects);
        }
      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Error in GetProjection: {0}", ex));
        throw ex;
      }
    }

    //Search
    public XDocument GetDataProjection(
      string projectName, string applicationName, string resourceName,
      ref string format, string query, int start, int limit, string sortOrder, string sortBy, bool fullIndex,
      NameValueCollection parameters)
    {
      try
      {
        InitializeScope(projectName, applicationName);
        InitializeDataLayer();

        if (!_dataDictionary.enableSearch)
          throw new WebFaultException(HttpStatusCode.NotFound);

        InitializeProjection(resourceName, ref format, false);

        IList<string> index = new List<string>();

        if (limit == 0)
        {
          limit = (_settings["DefaultPageSize"] != null) ? int.Parse(_settings["DefaultPageSize"]) : DEFAULT_PAGE_SIZE;
        }

        _projectionEngine.Start = start;
        _projectionEngine.Limit = limit;

        DataFilter filter = new DataFilter();
        if (parameters != null)
        {
          foreach (string key in parameters.AllKeys)
          {
            string[] expectedParameters = { 
                          "project",
                          "app",
                          "format", 
                          "start", 
                          "limit", 
                          "sortBy", 
                          "sortOrder",
                          "indexStyle",
                          "_dc",
                          "page",
                          "callback",
                          "q",
                        };

            if (!expectedParameters.Contains(key, StringComparer.CurrentCultureIgnoreCase))
            {
              string value = parameters[key];

              Expression expression = new Expression
              {
                PropertyName = key,
                RelationalOperator = RelationalOperator.EqualTo,
                Values = new Values { value },
                IsCaseSensitive = false,
              };

              if (filter.Expressions.Count > 0)
              {
                expression.LogicalOperator = LogicalOperator.And;
              }

              filter.Expressions.Add(expression);
            }
          }

          if (!String.IsNullOrEmpty(sortBy))
          {
            OrderExpression orderBy = new OrderExpression
            {
              PropertyName = sortBy,
            };

            if (String.Compare(SortOrder.Desc.ToString(), sortOrder, true) == 0)
            {
              orderBy.SortOrder = SortOrder.Desc;
            }
            else
            {
              orderBy.SortOrder = SortOrder.Asc;
            }

            filter.OrderExpressions.Add(orderBy);
          }

          _dataObjects = _dataLayer.Search(_dataObjDef.objectName, query, filter, limit, start);
          _projectionEngine.Count = _dataLayer.GetSearchCount(_dataObjDef.objectName, query, filter);
        }
        else
        {
          _dataObjects = _dataLayer.Search(_dataObjDef.objectName, query, limit, start);
          _projectionEngine.Count = _dataLayer.GetSearchCount(_dataObjDef.objectName, query);
        }
        _projectionEngine.FullIndex = fullIndex;

        if (_isProjectionPart7)
        {
          return _projectionEngine.ToXml(_graphMap.name, ref _dataObjects);
        }
        else
        {
          return _projectionEngine.ToXml(_dataObjDef.objectName, ref _dataObjects);
        }
      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Error in GetProjection: {0}", ex));
        throw ex;
      }
    }

    //List
    public XDocument GetDataProjection(
      string projectName, string applicationName, string resourceName,
      ref string format, int start, int limit, string sortOrder, string sortBy, bool fullIndex,
      NameValueCollection parameters)
    {
      try
      {
        _logger.DebugFormat("Initializing Scope: {0}.{1}", projectName, applicationName);
        InitializeScope(projectName, applicationName);
        _logger.Debug("Initializing DataLayer.");
        InitializeDataLayer();
        _logger.DebugFormat("Initializing Projection: {0} as {1}", resourceName, format);
        InitializeProjection(resourceName, ref format, false);

        IList<string> index = new List<string>();

        if (limit == 0)
        {
          limit = (_settings["DefaultPageSize"] != null) ? int.Parse(_settings["DefaultPageSize"]) : DEFAULT_PAGE_SIZE;
        }

        _projectionEngine.Start = start;
        _projectionEngine.Limit = limit;

                DataFilter dataFilter = new DataFilter();

                if (parameters != null)
                {
                    string filter = parameters["filter"];

                    if (filter != null)
                    {
                        dataFilter = Utility.DeserializeJson<DataFilter>(filter, true);
                    }
                    else
                    {
                        _logger.Debug("Preparing Filter from parameters.");

          foreach (string key in parameters.AllKeys)
          {
            string[] expectedParameters = { 
                          "project",
                          "app",
                          "format", 
                          "start", 
                          "limit", 
                          "sortBy", 
                          "sortOrder",
                          "indexStyle",
                          "_dc",
                          "page",
                          "callback",
                        };

            if (!expectedParameters.Contains(key, StringComparer.CurrentCultureIgnoreCase))
            {
              string value = parameters[key];

              Expression expression = new Expression
              {
                PropertyName = key,
                RelationalOperator = RelationalOperator.EqualTo,
                Values = new Values { value },
                IsCaseSensitive = false,
              };

                                if (dataFilter.Expressions.Count > 0)
                                {
                                    expression.LogicalOperator = LogicalOperator.And;
                                }

                                dataFilter.Expressions.Add(expression);
                            }
                        }
                    }

          if (!String.IsNullOrEmpty(sortBy))
          {
            OrderExpression orderBy = new OrderExpression
            {
              PropertyName = sortBy,
            };

            if (String.Compare(SortOrder.Desc.ToString(), sortOrder, true) == 0)
            {
              orderBy.SortOrder = SortOrder.Desc;
            }
            else
            {
              orderBy.SortOrder = SortOrder.Asc;
            }

                        dataFilter.OrderExpressions.Add(orderBy);
                    }

                    _logger.DebugFormat("Getting DataObjects Page: {0} {1}", start, limit);
                    _dataObjects = _dataLayer.Get(_dataObjDef.objectName, dataFilter, limit, start);
                    _projectionEngine.Count = _dataLayer.GetCount(_dataObjDef.objectName, dataFilter);
                    _logger.DebugFormat("DataObjects Total Count: {0}", _projectionEngine.Count);
                }
                else
                {
                    _logger.DebugFormat("Getting DataObjects Page: {0} {1}", start, limit);
                    _dataObjects = _dataLayer.Get(_dataObjDef.objectName, new DataFilter(), limit, start);
                    _projectionEngine.Count = _dataLayer.GetCount(_dataObjDef.objectName, new DataFilter());
                    _logger.DebugFormat("DataObjects Total Count: {0}", _projectionEngine.Count);
                }

        _projectionEngine.FullIndex = fullIndex;
        _projectionEngine.BaseURI = (projectName.ToLower() == "all")
            ? String.Format("/{0}/{1}", applicationName, resourceName)
            : String.Format("/{0}/{1}/{2}", applicationName, projectName, resourceName);

        if (_isProjectionPart7)
        {
          return _projectionEngine.ToXml(_graphMap.name, ref _dataObjects);
        }
        else
        {
          return _projectionEngine.ToXml(_dataObjDef.objectName, ref _dataObjects);
        }
      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Error in GetProjection: {0}", ex));
        throw ex;
      }
    }

    //Individual
    public object GetDataProjection(
      string projectName, string applicationName, string resourceName, string className,
       string classIdentifier, ref string format, bool fullIndex)
    {
      string dataObjectName = String.Empty;

      try
      {
        InitializeScope(projectName, applicationName);
        InitializeDataLayer();
        InitializeProjection(resourceName, ref format, true);

        if (_isFormatExpected)
        {
          if (_isResourceGraph)
          {
            _dataObjects = GetDataObject(className, classIdentifier);
          }
          else
          {
            List<string> identifiers = new List<string> { classIdentifier };
            _dataObjects = _dataLayer.Get(_dataObjDef.objectName, identifiers);
          }
          _projectionEngine.Count = _dataObjects.Count;

          _projectionEngine.BaseURI = (projectName.ToLower() == "all")
            ? String.Format("/{0}/{1}", applicationName, resourceName)
            : String.Format("/{0}/{1}/{2}", applicationName, projectName, resourceName);

          if (_dataObjects != null && _dataObjects.Count > 0)
          {
            if (_isProjectionPart7)
            {
              return _projectionEngine.ToXml(_graphMap.name, ref _dataObjects, className, classIdentifier);
            }
            else
            {
              return _projectionEngine.ToXml(_dataObjDef.objectName, ref _dataObjects);
            }
          }
          else
          {
            throw new Exception("Data object with identifier [" + classIdentifier + "] not found.");
          }
        }
        else
        {
          List<string> identifiers = new List<string> { classIdentifier };
          _dataObjects = _dataLayer.Get(_dataObjDef.objectName, identifiers);

          if (_dataObjects != null && _dataObjects.Count > 0)
          {
            IContentObject contentObject = (IContentObject)_dataObjects[0];
            return contentObject;
          }

          return null;
        }
      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Error in GetProjection: {0}", ex));
        throw ex;
      }
    }

    public IList<PicklistObject> GetPicklists(string projectName, string applicationName, string format)
    {
      string dataObjectName = String.Empty;
      IList<PicklistObject> objs;
      try
      {
        InitializeScope(projectName, applicationName);
        InitializeDataLayer();
        //InitializeProjection(resourceName, ref format, true);

        objs = _dataDictionary.picklists;
      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Error in GetPicklist: {0}", ex));
        throw ex;
      }

      return objs;
    }

    public Picklists GetPicklist(string projectName, string applicationName, string picklistName,
          string format, int start, int limit)
    {
      string dataObjectName = String.Empty;
      Picklists obj = new Picklists();
      try
      {
        InitializeScope(projectName, applicationName);
        InitializeDataLayer();
        //InitializeProjection(resourceName, ref format, true);

        obj = _dataLayer.GetPicklist(picklistName, start, limit);
      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Error in GetPicklist: {0}", ex));
        throw ex;
      }

      return obj;
    }

    //Related
    public XDocument GetDataProjection(
        string projectName, string applicationName, string resourceName, string id, string relatedResourceName,
        ref string format, int start, int limit)
    {
      try
      {
        InitializeScope(projectName, applicationName);
        InitializeDataLayer();
        InitializeProjection(resourceName, ref format, false);

        IDataObject parentDataObject = _dataLayer.Get(_dataObjDef.objectName, new List<string> { id }).FirstOrDefault<IDataObject>();
        if (parentDataObject == null) return new XDocument();

        DataRelationship dataRelationship = _dataObjDef.dataRelationships.First(c => c.relationshipName.ToLower() == relatedResourceName.ToLower());
        string relatedObjectType = dataRelationship.relatedObjectName;

        if (limit == 0)
        {
          limit = (_settings["DefaultPageSize"] != null) ? int.Parse(_settings["DefaultPageSize"]) : DEFAULT_PAGE_SIZE;
        }

        _projectionEngine.Start = start;
        _projectionEngine.Limit = limit;

        _projectionEngine.BaseURI = (projectName.ToLower() == "all")
            ? String.Format("/{0}/{1}/{2}/{3}", applicationName, resourceName, id, relatedResourceName)
            : String.Format("/{0}/{1}/{2}/{3}/{4}", applicationName, projectName, resourceName, id, relatedResourceName);

        _projectionEngine.Count = _dataLayer.GetRelatedCount(parentDataObject, relatedObjectType);
        _dataObjects = _dataLayer.GetRelatedObjects(parentDataObject, relatedObjectType, limit, start);

        XDocument xdoc = _projectionEngine.ToXml(relatedObjectType, ref _dataObjects);
        return xdoc;
      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Error in GetProjection: {0}", ex));
        throw ex;
      }
    }

    public XDocument GetDataProjection(string projectName, string applicationName, string resourceName, string id,
      string relatedResourceName, string relatedId, ref string format)
    {
      try
      {
        InitializeScope(projectName, applicationName);
        InitializeDataLayer();
        InitializeProjection(resourceName, ref format, false);

        IDataObject parentDataObject = _dataLayer.Get(_dataObjDef.objectName, new List<string> { id }).FirstOrDefault<IDataObject>();
        if (parentDataObject == null) return new XDocument();

        _projectionEngine.BaseURI = (projectName.ToLower() == "all")
            ? String.Format("/{0}/{1}/{2}/{3}", applicationName, resourceName, id, relatedResourceName)
            : String.Format("/{0}/{1}/{2}/{3}/{4}", applicationName, projectName, resourceName, id, relatedResourceName);

        DataRelationship relationship = _dataObjDef.dataRelationships.First(c => c.relationshipName.ToLower() == relatedResourceName.ToLower());
        DataObject relatedDataObject = _dataDictionary.dataObjects.First(c => c.objectName.ToLower() == relationship.relatedObjectName.ToLower());

        _dataObjects = _dataLayer.Get(relatedDataObject.objectName, new List<string> { relatedId });

        XDocument xdoc = _projectionEngine.ToXml(relatedDataObject.objectName, ref _dataObjects);
        return xdoc;
      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Error in GetProjection: {0}", ex));
        throw ex;
      }
    }

    private IList<IDataObject> GetDataObject(string className, string classIdentifier)
    {
      DataFilter filter = new DataFilter();

      IList<string> identifiers = new List<string> { classIdentifier };

      string fixedIdentifierBoundary = (_settings["fixedIdentifierBoundary"] == null)
        ? "#" : _settings["fixedIdentifierBoundary"];

      #region parse identifier to build data filter
      ClassTemplateMap classTemplateMap = _graphMap.GetClassTemplateMapByName(className);

      if (classTemplateMap != null && classTemplateMap.classMap != null)
      {
        mapping.ClassMap classMap = classTemplateMap.classMap;

        string[] identifierValues = !String.IsNullOrEmpty(classMap.identifierDelimiter)
          ? classIdentifier.Split(new string[] { classMap.identifierDelimiter }, StringSplitOptions.None)
          : new string[] { classIdentifier };

        for (int i = 0; i < classMap.identifiers.Count; i++)
        {
          if (!(classMap.identifiers[i].StartsWith(fixedIdentifierBoundary) && classMap.identifiers[i].EndsWith(fixedIdentifierBoundary)))
          {
            string clsIdentifier = classMap.identifiers[i];
            string identifierValue = identifierValues[i];

            if (clsIdentifier.Split('.').Length > 2)  // related property
            {
              string[] clsIdentifierParts = clsIdentifier.Split('.');
              string relatedObjectType = clsIdentifierParts[clsIdentifierParts.Length - 2];

              // get related object then assign its related properties to top level data object properties
              DataFilter relatedObjectFilter = new DataFilter();

              Expression relatedExpression = new Expression
              {
                PropertyName = clsIdentifierParts.Last(),
                Values = new Values { identifierValue }
              };

              relatedObjectFilter.Expressions.Add(relatedExpression);
              IList<IDataObject> relatedObjects = _dataLayer.Get(relatedObjectType, relatedObjectFilter, 0, 0);

              if (relatedObjects != null && relatedObjects.Count > 0)
              {
                IDataObject relatedObject = relatedObjects.First();
                DataRelationship dataRelationship = _dataObjDef.dataRelationships.Find(c => c.relatedObjectName == relatedObjectType);

                foreach (PropertyMap propertyMap in dataRelationship.propertyMaps)
                {
                  Expression expression = new Expression();

                  if (filter.Expressions.Count > 0)
                    expression.LogicalOperator = LogicalOperator.And;

                  expression.PropertyName = propertyMap.dataPropertyName;
                  expression.Values = new Values { 
                    relatedObject.GetPropertyValue(propertyMap.relatedPropertyName).ToString() 
                  };
                  filter.Expressions.Add(expression);
                }
              }
            }
            else  // direct property
            {
              Expression expression = new Expression();

              if (filter.Expressions.Count > 0)
                expression.LogicalOperator = LogicalOperator.And;

              expression.PropertyName = clsIdentifier.Substring(clsIdentifier.LastIndexOf('.') + 1);
              expression.Values = new Values { identifierValue };
              filter.Expressions.Add(expression);
            }
          }
        }

        identifiers = _dataLayer.GetIdentifiers(_dataObjDef.objectName, filter);
        if (identifiers == null || identifiers.Count == 0)
        {
          throw new Exception("Identifier [" + classIdentifier + "] of class [" + className + "] is not found.");
        }
      }
      #endregion

      IList<IDataObject> dataObjects = _dataLayer.Get(_dataObjDef.objectName, identifiers);
      if (dataObjects != null && dataObjects.Count > 0)
      {
        return dataObjects;
      }

      return null;
    }


    public Response Delete(string projectName, string applicationName, string graphName)
    {
      Response response = new Response();
      Status status = new Status();

      response.StatusList.Add(status);

      try
      {
        status.Identifier = String.Format("{0}.{1}.{2}", projectName, applicationName, graphName);

        InitializeScope(projectName, applicationName);

        _semanticEngine = _kernel.Get<ISemanticLayer>("dotNetRDF");

        response.Append(_semanticEngine.Delete(graphName));
      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Error deleting {0} graphs: {1}", graphName, ex));

        status.Level = StatusLevel.Error;
        status.Messages.Add(string.Format("Error deleting all graphs: {0}", ex));
      }

      return response;
    }

    public Response Post(string projectName, string applicationName, string graphName, string format, XDocument xml)
    {
      Response response = null;

      try
      {
        InitializeScope(projectName, applicationName);

        if (_settings["ReadOnlyDataLayer"] != null && _settings["ReadOnlyDataLayer"].ToString().ToLower() == "true")
        {
          string message = "Can not perform post on read-only data layer of [" + projectName + "." + applicationName + "].";
          _logger.Error(message);

          response = new Response();
          response.DateTimeStamp = DateTime.Now;
          response.Level = StatusLevel.Error;
          response.Messages = new Messages() { message };

          return response;
        }

        InitializeDataLayer();

        InitializeProjection(graphName, ref format, false);

        IList<IDataObject> dataObjects = null;
        if (_isProjectionPart7)
        {
          dataObjects = _projectionEngine.ToDataObjects(_graphMap.name, ref xml);
        }
        else
        {
          dataObjects = _projectionEngine.ToDataObjects(_dataObjDef.objectName, ref xml);
        }

        //_projectionEngine = _kernel.Get<IProjectionLayer>(format.ToLower());
        //IList<IDataObject> dataObjects = _projectionEngine.ToDataObjects(graphName, ref xml);
        response = _dataLayer.Post(dataObjects);

        response.DateTimeStamp = DateTime.Now;
        response.Level = StatusLevel.Success;
      }
      catch (Exception ex)
      {
        _logger.Error("Error in Post: " + ex);
        if (response == null)
        {
          response = new Response();
        }

        Status status = new Status
        {
          Level = StatusLevel.Error,
          Messages = new Messages { ex.Message },
        };

        response.DateTimeStamp = DateTime.Now;
        response.Level = StatusLevel.Error;
        response.StatusList.Add(status);
      }

      return response;
    }

    public Response PostContent(string projectName, string applicationName, string graphName, string format, string identifier, Stream stream)
    {
      Response response = null;

      try
      {
        InitializeScope(projectName, applicationName);

        if (_settings["ReadOnlyDataLayer"] != null && _settings["ReadOnlyDataLayer"].ToString().ToLower() == "true")
        {
          string message = "Can not perform post on read-only data layer of [" + projectName + "." + applicationName + "].";
          _logger.Error(message);

          response = new Response();
          response.DateTimeStamp = DateTime.Now;
          response.Level = StatusLevel.Error;
          response.Messages = new Messages() { message };

          return response;
        }

        InitializeDataLayer();

        //_projectionEngine = _kernel.Get<IProjectionLayer>(format.ToLower());

        IList<IDataObject> dataObjects = new List<IDataObject>();
        IList<string> identifiers = new List<string> { identifier };
        dataObjects = _dataLayer.Create(graphName, identifiers);

        IContentObject contentObject = (IContentObject)dataObjects[0];
        contentObject.content = stream;

        IncomingWebRequestContext request = WebOperationContext.Current.IncomingRequest;
        string contentType = request.ContentType;
        contentObject.contentType = contentType;

        dataObjects = new List<IDataObject>();
        dataObjects.Add(contentObject);

        response = _dataLayer.Post(dataObjects);
        response.DateTimeStamp = DateTime.Now;
        response.Level = StatusLevel.Success;
      }
      catch (Exception ex)
      {
        _logger.Error("Error in Post: " + ex);
        if (response == null)
        {
          response = new Response();
        }

        Status status = new Status
        {
          Level = StatusLevel.Error,
          Messages = new Messages { ex.Message },
        };

        response.DateTimeStamp = DateTime.Now;
        response.Level = StatusLevel.Error;
        response.StatusList.Add(status);
      }

      return response;
    }

    public Response DeleteIndividual(string projectName, string applicationName, string graphName, string identifier)
    {
      Response response = null;

      try
      {
        InitializeScope(projectName, applicationName);
        InitializeDataLayer();

        mapping.GraphMap graphMap = _mapping.FindGraphMap(graphName);

        string objectType = graphMap.dataObjectName;
        response = _dataLayer.Delete(objectType, new List<String> { identifier });

        response.DateTimeStamp = DateTime.Now;
        response.Level = StatusLevel.Success;
      }
      catch (Exception ex)
      {
        _logger.Error("Error in DeleteIndividual: " + ex);
        if (response == null)
        {
          response = new Response();
        }

        Status status = new Status
        {
          Level = StatusLevel.Error,
          Messages = new Messages { ex.Message },
        };

        response.DateTimeStamp = DateTime.Now;
        response.Level = StatusLevel.Error;
        response.StatusList.Add(status);
      }

      return response;
    }
    #endregion

    #region private methods

    private void GetResource()
    {
      WebHttpClient _javaCoreClient = new WebHttpClient(_settings["JavaCoreUri"]);
      System.Uri uri = new System.Uri(_settings["GraphBaseUri"]);
      string baseUrl = uri.Scheme + ":.." + uri.Host + ":" + uri.Port;
      _scopes = _javaCoreClient.PostMessage<Resource>("/directory/resource", baseUrl, true);
    }

    private void InitializeScope(string projectName, string applicationName, bool loadDataLayer)
    {
      try
      {
        if (_scopes.Locators == null)
          GetResource();

        string scope = String.Format("{0}.{1}", projectName, applicationName);

        if (!_isScopeInitialized)
        {
          _settings["ProjectName"] = projectName;
          _settings["ApplicationName"] = applicationName;

          string scopeSettingsPath = String.Format("{0}{1}.{2}.config", _settings["AppDataPath"], projectName, applicationName);

          if (File.Exists(scopeSettingsPath))
          {
            AppSettingsReader scopeSettings = new AppSettingsReader(scopeSettingsPath);
            _settings.AppendSettings(scopeSettings);
          }

          if (projectName.ToLower() != "all")
          {
            string appSettingsPath = String.Format("{0}All.{1}.config", _settings["AppDataPath"], applicationName);

            if (File.Exists(appSettingsPath))
            {
              AppSettingsReader appSettings = new AppSettingsReader(appSettingsPath);
              _settings.AppendSettings(appSettings);
            }
          }

          //scope stuff

          bool isScopeValid = false;
          foreach (Locator project in _scopes.Locators)
          {
            if (project.Context.ToUpper() == projectName.ToUpper())
            {
              foreach (EndpointApplication application in project.Applications)
              {
                if (application.Endpoint.ToUpper() == applicationName.ToUpper())
                {
                  _application = application;
                  isScopeValid = true;
                  break;
                }
              }
            }
          }

          if (!isScopeValid)
            scope = String.Format("all.{0}", applicationName);

          _settings["Scope"] = scope;

          string relativeBindingConfigPath = string.Format("{0}BindingConfiguration.{1}.{2}.xml",
            _settings["AppDataPath"], _settings["ProjectName"], _settings["ApplicationName"]);

          // NInject requires full qualified path
          string bindingConfigPath = Path.Combine(
            _settings["BaseDirectoryPath"],
            relativeBindingConfigPath
          );

          _settings["BindingConfigurationPath"] = bindingConfigPath;

          string dbDictionaryPath = String.Format("{0}DatabaseDictionary.{1}.xml", _settings["AppDataPath"], scope);

          _settings["DBDictionaryPath"] = dbDictionaryPath;

          string mappingPath = String.Format("{0}Mapping.{1}.xml", _settings["AppDataPath"], scope);

          if (File.Exists(mappingPath))
          {
            try
            {
              _mapping = Utility.Read<mapping.Mapping>(mappingPath);
            }
            catch (Exception legacyEx)
            {
              _logger.Warn("Error loading mapping file [" + mappingPath + "]:" + legacyEx);
              Status status = new Status();

              _mapping = LoadMapping(mappingPath, ref status);
              _logger.Info(status.ToString());
            }
          }
          else
          {
            _mapping = new mapping.Mapping();
            Utility.Write<mapping.Mapping>(_mapping, mappingPath);
          }

          _kernel.Bind<mapping.Mapping>().ToConstant(_mapping);
          _isScopeInitialized = true;
        }
      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Error initializing application: {0}", ex));
        throw new Exception(string.Format("Error initializing application: {0})", ex));
      }
    }

    private void InitializeScope(string projectName, string applicationName)
    {
      InitializeScope(projectName, applicationName, true);
    }

    private void InitializeProjection(string resourceName, ref string format, bool isIndividual)
    {
      try
      {
        string[] expectedFormats = { 
              "rdf", 
              "dto",
              "p7xml",
              "xml", 
              "json", 
              "html"
            };

        _graphMap = _mapping.FindGraphMap(resourceName);

        if (_graphMap != null)
        {
          _isResourceGraph = true;
          _dataObjDef = _dataDictionary.dataObjects.Find(o => o.objectName.ToUpper() == _graphMap.dataObjectName.ToUpper());

          if (_dataObjDef == null || _dataObjDef.isRelatedOnly)
          {
            _logger.Warn("Data object [" + _graphMap.dataObjectName + "] not found.");
            throw new WebFaultException(HttpStatusCode.NotFound);
          }
        }
        else
        {
          _dataObjDef = _dataDictionary.dataObjects.Find(o => o.objectName.ToUpper() == resourceName.ToUpper());

          if (_dataObjDef == null || _dataObjDef.isRelatedOnly)
          {
            _logger.Warn("Resource [" + resourceName + "] not found.");
            throw new WebFaultException(HttpStatusCode.NotFound);
          }
        }

        if (format == null)
        {
          if (isIndividual && !String.IsNullOrEmpty(_dataObjDef.defaultProjectionFormat))
          {
            format = _dataObjDef.defaultProjectionFormat;
          }
          else if (!String.IsNullOrEmpty(_dataObjDef.defaultListProjectionFormat))
          {
            format = _dataObjDef.defaultListProjectionFormat;
          }
          else
          {
            format = "json";
          }
        }
        _isFormatExpected = expectedFormats.Contains(format.ToLower());

        if (format != null && _isFormatExpected)
        {
          _projectionEngine = _kernel.Get<IProjectionLayer>(format.ToLower());

          if (_projectionEngine.GetType().BaseType == typeof(BasePart7ProjectionEngine))
          {
            _isProjectionPart7 = true;
            if (_graphMap == null)
            {
              throw new FileNotFoundException("Requested resource [" + resourceName + "] cannot be rendered as Part7.");
            }
          }
        }
        else if (format == _settings["DefaultProjectionFormat"] && _isResourceGraph)
        {
          format = "p7xml";
          _projectionEngine = _kernel.Get<IProjectionLayer>("p7xml");
          _isProjectionPart7 = true;
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
      InitializeDataLayer(true);
    }

    private void InitializeDataLayer(bool setDictionary)
    {
      try
      {
        if (!_isDataLayerInitialized)
        {
          _logger.Debug("Initializing data layer...");

          if (_settings["DumpSettings"] == "True")
          {
            Dictionary<string, string> settingsDictionary = new Dictionary<string, string>();

            foreach (string key in _settings.AllKeys)
            {
              settingsDictionary.Add(key, _settings[key]);
            }

            Utility.Write<Dictionary<string, string>>(settingsDictionary, @"AdapterSettings.xml");
            Utility.Write<IDictionary>(_keyRing, @"KeyRing.xml");
          }

          XElement bindingConfig = Utility.ReadXml(_settings["BindingConfigurationPath"]);
          string assembly = bindingConfig.Element("bind").Attribute("to").Value;
          DataLayers dataLayers = GetDataLayers();

          foreach (DataLayer dataLayer in dataLayers)
          {
            if (dataLayer.Assembly.ToLower() == assembly.ToLower())
            {
              if (dataLayer.External)
              {
                Assembly dataLayerAssembly = GetDataLayerAssembly(dataLayer);

                if (dataLayerAssembly == null)
                {
                  throw new Exception("Unable to load data layer assembly.");
                }

                _settings["DataLayerPath"] = dataLayer.Path;

                Type type = dataLayerAssembly.GetType(assembly.Split(',')[0]);
                ConstructorInfo[] ctors = type.GetConstructors();

                foreach (ConstructorInfo ctor in ctors)
                {
                  ParameterInfo[] paramList = ctor.GetParameters();

                  if (paramList.Length == 0)  // default constructor
                  {
                    _dataLayer = (IDataLayer2)Activator.CreateInstance(type);

                    break;
                  }
                  else if (paramList.Length == 1)  // constructor with 1 parameter
                  {
                    if (ctor.GetParameters()[0].ParameterType.FullName == typeof(AdapterSettings).FullName)
                    {
                      _dataLayer = (IDataLayer2)Activator.CreateInstance(type, _settings);
                    }
                    else if (ctor.GetParameters()[0].ParameterType.FullName == typeof(IDictionary).FullName)
                    {
                      _dataLayer = (IDataLayer2)Activator.CreateInstance(type, _settings);
                    }

                    break;
                  }
                  else if (paramList.Length == 2)  // constructor with 2 parameters
                  {
                    if (ctor.GetParameters()[0].ParameterType.FullName == typeof(AdapterSettings).FullName &&
                      ctor.GetParameters()[1].ParameterType.FullName == typeof(IDictionary).FullName)
                    {
                      _dataLayer = (IDataLayer2)Activator.CreateInstance(type, _settings, _keyRing);
                    }
                    else if (ctor.GetParameters()[0].ParameterType.FullName == typeof(IDictionary).FullName &&
                      ctor.GetParameters()[1].ParameterType.FullName == typeof(AdapterSettings).FullName)
                    {
                      _dataLayer = (IDataLayer2)Activator.CreateInstance(type, _keyRing, _settings);
                    }
                    else
                    {
                      throw new Exception("Data layer does not contain supported constructor.");
                    }

                    break;
                  }
                }
              }
              else
              {
                if (File.Exists(_settings["BindingConfigurationPath"]))
                {
                  try
                  {
                    _kernel.Load(_settings["BindingConfigurationPath"]);
                  }
                  catch
                  {
                    ///binding already loaded
                  }
                }
                else
                {
                  _logger.Error("Binding configuration not found.");
                }

                _dataLayer = _kernel.TryGet<IDataLayer2>("DataLayer");

                if (_dataLayer == null)
                {
                  _dataLayer = (IDataLayer2)_kernel.Get<IDataLayer>("DataLayer");
                }
              }

              _kernel.Rebind<IDataLayer2>().ToConstant(_dataLayer);
              break;
            }
          }

          if (_dataLayer == null)
          {
            throw new Exception("Error initializing data layer.");
          }

          if (setDictionary)
          {
            InitializeDictionary();
          }
        }
      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Error initializing application: {0}", ex));
        throw new Exception(string.Format("Error initializing application: {0})", ex));
      }
    }

    private void InitializeDictionary()
    {
      if (!_isDataLayerInitialized)
      {
        _dataDictionary = _dataLayer.GetDictionary();
        _kernel.Bind<DataDictionary>().ToConstant(_dataDictionary);
        _isDataLayerInitialized = true;
      }
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

    private Response Refresh(string graphName)
    {
      _semanticEngine = _kernel.Get<ISemanticLayer>("dotNetRDF");

      _projectionEngine = _kernel.Get<IProjectionLayer>("rdf");

      LoadDataObjectSet(graphName, null);

      XDocument rdf = _projectionEngine.ToXml(graphName, ref _dataObjects);

      return _semanticEngine.Refresh(graphName, rdf);
    }

    private long LoadDataObjectSet(string graphName, IList<string> identifiers)
    {
      _graphMap = _mapping.FindGraphMap(graphName);

      _dataObjects.Clear();

      if (identifiers != null)
        _dataObjects = _dataLayer.Get(_graphMap.dataObjectName, identifiers);
      else
        _dataObjects = _dataLayer.Get(_graphMap.dataObjectName, null);

      return _dataObjects.Count;
    }

    private long LoadDataObjectSet(string graphName, DataFilter dataFilter, int start, int limit)
    {
      _graphMap = _mapping.FindGraphMap(graphName);

      _dataObjects.Clear();

      if (dataFilter != null)
        _dataObjects = _dataLayer.Get(_graphMap.dataObjectName, dataFilter, limit, start);
      else
        _dataObjects = _dataLayer.Get(_graphMap.dataObjectName, null);

      long count = _dataLayer.GetCount(_graphMap.dataObjectName, dataFilter);

      return count;
    }

    //private void DeleteScope()
    //{
    //  try
    //  {
    //    // clean up ScopeList
    //    foreach (ScopeProject project in _scopes)
    //    {
    //      if (project.Name.ToUpper() == _settings["ProjectName"].ToUpper())
    //      {
    //        foreach (ScopeApplication application in project.Applications)
    //        {
    //          if (application.Name.ToUpper() == _settings["ApplicationName"].ToUpper())
    //          {
    //            project.Applications.Remove(application);
    //          }
    //          break;
    //        }
    //        break;
    //      }
    //    }

    //    // save ScopeList
    //    Utility.Write<ScopeProjects>(_scopes, _settings["ScopesPath"], true);

    //    // delete its bindings
    //    File.Delete(_settings["BindingConfigurationPath"]);
    //  }
    //  catch (Exception ex)
    //  {
    //    _logger.Error(string.Format("Error in DeleteScope: {0}", ex));
    //    throw ex;
    //  }
    //}

    private IList<IDataObject> CreateDataObjects(string graphName, string dataObjectsString)
    {
      IList<IDataObject> dataObjects = new List<IDataObject>();
      dataObjects = _dataLayer.Create(graphName, null);

      if (dataObjectsString != null && dataObjectsString != String.Empty)
      {
        XmlReader reader = XmlReader.Create(new StringReader(dataObjectsString));
        XDocument file = XDocument.Load(reader);
        file = Utility.RemoveNamespace(file);

        var dtoResults = from c in file.Elements("ArrayOf" + graphName).Elements(graphName) select c;
        int j = 0;
        foreach (var dtoResult in dtoResults)
        {
          var dtoProperties = from c in dtoResult.Elements("Properties").Elements("Property") select c;
          IDataObject dto = dataObjects[j];
          j++;
          foreach (var dtoProperty in dtoProperties)
          {
            dto.SetPropertyValue(dtoProperty.Attribute("name").Value, dtoProperty.Attribute("value").Value);
          }
          dataObjects.Add(dto);
        }
      }
      return dataObjects;
    }


    #endregion

    #region data layer management methods
    public DataLayers GetDataLayers()
    {
      DataLayers dataLayers = new DataLayers();
      
      try
      {
        if (File.Exists(_dataLayersBindingConfiguration))
        {
          dataLayers = Utility.Read<DataLayers>(_dataLayersBindingConfiguration);
          int dataLayersCount = dataLayers.Count;

          //
          // validate external data layers, remove from list if no longer exists
          //
          for (int i = 0; i < dataLayers.Count; i++)
          {
            DataLayer dataLayer = dataLayers[i];

            if (dataLayer.External)
            {
              string qualPath = dataLayer.Path + "\\" + dataLayer.MainDLL;

              if (!File.Exists(qualPath))
              {
                dataLayers.RemoveAt(i--);
              }
            }
          }

          if (dataLayersCount > dataLayers.Count)
          {
            Utility.Write<DataLayers>(dataLayers, _dataLayersBindingConfiguration);
          }
        }
        else
        {
          DataLayers internalDataLayers = GetInternalDataLayers();

          if (internalDataLayers != null && internalDataLayers.Count > 0)
          {
            dataLayers.AddRange(internalDataLayers);
          }

          Utility.Write<DataLayers>(dataLayers, _dataLayersBindingConfiguration);
        }
      }
      catch (Exception e)
      {
        _logger.Error("Error getting data layers: " + e);
        throw e;
      }

      return dataLayers;
    }

    public Response PostDataLayer(DataLayer dataLayer)
    {
      Response response = new Response();
      response.Level = StatusLevel.Success;
      
      try
      {
        DataLayers dataLayers = GetDataLayers();
        DataLayer dl = dataLayers.Find(x => x.Name.ToLower() == dataLayer.Name.ToLower());
        dataLayer.Path = _settings["DataLayersPath"] + dataLayer.Name + "\\";

        // extract package file
        if (dataLayer.Package != null)
        {
          try
          {
            Utility.Unzip(dataLayer.Package, dataLayer.Path);
            dataLayer.Package = null;
          }
          catch (UnauthorizedAccessException e)
          {
            _logger.Warn("Error extracting data layer package: " + e);
            response.Level = StatusLevel.Warning;
          }
        }

        // validate data layer
        Assembly dataLayerAssembly = GetDataLayerAssembly(dataLayer);
        if (dataLayerAssembly == null)
        {
          throw new Exception("Unable to load data layer assembly.");
        }

        dataLayer.Assembly = GetDataLayerAssemblyName(dataLayerAssembly);
        dataLayer.External = true;

        if (!string.IsNullOrEmpty(dataLayer.Assembly))
        {
          if (dl == null)  // data layer does not exist, add it
          {
            dataLayers.Add(dataLayer);
          }
          else  // data layer already exists, update it
          {
            dl = dataLayer;
          }

          Utility.Write<DataLayers>(dataLayers, _dataLayersBindingConfiguration);
          response.Messages.Add("Data layer [" + dataLayer.Name + "] saved successfully.");
        }
        else
        {
          if (Directory.Exists(dataLayer.Path))
          {
            Directory.Delete(dataLayer.Path, true);
          }
          
          response.Level = StatusLevel.Error;
          response.Messages.Add("Data layer [" + dataLayer.Name + "] is not compatible.");
        }
      }
      catch (Exception e)
      {
        _logger.Error("Error saving data layer: " + e);

        if (Directory.Exists(dataLayer.Path))
        {
          Directory.Delete(dataLayer.Path, true);
        }

        response.Level = StatusLevel.Error;
        response.Messages.Add("Error adding data layer [" + dataLayer.Name + "]. " + e);
      }

      return response;
    }

    public Response DeleteDataLayer(string dataLayerName)
    {
      Response response = new Response();

      try
      {
        DataLayers dataLayers = GetDataLayers();
        DataLayer dl = dataLayers.Find(x => x.Name.ToLower() == dataLayerName.ToLower());

        if (dl == null)
        {
          response.Level = StatusLevel.Error;
          response.Messages.Add("Data layer [" + dataLayerName + "] not found.");
        }
        else
        {
          if (dl.External)
          {            
            dataLayers.Remove(dl);
            Utility.Write<DataLayers>(dataLayers, _dataLayersBindingConfiguration);

            string dlPath = dl.Path;
            Directory.Delete(dlPath, true);

            response.Level = StatusLevel.Success;
            response.Messages.Add("Data layer [" + dataLayerName + "] deleted successfully.");
          }
          else
          {
            response.Level = StatusLevel.Error;
            response.Messages.Add("Deleting internal data layer [" + dataLayerName + "] is not allowed.");
          }
        }
      }
      catch (Exception e)
      {
        _logger.Error("Error getting data layers: " + e);

        response.Level = StatusLevel.Success;
        response.Messages.Add("Error deleting data layer [" + dataLayerName + "]." + e);
      }

      return response;
    }

    private DataLayers GetInternalDataLayers()
    {
      DataLayers dataLayers = new DataLayers();

      // load NHibernate data layer
      Type type = typeof(NHibernateDataLayer);
      string library = type.Assembly.GetName().Name;
      string assembly = string.Format("{0}, {1}", type.FullName, library);
      DataLayer dataLayer = new DataLayer { Assembly = assembly, Name = library, Configurable = true };
      dataLayers.Add(dataLayer);

      // load Spreadsheet data layer
      type = typeof(SpreadsheetDataLayer);
      library = type.Assembly.GetName().Name;
      assembly = string.Format("{0}, {1}", type.FullName, library);
      dataLayer = new DataLayer { Assembly = assembly, Name = library, Configurable = true };
      dataLayers.Add(dataLayer);

      return dataLayers;
    }

    private Assembly GetDataLayerAssembly(DataLayer dataLayer)
    {      
      byte[] bytes = Utility.GetBytes(dataLayer.Path + dataLayer.MainDLL);
      return Assembly.Load(bytes);
    }

    private string GetDataLayerAssemblyName(Assembly assembly)
    {
      Type dlType = typeof(IDataLayer);
      Type[] asmTypes = assembly.GetTypes();

      if (asmTypes != null)
      {
        foreach (System.Type asmType in asmTypes)
        {
          if (dlType.IsAssignableFrom(asmType) && !(asmType.IsInterface || asmType.IsAbstract))
          {
            bool configurable = asmType.BaseType.Equals(typeof(BaseConfigurableDataLayer));
            string name = assembly.FullName.Split(',')[0];

            return string.Format("{0}, {1}", asmType.FullName, name);
          }
        }
      }

      return string.Empty;
    }

    private Assembly DataLayerAssemblyResolveEventHandler(object sender, ResolveEventArgs args)
    {
      if (args.Name.Contains(".resources,")) 
      { 
        return null; 
      }

      if (Directory.Exists(_settings["DataLayerPath"]))
      {
        string[] files = Directory.GetFiles(_settings["DataLayerPath"]);

        foreach (string file in files)
        {
          if (file.ToLower().EndsWith(".dll") || file.ToLower().EndsWith(".exe"))
          {
            AssemblyName asmName = AssemblyName.GetAssemblyName(file);

            if (args.Name.StartsWith(asmName.Name))
            {
              byte[] bytes = Utility.GetBytes(file);
              return Assembly.Load(bytes);
            }
          }
        }

        _logger.Error("Unable to resolve assembly [" + args.Name + "].");
      }

      return null;
    }
    #endregion data layer management methods

    public void SetScopes(Resource importScopes)
    {
      _scopes = importScopes;
    }

    public Response Configure(string projectName, string applicationName, HttpRequest httpRequest)
    {
      Response response = new Response();
      response.Messages = new Messages();
      XElement binding;
      XElement configuration = null;
      try
      {
        string savedFileName = string.Empty;

        foreach (string file in httpRequest.Files)
        {
          HttpPostedFile hpf = httpRequest.Files[file] as HttpPostedFile;
          if (hpf.ContentLength == 0)
            continue;
          hpf.InputStream.Position = 0;

          savedFileName = Path.Combine(
          AppDomain.CurrentDomain.BaseDirectory, _settings["AppDataPath"],
          Path.GetFileName(hpf.FileName));
          hpf.SaveAs(savedFileName);
          hpf.InputStream.Flush();
        }

        InitializeScope(projectName, applicationName, false);
        //XElement bindingConfig = Utility.ReadXml(_settings["BindingConfigurationPath"]);
        
        //string dataLayer = httpRequest.Form["DataLayer"];
        // Check request whether have Configuration in Request or not. SP & ID don't have this ----------------------
        if (httpRequest.Form["Configuration"] != null)
        {
          configuration = Utility.DeserializeXml<XElement>(httpRequest.Form["Configuration"]);

          //binding = new XElement("module",
          //   new XAttribute("name", _settings["Scope"]),
          //     new XElement("bind",
          //       new XAttribute("name", "DataLayer"),
          //       new XAttribute("service", "org.iringtools.library.IDataLayer, iRINGLibrary"),
          //       new XAttribute("to", dataLayer)
          //     )
          //   );


          //bindingConfig.Save(_settings["BindingConfigurationPath"]);
          //try
          //{
          //  _kernel.Load(_settings["BindingConfigurationPath"]);
          //}
          //catch
          //{
          //  ///ignore error if already loaded
          //  ///this is required for Spreadsheet Datalayer 
          //  ///when spreadsheet is re-uploaded
          //}
        }
        InitializeDataLayer(false);
        if (httpRequest.Form["Configuration"] != null)
        {

          response = ((IDataLayer2)_dataLayer).Configure(configuration);
        }

        InitializeDictionary();

      }
      catch (Exception ex)
      {
        response.Messages.Add(String.Format("Failed to Upload Files[{0}]", _settings["Scope"]));
        response.Messages.Add(ex.Message);
        response.Level = StatusLevel.Error;
      }
      return response;
    }
    
    public DocumentBytes GetResourceData(string projectName, string applicationName)
    {
      try
      {
        InitializeScope(projectName, applicationName);
        InitializeDataLayer();

        return _dataLayer.GetResourceData();
      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Error in GetConfiguration: {0}", ex));
        throw new Exception(string.Format("Error getting configuration: {0}", ex));
      }
    }

    public XElement GetConfiguration(string projectName, string applicationName)
    {
      try
      {
        InitializeScope(projectName, applicationName);
        InitializeDataLayer();

        return _dataLayer.GetConfiguration();
      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Error in GetConfiguration: {0}", ex));
        throw new Exception(string.Format("Error getting configuration: {0}", ex));
      }
    }

    public Response RefreshDataObjects(string projectName, string applicationName)
    {
      try
      {
        InitializeScope(projectName, applicationName);
        InitializeDataLayer();

        return _dataLayer.RefreshAll();
      }
      catch (Exception ex)
      {
        string errMsg = String.Format("Error refreshing data objects: {0}", ex);
        _logger.Error(errMsg);
        throw new Exception(errMsg);
      }
    }

    public Response RefreshDataObject(string projectName, string applicationName, string dataObject)
    {
      try
      {
        InitializeScope(projectName, applicationName);
        InitializeDataLayer();

        return _dataLayer.Refresh(dataObject);
      }
      catch (Exception ex)
      {
        string errMsg = String.Format("Error refreshing data object [{0}]: {1}", dataObject, ex);
        _logger.Error(errMsg);
        throw new Exception(errMsg);
      }
    }

    public IList<Object> GetSummary(String projectName, String applicationName)
    {
      InitializeScope(projectName, applicationName);
      InitializeDataLayer();

      if (!_dataDictionary.enableSummary)
        throw new WebFaultException(HttpStatusCode.NotFound);

      return _dataLayer.GetSummary();
    }

    public void FormatOutgoingMessage<T>(T graph, string format, bool useDataContractSerializer)
    {
      if (format.ToUpper() == "JSON")
      {
        string json = Utility.SerializeJson<T>(graph, useDataContractSerializer);

        HttpContext.Current.Response.ContentType = "application/json; charset=utf-8";
        HttpContext.Current.Response.Write(json);
      }
      else
      {
        string xml = Utility.Serialize<T>(graph, useDataContractSerializer);

        HttpContext.Current.Response.ContentType = "application/xml";
        HttpContext.Current.Response.Write(xml);
      }
    }

    public void FormatOutgoingMessage(XElement xElement, string format)
    {
      if (format == null)
      {
        format = String.Empty;
      }

      if (format.ToUpper() == "HTML")
      {
        HttpContext.Current.Response.ContentType = "text/html";
        HttpContext.Current.Response.Write(xElement.ToString());
      }
      else if (format.ToUpper() == "JSON")
      {
        byte[] json = new byte[0];

        if (xElement != null)
        {
          DataItems dataItems = Utility.DeserializeDataContract<DataItems>(xElement.ToString());
          DataItemSerializer serializer = new DataItemSerializer(
            _settings["JsonIdField"], _settings["JsonLinksField"], bool.Parse(_settings["DisplayLinks"]));
          MemoryStream ms = serializer.SerializeToMemoryStream<DataItems>(dataItems, false);
          json = ms.ToArray();
          ms.Close();
        }

        HttpContext.Current.Response.ContentType = "application/json; charset=utf-8";
        HttpContext.Current.Response.Write(Encoding.UTF8.GetString(json, 0, json.Length));
      }
      else
      {
        HttpContext.Current.Response.ContentType = "application/xml";
        HttpContext.Current.Response.Write(xElement.ToString());
      }
    }

    public void FormatOutgoingMessage(object content, string format)
    {
      if (typeof(IContentObject).IsInstanceOfType(content))
      {
        IContentObject contentObject = (IContentObject)content;
        HttpContext.Current.Response.ContentType = contentObject.contentType;
        HttpContext.Current.Response.BinaryWrite(contentObject.content.ToMemoryStream().GetBuffer());
      }
      else if (content.GetType() == typeof(XDocument))
      {
        XDocument xDoc = (XDocument)content;
        FormatOutgoingMessage(xDoc.Root, format);
      }
      else
      {
        throw new Exception("Invalid response type from DataLayer.");
      }
    }
  }
}