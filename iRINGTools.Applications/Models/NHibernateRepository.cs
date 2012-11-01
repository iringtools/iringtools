using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.Web.Mvc;
using org.iringtools.library;
using org.iringtools.utility;
using System.Collections.Specialized;
using iRINGTools.Web.Helpers;
using log4net;
using Ninject;
using System.Configuration;

namespace org.iringtools.web.Models
{
  public class NHibernateRepository : INHibernateRe3pository
  {
    private WebHttpClient _adapterServiceClient = null;
    private WebHttpClient _hibernateServiceClient = null;
    private static readonly ILog _logger = LogManager.GetLogger(typeof(NHibernateRepository));
    private static Dictionary<string, NodeIconCls> nodeIconClsMap;
    private string proxyHost = "";
    private string proxyPort = "";
    private WebProxy webProxy = null;
    private string adapterServiceUri = "";
    private string hibernateServiceUri = "";

    [Inject]
    public NHibernateRepository()
    {
      //_settings = ConfigurationManager.AppSettings;
      //_adapterServiceClient = new WebHttpClient(_settings["AdapterServiceUri"]);
      SetNodeIconClsMap();
      NameValueCollection settings = ConfigurationManager.AppSettings;
      ServiceSettings _settings = new ServiceSettings();
      _settings.AppendSettings(settings);

      #region initialize webHttpClient for converting old mapping
      proxyHost = _settings["ProxyHost"];
      proxyPort = _settings["ProxyPort"];
      adapterServiceUri = _settings["AdapterServiceUri"];
      hibernateServiceUri = _settings["NHibernateServiceUri"];

      if (!String.IsNullOrEmpty(proxyHost) && !String.IsNullOrEmpty(proxyPort))
      {
        webProxy = _settings.GetWebProxyCredentials().GetWebProxy() as WebProxy;
        _adapterServiceClient = new WebHttpClient(adapterServiceUri, null, webProxy);
        _hibernateServiceClient = new WebHttpClient(hibernateServiceUri, null, webProxy);
      }
      else
      {
        _adapterServiceClient = new WebHttpClient(adapterServiceUri);
        _hibernateServiceClient = new WebHttpClient(hibernateServiceUri);
      }
      #endregion
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
      catch
      {
        return "folder";
      }
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

    public DataDictionary GetDictionary(string contextName, string endpoint, string baseUrl)
    {
      DataDictionary obj = null;

      try
      {
        WebHttpClient _newServiceClient = PrepareServiceClient(baseUrl, "hibernate");
        obj = _newServiceClient.Get<DataDictionary>(String.Format("/{0}/{1}/dictionary", contextName, endpoint), true);
      }
      catch (Exception ex)
      {
        _logger.Error(ex.ToString());
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

    #region NHibernate Configuration Wizard support methods
    public DataProviders GetDBProviders(string baseUrl)
    {
      WebHttpClient _newServiceClient = PrepareServiceClient(baseUrl, "hibernate");
      return _newServiceClient.Get<DataProviders>("/providers");
    }

    public string SaveDBDictionary(string scope, string application, string tree, string baseUrl)
    {
      WebHttpClient _newServiceClient = PrepareServiceClient(baseUrl, "hibernate");
      DatabaseDictionary dbDictionary = Utility.FromJson<DatabaseDictionary>(tree);

      string connStr = dbDictionary.ConnectionString;
      if (!String.IsNullOrEmpty(connStr))
      {
        string urlEncodedConnStr = Utility.DecodeFrom64(connStr);
        dbDictionary.ConnectionString = HttpUtility.UrlDecode(urlEncodedConnStr);
      }

      string postResult = null;
      try
      {
        postResult = _newServiceClient.Post<DatabaseDictionary>("/" + scope + "/" + application + "/dictionary", dbDictionary, true);
      }
      catch (Exception ex)
      {
        _logger.Error("Error posting DatabaseDictionary." + ex);
      }
      return postResult;
    }

    public DatabaseDictionary GetDBDictionary(string context, string application, string baseUrl)
    {
      WebHttpClient _newServiceClient = PrepareServiceClient(baseUrl, "hibernate");
      DatabaseDictionary dbDictionary = _newServiceClient.Get<DatabaseDictionary>(String.Format("/{0}/{1}/dictionary", context, application));

      string connStr = dbDictionary.ConnectionString;
      if (!String.IsNullOrEmpty(connStr))
      {
        dbDictionary.ConnectionString = Utility.EncodeTo64(connStr);
      }

      return dbDictionary;
    }

    public List<string> GetTableNames(string scope, string application, string dbProvider, string dbServer,
      string dbInstance, string dbName, string dbSchema, string dbUserName, string dbPassword, string portNumber, string serName, string baseUrl)
    {
      WebHttpClient _newServiceClient = PrepareServiceClient(baseUrl, "hibernate");
      string uri = String.Format("/{0}/{1}/tables", scope, application);

      Request request = new Request();
      request.Add("dbProvider", dbProvider);
      request.Add("dbServer", dbServer);
      request.Add("portNumber", portNumber);
      request.Add("dbInstance", dbInstance);
      request.Add("dbName", dbName);
      request.Add("dbSchema", dbSchema);
      request.Add("dbUserName", dbUserName);
      request.Add("dbPassword", dbPassword);
      request.Add("serName", serName);

      return _newServiceClient.Post<Request, List<string>>(uri, request, true);
    }

    // use appropriate icons especially node with children
    public Tree GetDBObjects(string contextName, string endpoint, string dbProvider, string dbServer,
      string dbInstance, string dbName, string dbSchema, string dbUserName, string dbPassword, string tableNames, string portNumber, string serName, string baseUrl, DatabaseDictionary databaseDictionary)
    {
      bool hasDataObjectinDBDictionary = false;
      bool hasDBDictionary = false;
      WebHttpClient _newServiceClient = PrepareServiceClient(baseUrl, "hibernate");
      string uri = String.Format("/{0}/{1}/objects", contextName, endpoint);
      Tree tree = null;

      Request request = new Request();
      request.Add("dbProvider", dbProvider);
      request.Add("dbServer", dbServer);
      request.Add("portNumber", portNumber);
      request.Add("dbInstance", dbInstance);
      request.Add("dbName", dbName);
      request.Add("dbSchema", dbSchema);
      request.Add("dbUserName", dbUserName);
      request.Add("dbPassword", dbPassword);
      request.Add("tableNames", tableNames);
      request.Add("serName", serName);

      List<DataObject> dataObjects = _newServiceClient.Post<Request, List<DataObject>>(uri, request, true);

      if (databaseDictionary != null)
        if (databaseDictionary.dataObjects.Count > 0)
          hasDBDictionary = true;

      if (dataObjects != null)
      {
        tree = new Tree();
        List<JsonTreeNode> dbObjectNodes = tree.getNodes();

        foreach (DataObject dataObject in dataObjects)
        {
          hasDataObjectinDBDictionary = false;

          if (hasDBDictionary)
            if (databaseDictionary.dataObjects.FirstOrDefault<DataObject>(o => o.tableName.ToLower() == dataObject.tableName.ToLower()) != null)
              hasDataObjectinDBDictionary = true;

          // create data object node
          TreeNode dataObjectNode = new TreeNode();
          dataObjectNode.text = dataObject.tableName;
          dataObjectNode.type = "DATAOBJECT";
          dataObjectNode.id = dataObjectNode.text;
          dataObjectNode.identifier = dataObjectNode.id;
          dataObjectNode.iconCls = "treeObject";
          dataObjectNode.leaf = false;
          dataObjectNode.children = new List<JsonTreeNode>();
          dataObjectNode.property = new Dictionary<string, string>()
          {
            {"objectNamespace", "org.iringtools.adapter.datalayer.proj_" + contextName + "." + endpoint},
            {"objectName", dataObject.objectName},
            {"tableName", dataObject.objectName},
            {"keyDelimiter", dataObject.keyDelimeter}
          };

          dataObjectNode.record = new
          {
            Name = dataObject.objectName
          };

          TreeNode keyPropertiesNode = new TreeNode();
          keyPropertiesNode.text = "Keys";
          keyPropertiesNode.type = "KEYS";
          keyPropertiesNode.id = dataObjectNode.id + "/" + keyPropertiesNode.text;
          keyPropertiesNode.identifier = keyPropertiesNode.id;
          keyPropertiesNode.expanded = true;
          keyPropertiesNode.iconCls = "folder";
          keyPropertiesNode.leaf = false;
          keyPropertiesNode.children = new List<JsonTreeNode>();

          TreeNode hiddenNodeRoot = new TreeNode();
          hiddenNodeRoot.text = "hiddenroot";
          hiddenNodeRoot.children = new List<JsonTreeNode>();
          Dictionary<string, TreeNode> hiddenNodes = new Dictionary<string, TreeNode>()
          {
           {"hiddenNode", hiddenNodeRoot}
          };

          JsonPropertyNode dataPropertiesNode = new JsonPropertyNode();
          dataPropertiesNode.text = "Properties";
          dataPropertiesNode.type = "PROPERTIES";
          dataPropertiesNode.id = dataObjectNode.id + "/" + dataPropertiesNode.text;
          dataPropertiesNode.identifier = dataPropertiesNode.id;
          dataPropertiesNode.expanded = true;
          dataPropertiesNode.iconCls = "folder";
          dataPropertiesNode.leaf = false;
          dataPropertiesNode.children = new List<JsonTreeNode>();
          dataPropertiesNode.hiddenNodes = hiddenNodes;

          TreeNode relationshipsNode = new TreeNode();
          relationshipsNode.text = "Relationships";
          relationshipsNode.type = "relationships";
          relationshipsNode.id = dataObjectNode.id + "/" + relationshipsNode.text;
          relationshipsNode.identifier = relationshipsNode.id;
          relationshipsNode.expanded = true;
          relationshipsNode.iconCls = "folder";
          relationshipsNode.leaf = false;
          relationshipsNode.children = new List<JsonTreeNode>();

          dataObjectNode.children.Add(keyPropertiesNode);
          dataObjectNode.children.Add(dataPropertiesNode);
          dataObjectNode.children.Add(relationshipsNode);

          // add key/data property nodes
          foreach (DataProperty dataProperty in dataObject.dataProperties)
          {
            Dictionary<string, string> properties = new Dictionary<string, string>()
            {
              {"columnName", dataProperty.columnName},
              {"propertyName", dataProperty.propertyName},
              {"dataType", dataProperty.dataType.ToString()},
              {"keyType", ""},
              {"dataLength", dataProperty.dataLength.ToString()},
              {"nullable", dataProperty.isNullable.ToString()},
              {"showOnIndex", dataProperty.showOnIndex.ToString()},
              {"numberOfDecimals", dataProperty.numberOfDecimals.ToString()},
              {"isHidden", dataProperty.isHidden.ToString()}
            };

            if (dataObject.isKeyProperty(dataProperty.propertyName) && !hasDataObjectinDBDictionary)
            {
              properties["keyType"] = dataProperty.keyType.ToString();
              JsonTreeNode keyPropertyNode = new JsonTreeNode();
              keyPropertyNode.text = dataProperty.columnName;
              keyPropertyNode.type = "KEYPROPERTY";
              keyPropertyNode.id = keyPropertiesNode.id + "/" + keyPropertyNode.text;
              keyPropertyNode.identifier = keyPropertyNode.id;
              keyPropertyNode.property = properties;
              keyPropertyNode.iconCls = "treeKey";
              keyPropertyNode.hidden = false;
              keyPropertyNode.leaf = true;
              keyPropertiesNode.children.Add(keyPropertyNode);
              keyPropertiesNode.record = new
              {
                Name = keyPropertyNode.text
              };
            }
            else 
            {
              JsonTreeNode dataPropertyNode = new JsonTreeNode();
              dataPropertyNode.text = dataProperty.columnName;
              dataPropertyNode.type = "DATAPROPERTY";
              dataPropertyNode.id = dataPropertiesNode.id + "/" + dataPropertyNode.text;
              dataPropertyNode.identifier = dataPropertyNode.id;
              dataPropertyNode.iconCls = "treeProperty";
              dataPropertyNode.leaf = true;              
              dataPropertyNode.property = properties;
              dataPropertyNode.record = new
              {
                Name = dataPropertyNode.text
              };

              if (!hasDataObjectinDBDictionary || dataProperty.isHidden)
              {
                dataPropertyNode.hidden = true;
                hiddenNodeRoot.children.Add(dataPropertyNode);
              }
              else
              {
                dataPropertyNode.hidden = false;
                dataPropertiesNode.children.Add(dataPropertyNode);
              }
            }            
          }

          dbObjectNodes.Add(dataObjectNode);
        }
      }

      if (hasDBDictionary)
        return dBOjbectsAndDBDictionary(tree, contextName, endpoint, baseUrl, databaseDictionary);

      return tree;
    }

    //rootNode(dataObjectNode) is from database tables
    //dbDict(dataObject) is from database dictionary
    private Tree dBOjbectsAndDBDictionary(Tree tree, string contextName, string endpoint, string baseUrl, DatabaseDictionary dbDict)
    {
      string[] relationTypeStr = { "OneToOne", "OneToMany" };
      JsonTreeNode hiddenNode = null;
      DataRelationship relation = null;
      bool hasProperty = false;
      DataProperty tempDataProperty = null;
      JsonTreeNode tempPropertyNode = null;

      if (tree == null)
        tree = new Tree();
      List<JsonTreeNode> dbObjectNodes = tree.getNodes();
      
      
      // sync data object tree with data dictionary
      for (int i = 0; i < dbObjectNodes.Count; i++)
      {
        TreeNode dataObjectNode = (TreeNode)dbObjectNodes[i];
        dataObjectNode.property["tableName"] = dataObjectNode.text;

        for (int ijk = 0; ijk < dbDict.dataObjects.Count; ijk++)
        {
          DataObject dataObject = dbDict.dataObjects[ijk];

          if (dataObjectNode.property["tableName"].ToUpper() != dataObject.tableName.ToUpper())
            continue;

          // sync data object
          dataObjectNode.property["objectNamespace"] = dataObject.objectNamespace;
          dataObjectNode.property["objectName"] = dataObject.objectName;
          dataObjectNode.property["keyDelimiter"] = dataObject.keyDelimeter;
          dataObjectNode.property["description"] = dataObject.description;
          dataObjectNode.text = dataObject.objectName;

          if (dataObject.objectName.ToLower() == dataObjectNode.text.ToLower())
          {
            List<string> shownProperty = new List<string>();
            TreeNode keysNode = (TreeNode)dataObjectNode.children[0];
            JsonPropertyNode propertiesNode = (JsonPropertyNode)dataObjectNode.children[1];
            TreeNode hiddenRootNode = propertiesNode.hiddenNodes["hiddenNode"];
            TreeNode relationshipsNode = (TreeNode)dataObjectNode.children[2];

            // sync data properties
            for (int jj = 0; jj < dataObject.dataProperties.Count; jj++)            
            {
              tempDataProperty = dataObject.dataProperties[jj];
              hasProperty = false;

              for (int j = 0; j < propertiesNode.children.Count; j++)
              {
                tempPropertyNode = propertiesNode.children[j];

                if (tempPropertyNode.text.ToLower() == tempDataProperty.columnName.ToLower())
                {
                  hasProperty = true;
                  
                  if (!tempDataProperty.isHidden)
                  {
                    if (!hasShown(shownProperty, tempPropertyNode.text.ToLower()))
                    {
                      shownProperty.Add(tempPropertyNode.text.ToLower());
                      tempPropertyNode.hidden = false;
                    }

                    tempPropertyNode.text = tempDataProperty.propertyName;
                    tempPropertyNode.property["keyType"] = tempDataProperty.keyType.ToString();
                    tempPropertyNode.property["propertyName"] = tempDataProperty.propertyName;
                    tempPropertyNode.property["isHidden"] = tempDataProperty.isHidden.ToString();
                  }
                  else
                  {
                    JsonTreeNode newHiddenNode = new JsonTreeNode();
                    newHiddenNode.text = tempDataProperty.propertyName;
                    newHiddenNode.type = "DATAPROPERTY";
                    newHiddenNode.id = propertiesNode.id + "/" + newHiddenNode.text;
                    newHiddenNode.identifier = newHiddenNode.id;
                    newHiddenNode.iconCls = "treeProperty";
                    newHiddenNode.leaf = true;
                    newHiddenNode.hidden = true;
                    newHiddenNode.property = new Dictionary<string, string>()
                    {
                      {"columnName", tempPropertyNode.property["columnName"]},
                      {"propertyName", tempDataProperty.propertyName},
                      {"dataType", tempPropertyNode.property["dataType"]},
                      {"keyType", tempDataProperty.keyType.ToString()},
                      {"dataLength", tempPropertyNode.property["dataLength"]},
                      {"nullable", tempPropertyNode.property["nullable"]},
                      {"showOnIndex", tempPropertyNode.property["showOnIndex"]},
                      {"numberOfDecimals", tempPropertyNode.property["numberOfDecimals"]},
                      {"isHidden", tempDataProperty.isHidden.ToString()}
                    };
                    newHiddenNode.record = new
                    {
                      Name = tempPropertyNode.text
                    };
                    hiddenRootNode.children.Add(newHiddenNode);
                    propertiesNode.children.RemoveAt(j);
                    j--;
                  }
                  break;
                }
              }

              if (!hasProperty)
                for (int j = 0; j < hiddenRootNode.children.Count; j++)
                {
                  if (hiddenRootNode.children[j].text.ToLower() == tempDataProperty.columnName.ToLower())
                  {
                    hasProperty = true;
                    if (!hasShown(shownProperty, hiddenRootNode.children[j].text.ToLower()))
                    {
                      shownProperty.Add(hiddenRootNode.children[j].text.ToLower());
                      hiddenNode = hiddenRootNode.children[j];
                      JsonTreeNode dataPropertyNode = new JsonTreeNode();
                      dataPropertyNode.text = tempDataProperty.propertyName;
                      dataPropertyNode.type = "DATAPROPERTY";
                      dataPropertyNode.id = propertiesNode.id + "/" + dataPropertyNode.text;
                      dataPropertyNode.identifier = dataPropertyNode.id;
                      dataPropertyNode.iconCls = "treeProperty";
                      dataPropertyNode.leaf = true;
                      dataPropertyNode.hidden = false;
                      dataPropertyNode.property = new Dictionary<string, string>()
                    {
                      {"columnName", hiddenNode.property["columnName"]},
                      {"propertyName", tempDataProperty.propertyName},
                      {"dataType", hiddenNode.property["dataType"]},
                      {"keyType", tempDataProperty.keyType.ToString()},
                      {"dataLength", hiddenNode.property["dataLength"]},
                      {"nullable", hiddenNode.property["nullable"]},
                      {"showOnIndex", hiddenNode.property["showOnIndex"]},
                      {"numberOfDecimals", hiddenNode.property["numberOfDecimals"]},
                      {"isHidden", tempDataProperty.isHidden.ToString()}
                    };
                      dataPropertyNode.record = new
                      {
                        Name = dataPropertyNode.text
                      };

                      propertiesNode.children.Add(dataPropertyNode);
                      hiddenRootNode.children.RemoveAt(j);
                      j--;
                      break;
                    }
                  }
                }

              if (!hasProperty)
              {
                Dictionary<string, string> properties = new Dictionary<string, string>()
                {
                  {"columnName", tempDataProperty.columnName},
                  {"propertyName", tempDataProperty.propertyName},
                  {"dataType", tempDataProperty.dataType.ToString()},
                  {"keyType", ""},
                  {"dataLength", tempDataProperty.dataLength.ToString()},
                  {"nullable", tempDataProperty.isNullable.ToString()},
                  {"showOnIndex", tempDataProperty.showOnIndex.ToString()},
                  {"numberOfDecimals", tempDataProperty.numberOfDecimals.ToString()},
                  {"isHidden", tempDataProperty.isHidden.ToString()}
                };

                JsonTreeNode dataPropertyNode = new JsonTreeNode();
                dataPropertyNode.text = tempDataProperty.columnName;
                dataPropertyNode.type = "DATAPROPERTY";
                dataPropertyNode.id = dataPropertyNode.id + "/" + dataPropertyNode.text;
                dataPropertyNode.identifier = dataPropertyNode.id;
                dataPropertyNode.iconCls = "treeProperty";
                dataPropertyNode.leaf = true;
                dataPropertyNode.property = properties;
                dataPropertyNode.record = new
                {
                  Name = dataPropertyNode.text
                };

                if (tempDataProperty.isHidden)
                {
                  dataPropertyNode.hidden = true;
                  hiddenRootNode.children.Add(dataPropertyNode);
                }
                else
                {
                  propertiesNode.children.Add(dataPropertyNode);
                }
              }
            }

            // sync key properties
            bool foundIkk = false;
            for (int ij = 0; ij < dataObject.keyProperties.Count; ij++)
            {
              for (int k = 0; k < keysNode.children.Count; k++)
              {
                for (int ikk = 0; ikk < dataObject.dataProperties.Count; ikk++)
                {
                  if (dataObject.keyProperties[ij].keyPropertyName.ToLower() == dataObject.dataProperties[ikk].propertyName.ToLower())
                  {
                    if (keysNode.children[k].text.ToLower() == dataObject.dataProperties[ikk].columnName.ToLower())
                    {
                      keysNode.children[k].text = dataObject.keyProperties[ij].keyPropertyName;
                      keysNode.children[k].property["propertyName"] = dataObject.keyProperties[ij].keyPropertyName;
                      keysNode.children[k].property["isHidden"] = "false";
                      keysNode.children[k].property["keyType"] = "assigned";
                      ij++;
                      foundIkk = true;
                      break;
                    }
                  }
                }
                if (foundIkk)
                  break;
              }
              if (ij < dataObject.keyProperties.Count)
              {
                string nodeText;
                for (int ijj = 0; ijj < propertiesNode.children.Count; ijj++)
                {
                  nodeText = dataObject.keyProperties[ij].keyPropertyName;
                  if (propertiesNode.children[ijj].text.ToLower() == nodeText.ToLower())
                  {
                    propertiesNode.children[ijj].property["propertyName"] = nodeText;
                    JsonTreeNode newKeyNode = new JsonTreeNode();
                    newKeyNode.text = nodeText;
                    newKeyNode.type = "keyProperty";
                    newKeyNode.id = keysNode.id + "/" + newKeyNode.text;
                    newKeyNode.identifier = newKeyNode.id;
                    newKeyNode.leaf = true;
                    newKeyNode.iconCls = "treeKey";
                    newKeyNode.hidden = false;
                    newKeyNode.property = propertiesNode.children[ijj].property;
                    newKeyNode.record = new
                    {
                      Name = newKeyNode.text
                    };
                    propertiesNode.children.RemoveAt(ijj);
                    ijj--;

                    if (newKeyNode != null)
                      keysNode.children.Add(newKeyNode);

                    break;
                  }
                }

                for (int ijj = 0; ijj < hiddenRootNode.children.Count; ijj++)
                {
                  nodeText = dataObject.keyProperties[ij].keyPropertyName;
                  if (hiddenRootNode.children[ijj].text.ToLower() == nodeText.ToLower())
                  {
                    hiddenRootNode.children[ijj].property["propertyName"] = nodeText;
                    JsonTreeNode newKeyNode = new JsonTreeNode();
                    newKeyNode.text = nodeText;
                    newKeyNode.type = "keyProperty";
                    newKeyNode.id = keysNode.id + "/" + newKeyNode.text;
                    newKeyNode.identifier = newKeyNode.id;
                    newKeyNode.leaf = true;
                    newKeyNode.iconCls = "treeKey";
                    newKeyNode.hidden = false;
                    newKeyNode.property = hiddenRootNode.children[ijj].property;
                    newKeyNode.record = new
                    {
                      Name = newKeyNode.text
                    };
                    hiddenRootNode.children.RemoveAt(ijj);
                    ijj--;

                    if (newKeyNode != null)
                      keysNode.children.Add(newKeyNode);

                    break;
                  }
                }
              }
            }

            // sync relationships 
            for (int kj = 0; kj < dataObject.dataRelationships.Count; kj++)
            {
              Dictionary<string, Object> relatedObjMap = new Dictionary<string, Object>();
              relation = dataObject.dataRelationships[kj];
              JsonRelationNode relationNode = new JsonRelationNode();
              relationNode.text = relation.relationshipName;
              relationNode.type = "relationship";
              relationNode.id = relationshipsNode.id + "/" + relationNode.text;
              relationNode.identifier = relationNode.id;
              relationNode.leaf = true;
              relationNode.iconCls = "treeRelation";              
              relationNode.relatedObjMap = relatedObjMap;
              relationNode.objectName = dataObjectNode.text;
              relationNode.relatedObjectName = relation.relatedObjectName;
              relationNode.relatedTableName = getRelatedTableName(relation.relatedObjectName, dbDict);
              relationNode.relationshipType = relation.relationshipType.ToString();
              relationNode.relationshipTypeIndex = ((int)relation.relationshipType).ToString();
              relationNode.record = new
              {
                Name = relationNode.text
              };
              List<Dictionary<string, string>> mapArray = new List<Dictionary<string, string>>();
              for (int kjj = 0; kjj < relation.propertyMaps.Count; kjj++)
              {
                Dictionary<string, string> mapItem = new Dictionary<string, string>()
                {
						      {"dataPropertyName", relation.propertyMaps[kjj].dataPropertyName},
                  {"dataColumnName", getColumnName(dataObject, relation.propertyMaps[kjj].dataPropertyName)},
						      {"relatedPropertyName", relation.propertyMaps[kjj].relatedPropertyName},
                  {"relatedColumnName", getColumnName(getRelatedDataObject(relationNode.relatedTableName, dbDict), relation.propertyMaps[kjj].relatedPropertyName)}
                };
                mapArray.Add(mapItem);
              }

              relationNode.propertyMap = mapArray;
              relationshipsNode.expanded = true;
              relationshipsNode.children.Add(relationNode);
            }
          }
        }
      }

      return tree;
    }

    private DataObject getRelatedDataObject(String relatedTableName, DatabaseDictionary dbDict)
    {
      if (dbDict == null)
        return null;

      for (int i = 0; i < dbDict.dataObjects.Count; i++)
      {
        if (relatedTableName.ToUpper() == dbDict.dataObjects[i].tableName.ToUpper())
          return dbDict.dataObjects[i];
      }
      return null;
    }

    private string getRelatedTableName(string relatedObjectName, DatabaseDictionary dbDict)
    {
      if (dbDict == null)
        return "";

      for (int i = 0; i < dbDict.dataObjects.Count; i++)
      {
        if (relatedObjectName.ToUpper() == dbDict.dataObjects[i].objectName.ToUpper())
          return dbDict.dataObjects[i].tableName;
      }
      return "";
    }    

    private string getColumnName(DataObject dataObject, string propertyName)
    {
      if (dataObject == null)
        return "";

      for (int i = 0; i < dataObject.dataProperties.Count; i++)
      {
        if (propertyName.ToUpper() == dataObject.dataProperties[i].propertyName.ToUpper())
          return dataObject.dataProperties[i].columnName;
      }
      return "";
    }

    private Boolean hasShown(List<string> shownArray, string text)
    {
      for (int shownIndex = 0; shownIndex < shownArray.Count; shownIndex++)
        if (shownArray[shownIndex] == text)
          return true;
      return false;
    }

    private WebHttpClient PrepareServiceClient(string baseUrl, string serviceName)
    {
      if (baseUrl == "" || baseUrl == null)
        return _hibernateServiceClient;

      string baseUri = CleanBaseUrl(baseUrl.ToLower(), '/');
      string adapterBaseUri = CleanBaseUrl(hibernateServiceUri.ToLower(), '/');

      if (!baseUri.Equals(adapterBaseUri))
        return GetServiceClient(baseUrl, serviceName);
      else
        return _hibernateServiceClient;
    }

    private WebHttpClient GetServiceClient(string uri, string serviceName)
    {
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

    private string CleanBaseUrl(string url, char con)
    {
      System.Uri uri = new System.Uri(url);
      return uri.Scheme + ":" + con + con + uri.Host + ":" + uri.Port;
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

    #endregion
  }

  public interface INHibernateRe3pository
  {
    DataDictionary GetDictionary(string contextName, string endpoint, string baseUrl);
  }

}

