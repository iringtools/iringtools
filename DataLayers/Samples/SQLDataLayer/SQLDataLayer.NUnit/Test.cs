using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using NUnit.Framework;
using org.iringtools.adapter;
using org.iringtools.library;
using org.iringtools.utility;
using StaticDust.Configuration;
using org.iringtools.sdk.sql;
using System.Data;

namespace org.iringtools.sdk.sql.test
{
    [TestFixture]
    public class SQLDataLayerTest
    {
        private string _baseDirectory = string.Empty;
        private NameValueCollection _settings;
        private AdapterSettings _adapterSettings;
        private SQLDataLayer _dataLayer;

        public SQLDataLayerTest()
        {
            _settings = new NameValueCollection();

            _settings["ProjectName"] = "12345_000";
            _settings["XmlPath"] = @"..\SQLDataLayer.NUnit\12345_000\";
            _settings["ApplicationName"] = "SQL";
            _settings["TestMode"] = "WriteFiles"; //UseFiles/WriteFiles

            _baseDirectory = Directory.GetCurrentDirectory();
            _baseDirectory = _baseDirectory.Substring(0, _baseDirectory.LastIndexOf("\\bin"));
            _settings["BaseDirectoryPath"] = _baseDirectory;
            Directory.SetCurrentDirectory(_baseDirectory);

            _adapterSettings = new AdapterSettings();
            _adapterSettings.AppendSettings(_settings);

            string appSettingsPath = String.Format("{0}{1}.{2}.config",
                _adapterSettings["XmlPath"],
                _settings["ProjectName"],
                _settings["ApplicationName"]
            );

            if (File.Exists(appSettingsPath))
            {
                AppSettingsReader appSettings = new AppSettingsReader(appSettingsPath);
                _adapterSettings.AppendSettings(appSettings);
            }

            _dataLayer = new SQLDataLayer(_adapterSettings);
        }

        [Test]
        public void ReadWithIdentifiers()
        {
            IList<string> identifiers = new List<string>() 
            { 
                "66015-O", 
                "90003-SC",
            };

            IList<IDataObject> dataObjects = _dataLayer.Get("Lines", identifiers);

            if (!(dataObjects.Count() > 0))
            {
                throw new AssertionException("No Rows returned.");
            }

            foreach (IDataObject dataObject in dataObjects)
            {

            }
        }

        [Test]
        public void ReadWithPaging()
        {
            DataFilter filter = new DataFilter();
            IList<IDataObject> dataObjects = _dataLayer.Get("Lines", filter , 2, 0);

            if (!(dataObjects.Count() > 0))
            {
                throw new AssertionException("No Rows returned.");
            }

            Assert.AreEqual(dataObjects.Count(), 2);

            foreach (IDataObject dataObject in dataObjects)
            {
            }
        }

        [Test]
        public void TestAll()
        {
            
            IDataObject dataObject = _dataLayer.Get("Lines", new DataFilter(), 1, 0).First();   
            string identifier = dataObject.GetPropertyValue("TAG").ToString();

            Response actual = _dataLayer.Delete("Lines", new List<string> { identifier });

            ((GenericDataObject)dataObject).ObjectType = "Lines";
            _dataLayer.Post(new List<IDataObject> {dataObject});

            Assert.IsTrue(actual.Level == StatusLevel.Success);
        }

        [Test]
        public void ReadWithFilter()
        {
            DataFilter dataFilter = new DataFilter
            {
                Expressions = new List<Expression>
                {
                    new Expression
                    {
                        PropertyName = "TAG",
                        RelationalOperator = RelationalOperator.EqualTo,
                        Values = new Values
                        {
                            "90003-SL",
                        }
                    }
                }
            };
            IList<IDataObject> dataObjects = _dataLayer.Get("Lines", dataFilter, 10, 0);

            if (!(dataObjects.Count() > 0))
            {
                throw new AssertionException("No Rows returned.");
            }

            Assert.AreEqual(dataObjects.Count(), 1);

            foreach (IDataObject dataObject in dataObjects)
            {
            }
        }

        [Test]
        public void GetDictionary()
        {
            DataDictionary benchmark = null;

            DataDictionary dictionary = _dataLayer.GetDictionary();

            Assert.IsNotNull(dictionary);

            string path = String.Format("{0}DataDictionary.{1}.xml",
                  _adapterSettings["XmlPath"],
                  _adapterSettings["ApplicationName"]
                );

            if (_settings["TestMode"].ToLower() != "usefiles")
            {
              Utility.Write<DataDictionary>(dictionary, path);
              Assert.AreNotEqual(null, dictionary);
            }
            else
            {
              benchmark = Utility.Read<DataDictionary>(path);
              Assert.AreEqual(
                Utility.SerializeDataContract<DataDictionary>(benchmark), 
                Utility.SerializeDataContract<DataDictionary>(dictionary)
              );
            }
        }

        [Test]
        public void DeleteDataTable()
        {
            DataFilter dataFilter = new DataFilter
            {
                Expressions = new List<Expression>
                {
                    new Expression
                    {
                        PropertyName = "TAG",
                        RelationalOperator = RelationalOperator.EqualTo,
                        Values = new Values
                        {
                            "Hello",
                        }
                    }
                }
            };
            Response actual = _dataLayer.Delete("Lines", dataFilter);

            if (actual.Level != StatusLevel.Success)
            {
                throw new AssertionException(Utility.SerializeDataContract<Response>(actual));
            }

            Assert.IsTrue(actual.Level == StatusLevel.Success);
        }

        [Test]
        public void DeleteWithIdentifiers()
        {
            IList<string> identifiers = new List<string>() 
            { 
                "Hello2", 
                "Hello1",
            };

            Response actual = _dataLayer.Delete("Lines", identifiers);

            if (actual.Level != StatusLevel.Success)
            {
                throw new AssertionException(Utility.SerializeDataContract<Response>(actual));
            }

            Assert.IsTrue(actual.Level == StatusLevel.Success);
        }

        [Test]
        public void Read()
        {
            DataFilter filter = new DataFilter();

            IList<string> identifiers = _dataLayer.GetIdentifiers("Lines", filter);

            if (!(identifiers.Count() > 0))
            {
                throw new AssertionException("No Rows returned.");
            }

            Assert.AreEqual(identifiers.Count(), 32);

        }
        
        [Test]
        public void GetCount()
        {
            DataFilter dataFilter = new DataFilter
            {
                Expressions = new List<Expression>
                {
                    new Expression
                    {
                        PropertyName = "TAG",
                        RelationalOperator = RelationalOperator.EqualTo,
                        Values = new Values
                        {
                            "90003-SL",
                        }
                    }
                }
            };
            long count = _dataLayer.GetCount("Lines", dataFilter);

            if (count == 0)
            {
                throw new AssertionException("No Rows returned.");
            }

            Assert.AreEqual(count, 1);

        }

        [Test]
        public void CreateDataTable()
        {

            IList<string> identifiers = new List<string>() 
            { 
                "66015-O", 
                "90003-S",
            };

            IList<IDataObject> dataObjects = _dataLayer.Create("Lines", identifiers);

            if (!(dataObjects.Count() > 0))
            {
                throw new AssertionException("No Rows returned.");
            }

        }

        [Test]
        public void GetRelatedDataTables()
        {

            IDataObject dataObject = _dataLayer.Get("Lines", new DataFilter(), 1, 0).First();
            
            ((GenericDataObject)dataObject).ObjectType = "Lines";

            IList<IDataObject> dataObjects = _dataLayer.GetRelatedObjects(dataObject, "Valves");
           
            if (!(dataObjects.Count() > 0))
            {
                throw new AssertionException("No Rows returned.");
            }
            Assert.Greater(dataObjects.Count, 0);
        }


    }
}
