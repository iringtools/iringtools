using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using org.iringtools.library;
using org.iringtools.utility;
using log4net;

namespace org.iringtools.adapter.datalayer
{
  public class SpreadsheetProvider  : IDisposable
  {
    private AdapterSettings _settings = null;
    private string _configurationPath = string.Empty;
    private SpreadsheetConfiguration _configuration = null;
    private SpreadsheetDocument _document = null;
    private static readonly ILog _logger = LogManager.GetLogger(typeof(SpreadsheetProvider));
    //private Stream _stream = null;

    public SpreadsheetProvider(SpreadsheetConfiguration configuration)
    {
      InitializeProvider(configuration);
    }
    public SpreadsheetProvider(AdapterSettings settings)
    {
      _settings = settings;
      _configurationPath = Path.Combine(_settings["AppDataPath"], "spreadsheet-configuration." + _settings["Scope"] + ".xml");

      if (File.Exists(_configurationPath))
      {
        InitializeProvider(Utility.Read<SpreadsheetConfiguration>(_configurationPath));

        if (_configuration.Generate)
        {
          _configuration = ProcessConfiguration(_configuration, null);
          _configuration.Generate = false;
          Utility.Write<SpreadsheetConfiguration>(_configuration, _configurationPath, true);
        }
      }
    }

    public void InitializeProvider(SpreadsheetConfiguration configuration)
    {
      if (configuration != null)
      {
        _configuration = configuration;
        if (File.Exists(_configuration.Location))
        {
         // if (_stream == null) _stream = OpenStream(_configuration.Location);
          if (_document == null) _document = GetDocument(_configuration.Location);
          if (_configuration.Generate)
          {
            _configuration = ProcessConfiguration(_configuration, null);
            _configuration.Generate = false;
            Utility.Write<SpreadsheetConfiguration>(_configuration, _configurationPath, true);
          }
        }
      }
    }

    private Stream OpenStream(string path)
    {
      Stream stream = File.Open(path, FileMode.Open);
      return stream;
    }

    private SpreadsheetDocument GetDocument(string path)
    {
      SpreadsheetDocument doc = null;
     
      try
      {
        doc = SpreadsheetDocument.Open(path, true);
      }
      catch (IOException e)
      {
        throw new IOException(string.Format("File {0} is locked by other process. " + e, _configuration.Location));
      }

      return doc;
    }

    public SpreadsheetConfiguration ProcessConfiguration(SpreadsheetConfiguration configuration, Stream inputFile)
    {
        List<SpreadsheetTable> tables = new List<SpreadsheetTable>();
        if (inputFile == null)
        {
          _document = GetDocument(_configuration.Location);
        }
        else
        {
          _document = SpreadsheetDocument.Open(inputFile, false);
        }
        DefinedNames definedNames = _document.WorkbookPart.Workbook.DefinedNames;
        if (definedNames != null)
        {
          foreach (DefinedName definedName in definedNames)
          {
            SpreadsheetTable table = new SpreadsheetTable
            {
              TableType = TableType.DefinedName,
              Name = definedName.Name,
              Label = definedName.Name,
              Reference = definedName.InnerText,
              HeaderRow = 1
            };
            table.Columns = GetColumns(table);
            table.Identifier = table.Columns[0].Name;

            tables.Add(table);
          }
        }

        Sheets sheets = _document.WorkbookPart.Workbook.Sheets;

        if (sheets != null)
        {
          foreach (Sheet sheet in sheets)
          {
              if (sheet.Name.InnerText.StartsWith("Sheet")) continue;
            WorksheetPart worksheetPart = (WorksheetPart)_document.WorkbookPart.GetPartById(sheet.Id);
            SpreadsheetTable table = new SpreadsheetTable
            {
              TableType = TableType.Worksheet,
              Name = sheet.Name,
              Label = sheet.Name,
              Reference = string.Format("'{0}!'{1}", sheet.Name, worksheetPart.Worksheet.SheetDimension.Reference),
              HeaderRow = 1
            };

            table.Columns = GetColumns(table);
            table.Identifier = table.Columns[0].Name;

            tables.Add(table);
          }
        }
        configuration.Tables = tables;
        return configuration;
    }

    public DocumentBytes GetResourceData()
    {
      try
      {
        string spreadsheetPath = Path.Combine(_settings["BaseDirectoryPath"], _settings["AppDataPath"], string.Format("SpreadsheetData.{0}.xlsx", _settings["Scope"]));
        FileStream fsSrc = new FileStream(spreadsheetPath, FileMode.Open, FileAccess.Read);
        DocumentBytes document = new DocumentBytes();
        document.DocumentPath = spreadsheetPath;

        using (FileStream fsSource = fsSrc)
        {

          // Read the source file into a byte array.
          byte[] bytes = new byte[fsSource.Length];
          int numBytesToRead = (int)fsSource.Length;
          int numBytesRead = 0;
          while (numBytesToRead > 0)
          {
            // Read may return anything from 0 to numBytesToRead.
            int n = fsSource.Read(bytes, numBytesRead, numBytesToRead);

            // Break when the end of the file is reached.
            if (n == 0)
              break;

            numBytesRead += n;
            numBytesToRead -= n;
          }
          numBytesToRead = bytes.Length;
          document.Content = bytes;
          return document;
        }
      }
      catch (Exception ioEx)
      {
        _logger.Error(ioEx.Message);
        throw ioEx;
      }
    }

    public static DataType GetDataType(EnumValue<CellValues> type)
    {
      if (type == CellValues.Boolean)
      {
        return DataType.Boolean;
      }
      else if (type == CellValues.Date)
      {
        return DataType.DateTime;
      }
      else if (type == CellValues.Number)
      {
        return DataType.Double;
      }
      else
      {
        return DataType.String;
      }
    }

    public static EnumValue<CellValues> GetCellValue(DataType type)
    {
      if (type == DataType.Boolean)
      {
        return CellValues.Boolean;
      }
      else if (type == DataType.DateTime)
      {
        return CellValues.Date;
      }
      else if (type == DataType.TimeStamp)
      {
        return CellValues.Date;
      }
      else if (type == DataType.Decimal)
      {
        return CellValues.Number;
      }
      else if (type == DataType.Double)
      {
        return CellValues.Number;
      }
      else if (type == DataType.Int16)
      {
        return CellValues.Number;
      }
      else if (type == DataType.Int32)
      {
        return CellValues.Number;
      }
      else if (type == DataType.Int64)
      {
        return CellValues.Number;
      }
      else if (type == DataType.Single)
      {
        return CellValues.Number;
      }
      else
      {
        return CellValues.String;
      }
    }

    private int GetDataLength(DataType type)
    {
      if (type == DataType.Boolean)
      {
        return 8;
      }
      else if (type == DataType.DateTime)
      {
        return 50;
      }
      else if (type == DataType.TimeStamp)
      {
        return 50;
      }
      else if (type == DataType.Decimal)
      {
        return 128;
      }
      else if (type == DataType.Double)
      {
        return 64;
      }
      else if (type == DataType.Int16)
      {
        return 16;
      }
      else if (type == DataType.Int32)
      {
        return 32;
      }
      else if (type == DataType.Int64)
      {
        return 64;
      }
      else if (type == DataType.Single)
      {
        return 32;
      }
      else
      {
        return 2048;
      }
    }

    public List<SpreadsheetColumn> GetColumns(SpreadsheetTable table)
    {
        if (table.Columns == null)
        {
          WorksheetPart worksheetPart = GetWorksheetPart(table);

          List<SpreadsheetColumn> columns = new List<SpreadsheetColumn>();

          Row row = worksheetPart.Worksheet.Descendants<Row>().Where(r => r.RowIndex == 1).First();

          foreach (Cell cell in row.ChildElements)
          {
            string value = GetValue(cell);

            if (table.HeaderRow == 0)
              value = SpreadsheetReference.GetColumnName(cell.CellReference);

            SpreadsheetColumn column = new SpreadsheetColumn
            {
              Name = value,
              Label = value,
              DataType = GetDataType(cell.DataType),
              ColumnIdx = SpreadsheetReference.GetColumnName(cell.CellReference),
              DataLength = GetDataLength(GetDataType(cell.DataType))
            };

            columns.Add(column);
          }

          return columns;
        }
        else
        {
          return table.Columns;
        }

    }


    public WorksheetPart GetWorksheetPart(SpreadsheetTable table)
    {
      string sheetName = SpreadsheetReference.GetSheetName(table.Reference);
      return GetWorksheetPart(sheetName);
    }

    public WorksheetPart GetWorksheetPart(string sheetName)
    {
         if(_document == null)
         _document = GetDocument(_configuration.Location);
        string relId = _document.WorkbookPart.Workbook.Descendants<Sheet>()
                             .Where(s => sheetName.Equals(s.Name))
                             .First()
                             .Id;

        return (WorksheetPart)_document.WorkbookPart.GetPartById(relId);
    
    }

    private string GetCellValue(WorksheetPart worksheetPart, string startCol, int startRow)
    {
      string reference = startCol + startRow;

      //get exact cell based on reference 
      Cell cell = worksheetPart.Worksheet.Descendants<Cell>()
                      .Where(c => reference.Equals(c.CellReference))
                      .First();

      return GetValue(cell);
    }

    public string GetValue(Cell cell)
    {
      if (cell.ChildElements.Count == 0)
        return null;

      //get cell value
      string value = cell.CellValue.InnerText;

      //Look up real value from shared string table 
      if ((cell.DataType != null) && (cell.DataType == CellValues.SharedString))
        value = _document.WorkbookPart.SharedStringTablePart.SharedStringTable.ChildElements[Int32.Parse(value)].InnerText;

      return value;
    }

    public SpreadsheetConfiguration GetConfiguration()
    {
      if (_configuration == null) 
        _configuration = new SpreadsheetConfiguration() { Tables = new List<SpreadsheetTable>() };
      return _configuration;
    }

    public SpreadsheetTable GetConfigurationTable(string objectType)
    {
      return _configuration.Tables.First<SpreadsheetTable>(t => objectType.Equals(t.Name));
    }

    public void Save()
    {
      _document.WorkbookPart.Workbook.Save();
    }

    public void Dispose()
    {
      if (_document != null)
      {
        _document.Close();
        _document = null;
      }
    }
  }
}
