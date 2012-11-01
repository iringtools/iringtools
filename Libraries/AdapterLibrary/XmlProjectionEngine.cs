using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using org.iringtools.library;
using System.Xml;
using System.Xml.Linq;
using Ninject;
using log4net;
using System.Text.RegularExpressions;
using VDS.RDF;
using VDS.RDF.Storage;
using org.iringtools.utility;
using org.iringtools.mapping;
using org.iringtools.dxfr.manifest;
using System.Web;

namespace org.iringtools.adapter.projection
{
  public class XmlProjectionEngine : BasePart7ProjectionEngine
  {
    private static readonly ILog _logger = LogManager.GetLogger(typeof(XmlProjectionEngine));
    private static readonly string ID_ATTR = "id";
    private static readonly string RDL_URI_ATTR = "rdlUri";
    private static readonly string POSSESSOR_ATTR = "possessorRole";
    private static readonly string REF_ATTR = "reference";

    private Dictionary<string, List<string>> _individualsCache = null;
    private XNamespace _appNamespace = null;

    [Inject]
    public XmlProjectionEngine(AdapterSettings settings, IDataLayer2 dataLayer, Mapping mapping)
      : base(settings, dataLayer, mapping)
    {
      _individualsCache = new Dictionary<string, List<string>>();

      string baseUri = _settings["GraphBaseUri"];
      string project = _settings["ProjectName"];
      string app = _settings["ApplicationName"];
      _appNamespace = Utility.FormEndpointBaseURI(_uriMaps, baseUri, project, app);
    }

    public override XDocument ToXml(string graphName, ref IList<IDataObject> dataObjects)
    {
      XElement rootElement = null;

      try
      {
        _graphMap = _mapping.FindGraphMap(graphName);
        _dataObjects = dataObjects;

        if (_graphMap != null && _graphMap.classTemplateMaps.Count > 0 &&
            _dataObjects != null && _dataObjects.Count > 0)
        {
          if (_dataObjects.Count == 1 || FullIndex)
          {
            rootElement = new XElement(_appNamespace + Utility.TitleCase(graphName),
             new XAttribute(XNamespace.Xmlns + "i", XSI_NS),
             new XAttribute(XNamespace.Xmlns + "rdl", RDL_NS),
             new XAttribute(XNamespace.Xmlns + "tpl", TPL_NS),
             new XAttribute(XNamespace.Xmlns + "rdf", RDF_NS));

            BuildXml(rootElement, String.Empty, String.Empty);
          }
          else
          {
            ClassMap classMap = _graphMap.classTemplateMaps.First().classMap;
            rootElement = new XElement(_appNamespace + Utility.TitleCase(graphName));
            
            for (int dataObjectIndex = 0; dataObjectIndex < _dataObjects.Count; dataObjectIndex++)
            {
              bool hasRelatedProperty;
              List<string> classIdentifiers = GetClassIdentifiers(classMap, dataObjectIndex, out hasRelatedProperty);

              if (classIdentifiers.Count > 0)
              {
                XElement rowElement = new XElement(_appNamespace + Utility.TitleCase(classMap.name));
                rowElement.Value = _appNamespace.ToString() + "/" + _graphMap.name + "/" + classIdentifiers.First();
                rootElement.Add(rowElement);
              }
              else
              {
                _logger.Warn("Class identifier of [" + classMap.name + "] not found.");
              }
            }
          }

          XAttribute total = new XAttribute("total", this.Count);
          rootElement.Add(total);
        }
      }
      catch (Exception ex)
      {
        _logger.Error("Error in ToXml: " + ex);
        throw ex;
      }

      return new XDocument(rootElement);
    }

    public override XDocument ToXml(string graphName, ref IList<IDataObject> dataObjects, string className, string classIdentifier)
    {
      XElement rootElement = null;

      try
      {
        _graphMap = _mapping.FindGraphMap(graphName);

        if (_graphMap != null && _graphMap.classTemplateMaps.Count > 0 && dataObjects != null)
        {
          rootElement = new XElement(_appNamespace + Utility.TitleCase(graphName),
            new XAttribute(XNamespace.Xmlns + "i", XSI_NS),
            new XAttribute(XNamespace.Xmlns + "rdl", RDL_NS),
            new XAttribute(XNamespace.Xmlns + "tpl", TPL_NS),
            new XAttribute(XNamespace.Xmlns + "rdf", RDF_NS));
         
          _dataObjects = dataObjects;
          BuildXml(rootElement, className, classIdentifier);
        }
      }
      catch (Exception ex)
      {
        _logger.Error("Error in ToXml: " + ex);
        throw ex;
      }

      return new XDocument(rootElement);
    }

    public override IList<IDataObject> ToDataObjects(string graphName, ref XDocument xml)
    {
      throw new NotImplementedException();
    }

    #region helper methods
    private void BuildXml(XElement rootElement, string startClassName, string startClassIdentifier)
    {
      ClassTemplateMap classTemplateMap = String.IsNullOrEmpty(startClassName) ?
        _graphMap.classTemplateMaps.First() : _graphMap.GetClassTemplateMapByName(startClassName);

      if (classTemplateMap != null)
      {
        ClassMap classMap = classTemplateMap.classMap;
        List<TemplateMap> templateMaps = classTemplateMap.templateMaps;

        if (classMap != null)
        {
          for (int dataObjectIndex = 0; dataObjectIndex < _dataObjects.Count; dataObjectIndex++)
          {
            bool hasRelatedProperty;
            List<string> classIdentifiers = GetClassIdentifiers(classMap, dataObjectIndex, out hasRelatedProperty);

            ProcessOutboundClass(dataObjectIndex, startClassName, startClassIdentifier, true, classIdentifiers, 
              hasRelatedProperty, rootElement, classTemplateMap.classMap, classTemplateMap.templateMaps);
          }
        }
      }
      else
      {
        throw new Exception("Class [" + startClassName + "] not found.");
      }
    }

    private void ProcessOutboundClass(int dataObjectIndex, string startClassName, string startClassIdentifier, bool isRootClass,
      List<string> classIdentifiers, bool hasRelatedProperty, XElement parentElement, ClassMap classMap, List<TemplateMap> templateMaps)
    {
      string className = Utility.TitleCase(classMap.name);

      for (int classIdentifierIndex = 0; classIdentifierIndex < classIdentifiers.Count; classIdentifierIndex++)
      {
        string classIdentifier = classIdentifiers[classIdentifierIndex];

        if (String.IsNullOrEmpty(startClassIdentifier) || className != startClassName || classIdentifier == startClassIdentifier)
        {
          XElement individualElement = CreateIndividualElement(isRootClass, parentElement, classMap.id, 
            Utility.TitleCase(classMap.name), classIdentifier);

          if (individualElement != null)
          {
            parentElement.Add(individualElement);

            // add primary classification template
            if (isRootClass && _primaryClassificationStyle == ClassificationStyle.Both)
            {
              TemplateMap classificationTemplate = _classificationConfig.TemplateMap;

              CreateTemplateElement(dataObjectIndex, startClassName, startClassIdentifier, classIdentifierIndex, 
                individualElement, classificationTemplate, hasRelatedProperty);
            }

            ProcessOutboundTemplates(dataObjectIndex, startClassName, startClassIdentifier, classIdentifierIndex,
              individualElement, templateMaps, hasRelatedProperty);
          }
        }
      }
    }

    private void ProcessOutboundTemplates(int dataObjectIndex, string startClassName, string startClassIdentifier, 
      int classIdentifierIndex, XElement individualElement, List<TemplateMap> templateMaps, bool hasRelatedProperty)
    {
      if (templateMaps != null && templateMaps.Count > 0)
      {
        foreach (TemplateMap templateMap in templateMaps)
        {
          CreateTemplateElement(dataObjectIndex, startClassName, startClassIdentifier, classIdentifierIndex,
            individualElement, templateMap, hasRelatedProperty);
        }
      }
    }

    private XElement CreateIndividualElement(bool isRootClass, XElement parentElement, string classId, 
      string className, string classIdentifier)
    {
      XElement individualElement = null;

      if (!String.IsNullOrEmpty(classIdentifier))
      {
        string individual = _appNamespace.NamespaceName + classIdentifier;
        bool individualCreated = true;

        if (!_individualsCache.ContainsKey(classId))
        {
          _individualsCache[classId] = new List<string> { individual };
          individualCreated = false;
        }
        else if (!_individualsCache[classId].Contains(individual))
        {
          _individualsCache[classId].Add(individual);
          individualCreated = false;
        }

        if (!individualCreated)
        {
          individualElement = new XElement(_appNamespace + className);
          individualElement.Add(new XAttribute(RDL_URI_ATTR, classId));
          individualElement.Add(new XAttribute(ID_ATTR, classIdentifier));
        }
        else if (!isRootClass)
        {
          parentElement.Add(new XAttribute(REF_ATTR, "#" + classIdentifier));
        }
      }

      return individualElement;
    }

    private void CreateTemplateElement(int dataObjectIndex, string startClassName, string startClassIdentifier,
      int classIdentifierIndex, XElement individualElement, TemplateMap templateMap, bool classIdentifierHasRelatedProperty)
    {
      IDataObject dataObject = _dataObjects[dataObjectIndex];

      List<RoleMap> classRoles = new List<RoleMap>();
      List<RoleMap> propertyRoles = new List<RoleMap>();

      XElement baseTemplateElement = new XElement(_appNamespace + templateMap.name);
      baseTemplateElement.Add(new XAttribute(RDL_URI_ATTR, templateMap.id));

      foreach (RoleMap roleMap in templateMap.roleMaps)
      {
        XElement roleElement = new XElement(_appNamespace + roleMap.name);

        switch (roleMap.type)
        {
          case RoleType.Possessor:
            baseTemplateElement.Add(new XAttribute(POSSESSOR_ATTR, roleMap.id));
            break;

          case RoleType.Reference:
            if (roleMap.classMap != null)
            {
              classRoles.Add(roleMap);
            }
            else
            {
              roleElement.Add(new XAttribute(RDL_URI_ATTR, roleMap.id));
              roleElement.Add(new XAttribute(REF_ATTR, roleMap.value));
              baseTemplateElement.Add(roleElement);
            }
            break;

          case RoleType.FixedValue:
            roleElement.Add(new XAttribute(RDL_URI_ATTR, roleMap.id));
            roleElement.Add(new XText(roleMap.value));
            baseTemplateElement.Add(roleElement);
            break;

          case RoleType.Property:
          case RoleType.DataProperty:
          case RoleType.ObjectProperty:
            if (String.IsNullOrEmpty(roleMap.propertyName))
            {
              throw new Exception("No data property mapped to role [" + startClassName + "." + templateMap.name + "." + roleMap.name + "]");
            }
            propertyRoles.Add(roleMap);
            break;
        }
      }

      if (propertyRoles.Count > 0)  // property template
      {
        bool isTemplateValid = true;  // template is not valid when value list uri is empty
        List<List<XElement>> multiPropertyElements = new List<List<XElement>>();
       
        // create property elements
        foreach (RoleMap propertyRole in propertyRoles)
        {
          List<XElement> propertyElements = new List<XElement>();
          multiPropertyElements.Add(propertyElements);

          string[] propertyParts = propertyRole.propertyName.Split('.');
          string propertyName = propertyParts[propertyParts.Length - 1];

          int lastDotPos = propertyRole.propertyName.LastIndexOf('.');
          string objectPath = propertyRole.propertyName.Substring(0, lastDotPos);

          if (propertyParts.Length == 2)  // direct property
          {
            string propertyValue = Convert.ToString(dataObject.GetPropertyValue(propertyName));
            XElement propertyElement = CreatePropertyElement(propertyRole, propertyValue);

            if (propertyElement == null)
            {
              isTemplateValid = false;
              break;
            }

            propertyElements.Add(propertyElement);
          }
          else  // related property
          {
            string key = objectPath + "." + dataObjectIndex;
            List<IDataObject> relatedObjects = null;

            if (!_relatedObjectsCache.TryGetValue(key, out relatedObjects))
            {
              relatedObjects = GetRelatedObjects(propertyRole.propertyName, dataObject);
              _relatedObjectsCache.Add(key, relatedObjects);
            }

            if (classIdentifierHasRelatedProperty)  // reference class identifier has related property
            {
              IDataObject relatedObject = relatedObjects[classIdentifierIndex];
              string propertyValue = Convert.ToString(relatedObject.GetPropertyValue(propertyName));
              XElement propertyElement = CreatePropertyElement(propertyRole, propertyValue);

              if (propertyElement == null)
              {
                isTemplateValid = false;
                break;
              }

              propertyElements.Add(propertyElement);
            }
            else  // related property is property map
            {
              foreach (IDataObject relatedObject in relatedObjects)
              {
                string propertyValue = Convert.ToString(relatedObject.GetPropertyValue(propertyName));
                XElement propertyElement = CreatePropertyElement(propertyRole, propertyValue);

                if (propertyElement == null)
                {
                  isTemplateValid = false;
                  break;
                }

                propertyElements.Add(propertyElement);
              }

              if (!isTemplateValid) break;
            }
          }
        }

        if (isTemplateValid)
        {
          // add property elements to template element(s)
          if (multiPropertyElements.Count > 0 && multiPropertyElements[0].Count > 0)
          {
            for (int i = 0; i < multiPropertyElements[0].Count; i++)
            {
              XElement templateElement = new XElement(baseTemplateElement);
              individualElement.Add(templateElement);

              for (int j = 0; j < multiPropertyElements.Count; j++)
              {
                XElement propertyElement = multiPropertyElements[j][i];
                templateElement.Add(propertyElement);
              }
            }
          }
        }
      }
      else if (classRoles.Count > 0)  // reference template with known class role
      {
        bool isTemplateValid = false;  // at least one class role identifier is not null or empty

        foreach (RoleMap classRole in classRoles)
        {
          XElement roleElement = new XElement(_appNamespace + classRole.name);

          ClassTemplateMap relatedClassTemplateMap = _graphMap.GetClassTemplateMap(classRole.classMap.id);
          bool refClassHasRelatedProperty;
          List<string> refClassIdentifiers = GetClassIdentifiers(classRole.classMap, dataObjectIndex, out refClassHasRelatedProperty);

          if (refClassIdentifiers.Count > 0 && !String.IsNullOrEmpty(refClassIdentifiers.First()))
          {
            isTemplateValid = true;
            roleElement.Add(new XAttribute(RDL_URI_ATTR, classRole.id));
            baseTemplateElement.Add(roleElement);

            if (relatedClassTemplateMap != null && relatedClassTemplateMap.classMap != null)
            {
              ProcessOutboundClass(dataObjectIndex, startClassName, startClassIdentifier, false, refClassIdentifiers,
                refClassHasRelatedProperty, roleElement, relatedClassTemplateMap.classMap, relatedClassTemplateMap.templateMaps);
            }
            else
            {
              roleElement.Add(new XAttribute(REF_ATTR, "#" + refClassIdentifiers.First()));
            }
          }
        }

        if (isTemplateValid)
          individualElement.Add(baseTemplateElement);
      }
      else  // reference template with no class role (e.g. primary classification template)
      {
        individualElement.Add(baseTemplateElement);
      }
    }

    private XElement CreatePropertyElement(RoleMap propertyRole, string propertyValue)
    {
      XElement propertyElement = new XElement(_appNamespace + propertyRole.name);
      propertyElement.Add(new XAttribute(RDL_URI_ATTR, propertyRole.id));

      if (String.IsNullOrEmpty(propertyRole.valueListName))
      {
        if (String.IsNullOrEmpty(propertyValue))
        {
          propertyElement.Add(new XAttribute(REF_ATTR, RDF_NIL));
        }
        else
        {
          if (propertyRole.dataType.Contains("dateTime"))
            propertyValue = Utility.ToXsdDateTime(propertyValue);

          propertyElement.Add(new XText(propertyValue));
        }
      }
      else  // resolve value list to uri
      {
        propertyValue = _mapping.ResolveValueList(propertyRole.valueListName, propertyValue);

        if (String.IsNullOrEmpty(propertyValue))
        {
          return null;
        }
        
        propertyValue = propertyValue.Replace(RDL_PREFIX, RDL_NS.NamespaceName);
        propertyElement.Add(new XAttribute(REF_ATTR, propertyValue));
      }

      return propertyElement;
    }
    #endregion
  }
}
