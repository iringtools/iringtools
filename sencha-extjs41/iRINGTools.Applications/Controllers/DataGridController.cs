using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;


using org.iringtools.library;

using iRINGTools.Web.Models;
using log4net;
using System.Web.Script.Serialization;

namespace org.iringtools.web.controllers
{
  public class DatagridController : BaseController
  {
    private static readonly ILog _logger = LogManager.GetLogger(typeof(DatagridController));
    private IGridRepository _repository { get; set; }
    private DataDictionary dataDict = null;

    private JavaScriptSerializer serializer;
    private string response = "";
    private string _key = null;
    private string _context = string.Empty;
    private string _endpoint = string.Empty;
    private string _baseUrl = string.Empty;    

    public DatagridController() : this(new GridRepository()) { }

    public DatagridController(IGridRepository repository)
    {
      _repository = repository;
      serializer = new JavaScriptSerializer();
    }

    public JsonResult GetData(FormCollection form)
    {
      try
      {
        SetContextEndpoint(form);
        response = _repository.DataServiceUri();
        if (response != "")
          return Json(new { success = false } + response, JsonRequestBehavior.AllowGet);

        var metaData = new Dictionary<string, object>();
        var gridData = new List<Dictionary<string, object>>();
        var encode = new Dictionary<string, object>();
        DataItems dataItems = new DataItems();        
        string graph = form["graph"];
        _key = adapter_PREFIX + string.Format("Datadictionary-{0}.{1}", _context, _endpoint, _baseUrl);
        string filter = form["filter"];
        string sort = form["sort"];
        string dir = form["dir"];
        int start = 0;
        int.TryParse(form["start"], out start);
        int limit = 25;
        int.TryParse(form["limit"], out limit);
        string currFilter = filter + "/" + sort + "/" + dir;
        DataFilter dataFilter = CreateDataFilter(filter, sort, dir);
        //DataFilter dataFilter = null;
        bool found = false;

        if (((DataDictionary)Session[_key]) == null)
          GetDatadictionary(_context, _endpoint, _baseUrl);

        DataObject dataObject = ((DataDictionary)Session[_key]).dataObjects.FirstOrDefault(d => d.objectName == graph);
        List<Field> fields = new List<Field>();

        foreach (DataProperty dataProperty in dataObject.dataProperties)
        {
          if (!dataProperty.isHidden)
          {
            Field field = new Field
                       {
                         name = dataProperty.propertyName,
                         header = dataProperty.propertyName,
                         dataIndex = dataProperty.propertyName,
                         sortable = true,
                         type = ToExtJsType(dataProperty.dataType),
                         filterable = true
                       };

            //if (dataProperty.keyType == KeyType.assigned || dataProperty.keyType == KeyType.foreign)
            //  field.keytype = "key";

            fields.Add(field);
          }
        }

        dataItems = GetDataObjects(_context, _endpoint, graph, dataFilter, start, limit, _baseUrl);

        long total = dataItems.total;

        foreach (DataItem dataItem in dataItems.items)
        {
          var rowData = new Dictionary<string, object>();

          foreach (Field field in fields)
          {
            found = false;          
            foreach (KeyValuePair<string, string> property in dataItem.properties)
            {
              if (field.dataIndex.ToLower() == property.Key.ToLower())
              {
                rowData.Add(property.Key, property.Value);
                found = true;
                break;
              }
            }

            if (!found)
            {
              rowData.Add(field.dataIndex, "");
            }
          }
          gridData.Add(rowData);
        }

        metaData.Add("root", "data");
        metaData.Add("fields", fields);
        encode.Add("metaData", metaData);
        encode.Add("success", "true");
        encode.Add("data", gridData);
        encode.Add("totalCount", total);
        return Json(encode, JsonRequestBehavior.AllowGet);
      }
      catch (Exception ex)
      {
        response = response + " " + ex.Message.ToString();
        _logger.Error(ex + " " + response);
        return Json(new { success = false } + response, JsonRequestBehavior.AllowGet);
      }
    }

    private RelationalOperator GetOpt(string opt)
    {
      switch (opt.ToLower())
      {
        case "eq":
          return RelationalOperator.EqualTo;
        case "lt":
          return RelationalOperator.LesserThan;
        case "gt":
          return RelationalOperator.GreaterThan;
      }
      return RelationalOperator.EqualTo;
    }

    private DataFilter CreateDataFilter(string filter, string sortBy, string sortOrder)
    {
      var dataFilter = new DataFilter();

      // process filtering
      if (!string.IsNullOrEmpty(filter))
      {
        try
        {
          List<Dictionary<String, String>> filterExpressions = (List<Dictionary<String, String>>)serializer.Deserialize(filter, typeof(List<Dictionary<String, String>>));

          if (filterExpressions != null && filterExpressions.Count > 0)
          {

            List<Expression> expressions = new List<Expression>();
            dataFilter.Expressions = expressions;

            foreach (Dictionary<String, String> filterExpression in filterExpressions)
            {
              Expression expression = new Expression();
              expressions.Add(expression);

              if (expressions.Count > 1)
              {
                expression.LogicalOperator = LogicalOperator.And;
              }

              if (filterExpression["comparison"] != null)
              {
                RelationalOperator optor = GetOpt(filterExpression["comparison"]);
                expression.RelationalOperator = optor;
              }
              else
              {
                expression.RelationalOperator = RelationalOperator.EqualTo;
              }

              expression.PropertyName = filterExpression["field"];

              Values values = new Values();
              expression.Values = values;
              string value = filterExpression["value"];
              values.Add(value);
            }
          }
        }
        catch (Exception ex)
        {
          _logger.Error("Error deserializing filter: " + ex);
          response = response + " " + ex.Message.ToString();
        }
      }

      // process sorting
      if (sortBy != null && sortBy.Count() > 0 && sortOrder != null && sortOrder.Count() > 0)
      {

        List<OrderExpression> orderExpressions = new List<OrderExpression>();
        dataFilter.OrderExpressions = orderExpressions;

        OrderExpression orderExpression = new OrderExpression();
        orderExpressions.Add(orderExpression);

        if (sortBy != null)
          orderExpression.PropertyName = sortBy;

        string Sortorder = sortOrder.Substring(0, 1).ToUpper() + sortOrder.Substring(1);

        if (Sortorder != null)
        {
          try
          {
            orderExpression.SortOrder = (SortOrder)Enum.Parse(typeof(SortOrder), Sortorder);
          }
          catch (Exception ex)
          {
            _logger.Error(ex.ToString());
            response = response + " " + ex.Message.ToString();
          }
        }
      }

      return dataFilter;
    }

    private void GetDatadictionary(string context, string endpoint, string baseurl)
    {
      try
      {
        if (Session[_key] == null)
        {
          Session[_key] = _repository.GetDictionary(context, endpoint, baseurl);
        }
        dataDict = (DataDictionary)Session[_key];
        if (dataDict.dataObjects.Count == 0)
          response = "There is no records in the database for data object \"" + endpoint + "\"";
      }
      catch (Exception ex)
      {
        _logger.Error("Error getting DatabaseDictionary." + ex);
        response = response + " " + ex.Message.ToString();
      }
    }


    private DataItems GetDataObjects(string context, string endpoint, string graph, DataFilter dataFilter, int start, int limit, string baseurl)
    {
      DataItems dataItems = null;
      try
      {
        dataItems = _repository.GetDataItems(endpoint, context, graph, dataFilter, start, limit, baseurl);
      }
      catch (Exception ex)
      {
        if (ex.InnerException != null)
          _logger.Error("Error deserializing filtered data objects: " + ex);
        if (response != "success")
        {
          response = ex.Message.ToString();
          if (ex.InnerException.Message != null)
            response = response + " " + ex.InnerException.Message.ToString();
        }
      }

      return dataItems;
    }

    private void SetContextEndpoint(FormCollection form)
    {
      _context = form["context"];
      _endpoint = form["endpoint"];
      _baseUrl = form["baseUrl"];
    }

    private String ToExtJsType(org.iringtools.library.DataType dataType)
    {
      switch (dataType)
      {
        case org.iringtools.library.DataType.Boolean:
          return "boolean";

        case org.iringtools.library.DataType.Char:
        case org.iringtools.library.DataType.String:
        case org.iringtools.library.DataType.DateTime:
          return "string";

        case org.iringtools.library.DataType.Byte:
        case org.iringtools.library.DataType.Int16:
        case org.iringtools.library.DataType.Int32:
        case org.iringtools.library.DataType.Int64:
          return "int";

        case org.iringtools.library.DataType.Single:
        case org.iringtools.library.DataType.Double:
        case org.iringtools.library.DataType.Decimal:
          return "float";

        default:
          return "auto";
      }
    }

  }
}