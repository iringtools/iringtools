using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using org.iringtools.adapter;
using org.iringtools.library;
using org.iringtools.utility;
using StaticDust.Configuration;
using System.Data;
using System.Text;
using System.Data.SqlClient;
using Oracle.DataAccess.Client;

namespace org.iringtools.datalayers.IIP.SPPID.Test
{
    [TestFixture]
    public class Tests
    {
        private IDataLayer2 _dataLayer;
        private string _objectType;
        private DataObject _objectDefinition;

        private DataFilter _filter;

        private SqlConnection _projConn;
        private SqlConnection _stageConn; 
        private SqlConnection _siteConn; 

        private OracleConnection _siteConnOracle; 
        private OracleConnection _plantConnOracle;
        private OracleConnection _plantDicConnOracle;
        private OracleConnection _PIDConnOracle;
        private OracleConnection _PIDDicConnOracle;

        public Tests()
        {
            string baseDir = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory(baseDir.Substring(0, baseDir.LastIndexOf("\\bin")));

            AdapterSettings adapterSettings = new AdapterSettings();
            adapterSettings.AppendSettings(new AppSettingsReader("App.config"));

            string sppidConfigFile = String.Format("{0}{1}.{2}.config",
              adapterSettings["AppDataPath"],
              adapterSettings["ProjectName"],
              adapterSettings["ApplicationName"]
            );

            AppSettingsReader sppidSettings = new AppSettingsReader(sppidConfigFile);
            adapterSettings.AppendSettings(sppidSettings);

             
            string  tmp = String.Format("{0}{1}.StagingConfiguration.{2}.xml", 
              adapterSettings["AppDataPath"],
              adapterSettings["ProjectName"],
              adapterSettings["ApplicationName"]
            );


            adapterSettings["StagingConfigurationPath"] = Path.Combine(Directory.GetCurrentDirectory(), tmp);

            adapterSettings["BaseDirectoryPath"] = Directory.GetCurrentDirectory();
        
            //Set Connection strings----------------
            if(adapterSettings["SPPIDPLantConnectionString"].Contains("PROTOCOL") == false)
            {
                _projConn = new SqlConnection(adapterSettings["SPPIDPLantConnectionString"]);

                _siteConn = new SqlConnection(adapterSettings["SPPIDSiteConnectionString"]);
            }
            else
            {
                _plantConnOracle = new OracleConnection(adapterSettings["SPPIDPLantConnectionString"]);
                _siteConnOracle = new OracleConnection(adapterSettings["SPPIDSiteConnectionString"]);
                _plantDicConnOracle = new OracleConnection(adapterSettings["PlantDataDicConnectionString"]);
                _PIDConnOracle = new OracleConnection(adapterSettings["PIDConnectionString"]);
                _PIDDicConnOracle = new OracleConnection(adapterSettings["PIDDataDicConnectionString"]);

                //Set Oracle Stagging Files-------------------------
                  tmp = String.Format("{0}{1}.StagingConfiguration.{2}.{3}.xml", 
                   adapterSettings["AppDataPath"],
                   adapterSettings["ProjectName"],
                   adapterSettings["ApplicationName"],
                   "Oracle"
                   );

                //Update Stagging Configuration Path with Oracle Config file---------------
                adapterSettings["StagingConfigurationPath"] = Path.Combine(Directory.GetCurrentDirectory(), tmp);
            }

            _stageConn = new SqlConnection(adapterSettings["iRingStagingConnectionString"]);


            FileInfo log4netConfig = new FileInfo("log4net.config");
            log4net.Config.XmlConfigurator.Configure(log4netConfig);

            _dataLayer = new SPPIDDataLayer(adapterSettings);
            _dataLayer.GetDictionary();

            _objectType = adapterSettings["ObjectType"];
            _objectDefinition = GetObjectDefinition(_objectType);

            _filter = Utility.Read<DataFilter>(adapterSettings["FilterPath"]);
        }

       // [Test]
        public void TestCreate()
        {
            IList<IDataObject> dataObjects = _dataLayer.Create(_objectType, null);
            Assert.AreNotEqual(dataObjects, null);
        }

        [Test]
        public void TestGetDictionary()
        {
            DataDictionary dictionary = _dataLayer.GetDictionary();
            Assert.IsNotNull(dictionary);
        }

        [Test]
        public void TestGetCount()
        {
            long count = _dataLayer.GetCount(_objectType, new DataFilter());
            Assert.Greater(count, 0);
        }

        [Test]
        public void TestGetPage()
        {
            IList<IDataObject> dataObjects = _dataLayer.Get(_objectType, new DataFilter(), 5, 0);
            Assert.Greater(dataObjects.Count, 0);
        }

        [Test]
        public void TestGetWithIdentifiers()
        {
            IList<string> identifiers = _dataLayer.GetIdentifiers(_objectType, new DataFilter());
            IList<string> identifier = ((List<string>)identifiers).GetRange(1, 1);
            IList<IDataObject> dataObjects = _dataLayer.Get(_objectType, identifier);
            Assert.Greater(dataObjects.Count, 0);
        }

        [Test]
        public void TestGetCountWithFilter()
        {
            long count = _dataLayer.GetCount(_objectType, _filter);
            Assert.Greater(count, 0);
        }

        [Test]
        public void TestGetPageWithFilter()
        {
            IList<IDataObject> dataObjects = _dataLayer.Get(_objectType, _filter, 5, 0);
            Assert.Greater(dataObjects.Count, 0);
        }

        [Test]
        public void TestGetIdentifiersWithFilter()
        {
            IList<string> identifiers = _dataLayer.GetIdentifiers(_objectType, _filter);
            Assert.Greater(identifiers.Count, 0);
        }

        //[Test]
        public void TestPostWithUpdate()
        {
           
        }

       // [Test]
        public void TestPostWithAddAndDeleteByIdentifier()
        {
        }

        //[Test]
        public void TestPostWithAddAndDeleteByFilter()
        {
        }

        [Test]
        public void TestRefresh()
        {
            Response response = _dataLayer.RefreshAll();
            Assert.AreEqual(response.Level, StatusLevel.Success);
        }

        #region Help Methods

        private DataObject GetObjectDefinition(string objectType)
        {
            DataDictionary dictionary = _dataLayer.GetDictionary();

            if (dictionary.dataObjects != null)
            {
                foreach (DataObject dataObject in dictionary.dataObjects)
                {
                    if (dataObject.objectName.ToLower() == objectType.ToLower())
                    {
                        return dataObject;
                    }
                }
            }

            return null;
        }

        #endregion
    }



}
