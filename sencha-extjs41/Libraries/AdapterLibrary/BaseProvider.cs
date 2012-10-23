using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using org.iringtools.library;
using org.iringtools.legacy;
using org.iringtools.mapping;
using org.iringtools.utility;
using log4net;
using org.ids_adi.qmxf;

namespace org.iringtools.adapter
{
  //TODO: move common methods from derived provider classes here
  public class BaseProvider
  {
    private static readonly ILog _logger = LogManager.GetLogger(typeof(BaseProvider));

    protected Dictionary<string, KeyValuePair<string, Dictionary<string, string>>> _qmxfTemplateResultCache = null;
    protected WebHttpClient _webHttpClient = null;  // for old mapping conversion
        
    #region Convert mapping
    public mapping.Mapping LoadMapping(string path, ref Status status)
    {
      XElement mappingXml = Utility.ReadXml(path);

      return LoadMapping(path, mappingXml, ref status);
    }

    public mapping.Mapping LoadMapping(string path, XElement mappingXml, ref Status status)
    {
      mapping.Mapping mapping = null;

      if (mappingXml.Name.NamespaceName.Contains("schemas.datacontract.org"))
      {
        status.Messages.Add("Detected legacy mapping. Attempting to convert it...");

        try
        {
          org.iringtools.legacy.Mapping legacyMapping = null;

          legacyMapping = Utility.DeserializeDataContract<legacy.Mapping>(mappingXml.ToString());

          mapping = ConvertMapping(legacyMapping);
        }
        catch (Exception legacyEx)
        {
          status.Messages.Add("Error loading legacy mapping file: " + legacyEx + ". Attempting to convert...");

          try
          {
            mapping = ConvertMapping(mappingXml);
          }
          catch (Exception oldEx)
          {
            status.Messages.Add("Legacy mapping could not be converted." + oldEx);
          }
        }

        if (mapping != null)
        {
          // write new mapping to disk
          Utility.Write<mapping.Mapping>(mapping, path, true);
          status.Messages.Add("Legacy mapping has been converted sucessfully.");
        }
      }
      else
      {
        mapping = Utility.DeserializeDataContract<mapping.Mapping>(mappingXml.ToString());
      }
      
      return mapping;
    }

    private mapping.Mapping ConvertMapping(XElement mappingXml)
    {
      mapping.Mapping mapping = new mapping.Mapping();
      _qmxfTemplateResultCache = new Dictionary<string, KeyValuePair<string, Dictionary<string, string>>>();

      #region convert graphMaps
      IEnumerable<XElement> graphMaps = mappingXml.Element("GraphMaps").Elements("GraphMap");
      foreach (XElement graphMap in graphMaps)
      {
        string dataObjectName = graphMap.Element("DataObjectMaps").Element("DataObjectMap").Attribute("name").Value;
        mapping.RoleMap roleMap = null;

        mapping.GraphMap newGraphMap = new mapping.GraphMap();
        newGraphMap.name = graphMap.Attribute("Name").Value;
        newGraphMap.dataObjectName = dataObjectName;
        mapping.graphMaps.Add(newGraphMap);

        ConvertClassMap(ref newGraphMap, ref roleMap, graphMap, dataObjectName);
      }
      #endregion

      #region convert valueMaps
      IEnumerable<XElement> valueMaps = mappingXml.Element("ValueMaps").Elements("ValueMap");
      string previousValueList = String.Empty;
      ValueListMap newValueList = null;

      foreach (XElement valueMap in valueMaps)
      {
        string valueList = valueMap.Attribute("valueList").Value;
        mapping.ValueMap newValueMap = new mapping.ValueMap
        {
          internalValue = valueMap.Attribute("internalValue").Value,
          uri = valueMap.Attribute("modelURI").Value
        };

        if (valueList != previousValueList)
        {
          newValueList = new ValueListMap
          {
            name = valueList,
            valueMaps = { newValueMap }
          };
          mapping.valueListMaps.Add(newValueList);

          previousValueList = valueList;
        }
        else
        {
          newValueList.valueMaps.Add(newValueMap);
        }
      }
      #endregion

      return mapping;
    }

    private void ConvertClassMap(ref mapping.GraphMap newGraphMap, ref mapping.RoleMap parentRoleMap, XElement classMap, string dataObjectName)
    {
      string classId = classMap.Attribute("classId").Value;

      mapping.ClassMap newClassMap = new mapping.ClassMap();
      newClassMap.id = classId;
      newClassMap.identifiers.Add(dataObjectName + "." + classMap.Attribute("identifier").Value);

      if (parentRoleMap == null)
      {
        newClassMap.name = GetClassName(classId);
      }
      else
      {
        newClassMap.name = classMap.Attribute("name").Value;
        parentRoleMap.classMap = newClassMap;
      }

      ClassTemplateMap newTemplateMaps = new ClassTemplateMap();
      newGraphMap.classTemplateMaps.Add(newTemplateMaps);

      IEnumerable<XElement> templateMaps = classMap.Element("TemplateMaps").Elements("TemplateMap");
      KeyValuePair<string, Dictionary<string, string>> templateNameRolesPair;

      foreach (XElement templateMap in templateMaps)
      {
        string classRoleId = String.Empty;

        try
        {
          classRoleId = templateMap.Attribute("classRole").Value;
        }
        catch (Exception)
        {
          continue; // class role not found, skip this template
        }

        IEnumerable<XElement> roleMaps = templateMap.Element("RoleMaps").Elements("RoleMap");
        string templateId = templateMap.Attribute("templateId").Value;

        mapping.TemplateMap newTemplateMap = new mapping.TemplateMap();
        newTemplateMap.id = templateId;
        newTemplateMaps.templateMaps.Add(newTemplateMap);

        if (_qmxfTemplateResultCache.ContainsKey(templateId))
        {
          templateNameRolesPair = _qmxfTemplateResultCache[templateId];
        }
        else
        {
          templateNameRolesPair = GetQmxfTemplateRolesPair(templateId);
          _qmxfTemplateResultCache[templateId] = templateNameRolesPair;
        }

        newTemplateMap.name = templateNameRolesPair.Key;

        mapping.RoleMap newClassRoleMap = new mapping.RoleMap();
        newClassRoleMap.type = mapping.RoleType.Possessor;
        newTemplateMap.roleMaps.Add(newClassRoleMap);
        newClassRoleMap.id = classRoleId;

        Dictionary<string, string> roles = templateNameRolesPair.Value;
        newClassRoleMap.name = roles[classRoleId];

        for (int i = 0; i < roleMaps.Count(); i++)
        {
          XElement roleMap = roleMaps.ElementAt(i);

          string value = String.Empty;
          try { value = roleMap.Attribute("value").Value; }
          catch (Exception ex)
          {
            _logger.Error("Error in ConvertClassMap: " + ex);
          }

          string reference = String.Empty;
          try { reference = roleMap.Attribute("reference").Value; }
          catch (Exception ex) { _logger.Error("Error in GetSection: " + ex); }

          string propertyName = String.Empty;
          try { propertyName = roleMap.Attribute("propertyName").Value; }
          catch (Exception ex) { _logger.Error("Error in ConvertClassMap: " + ex); }

          string valueList = String.Empty;
          try { valueList = roleMap.Attribute("valueList").Value; }
          catch (Exception ex) { _logger.Error("Error in ConvertClassMap: " + ex); }

          mapping.RoleMap newRoleMap = new mapping.RoleMap();
          newTemplateMap.roleMaps.Add(newRoleMap);
          newRoleMap.id = roleMap.Attribute("roleId").Value;
          newRoleMap.name = roles[newRoleMap.id];

          if (!String.IsNullOrEmpty(value))
          {
            newRoleMap.type = mapping.RoleType.FixedValue;
            newRoleMap.value = value;
          }
          else if (!String.IsNullOrEmpty(reference))
          {
            newRoleMap.type = mapping.RoleType.Reference;
            newRoleMap.value = reference;
          }
          else if (!String.IsNullOrEmpty(propertyName))
          {
            newRoleMap.propertyName = dataObjectName + "." + propertyName;

            if (!String.IsNullOrEmpty(valueList))
            {
              newRoleMap.type = mapping.RoleType.ObjectProperty;
              newRoleMap.valueListName = valueList;
            }
            else
            {
              newRoleMap.type = mapping.RoleType.DataProperty;
              newRoleMap.dataType = roleMap.Attribute("dataType").Value;
            }
          }

          if (roleMap.HasElements)
          {
            newRoleMap.type = mapping.RoleType.Reference;
            newRoleMap.value = roleMap.Attribute("dataType").Value;

            ConvertClassMap(ref newGraphMap, ref newRoleMap, roleMap.Element("ClassMap"), dataObjectName);
          }
        }
      }
    }

    private string GetClassName(string classId)
    {
      QMXF qmxf = _webHttpClient.Get<QMXF>("/classes/" + classId.Substring(classId.IndexOf(":") + 1), false);
      return qmxf.classDefinitions.First().name.First().value;
    }

    private KeyValuePair<string, Dictionary<string, string>> GetQmxfTemplateRolesPair(string templateId)
    {
      string templateName = String.Empty;
      Dictionary<string, string> roleIdNames = new Dictionary<string, string>();

      QMXF qmxf = _webHttpClient.Get<QMXF>("/templates/" + templateId.Substring(templateId.IndexOf(":") + 1), false);

      if (qmxf.templateDefinitions.Count > 0)
      {
        TemplateDefinition tplDef = qmxf.templateDefinitions.First();
        templateName = tplDef.name.First().value;

        foreach (RoleDefinition roleDef in tplDef.roleDefinition)
        {
          roleIdNames.Add(roleDef.identifier.Replace("http://tpl.rdlfacade.org/data#", "tpl:"), roleDef.name.First().value);
        }
      }
      else if (qmxf.templateQualifications.Count > 0)
      {
        TemplateQualification tplQual = qmxf.templateQualifications.First();
        templateName = tplQual.name.First().value;

        foreach (RoleQualification roleQual in tplQual.roleQualification)
        {
          roleIdNames.Add(roleQual.qualifies.Replace("http://tpl.rdlfacade.org/data#", "tpl:"), roleQual.name.First().value);
        }
      }

      return new KeyValuePair<string, Dictionary<string, string>>(templateName, roleIdNames);
    }

    private mapping.Mapping ConvertMapping(legacy.Mapping legacyMapping)
    {
      mapping.Mapping mapping = new mapping.Mapping();
      _qmxfTemplateResultCache = new Dictionary<string, KeyValuePair<string, Dictionary<string, string>>>();

      #region convert graphMaps
      IList<legacy.GraphMap> graphMaps = legacyMapping.graphMaps;
      foreach (legacy.GraphMap graphMap in graphMaps)
      {
        string dataObjectName = graphMap.dataObjectMap;
        mapping.RoleMap roleMap = null;

        mapping.GraphMap newGraphMap = new mapping.GraphMap();
        newGraphMap.name = graphMap.name;
        newGraphMap.dataObjectName = dataObjectName;
        mapping.graphMaps.Add(newGraphMap);

        ConvertGraphMap(ref newGraphMap, ref roleMap, graphMap, dataObjectName);
      }
      #endregion

      #region convert valueMaps
      IList<legacy.ValueList> valueLists = legacyMapping.valueLists;

      foreach (legacy.ValueList valueList in valueLists)
      {
        string valueListName = valueList.name;

        ValueListMap newValueList = new ValueListMap
        {
          name = valueList.name,
          valueMaps = new ValueMaps()
        };
        mapping.valueListMaps.Add(newValueList);

        foreach (legacy.ValueMap valueMap in valueList.valueMaps)
        {
          mapping.ValueMap newValueMap = new mapping.ValueMap
          {
            internalValue = valueMap.internalValue,
            uri = valueMap.uri
          };

          newValueList.valueMaps.Add(newValueMap);
        }
      }
      #endregion

      return mapping;
    }

    private void ConvertGraphMap(ref mapping.GraphMap newGraphMap, ref mapping.RoleMap parentRoleMap, legacy.GraphMap graphMap, string dataObjectName)
    {
      foreach (var classTemplateListMap in graphMap.classTemplateListMaps)
      {
        ClassTemplateMap classTemplateMap = new ClassTemplateMap();

        legacy.ClassMap legacyClassMap = classTemplateListMap.Key;

        Identifiers identifiers = new Identifiers();

        foreach (string identifier in legacyClassMap.identifiers)
        {
          identifiers.Add(identifier);
        }

        mapping.ClassMap newClassMap = new mapping.ClassMap
        {
          id = legacyClassMap.classId,
          identifierDelimiter = legacyClassMap.identifierDelimiter,
          identifiers = identifiers,
          identifierValue = legacyClassMap.identifierValue,
          name = legacyClassMap.name
        };

        classTemplateMap.classMap = newClassMap;

        TemplateMaps templateMaps = new TemplateMaps();
        foreach (legacy.TemplateMap templateMap in classTemplateListMap.Value)
        {
          mapping.TemplateType templateType = mapping.TemplateType.Definition;
          Enum.TryParse<mapping.TemplateType>(templateMap.templateType.ToString(), out templateType);

          mapping.TemplateMap newTemplateMap = new mapping.TemplateMap
          {
            id = templateMap.templateId,
            name = templateMap.name,
            type = templateType,
            roleMaps = new RoleMaps(),
          };

          foreach (legacy.RoleMap roleMap in templateMap.roleMaps)
          {
            mapping.RoleType roleType = mapping.RoleType.DataProperty;
            Enum.TryParse<mapping.RoleType>(roleMap.type.ToString(), out roleType);

            newClassMap = null;
            if (roleMap.classMap != null)
            {
              identifiers = new Identifiers();

              foreach (string identifier in roleMap.classMap.identifiers)
              {
                identifiers.Add(identifier);
              }

              newClassMap = new mapping.ClassMap
              {
                id = roleMap.classMap.classId,
                identifierDelimiter = roleMap.classMap.identifierDelimiter,
                identifiers = identifiers,
                identifierValue = roleMap.classMap.identifierValue,
                name = roleMap.classMap.name
              };
            }

            mapping.RoleMap newRoleMap = new mapping.RoleMap
            {
              id = roleMap.roleId,
              name = roleMap.name,
              type = roleType,
              classMap = newClassMap,
              dataType = roleMap.dataType,
              propertyName = roleMap.propertyName,
              value = roleMap.value,
              valueListName = roleMap.valueList
            };

            newTemplateMap.roleMaps.Add(newRoleMap);
          }

          templateMaps.Add(newTemplateMap);
        }

        classTemplateMap.templateMaps = templateMaps;

        if (classTemplateMap.classMap != null)
        {
          newGraphMap.classTemplateMaps.Add(classTemplateMap);
        }
      }
    }
    #endregion Convert mapping
  }
}
