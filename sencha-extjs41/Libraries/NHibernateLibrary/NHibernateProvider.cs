// Copyright (c) 2010, iringtools.org /////////////////////////////////////////////
// All rights reserved.
//------------------------------------------------------------------------------
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//     * Redistributions of source code must retain the above copyright
//       notice, this list of conditions and the following disclaimer.
//     * Redistributions in binary form must reproduce the above copyright
//       notice, this list of conditions and the following disclaimer in the
//       documentation and/or other materials provided with the distribution.
//     * Neither the name of the ids-adi.org nor the
//       names of its contributors may be used to endorse or promote products
//       derived from this software without specific prior written permission.
//------------------------------------------------------------------------------
// THIS SOFTWARE IS PROVIDED BY ids-adi.org ''AS IS'' AND ANY
// EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL ids-adi.org BE LIABLE FOR ANY
// DIRECT, INDIRECT, INCIDENTAL, SPECIAL + exEMPLARY, OR CONSEQUENTIAL DAMAGES
// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
// LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
// ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
// SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Xml.Linq;
using log4net;
using NHibernate;
using Ninject;
using org.iringtools.adapter;
using org.iringtools.library;
using org.iringtools.utility;
using Microsoft.SqlServer.Management.Smo.Wmi;

namespace org.iringtools.nhibernate
{
  public class NHibernateProvider
  {
    private static readonly ILog _logger = LogManager.GetLogger(typeof(NHibernateProvider));

    private IKernel _kernel = null;
    private NHibernateSettings _settings = null;

    [Inject]
    public NHibernateProvider(NameValueCollection settings)
    {
      _kernel = new StandardKernel(new NHibernateModule());
      _settings = _kernel.Get<NHibernateSettings>();
      _settings.AppendSettings(settings);

      Directory.SetCurrentDirectory(_settings["BaseDirectoryPath"]);
    }

    #region public methods
    public Response Generate(string projectName, string applicationName)
    {
      Response response = new Response();
      Status status = new Status();

      response.StatusList.Add(status);

      try
      {
        status.Identifier = String.Format("{0}.{1}", projectName, applicationName);
        InitializeScope(projectName, applicationName);

        DatabaseDictionary dbDictionary = NHibernateUtility.LoadDatabaseDictionary(_settings["DBDictionaryPath"]);
        if (String.IsNullOrEmpty(projectName) || String.IsNullOrEmpty(applicationName))
        {
          status.Messages.Add("Error project name and application name can not be null");
        }
        else if (ValidateDatabaseDictionary(dbDictionary))
        {
          EntityGenerator generator = _kernel.Get<EntityGenerator>();

          string compilerVersion = "v4.0";
          if (!String.IsNullOrEmpty(_settings["CompilerVersion"]))
          {
            compilerVersion = _settings["CompilerVersion"];
          }

          response.Append(generator.Generate(compilerVersion, dbDictionary, projectName, applicationName));
          status.Messages.Add("Database dictionary of [" + projectName + "." + applicationName + "] updated successfully.");
        }
      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Error in UpdateDatabaseDictionary: {0}", ex));

        status.Level = StatusLevel.Error;
        status.Messages.Add(string.Format("Error updating database dictionary: {0}", ex));
      }

      return response;
    }

    public DatabaseDictionary GetDictionary(string projectName, string applicationName)
    {
      DatabaseDictionary databaseDictionary = new DatabaseDictionary();

      try
      {
        InitializeScope(projectName, applicationName);

        if (File.Exists(_settings["DBDictionaryPath"]))
        {
          databaseDictionary = NHibernateUtility.LoadDatabaseDictionary(_settings["DBDictionaryPath"]);
        }
        else
        {
          databaseDictionary = new DatabaseDictionary();
          NHibernateUtility.SaveDatabaseDictionary(databaseDictionary, _settings["DBDictionaryPath"]);
        }
      }
      catch (Exception ex)
      {
        _logger.Error("Error in GetDbDictionary: " + ex);
        return null;
      }

      return databaseDictionary;
    }

    public Response PostDictionary(string projectName, string applicationName, DatabaseDictionary databaseDictionary)
    {
      Response response = new Response();
      Status status = new Status();
      DataObject tempDataObject = null;
      response.StatusList.Add(status);

      try
      {
        status.Identifier = String.Format("{0}.{1}", projectName, applicationName);
        InitializeScope(projectName, applicationName);
        DatabaseDictionary existDBDictionary = GetDictionary(projectName, applicationName);
        
        foreach (DataObject dataObject in databaseDictionary.dataObjects)
        {
          tempDataObject = existDBDictionary.GetTableObject(dataObject.tableName);
          if (tempDataObject != null)
            if (tempDataObject.dataFilter != null)
              dataObject.dataFilter = tempDataObject.dataFilter;
        }

        NHibernateUtility.SaveDatabaseDictionary(databaseDictionary, _settings["DBDictionaryPath"]);
        response.Append(Generate(projectName, applicationName));

        if (response.Level.ToString().ToUpper() == "SUCCESS")
          status.Messages.Add("Database Dictionary saved successfully");
        else
        {
          throw new Exception(response.StatusList[0].Messages[0].ToString());
        }

      }
      catch (Exception ex)
      {
        _logger.Error("Error in SaveDatabaseDictionary: " + ex);
        status.Messages.Add("Error in saving database dictionary" + ex.Message);
      }

      return response;
    }

    public DataRelationships GetRelationships()
    {
      try
      {
        return new DataRelationships();
      }
      catch (Exception ex)
      {
        _logger.Error("Error in GetRelationships: " + ex);
        return null;
      }
    }

    public DataProviders GetProviders()
    {
      try
      {
        return new DataProviders();
      }
      catch (Exception ex)
      {
        _logger.Error("Error in GetProviders: " + ex);
        return null;
      }
    }

    public DataObjects GetSchemaObjects(string projectName, string applicationName)
    {
      DataObjects tableNames = new DataObjects();
      DatabaseDictionary dbDictionary = new DatabaseDictionary();

      try
      {
        _logger.Debug(String.Format("In GetSchemaObjects({0}, {1})", projectName, applicationName));

        InitializeScope(projectName, applicationName);

        if (File.Exists(_settings["DBDictionaryPath"]))
          dbDictionary = NHibernateUtility.LoadDatabaseDictionary(_settings["DBDictionaryPath"]);
        else
          return tableNames;

        string connString = dbDictionary.ConnectionString;
        string dbProvider = dbDictionary.Provider.ToString().ToUpper();
        string schemaName = dbDictionary.SchemaName;
        string parsedConnStr = ParseConnectionString(connString, dbProvider);

        _logger.DebugFormat("ConnectString: {0} \r\n Provider: {1} \r\n SchemaName: {2} \r\n Parsed: {3}",
            connString,
            dbProvider,
            schemaName,
            parsedConnStr);

        Dictionary<string, string> properties = new Dictionary<string, string>();

        dbDictionary.ConnectionString = parsedConnStr;
        dbDictionary.dataObjects = new System.Collections.Generic.List<DataObject>();

        properties.Add("connection.provider", "NHibernate.Connection.DriverConnectionProvider");
        properties.Add("proxyfactory.factory_class", "NHibernate.ByteCode.Castle.ProxyFactoryFactory, NHibernate.ByteCode.Castle");
        properties.Add("connection.connection_string", parsedConnStr);

        string connDriver = GetConnectionDriver(dbProvider);

        _logger.DebugFormat("connection.driver_class: {0}", connDriver);

        properties.Add("connection.driver_class", connDriver);

        string databaseDialect = GetConnectionDriver(dbProvider);

        _logger.DebugFormat("dialect: {0}", databaseDialect);

        properties.Add("dialect", databaseDialect);

        NHibernate.Cfg.Configuration config = new NHibernate.Cfg.Configuration();

        _logger.Debug("Adding Properties to Config");

        config.AddProperties(properties);

        _logger.Debug("Building Session Factory");

        ISessionFactory sessionFactory = config.BuildSessionFactory();

        _logger.Debug("About to Open Session");

        ISession session = sessionFactory.OpenSession();

        _logger.Debug("Session Open");

        string sql = GetDatabaseMetaquery(dbProvider, parsedConnStr.Split(';')[1].Split('=')[1], schemaName);

        _logger.DebugFormat("SQL: {0}",
            sql);

        ISQLQuery query = session.CreateSQLQuery(sql);

        DataObjects metadataList = new DataObjects();
        foreach (string tableName in query.List<string>())
        {
          metadataList.Add(tableName);
        }
        session.Close();

        tableNames = metadataList;
        return tableNames;
      }
      catch (Exception ex)
      {
        _logger.ErrorFormat("Error while Getting Schema Objects from database. {0}",
              ex.ToString());

        return tableNames;
      }
    }

    public DataObject GetSchemaObjectSchema(string projectName, string applicationName, string schemaObjectName)
    {
      List<string> tableNames = new List<string>();
      DatabaseDictionary dbDictionary = new DatabaseDictionary();
      DataObject dataObject = new DataObject
      {
        tableName = schemaObjectName,
        dataProperties = new List<DataProperty>(),
        keyProperties = new List<KeyProperty>(),
        dataRelationships = new List<DataRelationship>(),
        objectNamespace = String.Format("org.iringtools.adapter.datalayer.proj_{0}.{1}", projectName, applicationName),
        objectName = Utility.ToSafeName(schemaObjectName)
      };

      try
      {
        InitializeScope(projectName, applicationName);

        if (File.Exists(_settings["DBDictionaryPath"]))
          dbDictionary = NHibernateUtility.LoadDatabaseDictionary(_settings["DBDictionaryPath"]);

        string connString = dbDictionary.ConnectionString;
        string dbProvider = dbDictionary.Provider.ToString().ToUpper();
        string schemaName = dbDictionary.SchemaName;
        string parsedConnStr = ParseConnectionString(connString, dbProvider);

        Dictionary<string, string> properties = new Dictionary<string, string>();

        dbDictionary.ConnectionString = parsedConnStr;
        dbDictionary.dataObjects = new System.Collections.Generic.List<DataObject>();

        properties.Add("connection.provider", "NHibernate.Connection.DriverConnectionProvider");
        properties.Add("proxyfactory.factory_class", "NHibernate.ByteCode.Castle.ProxyFactoryFactory, NHibernate.ByteCode.Castle");
        properties.Add("connection.connection_string", parsedConnStr);
        properties.Add("connection.driver_class", GetConnectionDriver(dbProvider));
        properties.Add("dialect", GetDatabaseDialect(dbProvider));

        NHibernate.Cfg.Configuration config = new NHibernate.Cfg.Configuration();
        config.AddProperties(properties);

        ISessionFactory sessionFactory = config.BuildSessionFactory();
        ISession session = sessionFactory.OpenSession();
        ISQLQuery query = session.CreateSQLQuery(GetTableMetaQuery(dbProvider, schemaName, schemaObjectName));
        IList<object[]> metadataList = query.List<object[]>();
        session.Close();

        foreach (object[] metadata in metadataList)
        {
          string columnName = Convert.ToString(metadata[0]);
          string dataType = Utility.SqlTypeToCSharpType(Convert.ToString(metadata[1]));
          int dataLength = Convert.ToInt32(metadata[2]);
          bool isIdentity = Convert.ToBoolean(metadata[3]);
          string nullable = Convert.ToString(metadata[4]).ToUpper();
          bool isNullable = (nullable == "Y" || nullable == "TRUE" || nullable == "1");
          string constraint = Convert.ToString(metadata[5]);

          if (String.IsNullOrEmpty(constraint)) // process columns
          {
            DataProperty column = new DataProperty()
            {
              columnName = columnName,
              dataType = (DataType)Enum.Parse(typeof(DataType), dataType),
              dataLength = dataLength,
              isNullable = isNullable,
              propertyName = Utility.ToSafeName(columnName)
            };

            dataObject.dataProperties.Add(column);
          }
          else
          {
            KeyType keyType = KeyType.assigned;

            if (isIdentity)
            {
              keyType = KeyType.identity;
            }
            else if (constraint.ToUpper() == "FOREIGN KEY" || constraint.ToUpper() == "R")
            {
              keyType = KeyType.foreign;
            }

            DataProperty key = new DataProperty()
            {
              columnName = columnName,
              dataType = (DataType)Enum.Parse(typeof(DataType), dataType),
              dataLength = dataLength,
              isNullable = isNullable,
              keyType = keyType,
              propertyName = Utility.ToSafeName(columnName),
            };
            dataObject.addKeyProperty(key);
          }
        }
        return dataObject;
      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Error in GetSchemaObjectSchema: {0}", ex));
        return dataObject;
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

    public List<string> GetTableNames(string projectName, string applicationName, string dbProvider,
      string dbServer, string portNumber, string dbInstance, string dbName, string dbSchema, string dbUserName, string dbPassword, string serName)
    {
      List<string> tableNames = new List<string>();

      try
      {
        InitializeScope(projectName, applicationName);

        List<DataObject> dataObjects = new List<DataObject>();
        ISession session = GetNHSession(dbProvider, dbServer, dbInstance, dbName, dbSchema, dbUserName, dbPassword, portNumber, serName);
        string sql = GetDatabaseMetaquery(dbProvider, dbName, dbSchema);
        ISQLQuery query = session.CreateSQLQuery(sql);

        foreach (string tableName in query.List<string>())
        {
          tableNames.Add(tableName);
        }

        session.Close();
      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Error in GetTableNames: {0}", ex));
        throw ex;
      }

      return tableNames;
    }

    public List<DataObject> GetDBObjects(string projectName, string applicationName, string dbProvider, string dbServer, string portNumber,
      string dbInstance, string dbName, string dbSchema, string dbUserName, string dbPassword, string tableNames, string serName)
    {
      List<DataObject> dataObjects = new List<DataObject>();
      ISession session = GetNHSession(dbProvider, dbServer, dbInstance, dbName, dbSchema, dbUserName, dbPassword, portNumber, serName);

      foreach (string tableName in tableNames.Split(','))
      {
        DataObject dataObject = new DataObject()
        {
          tableName = tableName,
          objectName = Utility.ToSafeName(tableName)
        };

        string sql = GetTableMetaQuery(dbProvider, dbSchema, tableName);
        ISQLQuery query = session.CreateSQLQuery(sql);
        IList<object[]> metadataList = query.List<object[]>();

        foreach (object[] metadata in metadataList)
        {
          string columnName = Convert.ToString(metadata[0]);
          string dataType = Utility.SqlTypeToCSharpType(Convert.ToString(metadata[1]));
          int dataLength = Convert.ToInt32(metadata[2]);
          bool isIdentity = Convert.ToBoolean(metadata[3]);
          bool isNullable;
          try
          {
            isNullable = Convert.ToBoolean(metadata[4]);
          }
          catch
          {
            string nullable = Convert.ToString(metadata[4]).ToUpper();
            isNullable = (nullable == "Y" || nullable == "TRUE" || nullable == "1");
          }
          string constraint = Convert.ToString(metadata[5]);

          if (String.IsNullOrEmpty(constraint)) // process columns
          {
            DataProperty column = new DataProperty()
            {
              columnName = columnName,
              dataType = (DataType)Enum.Parse(typeof(DataType), dataType),
              dataLength = dataLength,
              isNullable = isNullable,
              isHidden = true,
              propertyName = Utility.ToSafeName(columnName)
            };

            dataObject.dataProperties.Add(column);
          }
          else
          {
            KeyType keyType = KeyType.assigned;

            if (isIdentity)
            {
              keyType = KeyType.identity;
            }
            else if (constraint.ToUpper() == "FOREIGN KEY" || constraint.ToUpper() == "R")
            {
              keyType = KeyType.foreign;
            }

            DataProperty key = new DataProperty()
            {
              columnName = columnName,
              dataType = (DataType)Enum.Parse(typeof(DataType), dataType),
              dataLength = dataLength,
              isNullable = isNullable,
              keyType = keyType,
              propertyName = Utility.ToSafeName(columnName),
            };

            dataObject.addKeyProperty(key);
          }
        }

        dataObjects.Add(dataObject);
      }

      session.Close();

      return dataObjects;
    }
    #endregion

    #region private methods
    private string getMssqlInstanceName()
    {
      string mssqlInstanceName = "";
      ManagedComputer mc = new ManagedComputer();
      if (mc.ServerInstances.Count == 1)
        mssqlInstanceName = mc.ServerInstances[0].Name;

      if (mssqlInstanceName == "")
        mssqlInstanceName = "SQLEXPRESS";

      return mssqlInstanceName;
    }

    private ISession GetNHSession(string dbProvider, string dbServer, string dbInstance, string dbName, string dbSchema,
      string dbUserName, string dbPassword, string portNumber, string serName)
    {
      string connStr, defaultConnStr = "";

      if (portNumber == "")
      {
        if (dbProvider.ToUpper().Contains("ORACLE"))
          portNumber = "1521";
        else if (dbProvider.ToUpper().Contains("MYSQL"))
          portNumber = "3306";
      }

      if (dbProvider.ToUpper().Contains("MSSQL"))
        if (dbInstance == "default" || string.IsNullOrEmpty(dbInstance))
        {
          string mssqlInstanceName = getMssqlInstanceName();
          connStr = String.Format("Data Source={0};Initial Catalog={2};User ID={3};Password={4}",
            dbServer, dbInstance, dbName, dbUserName, dbPassword);
          defaultConnStr = String.Format("Data Source={0}\\{1};Initial Catalog={2};User ID={3};Password={4}",
           dbServer, mssqlInstanceName, dbName, dbUserName, dbPassword);
        }
        else
        {
          connStr = String.Format("Data Source={0}\\{1};Initial Catalog={2};User ID={3};Password={4}",
           dbServer, dbInstance, dbName, dbUserName, dbPassword);
        }
        else
          connStr = String.Format("Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST={0})(PORT={1})))(CONNECT_DATA=(SERVER=DEDICATED)({2}={3})));User ID={4};Password={5}",
            dbServer, portNumber, serName, dbInstance, dbUserName, dbPassword);

      Dictionary<string, string> properties = new Dictionary<string, string>();
      properties.Add("connection.provider", "NHibernate.Connection.DriverConnectionProvider");
      properties.Add("proxyfactory.factory_class", "NHibernate.ByteCode.Castle.ProxyFactoryFactory, NHibernate.ByteCode.Castle");
      properties.Add("connection.connection_string", connStr);
      properties.Add("connection.driver_class", GetConnectionDriver(dbProvider));
      properties.Add("dialect", GetDatabaseDialect(dbProvider));

      NHibernate.Cfg.Configuration config = new NHibernate.Cfg.Configuration();
      config.AddProperties(properties);

      ISessionFactory sessionFactory = null;

      try
      {
        sessionFactory = config.BuildSessionFactory();
      }
      catch (Exception e)
      {
        if (defaultConnStr != "")
        {
          config.Properties["connection.connection_string"] = defaultConnStr;
          sessionFactory = config.BuildSessionFactory();
        }
        else
        {
          _logger.Error(string.Format("Error get NH session: {0}", e));
          throw e;
        }
      }

      return sessionFactory.OpenSession();
    }

    private string GetDatabaseMetaquery(string dbProvider, string database, string schemaName)
    {
      string metaQuery = String.Empty;

      if (dbProvider.ToUpper().Contains("MYSQL"))
      {
        metaQuery = String.Format(@"
          select table_name from information_schema.tables 
          where upper(table_schema) = '{0}' order by table_name", schemaName.ToUpper());
      }
      else if (dbProvider.ToUpper().Contains("MSSQL"))
      {
        metaQuery = String.Format(@"
          select name from sys.objects where (upper(type) = 'U' or upper(type) = 'V') 
          and upper(schema_name(schema_id)) = '{0}' order by name", schemaName.ToUpper());
      }
      else if (dbProvider.ToUpper().Contains("ORACLE"))
      {
        metaQuery = String.Format(@"
          select object_name from all_objects where object_type in ('TABLE', 'VIEW', 'SYNONYM') 
          and upper(owner) = '{0}' order by object_name", schemaName.ToUpper());
      }
      else
      {
        throw new Exception(string.Format("Database provider {0} not supported.", dbProvider));
      }

      return metaQuery;
    }

    private string GetTableMetaQuery(string dbProvider, string schemaName, string tableName)
    {
      string tableQuery = string.Empty;

      if (dbProvider.ToUpper().Contains("MYSQL"))
      {
        tableQuery = String.Format(@"
          select t1.table_name, t1.column_name, t1.data_type, t1.character_maximum_length as data_length, 
          t1.extra as is_auto_increment, t1.is_nullable, t1.column_key from information_schema.columns t1 
          left join information_schema.views t2 on t2.table_name = t1.table_name 
          where upper(t1.table_schema) = '{0}' and upper(t1.table_name) = '{1}'",
          schemaName.ToUpper(), tableName.ToUpper());
      }
      else if (dbProvider.ToUpper().Contains("MSSQL"))
      {
        tableQuery = String.Format(@"
          select t2.name as column_name, type_name(t2.user_type_id) as data_type, 
          t2.max_length as data_length, t2.is_identity as is_identity, 
          t2.is_nullable as is_nullable, t4.column_id as is_primary_key from sys.objects t1
          inner join sys.columns t2 on t2.object_id = t1.object_id  
          left join sys.index_columns t4 on t4.object_id = t1.object_id and t4.column_id = t2.column_id
          left join sys.indexes t3 on t3.object_id = t1.object_id and t3.is_unique = 1
          and t3.index_id = t4.index_id
          where (upper(t1.type) = 'U' or upper(t1.type) = 'V') 
          and upper(schema_name(t1.schema_id)) = '{0}' and upper(t1.name) = '{1}'",
          schemaName.ToUpper(), tableName.ToUpper());
      }
      else if (dbProvider.ToUpper().Contains("ORACLE"))
      {
        tableQuery = string.Format(@"
          SELECT t2.column_name, t2.data_type, t2.data_length,
          0 AS is_sequence, t2.nullable, t4.constraint_type
          FROM all_objects t1 INNER JOIN all_tab_cols t2
          ON t2.table_name = t1.object_name AND t2.owner = t1.owner 
          LEFT JOIN all_cons_columns t3 ON t3.table_name = t2.table_name
          AND t3.column_name = t2.column_name AND t3.owner = t2.owner
          AND SUBSTR(t3.constraint_name, 0, 3) != 'SYS' LEFT JOIN all_constraints t4
          ON t4.constraint_name = t3.constraint_name AND t4.owner = t3.owner
          AND (t4.constraint_type = 'P' OR t4.constraint_type = 'R')
          WHERE UPPER(t1.owner) = '{0}' AND UPPER(t1.object_name) = '{1}' ORDER BY t2.column_name",
          schemaName.ToUpper(), tableName.ToUpper());
      }
      else
      {
        throw new Exception(string.Format("Database provider {0} not supported.", dbProvider));
      }

      return tableQuery;
    }

    private string GetDatabaseDialect(string dbProvider)
    {
      switch (dbProvider.ToUpper())
      {
        case "MSSQL2008":
          return "NHibernate.Dialect.MsSql2008Dialect";

        case "MSSQL2005":
          return "NHibernate.Dialect.MsSql2005Dialect";

        case "MSSQL2000":
          return "NHibernate.Dialect.MsSql2000Dialect";

        case "ORACLE10G":
          return "NHibernate.Dialect.Oracle10gDialect";

        case "ORACLE9I":
          return "NHibernate.Dialect.Oracle9iDialect";

        case "ORACLE8I":
          return "NHibernate.Dialect.Oracle8iDialect";

        case "ORACLELITE":
          return "NHibernate.Dialect.OracleLiteDialect";

        case "MYSQL3":
        case "MYSQL4":
        case "MYSQL5":
          return "NHibernate.Dialect.MySQL5Dialect";

        default:
          throw new Exception(string.Format("Database provider {0} not supported.", dbProvider));
      }
    }

    private string GetConnectionDriver(string dbProvider)
    {
      if (dbProvider.ToUpper().Contains("MSSQL"))
      {
        return "NHibernate.Driver.SqlClientDriver";
      }
      else if (dbProvider.ToUpper().Contains("MYSQL"))
      {
        return "NHibernate.Driver.MySqlDataDriver";
      }
      else if (dbProvider.ToUpper().Contains("ORACLE"))
      {
        return "NHibernate.Driver.OracleClientDriver";
      }
      else
        throw new Exception(string.Format("Database provider {0} is not supported", dbProvider));
    }

    private void InitializeScope(string projectName, string applicationName)
    {
      try
      {
        string scope = string.Format("{0}.{1}", projectName, applicationName);

        _settings["ProjectName"] = projectName;
        _settings["ApplicationName"] = applicationName;
        _settings["Scope"] = scope;
        _settings["DBDictionaryPath"] = String.Format("{0}DatabaseDictionary.{1}.xml", _settings["AppDataPath"], scope);
      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Error initializing application: {0}", ex));
        throw new Exception(string.Format("Error initializing application: {0})", ex));
      }
    }

    private bool ValidateDatabaseDictionary(DatabaseDictionary dbDictionary)
    {
      // Validate table key
      foreach (DataObject dataObject in dbDictionary.dataObjects)
      {
        if (dataObject.keyProperties == null || dataObject.keyProperties.Count == 0)
        {
          throw new Exception(string.Format("Table \"{0}\" has no key. Must select keys before saving.", dataObject.tableName));
        }
      }

      return true;
    }

    private static string ParseConnectionString(string connStr, string dbProvider)
    {
      try
      {
        string parsedConnStr = String.Empty;
        char[] ch = { ';' };
        string[] connStrKeyValuePairs = connStr.Split(ch, StringSplitOptions.RemoveEmptyEntries);

        foreach (string connStrKeyValuePair in connStrKeyValuePairs)
        {
          string[] connStrKeyValuePairTemp = connStrKeyValuePair.Split('=');
          string connStrKey = connStrKeyValuePairTemp[0].Trim();
          string connStrValue = connStrKeyValuePairTemp[1].Trim();

          if (connStrKey.ToUpper() == "DATA SOURCE" ||
              connStrKey.ToUpper() == "USER ID" ||
              connStrKey.ToUpper() == "PASSWORD")
          {
            parsedConnStr += connStrKey + "=" + connStrValue + ";";
          }

          if (dbProvider.ToUpper().Contains("MSSQL"))
          {
            if (connStrKey.ToUpper() == "INITIAL CATALOG" ||
                connStrKey.ToUpper() == "INTEGRATED SECURITY")
            {
              parsedConnStr += connStrKey + "=" + connStrValue + ";";
            }
          }
          else if (dbProvider.ToUpper().Contains("MYSQL"))
          {
            parsedConnStr += connStrKey + "=" + connStrValue + ";";
          }
        }

        return parsedConnStr;
      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Error in ParseConnectionString: {0}", ex));
        throw ex;
      }
    }
    #endregion
  }
}