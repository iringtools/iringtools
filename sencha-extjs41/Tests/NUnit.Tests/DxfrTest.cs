using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using log4net;
using NUnit.Framework;
using org.iringtools.adapter;
using org.iringtools.dxfr.manifest;
using org.iringtools.library;
using org.iringtools.utility;
using Ninject;
using Ninject.Extensions.Xml;
using org.iringtools.adapter.identity;
using org.iringtools.nhibernate;

namespace NUnit.Tests
{
    [TestFixture]
    public class DxfrTest : BaseTest
    {
	    private static readonly ILog _logger = LogManager.GetLogger(typeof(DxfrTest));      
      private AdapterSettings _settings = null;
      private string _baseDirectory = string.Empty;
      private DataTranferProvider _dxfrProvider = null;			

      public DxfrTest()
      {
        _settings = new AdapterSettings();
        _settings.AppendSettings(ConfigurationManager.AppSettings);

        _settings["ProjectName"] = "12345_000";
        _settings["ApplicationName"] = "ABC";
        _settings["GraphName"] = "Lines";
        _settings["Identifier"] = "90002-RV";
        _settings["ExecutingAssemblyName"] = "NUnit.Tests";
        _settings["GraphBaseUri"] = "http://www.example.com/";
				_baseDirectory = Directory.GetCurrentDirectory();
				_baseDirectory = _baseDirectory.Substring(0, _baseDirectory.LastIndexOf("\\Bin"));
				_settings["BaseDirectoryPath"] = _baseDirectory;       
				Directory.SetCurrentDirectory(_baseDirectory);
        _dxfrProvider = new DataTranferProvider(_settings);

        string scopesPath = String.Format("{0}Scopes.xml", _settings["AppDataPath"]);

        Resource importScopes = Utility.Read<Resource>(scopesPath);
        _dxfrProvider.setScopes(importScopes);

        ResetDatabase();
      }			

			private XDocument ToXml<T>(T dataList)
			{
				XDocument xDocument = null;

				try
				{
					string xml = Utility.SerializeDataContract<T>(dataList);
					XElement xElement = XElement.Parse(xml);
					xDocument = new XDocument(xElement);
				}
				catch (Exception ex)
				{
					_logger.Error("Error transferring data list to xml." + ex);
				}

				return xDocument;
			}

			private void deleteHashValue(ref DataTransferIndices dtiList)
			{
				foreach (DataTransferIndex dti in dtiList.DataTransferIndexList)
				{
					dti.HashValue = null;
				}
			}			      

      [Test]
      public void GetManifest()
      {
        XDocument benchmark = null;
        Manifest manifest = null;

        manifest = _dxfrProvider.GetManifest(_settings["ProjectName"], _settings["ApplicationName"]);

        string path = String.Format(
          "{0}DxfrGetManifest.xml",
            _settings["AppDataPath"]
            );

        XDocument xDocument = ToXml(manifest);
        xDocument.Save(path);
        Assert.AreNotEqual(null, xDocument);

        String manifestString = ToXml(manifest).ToString();
        benchmark = XDocument.Load(path);
        String benchmarkString = benchmark.ToString();
        Assert.AreEqual(manifestString, benchmarkString);		
      }


      [Test]
      public void GetDataTransferIndicesWithManifest()
      {
        XDocument benchmark = null;
        DataFilter filter = new DataFilter();
        filter.Expressions = null;
        filter.OrderExpressions = null;
        DataTransferIndices dtiList = null;
        Manifest manifest = null;

        manifest = _dxfrProvider.GetManifest(_settings["ProjectName"], _settings["ApplicationName"]);

        dtiList =
          _dxfrProvider.GetDataTransferIndicesWithManifest(
            _settings["ProjectName"], _settings["ApplicationName"],
            _settings["GraphName"], "MD5", manifest);

        deleteHashValue(ref dtiList);

        string path = String.Format(
          "{0}DxfrGetDataTransferIndicesWithManifest.xml",
            _settings["AppDataPath"]
            );

        XDocument xDocument = ToXml(dtiList.DataTransferIndexList);
        xDocument.Save(path);
        Assert.AreNotEqual(null, xDocument);

        benchmark = XDocument.Load(path);
        String dtiListString = ToXml(dtiList.DataTransferIndexList).ToString();
        String benchmarkString = benchmark.ToString();
        Assert.AreEqual(dtiListString, benchmarkString);  
      }

      [Test]
      public void GetDataTransferIndicesWithDxiRequest()
      {
        XDocument benchmark = null;
        DxiRequest dxiRequest = new DxiRequest();
        dxiRequest.DataFilter = new DataFilter();
        DataTransferIndices dtiList = null;

        dxiRequest.DataFilter.Expressions.Add(
          new Expression
          {
            PropertyName = "PipingNetworkSystem.NominalDiameter.valValue",
            Values = new Values
                {
                  "80"
                },
            RelationalOperator = RelationalOperator.EqualTo
          }
            );

        dxiRequest.DataFilter.OrderExpressions.Add(
          new OrderExpression
          {
            PropertyName = "PipingNetworkSystem.IdentificationByTag.valIdentifier",
            SortOrder = SortOrder.Asc
          }
        );

        dxiRequest.Manifest = _dxfrProvider.GetManifest(_settings["ProjectName"], _settings["ApplicationName"]);

        dtiList =
          _dxfrProvider.GetDataTransferIndicesByRequest(
            _settings["ProjectName"], _settings["ApplicationName"],
            _settings["GraphName"], "MD5", dxiRequest);

        deleteHashValue(ref dtiList);

        string path = String.Format(
            "{0}DxfrGetDataTransferIndicesByRequest.xml",
            _settings["AppDataPath"]
          );

        XDocument xDocument = ToXml(dtiList.DataTransferIndexList);
        xDocument.Save(path);
        Assert.AreNotEqual(null, xDocument);

        benchmark = XDocument.Load(path);
        String dtiListString = ToXml(dtiList.DataTransferIndexList).ToString();
        String benchmarkString = benchmark.ToString();
        Assert.AreEqual(dtiListString, benchmarkString);
      }

      [Test]
      public void GetDataTransferObjects()
      {
        XDocument benchmark = null;
        DataTransferIndices dtiList = null, dtiPage = new DataTransferIndices();
        DataTransferObjects dtos = null;
        Manifest manifest = null;
        int page = 25;

        manifest = _dxfrProvider.GetManifest(_settings["ProjectName"], _settings["ApplicationName"]);

        dtiList =
          _dxfrProvider.GetDataTransferIndicesWithManifest(
            _settings["ProjectName"], _settings["ApplicationName"],
            _settings["GraphName"], "MD5", manifest);

        dtiPage.DataTransferIndexList = dtiList.DataTransferIndexList.GetRange(0, page);

        dtos = _dxfrProvider.GetDataTransferObjects(_settings["ProjectName"], _settings["ApplicationName"],
            _settings["GraphName"], dtiPage);

        string path = String.Format(
            "{0}DxfrGetDataTransferObjects.xml",
            _settings["AppDataPath"]
          );

        XDocument xDocument = ToXml(dtos.DataTransferObjectList);
        xDocument.Save(path);
        Assert.AreNotEqual(null, xDocument);

        benchmark = XDocument.Load(path);
        String dtosString = ToXml(dtos.DataTransferObjectList).ToString();
        String benchmarkString = benchmark.ToString();
        Assert.AreEqual(dtosString, benchmarkString);  
      }

      [Test]
      public void GetDataTransferObjectsWithDxoRequest()
      {
        XDocument benchmark = null;
        DataTransferIndices dtiList = null;
        DataTransferObjects dtos = null;

        DxiRequest dxiRequest = new DxiRequest();
        dxiRequest.DataFilter = new DataFilter();

        dxiRequest.DataFilter.Expressions.Add(
          new Expression
          {
            PropertyName = "PipingNetworkSystem.NominalDiameter.valValue",
            Values = new Values
                {
                  "80"
                },
            RelationalOperator = RelationalOperator.EqualTo
          }
            );

        dxiRequest.DataFilter.OrderExpressions.Add(
          new OrderExpression
          {
            PropertyName = "PipingNetworkSystem.IdentificationByTag.valIdentifier",
            SortOrder = SortOrder.Asc
          }
        );

        dxiRequest.Manifest = _dxfrProvider.GetManifest(_settings["ProjectName"], _settings["ApplicationName"]);

        dtiList =
          _dxfrProvider.GetDataTransferIndicesByRequest(
            _settings["ProjectName"], _settings["ApplicationName"],
            _settings["GraphName"], "MD5", dxiRequest);

        DxoRequest dxoRequest = new DxoRequest();
        dxoRequest.DataTransferIndices = new DataTransferIndices();
        //int page = 25;

        dxoRequest.Manifest = _dxfrProvider.GetManifest(_settings["ProjectName"], _settings["ApplicationName"]);

        //dxoRequest.DataTransferIndices.DataTransferIndexList = dtiList.DataTransferIndexList.GetRange(0, page);
        dxoRequest.DataTransferIndices = dtiList;

        dtos = _dxfrProvider.GetDataTransferObjects(_settings["ProjectName"], _settings["ApplicationName"],
            _settings["GraphName"], dxoRequest);

        string path = String.Format(
            "{0}DxfrGetDataTransferObjectsWithDxoRequest.xml",
            _settings["AppDataPath"]
          );

        XDocument xDocument = ToXml(dtos.DataTransferObjectList);
        xDocument.Save(path);
        Assert.AreNotEqual(null, xDocument);
        benchmark = XDocument.Load(path);
        String dtosString = ToXml(dtos.DataTransferObjectList).ToString();
        String benchmarkString = benchmark.ToString();
        Assert.AreEqual(dtosString, benchmarkString);			
      }
			
			[Test]
			public void PostDataTransferObjects()
			{
        XDocument benchmark = null;
        Response response = null;
        DxoRequest dxoRequest = new DxoRequest();
        DataTransferObjects postDtos = null;
        List<DataTransferObject> dtoList = null;

        DxiRequest dxiRequest = new DxiRequest();
        dxiRequest.DataFilter = new DataFilter();
        DataTransferIndices dtiList = null;

        dxiRequest.DataFilter.Expressions.Add(
          new Expression
          {
            PropertyName = "PipingNetworkSystem.NominalDiameter.valValue",
            Values = new Values
                {
                  "80"
                },
            RelationalOperator = RelationalOperator.EqualTo
          }
            );

        dxiRequest.DataFilter.OrderExpressions.Add(
          new OrderExpression
          {
            PropertyName = "PipingNetworkSystem.IdentificationByTag.valIdentifier",
            SortOrder = SortOrder.Asc
          }
        );

        dxiRequest.Manifest = _dxfrProvider.GetManifest(_settings["ProjectName"], _settings["ApplicationName"]);

        dtiList =
          _dxfrProvider.GetDataTransferIndicesByRequest(
            _settings["ProjectName"], _settings["ApplicationName"],
            _settings["GraphName"], "MD5", dxiRequest);

        dxoRequest.Manifest = _dxfrProvider.GetManifest(_settings["ProjectName"], _settings["ApplicationName"]);

        dxoRequest.DataTransferIndices = dtiList;        

        postDtos = _dxfrProvider.GetDataTransferObjects(_settings["ProjectName"], _settings["ApplicationName"],
            _settings["GraphName"], dxoRequest);

        dtoList = postDtos.DataTransferObjectList;

        dtoList[0].transferType = TransferType.Delete;
        dtoList[1].classObjects[1].templateObjects[0].roleObjects[2].oldValue = dtoList[1].classObjects[1].templateObjects[0].roleObjects[2].value;
        dtoList[1].classObjects[1].templateObjects[0].roleObjects[2].value = "200";

        string path = String.Format(
            "{0}DxfrNewDto.xml",
            _settings["AppDataPath"]
          );
        benchmark = XDocument.Load(path);

        DataTransferObject newDto = Utility.DeserializeDataContract<DataTransferObject>(benchmark.ToString());

        dtoList.Add(newDto);

        response = _dxfrProvider.PostDataTransferObjects(_settings["ProjectName"], _settings["ApplicationName"],
            _settings["GraphName"], postDtos);

        path = String.Format(
            "{0}DxfrResponse.xml",
            _settings["AppDataPath"]
          );

        XDocument xDocument = ToXml(response);
        xDocument.Save(path);
        Assert.AreNotEqual(null, xDocument);
        benchmark = XDocument.Load(path);

        String res = ToXml(response).ToString();
        Response xmlResponse = Utility.DeserializeDataContract<Response>(benchmark.ToString());

        Assert.AreEqual(response.Level.ToString(), xmlResponse.Level.ToString());
        foreach (Status status in response.StatusList)
          foreach (Status xmlStatus in xmlResponse.StatusList)
          {
            Assert.AreEqual(status.Messages.ToString(), xmlStatus.Messages.ToString());
            Assert.AreEqual(status.Identifier, xmlStatus.Identifier);
            xmlResponse.StatusList.Remove(xmlStatus);
            break;
          }

        //restore the table
        ResetDatabase();
      }

    }
}

