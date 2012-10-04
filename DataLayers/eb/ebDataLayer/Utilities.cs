using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using org.iringtools.library;
using System.Text.RegularExpressions;
using org.iringtools.utility;

namespace org.iringtools.adapter.datalayer.eb
{
  public static class Utilities
  {
    private static readonly string PADDING = "__";
    private static readonly string PATTERN = PADDING + ".*" + PADDING + "$";

    public static readonly string SYSTEM_ATTRIBUTE_TOKEN = PADDING + "S" + PADDING;
    public static readonly string RELATED_ATTRIBUTE_TOKEN = PADDING + "R" + PADDING;
    public static readonly string USER_ATTRIBUTE_TOKEN = PADDING + "U" + PADDING;
    public static readonly string OTHER_ATTRIBUTE_TOKEN = PADDING + "O" + PADDING;

    public static void Append(ref Status status, Status newStatus)
    {
      if (status.Level < newStatus.Level)
      {
        status.Level = newStatus.Level;
      }

      status.Messages.AddRange(newStatus.Messages);
    }

    public static DataType ToCSharpType(string ebType)
    {
      switch (ebType)
      {
        case "CH":
        case "PD":
          ebType = "String";
          break;
        case "NU":
          ebType = "Decimal";
          break;
        case "DA":
          ebType = "DateTime";
          break;
      }

      return (DataType)Enum.Parse(typeof(DataType), ebType, true);
    }

    public static string ToSqlWhereClause(DataFilter filter, DataObject objDef)
    {
      DatabaseDictionary dbDictionary = new DatabaseDictionary();
      dbDictionary.Provider = "SQL Server";

      DataObject newObjDef = Utility.CloneDataContractObject<DataObject>(objDef);

      foreach (DataProperty prop in newObjDef.dataProperties)
      {
        prop.columnName = ToQueryItem(prop, false);
      }

      dbDictionary.dataObjects.Add(newObjDef);

      return filter.ToSqlWhereClause(dbDictionary, newObjDef.tableName, string.Empty);
    }

    public static string ToPropertyName(string columnName)
    {
      string propertyName = Regex.Replace(columnName, @"\(.*\)", string.Empty);
      propertyName = Regex.Replace(propertyName, @"\W", string.Empty);
      return Regex.Replace(propertyName, PATTERN, string.Empty);
    }

    public static string ExtractColumnName(string columnName)
    {
      return Regex.Replace(columnName, PATTERN, string.Empty);
    }

    public static string ToQueryItem(DataProperty dataProperty)
    {
      return ToQueryItem(dataProperty, true);
    }

    public static string ToQueryItem(DataProperty dataProperty, bool includeAlias)
    {
      string columnName = Utilities.ExtractColumnName(dataProperty.columnName);

      if (dataProperty.columnName.EndsWith(Utilities.USER_ATTRIBUTE_TOKEN))
      {
        string item = string.Format("Attributes[\"Global\", \"{0}\"].Value", columnName);

        if (includeAlias)
        {
          item += " " + dataProperty.propertyName;
        }

        return item;
      }
      else if (!dataProperty.columnName.EndsWith(Utilities.RELATED_ATTRIBUTE_TOKEN))
      {
        string item = columnName;

        if (includeAlias)
        {
          item += " " + dataProperty.propertyName;
        }

        return item;
      }

      return string.Empty;
    }
  }
}
