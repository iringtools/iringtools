using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Xml.Linq;
using System.Web;
using Ninject;
using log4net;
using org.iringtools.library;
using org.iringtools.utility;
using org.iringtools.mapping;
using iRINGTools.Web.Helpers;
using System.Text;
using System.Collections;
using System.Net;
using System.IO;


namespace iRINGTools.Web.Models
{
  public class AdapterRepository : IAdapterRepository
  {
    private ServiceSettings _settings = null;
    private WebHttpClient _adapterServiceClient = null;
    private WebHttpClient _hibernateServiceClient = null;
    private WebHttpClient _referenceDataServiceClient = null;
    private WebHttpClient _javaServiceClient = null;
    private string proxyHost = "";
    private string proxyPort = "";
    private WebProxy webProxy = null;
    private static readonly ILog _logger = LogManager.GetLogger(typeof(AdapterRepository));
    private static Dictionary<string, NodeIconCls> nodeIconClsMap;
    private string combinationMsg = null;
    private string adapterServiceUri = "";
    private string hibernateServiceUri = "";
    private string referenceDataServiceUri = "";

    [Inject]
    public AdapterRepository()
    {
      NameValueCollection settings = ConfigurationManager.AppSettings;
      _settings = new ServiceSettings();
      _settings.AppendSettings(settings);
      #region initialize webHttpClient for converting old mapping
      proxyHost = _settings["ProxyHost"];
      proxyPort = _settings["ProxyPort"];
      adapterServiceUri = _settings["AdapterServiceUri"];
      string javaCoreUri = _settings["JavaCoreUri"];
      hibernateServiceUri = _settings["NHibernateServiceUri"];
      referenceDataServiceUri = _settings["ReferenceDataServiceUri"];
      SetNodeIconClsMap();

      if (!String.IsNullOrEmpty(proxyHost) && !String.IsNullOrEmpty(proxyPort))
      {
        webProxy = _settings.GetWebProxyCredentials().GetWebProxy() as WebProxy;
        _javaServiceClient = new WebHttpClient(javaCoreUri, null, webProxy);
        _adapterServiceClient = new WebHttpClient(adapterServiceUri, null, webProxy);
        _hibernateServiceClient = new WebHttpClient(hibernateServiceUri, null, webProxy);
        _referenceDataServiceClient = new WebHttpClient(referenceDataServiceUri, null, webProxy);
      }
      else
      {
        _javaServiceClient = new WebHttpClient(javaCoreUri);
        _adapterServiceClient = new WebHttpClient(adapterServiceUri);
        _hibernateServiceClient = new WebHttpClient(hibernateServiceUri);
        _referenceDataServiceClient = new WebHttpClient(referenceDataServiceUri);
      }
      #endregion
    }

    public WebHttpClient getServiceClient(string uri, string serviceName)
    {
      getSetting();
      WebHttpClient _newServiceClient = null;
      string serviceUri = uri + "/" + serviceName;

      if (!String.IsNullOrEmpty(proxyHost) && !String.IsNullOrEmpty(proxyPort))
      {
        _newServiceClient = new WebHttpClient(serviceUri, null, webProxy);
      }
      else
      {
        _newServiceClient = new WebHttpClient(serviceUri);       
      }
      return _newServiceClient;
    }

    public Resources GetResource(String user)
    {
      Resources resources = null;      

      try
      {
        resources = _javaServiceClient.Get<Resources>("/directory/resources", true);
        HttpContext.Current.Session[user + "." + "resources"] = resources; 
      }
      catch (Exception ex)
      {
        _logger.Error(ex.ToString());
      }

      return resources;
    }

    public Directories GetScopes()
    {
      _logger.Debug("In AdapterRepository GetScopes");
      Directories obj = null;     

      try
      {
        obj = _javaServiceClient.Get<Directories>("/directory", true);             
        _logger.Debug("Successfully called Adapter.");
      }
      catch (Exception ex)
      {
        _logger.Error(ex.ToString());
      }

      return obj;
    }

    public string GetNodeIconCls(string type)
    {
      try
      {
        switch (nodeIconClsMap[type.ToLower()])
        {
          case NodeIconCls.folder: return "folder";
          case NodeIconCls.project: return "treeProject";
          case NodeIconCls.application: return "application";
          case NodeIconCls.resource: return "resource";
          case NodeIconCls.key: return "treeKey";
          case NodeIconCls.property: return "treeProperty";
          case NodeIconCls.relation: return "treeRelation";
          default: return "folder";
        }
      }
      catch (Exception)
      {
        return "folder";
      }
    }

    public Tree GetDirectoryTree(string user)
    {
      _logger.Debug("In ScopesNode case block");
      Directories directory = null;
      Resources resources = null;

      string _key = user + "." + "directory";
      string _resource = user + "." + "resource";
      directory = GetScopes();
      HttpContext.Current.Session[_key] = directory; 
      resources = GetResource(user);

      Tree tree = null;
      string context = "";
      string treePath = "";

      if (directory != null)
      {
        tree = new Tree();
        List<JsonTreeNode> folderNodes = tree.getNodes();

        foreach (Folder folder in directory)
        {
          TreeNode folderNode = new TreeNode();
          folderNode.text = folder.Name;
          folderNode.id = folder.Name;
          folderNode.identifier = folderNode.id;
          folderNode.hidden = false;
          folderNode.leaf = false;
          folderNode.iconCls = GetNodeIconCls(folder.Type);
          folderNode.type = "folder";
          treePath = folder.Name;

          if (folder.Context != null)
            context = folder.Context;

          Object record = new
          {
            Name = folder.Name,
            context = context,
            Description = folder.Description,
            securityRole = folder.SecurityRole
          };

          folderNode.record = record;
          folderNode.property = new Dictionary<string, string>();
          folderNode.property.Add("Name", folder.Name);
          folderNode.property.Add("Description", folder.Description);
          folderNode.property.Add("Context", folder.Context);
          folderNode.property.Add("User", folder.User);
          folderNodes.Add(folderNode);
          TraverseDirectory(folderNode, folder, treePath);
        }
      }
     
      return tree;
    }

    public XElement GetBinding(string context, string endpoint, string baseUrl)
    {
      XElement obj = null;

      try
      {
        WebHttpClient _newServiceClient = PrepareServiceClient(baseUrl, "adapter");
        obj = _newServiceClient.Get<XElement>(String.Format("/{0}/{1}/binding", context, endpoint), true);
      }
      catch (Exception ex)
      {
        _logger.Error(ex.ToString());
      }

      return obj;
    }

    public string TestBaseUrl(string baseUrl)
    {
      string obj = null;

      try
      {
        WebHttpClient _newServiceClient = PrepareServiceClient(baseUrl, "adapter");
        obj = _newServiceClient.Get<string>("/test");
      }
      catch (Exception ex)
      {
        _logger.Error(ex.ToString());
        return "ERROR";
      }

      return obj;
    }

    public string PostScopes(Directories scopes)
    {
      string obj = null;

      try
      {
        obj = _javaServiceClient.Post<Directories>("/directory", scopes, true);
        _logger.Debug("Successfully called Adapter.");
      }
      catch (Exception ex)
      {
        _logger.Error(ex.ToString());
      }

      return obj;
    }

    
    public DataLayers GetDataLayers(string baseUrl)
    {
      DataLayers obj = null;

      try
      {
        WebHttpClient _newServiceClient = PrepareServiceClient(baseUrl, "adapter");
        obj = _newServiceClient.Get<DataLayers>("/datalayers");
      }
      catch (Exception ex)
      {
        _logger.Error(ex.ToString());
      }

      return obj;
    }

    public Entity GetClassLabel(string classId)
    {
      Entity entity = new Entity();
      try
      {
        WebHttpClient _tempClient = new WebHttpClient(_settings["ReferenceDataServiceUri"]);
        entity = _tempClient.Get<Entity>(String.Format("/classes/{0}/label", classId), true);
      }
      catch (Exception ex)
      {
        _logger.Error(ex.ToString());
      }
      return entity;
    }

    public DataDictionary GetDictionary(string contextName, string endpoint, string baseUrl)
    {
      DataDictionary obj = null;

      try
      {
        WebHttpClient _newServiceClient = PrepareServiceClient(baseUrl, "adapter");
        obj = _newServiceClient.Get<DataDictionary>(String.Format("/{0}/{1}/dictionary", contextName, endpoint), true);
      }
      catch (Exception ex)
      {
        _logger.Error(ex.ToString());
      }

      return obj;
    }

    public Mapping GetMapping(string contextName, string endpoint, string baseUrl)
    {
      Mapping obj = null;

      try
      {
        WebHttpClient _newServiceClient = PrepareServiceClient(baseUrl, "adapter");
        obj = _newServiceClient.Get<Mapping>(String.Format("/{0}/{1}/mapping", contextName, endpoint), true);
      }
      catch (Exception ex)
      {
        _logger.Error(ex.ToString());
      }

      return obj;
    }

    public string UpdateBinding(string scope, string application, string dataLayer, string baseUrl)
    {
      string obj = null;

      try
      {
        XElement binding = new XElement("module",
            new XAttribute("name", string.Format("{0}.{1}", scope, application)),
            new XElement("bind",
            new XAttribute("name", "DataLayer"),
            new XAttribute("service", "org.iringtools.library.IDataLayer, iRINGLibrary"),
            new XAttribute("to", dataLayer)
          )
        );

        WebHttpClient _newServiceClient = PrepareServiceClient(baseUrl, "adapter");
        obj = _newServiceClient.Post<XElement>(String.Format("/{0}/{1}/binding", scope, application), binding, true);

      }
      catch (Exception ex)
      {
        _logger.Error(ex.ToString());
      }

      return obj;
    }

    public string GetRootSecurityRole()
    {
      string rootSecurityRole = "";

      try
      {
        rootSecurityRole = _javaServiceClient.GetMessage("/directory/security");
        _logger.Debug("Successfully called Adapter.");
      }
      catch (Exception ex)
      {
        _logger.Error(ex.ToString());
      }

      return rootSecurityRole;
    }

    public string GetUserLdap()
    {
      return _javaServiceClient.GetMessage("/directory/userldap");
    }

    public Urls GetEndpointBaseUrl(string user)
    {
      bool ifExit = false;
      Urls baseUrls = null;

      if (HttpContext.Current.Session[user + ".baseUrlList"] != null)
        baseUrls = (Urls)HttpContext.Current.Session[user + ".baseUrlList"];
      else
      {
        try
        {
          baseUrls = _javaServiceClient.Get<Urls>("/directory/baseUrls", true);
          HttpContext.Current.Session[user + ".baseUrlList"] = baseUrls;
          _logger.Debug("Successfully called Adapter.");
        }
        catch (Exception ex)
        {
          _logger.Error(ex.ToString());
        }
      }        

      string baseUri = _adapterServiceClient.GetBaseUri();


      foreach (Url baseUrl in baseUrls)
      {
        if (baseUrl.Urlocator.ToLower().Equals(baseUri.ToLower()))
          ifExit = true;
      }

      if (!ifExit)
      {
        Url newBaseUrl = new Url { Urlocator = baseUri };
        baseUrls.Add(newBaseUrl);
      }

      return baseUrls;
    }

    public ContextNames GetFolderContexts(string user)
    {
      ContextNames contextNames = null;
      if (HttpContext.Current.Session[user + ".contextList"] != null)
        contextNames = (ContextNames)HttpContext.Current.Session[user + ".contextList"];
      else
      {
        try
        {
          contextNames = _javaServiceClient.Get<ContextNames>("/directory/contextNames", true);
          HttpContext.Current.Session[user + ".contextList"] = contextNames;
          _logger.Debug("Successfully called Adapter.");
        }
        catch (Exception ex)
        {
          _logger.Error(ex.ToString());
        }
      }

      return contextNames;
    }

    public string Folder(string newFolderName, string description, string path, string state, string context, string oldContext, string user)
    {
      string obj = null;

      if (context == "")
        context = "0";

      if (state.Equals("new"))
      {
        if (path != "")
          path = path + '.' + newFolderName;
        else
          path = newFolderName;        
      }
     
      path = path.Replace('/', '.');       

      try
      {
        if (!state.Equals("new"))        
          CheckCombination(path, context, oldContext, user);

        Resources resources = (Resources)HttpContext.Current.Session[user + ".resources"];
        obj = _javaServiceClient.PostMessage(string.Format("/directory/folder/{0}/{1}/{2}/{3}", path, newFolderName, "folder", context), description, true);

        if (state != "new" && !context.Equals(oldContext))
        {
          Folder folder = PrepareFolder(user, path);

          if (folder != null)
            obj = UpdateFolders(folder, context, resources, oldContext);          
        }

        _logger.Debug("Successfully called Adapter and Java Directory Service.");    
        ClearDirSession(user);        
      }
      catch (Exception ex)
      {
        _logger.Error(ex.ToString());
        obj = "ERROR";
      }      

      return obj;
    }

    public string Endpoint(string newEndpointName, string path, string description, string state, string context, string oldAssembly, string newAssembly, string baseUrl, string oldBaseUrl, string key)
    {
      string obj = "";
      Locator scope = null;
      EndpointApplication application = null;
      string endpointName = null;
      Resource resourceOld = null;
      Resource resourceNew = null;
      bool createApp = false;

      string baseUri = CleanBaseUrl(baseUrl, '.');
      if (state.Equals("new"))
      {
        path = path + '/' + newEndpointName;
        endpointName = newEndpointName;
        oldAssembly = newAssembly;
        oldBaseUrl = baseUrl;
      }
      else
      {
        endpointName = path.Substring(path.LastIndexOf('/') + 1);
      }

      path = path.Replace('/', '.');      

      try
      {
        WebHttpClient _newServiceClient = PrepareServiceClient(baseUrl, "adapter");
        CheckeCombination(baseUrl, oldBaseUrl, context, context, newEndpointName, endpointName, path, key);
        Resources resourcesOld = (Resources)HttpContext.Current.Session[key + ".resources"];
        obj = _javaServiceClient.PostMessage(string.Format("/directory/endpoint/{0}/{1}/{2}/{3}/{4}", path, newEndpointName, "endpoint", baseUri.Replace('/', '.'), newAssembly), description, true);
        Resources resourcesNew = GetResource(key); 

        
        if (!state.Equals("new"))
        {
          if (newAssembly.ToLower() == oldAssembly.ToLower() && newEndpointName.ToLower() == endpointName.ToLower())
            return "";
          
          resourceOld = FindResource(CleanBaseUrl(baseUrl, '/'), resourcesOld); 
            
          if (resourceOld != null)
          {
            scope = resourceOld.Locators.FirstOrDefault<Locator>(o => o.Context.ToLower() == context.ToLower());
            application = scope.Applications.FirstOrDefault<EndpointApplication>(o => o.Endpoint.ToLower() == endpointName.ToLower());
            if (application == null)
              createApp = true;
          }
          else
            createApp = true;

          if (createApp)
          {
            application = new EndpointApplication()
            {
              Endpoint = endpointName,
              Description = description,
              Assembly = oldAssembly
            };
          }
          
          obj = _newServiceClient.Post<EndpointApplication>(String.Format("/scopes/{0}/apps/{1}", context, newEndpointName), application, true);
        }
        else if (state.Equals("new"))
        {
          resourceNew = FindResource(CleanBaseUrl(baseUrl, '.'), resourcesNew);           

          if (resourceNew != null)
          {
            scope = resourceNew.Locators.FirstOrDefault<Locator>(o => o.Context.ToLower() == context.ToLower());
            application = scope.Applications.FirstOrDefault<EndpointApplication>(o => o.Endpoint.ToLower() == newEndpointName.ToLower());
          }
          else
          {
            application = new EndpointApplication()
            {
              Endpoint = newEndpointName,
              Description = description,
              Assembly = newAssembly
            };
          }

          obj = _newServiceClient.Post<EndpointApplication>(String.Format("/scopes/{0}/apps", context), application, true);          
        }

        _logger.Debug("Successfully called Adapter and Java Directory Service.");
        ClearDirSession(key);
      }
      catch (Exception ex)
      {
        _logger.Error(ex.Message.ToString());
        obj = "ERROR";
      }     

      return obj;
    }

    public string DeleteEntry(string path, string type, string context, string baseUrl, string user)
    {
      string obj = null;     

      string name = null;
      path = path.Replace('/', '.');
      Locator scope = null;
      EndpointApplication application = null;

      try
      {
        Resources resources = (Resources)HttpContext.Current.Session[user + ".resources"];
        name = path.Substring(path.LastIndexOf('.') + 1);                  

        if (type.Equals("endpoint"))
        {
          Resource resource = FindResource(CleanBaseUrl(baseUrl, '/'), resources);
          scope = resource.Locators.FirstOrDefault<Locator>(o => o.Context.ToLower() == context.ToLower());
          application = scope.Applications.FirstOrDefault<EndpointApplication>(o => o.Endpoint.ToLower() == name.ToLower());

          WebHttpClient _newServiceClient = PrepareServiceClient(baseUrl, "adapter");
          obj = _newServiceClient.Post<EndpointApplication>(String.Format("/scopes/{0}/delete", context), application, true);
        }
        else if (type.Equals("folder"))
        {
          Folder folder = PrepareFolder(user, path);

          if (folder != null)
            DeleteFolders(folder, context, resources);          
        }

        obj = _javaServiceClient.Post<String>(String.Format("/directory/{0}", path), "", true);
        _logger.Debug("Successfully called Adapter.");      
        ClearDirSession(user);
      }
      catch (Exception ex)
      {
        _logger.Error(ex.Message.ToString());
      }

      return obj;
    }

    public Response RegenAll(string user)
    {
      Response totalObj = new Response();
      string _key = user + "." + "directory";
      Directories directory = null;
      if (HttpContext.Current.Session[_key] != null)      
        directory = (Directories)HttpContext.Current.Session[_key];

      foreach (Folder folder in directory)
      {
        GenerateFolders(folder, totalObj);
      }
      return totalObj;      
    }
    
    public string GetCombinationMsg()
    {
      return combinationMsg;
    }

    public Response SaveDataLayer(MemoryStream dataLayerStream)
    {
      try
      {
        return Utility.Deserialize<Response>(_adapterServiceClient.PostStream("/datalayers", dataLayerStream), true);
      }
      catch (Exception ex)
      {
        _logger.Error(ex.Message);

        Response response = new Response()
        {
          Level = StatusLevel.Error,
          Messages = new Messages { ex.Message }
        };

        return response;
      }
    }
    
    #region Private methods for Directory 

    static MemoryStream CreateDataLayerStream(string name, string mainDLL, string path)
    {
      DataLayer dataLayer = new DataLayer()
      {
        Name = name,
        MainDLL = mainDLL,
        Package = Utility.Zip(path),
      };

      MemoryStream dataLayerStream = new MemoryStream();
      DataContractSerializer serializer = new DataContractSerializer(typeof(DataLayer));
      
      serializer.WriteObject(dataLayerStream, dataLayer);
      dataLayerStream.Position = 0;

      return dataLayerStream;
    }

    private WebHttpClient PrepareServiceClient(string baseUrl, string serviceName)
    {
      if (baseUrl == "" || baseUrl == null)
        return _adapterServiceClient;

      string baseUri = CleanBaseUrl(baseUrl.ToLower(), '/');
      string adapterBaseUri = CleanBaseUrl(adapterServiceUri.ToLower(), '/');

      if (!baseUri.Equals(adapterBaseUri))
        return getServiceClient(baseUrl, serviceName);
      else
        return _adapterServiceClient;
    }

    private Response GenerateFolders(Folder folder, Response totalObj)
    {
      Response obj = null;
      Endpoints endpoints = folder.Endpoints;      

      if (endpoints != null)
      {
        foreach (Endpoint endpoint in endpoints)
        {
          WebHttpClient _newServiceClient = PrepareServiceClient(endpoint.BaseUrl, "adapter");
          obj = _newServiceClient.Get<Response>(String.Format("/{0}/{1}/generate", endpoint.Context, endpoint.Name));
          totalObj.Append(obj);          
        }
      }

      Folders subFolders = folder.Folders;

      if (subFolders == null)
        return totalObj;
      else
      {
        foreach (Folder subFolder in subFolders)
        {
          obj = GenerateFolders(subFolder, totalObj);
        }
      }

      return totalObj;
    }

    private Folder PrepareFolder(string user, string path)
    {
      string _key = user + "." + "directory";
      if (HttpContext.Current.Session[_key] != null)
      {
        Directories directory = (Directories)HttpContext.Current.Session[_key];
        return FindFolder(directory, path);        
      }
      return null;
    }

    private void getSetting()
    {
      if (_settings == null)
        _settings = new ServiceSettings();
      
      proxyHost = _settings["ProxyHost"];
      proxyPort = _settings["ProxyPort"];
    }

    private string UpdateFolders(Folder folder, string context, Resources resources, String oldContext)
    {
      string obj = null;
      Endpoints endpoints = folder.Endpoints;
      Resource resource = null;

      if (endpoints != null)
      {
        foreach (Endpoint endpoint in folder.Endpoints)
        {
          resource = FindResource(endpoint.BaseUrl, resources);
          Locator scope = resource.Locators.FirstOrDefault<Locator>(o => o.Context.ToLower() == oldContext.ToLower());

          WebHttpClient _newServiceClient = PrepareServiceClient(endpoint.BaseUrl, "adapter");
          obj = _newServiceClient.Post<Locator>(string.Format("/scopes/{0}", context), scope, true);
        }
      }

      Folders subFolders = folder.Folders;

      if (subFolders == null)
        return null;
      else
      {
        foreach (Folder subFolder in subFolders)
        {
          obj = UpdateFolders(subFolder, context, resources, oldContext);
        }
      }

      return obj;
    }

    private string DeleteFolders(Folder folder, string context, Resources resources)
    {
      string obj = null;
      Endpoints endpoints = folder.Endpoints;    
      Resource resource = null;
      EndpointApplication application = null;

      Locator scope = null;

      if (endpoints != null)
      {
        foreach (Endpoint endpoint in endpoints)
        {
          resource = FindResource(endpoint.BaseUrl, resources);
          scope = resource.Locators.FirstOrDefault<Locator>(o => o.Context.ToLower() == context.ToLower());
          application = scope.Applications.FirstOrDefault<EndpointApplication>(o => o.Endpoint.ToLower() == endpoint.Name.ToLower());

          WebHttpClient _newServiceClient = PrepareServiceClient(endpoint.BaseUrl, "adapter");
          obj = _newServiceClient.Post<EndpointApplication>(String.Format("/scopes/{0}/delete", context), application, true);
        }
      }

      Folders subFolders = folder.Folders;

      if (subFolders == null)
        return null;
      else
      {
        foreach (Folder subFolder in subFolders)
        {
          obj = DeleteFolders(subFolder, context, resources);
        }
      }

      return obj;
    }

    private static void SetNodeIconClsMap()
    {
      nodeIconClsMap = new Dictionary<string, NodeIconCls>()
      {
  	    {"folder", NodeIconCls.folder},
  	    {"project", NodeIconCls.project},
  	    {"scope", NodeIconCls.scope},
  	    {"proj", NodeIconCls.project},
  	    {"application", NodeIconCls.application},
  	    {"app", NodeIconCls.application},
  	    {"resource", NodeIconCls.resource},
  	    {"resrc", NodeIconCls.resource}, 	
        {"key", NodeIconCls.key}, 
        {"property", NodeIconCls.property}, 
        {"relation", NodeIconCls.relation} 
      };
    }
    
    private void ClearDirSession(string user)
    {
      if (HttpContext.Current.Session[user + "." + "directory"] != null)
        HttpContext.Current.Session[user + "." + "directory"] = null;

      if (HttpContext.Current.Session[user + "." + "resource"] != null)
        HttpContext.Current.Session[user + "." + "resource"] = null;
    }

    private Resource FindResource(string baseUrl, Resources resources)
    {
      foreach (Resource rc in resources)
      {
        if (rc.BaseUrl.ToLower().Equals(baseUrl.ToLower()))
        {
          return rc;
        }
      }
      return null;
    }

    private string CleanBaseUrl(string url, char con)
    {
      try
      {
        System.Uri uri = new System.Uri(url);
        return uri.Scheme + ":" + con + con + uri.Host + ":" + uri.Port;
      }
      catch(Exception){}
      return null;
    }

    private void CheckeCombination(string baseUrl, string oldBaseUrl, string context, string oldContext, string endpointName, string oldEndpointName, string path, string user)
    {
      string _resource = user + ".resources";
      string lpath = "";
      Locator scope = null;

      if (HttpContext.Current.Session[_resource] != null)
      {
        Resources resources = (Resources)HttpContext.Current.Session[_resource];
        Resource resource = FindResource(CleanBaseUrl(oldBaseUrl, '/'), resources);

        if (resource != null)
          scope = resource.Locators.FirstOrDefault<Locator>(o => o.Context.ToLower() == context.ToLower());
        
        if (scope != null)
        {
          EndpointApplication application = scope.Applications.FirstOrDefault<EndpointApplication>(o => o.Endpoint.ToLower() == endpointName.ToLower());
          
          if (application != null && !application.Path.Replace("/", ".").Equals(path))
          {
            lpath = application.Path;
            combinationMsg = "The combination of (" + baseUrl.Replace(".", "/") + ", " + context + ", " + endpointName + ") at " + path.Replace(".", "/") + " is allready existed at " + lpath + ".";
            _logger.Error("Duplicated combination of baseUrl, context, and endpoint name");
            throw new Exception("Duplicated combination of baseUrl, context, and endpoint name");
          }
        }        
      }      
    }

    private void CheckCombination(Folder folder, string path, string context, string oldContext, string user)
    {
      Endpoints endpoints = folder.Endpoints;
      string endpointPath = "";
      string folderPath = "";

      if (endpoints != null)
      {
        foreach (Endpoint endpoint in endpoints)
        {
          endpointPath = path + "." + endpoint.Name;
          CheckeCombination(endpoint.BaseUrl, endpoint.BaseUrl, context, oldContext, endpoint.Name, endpoint.Name, endpointPath, user);
        }
      }

      Folders subFolders = folder.Folders;

      if (subFolders == null)
        return;
      else
      {
        foreach (Folder subFolder in subFolders)
        {
          folderPath = path + "." + subFolder.Name;
          CheckCombination(subFolder, folderPath, context, oldContext, user);
        }
      }
    }

    private void CheckCombination(string path, string context, string oldContext, string user)
    {
      string _key = user + "." + "directory";
      if (HttpContext.Current.Session[_key] != null)
      {
        Directories directory = (Directories)HttpContext.Current.Session[_key];
        Folder folder = FindFolder(directory, path);
        CheckCombination(folder, path, context, oldContext, user);
      }
    }

    private void GetLastName(string path, out string newpath, out string name)
    {
      int dotPos = path.LastIndexOf('.');

      if (dotPos < 0)
      {
        newpath = "";
        name = path;
      }
      else
      {
        newpath = path.Substring(0, dotPos);
        name = path.Substring(dotPos + 1, path.Length - dotPos - 1);
      }
    }

    private Folder FindFolder(List<Folder> scopes, string path)
    {
      string folderName, newpath;
      GetLastName(path, out newpath, out folderName);

      if (newpath == "")
      {
        foreach (Folder folder in scopes)
        {
          if (folder.Name == folderName)
            return folder;
        }
      }
      else
      {
        Folders folders = GetFolders(scopes, newpath);
        return folders.FirstOrDefault<Folder>(o => o.Name == folderName);
      }
      return null;
    }

    private Folders GetFolders(List<Folder> scopes, string path)
    {
      if (path == "")
        return (Folders)scopes;

      string[] level = path.Split('.');

      foreach (Folder folder in scopes)
      {
        if (folder.Name.Equals(level[0]))
        {
          if (level.Length == 1)
            return folder.Folders;
          else
            return TraverseGetFolders(folder, level, 0);
        }
      }
      return null;
    }

    private Folders TraverseGetFolders(Folder folder, string[] level, int depth)
    {
      if (folder.Folders == null)
      {
        folder.Folders = new Folders();
        return folder.Folders;
      }
      else
      {
        if (level.Length > depth + 1)
        {
          foreach (Folder subFolder in folder.Folders)
          {
            if (subFolder.Name == level[depth + 1])
              return TraverseGetFolders(subFolder, level, depth + 1);
          }
        }
        else
        {
          return folder.Folders;
        }
      }
      return null;
    }

    private void TraverseDirectory(TreeNode folderNode, Folder folder, string treePath)
    {
      List<JsonTreeNode> folderNodeList = folderNode.getChildren();
      Endpoints endpoints = folder.Endpoints;
      string context = "";
      string endpointName;
      string folderName;
      string baseUrl = "";
      string assembly = "";
      string dataLayerName = "";
      string folderPath = treePath;

      if (endpoints != null)
      {
        foreach (Endpoint endpoint in endpoints)
        {
          LeafNode endPointNode = new LeafNode();
          endpointName = endpoint.Name;
          endPointNode.text = endpoint.Name;
          endPointNode.iconCls = "application";
          endPointNode.type = "ApplicationNode";
          endPointNode.setLeaf(false);
          endPointNode.hidden = false;
          endPointNode.id = folderNode.id + "/" + endpoint.Name;
          endPointNode.identifier = endPointNode.id;
          endPointNode.nodeType = "async";
          folderNodeList.Add(endPointNode);
          treePath = folderPath + "." + endpoint.Name;

          if (endpoint.Context != null)
            context = endpoint.Context;

          if (endpoint.BaseUrl != null)
            baseUrl = endpoint.BaseUrl + "/adapter";

          #region Get Assambly information
          XElement bindings = GetBinding(context, endpointName, baseUrl);
          DataLayer dataLayer = null;
          if (bindings != null)
          {
              dataLayer = new DataLayer();
              dataLayer.Assembly = bindings.Element("bind").Attribute("to").Value;
              dataLayer.Name = bindings.Element("bind").Attribute("to").Value.Split(',')[1].Trim();
          }

          if (dataLayer != null)
          {
              assembly = dataLayer.Assembly;
              dataLayerName = dataLayer.Name;
          } 
          #endregion        
          
          Object record = new
          {
            Name = endpointName,
            Description = endpoint.Description,
            DataLayer = dataLayerName,
            context = context,
            BaseUrl = baseUrl,
            endpoint = endpointName,
            Assembly = assembly,
            securityRole = endpoint.SecurityRole,
            dbInfo = new Object(),
            dbDict = new Object()
          };

          endPointNode.record = record;
          endPointNode.property = new Dictionary<string, string>();
          endPointNode.property.Add("Name", endpointName);
          endPointNode.property.Add("Description", endpoint.Description);
          endPointNode.property.Add("Context", context);
          endPointNode.property.Add("Data Layer", dataLayerName);
          endPointNode.property.Add("User", endpoint.User);
        }
      }

      if (folder.Folders == null)
        return;
      else
      {
        foreach (Folder subFolder in folder.Folders)
        {
          folderName = subFolder.Name;
          TreeNode subFolderNode = new TreeNode();
          subFolderNode.text = folderName;
          subFolderNode.iconCls = GetNodeIconCls(subFolder.Type);
          subFolderNode.type = "folder";
          subFolderNode.hidden = false;
          subFolderNode.leaf = false;
          subFolderNode.id = folderNode.id + "/" + subFolder.Name;
          subFolderNode.identifier = subFolderNode.id;

          if (subFolder.Context != null)
            context = subFolder.Context;

          Object record = new
          {
            Name = folderName,
            context = context,
            Description = subFolder.Description,
            securityRole = subFolder.SecurityRole
          };
          subFolderNode.record = record;
          subFolderNode.property = new Dictionary<string, string>();
          subFolderNode.property.Add("Name", folderName);
          subFolderNode.property.Add("Description", subFolder.Description);
          subFolderNode.property.Add("Context", subFolder.Context);
          subFolderNode.property.Add("User", subFolder.User);
          folderNodeList.Add(subFolderNode);
          treePath = folderPath + "." + folderName;
          TraverseDirectory(subFolderNode, subFolder, treePath);
        }
      }
    }
    #endregion   
  }
}