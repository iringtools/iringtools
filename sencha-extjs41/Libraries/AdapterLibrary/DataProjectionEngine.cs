using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Linq;
using log4net;
using Ninject;
using System.Web;
using org.iringtools.utility;
using org.iringtools.library;

namespace org.iringtools.adapter.projection
{
  public class DataProjectionEngine : BaseDataProjectionEngine
  {
    private static readonly ILog _logger = LogManager.GetLogger(typeof(DataProjectionEngine));
    private DataDictionary _dictionary = null;
    private XNamespace _graphNamespace = null;
    private string _graphName = String.Empty;

    [Inject]
    public DataProjectionEngine(AdapterSettings settings, IDataLayer2 dataLayer, DataDictionary dictionary) : base(settings)
    {
      _dataLayer = dataLayer;
      _dictionary = dictionary;
    }

    public override XDocument ToXml(string graphName, ref IList<IDataObject> dataObjects)
    {
      XElement xElement = null;

      try
      {
        string baseUri = _settings["GraphBaseUri"];
        string project = _settings["ProjectName"];
        string app = _settings["ApplicationName"];
        string appBaseUri = Utility.FormEndpointBaseURI(_uriMaps, baseUri, project, app);

        _graphName = graphName;
        _graphNamespace = appBaseUri + graphName + "/";        
        _dataObjects = dataObjects;

        if (_dataObjects != null && (_dataObjects.Count == 1 || FullIndex))
        {
          xElement = new XElement(_graphNamespace + Utility.TitleCase(graphName) + "List");
          DataObject dataObject = FindGraphDataObject(graphName);

          for (int i = 0; i < _dataObjects.Count; i++)
          {
            XElement rowElement = new XElement(_graphNamespace + Utility.TitleCase(dataObject.objectName));
            CreateHierarchicalXml(rowElement, dataObject, i);
            xElement.Add(rowElement);
          }
        }

        if (_dataObjects != null && (_dataObjects.Count > 1 && !FullIndex))
        {
          xElement = new XElement(_graphNamespace + Utility.TitleCase(graphName) + "List");

          XAttribute total = new XAttribute("total", this.Count);
          xElement.Add(total);

          DataObject dataObject = FindGraphDataObject(graphName);

          for (int i = 0; i < _dataObjects.Count; i++)
          {
            XElement rowElement = new XElement(_graphNamespace + Utility.TitleCase(dataObject.objectName));
            CreateIndexXml(rowElement, dataObject, i);
            xElement.Add(rowElement);
          }
        }
      }
      catch (Exception ex)
      {
        throw ex;
      }

      return new XDocument(xElement);
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
          XNamespace ns = xml.Root.Attribute("xmlns").Value;
          string objectName = Utility.TitleCase(objectDefinition.objectName);

          XElement rootEl = xml.Element(ns + objectName + "List");          
          IEnumerable<XElement> objEls = from el in rootEl.Elements(ns + objectName) select el;

          foreach (XElement objEl in objEls)
          {
            IDataObject dataObject = _dataLayer.Create(objectDefinition.objectName, null)[0];

            if (objectDefinition.hasContent)
            {
              XElement xElement = objEl.Element(ns + "content");

              if (xElement != null)
              {
                string base64Content = xElement.Value;

                if (!String.IsNullOrEmpty(base64Content))
                {
                  ((IContentObject)dataObject).content = base64Content.ToMemoryStream();
                }
              }
            }

            foreach (DataProperty property in objectDefinition.dataProperties)
            {
              string propertyName = property.propertyName;
              XElement valueEl = objEl.Element(ns + Utility.TitleCase(propertyName));

              if (valueEl != null)
              {
                string value = valueEl.Value;

                if (value != null)
                {
                  dataObject.SetPropertyValue(propertyName, value);
                }
              }
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
    private void CreateHierarchicalXml(XElement parentElement, DataObject dataObject, int dataObjectIndex)
    {
      foreach(DataProperty dataProperty in dataObject.dataProperties)
      {
        XElement propertyElement = new XElement(_graphNamespace + Utility.TitleCase(dataProperty.propertyName));
        
        var value = _dataObjects[dataObjectIndex].GetPropertyValue(dataProperty.propertyName);
        
        if (value != null)
        {
          if (dataProperty.dataType.ToString().ToLower().Contains("date"))
            value = Utility.ToXsdDateTime(value.ToString());

          propertyElement.Value = value.ToString();

          parentElement.Add(propertyElement);
        }
        
      }

      foreach (DataRelationship dataRelationship in dataObject.dataRelationships)
      {
        XElement relationshipElement = new XElement(_graphNamespace + Utility.TitleCase(dataRelationship.relationshipName));
        IList<IDataObject> relatedObjects = _dataLayer.GetRelatedObjects(_dataObjects[dataObjectIndex], dataRelationship.relatedObjectName);

        parentElement.Add(relationshipElement);
      }
    }

    private void CreateIndexXml(XElement parentElement, DataObject dataObject, int dataObjectIndex)
    {
      string uri = _graphNamespace.ToString();

      if (!uri.EndsWith("/"))
        uri += "/";

      int keyCounter = 0;

      foreach (KeyProperty keyProperty in dataObject.keyProperties)
      {
        DataProperty dataProperty = dataObject.dataProperties.Find(dp => dp.propertyName == keyProperty.keyPropertyName);

        var value = _dataObjects[dataObjectIndex].GetPropertyValue(dataProperty.propertyName);
        if (value != null)
        {
          XElement propertyElement = new XElement(_graphNamespace + Utility.TitleCase(dataProperty.propertyName), value);
          parentElement.Add(propertyElement);
          keyCounter++;

          if (keyCounter == dataObject.keyProperties.Count)
            uri += value;
          else
            uri += value + dataObject.keyDelimeter;
        }
      }

      List<DataProperty> indexProperties = dataObject.dataProperties.FindAll(dp => dp.showOnIndex == true);

      foreach (DataProperty indexProperty in indexProperties)
      {
        var value = _dataObjects[dataObjectIndex].GetPropertyValue(indexProperty.propertyName);
        if (value != null)
        {
          XElement propertyElement = new XElement(_graphNamespace + Utility.TitleCase(indexProperty.propertyName), value);
          parentElement.Add(propertyElement);
        }
      }

      XAttribute uriAttribute = new XAttribute("uri", uri);
      parentElement.Add(uriAttribute);
    }

    public DataObject FindGraphDataObject(string dataObjectName)
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
    #endregion
  }
}
