using System;
using System.Collections.Generic;
using System.Xml.Linq;
using log4net;
using Ninject;
using System.Web;
using org.iringtools.utility;
using org.iringtools.library;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;
using System.Runtime.Serialization.Json;
using System.IO;

namespace org.iringtools.adapter.projection
{
  public class JsonProjectionEngine : BaseDataProjectionEngine
  {
    private static readonly ILog _logger = LogManager.GetLogger(typeof(JsonProjectionEngine));
    private DataDictionary _dictionary = null;

    [Inject]
    public JsonProjectionEngine(AdapterSettings settings, DataDictionary dictionary, IDataLayer2 dataLayer)
      : base(settings)
    {
      _settings = settings;
      _dictionary = dictionary;
      _dataLayer = dataLayer;
    }

    public override XDocument ToXml(string graphName, ref IList<IDataObject> dataObjects)
    {
      try
      {
        string app = _settings["ApplicationName"].ToLower();
        string proj = _settings["ProjectName"].ToLower();
        string resource = graphName.ToLower();

        DataItems dataItems = new DataItems()
        {          
          total = this.Count,
          start = this.Start,
          items = new List<DataItem>()
        };

        if (dataObjects.Count > 0)
        {
          DataObject dataObject = FindGraphDataObject(graphName);
          dataItems.version = dataObject.version;

          if (dataObject == null)
          {
            return new XDocument();
          }

          bool showNullValue = _settings["ShowJsonNullValues"] != null && 
            _settings["ShowJsonNullValues"].ToString() == "True";

          for (int i = 0; i < dataObjects.Count; i++)
          {
            IDataObject dataObj = dataObjects[i];

            if (dataObj != null)
            {
              if (i == 0)
              {
                dataItems.type = graphName;
              }

              DataItem dataItem = new DataItem()
              {
                properties = new Dictionary<string, string>()
              };

              foreach (DataProperty dataProperty in dataObject.dataProperties)
              {
                object value = dataObj.GetPropertyValue(dataProperty.propertyName);

                if (value != null)
                {
                  string valueStr = Convert.ToString(value);

                  if (dataProperty.dataType == DataType.DateTime)
                    value = Utility.ToXsdDateTime(valueStr);

                  if (!dataProperty.isHidden)
                  {
                    dataItem.properties.Add(dataProperty.propertyName, valueStr);
                  }

                  if (dataObject.isKeyProperty(dataProperty.propertyName))
                  {
                    dataItem.id = valueStr;
                  }
                }
                else if (showNullValue) 
                {
                  dataItem.properties.Add(dataProperty.propertyName, null);
                }
              }

              if (_settings["DisplayLinks"].ToLower() == "true")
              {
                string itemHref = String.Format("{0}/{1}", BaseURI, dataItem.id);

                dataItem.links = new List<Link> 
                {
                  new Link {
                    href = itemHref,
                    rel = "self"
                  }
                };

                foreach (DataRelationship dataRelationship in dataObject.dataRelationships)
                {
                  long relObjCount = 0;
                  bool validateLinks = (_settings["ValidateLinks"].ToLower() == "true");

                  if (validateLinks)
                  {
                    relObjCount = _dataLayer.GetRelatedCount(dataObj, dataRelationship.relatedObjectName);
                  }

                  // only add link for related object that has data
                  if (!validateLinks || relObjCount > 0)
                  {
                    string relObj = dataRelationship.relatedObjectName.ToLower();
                    string relName = dataRelationship.relationshipName.ToLower();

                    Link relLink = new Link()
                    {
                      href = String.Format("{0}/{1}", itemHref, relName),
                      rel = relObj
                    };

                    dataItem.links.Add(relLink);
                  }
                }

              }

              dataItems.items.Add(dataItem);
            }
          }
        }

        dataItems.limit = dataItems.items.Count;

        string xml = Utility.SerializeDataContract<DataItems>(dataItems);
        XElement xElement = XElement.Parse(xml);
        return new XDocument(xElement);
      }
      catch (Exception e)
      {
        _logger.Error("Error creating JSON content: " + e);
        throw e;
      }
    }

    public override XDocument ToXml(string graphName, ref IList<IDataObject> dataObjects, string className, string classIdentifier)
    {
      return ToXml(graphName, ref dataObjects);
    }

    public override IList<IDataObject> ToDataObjects(string graphName, ref XDocument xml)
    {
      try
      {
        IList<IDataObject> dataObjects = new List<IDataObject>();
        DataObject objectDefinition = FindGraphDataObject(graphName);

        if (objectDefinition != null)
        {
          DataItems dataItems = Utility.DeserializeDataContract<DataItems>(xml.ToString());

          foreach (DataItem dataItem in dataItems.items)
          {
            IDataObject dataObject = _dataLayer.Create(graphName, null)[0];

            foreach (var pair in dataItem.properties)
            {
              dataObject.SetPropertyValue(pair.Key, pair.Value);
            }

            dataObjects.Add(dataObject);
          }
        }

        return dataObjects;
      }
      catch (Exception e)
      {
        string message = "Error marshalling data items to data objects." + e;
        _logger.Error(message);
        throw new Exception(message);
      }
    }

    #region helper methods
    private DataObject FindGraphDataObject(string dataObjectName)
    {
      foreach (DataObject dataObject in _dictionary.dataObjects)
      {
        if (dataObject.objectName.ToLower() == dataObjectName.ToLower())
        {
          return dataObject;
        }
      }

      throw new Exception("DataObject [" + dataObjectName + "] does not exist.");
    }

    private bool IsNumeric(DataType dataType)
    {
      return (dataType == DataType.Decimal ||
              dataType == DataType.Single ||
              dataType == DataType.Double ||
              dataType == DataType.Int16 ||
              dataType == DataType.Int32 ||
              dataType == DataType.Int64);
    }
    #endregion
  }
}
