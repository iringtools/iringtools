using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using org.iringtools.library;
using org.iringtools.adapter;
using org.iringtools.utility;
using System.Data;
using System.Data.SqlClient;
using System.Xml.Linq;
using log4net;

namespace org.iringtools.sdk.sql
{
  public class SQLDataLayer : BaseSQLDataLayer
  {
    private static readonly ILog logger = LogManager.GetLogger(typeof(SQLDataLayer));
    private SqlConnection _conn;
    private DatabaseDictionary _dictionary = null;
    private SqlDataAdapter _adapter = null;
    private SqlCommandBuilder _command = null;

    public SQLDataLayer(AdapterSettings settings) : base(settings) 
    { 
      //Some basic initialization can be done here.
    }

    public override DatabaseDictionary GetDatabaseDictionary()
    {
        _dictionary = Utility.Read<DatabaseDictionary>(@"./../SQLDataLayer/SampleDictionary.xml");
        string connStr = EncryptionUtility.Decrypt(_dictionary.ConnectionString);
        _conn = new SqlConnection(connStr);
      return _dictionary;
    }

    public override DataTable GetDataTable(string tableName, IList<string> identifiers)
    {
        string query = string.Empty;
        DataSet dataSet = new DataSet();
        string delimiter = string.Empty;
        StringBuilder ids = new StringBuilder();
        StringBuilder qry = new StringBuilder();
        string qrySeparator = "";
        string separator = "";
        IList<string> keyProperties = new List<string>();
        DataObject dataObject = _dictionary.dataObjects.Where<DataObject>(p => p.tableName == tableName).FirstOrDefault();
        keyProperties = (from p in dataObject.keyProperties
                         select p.keyPropertyName).ToList<string>();
       string tempQry = string.Empty;
        
        int i = 0;
        if (keyProperties.Count > 1)
        {
            delimiter = dataObject.keyDelimeter;

            foreach (string prop in keyProperties)
            {

                tempQry += prop + " in (";
                foreach (string identifier in identifiers)
                {
                    string[] idArray = null;
                    if (identifier.Contains(delimiter.FirstOrDefault()))
                    {
                        idArray = identifier.Split(delimiter.FirstOrDefault());
                        ids.Append(separator + "'" + idArray[i] + "'");
                        separator = ",";
                    }
                }
                qry.Append(tempQry + ids + ")");
                i++;
                if (i < keyProperties.Count)
                {
                    qrySeparator = " and ";
                }
                else
                    qrySeparator = "";
                tempQry = qry.ToString() + qrySeparator;
                ids.Clear();
                separator = "";
                qry.Clear();
            }
        }
        else 
        {
           StringBuilder idString = new StringBuilder();
           string diff = "";
            foreach(string identifier in identifiers)
            {
                idString.Append(diff + "'" + identifier + "'");
                diff = ",";

            }
            tempQry = keyProperties[0] + " in (" + idString +")";

        }
        try
        {
            query = "SELECT * FROM " + tableName + " where " + tempQry;

            _conn.Open();

            _adapter = new SqlDataAdapter();
            _adapter.SelectCommand = new SqlCommand(query, _conn);

            _command = new SqlCommandBuilder(_adapter);
            _adapter.UpdateCommand = _command.GetUpdateCommand();
                
            _adapter.Fill(dataSet, tableName);
            DataTable dataTable = dataSet.Tables[tableName];
            return dataTable;
        }
        catch (Exception ex)
        {
            logger.Info("Error while retrieving the data:   " + ex.Message);
            throw ex;
        }
        finally
        {
            if (_conn != null && _conn.State == ConnectionState.Open)
            {
                _conn.Close();
            }
        }

    }

    public override DataTable GetDataTable(string tableName, string whereClause, long start, long limit)
    {
        List<string> keys = (from p in _dictionary.dataObjects
                    where p.objectName == tableName
                    select p.keyProperties.FirstOrDefault().keyPropertyName).ToList();

        string key = keys[0];
        string query = string.Empty;

        if (string.IsNullOrEmpty(whereClause))
        {
            query = "select * from (SELECT *, ROW_NUMBER() OVER (order by " + key + ") AS RN FROM " + tableName +
                    ") As " + tableName + " where RN between " + start + " and " + limit;
        }
        else
        {
            query = "select * from (SELECT *, ROW_NUMBER() OVER (order by " + key + ") AS RN FROM " + tableName +
                    ") As " + tableName + whereClause + " and RN between " + start + " and " + limit;
        }

        try
        {
            _conn.Open();

            _adapter = new SqlDataAdapter();
            _adapter.SelectCommand = new SqlCommand(query, _conn);

            _command = new SqlCommandBuilder(_adapter);
            _adapter.UpdateCommand = _command.GetUpdateCommand();
            DataSet dataSet = new DataSet();
            _adapter.Fill(dataSet, tableName);
            return dataSet.Tables[tableName];
        }
        catch (Exception ex)
        {
            logger.Info("Error while retrieving the data:   " + ex.Message);
            throw ex;
        }
        finally
        {
            if (_conn != null && _conn.State == ConnectionState.Open)
            {
                _conn.Close();
            }
        }

    }

    public override Response PostDataTables(IList<DataTable> dataTables)
    {
      string tableName = dataTables.First().TableName;
      string query = "SELECT * FROM " + tableName;
      _adapter = new SqlDataAdapter();
      _adapter.SelectCommand = new SqlCommand(query, _conn);
            
      _command = new SqlCommandBuilder(_adapter);
      _adapter.UpdateCommand = _command.GetUpdateCommand();
      
      DataSet dataSet = new DataSet();
      foreach (DataTable dataTable in dataTables)
      {        
        dataSet.Tables.Add(dataTable);
      }

      _adapter.Update(dataSet, tableName);

      Response response = new Response();
      response.StatusList.Add(
        new Status()
        {
          Level = StatusLevel.Success,
          Messages = new Messages() { "successful" }
        }
      );
         
      return response;
    }

    public override Response DeleteDataTable(string tableName, string whereClause)
    {
        Response response = new Response();
        response.StatusList = new List<Status>();
        Status status = new Status();
        status.Level = StatusLevel.Error;
        status.Identifier = tableName;
        string query = "Delete FROM " + tableName + whereClause;

        try
        {
            _conn.Open();
            SqlCommand command = new SqlCommand(query, _conn);
            int numberDeleted = command.ExecuteNonQuery();
            if (numberDeleted > 0)
            {
                status.Level = StatusLevel.Success;
                response.Append(status);
            }
            return response;
        }
        catch (Exception ex)
        {
            logger.Info("Error while retrieving the data:   " + ex.Message);
            throw ex;
        }
        finally
        {
            if (_conn != null && _conn.State == ConnectionState.Open)
            {
                _conn.Close();
            }
        }
    }

    public override Response DeleteDataTable(string tableName, IList<string> identifiers)
    {
        Response response = new Response();
        response.StatusList = new List<Status>();
        
        string delimiter = string.Empty;
        IList<string> keyProperties = new List<string>();
        DataObject dataObject = _dictionary.dataObjects.Where<DataObject>(p => p.tableName == tableName).FirstOrDefault();
        keyProperties = (from p in dataObject.keyProperties
                         select p.keyPropertyName).ToList<string>();
        if (keyProperties.Count > 1)
        {
            delimiter = dataObject.keyDelimeter;
        }
        
        foreach (string identifier in identifiers)
        {
            int i = 0;
            string[] ids = null;
            string tempQry = string.Empty;
            if (keyProperties.Count > 1 && identifier.Contains(delimiter.FirstOrDefault()))
            {
                ids = identifier.Split(delimiter.FirstOrDefault());

            }
            while (i < keyProperties.Count())
            {
                if (i != 0)
                {
                    tempQry += " and ";
                }

              if (keyProperties.Count > 1)
                  tempQry = tempQry + keyProperties[i] + " = '" + ids[i] +"'";
              else
                  tempQry = tempQry + keyProperties[i] + " = '"+ identifier + "'";
                i++;
            }
            try
            {
                _conn.Open();
                string query = "Delete FROM " + tableName + " where " + tempQry;
                SqlCommand command = new SqlCommand(query, _conn);
                int numberDeleted = command.ExecuteNonQuery();
                if (numberDeleted > 0)
                {
                    Status status = new Status();
                    status.Messages = new Messages();
                    status.Identifier = identifier;
                    status.Messages.Add(string.Format("Record [{0}] have been deleted successfully.", identifier));
                    response.Append(status);
                }

            }
            catch (Exception ex)
            {
                logger.Info("Error while retrieving the data:   " + ex.Message);
                throw ex;
            }
            finally
            {
                if (_conn != null && _conn.State == ConnectionState.Open)
                {
                    _conn.Close();
                }
            }
        }
        return response;
    }

    public override IList<string> GetIdentifiers(string tableName, string whereClause)
    {
        IList<string> identifiers = new List<string>();
        string query = string.Empty;
        string delimiter = string.Empty;
        IList<string> keyProperties = new List<string>();
        DataObject dataObject = _dictionary.dataObjects.Where<DataObject>(p => p.tableName == tableName).FirstOrDefault();
        keyProperties = (from p in dataObject.keyProperties
                         select p.keyPropertyName).ToList<string>();

        StringBuilder keys = new StringBuilder();
        string separator = "";
        foreach (string prop in keyProperties)
        {
            keys.Append(separator + prop);
            separator = ",";
        }

        if (string.IsNullOrEmpty(whereClause))
        {
            query = "select " + keys + " from " + tableName;
        }
        else
        {
            query = "select " + keys + " from " + tableName + whereClause;
        }

        try
        {
            _conn.Open();

            _adapter = new SqlDataAdapter();
            SqlCommand sqlCmd = new SqlCommand();
            sqlCmd.CommandText = query;
            sqlCmd.Connection = _conn;
            _adapter = new SqlDataAdapter(sqlCmd);
            DataSet dataSet = new DataSet();
            _adapter.Fill(dataSet, tableName);

            DataTable dataTable = dataSet.Tables[tableName];
            if (keyProperties.Count > 1)
            {
                delimiter = dataObject.keyDelimeter;
            }

            foreach (DataRow row in dataTable.Rows)
            {
                if (keyProperties.Count > 1)
                {
                    identifiers.Add(Convert.ToString(row[0]) + delimiter + Convert.ToString(row[1]));
                }
                else
                    identifiers.Add(Convert.ToString(row[0]));
            }
            return identifiers;
        }
        catch (Exception ex)
        {
            logger.Info("Error while retrieving the data:   " + ex.Message);
            throw ex;
        }
        finally
        {
            if (_conn != null && _conn.State == ConnectionState.Open)
            {
                _conn.Close();
            }
        }
    }

    public override DataTable CreateDataTable(string tableName, IList<string> identifiers)
    {
        DataTable dataTable = new DataTable();
        DataTable dTable = new DataTable(tableName);
        string query = string.Empty;
        string delimiter = string.Empty;
        string[] idArray = null;
        DataObject dataObject = _dictionary.dataObjects.Where<DataObject>(p => p.tableName == tableName).FirstOrDefault();
        IList<string> keyProperties = keyProperties = (from p in dataObject.keyProperties
                         select p.keyPropertyName).ToList<string>();
        if (keyProperties.Count > 1)
        {
            delimiter = dataObject.keyDelimeter;
        }
        IList<string> idList = new List<string>();
        if (identifiers != null)
        {
            
            int j = 0;
            foreach (string identifier in identifiers)
            {
                idList.Add(identifier);
                dataTable = GetDataTable(tableName, idList);
                idList.Clear();
                if (dataTable != null && dataTable.Rows != null)
                {
                    dTable = dataTable.Clone();
                    if (dataTable.Rows.Count > 0)
                        dTable.Rows.Add(dataTable.Rows[j].ItemArray);
                    else
                    {
                        DataRow drow = null;

                        for (int i = 0; i < keyProperties.Count; i++)
                        {
                            if (identifier.Contains(delimiter.FirstOrDefault()))
                            {
                                idArray = identifier.Split(delimiter.FirstOrDefault());
                            }
                            else if (keyProperties.Count == 1)
                            {
                                idArray = identifier.Split();
                            }
                        }
                        drow = dTable.NewRow();
                        dTable.Rows.Add(drow);
                        for (int i = 0; i < keyProperties.Count; i++)
                        {
                            drow[keyProperties[i]] = idArray[i];
                            drow.AcceptChanges();
                        }
                    }
                }
                j++;
            }

        }
        return dTable;
    }

    public override DataTable GetRelatedDataTable(DataRow dataRow, string relatedTableName)
    {
        DataObject dataObject = (from p in _dictionary.dataObjects
                                 where p.tableName == dataRow.Table.TableName
                                 select p).FirstOrDefault();

        DataObject relatedDataObject = (from p in _dictionary.dataObjects
                                 where p.tableName == relatedTableName
                                 select p).FirstOrDefault(); 

        string relationshipType = (from p in dataObject.dataRelationships
                                    where p.relatedObjectName == relatedDataObject.objectName
                                    select p.relationshipType.ToString()).FirstOrDefault();

        IList<string> dataPropertyNames = (from p in dataObject.dataRelationships
                                         where p.relatedObjectName == relatedDataObject.objectName
                                              select p.propertyMaps.FirstOrDefault().dataPropertyName).ToList<string>();

        IList<string> relatedPropertyNames = (from p in dataObject.dataRelationships
                                         where p.relatedObjectName == relatedDataObject.objectName
                                              select p.propertyMaps.FirstOrDefault().relatedPropertyName).ToList<string>();
        string query = string.Empty;
        string tempqry= string.Empty;
        string qrySeparator = "";
        for (int i = 0; i < relatedPropertyNames.Count; i++)
        {

            if (tempqry.Length > 0)
                qrySeparator = " and ";

            tempqry = qrySeparator +  relatedPropertyNames[i] + " = '" + dataRow[dataPropertyNames[i]] + "'";
        }

        try
        {
            if (relationshipType.ToUpper() == "ONETOONE")
                query = "select Top 1 * from " + relatedTableName + " where " + tempqry;
            else
                query = "select * from " + relatedTableName + " where " + tempqry;

            _conn.Open();

            _adapter = new SqlDataAdapter();
            _adapter.SelectCommand = new SqlCommand(query, _conn);

            _command = new SqlCommandBuilder(_adapter);
            _adapter.UpdateCommand = _command.GetUpdateCommand();
            DataSet dataSet = new DataSet();
            _adapter.Fill(dataSet, relatedTableName);
            DataTable dataTable = dataSet.Tables[relatedTableName];
            return dataTable;
        }
        catch (Exception ex)
        {
            logger.Info("Error while retrieving the data:   " + ex.Message);
            throw ex;
        }
        finally
        {
            if (_conn != null && _conn.State == ConnectionState.Open)
            {
                _conn.Close();
            }
        }       
    }

    public override long GetCount(string tableName, string whereClause)
    {
        long count = 0;
        string query = string.Empty;
        if (string.IsNullOrEmpty(whereClause))
            query = "select count(*) from " + tableName + whereClause;
        else
            query = "select count(*) from" + tableName;
        try
        {
            _conn.Open();
            _adapter = new SqlDataAdapter();
            _adapter.SelectCommand = new SqlCommand(query, _conn);

            SqlCommand sqlCmd = new SqlCommand();
            sqlCmd.CommandText = query;
            sqlCmd.Connection = _conn;
            _adapter = new SqlDataAdapter(sqlCmd);
            DataSet dataSet = new DataSet(); 
            _adapter.Fill(dataSet, tableName);
            DataTable dataTable = dataSet.Tables[tableName];
            if (dataTable != null && dataTable.Rows != null && dataTable.Rows.Count > 0)
            {
                count = Convert.ToInt64(dataTable.Rows[0][0]);
            }
                 
        }
        catch (Exception ex)
        {
            logger.Info("Error while retrieving the data:   " + ex.Message);
            throw ex;
        }
        finally
        {
            if (_conn != null && _conn.State == ConnectionState.Open)
            {
                _conn.Close();
            }
        }
        return count;
    }



    public override Response Configure(XElement configuration)
    {
      throw new NotImplementedException();
    }



    public override XElement GetConfiguration()
    {
      throw new NotImplementedException();
    }



    public override Response RefreshDataTable(string tableName)
    {
      throw new NotImplementedException();
    }
  }
}
