using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using org.iringtools.web.controllers;
using org.iringtools.adapter.security;
using System.Collections;
using log4net;
using iRINGTools.Web.Helpers;
using org.iringtools.library;
using org.iringtools.web.Models;

namespace org.iringtools.web.Controllers
{
  public class NHibernateController : BaseController
  {
    private static readonly ILog _logger = LogManager.GetLogger(typeof(BaseController));
    private NHibernateRepository _repository;
    private string _keyFormat = "Datadictionary.{0}.{1}";

    public NHibernateController() : this(new NHibernateRepository()) { }

    public NHibernateController(NHibernateRepository repository)
        : base()
    {
      _repository = repository;
    }

    public ActionResult Index()
    {
      return View();
    } 

    private DatabaseDictionary GetDbDictionary(string contextName, string endpoint, string baseUrl)
    {
      string key = adapter_PREFIX + string.Format(_keyFormat, contextName, endpoint, baseUrl);
      DatabaseDictionary databaseDictionary = null;
      bool getDbDict = false;

      if (Session[key] != null)
      {
        databaseDictionary = (DatabaseDictionary)Session[key];
        if (databaseDictionary.ConnectionString == null || databaseDictionary.dataObjects.Count == 0)
          getDbDict = true;          
      }
      else
        getDbDict = true;       
     
      if (getDbDict)
      {
        databaseDictionary = _repository.GetDBDictionary(contextName, endpoint, baseUrl);
      }

      Session[key] = databaseDictionary;

      return (DatabaseDictionary)Session[key];
    }       

    public ActionResult DBObjects(FormCollection form)
    {
      try
      {
        DatabaseDictionary databaseDictionary = null;
        Tree dbObjects = null;

        if (form["contextName"] != null)
        {
          databaseDictionary = GetDbDictionary(form["contextName"], form["endpoint"], form["baseUrl"]);

          if (form["tableNames"] != null)
          {
            dbObjects = _repository.GetDBObjects(
              form["contextName"], form["endpoint"], form["dbProvider"], form["dbServer"], form["dbInstance"],
              form["dbName"], form["dbSchema"], form["dbUserName"], form["dbPassword"], form["tableNames"], form["portNumber"],
              form["serName"], form["baseUrl"], databaseDictionary);
            return Json(dbObjects.getNodes(), JsonRequestBehavior.AllowGet);
          }
        }

        return null;
      }
      catch (Exception e)
      {
        _logger.Error(e.ToString());
        throw e;
      }
    }

    public ActionResult Trees(FormCollection form)
    {
      try
      {
        string response = string.Empty;

        response = _repository.SaveDBDictionary(form["scope"], form["app"], form["tree"], form["baseUrl"]);

        if (response != null)
        {
          response = response.ToLower();
          if (response.Contains("error"))
          {
            int inds = response.IndexOf("<message>");
            int inde = response.IndexOf("</message>");
            string msg = response.Substring(inds + 9, inde - inds - 9);
            return Json(new { success = false } + msg, JsonRequestBehavior.AllowGet);
          }
        }
        return Json(new { success = true }, JsonRequestBehavior.AllowGet);
      }
      catch (Exception e)
      {
        _logger.Error(e.ToString());
        throw e;
      }
    }

    public ActionResult DataType()
    {
      try
      {
        Dictionary<String, String> dataTypeNames = new Dictionary<String, String>();

        foreach (DataType dataType in Enum.GetValues(typeof(DataType)))
        {
          dataTypeNames.Add(((int)dataType).ToString(), dataType.ToString());
        }

        return Json(dataTypeNames, JsonRequestBehavior.AllowGet);
      }
      catch (Exception e)
      {
        _logger.Error(e.ToString());
        throw e;
      }
    }        

    public ActionResult DBProviders(FormCollection form)
    {
      JsonContainer<List<DBProvider>> container = new JsonContainer<List<DBProvider>>();

      try
      {
        string baseUrl = form["baseUrl"];
        DataProviders dataProviders = _repository.GetDBProviders(baseUrl);                

        List<DBProvider> providers = new List<DBProvider>();
        foreach (Provider dataProvider in dataProviders.Distinct().ToList())
        {                   
          providers.Add(new DBProvider() { Provider = dataProvider.ToString() });                   
        }

        container.items = providers;
        container.success = true;
        container.total = providers.Count;
      }
      catch (Exception e)
      {
        _logger.Error(e.ToString());
        throw e;
      }

      return Json(container, JsonRequestBehavior.AllowGet);
    }

    public ActionResult DBDictionary(FormCollection form)
    {
      try
      {
        DatabaseDictionary dbDict = GetDbDictionary(form["scope"], form["app"], form["baseUrl"]);
        return Json(dbDict, JsonRequestBehavior.AllowGet);
      }
      catch (Exception e)
      {
        _logger.Error(e.ToString());
        throw e;
      }
    }

    public ActionResult TableNames(FormCollection form)
    {
      JsonContainer<List<string>> container = new JsonContainer<List<string>>();

      try
      {
        List<string> dataObjects = _repository.GetTableNames(
          form["scope"], form["app"], form["dbProvider"], form["dbServer"], form["dbInstance"],
          form["dbName"], form["dbSchema"], form["dbUserName"], form["dbPassword"], form["portNumber"], form["serName"],
          form["baseUrl"]);

        container.items = dataObjects;
        container.success = true;
        container.total = dataObjects.Count;
      }
      catch (Exception e)
      {
        _logger.Error(e.ToString());
        throw e;
      }

      return Json(container, JsonRequestBehavior.AllowGet);
    }

    private string GetKeytype(string name, List<DataProperty> properties)
    {
      string keyType = string.Empty;
      keyType = properties.FirstOrDefault(p => p.propertyName == name).keyType.ToString();
      return keyType;
    }
    private string GetDatatype(string name, List<DataProperty> properties)
    {
      string dataType = string.Empty;
      dataType = properties.FirstOrDefault(p => p.propertyName == name).dataType.ToString();
      return dataType;
    }

    private void AddContextEndpointtoNode(JsonTreeNode node, FormCollection form)
    {
      if (form["contextName"] != null)
        node.property.Add("context", form["contextName"]);
      if (form["endpoint"] != null)
        node.property.Add("endpoint", form["endpoint"]);
    }
  }

  public class DBProvider
  {
    public string Provider { get; set; }
  }
}
