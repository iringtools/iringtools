using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using log4net;
using NHibernate;
using NHibernate.Cfg;
using org.iringtools.utility;
using Microsoft.SqlServer.Management.Smo.Wmi;

namespace org.iringtools.nhibernate
{
  public sealed class NHibernateSessionManager
  {
    private static readonly ILog _logger = LogManager.GetLogger(typeof(NHibernateSessionManager));
    private static volatile NHibernateSessionManager _instance;
    private static object _lockObj = new Object();
    private static volatile Dictionary<string, ISessionFactory> _sessionFactories;
    
    private NHibernateSessionManager() {}

    public static NHibernateSessionManager Instance
    {
      get
      {
        if (_instance == null)
        {
          lock (_lockObj)
          {
            if (_instance == null)
            {
              _instance = new NHibernateSessionManager();
              _sessionFactories = new Dictionary<string, ISessionFactory>();
            }
          }
        }

        return _instance;
      }
    }

    public ISession GetSession(string path, string context)
    {
      try
      {
        string factoryKey = context.ToLower();

        if (!_sessionFactories.ContainsKey(factoryKey))
        {
          InitSessionFactory(path, context);
        }

        return _sessionFactories[factoryKey].OpenSession();
      }
      catch (Exception e)
      {
        _logger.Error("Unable to obtain session for [" + context + "]. " + e);
        throw e;
      }
    }

    private string getProcessedConnectionString(string connStr)
    {
      string mssqlInstanceName = "";
      ManagedComputer mc = new ManagedComputer();
      if (mc.ServerInstances.Count == 1)
        mssqlInstanceName = mc.ServerInstances[0].Name;

      if (mssqlInstanceName == "")
        mssqlInstanceName = "SQLEXPRESS";

      string[] parts = connStr.Split(';');
      parts[0] = parts[0] + mssqlInstanceName;

      return parts[0] + ";" + parts[1] + ";" + parts[2] + ";" + parts[3];
    }

    private void InitSessionFactory(string path, string context)
    {
      try
      {
        string cfgPath = string.Format("{0}nh-configuration.{1}.xml", path, context);
        string mappingPath = string.Format("{0}nh-mapping.{1}.xml", path, context);
        string connStr = "";

        if (File.Exists(cfgPath) && File.Exists(mappingPath))
        {
          Configuration cfg = new Configuration();
          cfg.Configure(cfgPath);

          string connStrProp = "connection.connection_string";
          string dialectPro = "dialect";
          ISessionFactory sessionFactory = null;
          connStr = cfg.Properties[connStrProp];

          if (connStr.ToUpper().Contains("DATA SOURCE"))
          {
            // connection string is not encrypted, encrypt and write it back
            string encryptedConnStr = EncryptionUtility.Encrypt(connStr);
            cfg.Properties[connStrProp] = encryptedConnStr;
            SaveConfiguration(cfg, cfgPath);

            // restore plain text connection string for creating session factory
            cfg.Properties[connStrProp] = connStr;
          }
          else
          {
            cfg.Properties[connStrProp] = EncryptionUtility.Decrypt(connStr);
          }

          Configuration ctfConfiguration = cfg.AddFile(mappingPath);

          try
          {
            sessionFactory = ctfConfiguration.BuildSessionFactory();
          }
          catch (Exception e)
          {
            if (cfg.Properties[dialectPro].ToLower().Contains("mssql"))
            {
              cfg.Properties[connStrProp] = getProcessedConnectionString(connStr);
              sessionFactory = ctfConfiguration.BuildSessionFactory();
            }
            else
            {
              _logger.Error(string.Format("Error get NH session: {0}", e));
              throw e;
            }
          }

          string factoryKey = context.ToLower();
          _sessionFactories[factoryKey] = sessionFactory;
        }
      }
      catch (Exception e)
      {
        _logger.Error("Error updating NHibernate session factory [" + context + "]. " + e);
        throw e;
      }
    }

    private void SaveConfiguration(Configuration cfg, string path)
    {
      try
      {
        XmlTextWriter writer = new XmlTextWriter(path, Encoding.UTF8);
        writer.Formatting = Formatting.Indented;

        writer.WriteStartElement("configuration");
        writer.WriteStartElement("hibernate-configuration", "urn:nhibernate-configuration-2.2");
        writer.WriteStartElement("session-factory");

        if (cfg.Properties != null)
        {
          foreach (var property in cfg.Properties)
          {
            if (property.Key != "use_reflection_optimizer")
            {
              writer.WriteStartElement("property");
              writer.WriteAttributeString("name", property.Key);
              writer.WriteString(property.Value);
              writer.WriteEndElement();
            }
          }
        }

        writer.WriteEndElement(); // end session-factory
        writer.WriteEndElement(); // end hibernate-configuration
        writer.WriteEndElement(); // end configuration

        writer.Close();
      }
      catch (Exception e)
      {
        _logger.Error("Error saving NHibernate configuration. " + e);
        throw e;
      }
    }
  }
}