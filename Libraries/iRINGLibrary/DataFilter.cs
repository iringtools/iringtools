// Copyright (c) 2009, ids-adi.org /////////////////////////////////////////////
// All rights reserved.
//------------------------------------------------------------------------------
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//     * Redistributions of source code must retain the above copyright
//       notice, this list of conditions and the following disclaimer.
//     * Redistributions in binary form must reproduce the above copyright
//       notice, this list of conditions and the following disclaimer in the
//       documentation and/or other materials provided with the distribution.
//     * Neither the name of the ids-adi.org nor the
//       names of its contributors may be used to endorse or promote products
//       derived from this software without specific prior written permission.
//------------------------------------------------------------------------------
// THIS SOFTWARE IS PROVIDED BY ids-adi.org ''AS IS'' AND ANY
// EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL ids-adi.org BE LIABLE FOR ANY
// DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
// LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
// ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
// SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
////////////////////////////////////////////////////////////////////////////////

using System.Runtime.Serialization;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Text;
using LINQ = System.Linq.Expressions;
using System.Collections;
using org.iringtools.utility;

namespace org.iringtools.library
{
  [DataContract(Namespace = "http://www.iringtools.org/data/filter", Name = "dataFilter")]
  public class DataFilter
  {
    public static readonly String SYSTEM_DATETIME_FORMAT = "yyyy-MM-dd HH:mm:ss.ff";
    public static readonly String UNIVERSAL_DATETIME_FORMAT = "YYYY-MM-DD HH24:MI:SS.FF TZH:TZM";
    
    private List<Expression> _filterBuffer = null;
    private String _provider = String.Empty;
    private DataObject _dataObjectDefinition = null;

    public DataFilter()
    {
      Expressions = new List<Expression>();
      OrderExpressions = new List<OrderExpression>();
    }

    [DataMember(Name = "expressions", Order = 0, EmitDefaultValue = false)]
    public List<Expression> Expressions { get; set; }

    [DataMember(Name = "orderExpressions", Order = 1, EmitDefaultValue = false)]
    public List<OrderExpression> OrderExpressions { get; set; }

    [Obsolete("Use ToSqlWhereClause(DatabaseDictionary dbDictionary, string tableName, string objectAlias) instead")]
    public string ToSqlWhereClause(DataDictionary dataDictionary, string tableName, string objectAlias)
    {
      DatabaseDictionary dbDictionary = new DatabaseDictionary();
      dbDictionary.Provider = String.Empty;
      dbDictionary.dataObjects = Utility.CloneDataContractObject<List<DataObject>>(dataDictionary.dataObjects);
      return ToSqlWhereClause(dbDictionary, tableName, objectAlias);
    }

    public string ToSqlWhereClause(DatabaseDictionary dbDictionary, string tableName, string objectAlias)
    {
      _provider = dbDictionary.Provider;
      DataObject dataObject = null;
      dataObject = dbDictionary.dataObjects.Find(x => x.tableName.ToUpper() == tableName.ToUpper());
      if (!String.IsNullOrEmpty(objectAlias)) objectAlias += ".";
      else objectAlias = String.Empty;

      try
      {
        if (dataObject == null)
        {
          throw new Exception("Data object not found.");
        }

        StringBuilder whereClause = new StringBuilder();

        if (Expressions != null && Expressions.Count > 0)
        {
          whereClause.Append(" WHERE ");

          foreach (Expression expression in this.Expressions)
          {
            if (whereClause.Length <= 8) // To avoid adding logical operator after where clause.
            {
              expression.LogicalOperator = iringtools.library.LogicalOperator.None;
            }

            string sqlExpression = ResolveSqlExpression(dataObject, expression, objectAlias);
            whereClause.Append(sqlExpression);
          }
        }

        if (OrderExpressions != null && OrderExpressions.Count > 0)
        {
          whereClause.Append(" ORDER BY ");

          foreach (OrderExpression orderExpression in this.OrderExpressions)
          {
            string propertyName = orderExpression.PropertyName;
            DataProperty dataProperty = null;
            dataProperty = dataObject.dataProperties.Find(x => x.propertyName.ToUpper() == propertyName.ToUpper());
            string orderStatement = ResolveOrderExpression(orderExpression, objectAlias + dataProperty.columnName);
            whereClause.Append(orderStatement);
          }
        }

        return whereClause.ToString();
      }
      catch (Exception ex)
      {
        throw new Exception("Error generating SQL WHERE clause.", ex);
      }
    }

    public void AppendFilter(DataFilter filter)
    {
      if (filter != null && (filter.Expressions.Count > 0 || filter.OrderExpressions.Count > 0))
      {
        if (Expressions == null)
          Expressions = new List<Expression>();

        if (OrderExpressions == null)
          OrderExpressions = new List<OrderExpression>();

      
        DataFilter clonedFilter = Utility.CloneDataContractObject<DataFilter>(filter);
        if (filter.Expressions != null)
        {
          int maxIndex = clonedFilter.Expressions.Count - 1;
          clonedFilter.Expressions[0].LogicalOperator = LogicalOperator.And;
          clonedFilter.Expressions[0].OpenGroupCount++;
          clonedFilter.Expressions[maxIndex].CloseGroupCount++;

          Expressions.AddRange(clonedFilter.Expressions);
        }

        if (clonedFilter.OrderExpressions != null)
          foreach (OrderExpression orderExpression in clonedFilter.OrderExpressions)
          {
            if (!DuplicateOrderExpression(orderExpression))
              OrderExpressions.Add(orderExpression);
          }
      }
    }

    private bool DuplicateOrderExpression(OrderExpression orderExpression)
    {
      foreach (OrderExpression item in OrderExpressions)
      {
        if (item.PropertyName.ToLower() == orderExpression.PropertyName.ToLower())
        {
          if (item.SortOrder == orderExpression.SortOrder)
            return true;
          else
          {
            item.SortOrder = orderExpression.SortOrder;
            return true;
          }
        }
      }
      return false;
    }    

    public string ToLinqExpression<T>(string objectVariable)
    {
      if (this == null || this.Expressions.Count == 0)
        return String.Empty;

      if (!String.IsNullOrEmpty(objectVariable)) objectVariable += ".";
      else throw new Exception("Object variable can not be null or empty.");

      try
      {
        StringBuilder linqExpression = new StringBuilder();

        foreach (Expression expression in this.Expressions)
        {
          string exp = ResolveLinqExpression<T>(expression, objectVariable);
          linqExpression.Append(exp);
        }

        return linqExpression.ToString();
      }
      catch (Exception ex)
      {
        throw new Exception("Error while generating LINQ expression.", ex);
      }
    }

    private bool IsNumeric(DataType dataType)
    {
      return 
        dataType == DataType.Byte ||
        dataType == DataType.Decimal ||
        dataType == DataType.Int16 ||
        dataType == DataType.Int32 ||
        dataType == DataType.Int64 ||
        dataType == DataType.Single ||
        dataType == DataType.Double;
    }

    private string ToOracleDateTimeExpression(Values values)
    {
      StringBuilder strBuilder = new StringBuilder();

      foreach (string value in values)
      {
        if (strBuilder.Length > 0)
        {
          strBuilder.Append(", ");
        }

        strBuilder.Append(String.Format("TO_TIMESTAMP_TZ('{0}','{1}')", value, UNIVERSAL_DATETIME_FORMAT));
      }

      return strBuilder.ToString();
    }

    private string ResolveSqlExpression(DataObject dataObject, Expression expression, string objectAlias)
    {
      string propertyName = expression.PropertyName;
      DataProperty dataProperty = null;
#if !SILVERLIGHT
      dataProperty = dataObject.dataProperties.Find(x => x.propertyName.ToUpper() == propertyName.ToUpper());
#endif

      if (dataProperty == null)
      {
        throw new Exception("Data property [" + expression.PropertyName + "] not found.");
      }

      DataType propertyType = dataProperty.dataType;
      string columnName = dataProperty.columnName;
      string qualColumnName = String.Empty;

      if (expression.IsCaseSensitive || IsNumeric(dataProperty.dataType) || dataProperty.dataType == DataType.DateTime)
      {
        qualColumnName = objectAlias + columnName;
      }
      else
      {
        qualColumnName = "UPPER(" + objectAlias + columnName + ")";
      }

      bool isString = (propertyType == DataType.String || propertyType == DataType.Reference || propertyType == DataType.Char);
      StringBuilder sqlExpression = new StringBuilder();

      if (expression.LogicalOperator != LogicalOperator.None)
      {
        string logicalOperator = ResolveLogicalOperator(expression.LogicalOperator);
        sqlExpression.Append(" " + logicalOperator + " ");
      }

      for (int i = 0; i < expression.OpenGroupCount; i++)
        sqlExpression.Append("(");

      if (propertyType == DataType.DateTime)
      {
        // convert datetime to correct format
        for (int i = 0; i < expression.Values.Count; i++)
        {
          string dateTimeValue = expression.Values[i];
          DateTime dateTime = DateTime.Parse(dateTimeValue);
          string formattedDateTimeValue = dateTime.ToString(SYSTEM_DATETIME_FORMAT);

          expression.Values[i] = formattedDateTimeValue;
        }
      }

      string value = String.Empty;
      
      switch (expression.RelationalOperator)
      {
        case RelationalOperator.StartsWith:
          if (!isString) throw new Exception("StartsWith operator used with non-string property");

          if (expression.IsCaseSensitive)
          {
            value = expression.Values.FirstOrDefault();
          }
          else
          {
            value = expression.Values.FirstOrDefault().ToUpper();
          }

          sqlExpression.Append(qualColumnName + " LIKE '" + value.Replace("'", "''") + "%'");

          break;

        case RelationalOperator.Contains:
          if (!isString) throw new Exception("Contains operator used with non-string property");

          if (expression.IsCaseSensitive)
          {
            value = expression.Values.FirstOrDefault();
          }
          else
          {
            value = expression.Values.FirstOrDefault().ToUpper();
          }

          sqlExpression.Append(qualColumnName + " LIKE '%" + value.Replace("'", "''") + "%'");

          break;

        case RelationalOperator.EndsWith:
          if (!isString) throw new Exception("EndsWith operator used with non-string property");

          if (expression.IsCaseSensitive)
          {
            value = expression.Values.FirstOrDefault();
          }
          else
          {
            value = expression.Values.FirstOrDefault().ToUpper();
          }

          sqlExpression.Append(qualColumnName + " LIKE '%" + value.Replace("'", "''") + "'");
          break;

        case RelationalOperator.In:
          if (isString)
          {
            if (expression.IsCaseSensitive)
            {
              value = String.Join("','", expression.Values.ToArray());
            }
            else
            {
              value = String.Join("','", expression.Values.ToArray()).ToUpper();
            }

            sqlExpression.Append(qualColumnName + " IN ('" + value.Replace("'", "''") + "')");
          }
          else if (dataProperty.dataType == DataType.DateTime)
          {
            if (_provider.ToUpper().StartsWith("ORACLE"))
            {
              //e.g. dateTimeCol IN (TO_TIMESTAMP_TZ('<date string 1>', '<format>'), TO_TIMESTAMP_TZ('<date string 2>', '<format>'))
              sqlExpression.Append(qualColumnName + " IN (" + ToOracleDateTimeExpression(expression.Values) + ")");
            }
            else
            {
              //e.g. dateTimeCol IN ('<date string 1>', '<date string 2>')
              value = String.Join("','", expression.Values.ToArray()).ToUpper();
              sqlExpression.Append(qualColumnName + " IN ('" + value + "')");
            }
          }
          else
          {
            sqlExpression.Append(qualColumnName + " IN (" + String.Join(",", expression.Values.ToArray()) + ")");
          }
          break;

        case RelationalOperator.EqualTo:
          if (isString)
          {
            if (expression.IsCaseSensitive)
            {
              value = expression.Values.FirstOrDefault();
            }
            else
            {
              value = expression.Values.FirstOrDefault().ToUpper();
            }

            sqlExpression.Append(qualColumnName + "='" + value.Replace("'", "''") + "'");
          }
          else if (dataProperty.dataType == DataType.DateTime)
          {
            if (_provider.ToUpper().StartsWith("ORACLE"))
            {
              //e.g. dateTimeCol = TO_TIMESTAMP_TZ('<date string>', '<format>')
              sqlExpression.Append(qualColumnName + "=" + ToOracleDateTimeExpression(expression.Values));
            }
            else
            {
              //e.g. dateTimeCol = '<date string>'
              sqlExpression.Append(qualColumnName + "='" + expression.Values.FirstOrDefault() + "'");
            }
          }
          else
          {
            sqlExpression.Append(qualColumnName + "=" + expression.Values.FirstOrDefault() + "");
          }
          break;

        case RelationalOperator.NotEqualTo:
          if (isString)
          {
            if (expression.IsCaseSensitive)
            {
              value = expression.Values.FirstOrDefault();
            }
            else
            {
              value = expression.Values.FirstOrDefault().ToUpper();
            }

            sqlExpression.Append(qualColumnName + "<>'" + value.Replace("'", "''") + "'");
          }
          else if (dataProperty.dataType == DataType.DateTime)
          {
            if (_provider.ToUpper().StartsWith("ORACLE"))
            {
              //e.g. dateTimeCol <> TO_TIMESTAMP_TZ('<date string>', '<format>')
              sqlExpression.Append(qualColumnName + "<>" + ToOracleDateTimeExpression(expression.Values));
            }
            else
            {
              //e.g. dateTimeCol <> '<date string>'
              sqlExpression.Append(qualColumnName + "<>'" + expression.Values.FirstOrDefault() + "'");
            }
          }
          else
          {
            sqlExpression.Append(qualColumnName + "<>" + expression.Values.FirstOrDefault() + "");
          }
          break;

        case RelationalOperator.GreaterThan:
          if (isString)
          {
            if (expression.IsCaseSensitive)
            {
              value = expression.Values.FirstOrDefault();
            }
            else
            {
              value = expression.Values.FirstOrDefault().ToUpper();
            }

            sqlExpression.Append(qualColumnName + ">'" + value.Replace("'", "''") + "'");
          }
          else if (dataProperty.dataType == DataType.DateTime)
          {
            if (_provider.ToUpper().StartsWith("ORACLE"))
            {
              //e.g. dateTimeCol > TO_TIMESTAMP_TZ('<date string>', '<format>')
              sqlExpression.Append(qualColumnName + ">" + ToOracleDateTimeExpression(expression.Values));
            }
            else
            {
              //e.g. dateTimeCol > '<date string>'
              sqlExpression.Append(qualColumnName + ">'" + expression.Values.FirstOrDefault() + "'");
            }
          }
          else
          {
            sqlExpression.Append(qualColumnName + ">" + expression.Values.FirstOrDefault());
          }
          break;

        case RelationalOperator.GreaterThanOrEqual:
          if (isString)
          {
            if (expression.IsCaseSensitive)
            {
              value = expression.Values.FirstOrDefault();
            }
            else
            {
              value = expression.Values.FirstOrDefault().ToUpper();
            }

            sqlExpression.Append(qualColumnName + ">='" + value.Replace("'", "''") + "'");
          }
          else if (dataProperty.dataType == DataType.DateTime)
          {
            if (_provider.ToUpper().StartsWith("ORACLE"))
            {
              //e.g. dateTimeCol >= TO_TIMESTAMP_TZ('<date string>', '<format>')
              sqlExpression.Append(qualColumnName + ">=" + ToOracleDateTimeExpression(expression.Values));
            }
            else
            {
              //e.g. dateTimeCol >= '<date string>'
              sqlExpression.Append(qualColumnName + ">='" + expression.Values.FirstOrDefault() + "'");
            }
          }
          else
          {
            sqlExpression.Append(qualColumnName + ">=" + expression.Values.FirstOrDefault());
          }
          break;

        case RelationalOperator.LesserThan:
          if (isString)
          {
            if (expression.IsCaseSensitive)
            {
              value = expression.Values.FirstOrDefault();
            }
            else
            {
              value = expression.Values.FirstOrDefault().ToUpper();
            }

            sqlExpression.Append(qualColumnName + "<'" + value.Replace("'", "''") + "'");
          }
          else if (dataProperty.dataType == DataType.DateTime)
          {
            if (_provider.ToUpper().StartsWith("ORACLE"))
            {
              //e.g. dateTimeCol < TO_TIMESTAMP_TZ('<date string>', '<format>')
              sqlExpression.Append(qualColumnName + "<" + ToOracleDateTimeExpression(expression.Values));
            }
            else
            {
              //e.g. dateTimeCol < '<date string>'
              sqlExpression.Append(qualColumnName + "<'" + expression.Values.FirstOrDefault() + "'");
            }
          }
          else
          {
            sqlExpression.Append(qualColumnName + "<" + expression.Values.FirstOrDefault());
          }
          break;

        case RelationalOperator.LesserThanOrEqual:
          if (isString)
          {
            if (expression.IsCaseSensitive)
            {
              value = expression.Values.FirstOrDefault();
            }
            else
            {
              value = expression.Values.FirstOrDefault().ToUpper();
            }

            sqlExpression.Append(qualColumnName + "<='" + value.Replace("'", "''") + "'");
          }
          else if (dataProperty.dataType == DataType.DateTime)
          {
            if (_provider.ToUpper().StartsWith("ORACLE"))
            {
              //e.g. dateTimeCol <= TO_TIMESTAMP_TZ('<date string>', '<format>')
              sqlExpression.Append(qualColumnName + "<=" + ToOracleDateTimeExpression(expression.Values));
            }
            else
            {
              //e.g. dateTimeCol <= '<date string>'
              sqlExpression.Append(qualColumnName + "<='" + expression.Values.FirstOrDefault() + "'");
            }
          }
          else
          {
            sqlExpression.Append(qualColumnName + "<=" + expression.Values.FirstOrDefault());
          }
          break;

        default:
          throw new Exception("Relational operator does not exist.");
      }

      for (int i = 0; i < expression.CloseGroupCount; i++)
        sqlExpression.Append(")");

      return sqlExpression.ToString();
    }

    private string ResolveOrderExpression(OrderExpression orderExpression, string qualColumnName)
    {
      StringBuilder sqlExpression = new StringBuilder();

      switch (orderExpression.SortOrder)
      {
        case SortOrder.Asc:
          sqlExpression.Append(qualColumnName + " ASC");
          break;

        case SortOrder.Desc:
          sqlExpression.Append(qualColumnName + " DESC");
          break;

        default:
          throw new Exception("Sort order is not specified.");
      }

      return sqlExpression.ToString();
    }

    private string ResolveLogicalOperator(LogicalOperator logicalOperator)
    {
      switch (logicalOperator)
      {
        case LogicalOperator.And:
          return "AND";

        case LogicalOperator.AndNot:
          return "AND NOT";

        case LogicalOperator.Not:
          return "NOT";

        case LogicalOperator.Or:
          return "OR";

        case LogicalOperator.OrNot:
          return "OR NOT";

        default:
          throw new Exception("Logical operator [" + logicalOperator + "] not supported.");
      }
    }

    public LINQ.Expression<Func<IDataObject, bool>> ToPredicate(DataObject dataObjectDefinition)
    {
      _dataObjectDefinition = dataObjectDefinition;

      return ToPredicate(0);
    }

    public LINQ.Expression<Func<IDataObject, bool>> ToPredicate(int groupLevel)
    {
      LINQ.Expression<Func<IDataObject, bool>> predicate = null;

      try
      {
        if (Expressions != null && Expressions.Count > 0)
        {
          if (_filterBuffer == null)
            _filterBuffer = Expressions.ToList();

          List<Expression> localBuffer = _filterBuffer.ToList();
          foreach (Expression expression in localBuffer)
          {
            _filterBuffer.Remove(expression);

            groupLevel += (expression.OpenGroupCount - expression.CloseGroupCount);

            switch (expression.LogicalOperator)
            {
              case LogicalOperator.And:
              case LogicalOperator.None:
                if (predicate == null)
                  predicate = PredicateBuilder.True<IDataObject>();
                predicate = predicate.And(ResolvePredicate(expression));
                break;

              case LogicalOperator.AndNot:
              case LogicalOperator.Not:
                if (predicate == null)
                  predicate = PredicateBuilder.True<IDataObject>();
                predicate = predicate.And(ResolvePredicate(expression));
                predicate = LINQ.Expression.Lambda<Func<IDataObject, bool>>(LINQ.Expression.Not(predicate.Body), predicate.Parameters[0]);
                break;

              case LogicalOperator.Or:
                if (predicate == null)
                  predicate = PredicateBuilder.False<IDataObject>();
                predicate = predicate.Or(ResolvePredicate(expression));
                break;

              case LogicalOperator.OrNot:
                if (predicate == null)
                  predicate = PredicateBuilder.False<IDataObject>();
                predicate = predicate.Or(ResolvePredicate(expression));
                predicate = LINQ.Expression.Lambda<Func<IDataObject, bool>>(LINQ.Expression.Not(predicate.Body), predicate.Parameters[0]);
                break;
            }

            if (groupLevel > 0)
            {
              predicate = predicate.And(ToPredicate(groupLevel));
            }
          }
        }

        if (predicate == null)
          predicate = PredicateBuilder.True<IDataObject>();

        return predicate;
      }
      catch (Exception ex)
      {
        throw new Exception("Error while generating Predicate.", ex);
      }
    }

    private string ResolveLinqExpression<T>(Expression expression, string objectVariable)
    {
      string propertyName = expression.PropertyName;
      string qualPropertyName = objectVariable + propertyName;
      Type propertyType = typeof(T).GetProperty(propertyName).PropertyType;
      bool isString = (propertyType == typeof(string));
      StringBuilder linqExpression = new StringBuilder();

      if (expression.LogicalOperator != LogicalOperator.None)
      {
        string logicalOperator = ResolveLogicalOperator(expression.LogicalOperator);
        linqExpression.Append(" " + logicalOperator + " ");
      }

      for (int i = 0; i < expression.OpenGroupCount; i++)
        linqExpression.Append("(");

      switch (expression.RelationalOperator)
      {
        case RelationalOperator.StartsWith:
          if (!isString) throw new Exception("StartsWith operator used with non-string property");
          linqExpression.Append(qualPropertyName + ".StartsWith(\"" + expression.Values.FirstOrDefault() + "\")");
          break;

        case RelationalOperator.Contains:
          if (!isString) throw new Exception("Contains operator used with non-string property");
          linqExpression.Append(qualPropertyName + ".Contains(\"" + expression.Values.FirstOrDefault() + "\")");
          break;

        case RelationalOperator.EndsWith:
          if (!isString) throw new Exception("EndsWith operator used with non-string property");
          linqExpression.Append(qualPropertyName + ".EndsWith(\"" + expression.Values.FirstOrDefault() + "\")");
          break;

        case RelationalOperator.In:
          if (isString)
            linqExpression.Append("(" + qualPropertyName + "=\"" + String.Join("\" OR " + qualPropertyName + "=\"", expression.Values.ToArray()) + "\")");
          else
            linqExpression.Append("(" + qualPropertyName + "=" + String.Join(" OR " + qualPropertyName + "=", expression.Values.ToArray()) + ")");
          break;

        case RelationalOperator.EqualTo:
          if (isString)
            linqExpression.Append(qualPropertyName + "=\"" + expression.Values.FirstOrDefault() + "\"");
          else
            linqExpression.Append(qualPropertyName + "=" + expression.Values.FirstOrDefault() + "");
          break;

        case RelationalOperator.NotEqualTo:
          if (isString)
            linqExpression.Append(qualPropertyName + "<>\"" + expression.Values.FirstOrDefault() + "\"");
          else
            linqExpression.Append(qualPropertyName + "<>" + expression.Values.FirstOrDefault() + "");
          break;

        case RelationalOperator.GreaterThan:
          if (isString)
            linqExpression.Append(qualPropertyName + ".CompareTo(\"" + expression.Values.FirstOrDefault() + "\")>0");
          else
            linqExpression.Append(qualPropertyName + ">" + expression.Values.FirstOrDefault());
          break;

        case RelationalOperator.GreaterThanOrEqual:
          if (isString)
            linqExpression.Append(qualPropertyName + ".CompareTo(\"" + expression.Values.FirstOrDefault() + "\")>=0");
          else
            linqExpression.Append(qualPropertyName + ">=" + expression.Values.FirstOrDefault());
          break;

        case RelationalOperator.LesserThan:
          if (isString)
            linqExpression.Append(qualPropertyName + ".CompareTo(\"" + expression.Values.FirstOrDefault() + "\")<0");
          else
            linqExpression.Append(qualPropertyName + "<" + expression.Values.FirstOrDefault());
          break;

        case RelationalOperator.LesserThanOrEqual:
          if (isString)
            linqExpression.Append(qualPropertyName + ".CompareTo(\"" + expression.Values.FirstOrDefault() + "\")<=0");
          else
            linqExpression.Append(qualPropertyName + "<=" + expression.Values.FirstOrDefault());
          break;

        default:
          throw new Exception("Relational operator does not exist.");
      }

      for (int i = 0; i < expression.CloseGroupCount; i++)
        linqExpression.Append(")");

      return linqExpression.ToString();
    }

    private LINQ.Expression<Func<IDataObject, bool>> ResolvePredicate(Expression expression)
    {
      string propertyName = expression.PropertyName;

      if (_dataObjectDefinition == null)
        throw new Exception("");

#if !SILVERLIGHT
      DataProperty dataProperty =
        _dataObjectDefinition.dataProperties.Find(
          o => o.propertyName.ToUpper() == propertyName.ToUpper()
        );
#else
      DataProperty dataProperty = null;
      foreach (DataProperty o in _dataObjectDefinition.dataProperties)
      {
        if (o.propertyName.ToUpper() == propertyName.ToUpper())
        {
          dataProperty = o;
          break;
        }
      }
#endif


      if (dataProperty == null)
        throw new Exception("");

      DataType propertyType = dataProperty.dataType;

      bool isString = (propertyType == DataType.String || propertyType == DataType.Char);
      bool isBoolean = propertyType == DataType.Boolean;

      switch (expression.RelationalOperator)
      {
        case RelationalOperator.StartsWith:
          if (!isString) throw new Exception("StartsWith operator used with non-string property");

          if (expression.IsCaseSensitive)
          {
            return o => o.GetPropertyValue(dataProperty.propertyName).ToString().StartsWith(expression.Values.FirstOrDefault());
          }
          else
          {
            return o => o.GetPropertyValue(dataProperty.propertyName).ToString().ToUpper().StartsWith(expression.Values.FirstOrDefault().ToUpper());
          }

        case RelationalOperator.Contains:
          if (!isString) throw new Exception("Contains operator used with non-string property");

          if (expression.IsCaseSensitive)
          {
            return o => o.GetPropertyValue(dataProperty.propertyName).ToString().Contains(expression.Values.FirstOrDefault());
          }
          else
          {
            return o => o.GetPropertyValue(dataProperty.propertyName).ToString().ToUpper().Contains(expression.Values.FirstOrDefault().ToUpper());
          }

        case RelationalOperator.EndsWith:
          if (!isString) throw new Exception("EndsWith operator used with non-string property");

          if (expression.IsCaseSensitive)
          {
            return o => o.GetPropertyValue(dataProperty.propertyName).ToString().EndsWith(expression.Values.FirstOrDefault());
          }
          else
          {
            return o => o.GetPropertyValue(dataProperty.propertyName).ToString().ToUpper().EndsWith(expression.Values.FirstOrDefault().ToUpper());
          }

        case RelationalOperator.In:
          if (expression.IsCaseSensitive)
          {
            if (!isString) throw new Exception("Case Sensitivity is not available with this operator and propertyType.");

            return o => expression.Values.Contains(o.GetPropertyValue(dataProperty.propertyName).ToString());
          }
          else
          {
            return o => expression.Values.Contains(o.GetPropertyValue(dataProperty.propertyName).ToString(), new GenericDataComparer(propertyType));
          }

        case RelationalOperator.EqualTo:
          if (expression.IsCaseSensitive)
          {
            if (!isString) throw new Exception("Case Sensitivity is not available with this operator and propertyType.");

            return o => o.GetPropertyValue(dataProperty.propertyName).ToString().Equals(expression.Values.FirstOrDefault());
          }
          else
          {
            GenericDataComparer comparer = new GenericDataComparer(propertyType);
            return o => comparer.Equals(o.GetPropertyValue(dataProperty.propertyName).ToString(), expression.Values.FirstOrDefault());
          }

        case RelationalOperator.NotEqualTo:
          if (expression.IsCaseSensitive)
          {
            if (!isString) throw new Exception("Case Sensitivity is not available with this operator and propertyType.");

            return o => !o.GetPropertyValue(dataProperty.propertyName).ToString().Equals(expression.Values.FirstOrDefault());
          }
          else
          {
            GenericDataComparer comparer = new GenericDataComparer(propertyType);
            return o => !comparer.Equals(o.GetPropertyValue(dataProperty.propertyName).ToString(), expression.Values.FirstOrDefault());
          }

        case RelationalOperator.GreaterThan:
          if (expression.IsCaseSensitive)
          {
            if (!isString) throw new Exception("Case Sensitivity is not available with this operator and propertyType.");

            return o => o.GetPropertyValue(dataProperty.propertyName).ToString().CompareTo(expression.Values.FirstOrDefault()) == 1;
          }
          else
          {
            if (!isBoolean) throw new Exception("GreaterThan operator cannot be used with Boolean property");

            GenericDataComparer comparer = new GenericDataComparer(propertyType);
            return o => comparer.Compare(o.GetPropertyValue(dataProperty.propertyName).ToString(), expression.Values.FirstOrDefault()) == 1;
          }

        case RelationalOperator.GreaterThanOrEqual:
          if (expression.IsCaseSensitive)
          {
            if (!isString) throw new Exception("Case Sensitivity is not available with this operator and propertyType.");

            return o => o.GetPropertyValue(dataProperty.propertyName).ToString().CompareTo(expression.Values.FirstOrDefault()) == 1 ||
                        o.GetPropertyValue(dataProperty.propertyName).ToString().CompareTo(expression.Values.FirstOrDefault()) == 0;
          }
          else
          {
            if (!isBoolean) throw new Exception("GreaterThan operator cannot be used with Boolean property");

            GenericDataComparer comparer = new GenericDataComparer(propertyType);
            return o => comparer.Compare(o.GetPropertyValue(dataProperty.propertyName).ToString(), expression.Values.FirstOrDefault()) == 1 ||
                        comparer.Compare(o.GetPropertyValue(dataProperty.propertyName).ToString(), expression.Values.FirstOrDefault()) == 0;
          }

        case RelationalOperator.LesserThan:
          if (expression.IsCaseSensitive)
          {
            if (!isString) throw new Exception("Case Sensitivity is not available with this operator and propertyType.");

            return o => o.GetPropertyValue(dataProperty.propertyName).ToString().CompareTo(expression.Values.FirstOrDefault()) == -1;
          }
          else
          {
            if (!isBoolean) throw new Exception("LesserThan operator cannot be used with Boolean property");

            GenericDataComparer comparer = new GenericDataComparer(propertyType);
            return o => comparer.Compare(o.GetPropertyValue(dataProperty.propertyName).ToString(), expression.Values.FirstOrDefault()) == -1;
          }

        case RelationalOperator.LesserThanOrEqual:
          if (expression.IsCaseSensitive)
          {
            if (!isString) throw new Exception("Case Sensitivity is not available with this operator and propertyType.");

            return o => o.GetPropertyValue(dataProperty.propertyName).ToString().CompareTo(expression.Values.FirstOrDefault()) == -1 ||
                        o.GetPropertyValue(dataProperty.propertyName).ToString().CompareTo(expression.Values.FirstOrDefault()) == 0;
          }
          else
          {
            if (!isBoolean) throw new Exception("GreaterThan operator cannot be used with Boolean property");

            GenericDataComparer comparer = new GenericDataComparer(propertyType);
            return o => comparer.Compare(o.GetPropertyValue(dataProperty.propertyName).ToString(), expression.Values.FirstOrDefault()) == -1 ||
                        comparer.Compare(o.GetPropertyValue(dataProperty.propertyName).ToString(), expression.Values.FirstOrDefault()) == 0;
          }

        default:
          throw new Exception("Relational operator does not exist.");
      }
    }
  }

  public class GenericDataComparer : IEqualityComparer<string>, IComparer<string>
  {
    private DataType _dataType { get; set; }

    public GenericDataComparer(DataType dataType)
    {
      _dataType = dataType;
    }

    // Implement the IComparable interface. 
    public bool Equals(string str1, string str2)
    {
      if (str1 != null && str2 != null)
        return Compare(str1, str2) == 0;
      return false;
    }

    public int Compare(string str1, string str2)
    {
      switch (_dataType)
      {
        case DataType.Boolean:
          bool bool1 = false;
          Boolean.TryParse(str1, out bool1);

          bool bool2 = false;
          Boolean.TryParse(str2, out bool1);

          if (Boolean.Equals(bool1, bool2))
          {
            return 0;
          }
          else if (bool1)
          {
            return 1;
          }
          else
          {
            return -1;
          }

        case DataType.Byte:
          byte byte1 = 0;
          Byte.TryParse(str1, out byte1);

          byte byte2 = 0;
          Byte.TryParse(str2, out byte2);

          if (byte1 == byte2)
          {
            return 0;
          }
          else if (byte1 > byte2)
          {
            return 1;
          }
          else
          {
            return -1;
          }

        case DataType.Char:
          char char1 = Char.MinValue;
          Char.TryParse(str1, out char1);

          char char2 = Char.MinValue;
          Char.TryParse(str2, out char2);

          if (char1 == char2)
          {
            return 0;
          }
          else if (char1 > char2)
          {
            return 1;
          }
          else
          {
            return -1;
          }

        case DataType.DateTime:
          DateTime dateTime1 = DateTime.MinValue;
          DateTime.TryParse(str1, out dateTime1);

          DateTime dateTime2 = DateTime.MinValue;
          DateTime.TryParse(str2, out dateTime2);

          return DateTime.Compare(dateTime1, dateTime2);

        case DataType.Decimal:
          decimal decimal1 = 0;
          Decimal.TryParse(str1, out decimal1);

          decimal decimal2 = 0;
          Decimal.TryParse(str2, out decimal2);

          return Decimal.Compare(decimal1, decimal2);

        case DataType.Double:
          double double1 = 0;
          Double.TryParse(str1, out double1);

          double double2 = 0;
          Double.TryParse(str2, out double2);

          if (Double.Equals(double1, double2))
          {
            return 0;
          }
          else if (double1 > double2)
          {
            return 1;
          }
          else
          {
            return -1;
          }

        case DataType.Int16:
          Int16 int161 = 0;
          Int16.TryParse(str1, out int161);

          Int16 int162 = 0;
          Int16.TryParse(str2, out int162);

          if (Int16.Equals(int161, int162))
          {
            return 0;
          }
          else if (int161 > int162)
          {
            return 1;
          }
          else
          {
            return -1;
          }

        case DataType.Int32:
          int int1 = 0;
          Int32.TryParse(str1, out int1);

          int int2 = 0;
          Int32.TryParse(str2, out int2);

          if (Int32.Equals(int1, int2))
          {
            return 0;
          }
          else if (int1 > int2)
          {
            return 1;
          }
          else
          {
            return -1;
          }

        case DataType.Int64:
          Int64 int641 = 0;
          Int64.TryParse(str1, out int641);

          Int64 int642 = 0;
          Int64.TryParse(str2, out int642);

          if (Int16.Equals(int641, int642))
          {
            return 0;
          }
          else if (int641 > int642)
          {
            return 1;
          }
          else
          {
            return -1;
          }

        case DataType.Single:
          Single single1 = 0;
          Single.TryParse(str1, out single1);

          Single single2 = 0;
          Single.TryParse(str2, out single2);

          if (Single.Equals(single1, single2))
          {
            return 0;
          }
          else if (single1 > single2)
          {
            return 1;
          }
          else
          {
            return -1;
          }

        //Case Insensitive!
        case DataType.String:
#if !SILVERLIGHT
          return String.Compare(str1, str2, true);
#else
          return String.Compare(str1, str2, StringComparison.InvariantCultureIgnoreCase);
#endif

        default:
          throw new Exception("Invalid property datatype.");
      }
    }

    public int GetHashCode(string obj)
    {
      throw new NotImplementedException();
    }
  }

  [DataContract(Namespace = "http://www.iringtools.org/data/filter", Name = "expression")]
  public class Expression
  {
    public Expression()
    {
      Values = new Values();
    }

    [DataMember(Name = "openGroupCount", Order = 0, EmitDefaultValue = false)]
    public int OpenGroupCount { get; set; }

    [DataMember(Name = "propertyName", Order = 1, IsRequired = true)]
    public string PropertyName { get; set; }

    [DataMember(Name = "relationalOperator", Order = 2, IsRequired = true)]
    public RelationalOperator RelationalOperator { get; set; }

    [DataMember(Name = "values", Order = 3, IsRequired = true)]
    public Values Values { get; set; }

    [DataMember(Name = "logicalOperator", Order = 4, EmitDefaultValue = false)]
    public LogicalOperator LogicalOperator { get; set; }

    [DataMember(Name = "closeGroupCount", Order = 5, EmitDefaultValue = false)]
    public int CloseGroupCount { get; set; }

    [DataMember(Name = "isCaseSensitive", Order = 6, EmitDefaultValue = false)]
    public bool IsCaseSensitive { get; set; }
  }

  [CollectionDataContract(Namespace = "http://www.iringtools.org/data/filter", Name = "values", ItemName = "value")]
  public class Values : List<string> { }

  [DataContract(Namespace = "http://www.iringtools.org/data/filter", Name = "orderExpression")]
  public class OrderExpression
  {
    [DataMember(Name = "propertyName", Order = 0, IsRequired = true)]
    public string PropertyName { get; set; }

    [DataMember(Name = "sortOrder", Order = 1, EmitDefaultValue = false)]
    public SortOrder SortOrder { get; set; }
  }

  [DataContract(Namespace = "http://www.iringtools.org/data/filter", Name = "logicalOperator")]
  public enum LogicalOperator
  {
    [EnumMember]
    None,
    [EnumMember]
    And,
    [EnumMember]
    Or,
    [EnumMember]
    Not,
    [EnumMember]
    AndNot,
    [EnumMember]
    OrNot,
  }

  [DataContract(Namespace = "http://www.iringtools.org/data/filter", Name = "relationalOperator")]
  public enum RelationalOperator
  {
    [EnumMember]
    EqualTo,
    [EnumMember]
    NotEqualTo,
    [EnumMember]
    StartsWith,
    [EnumMember]
    EndsWith,
    [EnumMember]
    Contains,
    [EnumMember]
    In,
    [EnumMember]
    GreaterThan,
    [EnumMember]
    GreaterThanOrEqual,
    [EnumMember]
    LesserThan,
    [EnumMember]
    LesserThanOrEqual,
  }

  [DataContract(Namespace = "http://www.iringtools.org/data/filter", Name = "sortOrder")]
  public enum SortOrder
  {
    [EnumMember]
    Asc,
    [EnumMember]
    Desc,
  }
}
