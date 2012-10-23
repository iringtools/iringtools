using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using org.iringtools.adapter;
using log4net;
using System.Linq.Expressions;
using System.IO;

namespace org.iringtools.library
{
  public abstract class BaseDataLayer : IDataLayer2
  {
    protected AdapterSettings _settings = null;
    protected DataObject _dataObjectDefinition = null;
    protected List<IDataObject> _dataObjects = null;
    protected XElement _configuration = null;

    private static readonly ILog _logger = LogManager.GetLogger(typeof(BaseDataLayer));
    
    public BaseDataLayer(AdapterSettings settings)
    {
      _settings = settings;
    }

    public virtual IList<IDataObject> Create(string objectType, IList<string> identifiers)
    {
      IDataObject dataObject = null;
      IList<IDataObject> dataObjects = new List<IDataObject>();

      try
      {
        LoadDataDictionary(objectType);

        if (identifiers == null || identifiers.Count == 0)
        {
          dataObject = new GenericDataObject
          {
            ObjectType = objectType,
          };

          SetKeys(dataObject, null);

          dataObjects.Add(dataObject);
        }
        else
        {
          dataObjects = Get(objectType, identifiers);

          foreach (string identifier in identifiers)
          {
            var predicate = FormKeyPredicate(identifier);

            if (predicate != null)
            {
              dataObject = dataObjects.AsQueryable().Where(predicate).FirstOrDefault();
            }

            if (dataObject == null)
            {
              dataObject = new GenericDataObject
              {
                ObjectType = objectType,
              };

              SetKeys(dataObject, identifier);

              dataObjects.Add(dataObject);
            }
          }
        }

        return dataObjects;
      }
      catch (Exception ex)
      {
        _logger.Error("Error in Create: " + ex);

        throw new Exception(
          "Error while creating a list of data objects of type [" + objectType + "].",
          ex
        );
      }
    }

    public virtual long GetCount(string objectType, DataFilter filter)
    {
      try
      {
        if (_dataObjects == null)
        {
          IList<IDataObject> dataObjects = Get(objectType, filter, 0, 0);
        }

        return _dataObjects.Count();
      }
      catch (Exception ex)
      {
        _logger.Error("Error in GetCount: " + ex);

        throw new Exception(
          "Error while getting a count of type [" + objectType + "].",
          ex
        );
      }
    }

    public virtual IList<IDataObject> GetRelatedObjects(IDataObject dataObject, string relatedObjectType)
    {
      return null;
    }

    public virtual long GetRelatedCount(IDataObject dataObject, string relatedObjectType)
    {
      return 0;
    }

    public virtual IList<IDataObject> GetRelatedObjects(IDataObject dataObject, string relatedObjectType, int pageSize, int startIndex)
    {
      return null;
    }

    #region Abstract Public Interface Methods

    public abstract IList<string> GetIdentifiers(string objectType, DataFilter filter);

    public abstract IList<IDataObject> Get(string objectType, IList<string> identifiers);

    public abstract IList<IDataObject> Get(string objectType, DataFilter filter, int pageSize, int startIndex);

    public abstract Response Post(IList<IDataObject> dataObjects);

    public abstract Response Delete(string objectType, IList<string> identifiers);

    public abstract Response Delete(string objectType, DataFilter filter);

    public abstract DataDictionary GetDictionary();

    #endregion

    public virtual Response Configure(XElement configuration)
    {
      throw new NotImplementedException();
    }

    public virtual XElement GetConfiguration()
    {
			_logger.Error("NotImplementedException");
      throw new NotImplementedException();
    }

    public virtual DocumentBytes GetResourceData()
    {
      _logger.Error("NotImplementedException");
      throw new NotImplementedException();
    }

    protected void LoadDataDictionary(string objectType)
    {
      DataDictionary dataDictionary = GetDictionary();

      _dataObjectDefinition =
        dataDictionary.dataObjects.Find(
            o => o.objectName.ToUpper() == objectType.ToUpper()
        );
    }

    protected DataFilter FormMultipleKeysFilter(IDataObject dataObject)
    {
      List<Expression> expressions = BuildKeyFilter(dataObject);
      
      DataFilter dataFilter = new DataFilter
      {
        Expressions = expressions,
      };

      return dataFilter;
    }

    protected DataFilter FormMultipleKeysFilter(IList<string> identifiers)
    {
      List<Expression> expressions = new List<Expression>();
      foreach (string identifier in identifiers)
      {
        List<Expression> identifierExpressions = BuildKeyFilter(identifier);

        if (identifierExpressions.Count > 1)
        {
          Expression expression = identifierExpressions.First();
          expression.OpenGroupCount++;

          expression = identifierExpressions.Last();
          expression.CloseGroupCount++;
          expression.LogicalOperator = LogicalOperator.Or;
        }
        else
        {
          Expression expression = identifierExpressions.Last();
          expression.LogicalOperator = LogicalOperator.Or;
        }

        expressions.AddRange(identifierExpressions);
      }

      DataFilter dataFilter = new DataFilter
      {
        Expressions = expressions,
      };

      return dataFilter;
    }

    protected Expression<Func<IDataObject, bool>> FormMultipleKeysPredicate(IList<string> identifiers)
    {
      Expression<Func<IDataObject, bool>> predicate = null;

      DataFilter dataFilter = FormMultipleKeysFilter(identifiers);
      
      predicate = dataFilter.ToPredicate(_dataObjectDefinition);

      return predicate; 
    }

    protected Expression<Func<IDataObject, bool>> FormKeyPredicate(string identifier)
    {
      Expression<Func<IDataObject, bool>> predicate = null;
      List<Expression> expressions = BuildKeyFilter(identifier);

      DataFilter dataFilter = new DataFilter
      {
        Expressions = expressions,
      };

      predicate = dataFilter.ToPredicate(_dataObjectDefinition);

      return predicate; 
    }

    protected List<Expression> BuildKeyFilter(IDataObject dataObject)
    {
      List<Expression> expressions = new List<Expression>();

      foreach (KeyProperty keyProperty in _dataObjectDefinition.keyProperties)
      {
        string identifier = dataObject.GetPropertyValue(keyProperty.keyPropertyName).ToString();

        Expression expression = new Expression
        {
          PropertyName = keyProperty.keyPropertyName,
          RelationalOperator = RelationalOperator.EqualTo,
          Values = new Values
          {
            identifier,
          }
        };

        if (expressions.Count > 0)
        {
          expression.LogicalOperator = LogicalOperator.And;
        }

        expressions.Add(expression);
      }

      return expressions;
    }

    protected List<Expression> BuildKeyFilter(string identifier)
    {
      List<Expression> expressions = new List<Expression>();

      string[] delimiter = new string[] { _dataObjectDefinition.keyDelimeter };
      string[] identifierParts = identifier.Split(delimiter, StringSplitOptions.None);

      if (identifierParts.Count() == _dataObjectDefinition.keyProperties.Count)
      {
          for (int i = 0; i < _dataObjectDefinition.keyProperties.Count; i++)
          {
              string identifierPart = identifierParts[i];

              Expression expression = new Expression
              {
                  PropertyName = _dataObjectDefinition.keyProperties[i].keyPropertyName,
                  RelationalOperator = RelationalOperator.EqualTo,
                  Values = new Values
                  {
                    identifierPart,
                  }
              };

              if (expressions.Count > 0)
              {
                  expression.LogicalOperator = LogicalOperator.And;
              }

              expressions.Add(expression);
          }
      }
      else
      {
          Expression expression = new Expression
          {
              PropertyName = _dataObjectDefinition.keyProperties[0].keyPropertyName,
              RelationalOperator = RelationalOperator.EqualTo,
              Values = new Values
                {
                    identifier,
                }
          };

          expressions.Add(expression);
      }

      return expressions;
    }

    protected List<Expression> CreateRelatedFilterExpressions(IDataObject parentDataObject, DataRelationship dataRelationship)
    {
      List<Expression> expressions = new List<Expression>();

      foreach (PropertyMap propertyMap in dataRelationship.propertyMaps)
      {
        Expression expression = new Expression()
        {
          PropertyName = propertyMap.relatedPropertyName,
          RelationalOperator = RelationalOperator.EqualTo,
          Values = new Values
          {
            parentDataObject.GetPropertyValue(propertyMap.dataPropertyName).ToString()
          }
        };

        if (expressions.Count > 0)
        {
          expression.LogicalOperator = LogicalOperator.And;
        }

        expressions.Add(expression);
      }

      return expressions;
    }

    protected DataFilter CreateDataFilter(IDataObject parentDataObject, string relatedObjectType)
    {
        string objectType = parentDataObject.GetType().Name;

        if (objectType == typeof(GenericDataObject).Name)
        {
            objectType = ((GenericDataObject)parentDataObject).ObjectType;
        }

      DataDictionary dataDictionary = GetDictionary();

      DataObject dataObject = dataDictionary.dataObjects.Find(c => c.objectName.ToLower() == objectType.ToLower());
      if (dataObject == null)
      {
          throw new Exception("Parent data object [" + objectType + "] not found.");
      }

      DataRelationship dataRelationship = dataObject.dataRelationships.Find(c => c.relatedObjectName.ToLower() == relatedObjectType.ToLower());
      if (dataRelationship == null)
      {
          throw new Exception("Relationship between data object [" + objectType +
          "] and related data object [" + relatedObjectType + "] not found.");
      }

      List<Expression> expressions = CreateRelatedFilterExpressions(parentDataObject, dataRelationship);

      DataFilter filter = new DataFilter()
      {
        Expressions = expressions
      };

      return filter;
    }

    protected void SetKeys(IDataObject dataObject, string identifier)
    {
      string[] delimiter = new string[] { _dataObjectDefinition.keyDelimeter ?? string.Empty };

      if (identifier == null)
      {
        foreach (KeyProperty keyProperty in _dataObjectDefinition.keyProperties)
        {
          dataObject.SetPropertyValue(keyProperty.keyPropertyName, null);
        }
      }
      else
      {
        string[] identifierParts = identifier.Split(delimiter, StringSplitOptions.None);

        if (identifierParts.Count() == _dataObjectDefinition.keyProperties.Count)
        {
          int i = 0;
          foreach (KeyProperty keyProperty in _dataObjectDefinition.keyProperties)
          {
            string identifierPart = identifierParts[i];
            if (!String.IsNullOrEmpty(identifierPart))
            {
              dataObject.SetPropertyValue(keyProperty.keyPropertyName, identifierPart);
            }
            i++;
          }
        }
      }
    }

    protected void SetKeyProperties(DataObject dataObjectDefinition, IDataObject dataObject, string identifier)
    {
      string[] delimiter = new string[] { dataObjectDefinition.keyDelimeter ?? string.Empty };

      if (identifier == null)
      {
        foreach (KeyProperty keyProperty in dataObjectDefinition.keyProperties)
        {
          dataObject.SetPropertyValue(keyProperty.keyPropertyName, null);
        }
      }
      else
      {
        string[] identifierParts = identifier.Split(delimiter, StringSplitOptions.None);

        if (identifierParts.Count() == dataObjectDefinition.keyProperties.Count)
        {
          int i = 0;
          foreach (KeyProperty keyProperty in dataObjectDefinition.keyProperties)
          {
            string identifierPart = identifierParts[i];
            if (!String.IsNullOrEmpty(identifierPart))
            {
              dataObject.SetPropertyValue(keyProperty.keyPropertyName, identifierPart);
            }
            i++;
          }
        }
      }
    }

    protected string GetIdentifier(IDataObject dataObject)
    {
      return GetIdentifier(_dataObjectDefinition, dataObject);
    }

    protected string GetIdentifier(DataObject dataObjectDefinition, IDataObject dataObject)
    {
      _dataObjectDefinition = dataObjectDefinition;

      string[] identifierParts = new string[_dataObjectDefinition.keyProperties.Count];

      int i = 0;
      foreach (KeyProperty keyProperty in _dataObjectDefinition.keyProperties)
      {
          object value = dataObject.GetPropertyValue(keyProperty.keyPropertyName);
          if (value != null)
          {
              identifierParts[i] = value.ToString();
          }
          else
          {
              identifierParts[i] = String.Empty;
          }

          i++;
      }

      return String.Join(_dataObjectDefinition.keyDelimeter, identifierParts);
    }

    public virtual IList<IDataObject> Search(string objectType, string query, int pageSize, int startIndex)
    {
      throw new NotImplementedException();
    }

    public virtual IList<IDataObject> Search(string objectType, string query, DataFilter filter, int pageSize, int startIndex)
    {
      throw new NotImplementedException();
    }

    public virtual long GetSearchCount(string objectType, string query)
    {
      throw new NotImplementedException();
    }

    public virtual long GetSearchCount(string objectType, string query, DataFilter filter)
    {
      throw new NotImplementedException();
    }

    public virtual Response RefreshAll()
    {
      return Refresh(string.Empty);
    }

    public virtual Response Refresh(string objectType)
    {
      Response response = new Response()
      {
        Level = StatusLevel.Warning,
        Messages = new Messages { "Method not implemented." }
      };

      return response;
    }

    public virtual IList<Object> GetSummary()
    {
      return null;
    }

    public virtual Picklists GetPicklist(string name, int start, int limit)
    {
      throw new NotImplementedException();
    }
  }
}
