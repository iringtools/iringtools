using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Collections.ObjectModel;
using org.iringtools.library;
using System.Web.Configuration;
using org.iringtools.utility;
using NHibernate;

namespace DbDictionaryEditor.Web
{
    public class DbDictionaryService : IDbDictionaryService
    {
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
            adapterServiceUri = WebConfigurationManager.AppSettings["AdapterServiceUri"];
            dbDictionaryFullFilePath = WebConfigurationManager.AppSettings["DbDictionaryFullFilePath"];
            proxyCredentialToken = WebConfigurationManager.AppSettings["ProxyCredentialToken"];
            proxyPort = WebConfigurationManager.AppSettings["ProxyPort"];
            proxyHost = WebConfigurationManager.AppSettings["ProxyHost"];
            if(!string.IsNullOrEmpty(proxyHost) || (!string.IsNullOrEmpty(proxyPort)))
            {
                _proxyCredentials.proxyHost = proxyHost;
                _proxyCredentials.proxyPort = Convert.ToInt32(proxyPort);
            }

        }

        public Collection<ScopeProject> GetScopes()
        {
            
            Collection<ScopeProject> scopes = new Collection<ScopeProject>();
            string relativeUri = "/scopes";
            WebCredentials cred = new WebCredentials();
            try
            {
                WebHttpClient webHttpClient = new WebHttpClient(adapterServiceUri, cred.GetNetworkCredential(), _proxyCredentials.GetWebProxy());
                scopes = webHttpClient.Get<Collection<ScopeProject>>(relativeUri, true);
            }
            catch (Exception ex)
            { }
            return scopes;
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
            }
            catch (Exception ex)
            { 
            }
            return dict;
        }

        public void SaveDabaseDictionary(DatabaseDictionary dict, string project, string application)
        {
            StringBuilder sb = new StringBuilder();
            try
            {
                sb.Append(dbDictionaryFullFilePath);
                sb.Append("DatabaseDictionary.");
                sb.Append(project);
                sb.Append(".");
                sb.Append(application);
                sb.Append(".xml");
                Utility.Write<DatabaseDictionary>(dict, sb.ToString());
            }
            catch (Exception ex)
            {
            }
        }

        public DatabaseDictionary GetDatabaseSchema(string connString, string dbProvider)
        {
            dbProvider = dbProvider.ToUpper();
            string parsedConnStr = ParseConnectionString(connString, dbProvider);

            DatabaseDictionary dbDictionary = new DatabaseDictionary();
            Dictionary<string, string> properties = new Dictionary<string, string>();
            string metadataQuery = string.Empty;
            dbDictionary.connectionString = parsedConnStr;
            dbDictionary.tables = new System.Collections.Generic.List<Table>();

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
                                string.Format("WHERE TABLE_SCHEMA = '{0}'",connString.Split(';')[1].Split('=')[1]);
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

            Table table = null;
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
                    table = new Table()
                    {
                        tableName = tableName,
                        columns = new List<Column>(),
                        keys = new List<Key>(),
                        associations = new List<Association>(), // to be supported in the future
                        entityName = Utility.NameSafe(tableName)
                    };

                    dbDictionary.tables.Add(table);
                    prevTableName = tableName;
                }

                if (String.IsNullOrEmpty(constraint)) // process columns
                {
                    Column column = new Column()
                    {
                        columnName = columnName,
                        columnType = (ColumnType)Enum.Parse(typeof(ColumnType),dataType),
                       // dataType = (DataType)Enum.Parse(typeof(DataType), dataType),
                        dataLength = dataLength,
                        isNullable = isNullable,
                        propertyName = Utility.NameSafe(columnName)
                    };

                    table.columns.Add(column);
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

                    Key key = new Key()
                    {
                        columnName = columnName,
                        columnType = (ColumnType)Enum.Parse(typeof(ColumnType), dataType),
                        //   dataType = (DataType)Enum.Parse(typeof(DataType), dataType),
                        dataLength = dataLength,
                        isNullable = isNullable,
                        keyType = keyType,
                        propertyName = Utility.NameSafe(columnName),
                    };

                    table.keys.Add(key);
                }
            }
            return dbDictionary;
        }


        static string ParseConnectionString(string connStr, string dbProvider)
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

        public List<string> GetExistingDbDictionaryFiles()
        {
            List<string> resultFiles = new List<string>();
            string filePath = WebConfigurationManager.AppSettings["DbDictionaryFullFilePath"];
            DirectoryInfo di = new DirectoryInfo(filePath);
            FileInfo[] files = di.GetFiles("DatabaseDictionary.*.xml");
            foreach (FileInfo file in files)
                resultFiles.Add(file.Name);
            return resultFiles;
        }

        public String[] GetProviders()
        {
            return Enum.GetNames(typeof(Provider));
        }

        public Response PostDictionaryToAdapterService(string projectName, string applicationName)
        {
            StringBuilder relativeUri = new StringBuilder();
            string filePath = WebConfigurationManager.AppSettings["DbDictionaryFullFilePath"];
            StringBuilder filename = new StringBuilder();
            filename.Append(filePath);
            filename.Append("DatabaseDictionary.");
            filename.Append(projectName);
            filename.Append(".");
            filename.Append(applicationName);
            filename.Append(".xml");

            relativeUri.Append("/");
            relativeUri.Append(projectName);
            relativeUri.Append("/");
            relativeUri.Append(applicationName);
            relativeUri.Append("/dbdictionary");

            DatabaseDictionary dbDictionary = Utility.Read<DatabaseDictionary>(filename.ToString());
            WebHttpClient httpClient = new WebHttpClient(adapterServiceUri, null);
            Response response = httpClient.Post<DatabaseDictionary, Response>(relativeUri.ToString(), dbDictionary, true);

            return response;
        }

        public Response ClearTripleStore(string projectName, string applicationName)
        {
            StringBuilder relativeUri = new StringBuilder();
            relativeUri.Append("/");
            relativeUri.Append(projectName);
            relativeUri.Append("/");
            relativeUri.Append(applicationName);
            relativeUri.Append("/clear");
            WebHttpClient httpClient = new WebHttpClient(adapterServiceUri,null);
            Response response = httpClient.Get<Response>(relativeUri.ToString());
            return response;
        }

        public Response DeleteApp(string projectName, string applicationName)
        {
            StringBuilder relativeUri = new StringBuilder();
            relativeUri.Append("/");
            relativeUri.Append(projectName);
            relativeUri.Append("/");
            relativeUri.Append(applicationName);
            relativeUri.Append("/delete");
            WebHttpClient httpClient = new WebHttpClient(adapterServiceUri, null);
            Response response = httpClient.Get<Response>(relativeUri.ToString());
            return response;
        }
    }
}
