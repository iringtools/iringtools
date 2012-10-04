using System;
using System.Data;
using System.Collections;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq.Expressions;
using System.Xml.Linq;
using Ciloci.Flee;
using log4net;
using Ninject;
using org.iringtools.adapter;
using org.iringtools.library;
using org.iringtools.utility;
using StaticDust.Configuration;
using Ingr.SP3D.Common.Middle.Services;
using Ingr.SP3D.Common.Middle;
using Ingr.SP3D.Structure.Middle;
using Ingr.SP3D.ReferenceData.Middle;
using Ingr.SP3D.Systems.Middle;
using Ingr.SP3D.ReferenceData.Middle.Services;
using NHibernate;
using Ninject.Extensions.Xml;


namespace iringtools.sdk.sp3ddatalayer
{
  public class SP3DDataLayer : BaseDataLayer
  {
    private static readonly ILog _logger = LogManager.GetLogger(typeof(SP3DDataLayer));
    private string _dataPath = string.Empty;
    private string _scope = string.Empty;
    private string _dictionaryPath = string.Empty;
    private string _communityName = string.Empty;
    private string _configurationPath = string.Empty;
    private DatabaseDictionary _databaseDictionary = null;
    private DataDictionary _dataDictionary = null;
    MetadataManager metadataManager = null;
    private BusinessObjectConfiguration _config = null;
    private string projectNameSpace = null;

    [Inject]
    public SP3DDataLayer(AdapterSettings settings)
      : base(settings)
    {
      ServiceSettings servcieSettings = new ServiceSettings();
      _settings.AppendSettings(servcieSettings);

      if (settings["DataLayerPath"] != null)
        _dataPath = settings["DataLayerPath"];
      else    
        _dataPath = settings["AppDataPath"];       

      _scope = _settings["ProjectName"] + "." + _settings["ApplicationName"];
      _settings["BinaryPath"] = @".\Bin\";

      _configurationPath = string.Format("{0}Configuration.{1}.xml", _dataPath, _scope);
      projectNameSpace = "org.iringtools.adapter.datalayer.proj_" + _scope;
      //"org.iringtools.adapter.datalayer.proj_12345_000.ABC"

      _dictionaryPath = string.Format("{0}DataDictionary.{1}.xml", _dataPath, _scope);
    }

    public override DataDictionary GetDictionary()
    {
      if (_dataDictionary == null)
      {
        try
        {
          getConfigure();
          Connect();
          _databaseDictionary = new DatabaseDictionary();
          _databaseDictionary.dataObjects = new List<DataObject>();
          foreach (BusinessObject businessObject in _config.businessObjects)
          {
            DataObject dataObject = CreateDataObject(businessObject);
            _databaseDictionary.dataObjects.Add(dataObject);
          }

          Utility.Write<DatabaseDictionary>(_databaseDictionary, "C:\\temp\\DatabaseDictionary.12345_000.SP3D.xml");

          Generate(_settings["ProjectName"], _settings["ApplicationName"]);
          readDictionary();           
        }
        catch (Exception ex)
        {
          _logger.Error("connect SP3D: " + ex.ToString());
          throw ex;
        }
      }
      return _dataDictionary;
    }

    private void readDictionary()
    {
      _dataDictionary = new DataDictionary();
      _dataDictionary = Utility.Read<DataDictionary>(_dictionaryPath);      
    }

    private void Generate(string projectName, string applicationname)
    {
      if (_databaseDictionary != null && _databaseDictionary.dataObjects != null)
      {
        EntityGenerator generator = new EntityGenerator(_settings);

        string compilerVersion = "v4.0";
        if (!String.IsNullOrEmpty(_settings["CompilerVersion"]))
        {
          compilerVersion = _settings["CompilerVersion"];
        }

        generator.Generate(compilerVersion, _databaseDictionary, projectName, applicationname);
      }
    }

    private DataObject GetDataObject(string objectName)
    {
      DataDictionary dictionary = GetDictionary();
      DataObject dataObject = dictionary.dataObjects.Find(x => x.objectName.ToLower() == objectName.ToLower());
      return dataObject;
    }

    private DataObject CreateDataObject(BusinessObject businessObject)
    {
      string propertyName = string.Empty;
      string keyPropertyName = string.Empty;
      string relatedPropertyName = string.Empty;
      string relatedObjectName = string.Empty;
      string relationshipName = string.Empty;      
      DataObject dataObject = new DataObject();
      string objectName = businessObject.objectName;
      dataObject.objectName = objectName;
      dataObject.objectNamespace = projectNameSpace;
      dataObject.tableName = objectName;
      dataObject.keyProperties = new List<KeyProperty>();
      dataObject.dataProperties = new List<DataProperty>();
      dataObject.dataRelationships = new List<DataRelationship>();

      if (businessObject.dataFilter != null)
        dataObject.dataFilter = businessObject.dataFilter;

      foreach (BusinessKeyProperty businessKeyProerpty in businessObject.businessKeyProperties)
      {
        KeyProperty keyProperty = new KeyProperty();
        DataProperty dataProperty = new DataProperty();
        keyPropertyName = businessKeyProerpty.keyPropertyName;;
        keyProperty.keyPropertyName = keyPropertyName;
        dataProperty.propertyName = keyPropertyName;
        dataProperty.dataType = DataType.String;
        dataProperty.columnName = keyPropertyName;
        dataProperty.isNullable = false;        
        dataProperty.keyType = KeyType.assigned;
        dataObject.keyProperties.Add(keyProperty);
        dataObject.dataProperties.Add(dataProperty);
      }

      foreach (BusinessInterface businessInterface in businessObject.businessInterfaces)
      {
        InterfaceInformation interfaceInfo = GetInterfaceInformation(businessInterface.interfaceName);

        foreach (BusinessProperty businessProperty in businessInterface.businessProperties)
        {
          propertyName = businessProperty.propertyName;

          if (interfaceInfo != null)
          {
            if (HasProperty(interfaceInfo, propertyName))
            {
              DataProperty dataProperty = new DataProperty();

              if (!String.IsNullOrEmpty(businessProperty.dbColumn))
                dataProperty.columnName = businessProperty.dbColumn;
              else
                dataProperty.columnName = propertyName;
              dataProperty.propertyName = propertyName;

              dataProperty.dataType = GetDatatype(businessProperty.dataType);
              dataProperty.isNullable = businessProperty.isNullable;
              dataProperty.isReadOnly = businessObject.isReadOnly;

              if (!String.IsNullOrEmpty(businessProperty.description) != null)
                dataProperty.description = businessObject.description;

              dataObject.dataProperties.Add(dataProperty);
            }
            else
            {
              throw new Exception("Property [" + propertyName + "] not found.");
            }
          }
          else
            throw new Exception("Interface [" + businessInterface.interfaceName + "] not found.");
        }
      }

      foreach (BusinessRelationship businessRelationship in businessObject.businessRelationships)
      {
        DataRelationship dataRelationship = new DataRelationship();
        relationshipName = businessRelationship.relationshipName;
        relatedObjectName = businessRelationship.relatedObjectName;

        if (IsRelated(relationshipName, relatedObjectName, objectName))
        {
          dataRelationship.relatedObjectName = businessRelationship.relatedObjectName;
          dataRelationship.relationshipName = businessRelationship.relationshipName;
          dataRelationship.propertyMaps = new List<PropertyMap>();

          if (businessRelationship.businessRelatedInterfaces != null)
          {
            foreach (BusinessInterface businessInterface in businessRelationship.businessRelatedInterfaces)
            {
              foreach (BusinessProperty businessRelationProperty in businessInterface.businessProperties)
              {
                InterfaceInformation interfaceInfo = GetInterfaceInformation(businessInterface.interfaceName);
                relatedPropertyName = businessRelationProperty.propertyName;

                if (interfaceInfo != null)
                {
                  if (HasProperty(interfaceInfo, relatedPropertyName))
                  {
                    DataProperty dataProperty = new DataProperty();
                    PropertyMap propertyMap = new PropertyMap();
                    propertyMap.relatedPropertyName = relatedPropertyName;
                    dataProperty.propertyName = dataRelationship.relatedObjectName + "_" + relatedPropertyName;
                    dataProperty.dataType = GetDatatype(businessRelationProperty.dataType);

                    if (!String.IsNullOrEmpty(businessRelationProperty.dbColumn))
                      dataProperty.columnName = businessRelationProperty.dbColumn;
                    else
                      dataProperty.columnName = relatedPropertyName;

                    dataRelationship.propertyMaps.Add(propertyMap);
                    dataObject.dataProperties.Add(dataProperty);
                  }
                  else
                  {
                    throw new Exception("Property [" + relatedPropertyName + "] not found.");
                  }
                }
                else
                  throw new Exception("Interface [" + businessInterface.interfaceName + "] not found.");
              }
            }
            dataObject.dataRelationships.Add(dataRelationship);
          }
        }
      }
      return dataObject;
    }

    private bool IsRelated(string relationshipName, string relatedObjectName, string objectName)
    {

    }

    private DataType GetDatatype(string datatype)
    {
      if (datatype == null)
        return DataType.String;

      switch (datatype.ToLower())
      {
        case "string":
          return DataType.String;
        case "bool":
        case "boolean":
          return DataType.Boolean;
        case "float":
        case "decimal":
        case "double":
          return DataType.Double;
        case "integer":
        case "int":
        case "number":
          return DataType.Int64;
        default:
          return DataType.String;
      }      
    }

    private InterfaceInformation GetInterfaceInformation(string propertyInterfaceName)
    {
      InterfaceInformation interfaceInfo = null;
      ReadOnlyDictionary<InterfaceInformation> interfaces = metadataManager.Interfaces;
      string key = LookIntoICollection(interfaces.Keys, propertyInterfaceName);

      if (key != null)
        interfaces.TryGetValue(key, out interfaceInfo);      
      return interfaceInfo;
    }

    private bool HasProperty(InterfaceInformation interfaceInfo, string propertyName)
    {
      ReadOnlyDictionary<PropertyInformation> propertyInformation = interfaceInfo.Properties;
      if (LookIntoICollection(propertyInformation.Keys, propertyName) != null)
        return true;
      else
        return false;      
    }

    private string LookIntoICollection(ICollection<string> collection, string target)
    {
      string fullKey = null;
      System.Collections.IEnumerator keyIe = collection.GetEnumerator();

      while (keyIe.MoveNext())
      {
        fullKey = keyIe.Current.ToString();
        if (fullKey.ToLower().Contains(target.ToLower()))
        {
          return fullKey;
        }
      }

      return null;
    }

    private void getConfigure()
    {
      if (_config == null)
        _config = Utility.Read<BusinessObjectConfiguration>(_configurationPath);
    }

    public override IList<IDataObject> Get(string objectType, DataFilter filter, int pageSize, int startIndex)
    {
      ISession session = NHibernateSessionManager.Instance.GetSession(_settings["AppDataPath"], _settings["Scope"]);

      try
      {
        if (_databaseDictionary.IdentityConfiguration != null)
        {
          IdentityProperties identityProperties = _databaseDictionary.IdentityConfiguration[objectType];
          if (identityProperties.UseIdentityFilter)
          {
            filter = FilterByIdentity(objectType, filter, identityProperties);
          }
        }

        DataObject objectDefinition = _databaseDictionary.dataObjects.Find(x => x.objectName.ToUpper() == objectType.ToUpper());

        if (objectDefinition == null)
        {
          throw new Exception("Object type [" + objectType + "] not found.");
        }

        string ns = String.IsNullOrEmpty(objectDefinition.objectNamespace)
          ? String.Empty : (objectDefinition.objectNamespace + ".");

        Type type = Type.GetType(ns + objectType + ", " + _settings["ExecutingAssemblyName"]);

        // make an exception for tests
        if (type == null)
        {
          type = Type.GetType(ns + objectType + ", NUnit.Tests");
        }

        ICriteria criteria = NHibernateUtility.CreateCriteria(session, type, objectDefinition, filter);

        if (pageSize == 0 && startIndex == 0)
        {
          List<IDataObject> dataObjects = new List<IDataObject>();
          long totalCount = GetCount(objectType, filter);
          int internalPageSize = (_settings["InternalPageSize"] != null) ? int.Parse(_settings["InternalPageSize"]) : 1000;
          int numOfRows = 0;

          while (numOfRows < totalCount)
          {
            criteria.SetFirstResult(numOfRows).SetMaxResults(internalPageSize);
            dataObjects.AddRange(criteria.List<IDataObject>());
            numOfRows += internalPageSize;
          }

          return dataObjects;
        }
        else
        {
          criteria.SetFirstResult(startIndex).SetMaxResults(pageSize);
          IList<IDataObject> dataObjects = criteria.List<IDataObject>();
          return dataObjects;
        }
      }
      catch (Exception ex)
      {
        _logger.Error("Error in Get: " + ex);
        throw new Exception(string.Format("Error while getting a list of data objects of type [{0}]. {1}", objectType, ex));
      }
      finally
      {
        CloseSession(session);
      }
    }

    private void CloseSession(ISession session)
    {
      try
      {
        if (session != null)
        {
          session.Close();
          session = null;
        }
      }
      catch (Exception ex)
      {
        _logger.Error("Error closing NHibernate session: " + ex);
      }
    }

    private DataFilter FilterByIdentity(string objectType, DataFilter filter, IdentityProperties identityProperties)
    {
      DataObject dataObject = _databaseDictionary.dataObjects.Find(d => d.objectName == objectType);
      DataProperty dataProperty = dataObject.dataProperties.Find(p => p.columnName == identityProperties.IdentityProperty);

      if (dataProperty != null)
      {
        if (filter == null)
        {
          filter = new DataFilter();
        }

        //bool hasExistingExpression = false;

        if (filter.Expressions == null)
        {
          filter.Expressions = new List<org.iringtools.library.Expression>();
        }
        else if (filter.Expressions.Count > 0)
        {
          org.iringtools.library.Expression firstExpression = filter.Expressions.First();
          org.iringtools.library.Expression lastExpression = filter.Expressions.Last();
          firstExpression.OpenGroupCount++;
          lastExpression.CloseGroupCount++;
          //hasExistingExpression = true;
        }

        //string identityValue = _keyRing[identityProperties.KeyRingProperty].ToString();

        //org.iringtools.library.Expression expression = new org.iringtools.library.Expression
        //{
        //  PropertyName = dataProperty.propertyName,
        //  RelationalOperator = RelationalOperator.EqualTo,
        //  Values = new Values
        //  {
        //    identityValue,
        //  },
        //  IsCaseSensitive = identityProperties.IsCaseSensitive
        //};

        //if (hasExistingExpression)
        //  expression.LogicalOperator = LogicalOperator.And;
        //filter.Expressions.Add(expression);
      }

      return filter;
    }

    private void Connect()
    {
      Site SP3DSite = null;
      SP3DSite = MiddleServiceProvider.SiteMgr.ConnectSite();
      
      if (SP3DSite != null)
      {
        if( SP3DSite.Plants.Count > 0 )
          MiddleServiceProvider.SiteMgr.ActiveSite.OpenPlant((Plant)SP3DSite.Plants[0]);
      }

      Catalog SP3DCatalog = null;
      SP3DCatalog = MiddleServiceProvider.SiteMgr.ActiveSite.ActivePlant.PlantCatalog;
      metadataManager = SP3DCatalog.MetadataMgr;

      //string displayName, name, showupMsg = "", category, iid, interfaceInfoNamespace, propertyName, propertyDescriber;
      //string propertyInterfaceInformationStr, unitTypeString;
      //Type type;
      //ReadOnlyDictionary<InterfaceInformation> interfactInfo, commonInterfaceInfo;
      //ReadOnlyDictionary<PropertyInformation> properties;
      //ReadOnlyDictionary<BOCInformation> oSystemsByName = metadataManager.BOCs;
      //bool complex, comAccess, displayedOnPage, isvalueRequired, metaDataAccess, metadataReadOnly, SqlAccess;
      //string propertyDisplayName, proPropertyName, uomType;
      //CodelistInformation codeListInfo;
      //InterfaceInformation propertyInterfaceInformation;
      //SP3DPropType sp3dProType;
      //UnitType unitType;
      //string showupPropertyMessage = "";
      //string showupProMsg = "";
      //foreach (string key in oSystemsByName.Keys)
      //{
      //  BOCInformation bocInfo = null;
      //  oSystemsByName.TryGetValue(key, out bocInfo);
      //  displayName = bocInfo.DisplayName;
      //  name = bocInfo.Name;
      //  type = bocInfo.GetType();
      //  interfactInfo = bocInfo.DefiningInterfaces;
      //  foreach (string infoKey in interfactInfo.Keys)
      //  {
      //    InterfaceInformation itemInterfaceInfo;
      //    interfactInfo.TryGetValue(infoKey, out itemInterfaceInfo);
      //    interfaceInfoNamespace = itemInterfaceInfo.Namespace;
      //    category = itemInterfaceInfo.Category;
      //    iid = itemInterfaceInfo.IID;
      //    properties = itemInterfaceInfo.Properties;

      //    foreach (string propertyKey in properties.Keys)
      //    {
      //      PropertyInformation propertyInfo;
      //      properties.TryGetValue(propertyKey, out propertyInfo);
      //      complex = propertyInfo.Complex;

      //      codeListInfo = propertyInfo.CodeListInfo;
      //      comAccess = propertyInfo.COMAccess;
      //      displayedOnPage = propertyInfo.DisplayedOnPropertyPage;
      //      propertyDisplayName = propertyInfo.DisplayName;
      //      propertyInterfaceInformation = propertyInfo.InterfaceInfo;
      //      propertyInterfaceInformationStr = propertyInterfaceInformation.ToString();
      //      isvalueRequired = propertyInfo.IsValueRequired;
      //      metaDataAccess = propertyInfo.MetadataAccess;
      //      metadataReadOnly = propertyInfo.MetadataReadOnly;
      //      proPropertyName = propertyInfo.Name;
      //      sp3dProType = propertyInfo.PropertyType;
      //      SqlAccess = propertyInfo.SQLAccess;
      //      unitType = propertyInfo.UOMType;
      //      unitTypeString = unitType.ToString();

      //      showupPropertyMessage = showupPropertyMessage + "\n propertyInfo.key: " + propertyKey + "\n"
      //                            + "CodeListInfo.DisplayName: " + codeListInfo.DisplayName + "\n"
      //                            + "comAccess: " + comAccess + "\n"
      //                            + "propertyDisplayName: " + propertyDisplayName + "\n"
      //                            + "propertyInterfaceInformation: " + propertyInterfaceInformation.Name + "\n"
      //                            + "proPropertyName: " + proPropertyName;


      //    }
      //  }

      //  commonInterfaceInfo = bocInfo.CommonInterfaces;
      //  foreach (string comInfoKey in commonInterfaceInfo.Keys)
      //  {
      //    InterfaceInformation comItemInterfaceInfo;
      //    commonInterfaceInfo.TryGetValue(comInfoKey, out comItemInterfaceInfo);
      //    interfaceInfoNamespace = comItemInterfaceInfo.Namespace;
      //    category = comItemInterfaceInfo.Category;
      //    iid = comItemInterfaceInfo.IID;
      //    properties = comItemInterfaceInfo.Properties;
          
      //    foreach (string propertyKey in properties.Keys)
      //    {
      //      PropertyInformation propertyInfo;
      //      properties.TryGetValue(propertyKey, out propertyInfo);
      //      complex = propertyInfo.Complex;
           
      //      codeListInfo = propertyInfo.CodeListInfo;
      //      comAccess = propertyInfo.COMAccess;
      //      displayedOnPage = propertyInfo.DisplayedOnPropertyPage;
      //      propertyDisplayName = propertyInfo.DisplayName;
      //      propertyInterfaceInformation = propertyInfo.InterfaceInfo;
      //      propertyInterfaceInformationStr = propertyInterfaceInformation.ToString();
      //      isvalueRequired = propertyInfo.IsValueRequired;
      //      metaDataAccess = propertyInfo.MetadataAccess;
      //      metadataReadOnly = propertyInfo.MetadataReadOnly;
      //      proPropertyName = propertyInfo.Name;
      //      sp3dProType = propertyInfo.PropertyType;
      //      SqlAccess = propertyInfo.SQLAccess;
      //      unitType = propertyInfo.UOMType;
      //      unitTypeString = unitType.ToString();

      //      showupProMsg = showupProMsg + "\n propertyInfo.key: " + propertyKey + "\n"
      //                            + "CodeListInfo.DisplayName: " + codeListInfo.DisplayName + "\n"
      //                            + "comAccess: " + comAccess + "\n"
      //                            + "propertyDisplayName: " + propertyDisplayName + "\n"
      //                            + "propertyInterfaceInformation: " + propertyInterfaceInformation.Name + "\n"
      //                            + "proPropertyName: " + proPropertyName;          


      //    }


      //  }
        
      //  showupMsg = showupMsg + "\n bocInfo.key: " + key + "\n"
      //            + "bocInfo.DisplayName: " + displayName + "\n"
      //            + "bocInfo.Name: " + name + "\n"
      //            + "bocInfo.type: " + type.FullName + "\n"
      //           // + "bocInfo.DefiningInterfaces: " + showupPropertyMessage + "\n"
      //            + "bocInfo.commonInterfaceInfo: " + showupProMsg + "\n";                 

      //}
      //File.WriteAllText(@"C:\temp\sp3d.txt", showupMsg);

      //System.Windows.Forms.MessageBox.Show(showupMsg);
      //oSystemsByName
    }

    public override long GetCount(string objectType, DataFilter filter)
    {
            try
            {
              Connect();

              //NOTE: pageSize of 0 indicates that all rows should be returned.
              //IList<IDataObject> dataObjects = Get(objectType, filter, 0, 0);

                //return dataObjects.Count();
              return 5;
            }
            catch (Exception ex)
            {
                _logger.Error("Error in GetIdentifiers: " + ex);

                throw new Exception(
                  "Error while getting a count of type [" + objectType + "].",
                  ex
                );
            }
        }

        public override IList<string> GetIdentifiers(string objectType, DataFilter filter)
        {
          throw new Exception("Error while getting a count of type ");
            //try
            //{
            //    List<string> identifiers = new List<string>();

            //    //NOTE: pageSize of 0 indicates that all rows should be returned.
            //    IList<IDataObject> dataObjects = Get(objectType, filter, 0, 0);

            //    foreach (IDataObject dataObject in dataObjects)
            //    {
            //        identifiers.Add((string)dataObject.GetPropertyValue("Tag"));
            //    }

            //    return identifiers;
            //}
            //catch (Exception ex)
            //{
            //    _logger.Error("Error in GetIdentifiers: " + ex);

            //    throw new Exception(
            //      "Error while getting a list of identifiers of type [" + objectType + "].",
            //      ex
            //    );
            //}
        }

       

       

        protected DataObject GetObjectDefinition(string objectType)
        {
          DataDictionary dictionary = GetDictionary();
          DataObject objDef = dictionary.dataObjects.Find(x => x.objectName.ToLower() == objectType.ToLower());
          return objDef;
        }      

        public override Response Post(IList<IDataObject> dataObjects)
        {
          Response response = new Response();
          return response;
        }       

        public override Response Refresh(string objectType)
        {
          return RefreshAll();
        }

        public override Response RefreshAll()
        {
          Response response = new Response();

          try
          {
            _databaseDictionary = null;
            System.IO.File.Delete(_dictionaryPath);
            GetDictionary();
            response.Level = StatusLevel.Success;
          }
          catch (Exception e)
          {
            response.Level = StatusLevel.Error;
            response.Messages = new Messages() { e.Message };
          }

          return response;
        }

        public override IList<IDataObject> GetRelatedObjects(IDataObject dataObject, string relatedObjectType)
        {
            throw new NotImplementedException();
        }        

        public override Response Delete(string objectType, IList<string> identifiers)
        {
          throw new Exception("Error while getting a count of type ");
          //// Not gonna do it. Wouldn't be prudent.
          //Response response = new Response();
          //Status status = new Status();
          //status.Level = StatusLevel.Error;
          //status.Messages.Add("Delete not supported by the SP3D DataLayer.");
          //response.Append(status);
          //return response;
        }

        public override Response Delete(string objectType, DataFilter filter)
        {
          throw new Exception("Error while getting a count of type ");
          //// Not gonna do it. Wouldn't be prudent with a filter either.
          //Response response = new Response();
          //Status status = new Status();
          //status.Level = StatusLevel.Error;
          //status.Messages.Add("Delete not supported by the SP3D DataLayer.");
          //response.Append(status);
          //return response;
        }     

        private void LoadConfiguration()
        {
            if (_config == null)
            {
                string uri = String.Format(
                    "{0}Configuration.{1}.xml",
                    _settings["XmlPath"],
                    _settings["ApplicationName"]
                );

                XElement configDocument = Utility.ReadXml(uri);
                _config = Utility.DeserializeDataContract<BusinessObjectConfiguration>(configDocument.ToString());
            }
        }

        private IList<IDataObject> LoadDataObjects(string objectType)
        {
            try
            {
                IList<IDataObject> dataObjects = new List<IDataObject>();

                //Get Path from Scope.config ({project}.{app}.config)
                string path = String.Format(
                    "{0}{1}\\{2}.csv",
                     _settings["BaseDirectoryPath"],
                    _settings["SP3DFolderPath"],
                    objectType
                );

                IDataObject dataObject = null;
                TextReader reader = new StreamReader(path);
                while (reader.Peek() >= 0)
                {
                    string csvRow = reader.ReadLine();

                    dataObject = FormDataObject(objectType, csvRow);

                    if (dataObject != null)
                        dataObjects.Add(dataObject);
                }
                reader.Close();

                return dataObjects;
            }
            catch (Exception ex)
            {
                _logger.Error("Error in LoadDataObjects: " + ex);
                throw new Exception("Error while loading data objects of type [" + objectType + "].", ex);
            }
        }

        private IDataObject FormDataObject(string objectType, string csvRow)
        {
            try
            {
                IDataObject dataObject = new GenericDataObject
                {
                    ObjectType = objectType,
                };

                XElement commodityElement = new XElement("a");
                  //GetCommodityConfig(objectType);

                if (!String.IsNullOrEmpty(csvRow))
                {
                    IEnumerable<XElement> attributeElements = commodityElement.Element("attributes").Elements("attribute");

                    string[] csvValues = csvRow.Split(',');

                    int index = 0;
                    foreach (var attributeElement in attributeElements)
                    {
                        string name = attributeElement.Attribute("name").Value;
                        string dataType = attributeElement.Attribute("dataType").Value.ToLower();
                        string value = csvValues[index++].Trim();

                        // if data type is not nullable, make sure it has a value
                        if (!(dataType.EndsWith("?") && value == String.Empty))
                        {
                            if (dataType.Contains("bool"))
                            {
                                if (value.ToUpper() == "TRUE" || value.ToUpper() == "YES")
                                {
                                    value = "1";
                                }
                                else
                                {
                                    value = "0";
                                }
                            }
                            else if (value == String.Empty && (
                                     dataType.StartsWith("int") ||
                                     dataType == "double" ||
                                     dataType == "single" ||
                                     dataType == "float" ||
                                     dataType == "decimal"))
                            {
                                value = "0";
                            }
                        }

                        dataObject.SetPropertyValue(name, value);
                    }
                }

                return dataObject;
            }
            catch (Exception ex)
            {
                _logger.Error("Error in FormDataObject: " + ex);

                throw new Exception(
                  "Error while forming a dataObject of type [" + objectType + "] from SPPID.",
                  ex
                );
            }
        }        

        private Response SaveDataObjects(string objectType, IList<IDataObject> dataObjects)
        {
            try
            {
                Response response = new Response();

                // Create data object directory in case it does not exist
                Directory.CreateDirectory(_settings["SP3DFolderPath"]);

                string path = String.Format(
                 "{0}{1}\\{2}.csv",
                   _settings["BaseDirectoryPath"],
                  _settings["SP3DFolderPath"],
                  objectType
                );

                //TODO: Need to update file, not replace it!
                TextWriter writer = new StreamWriter(path);

                foreach (IDataObject dataObject in dataObjects)
                {
                    Status status = new Status();

                    try
                    {
                        string identifier = GetIdentifier(dataObject);
                        status.Identifier = identifier;

                        List<string> csvRow = new List<string>();
                          //FormCSVRow(objectType, dataObject);

                        writer.WriteLine(String.Join(", ", csvRow.ToArray()));
                        status.Messages.Add("Record [" + identifier + "] has been saved successfully.");
                    }
                    catch (Exception ex)
                    {
                        status.Level = StatusLevel.Error;

                        string message = String.Format(
                          "Error while posting dataObject [{0}]. {1}",
                          dataObject.GetPropertyValue("Tag"),
                          ex.ToString()
                        );

                        status.Messages.Add(message);
                    }

                    response.Append(status);
                }

                writer.Close();

                return response;
            }
            catch (Exception ex)
            {
                _logger.Error("Error in LoadDataObjects: " + ex);
                throw new Exception("Error while loading data objects of type [" + objectType + "].", ex);
            }
        }       

        public override IList<IDataObject> Get(string objectType, IList<string> identifiers)
        {
          throw new Exception("Error while getting a count of type ");
            //try
            //{
            //    LoadDataDictionary(objectType);

            //    IList<IDataObject> allDataObjects = LoadDataObjects(objectType);

            //    var expressions = FormMultipleKeysPredicate(identifiers);

            //    if (expressions != null)
            //    {
            //        _dataObjects = allDataObjects.AsQueryable().Where(expressions).ToList();
            //    }

            //    return _dataObjects;
            //}
            //catch (Exception ex)
            //{
            //    _logger.Error("Error in GetList: " + ex);
            //    throw new Exception("Error while getting a list of data objects of type [" + objectType + "].", ex);
            //}
        }
    }
}







