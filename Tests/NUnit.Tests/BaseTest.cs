using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using org.iringtools.utility;
using System.Xml.Linq;

namespace NUnit.Tests
{
  public abstract class BaseTest
  {
    public void ResetDatabase()
    {
      ResetDatabase(
        @"..\..\iRINGTools.Services\App_Data\ABC.Data.Complete.sql",
        @".\App_Data\nh-configuration.12345_000.ABC.xml");

      ResetDatabase(
        @"..\..\iRINGTools.Services\App_Data\DEF.Data.Small.sql",
        @".\App_Data\nh-configuration.12345_000.DEF.xml");
    }

    private void ResetDatabase(string sqlScript, string nhConfigPath)
    {
      try
      {
        string sql = Utility.ReadString(sqlScript);
        XDocument nhConfig = XDocument.Load(nhConfigPath);

        var properties = nhConfig
          .Element("configuration")
          .Element("{urn:nhibernate-configuration-2.2}hibernate-configuration")
          .Element("{urn:nhibernate-configuration-2.2}session-factory")
          .Descendants("{urn:nhibernate-configuration-2.2}property");

        var property = from p in properties
                       where p.Attribute("name").Value == "connection.connection_string"
                       select p;

        string connectionString = property.FirstOrDefault().Value;

        if (!connectionString.ToUpper().Contains("DATA SOURCE"))
        {
          connectionString = EncryptionUtility.Decrypt(connectionString);
        }

        Utility.ExecuteSQL(sql, connectionString);
      }
      catch (Exception ex)
      {
        string message = "Error cleaning up Database: " + ex;
        throw new Exception(message);
      }
    }
  }
}
