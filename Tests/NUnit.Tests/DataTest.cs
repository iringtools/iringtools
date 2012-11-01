using System;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Xml.Linq;
using NUnit.Framework;
using org.iringtools.adapter;
using org.iringtools.library;
using org.iringtools.utility;

namespace NUnit.Tests
{
  [TestFixture]
  public class DataTest : BaseTest
  {
    private AdapterProvider _adapterProvider = null;
    private AdapterSettings _settings = null;
    private string _baseDirectory = string.Empty;

    public DataTest()
    {
      _settings = new AdapterSettings();
      _settings.AppendSettings(ConfigurationManager.AppSettings);

      //_settings["BaseDirectoryPath"] = @"E:\iring-tools\branches\2.0.x\Tests\NUnit.Tests";
      _settings["ProjectName"] = "12345_000";
      _settings["ApplicationName"] = "ABC";
      _settings["GraphName"] = "Lines";
      _settings["Identifier"] = "90002-RV";
      _settings["ClassName"] = "PIPINGNETWORKSYSTEM";

      _baseDirectory = Directory.GetCurrentDirectory();
      _baseDirectory = _baseDirectory.Substring(0, _baseDirectory.LastIndexOf("\\Bin"));
      _settings["BaseDirectoryPath"] = _baseDirectory;
      Directory.SetCurrentDirectory(_baseDirectory);
      _settings["GraphBaseUri"] = "http://www.example.com/";

      _adapterProvider = new AdapterProvider(_settings);

      string scopesPath = String.Format("{0}Scopes.xml", _settings["AppDataPath"]);

      Resource importScopes = Utility.Read<Resource>(scopesPath);
      _adapterProvider.SetScopes(importScopes);

      ResetDatabase();
    }

    [Test]
    public void GetIndividual()
    {
      XDocument benchmark = null;

      string format = "Xml";

      object response =
        _adapterProvider.GetDataProjection(
          _settings["ProjectName"], _settings["ApplicationName"],
          _settings["GraphName"], _settings["ClassName"],
          _settings["Identifier"],
          ref format, false
        );

      XDocument xDocument = (XDocument)response;

      string path = String.Format(
         "{0}GetIndividual.xml",
         _settings["AppDataPath"]
       );

      xDocument.Save(path);
      benchmark = XDocument.Load(path);
      Assert.AreEqual(benchmark.ToString(), xDocument.ToString());
     
    }

    [Test]
    public void GetIndex()
    {
      XDocument benchmark = null;

      string format = "Xml";

      XDocument xDocument =
        _adapterProvider.GetDataProjection(
          _settings["ProjectName"], _settings["ApplicationName"],
          _settings["GraphName"],
          ref format,
          0, 0, null, null,
          false,
          null
        );

      string path = String.Format(
          "{0}GetIndex.xml",
          _settings["AppDataPath"]
        );

      xDocument.Save(path);      
      benchmark = XDocument.Load(path);
      Assert.AreEqual(benchmark.ToString(), xDocument.ToString());
      
    }

    [Test]
    public void GetFull()
    {
      XDocument benchmark = null;

      string format = "Xml";

      XDocument xDocument =
        _adapterProvider.GetDataProjection(
          _settings["ProjectName"], _settings["ApplicationName"],
          _settings["GraphName"],
          ref format,
          0, 0, null, null,
          true,
          null
        );

      string path = String.Format(
          "{0}GetFull.xml",
          _settings["AppDataPath"]
        );

      xDocument.Save(path);
      benchmark = XDocument.Load(path);
      Assert.AreEqual(benchmark.ToString(), xDocument.ToString());      
    }

    [Test]
    public void GetPageIndex()
    {
      XDocument benchmark = null;

      string format = "Xml";

      XDocument xDocument =
        _adapterProvider.GetDataProjection(
          _settings["ProjectName"], _settings["ApplicationName"],
          _settings["GraphName"],
          ref format,
          0, 5, null, null,
          false,
          null
        );

      string path = String.Format(
          "{0}GetPageIndex.xml",
          _settings["AppDataPath"]
        );

      xDocument.Save(path);
      benchmark = XDocument.Load(path);
      Assert.AreEqual(benchmark.ToString(), xDocument.ToString());
      

      int total = 0;

      if (xDocument.Root.Attribute("total") != null)
        int.TryParse(xDocument.Root.Attribute("total").Value, out total);

      for (int i = 5; i < total; i += 5)
      {
        xDocument =
         _adapterProvider.GetDataProjection(
           _settings["ProjectName"], _settings["ApplicationName"],
           _settings["GraphName"],
           ref format,
           i, 5, null, null,
           false,
           null
         );

        path = String.Format(
            "{0}GetPageIndex.{1}.xml",
            _settings["AppDataPath"],
            (i / 5) + 1
          );

        xDocument.Save(path);
        benchmark = XDocument.Load(path);
        Assert.AreEqual(benchmark.ToString(), xDocument.ToString());       
      }
    }

    [Test]
    public void GetPageFull()
    {
      XDocument benchmark = null;
      
      string format = "Xml";

      XDocument xDocument =
        _adapterProvider.GetDataProjection(
          _settings["ProjectName"], _settings["ApplicationName"],
          _settings["GraphName"],
          ref format,
          0, 5, null, null,
          true,
          null
        );

      string path = String.Format(
          "{0}GetPageFull.xml",
          _settings["AppDataPath"]
        );

      xDocument.Save(path);
      benchmark = XDocument.Load(path);
      Assert.AreEqual(benchmark.ToString(), xDocument.ToString());
      

      int total = 0;

      if (xDocument.Root.Attribute("total") != null)
        int.TryParse(xDocument.Root.Attribute("total").Value, out total);

      for (int i = 5; i < total; i += 5)
      {
        xDocument =
         _adapterProvider.GetDataProjection(
           _settings["ProjectName"], _settings["ApplicationName"],
           _settings["GraphName"],
           ref format,
           i, 5, null, null,
           true,
           null
         );

        path = String.Format(
            "{0}GetPageFull.{1}.xml",
            _settings["AppDataPath"],
            (i / 5) + 1
          );

        xDocument.Save(path);
        benchmark = XDocument.Load(path);
        Assert.AreEqual(benchmark.ToString(), xDocument.ToString());        
      }
    }

    [Test]
    public void GetFilterIndex()
    {
      NameValueCollection parameters = new NameValueCollection();

      parameters.Add("System", "SC");

      XDocument benchmark = null;

      string format = "Xml";

      XDocument xDocument =
        _adapterProvider.GetDataProjection(
          _settings["ProjectName"], _settings["ApplicationName"],
          _settings["GraphName"],
          ref format,
          0, 0, null, null,
          false,
          parameters
        );

      string path = String.Format(
          "{0}GetFilterIndex.xml",
          _settings["AppDataPath"]
        );

      xDocument.Save(path);
      benchmark = XDocument.Load(path);
      Assert.AreEqual(benchmark.ToString(), xDocument.ToString());
     
    }

    [Test]
    public void GetFilterFull()
    {
      NameValueCollection parameters = new NameValueCollection();

      parameters.Add("System", "SC");

      XDocument benchmark = null;

      string format = "Xml";

      XDocument xDocument =
        _adapterProvider.GetDataProjection(
          _settings["ProjectName"], _settings["ApplicationName"],
          _settings["GraphName"],
          ref format,
          0, 0, null, null,
          true,
          parameters
        );

      string path = String.Format(
          "{0}GetFilterFull.xml",
          _settings["AppDataPath"]
        );

      xDocument.Save(path);
      benchmark = XDocument.Load(path);
      Assert.AreEqual(benchmark.ToString(), xDocument.ToString());
      
    }

    [Test]
    public void GetSortIndex()
    {
      NameValueCollection parameters = new NameValueCollection();

      XDocument benchmark = null;

      string format = "Xml";

      XDocument xDocument =
        _adapterProvider.GetDataProjection(
          _settings["ProjectName"], _settings["ApplicationName"],
          _settings["GraphName"],
          ref format,
          0, 0, "Asc", "System",
          false,
          parameters
        );

      string path = String.Format(
          "{0}GetSortIndex.xml",
          _settings["AppDataPath"]
        );

      xDocument.Save(path);
      benchmark = XDocument.Load(path);
      Assert.AreEqual(benchmark.ToString(), xDocument.ToString());      
    }

    [Test]
    public void GetSortFull()
    {
      NameValueCollection parameters = new NameValueCollection();

      XDocument benchmark = null;

      string format = "Xml";

      XDocument xDocument =
        _adapterProvider.GetDataProjection(
          _settings["ProjectName"], _settings["ApplicationName"],
          _settings["GraphName"],
          ref format,
          0, 0, "Asc", "System",
          true,
          parameters
        );

      string path = String.Format(
          "{0}GetSortFull.xml",
          _settings["AppDataPath"]
        );

      xDocument.Save(path);
      benchmark = XDocument.Load(path);
      Assert.AreEqual(benchmark.ToString(), xDocument.ToString());
      
    }

    [Test]
    public void GetFilterPageSortIndex()
    {
      NameValueCollection parameters = new NameValueCollection();

      parameters.Add("Area", "90");

      XDocument benchmark = null;

      string format = "Xml";

      XDocument xDocument =
        _adapterProvider.GetDataProjection(
          _settings["ProjectName"], _settings["ApplicationName"],
          _settings["GraphName"],
          ref format,
          0, 5, "Desc", "System",
          false,
          parameters
        );

      string path = String.Format(
          "{0}GetFilterPageSortIndex.xml",
          _settings["AppDataPath"]
        );

      xDocument.Save(path);
      benchmark = XDocument.Load(path);
      Assert.AreEqual(benchmark.ToString(), xDocument.ToString());
      

      int total = 0;

      if (xDocument.Root.Attribute("total") != null)
        int.TryParse(xDocument.Root.Attribute("total").Value, out total);

      for (int i = 5; i < total; i += 5)
      {
        xDocument =
         _adapterProvider.GetDataProjection(
           _settings["ProjectName"], _settings["ApplicationName"],
           _settings["GraphName"],
           ref format,
           i, 5, "Desc", "System",
           false,
           parameters
         );

        path = String.Format(
            "{0}GetFilterPageSortIndex.{1}.xml",
            _settings["AppDataPath"],
            (i / 5) + 1
          );

        xDocument.Save(path);
        benchmark = XDocument.Load(path);
        Assert.AreEqual(benchmark.ToString(), xDocument.ToString());          
      }
    }

    //[Test]
    //public void GetFilterPageSortFull()
    //{
    //  NameValueCollection parameters = new NameValueCollection();

    //  parameters.Add("Area", "90");

    //  XDocument benchmark = null;

    //  string format = "Xml";

    //  XDocument xDocument =
    //    _adapterProvider.GetDataProjection(
    //      _settings["ProjectName"], _settings["ApplicationName"],
    //      _settings["GraphName"],
    //      ref format,
    //      0, 5, "Desc", "System",
    //      true,
    //      parameters
    //    );

    //  string path = String.Format(
    //      "{0}GetFilterPageSortFull.xml",
    //      _settings["AppDataPath"]
    //    );

    //  if (_settings["TestMode"].ToLower() != "usefiles")
    //  {
    //    xDocument.Save(path);
    //    Assert.AreNotEqual(null, xDocument);
    //  }
    //  else
    //  {
    //    benchmark = XDocument.Load(path);
    //    Assert.AreEqual(benchmark.ToString(), xDocument.ToString());
    //  }

    //  int total = 0;

    //  if (xDocument.Root.Attribute("total") != null)
    //    int.TryParse(xDocument.Root.Attribute("total").Value, out total);

    //  for (int i = 5; i < total; i += 5)
    //  {
    //    xDocument =
    //     _adapterProvider.GetDataProjection(
    //       _settings["ProjectName"], _settings["ApplicationName"],
    //       _settings["GraphName"],
    //       ref format,
    //       i, 5, "Desc", "System",
    //       true,
    //       parameters
    //     );

    //    path = String.Format(
    //        "{0}GetFilterPageSortFull.{1}.xml",
    //        _settings["AppDataPath"],
    //        (i / 5) + 1
    //      );

    //    if (_settings["TestMode"].ToLower() != "usefiles")
    //    {
    //      xDocument.Save(path);
    //      Assert.AreNotEqual(null, xDocument);
    //    }
    //    else
    //    {
    //      benchmark = XDocument.Load(path);
    //      Assert.AreEqual(benchmark.ToString(), xDocument.ToString());
    //    }
    //  }
    //}

    //[Test]
    //public void GetDataFilterIndex()
    //{
    //  DataFilter filter = new DataFilter();

    //  Expression expression = new Expression
    //  {
    //    PropertyName = "System",
    //    Values = new Values
    //     {
    //       "SC"
    //     },
    //    RelationalOperator = RelationalOperator.EqualTo
    //  };

    //  filter.Expressions.Add(expression);

    //  XDocument benchmark = null;

    //  string format = "Xml";

    //  XDocument xDocument =
    //    _adapterProvider.GetDataProjection(
    //      _settings["ProjectName"], _settings["ApplicationName"],
    //      _settings["GraphName"],
    //      filter, ref format,
    //      0, 0,
    //      false
    //    );

    //  string path = String.Format(
    //      "{0}GetDataFilterIndex.xml",
    //      _settings["AppDataPath"]
    //    );

    //  if (_settings["TestMode"].ToLower() != "usefiles")
    //  {
    //    xDocument.Save(path);
    //    Assert.AreNotEqual(null, xDocument);
    //  }
    //  else
    //  {
    //    benchmark = XDocument.Load(path);
    //    Assert.AreEqual(benchmark.ToString(), xDocument.ToString());
    //  }
    //}

    //[Test] Move to JUnit
    //public void GetDataFilterFull()
    //{
    //  DataFilter filter = new DataFilter();

    //  Expression expression = new Expression
    //  {
    //    PropertyName = "System",
    //    Values = new Values
    //     {
    //       "SC"
    //     },
    //    RelationalOperator = RelationalOperator.EqualTo
    //  };

    //  filter.Expressions.Add(expression);

    //  XDocument benchmark = null;

    //  string format = "Xml";

    //  XDocument xDocument =
    //    _adapterProvider.GetDataProjection(
    //      _settings["ProjectName"], _settings["ApplicationName"],
    //      _settings["GraphName"],
    //      filter, ref format,
    //      0, 0,
    //      true
    //    );

    //  string path = String.Format(
    //      "{0}GetDataFilterFull.xml",
    //      _settings["AppDataPath"]
    //    );

    //  if (_settings["TestMode"].ToLower() != "usefiles")
    //  {
    //    xDocument.Save(path);
    //    Assert.AreNotEqual(null, xDocument);
    //  }
    //  else
    //  {
    //    benchmark = XDocument.Load(path);
    //    Assert.AreEqual(benchmark.ToString(), xDocument.ToString());
    //  }
    //}

    //[Test]
    //public void GetDataFilterPageSortIndex()
    //{
    //  DataFilter filter = new DataFilter();

    //  Expression expression = new Expression
    //  {
    //    PropertyName = "Area",
    //    Values = new Values
    //     {
    //       "90"
    //     },
    //    RelationalOperator = RelationalOperator.EqualTo
    //  };

    //  filter.Expressions.Add(expression);

    //  OrderExpression orderExpression = new OrderExpression
    //  {
    //    PropertyName = "System",
    //    SortOrder = SortOrder.Desc
    //  };

    //  filter.OrderExpressions.Add(orderExpression);

    //  XDocument benchmark = null;

    //  string format = "Xml";

    //  XDocument xDocument =
    //    _adapterProvider.GetDataProjection(
    //      _settings["ProjectName"], _settings["ApplicationName"],
    //      _settings["GraphName"],
    //      filter, ref format,
    //      0, 5,
    //      false
    //    );

    //  string path = String.Format(
    //      "{0}GetDataFilterPageSortIndex.xml",
    //      _settings["AppDataPath"]
    //    );

    //  if (_settings["TestMode"].ToLower() != "usefiles")
    //  {
    //    xDocument.Save(path);
    //    Assert.AreNotEqual(null, xDocument);
    //  }
    //  else
    //  {
    //    benchmark = XDocument.Load(path);
    //    Assert.AreEqual(benchmark.ToString(), xDocument.ToString());
    //  }

    //  int total = 0;

    //  if (xDocument.Root.Attribute("total") != null)
    //    int.TryParse(xDocument.Root.Attribute("total").Value, out total);

    //  for (int i = 5; i < total; i += 5)
    //  {
    //    //the Provider clears the orderExpressions for getCount
    //    //not a problem for webservice, but we need to handle it here.
    //    filter.OrderExpressions.Add(orderExpression);

    //    xDocument =
    //    _adapterProvider.GetDataProjection(
    //      _settings["ProjectName"], _settings["ApplicationName"],
    //      _settings["GraphName"],
    //      filter, ref format,
    //      i, 5,
    //      false
    //    );

    //    path = String.Format(
    //        "{0}GetDataFilterPageSortIndex.{1}.xml",
    //        _settings["AppDataPath"],
    //        (i / 5) + 1
    //      );

    //    if (_settings["TestMode"].ToLower() != "usefiles")
    //    {
    //      xDocument.Save(path);
    //      Assert.AreNotEqual(null, xDocument);
    //    }
    //    else
    //    {
    //      benchmark = XDocument.Load(path);
    //      Assert.AreEqual(benchmark.ToString(), xDocument.ToString());
    //    }
    //  }
    //}

    //[Test]
    //public void GetDataFilterPageSortFull()
    //{
    //  DataFilter filter = new DataFilter();

    //  Expression expression = new Expression
    //  {
    //    PropertyName = "Area",
    //    Values = new Values
    //     {
    //       "90"
    //     },
    //    RelationalOperator = RelationalOperator.EqualTo
    //  };

    //  filter.Expressions.Add(expression);

    //  OrderExpression orderExpression = new OrderExpression
    //  {
    //    PropertyName = "System",
    //    SortOrder = SortOrder.Desc
    //  };

    //  filter.OrderExpressions.Add(orderExpression);

    //  XDocument benchmark = null;
      
    //  string format = "Xml";

    //  XDocument xDocument =
    //    _adapterProvider.GetDataProjection(
    //      _settings["ProjectName"], _settings["ApplicationName"],
    //      _settings["GraphName"],
    //      filter, ref format,
    //      10, 5,
    //      true
    //    );

    //  string path = String.Format(
    //      "{0}GetDataFilterPageSortFull.xml",
    //      _settings["AppDataPath"]
    //    );

    //  if (_settings["TestMode"].ToLower() != "usefiles")
    //  {
    //    xDocument.Save(path);
    //    Assert.AreNotEqual(null, xDocument);
    //  }
    //  else
    //  {
    //    benchmark = XDocument.Load(path);
    //    Assert.AreEqual(benchmark.ToString(), xDocument.ToString());
    //  }

    //  int total = 0;

    //  if (xDocument.Root.Attribute("total") != null)
    //    int.TryParse(xDocument.Root.Attribute("total").Value, out total);

    //  for (int i = 5; i < total; i += 5)
    //  {
    //    //the Provider clears the orderExpressions for getCount
    //    //not a problem for webservice, but we need to handle it here.
    //    filter.OrderExpressions.Add(orderExpression);

    //    xDocument =
    //    _adapterProvider.GetDataProjection(
    //      _settings["ProjectName"], _settings["ApplicationName"],
    //      _settings["GraphName"],
    //      filter, ref format,
    //      i, 5,
    //      true
    //    );

    //    path = String.Format(
    //        "{0}GetDataFilterPageSortFull.{1}.xml",
    //        _settings["AppDataPath"],
    //        (i / 5) + 1
    //      );

    //    if (_settings["TestMode"].ToLower() != "usefiles")
    //    {
    //      xDocument.Save(path);
    //      Assert.AreNotEqual(null, xDocument);
    //    }
    //    else
    //    {
    //      benchmark = XDocument.Load(path);
    //      Assert.AreEqual(benchmark.ToString(), xDocument.ToString());
    //    }
    //  }
    //}
  }
}

