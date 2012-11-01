using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace org.iringtools.adapter.datalayer
{
  public class SpreadsheetReference
  {
    public string SheetName { get; set; }
    public string StartCol { get; set; }
    public string EndCol { get; set; }
    public uint StartRow { get; set; }
    public uint EndRow { get; set; }

    public SpreadsheetReference()
    {
    }

    public SpreadsheetReference(string reference)
    {
      SheetName = GetSheetName(reference);
      
      //Assumption: None of my defined names are relative defined names (i.e. A1)
      string range = reference.Split('!')[1];
      string[] rangeArray = range.Split(':');

      StartCol = GetColumnName(rangeArray[0]);
      StartRow = GetRowIndex(rangeArray[0]);
            
      if (rangeArray.Length > 1)
      {
        EndCol = GetColumnName(rangeArray[1]);
        EndRow = GetRowIndex(rangeArray[1]);
      }
    }

    // Given a cell name, parses the specified cell to get the column name.
    public static string GetColumnName(string cellName)
    {
      // Create a regular expression to match the column name portion of the cell name.
      Regex regex = new Regex("[A-Za-z]+");
      Match match = regex.Match(cellName);

      return match.Value;
    }

    // Given a cell name, parses the specified cell to get the row index.
    public static uint GetRowIndex(string cellName)
    {
      // Create a regular expression to match the row index portion the cell name.
      Regex regex = new Regex(@"\d+");
      Match match = regex.Match(cellName);

      return uint.Parse(match.Value);
    }

    public static string GetSheetName(string cellReference)
    {
      return cellReference.Split('!')[0].Trim('\'');
    }

    public string GetReference(bool withSheetName)
    {
      if (!EndCol.Equals(string.Empty))
      {
        if (withSheetName)
        {
          return string.Format("'{0}'!{1}{2}:{3}{4}", SheetName, StartCol, StartRow, EndCol, EndRow);
        }
        else
        {
          return string.Format("{0}{1}:{2}{3}", StartCol, StartRow, EndCol, EndRow); 
        }
      }
      else
      {
        return string.Format("{0}{1}", SheetName, StartCol, StartRow, EndRow, EndCol);
      }
    }
  }

}
