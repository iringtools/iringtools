using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using org.iringtools.library;
using org.iringtools.adapter;
using org.iringtools.utility;
using System.Data;
using System.Data.SqlClient;

namespace org.iringtools.sdk.sql
{
  public class SampleSQLDataLayer : BaseSQLDataLayer
  {
    private SqlConnection _conn;

    public SampleSQLDataLayer(AdapterSettings settings) : base(settings) 
    {
      string connStr = "server=.\\SQLEXPRESS;database=ABC;user id=abc;password=abc";
      _conn = new SqlConnection(connStr);      
    }

    public override DatabaseDictionary GetDatabaseDictionary()
    {
      return Utility.Read<DatabaseDictionary>("../../SampleDictionary.xml");
    }

    public override System.Data.DataTable GetDataTable(string tableName, IList<string> identifiers)
    {
      string query = "SELECT * FROM " + tableName;

      SqlDataAdapter adapter = new SqlDataAdapter();
      adapter.SelectCommand = new SqlCommand(query, _conn);

      SqlCommandBuilder command = new SqlCommandBuilder(adapter);

      DataSet dataSet = new DataSet();
      adapter.Fill(dataSet, tableName);

      return dataSet.Tables[tableName];
    }

    public override Response PostDataTables(IList<System.Data.DataTable> dataTables)
    {
      string tableName = dataTables.First().TableName;
      string query = "SELECT * FROM " + tableName;
            
      SqlDataAdapter adapter = new SqlDataAdapter();
      adapter.SelectCommand = new SqlCommand(query, _conn);
            
      SqlCommandBuilder command = new SqlCommandBuilder(adapter);
      adapter.UpdateCommand = command.GetUpdateCommand();
      
      DataSet dataSet = new DataSet();
      foreach (DataTable dataTable in dataTables)
      {        
        dataSet.Tables.Add(dataTable);
      }

      adapter.Update(dataSet, tableName);

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

    public override Response Configure(System.Xml.Linq.XElement configuration)
    {
      throw new NotImplementedException();
    }

    public override System.Data.DataTable CreateDataTable(string tableName, IList<string> identifiers)
    {
      throw new NotImplementedException();
    }

    public override Response DeleteDataTable(string tableName, IList<string> identifiers)
    {
      throw new NotImplementedException();
    }

    public override Response DeleteDataTable(string tableName, string whereClause)
    {
      throw new NotImplementedException();
    }

    public override System.Xml.Linq.XElement GetConfiguration()
    {
      throw new NotImplementedException();
    }

    public override long GetCount(string tableName, string whereClause)
    {
      throw new NotImplementedException();
    }

    public override System.Data.DataTable GetDataTable(string tableName, string whereClause, long start, long limit)
    {
      throw new NotImplementedException();
    }

    public override IList<string> GetIdentifiers(string tableName, string whereClause)
    {
      throw new NotImplementedException();
    }

    public override System.Data.DataTable GetRelatedDataTable(System.Data.DataRow dataRow, string relatedTableName)
    {
      throw new NotImplementedException();
    }

    public override Response RefreshDataTable(string tableName)
    {
      throw new NotImplementedException();
    }
  }
}
