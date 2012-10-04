using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using org.iringtools.utility;
using System.Net;

namespace org.iringtools.sdk.objects.widgets
{
  //This class is not very useful outside of this very simple example
  public static class FleeUtility
  {
    public static string ToLinqExpression<T>(this IList<Filter> filterList, string objectVariable)
    {
      if (filterList == null || filterList.Count == 0)
        return String.Empty;

      if (!String.IsNullOrEmpty(objectVariable)) objectVariable += ".";
      else throw new Exception("Object variable can not be null or empty.");

      try
      {
        StringBuilder linqExpression = new StringBuilder();

        foreach (Filter filter in filterList)
        {
          string exp = ResolveLinqExpression<T>(filter, objectVariable);
          linqExpression.Append(exp);
        }

        return linqExpression.ToString();
      }
      catch (Exception ex)
      {
        throw new Exception("Error while generating LINQ expression.", ex);
      }
    }

    private static string ResolveLinqExpression<T>(Filter filter, string objectVariable)
    {
      string attributeName = filter.AttributeName;
      string qualPropertyName = objectVariable + attributeName;
      PropertyInfo propertyInfo = typeof(T).GetProperty(attributeName);
      if (propertyInfo == null)
        throw new Exception("Filter property does not exist.");

      bool isString = (propertyInfo.PropertyType == typeof(string));

      StringBuilder linqExpression = new StringBuilder();

      if (filter.Logical != String.Empty)
      {
        linqExpression.Append(" " + filter.Logical + " ");
      }

      switch (filter.RelationalOperator.ToUpper())
      {
        case "LIKE":
          if (!isString) throw new Exception("Contains operator used with non-string property");
          linqExpression.Append(qualPropertyName + ".ToUpper().Contains(" + filter.Value.ToUpper() + ")");
          break;

        case "EQUALTO":

              if (isString)
                  linqExpression.Append(qualPropertyName + ".ToUpper()=" + filter.Value.ToUpper());
              else
                  linqExpression.Append(qualPropertyName.ToUpper() + "=" + filter.Value.ToUpper());
         
          break;

        default:
          throw new Exception("Relational operator does not exist.");
      }

      return linqExpression.ToString();
    }
  }
}
