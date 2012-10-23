using System;
using System.Collections.Generic;
using System.Xml.Linq;
using log4net;
using Ninject;
using System.Web;
using org.iringtools.utility;
using org.iringtools.library;
using System.Text.RegularExpressions;

namespace org.iringtools.adapter.projection
{
  public class HtmlProjectionEngine : BaseDataProjectionEngine
  {
    private static readonly ILog _logger = LogManager.GetLogger(typeof(HtmlProjectionEngine));
    private DataDictionary _dictionary = null;

    [Inject]
    public HtmlProjectionEngine(AdapterSettings settings, DataDictionary dictionary) : base(settings)
    {
      _settings = settings;
      _dictionary = dictionary;
    }

    public override XDocument ToXml(string graphName, ref IList<IDataObject> dataObjects)
    {
      try
      {
        XDocumentType docType = new XDocumentType(
          "html", "-//W3C//DTD XHTML 1.0 Strict//EN", "http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd", null);

        XDocument doc = new XDocument();
        doc.AddFirst(docType);

        if (dataObjects.Count == 1)
        {
          XElement html = CreateHtmlListView(graphName, dataObjects[0]);
          doc.Add(html);         
        }
        else
        {
          XElement html = CreateHtmlTableView(graphName, ref dataObjects);
          doc.Add(html); 
        }

        return doc;
      }
      catch (Exception e)
      {
        _logger.Error("Error creating HTML content: " + e);
        throw e;
      }      
    }

    public override XDocument ToXml(string graphName, ref IList<IDataObject> dataObjects, string className, string classIdentifier)
    {
      return ToXml(graphName, ref dataObjects);
    }

    public override IList<IDataObject> ToDataObjects(string graphName, ref XDocument xml)
    {
      throw new NotImplementedException();
    }

    #region helper methods    
    private XElement CreateHtmlTableView(string graphName, ref IList<IDataObject> dataObjects)
    {
      XElement html = new XElement("html");
      
      XElement head = new XElement("head");
      html.Add(head);

      XElement style = new XElement("style");
      head.Add(style);

      style.Add(new XAttribute("type", "text/css"));

      string css = Utility.ReadString(_settings["DefaultStyleSheet"]);
      style.Add(Regex.Replace(css, @"\s+", " "));

      XElement body = new XElement("body");
      html.Add(body);

      XElement count = new XElement("span", "Total Count: " + this.Count);
      body.Add(count);

      XElement table = new XElement("table");
      body.Add(table);

      XElement headers = new XElement("tr");
      table.Add(headers);

      DataObject dataObjectDef = FindGraphDataObject(graphName);

      foreach (DataProperty dataProperty in dataObjectDef.dataProperties)
      {
        headers.Add(new XElement("th", dataProperty.propertyName));
      }

      for (int i = 0; i < dataObjects.Count; i++)
      {
        IDataObject dataObject = dataObjects[i];

        XElement row = new XElement("tr");
        table.Add(row);

        if (i % 2 == 0)
        {
          row.Add(new XAttribute("class", "even"));
        }
        else
        {
          row.Add(new XAttribute("class", "odd"));
        }

        foreach (DataProperty dataProperty in dataObjectDef.dataProperties)
        {
          string value = Convert.ToString(dataObject.GetPropertyValue(dataProperty.propertyName));

          if (value == null)
          {
            value = String.Empty;
          }
          else if (dataProperty.dataType == DataType.DateTime)
          {
            value = Utility.ToXsdDateTime(value);
          }

          XElement cell = new XElement("td", value);
          row.Add(cell);

          if (IsNumeric(dataProperty.dataType))
          {
            cell.Add(new XAttribute("class", "right"));
          }
        }
      }

      return html;
    }

    private XElement CreateHtmlListView(string graphName, IDataObject dataObject)
    {
      XElement html = new XElement("html");

      XElement head = new XElement("head");
      html.Add(head);

      XElement style = new XElement("style");
      head.Add(style);

      style.Add(new XAttribute("type", "text/css"));

      string css = Utility.ReadString(_settings["DefaultStyleSheet"]);
      style.Add(Regex.Replace(css, @"\s+", " "));

      XElement body = new XElement("body");
      html.Add(body);

      XElement table = new XElement("table");
      body.Add(table);

      DataObject dataObjectDef = FindGraphDataObject(graphName);

      for (int i = 0; i < dataObjectDef.dataProperties.Count; i++ )
      {
        DataProperty dataProperty = dataObjectDef.dataProperties[i];
        string propertyName = dataProperty.propertyName;
        string value = Convert.ToString(dataObject.GetPropertyValue(propertyName));

        XElement row = new XElement("tr");
        table.Add(row);

        if (i % 2 == 0)
        {
          row.Add(new XAttribute("class", "even"));
        }
        else
        {
          row.Add(new XAttribute("class", "odd"));
        }

        XElement nameCol = new XElement("td", propertyName);
        row.Add(nameCol);

        if (value == null)
        {
          value = String.Empty;
        }
        else if (dataProperty.dataType == DataType.DateTime)
        {
          value = Utility.ToXsdDateTime(value);
        }

        XElement valueCol = new XElement("td", value);
        row.Add(valueCol);
      }

      return html;
    }

    private DataObject FindGraphDataObject(string dataObjectName)
    {
      foreach (DataObject dataObject in _dictionary.dataObjects)
      {
        if (dataObject.objectName.ToLower() == dataObjectName.ToLower())
        {
          return dataObject;
        }
      }

      throw new Exception("DataObject [" + dataObjectName + "] does not exist.");
    }

    private bool IsNumeric(DataType dataType)
    {
      return (dataType == DataType.Decimal ||
              dataType == DataType.Single ||
              dataType == DataType.Double ||
              dataType == DataType.Int16 ||
              dataType == DataType.Int32 ||
              dataType == DataType.Int64);
    }
    #endregion
  }
}
