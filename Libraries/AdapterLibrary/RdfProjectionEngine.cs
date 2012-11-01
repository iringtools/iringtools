using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using org.iringtools.library;
using System.Xml.Linq;
using Ninject;
using log4net;
using System.Text.RegularExpressions;
using VDS.RDF;
using VDS.RDF.Storage;
using org.iringtools.utility;
using System.Xml;
using System.IO;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using System.Web;
using org.iringtools.mapping;

namespace org.iringtools.adapter.projection
{
  public class RdfProjectionEngine : BasePart7ProjectionEngine
  {
    private static readonly ILog _logger = LogManager.GetLogger(typeof(RdfProjectionEngine));
    protected static readonly string QUALIFIED_RDF_NIL = RDF_NS.NamespaceName + "nil";

    private Dictionary<string, List<string>> _individualsCache;
    private string _graphBaseUri;
    private XElement _rdfXml;

    [Inject]
    public RdfProjectionEngine(AdapterSettings settings, IDataLayer2 dataLayer, Mapping mapping) 
      : base(settings, dataLayer, mapping)
    {
      _individualsCache = new Dictionary<string, List<string>>();
    }

    public override XDocument ToXml(string graphName, ref IList<IDataObject> dataObjects)
    {
      XDocument rdfDoc = null;

      _rdfXml = new XElement(RDF_NS + "RDF",
        new XAttribute(XNamespace.Xmlns + "rdf", RDF_NS),
        new XAttribute(XNamespace.Xmlns + "owl", OWL_NS),
        new XAttribute(XNamespace.Xmlns + "xsd", XSD_NS),
        new XAttribute(XNamespace.Xmlns + "tpl", TPL_NS));

      try
      {
        _graphMap = _mapping.FindGraphMap(graphName);

        if (_graphMap != null && _graphMap.classTemplateMaps.Count > 0 &&
          dataObjects != null && dataObjects.Count > 0)
        {
          string baseUri = _settings["GraphBaseUri"];
          string project = _settings["ProjectName"];
          string app = _settings["ApplicationName"];
          string appBaseUri = Utility.FormEndpointBaseURI(_uriMaps, baseUri, project, app);
        
          _graphBaseUri = appBaseUri + _graphMap.name + "/";
          _dataObjects = dataObjects;
          rdfDoc = new XDocument(BuildRdfXml());
        }
        else
        {
          rdfDoc = new XDocument(_rdfXml);
        }
      }
      catch (Exception ex)
      {
        _logger.Error("Error in ToXml: " + ex);
        throw ex;
      }

      return rdfDoc;
    }

    public override XDocument ToXml(string graphName,  ref IList<IDataObject> dataObjects, string className, string classIdentifier)
    {
      XDocument rdfDoc = null;

      _rdfXml = new XElement(RDF_NS + "RDF",
        new XAttribute(XNamespace.Xmlns + "rdf", RDF_NS),
        new XAttribute(XNamespace.Xmlns + "owl", OWL_NS),
        new XAttribute(XNamespace.Xmlns + "xsd", XSD_NS),
        new XAttribute(XNamespace.Xmlns + "tpl", TPL_NS));

      try
      {
        _graphMap = _mapping.FindGraphMap(graphName);

        if (_graphMap != null && _graphMap.classTemplateMaps.Count > 0 && dataObjects != null)
        {
          string baseUri = _settings["GraphBaseUri"];
          string project = _settings["ProjectName"];
          string app = _settings["ApplicationName"];
          string appBaseUri = Utility.FormEndpointBaseURI(_uriMaps, baseUri, project, app);
        
          _graphBaseUri = appBaseUri + _graphMap.name + "/";
          _dataObjects = dataObjects;          
          rdfDoc = new XDocument(BuildRdfXml(className, classIdentifier));
        }
        else
        {
          rdfDoc = new XDocument(_rdfXml);
        }
      }
      catch (Exception ex)
      {
        _logger.Error("Error in ToXml: " + ex);
        throw ex;
      }

      return rdfDoc;
    }

    public override IList<IDataObject> ToDataObjects(string graphName, ref XDocument xDocument)
    {
      _dataObjects = null;

      try
      {
        if (xDocument != null)
        {
          _graphMap = _mapping.FindGraphMap(graphName);

          if (_graphMap != null && _graphMap.classTemplateMaps.Count > 0)
          {
            XmlDocument xmlDocument = new XmlDocument();
            using (XmlReader xmlReader = xDocument.CreateReader())
            {
              xmlDocument.Load(xmlReader);
            }
            xDocument.Root.RemoveAll();

            RdfXmlParser parser = new RdfXmlParser();
            Graph graph = new Graph();
            parser.Load(graph, xmlDocument);
            xmlDocument.RemoveAll();

            // load graph to memory store to allow querying locally
            _memoryStore = new TripleStore();
            _memoryStore.Add(graph);
            graph.Dispose();

            if (_memoryStore != null)
            {
              ClassMap rootClassMap = _graphMap.classTemplateMaps.First().classMap;
              string rootClassId = rootClassMap.id;
              List<string> rootClassInstances = GetClassInstances(rootClassId);

              if (rootClassInstances.Count > 0)
              {
                _dataObjects = new List<IDataObject>();
                _dataRecords = new Dictionary<string, string>[rootClassInstances.Count];
                _relatedRecordsMaps = new Dictionary<string, List<Dictionary<string, string>>>[rootClassInstances.Count];
                _relatedObjectPaths = new List<string>();

                for (int i = 0; i < rootClassInstances.Count; i++)
                {
                  List<string> rootClassInstance = new List<string> { rootClassInstances[i] };

                  _dataRecords[i] = new Dictionary<string, string>();
                  _relatedRecordsMaps[i] = new Dictionary<string, List<Dictionary<string, string>>>();

                  ProcessInboundClass(i, rootClassMap, rootClassInstance);

                  if (_primaryClassificationStyle == ClassificationStyle.Both)
                  {
                    TemplateMap classificationTemplate = _classificationConfig.TemplateMap;
                    ProcessInboundTemplates(i, rootClassInstance, new List<TemplateMap> { classificationTemplate });
                  }

                  try
                  {
                    IDataObject dataObject = CreateDataObject(_graphMap.dataObjectName, i);
                    _dataObjects.Add(dataObject);
                  }
                  catch (Exception e)
                  {
                    StringBuilder builder = new StringBuilder();
                    Dictionary<string, string> dataRecord = _dataRecords[i];

                    builder.AppendLine("Error creating data object. " + e);
                    builder.AppendLine("Data Record: ");

                    foreach (var pair in dataRecord)
                    {
                      builder.AppendLine("\t" + pair.Key + ": " + pair.Value);
                    }

                    _logger.Error(builder.ToString());
                  }
                }

                // fill related data objects and append them to top level data objects
                if (_relatedObjectPaths != null && _relatedObjectPaths.Count > 0)
                {
                  ProcessRelatedItems();
                  CreateRelatedObjects();
                }
              }
            }
          }
        }
      }
      catch (Exception ex)
      {
        _logger.Error("Error in ToDataObjects: " + ex);
        throw ex;
      }

      return _dataObjects;
    }

    #region outbound helper methods
    private XElement BuildRdfXml()
    {
      ClassTemplateMap classTemplateMap = _graphMap.classTemplateMaps.First();

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

            ProcessOutboundClass(dataObjectIndex, String.Empty, String.Empty, true, classIdentifiers, hasRelatedProperty,
              classTemplateMap.classMap, classTemplateMap.templateMaps);
          }
        }
      }

      return _rdfXml;
    }

    // build RDF that's rooted at className with classIdentifier
    private XElement BuildRdfXml(string startClassName, string startClassIdentifier)
    {
      ClassTemplateMap classTemplateMap = _graphMap.GetClassTemplateMapByName(startClassName);

      if (classTemplateMap != null)
      {
        ClassMap classMap = classTemplateMap.classMap;
        List<TemplateMap> templateMaps = classTemplateMap.templateMaps;

        if (classMap != null)
        {
          bool hasRelatedProperty;
          List<string> classIdentifiers = GetClassIdentifiers(classMap, 0, out hasRelatedProperty);

          ProcessOutboundClass(0, startClassName, startClassIdentifier, true, classIdentifiers, 
            hasRelatedProperty, classTemplateMap.classMap, classTemplateMap.templateMaps);
        }
      }

      return _rdfXml;
    }

    private void ProcessOutboundClass(int dataObjectIndex, string startClassName, string startClassIdentifier, bool isRootClass,
      List<string> classIdentifiers, bool hasRelatedProperty, ClassMap classMap, List<TemplateMap> templateMaps)
    {
      string className = Utility.TitleCase(classMap.name);
      string baseUri = _graphBaseUri + className + "/";
      string classId = classMap.id.Substring(classMap.id.IndexOf(":") + 1);

      for (int classIdentifierIndex = 0; classIdentifierIndex < classIdentifiers.Count; classIdentifierIndex++)
      {
        string classIdentifier = classIdentifiers[classIdentifierIndex];

        if (String.IsNullOrEmpty(startClassIdentifier) || className != startClassName || classIdentifier == startClassIdentifier)
        {
          XElement individualElement = CreateIndividualElement(baseUri, classId, classIdentifier);

          if (individualElement != null)
          {
            _rdfXml.Add(individualElement);

            // add primary classification template
            if (isRootClass && _primaryClassificationStyle == ClassificationStyle.Both)
            {
              TemplateMap classificationTemplate = _classificationConfig.TemplateMap;
              CreateTemplateElement(dataObjectIndex, startClassName, startClassIdentifier, baseUri, classIdentifier,
                classIdentifierIndex, classificationTemplate, hasRelatedProperty);
            }

            ProcessOutboundTemplates(startClassName, startClassIdentifier, dataObjectIndex, individualElement, templateMaps,
              baseUri, classIdentifier, classIdentifierIndex, hasRelatedProperty);
          }
        }
      }
    }

    private void ProcessOutboundTemplates(string startClassName, string startClassIdentifier, int dataObjectIndex, XElement individualElement,
      List<TemplateMap> templateMaps, string baseUri, string classIdentifier, int classIdentifierIndex, bool hasRelatedProperty)
    {
      if (templateMaps != null && templateMaps.Count > 0)
      {
        foreach (TemplateMap templateMap in templateMaps)
        {
          if ((_secondaryClassificationStyle == ClassificationStyle.Type ||
                _secondaryClassificationStyle == ClassificationStyle.Both) &&
              _classificationConfig.TemplateIds.Contains(templateMap.id))
          {
            foreach (RoleMap roleMap in templateMap.roleMaps)
            {
              if (roleMap.type == RoleType.Reference)
              {
                string value = GetReferenceRoleValue(roleMap);
                individualElement.Add(new XElement(RDF_TYPE, new XAttribute(RDF_RESOURCE, value)));
              }
            }

            continue;
          }

          CreateTemplateElement(dataObjectIndex, startClassName, startClassIdentifier, baseUri, classIdentifier,
            classIdentifierIndex, templateMap, hasRelatedProperty);
        }
      }
    }

    private XElement CreateIndividualElement(string baseUri, string classId, string classIdentifier)
    {
      XElement individualElement = null;

      if (!String.IsNullOrEmpty(classIdentifier))
      {
        string individual = baseUri + classIdentifier;
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
          individualElement = new XElement(OWL_THING, new XAttribute(RDF_ABOUT, individual));
          individualElement.Add(new XElement(RDF_TYPE, new XAttribute(RDF_RESOURCE, RDL_NS.NamespaceName + classId)));
        }
      }

      return individualElement;
    }

    private void CreateTemplateElement(int dataObjectIndex, string startClassName, string startClassIdentifier, string baseUri,
      string classIdentifier, int classIdentifierIndex, TemplateMap templateMap, bool classIdentifierHasRelatedProperty)
    {
      string classInstance = baseUri + classIdentifier;
      IDataObject dataObject = _dataObjects[dataObjectIndex];
      string templateId = templateMap.id.Replace(TPL_PREFIX, TPL_NS.NamespaceName);
      List<RoleMap> propertyRoles = new List<RoleMap>();
      XElement baseTemplateElement = new XElement(OWL_THING);
      StringBuilder baseValues = new StringBuilder(templateMap.id);
      List<RoleMap> classRoles = new List<RoleMap>();

      baseTemplateElement.Add(new XElement(RDF_TYPE, new XAttribute(RDF_RESOURCE, templateId)));

      foreach (RoleMap roleMap in templateMap.roleMaps)
      {
        string roleId = roleMap.id.Substring(roleMap.id.IndexOf(":") + 1);
        XElement roleElement = new XElement(TPL_NS + roleId);

        switch (roleMap.type)
        {
          case RoleType.Possessor:
            roleElement.Add(new XAttribute(RDF_RESOURCE, classInstance));
            baseTemplateElement.Add(roleElement);
            baseValues.Append(classIdentifier);
            break;

          case RoleType.FixedValue:
            string dataType = roleMap.dataType.Replace(XSD_PREFIX, XSD_NS.NamespaceName);
            roleElement.Add(new XAttribute(RDF_DATATYPE, dataType));
            roleElement.Add(new XText(roleMap.value));
            baseTemplateElement.Add(roleElement);
            baseValues.Append(roleMap.value);
            break;

          case RoleType.Reference:
            if (roleMap.classMap != null)
            {
              classRoles.Add(roleMap);
            }
            else
            {
              string value = GetReferenceRoleValue(roleMap);
              roleElement.Add(new XAttribute(RDF_RESOURCE, value));
              baseTemplateElement.Add(roleElement);
              baseValues.Append(roleMap.value);
            }
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
            // enforce dotNetRDF to store/retrieve templates in order as expressed in RDF
            string hashPrefixFormat = Regex.Replace(multiPropertyElements[0].Count.ToString(), "\\d", "0") + "0";

            for (int i = 0; i < multiPropertyElements[0].Count; i++)
            {
              XElement templateElement = new XElement(baseTemplateElement);
              _rdfXml.Add(templateElement);

              StringBuilder templateValue = new StringBuilder(baseValues.ToString());
              for (int j = 0; j < multiPropertyElements.Count; j++)
              {
                XElement propertyElement = multiPropertyElements[j][i];
                templateElement.Add(propertyElement);

                if (!String.IsNullOrEmpty(propertyElement.Value))
                  templateValue.Append(propertyElement.Value);
                else
                  templateValue.Append(propertyElement.Attribute(RDF_RESOURCE).Value);
              }

              string hashCode = Utility.MD5Hash(templateValue.ToString());
              hashCode = i.ToString(hashPrefixFormat) + hashCode.Substring(hashPrefixFormat.Length);
              templateElement.Add(new XAttribute(RDF_ABOUT, hashCode));
            }
          }
        }
      }
      else if (classRoles.Count > 0)  // relationship template with known class role
      {
        bool isTemplateValid = false;  // template is valid when there is at least one class referernce identifier that is not null
        Dictionary<RoleMap, List<string>> relatedClassRoles = new Dictionary<RoleMap, List<string>>();

        foreach (RoleMap classRole in classRoles)
        {
          bool refClassHasRelatedProperty;
          List<string> refClassIdentifiers = GetClassIdentifiers(classRole.classMap, dataObjectIndex, out refClassHasRelatedProperty);

          if (refClassHasRelatedProperty)
          {
            relatedClassRoles[classRole] = refClassIdentifiers;
          }
          else
          {
            string refClassIdentifier = refClassIdentifiers.First();

            if (!String.IsNullOrEmpty(refClassIdentifier))
            {
              isTemplateValid = true;
              baseValues.Append(refClassIdentifier);

              string roleId = classRole.id.Substring(classRole.id.IndexOf(":") + 1);
              XElement roleElement = new XElement(TPL_NS + roleId);
              roleElement.Add(new XAttribute(RDF_RESOURCE, _graphBaseUri +
                Utility.TitleCase(classRole.classMap.name) + "/" + refClassIdentifier));
              baseTemplateElement.Add(roleElement);              
            }

            ClassTemplateMap relatedClassTemplateMap = _graphMap.GetClassTemplateMap(classRole.classMap.id);

            if (relatedClassTemplateMap != null && relatedClassTemplateMap.classMap != null)
            {
              ProcessOutboundClass(dataObjectIndex, startClassName, startClassIdentifier, false, refClassIdentifiers, 
                refClassHasRelatedProperty, relatedClassTemplateMap.classMap, relatedClassTemplateMap.templateMaps);
            }
          }
        }

        if (relatedClassRoles.Count > 0)
        {
          string refClassBaseValues = baseValues.ToString();

          // enforce dotNetRDF to store/retrieve templates in order as expressed in RDF
          string hashPrefixFormat = Regex.Replace(relatedClassRoles.Count.ToString(), "\\d", "0") + "0";

          foreach (var pair in relatedClassRoles)
          {
            RoleMap classRole = pair.Key;
            List<string> refClassIdentifiers = pair.Value;

            string roleId = classRole.id.Substring(classRole.id.IndexOf(":") + 1);
            string baseRelatedClassUri = _graphBaseUri + Utility.TitleCase(classRole.classMap.name) + "/";

            for (int i = 0; i < refClassIdentifiers.Count; i++)
            {
              string refClassIdentifier = refClassIdentifiers[i];

              if (!String.IsNullOrEmpty(refClassIdentifier))
              {
                XElement refBaseTemplateElement = new XElement(baseTemplateElement);

                string hashCode = Utility.MD5Hash(refClassBaseValues + refClassIdentifier);
                hashCode = i.ToString(hashPrefixFormat) + hashCode.Substring(hashPrefixFormat.Length);
                refBaseTemplateElement.Add(new XAttribute(RDF_ABOUT, hashCode));

                XElement roleElement = new XElement(TPL_NS + roleId);
                roleElement.Add(new XAttribute(RDF_RESOURCE, baseRelatedClassUri + refClassIdentifier));
                refBaseTemplateElement.Add(roleElement);
                _rdfXml.Add(refBaseTemplateElement);
              }
            }

            ClassTemplateMap relatedClassTemplateMap = _graphMap.GetClassTemplateMap(classRole.classMap.id);

            if (relatedClassTemplateMap != null && relatedClassTemplateMap.classMap != null)
            {
              ProcessOutboundClass(dataObjectIndex, startClassName, startClassIdentifier, false, refClassIdentifiers,
                true, relatedClassTemplateMap.classMap, relatedClassTemplateMap.templateMaps);
            }
          }
        }
        else if (isTemplateValid)
        {
          string hashCode = Utility.MD5Hash(baseValues.ToString());
          baseTemplateElement.Add(new XAttribute(RDF_ABOUT, hashCode));
          _rdfXml.Add(baseTemplateElement);
        }
      }
      else  // relationship template with no class role (e.g. primary classification template)
      {
        string hashCode = Utility.MD5Hash(baseValues.ToString());
        baseTemplateElement.Add(new XAttribute(RDF_ABOUT, hashCode));
        _rdfXml.Add(baseTemplateElement);
      }
    }

    private XElement CreatePropertyElement(RoleMap propertyRole, string propertyValue)
    {
      XElement propertyElement = new XElement(TPL_NS + propertyRole.id.Replace(TPL_PREFIX, String.Empty));

      if (String.IsNullOrEmpty(propertyRole.valueListName))
      {
        if (String.IsNullOrEmpty(propertyValue))
        {
          propertyElement.Add(new XAttribute(RDF_RESOURCE, QUALIFIED_RDF_NIL));
        }
        else
        {
          propertyElement.Add(new XAttribute(RDF_DATATYPE,
            propertyRole.dataType.Replace(XSD_PREFIX, XSD_NS.NamespaceName)));

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
        propertyElement.Add(new XAttribute(RDF_RESOURCE, propertyValue));
      }

      return propertyElement;
    }

    private string GetReferenceRoleValue(RoleMap referenceRole)
    {
      string value = referenceRole.value;

      if (!String.IsNullOrEmpty(referenceRole.valueListName))
        value = _mapping.ResolveValueList(referenceRole.valueListName, value);

      return value.Replace(RDL_PREFIX, RDL_NS.NamespaceName);
    }
    #endregion

    #region inbound helper methods
    private List<string> GetClassInstances(string classId)
    {
      List<string> classInstances = new List<string>();
      string query = String.Format(CLASS_INSTANCE_QUERY_TEMPLATE, classId);
      object results = _memoryStore.ExecuteQuery(query);

      if (results != null)
      {
        SparqlResultSet resultSet = (SparqlResultSet)results;

        foreach (SparqlResult result in resultSet)
        {
          string classInstance = result.Value("class").ToString();

          if (!String.IsNullOrEmpty(classInstance))
          {
            classInstances.Add(classInstance);
          }
          else
          {
            _logger.Debug(query);
            throw new Exception("Individual of class [" + classId + "] not found!");
          }
        }
      }

      return classInstances;
    }

    private void ProcessInboundClass(int dataObjectIndex, ClassMap classMap, List<string> classInstances)
    {
      for (int classInstanceIndex = 0; classInstanceIndex < classInstances.Count; classInstanceIndex++)
      {
        string classInstance = classInstances[classInstanceIndex];
        string identifierValue = classInstance.Substring(classInstance.LastIndexOf("/") + 1);
        ProcessInboundClassIdentifiers(dataObjectIndex, classMap, classInstanceIndex, identifierValue);
      }

      ClassTemplateMap classTemplateMap = _graphMap.GetClassTemplateMap(classMap.id);

      if (classTemplateMap != null)
      {
        List<TemplateMap> templateMaps = classTemplateMap.templateMaps;

        if (templateMaps != null && templateMaps.Count > 0)
        {
          ProcessInboundTemplates(dataObjectIndex, classInstances, templateMaps);
        }
      }
    }

    private void ProcessInboundTemplates(int dataObjectIndex, List<string> classInstances, List<TemplateMap> templateMaps)
    {
      foreach (TemplateMap templateMap in templateMaps)
      {
        string possessorRoleId = String.Empty;
        RoleMap referenceRole = null;
        List<RoleMap> propertyRoles = new List<RoleMap>();
        List<RoleMap> classRoles = new List<RoleMap>();

        // find property roles
        foreach (RoleMap roleMap in templateMap.roleMaps)
        {
          switch (roleMap.type)
          {
            case RoleType.Possessor:
              possessorRoleId = roleMap.id;
              break;

            case RoleType.Reference:
              if (roleMap.classMap != null)
                classRoles.Add(roleMap);
              else
                referenceRole = roleMap;
              break;

            case RoleType.Property:
            case RoleType.DataProperty:
            case RoleType.ObjectProperty:
              propertyRoles.Add(roleMap);
              break;
          }
        }

        string referenceVariable = String.Empty;
        string referenceRoleId = String.Empty;
        string referenceRoleValue = String.Empty;
        string referenceEndStmt = String.Empty;

        if (referenceRole != null)
        {
          referenceVariable = BLANK_NODE;
          referenceRoleId = referenceRole.id;
          referenceRoleValue = referenceRole.value;
          referenceEndStmt = END_STATEMENT;
        }

        for (int classInstanceIndex = 0; classInstanceIndex < classInstances.Count; classInstanceIndex++)
        {
          if (classRoles.Count > 0)
          {
            foreach (RoleMap classRole in classRoles)
            {
              string query = String.Format(SUBCLASS_INSTANCE_QUERY_TEMPLATE, possessorRoleId,
                classInstances[classInstanceIndex], templateMap.id, referenceVariable, referenceRoleId,
                referenceRoleValue, referenceEndStmt, classRole.id);

              object results = _memoryStore.ExecuteQuery(query);

              if (results is SparqlResultSet)
              {
                SparqlResultSet resultSet = (SparqlResultSet)results;
                List<string> subclassInstances = new List<string>();

                foreach (SparqlResult result in resultSet)
                {
                  string subclassInstance = result.Value("class").ToString();
                  subclassInstances.Add(subclassInstance);
                }

                ProcessInboundClass(dataObjectIndex, classRole.classMap, subclassInstances);
              }
            }
          }
          else if (propertyRoles.Count > 0)  // query for property values
          {
            foreach (RoleMap propertyRole in propertyRoles)
            {
              List<string> values = new List<string>();
              string[] propertyPath = propertyRole.propertyName.Split('.');
              string property = propertyPath[propertyPath.Length - 1];

              string query = String.Format(LITERAL_QUERY_TEMPLATE, possessorRoleId, classInstances[classInstanceIndex],
                templateMap.id, referenceVariable, referenceRoleId, referenceRoleValue, referenceEndStmt,
                propertyRole.id);

              object results = _memoryStore.ExecuteQuery(query);

              if (results is SparqlResultSet)
              {
                SparqlResultSet resultSet = (SparqlResultSet)results;

                foreach (SparqlResult result in resultSet)
                {
                  string value = Regex.Replace(result.ToString(), @".*= ", String.Empty);

                  if (value == QUALIFIED_RDF_NIL)
                  {
                    value = String.Empty;
                  }
                  else if (value.Contains("^^"))
                  {
                    value = value.Substring(0, value.IndexOf("^^"));
                  }
                  else if (!String.IsNullOrEmpty(propertyRole.valueListName))
                  {
                    ValueListMap valueListMap = _mapping.valueListMaps.Find(x => x.name.ToLower() == propertyRole.valueListName.ToLower());

                    if (valueListMap != null && valueListMap.valueMaps != null)
                    {
                      ValueMap valueMap = valueListMap.valueMaps.Find(x => x.uri == value);

                      if (valueMap != null)
                      {
                        value = valueMap.internalValue;
                      }
                      else
                      {
                        value = valueListMap.valueMaps[0].internalValue;
                      }
                    }
                  }

                  if (propertyPath.Length > 2)  // related property
                  {
                    values.Add(value);
                  }
                  else  // direct property
                  {
                    _dataRecords[dataObjectIndex][property] = value;
                  }
                }

                if (propertyPath.Length > 2 && values.Count > 0)
                {
                  SetRelatedRecords(dataObjectIndex, classInstanceIndex, propertyRole.propertyName, values);
                }
              }
            }
          }
        }
      }
    }
    #endregion
  }
}