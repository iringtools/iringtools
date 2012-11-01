using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Web;
using System.Web.Mvc;
using iRINGTools.Web.Helpers;
using iRINGTools.Web.Models;
using log4net;
using org.iringtools.adapter;
using org.iringtools.library;
using org.iringtools.mapping;
using org.iringtools.utility;


namespace org.iringtools.web.controllers
{
  public class DirectoryController : BaseController
  {
    IAdapterRepository _repository;
    private string _keyFormat = "Mapping.{0}.{1}";
    private static readonly ILog _logger = LogManager.GetLogger(typeof(DirectoryController));
    private System.Web.Script.Serialization.JavaScriptSerializer _serializer;
    private AdapterSettings _settings { get; set; }

    public DirectoryController()
      : this(new AdapterRepository())
    {
      string user = GetUserId((IDictionary<string, string>)_allClaims);
      Session[user + "." + "directory"] = null;
      Session[user + "." + "resource"] = null;
    }

    public DirectoryController(IAdapterRepository repository)
    {
      _repository = repository;
      _serializer =
         new System.Web.Script.Serialization.JavaScriptSerializer();

      AddDataLayarDLLinAppDomain();
    }

    public ActionResult Index()
    {
      return View(_repository.GetScopes());
    }

    public ActionResult GetNode(FormCollection form)
    {
      try
      {
        _logger.Debug("Starting Switch block");
        _logger.Debug(form["type"]);
        string securityRole = form["security"];

        switch (form["type"])
        {
          case "ScopesNode":
            {
              System.Collections.IEnumerator ie = Session.GetEnumerator();
              while (ie.MoveNext())
              {
                if (ie.Current.ToString().StartsWith(adapter_PREFIX))
                {
                  Session.Remove(ie.Current.ToString());
                  ie = Session.GetEnumerator();
                }
              }
              string directoryKey = adapter_PREFIX + GetUserId((IDictionary<string, string>)_allClaims);
              Tree directoryTree = _repository.GetDirectoryTree(directoryKey);
              return Json(directoryTree.getNodes(), JsonRequestBehavior.AllowGet);
            }
          case "ApplicationNode":
            {
              string context = form["node"];

              List<JsonTreeNode> nodes = new List<JsonTreeNode>();

              TreeNode dataObjectsNode = new TreeNode
              {
                nodeType = "async",
                type = "DataObjectsNode",
                iconCls = "folder",
                id = context + "/DataObjects",
                text = "Data Objects",
                //expanded = false,
                leaf = false,
                children = null,
                record = new
                {
                  securityRole = securityRole
                }
              };
              dataObjectsNode.property = new Dictionary<string, string>();
              AddPropertiestoNode(dataObjectsNode, form);

              TreeNode graphsNode = new TreeNode
              {
                nodeType = "async",
                type = "GraphsNode",
                iconCls = "folder",
                id = context + "/Graphs",
                text = "Mapped Objects (Graphs)",
                //expanded = false,
                leaf = false,
                children = null,
                record = new
                {
                  securityRole = securityRole
                }
              };
              graphsNode.property = new Dictionary<string, string>();
              AddPropertiestoNode(graphsNode, form);

              TreeNode ValueListsNode = new TreeNode
              {
                nodeType = "async",
                type = "ValueListsNode",
                iconCls = "folder",
                id = context + "/ValueLists",
                text = "ValueLists",
                //expanded = false,
                leaf = false,
                children = null,
                record = new
                {
                  securityRole = securityRole
                }
              };
              ValueListsNode.property = new Dictionary<string, string>();
              AddPropertiestoNode(ValueListsNode, form);
              nodes.Add(dataObjectsNode);
              nodes.Add(graphsNode);
              nodes.Add(ValueListsNode);

              return Json(nodes, JsonRequestBehavior.AllowGet);
            }
          case "ValueListsNode":
            {
              string context = form["node"];
              string contextName = form["contextName"];
              string endpoint = form["endpoint"];
              string baseUrl = form["baseUrl"];

              Mapping mapping = GetMapping(contextName, endpoint, baseUrl);

              List<JsonTreeNode> nodes = new List<JsonTreeNode>();

              foreach (ValueListMap valueList in mapping.valueListMaps)
              {
                TreeNode node = new TreeNode
                {
                  nodeType = "async",
                  type = "ValueListNode",
                  iconCls = "valuelistmap",
                  id = context + "/ValueList/" + valueList.name,
                  text = valueList.name,
                  //expanded = false,
                  leaf = false,
                  record = new
                  {
                    securityRole = securityRole,
                    record = valueList
                  }
                };

                node.property = new Dictionary<string, string>();
                node.property.Add("Name", valueList.name);
                AddPropertiestoNode(node, form);
                nodes.Add(node);
              }

              return Json(nodes, JsonRequestBehavior.AllowGet);
            }
          case "ValueListNode":
            {
              string context = form["node"];
              string valueList = form["text"];
              string contextName = form["contextName"];
              string endpoint = form["endpoint"];
              string baseUrl = form["baseUrl"];

              List<JsonTreeNode> nodes = new List<JsonTreeNode>();
              Mapping mapping = GetMapping(contextName, endpoint, baseUrl);
              ValueListMap valueListMap = mapping.valueListMaps.Find(c => c.name == valueList);

              foreach (var valueMap in valueListMap.valueMaps)
              {
                string valueMapUri = valueMap.uri.Split(':')[1];
                string classLabel = String.Empty;

                if (!String.IsNullOrEmpty(valueMap.label))
                {
                  classLabel = valueMap.label;
                }
                else if (Session[valueMapUri] != null)
                {
                  classLabel = (string)Session[valueMapUri];
                }
                else
                {
                  classLabel = GetClassLabel(valueMapUri);
                  Session[valueMapUri] = classLabel;
                }

                JsonTreeNode node = new JsonTreeNode
                {
                  //nodeType = "async",
                  type = "ListMapNode",
                  iconCls = "valuemap",
                  id = context + "/ValueMap/" + valueMap.internalValue,
                  text = classLabel + " [" + valueMap.internalValue + "]",
                  //expanded = false,
                  leaf = true,
                  record = new
                  {
                    securityRole = securityRole,
                    record = valueMap
                  }

                };

                node.property = new Dictionary<string, string>();
                node.property.Add("Name", valueMap.internalValue);
                node.property.Add("Class Label", classLabel);
                AddPropertiestoNode(node, form);
                nodes.Add(node);
              }

              return Json(nodes, JsonRequestBehavior.AllowGet);
            }

          case "DataObjectsNode":
            {
              string contextName = form["contextName"];
              string endpoint = form["endpoint"];
              string baseUrl = form["baseUrl"];
              DataDictionary dictionary = _repository.GetDictionary(contextName, endpoint, baseUrl);
              List<JsonTreeNode> nodes = new List<JsonTreeNode>();

              foreach (DataObject dataObject in dictionary.dataObjects)
              {
                JsonTreeNode node = new JsonTreeNode
                {
                  nodeType = "async",
                  type = "DataObjectNode",
                  iconCls = "treeObject",
                  id = form["node"] + "/DataObject/" + dataObject.objectName,
                  text = dataObject.objectName,
                  //expanded = false,
                  leaf = false,

                  record = new
                  {
                    Name = dataObject.objectName,
                    securityRole = securityRole
                  }
                };
                node.property = new Dictionary<string, string>();
                node.property.Add("Name", dataObject.objectName);
                AddPropertiestoNode(node, form);
                nodes.Add(node);
              }
              return Json(nodes, JsonRequestBehavior.AllowGet);

            }
          case "DataObjectNode":
            {
              string datatype, keytype;
              string context = form["node"];
              string dataObjectName = form["text"];
              string contextName = form["contextName"];
              string endpoint = form["endpoint"];
              string baseUrl = form["baseUrl"];

              DataDictionary dictionary = _repository.GetDictionary(contextName, endpoint, baseUrl);
              DataObject dataObject = dictionary.dataObjects.FirstOrDefault(o => o.objectName == dataObjectName);

              List<JsonTreeNode> nodes = new List<JsonTreeNode>();

              foreach (DataProperty properties in dataObject.dataProperties)
              {
                keytype = GetKeytype(properties.propertyName, dataObject.dataProperties);
                datatype = GetDatatype(properties.propertyName, dataObject.dataProperties);
                TreeNode node = new TreeNode
                {
                  //nodeType = "async",
                  type = (dataObject.isKeyProperty(properties.propertyName)) ? "KeyDataPropertyNode" : "DataPropertyNode",
                  iconCls = (dataObject.isKeyProperty(properties.propertyName)) ? _repository.GetNodeIconCls("key") : _repository.GetNodeIconCls("property"),
                  id = context + "/" + properties.propertyName,
                  text = properties.propertyName,
                  expanded = true,
                  leaf = false,
                  children = new List<JsonTreeNode>(),
                  record = new
                  {
                    Name = properties.propertyName,
                    Keytype = keytype,
                    Datatype = datatype,
                    securityRole = securityRole
                  }
                };
                node.property = new Dictionary<string, string>();
                node.property.Add("Name", properties.propertyName);
                node.property.Add("Keytype", keytype);
                node.property.Add("Datatype", datatype);
                AddPropertiestoNode(node, form);
                nodes.Add(node);
              }
              if (dataObject.dataRelationships.Count > 0)
              {
                foreach (DataRelationship relation in dataObject.dataRelationships)
                {
                  TreeNode node = new TreeNode
                  {
                    nodeType = "async",
                    type = "RelationshipNode",
                    iconCls = "treeRelation",
                    id = context + "/" + dataObject.objectName + "/" + relation.relationshipName,
                    text = relation.relationshipName,
                    //expanded = false,
                    leaf = false,

                    record = new
                    {
                      Name = relation.relationshipName,
                      Type = relation.relationshipType,
                      Related = relation.relatedObjectName,
                      securityRole = securityRole
                    }
                  };
                  node.property = new Dictionary<string, string>();
                  node.property.Add("Name", relation.relationshipName);
                  node.property.Add("Type", relation.relationshipType.ToString());
                  node.property.Add("Related", relation.relatedObjectName);
                  AddPropertiestoNode(node, form);
                  nodes.Add(node);
                }
              }
              return Json(nodes, JsonRequestBehavior.AllowGet);

            }

          case "RelationshipNode":
            {
              string keytype, datatype;
              string context = form["node"];
              string related = form["related"];
              string contextName = form["contextName"];
              string endpoint = form["endpoint"];
              string baseUrl = form["baseUrl"];

              List<JsonTreeNode> nodes = new List<JsonTreeNode>();
              DataDictionary dictionary = _repository.GetDictionary(contextName, endpoint, baseUrl);
              DataObject dataObject = dictionary.dataObjects.FirstOrDefault(o => o.objectName.ToUpper() == related.ToUpper());
              foreach (DataProperty properties in dataObject.dataProperties)
              {
                keytype = GetKeytype(properties.propertyName, dataObject.dataProperties);
                datatype = GetDatatype(properties.propertyName, dataObject.dataProperties);
                TreeNode node = new TreeNode
                {
                  //nodeType = "async",
                  type = (dataObject.isKeyProperty(properties.propertyName)) ? "KeyDataPropertyNode" : "DataPropertyNode",
                  iconCls = (dataObject.isKeyProperty(properties.propertyName)) ? _repository.GetNodeIconCls("key") : _repository.GetNodeIconCls("property"),
                  id = context + "/" + properties.propertyName,
                  text = properties.propertyName,
                  expanded = true,
                  leaf = false,
                  children = new List<JsonTreeNode>(),
                  record = new
                  {
                    Name = properties.propertyName,
                    Keytype = keytype,
                    Datatype = datatype,
                    securityRole = securityRole
                  }
                };
                node.property = new Dictionary<string, string>();
                node.property.Add("Name", properties.propertyName);
                node.property.Add("Type", keytype);
                node.property.Add("Related", related);
                AddPropertiestoNode(node, form);
                nodes.Add(node);
              }
              return Json(nodes, JsonRequestBehavior.AllowGet);
            }

          case "GraphsNode":
            {
              string context = form["node"];
              string contextName = form["contextName"];
              string endpoint = form["endpoint"];
              string baseUrl = form["baseUrl"];
              Mapping mapping = GetMapping(contextName, endpoint, baseUrl);
              List<JsonTreeNode> nodes = new List<JsonTreeNode>();

              foreach (GraphMap graph in mapping.graphMaps)
              {
                TreeNode node = new TreeNode
                {
                  //nodeType = "async",
                  type = "GraphNode",
                  iconCls = "graphmap",
                  id = context + "/Graph/" + graph.name,
                  text = graph.name,
                  expanded = true,
                  leaf = false,
                  children = new List<JsonTreeNode>(),
                  record = new
                  {
                    securityRole = securityRole,
                    record = graph
                  }
                };
                node.property = new Dictionary<string, string>();
                node.property.Add("Data Object Name", graph.dataObjectName);
                node.property.Add("Name", graph.name);
                node.property.Add("Identifier", graph.classTemplateMaps[0].classMap.identifiers[0].Split('.')[1]);
                node.property.Add("Class Label", graph.classTemplateMaps[0].classMap.name);
                AddPropertiestoNode(node, form);
                nodes.Add(node);
              }

              return Json(nodes, JsonRequestBehavior.AllowGet);
            }
          default:
            {
              return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }
        }
      }
      catch (Exception e)
      {
        _logger.Error(e.ToString());
        throw e;
      }
    }

    private void AddPropertiestoNode(JsonTreeNode node, FormCollection form)
    {
      if (form["contextName"] != null)
        node.property.Add("context", form["contextName"]);
      if (form["endpoint"] != null)
        node.property.Add("endpoint", form["endpoint"]);
      if (form["baseUrl"] != null)
        node.property.Add("baseUrl", form["baseUrl"]);
    }

    public string DataLayer(JsonTreeNode node, FormCollection form)
    {
      HttpFileCollectionBase files = Request.Files;
      HttpPostedFileBase hpf = files[0] as HttpPostedFileBase;

      DataLayer dataLayer = new DataLayer()
      {
        Name = form["Name"],
        MainDLL = form["MainDLL"],
        Package = Utility.ToMemoryStream(hpf.InputStream)
      };

      MemoryStream dataLayerStream = new MemoryStream();
      DataContractSerializer serializer = new DataContractSerializer(typeof(DataLayer));
      serializer.WriteObject(dataLayerStream, dataLayer);
      dataLayerStream.Position = 0;

      Response response = _repository.SaveDataLayer(dataLayerStream);
      //return Json(response, JsonRequestBehavior.AllowGet);
      return Utility.ToJson<Response>(response);
    }

    public ActionResult DataLayers(JsonTreeNode node, FormCollection form)
    {
      DataLayers dataLayers = _repository.GetDataLayers(form["baseUrl"]);
      JsonContainer<DataLayers> container = new JsonContainer<DataLayers>();
      container.items = dataLayers;
      container.success = true;
      container.total = dataLayers.Count;
      return Json(container, JsonRequestBehavior.AllowGet);
    }

    public JsonResult Folder(FormCollection form)
    {
      string success;
      string key = adapter_PREFIX + GetUserId((IDictionary<string, string>)_allClaims);
      success = _repository.Folder(form["foldername"], form["description"], form["path"], form["state"], form["contextName"], form["oldContext"], key);

      if (success == "ERROR")
      {
        string msg = _repository.GetCombinationMsg();
        return Json(new { success = false } + msg, JsonRequestBehavior.AllowGet);
      }

      return Json(new { success = true }, JsonRequestBehavior.AllowGet);
    }

    public JsonResult TestBaseUrl(FormCollection form)
    {
      string success = _repository.TestBaseUrl(form["baseUrl"]);

      if (success == "ERROR")
      {
        return Json(new { error = true }, JsonRequestBehavior.AllowGet);
      }

      return Json(new { success = true }, JsonRequestBehavior.AllowGet);
    }

    public JsonResult Endpoint(FormCollection form)
    {
      string success;
      string key = adapter_PREFIX + GetUserId((IDictionary<string, string>)_allClaims);

      success = _repository.Endpoint(form["endpoint"], form["path"], form["description"], form["state"], form["contextValue"], form["oldAssembly"], form["assembly"], form["baseUrl"], form["oldBaseUrl"], key);

      if (success == "ERROR")
      {
        string msg = _repository.GetCombinationMsg();
        return Json(new { success = false } + msg, JsonRequestBehavior.AllowGet);
      }

      return Json(new { success = true }, JsonRequestBehavior.AllowGet);
    }

    public JsonResult DeleteEntry(FormCollection form)
    {
      string key = adapter_PREFIX + GetUserId((IDictionary<string, string>)_allClaims);
      _repository.DeleteEntry(form["path"], form["type"], form["contextName"], form["baseUrl"], key);
      return Json(new { success = true }, JsonRequestBehavior.AllowGet);
    }

    public JsonResult RegenAll()
    {
      string resourceKey = adapter_PREFIX + GetUserId((IDictionary<string, string>)_allClaims);
      Response response = _repository.RegenAll(resourceKey);
      return Json(response, JsonRequestBehavior.AllowGet);
    }

    public JsonResult RootSecurityRole()
    {
      string rootSecuirtyRole = _repository.GetRootSecurityRole();
      return Json(rootSecuirtyRole, JsonRequestBehavior.AllowGet);
    }

    public JsonResult UseLdap()
    {
      string ifUseLdap = _repository.GetUserLdap();
      return Json(ifUseLdap, JsonRequestBehavior.AllowGet);
    }

    public JsonResult EndpointBaseUrl()
    {
      string resourceKey = adapter_PREFIX + GetUserId((IDictionary<string, string>)_allClaims);
      Urls baseUrls = _repository.GetEndpointBaseUrl(resourceKey);
      JsonContainer<Urls> container = new JsonContainer<Urls>();
      container.items = baseUrls;
      container.success = true;
      container.total = baseUrls.Count;
      return Json(container, JsonRequestBehavior.AllowGet);
    }

    public JsonResult FolderContext()
    {
      string resourceKey = adapter_PREFIX + GetUserId((IDictionary<string, string>)_allClaims);
      ContextNames contexts = _repository.GetFolderContexts(resourceKey);
      JsonContainer<ContextNames> container = new JsonContainer<ContextNames>();
      container.items = contexts;
      container.success = true;
      container.total = contexts.Count;
      return Json(container, JsonRequestBehavior.AllowGet);
    }

    #region Private Methods

    private Mapping GetMapping(string contextName, string endpoint, string baseUrl)
    {
      string key = adapter_PREFIX + string.Format(_keyFormat, contextName, endpoint, baseUrl);

      if (Session[key] == null)
      {
        Session[key] = _repository.GetMapping(contextName, endpoint, baseUrl);
      }

      return (Mapping)Session[key];
    }

    private string GetClassLabel(string classId)
    {
      Entity dataEntity = _repository.GetClassLabel(classId);
      return Convert.ToString(dataEntity.Label);
    }

    private string GetKeytype(string name, List<DataProperty> properties)
    {
      string keyType = string.Empty;
      keyType = properties.FirstOrDefault(p => p.propertyName == name).keyType.ToString();
      return keyType;
    }
    private string GetDatatype(string name, List<DataProperty> properties)
    {
      string dataType = string.Empty;
      dataType = properties.FirstOrDefault(p => p.propertyName == name).dataType.ToString();
      return dataType;
    }

    private Response PrepareErrorResponse(Exception ex)
    {
      Response response = new Response();
      response.Level = StatusLevel.Error;
      response.Messages = new Messages();
      response.Messages.Add(ex.Message);
      response.Messages.Add(ex.StackTrace);
      return response;
    }

    #region Manage DataLayer DLLs

    private void AddDataLayarDLLinAppDomain()
    {
      _settings = new AdapterSettings();
      _settings.AppendSettings(ConfigurationManager.AppSettings);

      AppDomain currentDomain = AppDomain.CurrentDomain;
      currentDomain.AssemblyResolve += new ResolveEventHandler(DataLayerAssemblyResolveEventHandler);
      if (Directory.Exists(_settings["DataLayerPath"]))
      {
        string[] datalayerdirectories = Directory.GetDirectories(_settings["DataLayerPath"]);
        foreach (string _dldir in datalayerdirectories)
        {
          if (_dldir.Contains("DataLayers"))
          {
            string[] directories = Directory.GetDirectories(_dldir);
            foreach (string dir in directories)
            {
              string[] files = Directory.GetFiles(dir);
              foreach (string file in files)
              {
                if (file.ToLower().EndsWith(".dll") || file.ToLower().EndsWith(".exe"))
                {
                  byte[] bytes = Utility.GetBytes(file);
                  Assembly.Load(bytes);
                }
              }
            }
          }
        }
      }
    }

    private Assembly DataLayerAssemblyResolveEventHandler(object sender, ResolveEventArgs args)
    {
      if (args.Name.Contains(".resources,"))
      {
        return null;
      }

      if (Directory.Exists(@"C:\Project\New folder\iRINGTools.Services\App_Data\DataLayers\SPPIDDataLayer"))
      {
        string[] files = Directory.GetFiles(@"C:\Project\New folder\iRINGTools.Services\App_Data\DataLayers\SPPIDDataLayer");

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

    #endregion

    #endregion
  }

}

