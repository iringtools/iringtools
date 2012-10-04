using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using org.iringtools.library;
using org.iringtools.adapter;
using System.Reflection;
using org.iringtools.utility;

namespace org.iringtools.sdk.sql
{
  public class Program
  {
    static IDataLayer2 sampleDL = null;

    static void Main(string[] args)
    {
      AdapterSettings settings = new AdapterSettings();
      settings["WhereClauseAlias"] = "_t";
      sampleDL = new SampleSQLDataLayer(settings);

      #region test get
      // from iRING service point of view
      IList<IDataObject> dataObjects = sampleDL.Get("LINES", null);
      
      Console.WriteLine("Object Count: " + dataObjects.Count);
      foreach (IDataObject dataObject in dataObjects)
      {
        DebugDataObject(dataObject, "LINES");
      }
      #endregion

      #region test post
      foreach (IDataObject dataObject in dataObjects)
      {
        ((GenericDataObject)dataObject).ObjectType = "LINES";

        // make tag different by appending timestamp on it
        string tag = dataObject.GetPropertyValue("TAG").ToString();
        string newTag = tag + DateTime.Now.ToString();

        if (newTag.Length > 100)
          newTag = newTag.Substring(0, 100);

        dataObject.SetPropertyValue("TAG", newTag);
      }

      // from iRING service point of view
      Response response = sampleDL.Post(dataObjects);

      Console.WriteLine("Post result: " + response.Level.ToString());
      #endregion

      Console.WriteLine("Test completed!");
      Console.ReadKey();
    }

    static void DebugDataObject(IDataObject dataObject, string objectTypeName)
    {
      DataObject objDef = ((SampleSQLDataLayer)sampleDL).GetObjectDefinition(objectTypeName);

      foreach (DataProperty prop in objDef.dataProperties)
      {
        string propName = prop.propertyName;
        Console.WriteLine(propName + ": " + Convert.ToString(dataObject.GetPropertyValue(propName)));
      }
    }
  }
}
