using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using log4net;
using NHibernate;
using Ninject;
using org.iringtools.library;
using org.iringtools.nhibernate;
using org.iringtools.utility;
using Ninject.Extensions.Xml;

namespace org.iringtools.adapter.datalayer
{
  public class NHibernateDataLayer : BaseConfigurableDataLayer
  {
    private static readonly ILog _logger = LogManager.GetLogger(typeof(NHibernateDataLayer));
    private IKernel _kernel = null;
    private IAuthorization _authorization;
    protected const string UNAUTHORIZED_ERROR = "User not authorized to access NHibernate data layer of [{0}]";
    
    protected string _dataDictionaryPath = String.Empty;
    protected string _databaseDictionaryPath = String.Empty;
    
    protected string _authorizationBindingPath = String.Empty;
    protected string _summaryBindingPath = String.Empty;

    protected DataDictionary _dataDictionary;
    protected DatabaseDictionary _dbDictionary;
    protected IDictionary _keyRing = null;
    protected NHibernateSettings _nSettings = null;

    [Inject]
    public NHibernateDataLayer(AdapterSettings settings, IDictionary keyRing) : base(settings)
    {
      var ninjectSettings = new NinjectSettings { LoadExtensions = false, UseReflectionBasedInjection = true };
      _kernel = new StandardKernel(ninjectSettings, new NHibernateModule());
      _kernel.Load(new XmlExtensionModule());
      _nSettings = _kernel.Get<NHibernateSettings>();
      _nSettings.AppendSettings(settings);
      _keyRing = keyRing;

      _kernel.Rebind<AdapterSettings>().ToConstant(_settings);
      _kernel.Bind<IDictionary>().ToConstant(_keyRing).Named("KeyRing");

      _dataDictionaryPath = string.Format("{0}DataDictionary.{1}.xml",
        _settings["AppDataPath"],
        _settings["Scope"]
      );

      string dbDictionaryPath = string.Format("{0}DatabaseDictionary.{1}.xml",
        _settings["AppDataPath"],
        _settings["Scope"]
      );

      if (File.Exists(dbDictionaryPath))
      {
        _dbDictionary = NHibernateUtility.LoadDatabaseDictionary(dbDictionaryPath);
      }

      string relativePath = String.Format("{0}AuthorizationBindingConfiguration.{1}.xml",
        _settings["AppDataPath"],
        _settings["Scope"]
      );

      _authorizationBindingPath = Path.Combine(
        _settings["BaseDirectoryPath"],
        relativePath
      );
      
      relativePath = String.Format("{0}SummaryBindingConfiguration.{1}.xml",
        _settings["AppDataPath"],
        _settings["Scope"]
      );

      _summaryBindingPath = Path.Combine(
        _settings["BaseDirectoryPath"],
        relativePath
      );
    }

    #region public methods
    public override IList<IDataObject> Create(string objectType, IList<string> identifiers)
    {
      AccessLevel accessLevel = Authorize(objectType);

      if (accessLevel < AccessLevel.Read)
        throw new UnauthorizedAccessException(String.Format(UNAUTHORIZED_ERROR, _settings["scope"]));

      ISession session = NHibernateSessionManager.Instance.GetSession(_settings["AppDataPath"], _settings["Scope"]);

      try
      {
        IList<IDataObject> dataObjects = new List<IDataObject>();
        DataObject objectDefinition = _dataDictionary.dataObjects.First(c => c.objectName.ToUpper() == objectType.ToUpper());

        string ns = String.IsNullOrEmpty(objectDefinition.objectNamespace)
          ? String.Empty : (objectDefinition.objectNamespace + ".");

        Type type = Type.GetType(ns + objectType + ", " + _settings["ExecutingAssemblyName"]);
        IDataObject dataObject = null;

        if (identifiers != null)
        {
          foreach (string identifier in identifiers)
          {
            if (!String.IsNullOrEmpty(identifier))
            {
              IQuery query = session.CreateQuery("from " + objectType + " where Id = ?");
              query.SetString(0, identifier);
              dataObject = query.List<IDataObject>().FirstOrDefault<IDataObject>();

              if (dataObject == null)
              {
                dataObject = (IDataObject)Activator.CreateInstance(type);
                dataObject.SetPropertyValue("Id", identifier);
              }
            }
            else
            {
              dataObject = (IDataObject)Activator.CreateInstance(type);
            }

            dataObjects.Add(dataObject);
          }
        }
        else
        {
          dataObject = (IDataObject)Activator.CreateInstance(type);
          dataObjects.Add(dataObject);
        }

        return dataObjects;
      }
      catch (Exception ex)
      {
        _logger.Error("Error in CreateList: " + ex);
        throw new Exception(string.Format("Error while creating a list of data objects of type [{0}]. {1}", objectType, ex));
      }
      finally
      {
        CloseSession(session);
      }
    }

    public override long GetCount(string objectType, DataFilter filter)
    {
      AccessLevel accessLevel = Authorize(objectType, ref filter);

      if (accessLevel < AccessLevel.Read)
        throw new UnauthorizedAccessException(String.Format(UNAUTHORIZED_ERROR, _settings["scope"]));

      ISession session = NHibernateSessionManager.Instance.GetSession(_settings["AppDataPath"], _settings["Scope"]);

      try
      {
        if (_dbDictionary.IdentityConfiguration != null)
        {
          IdentityProperties identityProperties = _dbDictionary.IdentityConfiguration[objectType];

          if (identityProperties.UseIdentityFilter)
          {
            filter = FilterByIdentity(objectType, filter, identityProperties);
          }
        }

        StringBuilder queryString = new StringBuilder();
        queryString.Append("select count(*) from " + objectType);

        if (filter != null && filter.Expressions != null && filter.Expressions.Count > 0)
        {
          DataFilter clonedFilter = Utility.CloneDataContractObject<DataFilter>(filter);
          clonedFilter.OrderExpressions = null;
          DataObject dataObject = _dbDictionary.dataObjects.Find(x => x.objectName.ToUpper() == objectType.ToUpper());
          string whereClause = clonedFilter.ToSqlWhereClause(_dbDictionary, dataObject.tableName, String.Empty);
          queryString.Append(whereClause);
        }

        IQuery query = session.CreateQuery(queryString.ToString());
        long count = query.List<long>().First();
        return count;
      }
      catch (Exception ex)
      {
        _logger.Error("Error in GetIdentifiers: " + ex);
        throw new Exception(string.Format("Error while getting a list of identifiers of type [{0}]. {1}", objectType, ex));
      }
      finally
      {
        CloseSession(session);
      }
    }

    public override IList<string> GetIdentifiers(string objectType, DataFilter filter)
    {
      AccessLevel accessLevel = Authorize(objectType, ref filter);

      if (accessLevel < AccessLevel.Read)
        throw new UnauthorizedAccessException(String.Format(UNAUTHORIZED_ERROR, _settings["scope"]));

      ISession session = NHibernateSessionManager.Instance.GetSession(_settings["AppDataPath"], _settings["Scope"]);

      try
      {
        if (_dbDictionary.IdentityConfiguration != null)
        {
          IdentityProperties identityProperties = _dbDictionary.IdentityConfiguration[objectType];
          if (identityProperties.UseIdentityFilter)
          {
            filter = FilterByIdentity(objectType, filter, identityProperties);
          }
        }
        StringBuilder queryString = new StringBuilder();
        queryString.Append("select Id from " + objectType);

        if (filter != null && filter.Expressions.Count > 0)
        {
          DataObject dataObject = _dbDictionary.dataObjects.Find(x => x.objectName.ToUpper() == objectType.ToUpper());
          string whereClause = filter.ToSqlWhereClause(_dbDictionary, dataObject.tableName, String.Empty);
          queryString.Append(whereClause);
        }

        IQuery query = session.CreateQuery(queryString.ToString());
        return query.List<string>();
      }
      catch (Exception ex)
      {
        _logger.Error("Error in GetIdentifiers: " + ex);
        throw new Exception(string.Format("Error while getting a list of identifiers of type [{0}]. {1}", objectType, ex));
      }
      finally
      {
        CloseSession(session);
      }
    }

    public override IList<IDataObject> Get(string objectType, IList<string> identifiers)
    {
      AccessLevel accessLevel = Authorize(objectType);

      if (accessLevel < AccessLevel.Read)
        throw new UnauthorizedAccessException(String.Format(UNAUTHORIZED_ERROR, _settings["scope"]));

      ISession session = NHibernateSessionManager.Instance.GetSession(_settings["AppDataPath"], _settings["Scope"]);

      try
      {
        StringBuilder queryString = new StringBuilder();
        queryString.Append("from " + objectType);

        if (identifiers != null && identifiers.Count > 0)
        {
          DataObject dataObjectDef = (from DataObject o in _dbDictionary.dataObjects
                                   where o.objectName == objectType
                                   select o).FirstOrDefault();

          if (dataObjectDef == null)
            return null;

          if (dataObjectDef.keyProperties.Count == 1)
          {
            queryString.Append(" where Id in ('" + String.Join("','", identifiers.ToArray()) + "')");
          }
          else if (dataObjectDef.keyProperties.Count > 1)
          {
            string[] keyList = null;
            int identifierIndex = 1;
            foreach (string identifier in identifiers)
            {
              string[] idParts = identifier.Split(dataObjectDef.keyDelimeter.ToCharArray()[0]);

              keyList = new string[idParts.Count()];

              int partIndex = 0;
              foreach (string part in idParts)
              {
                if (identifierIndex == identifiers.Count())
                {
                  keyList[partIndex] += part;
                }
                else
                {
                  keyList[partIndex] += part + ", ";
                }

                partIndex++;
              }

              identifierIndex++;
            }

            int propertyIndex = 0;
            foreach (KeyProperty keyProperty in dataObjectDef.keyProperties)
            {
              string propertyValues = keyList[propertyIndex];

              if (propertyIndex == 0)
              {
                queryString.Append(" where " + keyProperty.keyPropertyName + " in ('" + propertyValues + "')");
              }
              else
              {
                queryString.Append(" and " + keyProperty.keyPropertyName + " in ('" + propertyValues + "')");
              }

              propertyIndex++;
            }
          }
        }

        IQuery query = session.CreateQuery(queryString.ToString());
        IList<IDataObject> dataObjects = query.List<IDataObject>();

        // order data objects as list of identifiers
        if (identifiers != null)
        {
          IList<IDataObject> orderedDataObjects = new List<IDataObject>();

          foreach (string identifier in identifiers)
          {
            if (identifier != null)
            {
              foreach (IDataObject dataObject in dataObjects)
              {
                if (dataObject.GetPropertyValue("Id").ToString().ToLower() == identifier.ToLower())
                {
                  orderedDataObjects.Add(dataObject);
                  //break;  // include dups also
                }
              }
            }
          }

          return orderedDataObjects;
        }

        return dataObjects;
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

    public override IList<IDataObject> Get(string objectType, DataFilter filter, int pageSize, int startIndex)
    {
      AccessLevel accessLevel = Authorize(objectType, ref filter);

      if (accessLevel < AccessLevel.Read)
        throw new UnauthorizedAccessException(String.Format(UNAUTHORIZED_ERROR, _settings["scope"]));

      ISession session = NHibernateSessionManager.Instance.GetSession(_settings["AppDataPath"], _settings["Scope"]);
      
      try
      {
        if (_dbDictionary.IdentityConfiguration != null)
        {
          IdentityProperties identityProperties = _dbDictionary.IdentityConfiguration[objectType];
          if (identityProperties.UseIdentityFilter)
          {
            filter = FilterByIdentity(objectType, filter, identityProperties);
          }
        }

        DataObject objectDefinition = _dbDictionary.dataObjects.Find(x => x.objectName.ToUpper() == objectType.ToUpper());

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

    private DataFilter FilterByIdentity(string objectType, DataFilter filter, IdentityProperties identityProperties)
    {
      DataObject dataObject = _dbDictionary.dataObjects.Find(d => d.objectName == objectType);
      DataProperty dataProperty = dataObject.dataProperties.Find(p => p.columnName == identityProperties.IdentityProperty);

      if (dataProperty != null)
      {
        if (filter == null)
        {
          filter = new DataFilter();
        }

        bool hasExistingExpression = false;

        if (filter.Expressions == null)
        {
          filter.Expressions = new List<Expression>();
        }
        else if (filter.Expressions.Count > 0)
        {
          Expression firstExpression = filter.Expressions.First();
          Expression lastExpression = filter.Expressions.Last();
          firstExpression.OpenGroupCount++;
          lastExpression.CloseGroupCount++;
          hasExistingExpression = true;
        }

        string identityValue = _keyRing[identityProperties.KeyRingProperty].ToString();

        Expression expression = new Expression
        {
          PropertyName = dataProperty.propertyName,
          RelationalOperator = RelationalOperator.EqualTo,
          Values = new Values
          {
            identityValue,
          },
          IsCaseSensitive = identityProperties.IsCaseSensitive
        };

        if (hasExistingExpression)
          expression.LogicalOperator = LogicalOperator.And;
        filter.Expressions.Add(expression);
      }

      return filter;
    }

    public override Response Post(IList<IDataObject> dataObjects)
    {
      Response response = new Response();
      ISession session = NHibernateSessionManager.Instance.GetSession(_settings["AppDataPath"], _settings["Scope"]);
      
      try
      {
        if (dataObjects != null && dataObjects.Count > 0)
        {
          string objectType = dataObjects[0].GetType().Name;

          AccessLevel accessLevel = Authorize(objectType);

          if (accessLevel < AccessLevel.Write)
            throw new UnauthorizedAccessException(String.Format(UNAUTHORIZED_ERROR, _settings["scope"]));

          foreach (IDataObject dataObject in dataObjects)
          {
            Status status = new Status();
            status.Messages = new Messages();

            if (dataObject != null)
            {
              string identifier = String.Empty;

              try
              {
                // NOTE: Id property is not available if it's not mapped and will cause exception
                identifier = dataObject.GetPropertyValue("Id").ToString();
              }
              catch (Exception ex)
              {
                _logger.Error(string.Format("Error in Post: {0}", ex));
              }  // no need to handle exception because identifier is only used for statusing

              status.Identifier = identifier;

              try
              {
                session.SaveOrUpdate(dataObject);
                session.Flush();
                status.Messages.Add(string.Format("Record [{0}] have been saved successfully.", identifier));
              }
              catch (Exception ex)
              {
                status.Level = StatusLevel.Error;
                status.Messages.Add(string.Format("Error while posting record [{0}]. {1}", identifier, ex));
                status.Results.Add("ResultTag", identifier);
                _logger.Error("Error posting data object to data layer: " + ex);
              }
            }
            else
            {
              status.Level = StatusLevel.Error;
              status.Identifier = String.Empty;
              status.Messages.Add("Data object is null or duplicate. See log for details.");
            }

            response.Append(status);
          }
        }

        return response;
      }
      catch (Exception ex)
      {
        _logger.Error("Error in Post: " + ex);

        object sample = dataObjects.FirstOrDefault();
        string objectType = (sample != null) ? sample.GetType().Name : String.Empty;
        throw new Exception(string.Format("Error while posting data objects of type [{0}]. {1}", objectType, ex));
      }
      finally
      {
        CloseSession(session);
      }
    }

    public override Response Delete(string objectType, IList<string> identifiers)
    {
      AccessLevel accessLevel = Authorize(objectType);

      if (accessLevel < AccessLevel.Delete)
        throw new UnauthorizedAccessException(String.Format(UNAUTHORIZED_ERROR, _settings["scope"]));

      Response response = new Response();
      ISession session = NHibernateSessionManager.Instance.GetSession(_settings["AppDataPath"], _settings["Scope"]);

      try
      {
        IList<IDataObject> dataObjects = Create(objectType, identifiers);

        foreach (IDataObject dataObject in dataObjects)
        {
          string identifier = dataObject.GetPropertyValue("Id").ToString();
          session.Delete(dataObject);

          Status status = new Status();
          status.Messages = new Messages();
          status.Identifier = identifier;
          status.Messages.Add(string.Format("Record [{0}] have been deleted successfully.", identifier));

          response.Append(status);
        }

        session.Flush();
      }
      catch (Exception ex)
      {
        _logger.Error("Error in Delete: " + ex);

        Status status = new Status();
        status.Level = StatusLevel.Error;
        status.Messages.Add(string.Format("Error while deleting data objects of type [{0}]. {1}", objectType, ex));
        response.Append(status);
      }
      finally
      {
        CloseSession(session);
      }

      return response;
    }

    public override Response Delete(string objectType, DataFilter filter)
    {
      AccessLevel accessLevel = Authorize(objectType, ref filter);

      if (accessLevel < AccessLevel.Delete)
        throw new UnauthorizedAccessException(String.Format(UNAUTHORIZED_ERROR, _settings["scope"]));

      Response response = new Response();
      response.StatusList = new List<Status>();
      Status status = new Status();
      ISession session = NHibernateSessionManager.Instance.GetSession(_settings["AppDataPath"], _settings["Scope"]);

      try
      {
        if (_dbDictionary.IdentityConfiguration != null)
        {
          IdentityProperties identityProperties = _dbDictionary.IdentityConfiguration[objectType];
          if (identityProperties.UseIdentityFilter)
          {
            filter = FilterByIdentity(objectType, filter, identityProperties);
          }
        }
        status.Identifier = objectType;

        StringBuilder queryString = new StringBuilder();
        queryString.Append("from " + objectType);

        if (filter.Expressions.Count > 0)
        {
          DataObject dataObject = _dbDictionary.dataObjects.Find(x => x.objectName.ToUpper() == objectType.ToUpper());
          string whereClause = filter.ToSqlWhereClause(_dbDictionary, dataObject.tableName, String.Empty);          
          queryString.Append(whereClause);
        }

        session.Delete(queryString.ToString());
        session.Flush();
        status.Messages.Add(string.Format("Records of type [{0}] has been deleted succesfully.", objectType));
      }
      catch (Exception ex)
      {
        _logger.Error("Error in Delete: " + ex);
        throw new Exception(string.Format("Error while deleting data objects of type [{0}]. {1}", objectType, ex));
        //no need to status, thrown exception will be statused above.
      }
      finally
      {
        CloseSession(session);
      }

      response.Append(status);
      return response;
    }

    public override DataDictionary GetDictionary()
    {
      if (File.Exists(_dataDictionaryPath))
      {
        _dataDictionary = Utility.Read<DataDictionary>(_dataDictionaryPath);
        return _dataDictionary;
      }
      else
      {
        return new DataDictionary();
      }
    }

    public override IList<IDataObject> GetRelatedObjects(IDataObject parentDataObject, string relatedObjectType)
    {
      IList<IDataObject> relatedObjects = null;
      ISession session = null;
       
      try
      {
        DataObject dataObject = _dataDictionary.dataObjects.Find(c => c.objectName.ToLower() == parentDataObject.GetType().Name.ToLower());
        if (dataObject == null)
        {
          throw new Exception("Parent data object [" + parentDataObject.GetType().Name + "] not found.");
        }

        DataRelationship dataRelationship = dataObject.dataRelationships.Find(c => c.relatedObjectName.ToLower() == relatedObjectType.ToLower());
        if (dataRelationship == null)
        {
          throw new Exception("Relationship between data object [" + parentDataObject.GetType().Name +
            "] and related data object [" + relatedObjectType + "] not found.");
        }

        session = NHibernateSessionManager.Instance.GetSession(_settings["AppDataPath"], _settings["Scope"]);
        
        StringBuilder sql = new StringBuilder();
        sql.Append("from " + dataRelationship.relatedObjectName + " where ");

        foreach (PropertyMap map in dataRelationship.propertyMaps)
        {
          DataProperty propertyMap = dataObject.dataProperties.First(c => c.propertyName == map.dataPropertyName);

          if (propertyMap.dataType == DataType.String)
          {
            sql.Append(map.relatedPropertyName + " = '" + parentDataObject.GetPropertyValue(map.dataPropertyName) + "' and ");
          }
          else
          {
            sql.Append(map.relatedPropertyName + " = " + parentDataObject.GetPropertyValue(map.dataPropertyName) + " and ");
          }
        }

        sql.Remove(sql.Length - 4, 4);  // remove the tail "and "
        IQuery query = session.CreateQuery(sql.ToString());
        relatedObjects = query.List<IDataObject>();

        if (relatedObjects != null && relatedObjects.Count > 0 && dataRelationship.relationshipType == RelationshipType.OneToOne)
        {
          return new List<IDataObject> { relatedObjects.First() };
        }

        return relatedObjects;
      }
      catch (Exception e)
      {
        string error = "Error getting related objects [" + relatedObjectType + "] " + e;
        _logger.Error(error);
        throw new Exception(error);
      }
      finally
      {
        CloseSession(session);
      }
    }

    public override long GetRelatedCount(IDataObject parentDataObject, string relatedObjectType)
    {
      try
      {
        DataFilter filter = CreateDataFilter(parentDataObject, relatedObjectType);
        return GetCount(relatedObjectType, filter);
      }
      catch (Exception ex)
      {
        string error = String.Format("Error getting related object count for object {0}: {1}", relatedObjectType, ex);
        _logger.Error(error);
        throw new Exception(error);
      }
    }

    public override IList<IDataObject> GetRelatedObjects(IDataObject parentDataObject, string relatedObjectType, int pageSize, int startIndex)
    {
      try
      {
        DataFilter filter = CreateDataFilter(parentDataObject, relatedObjectType);
        return Get(relatedObjectType, filter, pageSize, startIndex);
      }
      catch (Exception ex)
      {
        string error = String.Format("Error getting related objects for object {0}: {1}", relatedObjectType, ex);
        _logger.Error(error);
        throw new Exception(error);
      }
    }

    public override IList<Object> GetSummary()
    {
      try
      {
        AccessLevel accessLevel = Authorize("summary");

        if (accessLevel < AccessLevel.Read)
          throw new UnauthorizedAccessException(String.Format(UNAUTHORIZED_ERROR, _settings["scope"]));

        _kernel.Load(_summaryBindingPath);
        ISummary summary = _kernel.Get<ISummary>();
        return summary.GetSummary();
      }
      catch (Exception e)
      {
        _logger.Error("Error getting summary: " + e);
        throw e;
      }
    }

    public VersionInfo GetVersion()
    {
      Version version = this.GetType().Assembly.GetName().Version;
      return new VersionInfo()
      {
        Major = version.Major,
        Minor = version.Minor,
        Build = version.Build,
        Revision = version.Revision
      };
    }
    #endregion

    #region private methods
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

    private AccessLevel Authorize(string objectType)
    {
      DataFilter dataFilter = new DataFilter();
      return Authorize(objectType, ref dataFilter);
    }

    private AccessLevel Authorize(string objectType, ref DataFilter dataFilter)
    {
      try
      {
        if (_authorization == null)
        {
          _kernel.Load(_authorizationBindingPath);
          _authorization = _kernel.Get<IAuthorization>();
        }
        return _authorization.Authorize(objectType, ref dataFilter);
      }
      catch (Exception e)
      {
        _logger.Error("Error authorizing: " + e);
        throw e;
      }
    }
    #endregion
  }
}
