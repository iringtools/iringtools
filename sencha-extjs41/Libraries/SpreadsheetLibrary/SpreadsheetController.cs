using System;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net;
using System.Configuration;
using System.Collections.Specialized;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

using org.iringtools.library;
using org.iringtools.utility;
using org.iringtools.mapping;
using DocumentFormat.OpenXml.Packaging;
using log4net;

namespace org.iringtools.adapter.datalayer
{

  public class JsonTreeNode
  {
    public string id { get; set; }
    public string text { get; set; }
    public string icon { get; set; }
    public bool leaf { get; set; }
    public bool expanded { get; set; }
    public List<JsonTreeNode> children { get; set; }
    public string type { get; set; }
    public string nodeType { get; set; }
    public object @checked { get; set; }
    public object record { get; set; }
  }

  public class JsonContainer<T>
  {
    public T items { get; set; }
    public string message { get; set; }
    public Boolean success { get; set; }
    public int total { get; set; }
    public string errors { get; set; }
  }

  public class SpreadsheetController : Controller
  {

    private ServiceSettings _settings = null;
    private ISpreadsheetRepository _repository { get; set; }
    private string _keyFormat = "adpmgr-Configuration.{0}.{1}";
    private string _appData = string.Empty;    
    private static readonly ILog _logger = LogManager.GetLogger(typeof(SpreadsheetController));
    private string _context = string.Empty;
    private string _endpoint = string.Empty;
    private string _baseUrl = string.Empty;    

    public SpreadsheetController()
      : this(new SpreadsheetRepository())
    {
    }

    public SpreadsheetController(ISpreadsheetRepository repository)
    {
      NameValueCollection settings = ConfigurationManager.AppSettings;
      _settings = new ServiceSettings();
      _settings.AppendSettings(settings);
      _repository = repository;
    }

    //
    // GET: /Excel/

    public ActionResult Index()
    {
      return View();
    }

    public JsonResult Upload(FormCollection form)
    {
      try
      {
        SetContextEndpoint(form);
        string datalayer = form["DataLayer"];
        string savedFileName = string.Empty;

        HttpFileCollectionBase files = Request.Files;

        foreach (string file in files)
        {
          HttpPostedFileBase hpf = files[file] as HttpPostedFileBase;
          if (hpf.ContentLength == 0)
            continue;
          string fileLocation = string.Format(@"{0}SpreadsheetData.{1}.{2}.xlsx",_settings["AppDataPath"], _context, _endpoint);

          SpreadsheetConfiguration configuration = new SpreadsheetConfiguration()
          {
            Location = fileLocation
          };

          if (form["Generate"] != null)
          {
            configuration = _repository.ProcessConfiguration(configuration, hpf.InputStream, _baseUrl);
            hpf.InputStream.Flush();
            hpf.InputStream.Position = 0;
            _repository.Configure(_context, _endpoint, datalayer, configuration, hpf.InputStream, _baseUrl);
          }
          else
          {
            configuration.Generate = false;
            configuration = _repository.ProcessConfiguration(configuration, hpf.InputStream, _baseUrl);
          }

          SetConfiguration(_context, _endpoint, configuration, _baseUrl);

          //break;
        }
      }
      catch (Exception ex)
      {
        ;
        return new JsonResult()
        {
          ContentType = "text/html",
          Data = PrepareErrorResponse(ex)
        };
      }
      return new JsonResult()
        {
          ContentType = "text/html",
          Data = new { success = true }
        };
    }

    public ActionResult Export(string context, string endpoint, string baseurl)
    {
      try
      {
        byte[] bytes = _repository.getExcelFile(context, endpoint, baseurl);
       // return File(bytes, "application/vnd.ms-excel---.xls", string.Format("SpreadsheetData.{0}.{1}.xlsx", context, endpoint));
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", string.Format("SpreadsheetData.{0}.{1}.xlsx", context, endpoint));
      }
      catch (Exception ioEx)
      {
        _logger.Error(ioEx.Message);
        throw ioEx;
      }
    }    

    public ActionResult UpdateConfiguration(FormCollection form)
    {
      SetContextEndpoint(form);
      SpreadsheetConfiguration configuration = GetConfiguration(_context, _endpoint, _baseUrl);
      if (configuration != null)
      {
        foreach (SpreadsheetTable workSheet in configuration.Tables)
        {
          if (workSheet.Name == form["Name"])
            workSheet.Label = form["Label"];
          if (workSheet.Columns != null)
          {
            foreach (SpreadsheetColumn column in workSheet.Columns)
            {
              if (column.Name == form["Name"])
                column.Label = form["Label"];
            }
          }
        }
        _repository.Configure(_context, _endpoint, form["datalayer"], configuration, null, _baseUrl);
        return Json(new { success = true }, JsonRequestBehavior.AllowGet);
      }
      else
      {
        return Json(new { success = false }, JsonRequestBehavior.AllowGet);
      }

    }

    public ActionResult GetNode(FormCollection form)
    {
      List<JsonTreeNode> nodes = new List<JsonTreeNode>();

      if (_repository != null)
      {
        SpreadsheetConfiguration configuration = GetConfiguration(form["context"], form["endpoint"], form["baseurl"]);

        if (configuration != null)
        {

          switch (form["type"])
          {
            case "ExcelWorkbookNode":
              {
                List<SpreadsheetTable> worksheets = configuration.Tables;

                if (worksheets != null)
                {
                  foreach (SpreadsheetTable worksheet in worksheets)
                  {
                    List<JsonTreeNode> columnNodes = new List<JsonTreeNode>();
                    JsonTreeNode keyIdentifierNode = new JsonTreeNode()
                    {
                      text = "Identifier",
                      type = "Identifier",
                      expanded = true,
                      leaf = false,
                      children = new List<JsonTreeNode>()
                    };

                    JsonTreeNode dataPropertiesNode = new JsonTreeNode()
                    {
                      text = "Columns",
                      type = "columns",
                      expanded = true,
                      leaf = false,
                      children = new List<JsonTreeNode>()
                    };

                    JsonTreeNode dataObjectNode = new JsonTreeNode()
                    {
                      nodeType = "async",
                      type = "ExcelWorksheetNode",
                      icon = "Content/img/excelworksheet.png",
                      id = worksheet.Name,
                      text = worksheet.Name.Equals(worksheet.Label) ? worksheet.Name : string.Format("{0} [{1}]", worksheet.Name, worksheet.Label),
                      expanded = false,
                      leaf = false,
                      children = new List<JsonTreeNode>()
                                        {
                                        keyIdentifierNode, dataPropertiesNode
                                        },
                      record = worksheet
                    };

                    columnNodes.Add(dataPropertiesNode);

                    if (worksheet.Columns != null)
                    {
                      foreach (SpreadsheetColumn column in worksheet.Columns)
                      {
                        if (column.Name.ToUpper() == worksheet.Identifier.ToUpper())
                        {
                          JsonTreeNode keyNode = new JsonTreeNode
                          {
                            nodeType = "async",
                            type = "ExcelColumnNode",
                            icon = "Content/img/excelcolumn.png",
                            id = worksheet.Name + "/" + column.Name,
                            text = column.Name.Equals(column.Label) ? column.Name : string.Format("{0} [{1}]", column.Name, column.Label),
                            expanded = false,
                            leaf = true,
                            children = null,
                            record = new
                            {
                              Datatype = column.DataType.ToString(),
                              Index = column.ColumnIdx,
                              Label = column.Label.ToString(),
                              Name = column.Name.ToString()
                            }
                          };
                          keyIdentifierNode.children.Add(keyNode);
                        }
                        else
                        {

                          JsonTreeNode columnNode = new JsonTreeNode
                          {
                            nodeType = "async",
                            type = "ExcelColumnNode",
                            icon = "Content/img/excelcolumn.png",
                            id = worksheet.Name + "/" + column.Name,
                            text = column.Name.Equals(column.Label) ? column.Name : string.Format("{0} [{1}]", column.Name, column.Label),
                            expanded = false,
                            leaf = true,
                            children = null,
                            // record = column
                            record = new
                            {
                              Datatype = column.DataType.ToString(),
                              Index = column.ColumnIdx,
                              Label = column.Label.ToString(),
                              Name = column.Name.ToString()
                            }
                          };

                          dataPropertiesNode.children.Add(columnNode);
                        }
                      }
                      nodes.Add(dataObjectNode);
                    }
                  }
                }

                break;
              }
          }
        }
      }

      return Json(nodes, JsonRequestBehavior.AllowGet);
    }

    public JsonResult Configure(FormCollection form)
    {
      SetContextEndpoint(form);

      SpreadsheetConfiguration configuration = GetConfiguration(_context, _endpoint, _baseUrl);

      if (configuration != null && configuration.Tables.Count > 0)
      {
        _repository.Configure(_context, _endpoint, form["DataLayer"], configuration, null, _baseUrl);
        return new JsonResult() //(6)
            {
                ContentType = "text/html",
                Data = new { success = true }
            };
        }
      
      else
      {
        return new JsonResult() //(6)
        {
          ContentType = "text/html",
          Data = new { success = false }
        };
      }
    }

    public JsonResult GetConfigurationWorksheets(FormCollection form)
    {
      JsonContainer<List<WorksheetPart>> container = new JsonContainer<List<WorksheetPart>>();
      container.items = _repository.GetWorksheets(GetConfiguration(form["context"], form["endpoint"], form["baseurl"]));
      container.success = true;

      return Json(container, JsonRequestBehavior.AllowGet);
    }

    public JsonResult GetConfigurationColumns(FormCollection form)
    {
      JsonContainer<List<SpreadsheetColumn>> container = new JsonContainer<List<SpreadsheetColumn>>();
      container.items = _repository.GetColumns(GetConfiguration(form["context"], form["endpoint"], form["baseurl"]), form["worksheet"]);
      container.success = true;

      return Json(container, JsonRequestBehavior.AllowGet);
    }

    private Response PrepareErrorResponse(Exception ex)
    {
      Response response = new Response();
      response.Level = StatusLevel.Error;
      response.Messages = new Messages();
      response.Messages.Add(ex.Message);
      response.Messages.Add(ex.StackTrace);
      return response;
    }

    private SpreadsheetConfiguration GetConfiguration(string context, string endpoint, string baseurl)
    {
      string key = string.Format(_keyFormat, context, endpoint, baseurl);

      if (Session[key] == null)
      {
        Session[key] = _repository.GetConfiguration(context, endpoint, baseurl);
      }

      return (SpreadsheetConfiguration)Session[key];
    }

    private void SetConfiguration(string context, string endpoint, SpreadsheetConfiguration configuration, string baseurl)
    {
      string key = string.Format(_keyFormat, context, endpoint, baseurl);

      Session[key] = configuration;
    }

    private void SetContextEndpoint(FormCollection form)
    {
      _context = form["context"];
      _endpoint = form["endpoint"];
      _baseUrl = form["baseurl"];
    }
  }
}

