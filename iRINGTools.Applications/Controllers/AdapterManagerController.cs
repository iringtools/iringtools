using System.Collections.Generic;
using System.Web.Mvc;
using iRINGTools.Web.Models;
using org.iringtools.library;
using org.iringtools.adapter.security;
using iRINGTools.Web.Helpers;
using System;
using System.Web;
using System.IO;
using log4net;
using System.Configuration;
using System.Collections;
using org.iringtools.utility;

namespace org.iringtools.web.controllers
{


  public class AdapterManagerController : BaseController
  {
    private AdapterRepository _repository;
    private static readonly ILog _logger = LogManager.GetLogger(typeof(AdapterManagerController));

    public AdapterManagerController() : this(new AdapterRepository()) { }

    public AdapterManagerController(AdapterRepository repository)
      : base()
    {
      _repository = repository;
    }

    public ActionResult Index()
    {
      return View();
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

    public class DBProvider
    {
      public string Provider { get; set; }
    }
  }
}
