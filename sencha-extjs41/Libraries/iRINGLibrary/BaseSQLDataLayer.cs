using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using org.iringtools.library;
using System.Data;
using log4net;
using System.Xml.Linq;
using org.iringtools.adapter;
using org.iringtools.utility;

namespace org.iringtools.library
{
  public abstract class BaseSQLDataLayer : BaseDataLayer
  {
    private static readonly ILog _logger = LogManager.GetLogger(typeof(BaseSQLDataLayer));
    protected DatabaseDictionary _dbDictionary = null;
    protected DataFilter _dataFilter = null;
    protected string _whereClauseAlias = String.Empty;
     
    #region BaseSQLDataLayer methods
    public BaseSQLDataLayer(AdapterSettings settings) : base(settings)
    {
      if (!String.IsNullOrEmpty(settings["WhereClauseAlias"]))
      {
        _whereClauseAlias = settings["WhereClauseAlias"];
      }
    }

    // get number of rows with (optional) filter
    public abstract DatabaseDictionary GetDatabaseDictionary();

    public abstract long GetCount(string tableName, string whereClause);

    // get list of identifiers with (optional) filter
    public abstract IList<string> GetIdentifiers(string tableName, string whereClause);

    // create or fetch data rows of given identifiers
    public abstract DataTable CreateDataTable(string tableName, IList<string> identifiers);

    // fetch data rows of given identifiers
    public abstract DataTable GetDataTable(string tableName, IList<string> identifiers);

    // get a page of data rows with (optional) filter
    public abstract DataTable GetDataTable(string tableName, string whereClause, long start, long limit);

    // get related data rows of a given data row
    public abstract DataTable GetRelatedDataTable(DataRow dataRow, string relatedTableName);

    // get related data rows of a given data row
    public abstract long GetRelatedCount(DataRow dataRow, string relatedTableName);

    // get related data rows of a given data row
    public abstract DataTable GetRelatedDataTable(DataRow dataRow, string relatedTableName, long start, long limit);

    // post data rows and its related items (data rows)
    public abstract Response PostDataTables(IList<DataTable> dataTables);
    
    // delete data rows with filter
    public abstract Response DeleteDataTable(string tableName, string whereClause);

    // delete data rows by identifiers
    public abstract Response DeleteDataTable(string tableName, IList<string> identifiers);

    // refresh dictionary for a specific data table
    public abstract Response RefreshDataTable(string tableName);
    #endregion

    #region IDataLayer implementation methods
    public override DataDictionary GetDictionary()
    {
      InitializeDatabaseDictionary();

      DataDictionary dictionary = new DataDictionary();
      dictionary.dataObjects = utility.Utility.CloneDataContractObject<List<DataObject>>(_dbDictionary.dataObjects);
      dictionary.picklists = utility.Utility.CloneDataContractObject<List<PicklistObject>>(_dbDictionary.picklists);

      return dictionary;
    }

    public override long GetCount(string objectType, DataFilter filter)
    {
      _dataFilter = filter;

      try
      {
        InitializeDatabaseDictionary();

        string tableName = GetTableName(objectType);
        string whereClause = string.Empty;
        
        if (filter != null)
          whereClause = filter.ToSqlWhereClause(_dbDictionary, tableName, _whereClauseAlias);

        return GetCount(tableName, whereClause);
      }
      catch (Exception ex)
      {
        _logger.Error("Error getting data count: " + ex);
        throw ex;
      }
    }

    public override IList<string> GetIdentifiers(string objectType, DataFilter filter)
    {
      _dataFilter = filter;
      
      try
      {
        InitializeDatabaseDictionary();
        
        string tableName = GetTableName(objectType);
        string whereClause = string.Empty;

        if (filter != null)
          whereClause = filter.ToSqlWhereClause(_dbDictionary, tableName, _whereClauseAlias);

        return GetIdentifiers(tableName, whereClause);
      }
      catch (Exception ex)
      {
        _logger.Error("Error getting data table: " + ex);
        throw ex;
      }
    }

    public override IList<IDataObject> Create(string objectType, IList<string> identifiers)
    {
      string tableName = GetTableName(objectType);

      try
      {
        DataTable dataTable = CreateDataTable(tableName, identifiers);
        return ToDataObjects(dataTable, objectType);
      }
      catch (Exception ex)
      {
        _logger.Error("Error creating data table: " + ex);
        throw ex;
      }
    }

    public override IList<IDataObject> Get(string objectType, IList<string> identifiers)
    {
      string tableName = GetTableName(objectType);

      try
      {
        DataTable dataTable = GetDataTable(tableName, identifiers);
        return ToDataObjects(dataTable, objectType);
      }
      catch (Exception ex)
      {
        _logger.Error("Error getting data table: " + ex);
        throw ex;
      }
    }

    public override IList<IDataObject> Get(string objectType, DataFilter filter, int limit, int start)
    {
      _dataFilter = filter;

      try
      {
        InitializeDatabaseDictionary();

        string tableName = GetTableName(objectType);
        string whereClause = string.Empty;

        if (filter != null)
          whereClause = filter.ToSqlWhereClause(_dbDictionary, tableName, _whereClauseAlias);

        DataTable dataTable = GetDataTable(tableName, whereClause, start, limit);
        IList<IDataObject> dataObjects = ToDataObjects(dataTable, objectType);
        return dataObjects;
      }
      catch (Exception ex)
      {
        _logger.Error("Error get data table: " + ex);
        throw ex;
      }
    }

    public override IList<IDataObject> GetRelatedObjects(IDataObject dataObject, string relatedObjectType)
    {
      IList<IDataObject> relatedDataObjects = null;
      string objectType = dataObject.GetType().Name;

      if (objectType == typeof(GenericDataObject).Name)
      {
        objectType = ((GenericDataObject)dataObject).ObjectType;
      }

      try
      {
        DataObject objectDefinition = GetObjectDefinition(objectType);
        DataObject relatedObjectDefinition = GetObjectDefinition(relatedObjectType);

        DataTable dataTable = NewDataTable(objectDefinition);
        DataRow dataRow = CreateDataRow(dataTable, dataObject, objectDefinition);
        
        if (dataRow != null)
        {
          DataTable relatedDataTable = GetRelatedDataTable(dataRow, relatedObjectDefinition.tableName);
          relatedDataObjects = ToDataObjects(relatedDataTable, relatedObjectType);
        }
        else
        {
          throw new Exception("Error creating/getting data row for object [" + objectDefinition.objectName + "]");
        }
      }
      catch (Exception ex)
      {
        _logger.Error("Error getting related objects: " + ex);
        throw ex;
      }

      return relatedDataObjects;
    }

    public override long GetRelatedCount(IDataObject dataObject, string relatedObjectType)
    {
      string objectType = dataObject.GetType().Name;

      if (objectType == typeof(GenericDataObject).Name)
      {
        objectType = ((GenericDataObject)dataObject).ObjectType;
      }

      try
      {
        DataObject objectDefinition = GetObjectDefinition(objectType);
        DataObject relatedObjectDefinition = GetObjectDefinition(relatedObjectType);

        DataTable dataTable = NewDataTable(objectDefinition);
        DataRow dataRow = CreateDataRow(dataTable, dataObject, objectDefinition);

        if (dataRow != null)
        {
          return GetRelatedCount(dataRow, relatedObjectDefinition.tableName);
        }
        else
        {
          throw new Exception("Error creating/getting data row for object [" + objectDefinition.objectName + "]");
        }
      }
      catch (Exception ex)
      {
        _logger.Error("Error getting related objects: " + ex);
        throw ex;
      }
    }

    public override IList<IDataObject> GetRelatedObjects(IDataObject dataObject, string relatedObjectType, int pageSize, int startIndex)
    {
      string objectType = dataObject.GetType().Name;

      if (objectType == typeof(GenericDataObject).Name)
      {
        objectType = ((GenericDataObject)dataObject).ObjectType;
      }

      try
      {
        DataObject objectDefinition = GetObjectDefinition(objectType);
        DataObject relatedObjectDefinition = GetObjectDefinition(relatedObjectType);

        DataTable dataTable = NewDataTable(objectDefinition);
        DataRow dataRow = CreateDataRow(dataTable, dataObject, objectDefinition);

        if (dataRow != null)
        {
          DataTable relatedDataTable = GetRelatedDataTable(dataRow, relatedObjectDefinition.tableName, startIndex, pageSize);
          return ToDataObjects(relatedDataTable, relatedObjectDefinition.objectName);
        }
        else
        {
          throw new Exception("Error creating/getting data row for object [" + objectDefinition.objectName + "]");
        }
      }
      catch (Exception ex)
      {
        _logger.Error("Error getting related objects: " + ex);
        throw ex;
      }
    }

    public override Response Post(IList<IDataObject> dataObjects)
    {
      try
      {
        IList<DataTable> dataTables = ToDataTables(dataObjects);
        return PostDataTables(dataTables);
      }
      catch (Exception ex)
      {
        _logger.Error("Error posting data tables: " + ex);
        throw ex;
      }
    }

    public override Response Delete(string objectType, DataFilter filter)
    {
      _dataFilter = filter;

      try
      {
        InitializeDatabaseDictionary();

        string tableName = GetTableName(objectType);
        string whereClause = string.Empty;

        if (filter != null)
          whereClause = filter.ToSqlWhereClause(_dbDictionary, tableName, _whereClauseAlias);

        return DeleteDataTable(tableName, whereClause);
      }
      catch (Exception ex)
      {
        _logger.Error("Error deleting data table: " + ex);
        throw ex;
      }
    }

    public override Response Delete(string objectType, IList<string> identifiers)
    {
      string tableName = GetTableName(objectType);

      try
      {
        return DeleteDataTable(tableName, identifiers);
      }
      catch (Exception ex)
      {
        _logger.Error("Error deleting data table: " + ex);
        throw ex;
      }
    }

    public override Response Refresh(string objectType) 
    {
      string tableName = string.Empty;

      if (!string.IsNullOrEmpty(objectType))
        tableName = GetTableName(objectType);

      return RefreshDataTable(tableName);
    }
    #endregion

    #region helper methods
    public string GetTableName(string objectType)
    {
      InitializeDatabaseDictionary();

      if (_dbDictionary.dataObjects != null)
      {
        foreach (DataObject dataObject in _dbDictionary.dataObjects)
        {
          if (dataObject.objectName.ToLower() == objectType.ToLower())
          {
            return dataObject.tableName;
          }
        }
      }

      throw new Exception("ObjectType [" + objectType + "] not found in dictionary [" +
        utility.Utility.SerializeDataContract<DatabaseDictionary>(_dbDictionary) + "]");
    }

    public DataObject GetObjectDefinition(string objectType)
    {
      InitializeDatabaseDictionary();

      if (_dbDictionary.dataObjects != null)
      {
        foreach (DataObject dataObject in _dbDictionary.dataObjects)
        {
          if (dataObject.objectName.ToLower() == objectType.ToLower())
          {
            return dataObject;
          }
        }
      }

      return null;
    }

    protected IDataObject ToDataObject(DataRow dataRow, DataObject objectDefinition)
    {
      IDataObject dataObject = null;

      if (dataRow != null)
      {
        try
        {
          dataObject = new GenericDataObject() { ObjectType = objectDefinition.objectName };
        }
        catch (Exception ex)
        {
          _logger.Error("Error instantiating data object: " + ex);
          throw ex;
        }

        if (dataObject != null && objectDefinition.dataProperties != null)
        {
          foreach (DataProperty objectProperty in objectDefinition.dataProperties)
          {
            try
            {
              if (dataRow.Table.Columns.Contains(objectProperty.columnName))
              {
                object value = dataRow[objectProperty.columnName];

                if (value.GetType() == typeof(System.DBNull))
                {
                  value = null;
                }

                dataObject.SetPropertyValue(objectProperty.propertyName, value);
              }
              else
              {
                _logger.Warn(String.Format("Value for column [{0}] not found in data row of table [{1}]",
                  objectProperty.columnName, objectDefinition.tableName));
              }
            }
            catch (Exception ex)
            {
              _logger.Error("Error getting data row value: " + ex);
              throw ex;
            }
          }
        }
      }
      else
      {
        dataObject = new GenericDataObject() { ObjectType = objectDefinition.objectName };

        foreach (DataProperty objectProperty in objectDefinition.dataProperties)
        {
          dataObject.SetPropertyValue(objectProperty.propertyName, null);
        }
      }

      return dataObject;
    }

    protected IList<IDataObject> ToDataObjects(DataTable dataTable, string objectType)
    {
      IList<IDataObject> dataObjects = new List<IDataObject>();
      DataObject objectDefinition = GetObjectDefinition(objectType);
      IDataObject dataObject = null;
          
      if (objectDefinition != null && dataTable.Rows != null)
      {
        if (dataTable.Rows.Count == 0)
        {
          dataObject = ToDataObject(null, objectDefinition);
          dataObjects.Add(dataObject);
        }
        else
        {
          foreach (DataRow dataRow in dataTable.Rows)
          {
            try
            {
              dataObject = ToDataObject(dataRow, objectDefinition);
            }
            catch (Exception ex)
            {
              _logger.Error("Error converting data row to data object: " + ex);
              throw ex;
            }

            if (dataObjects != null)
            {
              dataObjects.Add(dataObject);
            }
          }
        }
      }

      return dataObjects;
    }

    protected DataRow CreateDataRow(DataTable dataTable, IDataObject dataObject, DataObject objectDefinition)
    {
      DataRow dataRow = null;

      if (dataObject != null)
      {
        try
        {
          dataRow = dataTable.NewRow();

          foreach (DataProperty objectProperty in objectDefinition.dataProperties)
          {
            object value = dataObject.GetPropertyValue(objectProperty.propertyName);

            if (value != null && value.ToString().Trim().Length > 0)
            {
              switch (objectProperty.dataType)
              {
                case DataType.Boolean:
                  dataRow[objectProperty.columnName] = Convert.ToBoolean(value);
                  break;
                case DataType.Byte:
                  dataRow[objectProperty.columnName] = Convert.ToByte(value);
                  break;
                case DataType.Int16:
                  dataRow[objectProperty.columnName] = Convert.ToInt16(value);
                  break;
                case DataType.Int32:
                  dataRow[objectProperty.columnName] = Convert.ToInt32(value);
                  break;
                case DataType.Int64:
                  dataRow[objectProperty.columnName] = Convert.ToInt64(value);
                  break;
                case DataType.Decimal:
                  dataRow[objectProperty.columnName] = Convert.ToDecimal(value);
                  break;
                case DataType.Single:
                  dataRow[objectProperty.columnName] = Convert.ToSingle(value);
                  break;
                case DataType.Double:
                  dataRow[objectProperty.columnName] = Convert.ToDouble(value);
                  break;
                case DataType.DateTime:
                  dataRow[objectProperty.columnName] = Convert.ToDateTime(value);
                  break;
                default:
                  dataRow[objectProperty.columnName] = value;
                  break;
              }
            }
            else if (objectProperty.dataType == DataType.String || objectProperty.isNullable)
            {
              dataRow[objectProperty.columnName] = DBNull.Value;
            }
            else
            {
              _logger.Error("Object property is set to not nullable but received a null value.");
              return null;
            }
          }
        }
        catch (Exception ex)
        {
          _logger.Error("Error getting data row value: " + ex);
          throw ex;
        }
      }

      return dataRow;
    }

    protected DataTable NewDataTable(DataObject objectDefinition)
    {
      DataTable dataTable = new DataTable(objectDefinition.tableName);

      foreach (DataProperty objectProperty in objectDefinition.dataProperties)
      {
        DataColumn dataColumn = new DataColumn()
        {
          ColumnName = objectProperty.columnName,
          DataType = Type.GetType("System." + objectProperty.dataType.ToString())
        };

        if (objectProperty.dataType == DataType.String)
        {
          dataColumn.MaxLength = objectProperty.dataLength;
        }

        dataTable.Columns.Add(dataColumn);
      }

      return dataTable;
    }

    protected IList<string> GetKeyColumns(DataObject objectDefinition)
    {
      IList<string> keyCols = new List<string>();

      foreach (DataProperty dataProp in objectDefinition.dataProperties)
      {
        foreach (KeyProperty keyProp in objectDefinition.keyProperties)
        {
          if (dataProp.propertyName == keyProp.keyPropertyName)
          {
            keyCols.Add(dataProp.columnName);
          }
        }
      }

      return keyCols;
    }

    protected string GetIdentifier(DataObject objectDefinition, DataRow row)
    {
      IList<string> keyCols = GetKeyColumns(objectDefinition);
      string identifier = string.Empty;

      foreach (string key in keyCols)
      {
        if (identifier.Length > 0)
          identifier += objectDefinition.keyDelimeter;

        identifier += row[key];
      }

      return identifier;
    }

    protected IList<DataTable> ToDataTables(IList<IDataObject> dataObjects)
    {
      try
      {
        IList<DataTable> dataTables = new List<DataTable>();

        //TODO: create a class for these structures
        Dictionary<string, DataObject> objectTypesObjectDefinitions = new Dictionary<string, DataObject>();
        Dictionary<string, IList<string>> objectTypesIdentifiers = new Dictionary<string, IList<string>>();
        Dictionary<string, IList<IDataObject>> objectTypesDataObjects = new Dictionary<string, IList<IDataObject>>();

        // collect info about each object type
        if (dataObjects != null)
        {
          foreach (IDataObject dataObject in dataObjects)
          {
            string objectType = dataObject.GetType().Name;

            if (objectType == typeof(GenericDataObject).Name)
            {
              objectType = ((GenericDataObject)dataObject).ObjectType;
            }

            if (objectTypesIdentifiers.ContainsKey(objectType))
            {
              DataObject objectDefinition = objectTypesObjectDefinitions[objectType];
              string identifier = GetIdentifier(objectDefinition, dataObject);
              objectTypesIdentifiers[objectType].Add(identifier);
              objectTypesDataObjects[objectType].Add(dataObject);
            }
            else
            {
              DataObject objectDefinition = GetObjectDefinition(objectType);
              string identifier = GetIdentifier(objectDefinition, dataObject);
              objectTypesObjectDefinitions[objectType] = objectDefinition;
              objectTypesIdentifiers[objectType] = new List<string>() { identifier };
              objectTypesDataObjects[objectType] = new List<IDataObject>() { dataObject };
            }
          }
        }

        // create or update rows accordingly
        foreach (var pair in objectTypesIdentifiers)
        {
          DataObject objectDefinition = objectTypesObjectDefinitions[pair.Key];

          if (!objectDefinition.isReadOnly)
          {
            IList<string> identifiers = objectTypesIdentifiers[pair.Key];
            DataTable dataTable = GetDataTable(objectDefinition.tableName, pair.Value);
            DataRow dataRow = null;

            for (int i = 0; i < identifiers.Count; i++)
            {
              string identifier = identifiers[i];

              // find row with same identifier
              foreach (DataRow row in dataTable.Rows)
              {
                string rowIdentifier = GetIdentifier(objectDefinition, row);

                if (rowIdentifier == identifier)
                {
                  dataRow = row;
                  dataTable.AcceptChanges();
                  break;
                }
              }

              // if row does not exist, create new one
              if (dataRow == null)
              {
                dataRow = dataTable.NewRow();
                dataTable.Rows.Add(dataRow);
              }

              // update or fill row values from data object properties
              IDataObject dataObject = objectTypesDataObjects[pair.Key][i];

              foreach (DataProperty objectProperty in objectDefinition.dataProperties)
              {
                if (!objectProperty.isReadOnly)
                {
                  object value = dataObject.GetPropertyValue(objectProperty.propertyName);

                  if (value != null && value.ToString().Trim().Length > 0)
                  {
                    switch (objectProperty.dataType)
                    {
                      case DataType.Boolean:
                        dataRow[objectProperty.columnName] = Convert.ToBoolean(value);
                        break;
                      case DataType.Byte:
                        dataRow[objectProperty.columnName] = Convert.ToByte(value);
                        break;
                      case DataType.Int16:
                        dataRow[objectProperty.columnName] = Convert.ToInt16(value);
                        break;
                      case DataType.Int32:
                        dataRow[objectProperty.columnName] = Convert.ToInt32(value);
                        break;
                      case DataType.Int64:
                        dataRow[objectProperty.columnName] = Convert.ToInt64(value);
                        break;
                      case DataType.Decimal:
                        dataRow[objectProperty.columnName] = Convert.ToDecimal(value);
                        break;
                      case DataType.Single:
                        dataRow[objectProperty.columnName] = Convert.ToSingle(value);
                        break;
                      case DataType.Double:
                        dataRow[objectProperty.columnName] = Convert.ToDouble(value);
                        break;
                      case DataType.DateTime:
                        dataRow[objectProperty.columnName] = Convert.ToDateTime(value);
                        break;
                      default:
                        dataRow[objectProperty.columnName] = value;
                        break;
                    }
                  }
                  else if (objectProperty.dataType == DataType.String || objectProperty.isNullable)
                  {
                    dataRow[objectProperty.columnName] = DBNull.Value;
                  }
                  else
                  {
                    _logger.Error(string.Format("Object property [{0}] does not allow null value.", objectProperty.propertyName));
                  }
                }
              }
            }

            dataTables.Add(dataTable);
          }
        }

        return dataTables;
      }
      catch (Exception e)
      {
        _logger.Error("Error marshalling data objects to data tables: " + e);
        throw e;
      }
    }

    private void InitializeDatabaseDictionary()
    {
      if (_dbDictionary == null)
      {
        try
        {
          _dbDictionary = GetDatabaseDictionary();
        }
        catch (Exception ex)
        {
          _logger.Error("Error initializing dictionary: " + ex);
          throw ex;
        }
      }
    }

    protected DataFilter CreateRelatedDataFilter(DataRow parentDataRow, string relatedTableName)
    {
      DataObject parentDataObject = _dbDictionary.dataObjects.Find(x => x.tableName == parentDataRow.Table.TableName);
      if (parentDataObject == null)
      {
        throw new Exception("Parent data table [" + parentDataRow.Table.TableName + "] not found.");
      }

      DataObject relatedDataObject = _dbDictionary.dataObjects.Find(x => x.tableName == relatedTableName);
      if (relatedDataObject == null)
      {
        throw new Exception("Related data table [" + relatedTableName + "] not found.");
      }

      DataRelationship dataRelationship = parentDataObject.dataRelationships.Find(c => c.relatedObjectName.ToLower() == relatedDataObject.tableName.ToLower());
      if (dataRelationship == null)
      {
        throw new Exception("Relationship between data table [" + parentDataRow.Table.TableName +
          "] and related data table [" + relatedTableName + "] not found.");
      }

      DataFilter filter = new DataFilter();

      foreach (PropertyMap propertyMap in dataRelationship.propertyMaps)
      {
        DataProperty parentDataProperty = parentDataObject.dataProperties.Find(x => x.propertyName.ToLower() == propertyMap.dataPropertyName);
        DataProperty relatedDataProperty = relatedDataObject.dataProperties.Find(x => x.propertyName.ToLower() == propertyMap.relatedPropertyName);

        Expression expression = new Expression()
        {
          PropertyName = relatedDataProperty.columnName,
          RelationalOperator = RelationalOperator.EqualTo,
          Values = new Values
          {
            parentDataRow[parentDataProperty.columnName].ToString()
          }
        };

        if (filter.Expressions.Count > 0)
        {
          expression.LogicalOperator = LogicalOperator.And;
        }

        filter.Expressions.Add(expression);
      }

      return filter;
    }
    #endregion
  }
}
