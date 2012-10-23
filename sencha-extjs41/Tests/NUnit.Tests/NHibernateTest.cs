using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using NUnit.Framework;
using org.iringtools.adapter;
using org.iringtools.library;
using org.iringtools.nhibernate;
using org.iringtools.utility;

namespace NUnit.Tests
{
  [TestFixture]
  public class NHibernateTest
  {
    private NHibernateProvider _hibernateProvider = null;
    private AdapterSettings _settings = null;
    private string _baseDirectory = string.Empty;

    public NHibernateTest()
    {
      _settings = new AdapterSettings();
      _settings.AppendSettings(ConfigurationManager.AppSettings);

      //_settings["BaseDirectoryPath"] = @"E:\iring-tools\branches\2.0.x\Tests\NUnit.Tests";
      _settings["ProjectName"] = "12345_000";
      _settings["ApplicationName"] = "ABC";
      _settings["SchemaObjectName"] = "LINES";
      _settings["ExecutingAssemblyName"] = "NUnit.Tests";

      _baseDirectory = Directory.GetCurrentDirectory();
      _baseDirectory = _baseDirectory.Substring(0, _baseDirectory.LastIndexOf("\\Bin"));
      _settings["BaseDirectoryPath"] = _baseDirectory;
      Directory.SetCurrentDirectory(_baseDirectory);

      _hibernateProvider = new NHibernateProvider(_settings);
    }

    [Test]
    public void GetProviders()
    {
      DataProviders providers = _hibernateProvider.GetProviders();

      Assert.AreNotEqual(0, providers.Count);
    }

    [Test]
    public void GetRelationships()
    {
      DataRelationships relationships = _hibernateProvider.GetRelationships();

      Assert.AreNotEqual(0, relationships.Count);
    }

    [Test]
    public void GetDictionary()
    {
      DatabaseDictionary dictionary = _hibernateProvider.GetDictionary(
        _settings["ProjectName"], _settings["ApplicationName"]);

      Assert.AreNotEqual(0, dictionary.dataObjects.Count);
    }

    [Test]
    public void GetSchemaObjects()
    {
      List<string> schemaObjects = _hibernateProvider.GetSchemaObjects(
        _settings["ProjectName"], _settings["ApplicationName"]);

      Assert.AreNotEqual(0, schemaObjects.Count);
    }

    [Test]
    public void GetSchemaObjectSchema()
    {
      DataObject schemaObjectSchema = _hibernateProvider.GetSchemaObjectSchema(
        _settings["ProjectName"], _settings["ApplicationName"],
        _settings["SchemaObjectName"]);

      Assert.AreNotEqual(0, schemaObjectSchema.dataProperties.Count);
    }

    [Test]
    public void PostDictionary()
    {
      string dictionaryPath = String.Format(
          "{0}DatabaseDictionary.{1}.{2}.xml",
          _settings["AppDataPath"],
          _settings["ProjectName"],
          _settings["ApplicationName"]
        );

      DatabaseDictionary dictionary = Utility.Read<DatabaseDictionary>(dictionaryPath);

      List<string> schemaObjects = _hibernateProvider.GetSchemaObjects(
        _settings["ProjectName"], _settings["ApplicationName"]);

      dictionary.dataObjects.Clear();

      foreach (string schemaObjectName in schemaObjects)
      {
        DataObject schemaObject = _hibernateProvider.GetSchemaObjectSchema(
             _settings["ProjectName"], _settings["ApplicationName"],
             schemaObjectName
          );

        dictionary.dataObjects.Add(schemaObject);
      }

      Response response = _hibernateProvider.PostDictionary(
        _settings["ProjectName"], _settings["ApplicationName"],
        dictionary);

      Assert.AreEqual(StatusLevel.Success, response.Level);
      Assert.AreNotEqual(0, response.StatusList.Count);
    }

    [Test]
    public void Generate()
    {
      Response response = _hibernateProvider.Generate(
        _settings["ProjectName"], _settings["ApplicationName"]);

      Assert.AreEqual(StatusLevel.Success, response.Level);
      Assert.AreNotEqual(0, response.StatusList.Count);
    }
  }
}

