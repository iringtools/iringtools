using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using log4net;
using Ninject;

using org.iringtools.library;
using org.iringtools.utility;
using org.iringtools.adapter;

using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml;
using org.iringtools.adapter.datalayer;
using System.Xml.Linq;

namespace org.iringtools.adapter.datalayer
{
  public class SpreadsheetDataLayer : BaseConfigurableDataLayer
  {
    private SpreadsheetProvider _provider = null;
    //private List<IDataObject> _dataObjects = null;
    private ILog _logger = LogManager.GetLogger(typeof(SpreadsheetDataLayer));

    [Inject]
    public SpreadsheetDataLayer(AdapterSettings settings)
      : base(settings)
    {
      _provider = new SpreadsheetProvider(settings);
    }

    public override DataDictionary GetDictionary()
    {
      try
      {
        DataDictionary dataDictionary = new DataDictionary()
        {
          dataObjects = new List<DataObject>()
        };

        foreach (SpreadsheetTable table in _provider.GetConfiguration().Tables)
        {
          DataObject dataObject = new DataObject()
          {
            objectName = table.Label,
            tableName = table.Name,
            dataProperties = new List<DataProperty>()
          };

          dataDictionary.dataObjects.Add(dataObject);

          foreach (SpreadsheetColumn column in table.Columns)
          {
            DataProperty dataProperty = new DataProperty()
            {
              propertyName = column.Label,
              columnName = column.Name,
              dataType = column.DataType,
              dataLength = column.DataLength
            };

            if (table.Identifier.Equals(column.Label))
            {
              dataObject.addKeyProperty(dataProperty);
            }
            else
            {
              dataObject.dataProperties.Add(dataProperty);
            }
          }
        }

        return dataDictionary;
      }
      catch (Exception e)
      {
        throw new Exception("Error while creating dictionary.", e);
      }
      finally
      {
        _provider.Dispose();
      }

    }

    public override IList<IDataObject> Get(string objectType, IList<string> identifiers)
    {
      try
      {
        LoadDataDictionary(objectType);

        IList<IDataObject> allDataObjects = LoadDataObjects(objectType, null);

        var expressions = FormMultipleKeysPredicate(identifiers);

        if (expressions != null)
        {
          _dataObjects = allDataObjects.AsQueryable().Where(expressions).ToList();
        }

        return _dataObjects;
      }
      catch (Exception ex)
      {
        _logger.Error("Error in GetList: " + ex);
        throw new Exception("Error while getting a list of data objects of type [" + objectType + "].", ex);
      }
      finally
      {
        _provider.Dispose();
      }
    }

    public override IList<IDataObject> Get(string objectType, DataFilter filter, int pageSize, int startIndex)
    {
      try
      {
        LoadDataDictionary(objectType);

        IList<IDataObject> allDataObjects = LoadDataObjects(objectType, filter);

        // Apply filter
        if (filter != null && filter.Expressions != null && filter.Expressions.Count > 0)
        {
          var predicate = filter.ToPredicate(_dataObjectDefinition);

          if (predicate != null)
          {
            _dataObjects = allDataObjects.AsQueryable().Where(predicate).ToList();
          }
        }

        //if (filter != null && filter.OrderExpressions != null && filter.OrderExpressions.Count > 0)
        //{
        //  throw new NotImplementedException("OrderExpressions are not supported by the CSV DataLayer.");
        //}
        else
        {
          _dataObjects = allDataObjects.ToList();
        }
        //Page and Sort The Data
        if (pageSize > _dataObjects.Count() || pageSize == 0)
          pageSize = _dataObjects.Count();
        _dataObjects = _dataObjects.Skip(startIndex).Take(pageSize).ToList();

        return _dataObjects;
      }
      catch (Exception ex)
      {
        _logger.Error("Error in GetList: " + ex);

        throw new Exception(
          "Error while getting a list of data objects of type [" + objectType + "].",
          ex
        );
      }
      finally
      {
        _provider.Dispose();
      }
    }

    public override long GetCount(string objectType, DataFilter filter)
    {
      try
      {
        //NOTE: pageSize of 0 indicates that all rows should be returned.
        return Get(objectType, filter, 0, 0).Count;
      }
      catch (Exception ex)
      {
        _logger.Error("Error in GetIdentifiers: " + ex);

        throw new Exception(
          "Error while getting a count of type [" + objectType + "].",
          ex
        );
      }
      finally
      {
        _provider.Dispose();
      }
    }

    public override IList<string> GetIdentifiers(string objectType, DataFilter filter)
    {
      try
      {
        List<string> identifiers = new List<string>();
        var dataDictionary = GetDictionary();
        var obj = dataDictionary.dataObjects.First(c => c.objectName.Equals(objectType));
        var ids = obj.keyProperties.ToList();
        //NOTE: pageSize of 0 indicates that all rows should be returned.
        IList<IDataObject> dataObjects = Get(objectType, filter, 0, 0);

        foreach (IDataObject dataObject in dataObjects)
        {
          identifiers.Add((string)dataObject.GetPropertyValue(ids[0].keyPropertyName));
        }

        return identifiers;
      }
      catch (Exception ex)
      {
        _logger.Error("Error in GetIdentifiers: " + ex);

        throw new Exception(
          "Error while getting a list of identifiers of type [" + objectType + "].",
          ex
        );
      }
      finally
      {
        _provider.Dispose();
      }
    }

    public override IList<IDataObject> GetRelatedObjects(IDataObject dataObject, string relatedObjectType)
    {
      throw new NotImplementedException();
    }

    public override Response Post(IList<IDataObject> dataObjects)
    {
      Response response = new Response();
      string objectType = String.Empty;

      if (dataObjects == null || dataObjects.Count == 0)
      {
        Status status = new Status();
        status.Level = StatusLevel.Warning;
        status.Messages.Add("Nothing to update.");
        response.Append(status);
        return response;
      }

      try
      {
        objectType = ((GenericDataObject)dataObjects.FirstOrDefault()).ObjectType;

        LoadDataDictionary(objectType);

        response = SaveDataObjects(objectType, dataObjects);
        //update configuration accordingly
        Response resp = Configure(GetConfiguration());
        return response;
      }
      catch (Exception ex)
      {
        _logger.Error("Error in Post: " + ex);

        throw new Exception(
          "Error while posting dataObjects of type [" + objectType + "].",
          ex
        );
      }
      finally
      {
        _provider.Dispose();
      }
    }

    public override Response Delete(string objectType, IList<string> identifiers)
    {
      Response response = new Response();
      try
      {
        if (identifiers == null || identifiers.Count == 0)
        {
          Status status = new Status();
          status.Level = StatusLevel.Warning;
          status.Messages.Add("Nothing to delete.");
          response.Append(status);
          return response;
        }

        SpreadsheetTable cftable = _provider.GetConfigurationTable(objectType);
        SpreadsheetReference tableReference = cftable.GetReference();

        WorksheetPart worksheetPart = _provider.GetWorksheetPart(tableReference.SheetName);
        SpreadsheetColumn column = cftable.Columns.First<SpreadsheetColumn>(c => cftable.Identifier.Equals(c.Name));

        IEnumerable<Row> rows = worksheetPart.Worksheet.Descendants<Row>();

        foreach (string identifier in identifiers)
        {
          Status status = new Status();
          status.Identifier = identifier;
          try
          {
            foreach (Row row in rows)
            {
              Cell cell = row.Descendants<Cell>().First(c => SpreadsheetReference.GetColumnName(c.CellReference).Equals(column.ColumnIdx));

              if (_provider.GetValue(cell).Equals(identifier))
              {
                row.Remove();
                string message = String.Format(
                  "DataObject [{0}] deleted successfully.",
                  identifier
                );
                status.Messages.Add(message);
              }
            }
          }
          catch (Exception ex)
          {
            _logger.Error("Error in Delete: " + ex);
            status.Level = StatusLevel.Error;
            string message = String.Format(
              "Error while deleting dataObject [{0}]. {1}",
              identifier,
              ex
            );
            status.Messages.Add(message);
          }
          response.Append(status);
          rows = worksheetPart.Worksheet.Descendants<Row>().OrderBy(r => r.RowIndex.Value);
          uint i = 1;
          foreach (Row row in rows)
          {
            row.RowIndex.Value = i++;
          }
          tableReference.EndRow = --i;
          worksheetPart.Worksheet.SheetDimension.Reference = tableReference.GetReference(false);
          cftable.Reference = tableReference.GetReference(true);
          worksheetPart.Worksheet.Save();
        }
      }
      finally
      {
        _provider.Dispose();
      }
      return response;
    }

    public override Response Delete(string objectType, DataFilter filter)
    {
      try
      {
        IList<string> identifiers = new List<string>();

        //NOTE: pageSize of 0 indicates that all rows should be returned.
        IList<IDataObject> dataObjects = Get(objectType, filter, 0, 0);

        foreach (IDataObject dataObject in dataObjects)
        {
          identifiers.Add((string)dataObject.GetPropertyValue("Tag"));
        }

        return Delete(objectType, identifiers);
      }
      catch (Exception ex)
      {
        _logger.Error("Error in Delete: " + ex);

        throw new Exception(
          "Error while deleting data objects of type [" + objectType + "].",
          ex
        );
      }
      finally
      {
        _provider.Dispose();
      }
    }

    private IList<IDataObject> LoadDataObjects(string objectType, DataFilter filter)
    {
      try
      {
        bool addDataObject = true, containFilterProperty = false;
        IList<IDataObject> dataObjects = new List<IDataObject>();
        int index = 0;

        SpreadsheetTable cfTable = _provider.GetConfigurationTable(objectType);
        SpreadsheetReference tableReference = cfTable.GetReference();

        WorksheetPart worksheetPart = _provider.GetWorksheetPart(tableReference.SheetName);

        IEnumerable<Row> rows = worksheetPart.Worksheet.Descendants<Row>().Where(r => r.RowIndex > tableReference.StartRow && r.RowIndex <= tableReference.EndRow);

        foreach (Row row in rows)
        {
          index = 0;
          addDataObject = true;
          containFilterProperty = false;
          IDataObject dataObject = new GenericDataObject
          {
            ObjectType = objectType,
          };

          foreach (Cell col in row.ChildElements)
          {
            index++;
            string columnIdx = SpreadsheetReference.GetColumnName(col.CellReference);
            SpreadsheetColumn column = cfTable.Columns.First<SpreadsheetColumn>(c => columnIdx.Equals(c.ColumnIdx));

            if (column != null)
            {
              if (index == 1)
              {
                if (_provider.GetValue(col) == null)
                {
                  addDataObject = false;
                  break;
                }
              }

              if (filter != null)
              {
                foreach (Expression expression in filter.Expressions)
                {
                  if (expression.PropertyName.ToLower().Equals(column.Name.ToLower()))
                  {
                    containFilterProperty = true;
                    if (_provider.GetValue(col) == null)
                    {
                      addDataObject = false;
                      break;
                    }
                  }
                }

                if (filter.Expressions.Count == 0)
                  addDataObject = true;
              }

              if (addDataObject)
                dataObject.SetPropertyValue(column.Name, _provider.GetValue(col));
            }
          }

          if (!containFilterProperty && filter != null)
            if (filter.Expressions.Count > 0)
              addDataObject = false;

          if (addDataObject)
            dataObjects.Add(dataObject);

          //foreach (var col in cfTable.Columns)
          //{
          //    if (!((GenericDataObject)dataObject).Dictionary.ContainsKey(col.Name))
          //        dataObject.SetPropertyValue(col.Name, null);
          //}


        }

        return dataObjects;
      }
      catch (Exception ex)
      {
        _logger.Error("Error in LoadDataObjects: " + ex);
        throw new Exception("Error while loading data objects of type [" + objectType + "].", ex);
      }
      finally
      {
        //  _provider.Dispose();
      }
    }

    private Response SaveDataObjects(string objectType, IList<IDataObject> dataObjects)
    {
      try
      {
        Response response = new Response();

        SpreadsheetTable cfTable = _provider.GetConfigurationTable(objectType);
        SpreadsheetReference tableReference = cfTable.GetReference();
        WorksheetPart worksheetPart = _provider.GetWorksheetPart(cfTable);

        foreach (IDataObject dataObject in dataObjects)
        {
          Status status = new Status();

          try
          {
            string identifier = GetIdentifier(dataObject);
            status.Identifier = identifier;

            SpreadsheetColumn column = cfTable.Columns.First<SpreadsheetColumn>(c => c.Name.Equals(cfTable.Identifier));
            Cell cell = worksheetPart.Worksheet.Descendants<Cell>().FirstOrDefault(c => SpreadsheetReference.GetColumnName(c.CellReference).Equals(column.ColumnIdx) && _provider.GetValue(c).Equals(identifier));

            if (cell != null)
            {
              Row existingRow = cell.Ancestors<Row>().First();

              foreach (SpreadsheetColumn col in cfTable.Columns)
              {
                Cell existingCell = existingRow.Descendants<Cell>().First(c => SpreadsheetReference.GetColumnName(c.CellReference).Equals(col.ColumnIdx));
                existingCell.DataType = SpreadsheetProvider.GetCellValue(col.DataType);
                if (!string.IsNullOrEmpty(Convert.ToString(dataObject.GetPropertyValue(col.Name))))
                  existingCell.CellValue.Text = Convert.ToString(dataObject.GetPropertyValue(col.Name));
                else if (existingCell.CellValue == null)
                  existingCell.CellValue = new CellValue(Convert.ToString(dataObject.GetPropertyValue(col.Name)));
              }
            }
            else
            {
              tableReference.EndRow++;

              Row newRow = new Row
              {
                RowIndex = (UInt32Value)tableReference.EndRow,
                Spans = new ListValue<StringValue>
                {
                  InnerText = string.Format("1:{0}", cfTable.Columns.Count)
                }
              };

              foreach (SpreadsheetColumn col in cfTable.Columns)
              {
                Cell newCell = new Cell
                {
                  CellReference = string.Format("{0}{1}", col.ColumnIdx, newRow.RowIndex),
                  DataType = SpreadsheetProvider.GetCellValue(col.DataType),
                  CellValue = new CellValue(Convert.ToString(dataObject.GetPropertyValue(col.Name)))
                };

                newRow.Append(newCell);
              }

              SheetData sheetData = (SheetData)worksheetPart.Worksheet.Descendants<SheetData>().First();
              sheetData.Append(newRow);
            }

            worksheetPart.Worksheet.SheetDimension.Reference = tableReference.GetReference(false);
            cfTable.Reference = tableReference.GetReference(true);

            worksheetPart.Worksheet.Save();

            status.Messages.Add("Record [" + identifier + "] has been saved successfully.");
          }
          catch (Exception ex)
          {
            status.Level = StatusLevel.Error;

            string message = String.Format(
              "Error while posting dataObject [{0}]. {1}",
              dataObject.GetPropertyValue("Tag"),
              ex.ToString()
            );

            status.Messages.Add(message);
          }

          response.Append(status);
        }

        return response;
      }
      catch (Exception ex)
      {
        _logger.Error("Error in LoadDataObjects: " + ex);
        throw new Exception("Error while loading data objects of type [" + objectType + "].", ex);
      }
      finally
      {
        _provider.Dispose();
      }
    }      

    public override Response Configure(XElement configuration)
    {
      Response resp = new Response(){ StatusList = new List<Status>()};
      try
      {
        SpreadsheetConfiguration config = Utility.DeserializeFromXElement<SpreadsheetConfiguration>(configuration);
        string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _settings["AppDataPath"], string.Format("spreadsheet-configuration.{0}.xml", _settings["scope"]));
        Utility.Write<SpreadsheetConfiguration>(config, path, true);

      }
      catch (Exception)
      {
        Status stat  = new Status { Level = StatusLevel.Error };
        resp.StatusList.Add(stat);
      }
      return resp;
    }

    public override XElement GetConfiguration()
    {      
      SpreadsheetConfiguration sc = _provider.GetConfiguration();
      return Utility.SerializeToXElement<SpreadsheetConfiguration>(sc);
    }

    public override DocumentBytes GetResourceData()
    {
      DocumentBytes sc = _provider.GetResourceData();
      return sc;
    }
  }
}
