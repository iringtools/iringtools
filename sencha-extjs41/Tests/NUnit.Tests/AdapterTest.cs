using org.iringtools.adapter;
using org.iringtools.library;
using org.iringtools.utility;
using System.IO;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using System.Xml;
using System.Linq;
using System.Xml.Linq;
using System;
using System.Configuration;
using NUnit.Framework;
using org.iringtools.mapping;

namespace NUnit.Tests
{
  [TestFixture]
  public class AdapterTest
  {
    private AdapterProvider _adapterProvider = null;
    private AdapterSettings _settings = null;
    private string _baseDirectory = string.Empty;

    public AdapterTest()
    {
      _settings = new AdapterSettings();
      _settings.AppendSettings(ConfigurationManager.AppSettings);

      //_settings["BaseDirectoryPath"] = @"E:\iring-tools\branches\2.0.x\Tests\NUnit.Tests";
      _settings["ProjectName"] = "12345_000";
      _settings["ApplicationName"] = "ABC";
      _settings["GraphName"] = "Lines";

      _baseDirectory = Directory.GetCurrentDirectory();
      _baseDirectory = _baseDirectory.Substring(0, _baseDirectory.LastIndexOf("\\Bin"));
      _settings["BaseDirectoryPath"] = _baseDirectory;
      Directory.SetCurrentDirectory(_baseDirectory);
      _adapterProvider = new AdapterProvider(_settings);

      string scopesPath = String.Format("{0}Scopes.xml", _settings["AppDataPath"]);
      
      Resource importScopes = Utility.Read<Resource>(scopesPath);
      _adapterProvider.SetScopes(importScopes);
    }

    [Test]
    public void GetDataLayers()
    {
      DataLayers dataLayers = _adapterProvider.GetDataLayers();     
      Assert.AreNotEqual(0, dataLayers.Count);
    }

    [Test]
    public void UpdateBinding()
    {
      string bindingPath = String.Format(
         "{0}BindingConfiguration.{1}.{2}.xml",
         _settings["AppDataPath"],
         _settings["ProjectName"], 
         _settings["ApplicationName"]
       );

      XElement binding = XDocument.Load(bindingPath).Root;
      
      Response response = _adapterProvider.UpdateBinding(
        _settings["ProjectName"], _settings["ApplicationName"],
        binding);

      Assert.AreEqual(StatusLevel.Success, response.Level);
      Assert.AreNotEqual(0, response.StatusList.Count);
    }

    [Test]
    public void UpdateMapping()
    {
      string mappingPath = String.Format(
         "{0}Mapping.{1}.{2}.xml",
         _settings["AppDataPath"],
         _settings["ProjectName"],
         _settings["ApplicationName"]
       );

        XElement mapping = XDocument.Load(mappingPath).Root;

        Response response = _adapterProvider.UpdateMapping(
          _settings["ProjectName"], _settings["ApplicationName"],
          mapping);

        Assert.AreEqual(StatusLevel.Success, response.Level);
        Assert.AreNotEqual(0, response.StatusList.Count);
    }

    [Test] 
    public void GetScopes()
    {
      Resource scopes = _adapterProvider.GetScopes();

      Assert.AreNotEqual(0, scopes.Locators.Count);
    }    

    [Test] 
    public void GetBinding()
    {
      XElement binding = _adapterProvider.GetBinding(
        _settings["ProjectName"], _settings["ApplicationName"]);

      Assert.AreNotEqual(0, binding.Elements("bind").Count());
    }

    [Test] 
    public void GetDictionary()
    {
        DataDictionary dictionary = _adapterProvider.GetDictionary(
          _settings["ProjectName"], _settings["ApplicationName"]);

        Assert.AreNotEqual(0, dictionary.dataObjects.Count);
    }

    [Test]
    public void GetMapping()
    {
      Mapping mapping = _adapterProvider.GetMapping(
      _settings["ProjectName"], _settings["ApplicationName"]);

      Assert.AreNotEqual(0, mapping.graphMaps.Count);
    }

    //[Test] Move to JUnit
    //public void UpdateScopes()
    //{
    //  string scopesPath = String.Format(
    //     "{0}Scopes.xml",
    //     _settings["AppDataPath"]
    //   );

    //  ScopeProjects scopes = Utility.Read<ScopeProjects>(scopesPath);

    //  scopes.Clear();

    //  ScopeProject scopeProject = new ScopeProject
    //  {
    //    Name = _settings["ProjectName"],
    //    Description = "Test Project",
    //    Applications = new ScopeApplications
    //    {
    //      new ScopeApplication
    //      {
    //        Name = _settings["ApplicationName"],
    //        Description = "Test Application"
    //      }
    //    }
    //  };

    //  scopes.Add(scopeProject);

    //  Response response = _adapterProvider.UpdateScopes(scopes);

    //  Assert.AreEqual(StatusLevel.Success, response.Level);
    //  Assert.AreNotEqual(0, response.StatusList.Count);
    //}
  }
}

