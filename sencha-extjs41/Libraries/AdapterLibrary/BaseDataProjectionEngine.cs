using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using org.iringtools.library;
using System;
using System.Web;
using org.iringtools.utility;
using log4net;
using System.IO;

namespace org.iringtools.adapter.projection
{
  public abstract class BaseDataProjectionEngine : IProjectionLayer
  {
    private static readonly ILog _logger = LogManager.GetLogger(typeof(BaseDataProjectionEngine));
    
    protected static readonly XNamespace XSD_NS = "http://www.w3.org/2001/XMLSchema#";
    protected static readonly XNamespace XSI_NS = "http://www.w3.org/2001/XMLSchema-instance#";

    protected static readonly string XSD_PREFIX = "xsd:";

    protected IDataLayer2 _dataLayer = null;
    protected AdapterSettings _settings = null;
    protected IList<IDataObject> _dataObjects = null;
    protected List<string> _relatedObjectPaths = null;
    protected Dictionary<string, IList<IDataObject>>[] _relatedObjects = null;
    protected Properties _uriMaps;

    public bool FullIndex { get; set; }
    public long Count { get; set; }
    public int Start { get; set; }
    public int Limit { get; set; }
    public string BaseURI { get; set; }

    public BaseDataProjectionEngine(AdapterSettings settings)
    {
      _settings = settings;

      _dataObjects = new List<IDataObject>();

      // load uri maps config
      _uriMaps = new Properties();

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
    }

    public abstract XDocument ToXml(string graphName, ref IList<IDataObject> dataObjects);
    public abstract XDocument ToXml(string graphName, ref IList<IDataObject> dataObjects, string className, string classIdentifier);
    public abstract IList<IDataObject> ToDataObjects(string graphName, ref XDocument xDocument);

    //propertyPath = "Instrument.LineItems.Tag";
    protected List<IDataObject> GetRelatedObjects(string propertyPath, IDataObject dataObject)
    {
      List<IDataObject> parentObjects = new List<IDataObject>();
      string[] objectPath = propertyPath.Split('.');

      parentObjects.Add(dataObject);

      for (int i = 0; i < objectPath.Length - 1; i++)
      {
        foreach (IDataObject parentObj in parentObjects)
        {
          string objectType = parentObj.GetType().Name;

          if (objectType == typeof(GenericDataObject).Name)
          {
            objectType = ((GenericDataObject)parentObj).ObjectType;
          }

          if (objectType.ToLower() != objectPath[i].ToLower())
          {
            List<IDataObject> relatedObjects = new List<IDataObject>();

            foreach (IDataObject relatedObj in _dataLayer.GetRelatedObjects(parentObj, objectPath[i]))
            {
              if (!relatedObjects.Contains(relatedObj))
              {
                relatedObjects.Add(relatedObj);
              }
            }

            parentObjects = relatedObjects;
          }
        }
      }

      return parentObjects;
    }

    // senario (assume no circular relationships - should be handled by AppEditor): 
    //  dataObject1.L1RelatedDataObjects.L2RelatedDataObjects.LnRelatedDataObjects.property1
    //  dataObject1.L1RelatedDataObjects.L2RelatedDataObjects.LnRelatedDataObjects.property2
    protected void SetObjects(int dataObjectIndex, string propertyPath, List<string> relatedValues)
    {
      Dictionary<string, IList<IDataObject>> relatedObjectDictionary = _relatedObjects[dataObjectIndex];
      int lastDotPosition = propertyPath.LastIndexOf('.');
      string property = propertyPath.Substring(lastDotPosition + 1);
      string objectPathString = propertyPath.Substring(0, lastDotPosition);  // exclude property
      string[] objectPath = objectPathString.Split('.');

      if (!_relatedObjectPaths.Contains(objectPathString))
        _relatedObjectPaths.Add(objectPathString);

      // top level data objects are processed separately, so start with 1
      for (int i = 1; i < objectPath.Length; i++)
      {
        string relatedObjectType = objectPath[i];
        IList<IDataObject> relatedObjects = null;

        if (relatedObjectDictionary.ContainsKey(relatedObjectType))
        {
          relatedObjects = relatedObjectDictionary[relatedObjectType];
        }
        else
        {
          if (i == objectPath.Length - 1)  // last related object in the chain
          {
            relatedObjects = _dataLayer.Create(relatedObjectType, new string[relatedValues.Count]);
          }
          else // intermediate related object
          {
            relatedObjects = _dataLayer.Create(relatedObjectType, null);
          }

          relatedObjectDictionary.Add(relatedObjectType, relatedObjects);
        }

        // only fill last related object values now; values of intermediate related objects' parent might not be available yet.
        if (i == objectPath.Length - 1)
        {
          for (int j = 0; j < relatedValues.Count; j++)
          {
            relatedObjects[j].SetPropertyValue(property, relatedValues[j]);
          }
        }
      }
    }

    // senario:
    //  dataObject1.L1RelatedDataObjects.property1
    //  dataObject1.L1RelatedDataObjects.property2
    //  dataObject1.L1RelatedDataObjects.L2RelatedDataObjects.property3.value1
    //  dataObject1.L1RelatedDataObjects.L2RelatedDataObjects.property4.value2
    //
    // L2RelatedDataObjects result:
    //  dataObject1.L1RelatedDataObjects[1].L2RelatedDataObjects[1].property3.value1
    //  dataObject1.L1RelatedDataObjects[1].L2RelatedDataObjects[2].property4.value2
    //  dataObject1.L1RelatedDataObjects[2].L2RelatedDataObjects[1].property3.value1
    //  dataObject1.L1RelatedDataObjects[2].L2RelatedDataObjects[2].property4.value2
    protected void SetRelatedObjects()
    {
      DataDictionary dictionary = _dataLayer.GetDictionary();

      for (int i = 0; i < _dataObjects.Count; i++)
      {
        Dictionary<string, IList<IDataObject>> relatedObjectDictionary = _relatedObjects[i];

        foreach (string relatedObjectPath in _relatedObjectPaths)
        {
          string[] relatedObjectPathElements = relatedObjectPath.Split('.');

          for (int j = 0; j < relatedObjectPathElements.Length - 1; j++)
          {
            string parentObjectType = relatedObjectPathElements[j];
            string relatedObjectType = relatedObjectPathElements[j + 1];

            if (relatedObjectDictionary.ContainsKey(relatedObjectType))
            {
              IList<IDataObject> parentObjects = null;

              if (j == 0)
                parentObjects = new List<IDataObject> { _dataObjects[i] };
              else
                parentObjects = relatedObjectDictionary[parentObjectType];

              IList<IDataObject> relatedObjects = relatedObjectDictionary[relatedObjectType];

              foreach (IDataObject parentObject in parentObjects)
              {
                DataObject dataObject = dictionary.dataObjects.First(c => c.objectName == parentObjectType);
                DataRelationship dataRelationship = dataObject.dataRelationships.First(c => c.relationshipName == relatedObjectType);

                foreach (IDataObject relatedObject in relatedObjects)
                {
                  foreach (PropertyMap map in dataRelationship.propertyMaps)
                  {
                    relatedObject.SetPropertyValue(map.relatedPropertyName, parentObject.GetPropertyValue(map.dataPropertyName));
                  }
                }
              }
            }
          }
        }
      }
    }
  }
}
