using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Web.Configuration;
using org.iringtools.utility;
using org.iringtools.library;
using NHibernate;
using log4net;

namespace DbDictionaryService
{
    public class DbDictionaryService : IService
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(DbDictionaryService));
        WebProxyCredentials _proxyCredentials = null;
        string adapterServiceUri = string.Empty;
        string dbDictionaryFullFilePath = string.Empty;
        string proxyPort = string.Empty;
        string proxyHost = string.Empty;
        string proxyCredentialToken = string.Empty;

        public DbDictionaryService()
        {
            Directory.SetCurrentDirectory(System.AppDomain.CurrentDomain.BaseDirectory);
            _proxyCredentials = new WebProxyCredentials();
            dbDictionaryFullFilePath = WebConfigurationManager.AppSettings["DbDictionaryFullFilePath"];
            proxyCredentialToken = WebConfigurationManager.AppSettings["ProxyCredentialToken"];
            proxyPort = WebConfigurationManager.AppSettings["ProxyPort"];
            proxyHost = WebConfigurationManager.AppSettings["ProxyHost"];
            if (!string.IsNullOrEmpty(proxyHost) || (!string.IsNullOrEmpty(proxyPort)))
            {
                _proxyCredentials.proxyHost = proxyHost;
                _proxyCredentials.proxyPort = Convert.ToInt32(proxyPort);
            }

        }

        public DatabaseDictionary GetDbDictionary(string project, string application)
        {
            DatabaseDictionary dict = new DatabaseDictionary();
            StringBuilder sb = new StringBuilder();
            try
            {
                sb.Append(dbDictionaryFullFilePath);
                sb.Append("DatabaseDictionary.");
                sb.Append(project);
                sb.Append(".");
                sb.Append(application);
                sb.Append(".xml");
                dict = Utility.Read<DatabaseDictionary>(sb.ToString());

                return dict;
            }
            catch (Exception ex)
            {
                _logger.Error("Error in GetDbDictionary: " + ex);
                return null;
            }            
        }

        public Response SaveDatabaseDictionary(string project, string application, DatabaseDictionary dict)
        {
            StringBuilder sb = new StringBuilder();
            Response response = new Response();
            try
            {
                sb.Append(dbDictionaryFullFilePath);
                sb.Append("DatabaseDictionary.");
                sb.Append(project);
                sb.Append(".");
                sb.Append(application);
                sb.Append(".xml");
                Utility.Write<DatabaseDictionary>(dict, sb.ToString());

                response.Add("Database Dictionary saved successfully");
                return response;
            }
            catch (Exception ex)
            {
                _logger.Error("Error in SaveDatabaseDictionary: " + ex);
                response.Add("Error in saving database dictionary" + ex.Message);
                return response;
            }            
        }

        public DatabaseDictionary GetDatabaseSchema(Request request)
        {
            try
            {
                string connString = request["connectionString"];
                string dbProvider = request["dbProvider"];
                dbProvider = dbProvider.ToUpper();
                string parsedConnStr = ParseConnectionString(connString, dbProvider);

                DatabaseDictionary dbDictionary = new DatabaseDictionary();
                Dictionary<string, string> properties = new Dictionary<string, string>();
                string metadataQuery = string.Empty;
                dbDictionary.connectionString = parsedConnStr;
                dbDictionary.dataObjects = new System.Collections.Generic.List<DataObject>();

                properties.Add("connection.provider", "NHibernate.Connection.DriverConnectionProvider");
                properties.Add("proxyfactory.factory_class", "NHibernate.ByteCode.Castle.ProxyFactoryFactory, NHibernate.ByteCode.Castle");
                properties.Add("connection.connection_string", parsedConnStr);

                if (dbProvider.Contains("MSSQL"))
                {
                    metadataQuery =
                        "select t1.table_name, t1.column_name, t1.data_type, t2.max_length, t2.is_identity, t2.is_nullable, t5.constraint_type " +
                        "from information_schema.columns t1 " +
                        "inner join sys.columns t2 on t2.name = t1.column_name " +
                        "inner join sys.tables t3 on t3.name = t1.table_name and t3.object_id = t2.object_id " +
                        "left join information_schema.key_column_usage t4 on t4.table_name = t1.table_name and t4.column_name = t1.column_name " +
                        "left join information_schema.table_constraints t5 on t5.constraint_name = t4.constraint_name " +
                        "order by t1.table_name, t5.constraint_type, t1.column_name";
                    properties.Add("connection.driver_class", "NHibernate.Driver.SqlClientDriver");

                    switch (dbProvider)
                    {
                        case "MSSQL2008":
                            dbDictionary.provider = Provider.MsSql2008;
                            properties.Add("dialect", "NHibernate.Dialect.MsSql2008Dialect");
                            break;

                        case "MSSQL2005":
                            dbDictionary.provider = Provider.MsSql2005;
                            properties.Add("dialect", "NHibernate.Dialect.MsSql2005Dialect");
                            break;

                        case "MSSQL2000":
                            dbDictionary.provider = Provider.MsSql2000;
                            properties.Add("dialect", "NHibernate.Dialect.MsSql2000Dialect");
                            break;

                        default:
                            throw new Exception("Database provider not supported.");
                    }
                }
                else if (dbProvider.Contains("ORACLE"))
                {
                    metadataQuery =
                      "select t1.object_name, t2.column_name, t2.data_type, t2.data_length, 0 as is_sequence, t2.nullable, t4.constraint_type " +
                      "from user_objects t1 " +
                      "inner join all_tab_cols t2 on t2.table_name = t1.object_name " +
                      "left join all_cons_columns t3 on t3.table_name = t2.table_name and t3.column_name = t2.column_name " +
                      "left join all_constraints t4 on t4.constraint_name = t3.constraint_name and (t4.constraint_type = 'P' or t4.constraint_type = 'R') " +
                      "where t1.object_type = 'TABLE' order by t1.object_name, t4.constraint_type, t2.column_name";
                    properties.Add("connection.driver_class", "NHibernate.Driver.OracleClientDriver");

                    switch (dbProvider)
                    {
                        case "ORACLE10G":
                            dbDictionary.provider = Provider.Oracle10g;
                            properties.Add("dialect", "NHibernate.Dialect.Oracle10gDialect");
                            break;

                        case "ORACLE9I":
                            dbDictionary.provider = Provider.Oracle9i;
                            properties.Add("dialect", "NHibernate.Dialect.Oracle9iDialect");
                            break;

                        case "ORACLE8I":
                            dbDictionary.provider = Provider.Oracle8i;
                            properties.Add("dialect", "NHibernate.Dialect.Oracle8iDialect");
                            break;

                        case "ORACLELITE":
                            dbDictionary.provider = Provider.OracleLite;
                            properties.Add("dialect", "NHibernate.Dialect.OracleLiteDialect");
                            break;

                        default:
                            throw new Exception("Database provider not supported.");
                    }
                }
                else if (dbProvider.Contains("MYSQL"))
                {
                    metadataQuery = "SELECT TABLE_NAME, COLUMN_NAME, DATA_TYPE,CHARACTER_MAXIMUM_LENGTH, COLUMN_KEY, IS_NULLABLE " +
                                    "FROM INFORMATION_SCHEMA.COLUMNS " +
                                    string.Format("WHERE TABLE_SCHEMA = '{0}'", connString.Split(';')[1].Split('=')[1]);
                    properties.Add("connection.driver_class", "NHibernate.Driver.MySqlDataDriver");

                    switch (dbProvider)
                    {
                        case "MYSQL3":
                            dbDictionary.provider = Provider.MySql3;
                            properties.Add("dialect", "NHibernate.Dialect.MySQLDialect");
                            break;
                        case "MYSQL4":
                            dbDictionary.provider = Provider.MySql4;
                            properties.Add("dialect", "NHibernate.Dialect.MySQLDialect");
                            break;
                        case "MYSQL5":
                            dbDictionary.provider = Provider.MySql5;
                            properties.Add("dialect", "NHibernate.Dialect.MySQL5Dialect");
                            break;
                    }
                }


                NHibernate.Cfg.Configuration config = new NHibernate.Cfg.Configuration();
                config.AddProperties(properties);

                ISessionFactory sessionFactory = config.BuildSessionFactory();
                ISession session = sessionFactory.OpenSession();
                ISQLQuery query = session.CreateSQLQuery(metadataQuery);
                IList<object[]> metadataList = query.List<object[]>();
                session.Close();

                DataObject table = null;
                string prevTableName = String.Empty;
                foreach (object[] metadata in metadataList)
                {
                    string tableName = Convert.ToString(metadata[0]);
                    string columnName = Convert.ToString(metadata[1]);
                    string dataType = Utility.SqlTypeToCSharpType(Convert.ToString(metadata[2]));
                    int dataLength = Convert.ToInt32(metadata[3]);
                    bool isIdentity = Convert.ToBoolean(metadata[4]);
                    string nullable = Convert.ToString(metadata[5]).ToUpper();
                    bool isNullable = (nullable == "Y" || nullable == "TRUE");
                    string constraint = Convert.ToString(metadata[6]);

                    if (tableName != prevTableName)
                    {
                        table = new DataObject()
                        {
                            tableName = tableName,
                            dataProperties = new List<DataProperty>(),
                            keyProperties = new KeyProperties(),
                            dataRelationships = new List<DataRelationship>(), // to be supported in the future
                            objectName = Utility.NameSafe(tableName)
                        };

                        dbDictionary.dataObjects.Add(table);
                        prevTableName = tableName;
                    }

                    if (String.IsNullOrEmpty(constraint)) // process columns
                    {
                        DataProperty column = new DataProperty()
                          {
                              columnName = columnName,
                              dataType = (DataType)Enum.Parse(typeof(DataType), dataType),
                              // dataType = (DataType)Enum.Parse(typeof(DataType), dataType),
                              dataLength = dataLength,
                              isNullable = isNullable,
                              propertyName = Utility.NameSafe(columnName)
                          };

                        table.dataProperties.Add(column);
                    }
                    else // process keys
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

                        KeyProperty key = new KeyProperty()
                        {
                            columnName = columnName,
                            dataType = (DataType)Enum.Parse(typeof(DataType), dataType),
                            //   dataType = (DataType)Enum.Parse(typeof(DataType), dataType),
                            dataLength = dataLength,
                            isNullable = isNullable,
                            keyType = keyType,
                            propertyName = Utility.NameSafe(columnName),
                        };

                        table.keyProperties.Add(key);
                    }
                }
                return dbDictionary;
            }
            catch (Exception ex)
            {
                _logger.Error("Error in GetDatabaseSchema: " + ex);
                return null;
            }
        }

        static string ParseConnectionString(string connStr, string dbProvider)
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
                throw ex;
            }
        }

        public List<string> GetExistingDbDictionaryFiles()
        {
            List<string> resultFiles = new List<string>();
            try
            {
                DirectoryInfo di = new DirectoryInfo(dbDictionaryFullFilePath);
                FileInfo[] files = di.GetFiles("DatabaseDictionary.*.xml");
                foreach (FileInfo file in files)
                    resultFiles.Add(file.Name);
                
            }
            catch (Exception ex)
            {
                _logger.Error("Error in GetExistingDbDictionaryFiles: " + ex);
            }
            return resultFiles;
        }

        public String[] GetProviders()
        {
            try
            {
                return Enum.GetNames(typeof(Provider));
            }
            catch (Exception ex)
            {
                _logger.Error("Error in GetProviders: " + ex);
                return null;
            }
        }
    }
}
