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


namespace org.iringtools.sdk.objects.test
{
    [TestFixture]
    public class ObjectDataLayerTest
    {
        private string _baseDirectory = string.Empty;
        private NameValueCollection _settings;
        private AdapterSettings _adapterSettings;
        private ObjectDataLayer _dataLayer;

        public ObjectDataLayerTest()
        {
            _settings = new NameValueCollection();

            _settings["ProjectName"] = "12345_000";
            _settings["XmlPath"] = @"..\ObjectDataLayer.NUnit\12345_000\";
            _settings["ApplicationName"] = "OBJ";
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

            _dataLayer = new ObjectDataLayer(_adapterSettings);
        }

        [Test]
        public void Create()
        {
            Random random = new Random();

            IList<IDataObject> dataObjects = _dataLayer.Create("Widget", null);
            foreach (IDataObject dataObject in dataObjects)
            {
                dataObject.SetPropertyValue("Name", "Widget-" + random.Next(2, 10));
                dataObject.SetPropertyValue("Description", "This is Widget #" + random.Next(2, 10));
                dataObject.SetPropertyValue("Length", (double)random.Next(2, 10));
                dataObject.SetPropertyValue("Width", (double)random.Next(2, 10));
                dataObject.SetPropertyValue("Height", (double)random.Next(2, 10));
                dataObject.SetPropertyValue("Weight", (double)random.Next(2, 10));
                dataObject.SetPropertyValue("LengthUOM", "inches");
                dataObject.SetPropertyValue("WeightUOM", "pounds");
                dataObject.SetPropertyValue("Material", "Wood");
                dataObject.SetPropertyValue("Color", "Red");
            }
            Response actual = _dataLayer.Post(dataObjects);

            if (actual.Level != StatusLevel.Success)
            {
                throw new AssertionException(Utility.SerializeDataContract<Response>(actual));
            }

            Assert.IsTrue(actual.Level == StatusLevel.Success);
        }

        [Test]
        public void Read()
        {
            IList<string> identifiers = new List<string>() 
            { 
                "1", 
                "2",
            };

            IList<IDataObject> dataObjects = _dataLayer.Get("Widget", identifiers);

            if (!(dataObjects.Count() > 0))
            {
                throw new AssertionException("No Rows returned.");
            }

            foreach (IDataObject dataObject in dataObjects)
            {
              Assert.IsNotNull(dataObject.GetPropertyValue("Name"));
              Assert.IsNotNull(dataObject.GetPropertyValue("Description"));
              Assert.IsNotNull(dataObject.GetPropertyValue("Length"));
              Assert.IsNotNull(dataObject.GetPropertyValue("Width"));
              Assert.IsNotNull(dataObject.GetPropertyValue("Height"));
              Assert.IsNotNull(dataObject.GetPropertyValue("Weight"));
              Assert.IsNotNull(dataObject.GetPropertyValue("LengthUOM"));
              Assert.IsNotNull(dataObject.GetPropertyValue("WeightUOM"));
              Assert.IsNotNull(dataObject.GetPropertyValue("Material"));
              Assert.IsNotNull(dataObject.GetPropertyValue("Color"));
            }
        }

        [Test]
        public void ReadWithPaging()
        {
          IList<IDataObject> dataObjects = _dataLayer.Get("Widget", null, 2, 0);

          if (!(dataObjects.Count() > 0))
          {
            throw new AssertionException("No Rows returned.");
          }

          Assert.AreEqual(dataObjects.Count(), 2);

          foreach (IDataObject dataObject in dataObjects)
          {
            Assert.IsNotNull(dataObject.GetPropertyValue("Name"));
            Assert.IsNotNull(dataObject.GetPropertyValue("Description"));
            Assert.IsNotNull(dataObject.GetPropertyValue("Color"));
            Assert.IsNotNull(dataObject.GetPropertyValue("Material"));
            Assert.IsNotNull(dataObject.GetPropertyValue("Length"));
            Assert.IsNotNull(dataObject.GetPropertyValue("Height"));
            Assert.IsNotNull(dataObject.GetPropertyValue("Width"));
            Assert.IsNotNull(dataObject.GetPropertyValue("LengthUOM"));
            Assert.IsNotNull(dataObject.GetPropertyValue("Weight"));
            Assert.IsNotNull(dataObject.GetPropertyValue("WeightUOM"));
          }
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
                        PropertyName = "Material",
                        RelationalOperator = RelationalOperator.Contains,
                        Values = new Values
                        {
                            "Wood",
                        }
                    }
                }
          };

          IList<IDataObject> dataObjects = _dataLayer.Get("Widget", dataFilter, 10, 0);

          if (!(dataObjects.Count() > 0))
          {
            throw new AssertionException("No Rows returned.");
          }

          //TODO: Need to implement filtering in Provider.
          //Assert.AreEqual(1, dataObjects.Count());

          foreach (IDataObject dataObject in dataObjects)
          {
            Assert.IsNotNull(dataObject.GetPropertyValue("Name"));
            Assert.IsNotNull(dataObject.GetPropertyValue("Description"));
            //TODO: Need to implement filtering in Provider.
            Assert.IsTrue(dataObject.GetPropertyValue("Material").ToString().Contains("Wood"));
            Assert.IsNotNull(dataObject.GetPropertyValue("Color"));
            Assert.IsNotNull(dataObject.GetPropertyValue("Material"));
            Assert.IsNotNull(dataObject.GetPropertyValue("Length"));
            Assert.IsNotNull(dataObject.GetPropertyValue("Height"));
            Assert.IsNotNull(dataObject.GetPropertyValue("Width"));
            Assert.IsNotNull(dataObject.GetPropertyValue("LengthUOM"));
            Assert.IsNotNull(dataObject.GetPropertyValue("Weight"));
            Assert.IsNotNull(dataObject.GetPropertyValue("WeightUOM"));
          }
        }

        [Test]
        public void GetDictionary()
        {
            DataDictionary benchmark = null;

            DataDictionary dictionary = _dataLayer.GetDictionary();

            Assert.IsNotNull(dictionary);

            string path = String.Format("{0}DataDictionary.{1}.{2}.xml",
                  _adapterSettings["XmlPath"],
                  _adapterSettings["ProjectName"],
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
    }
}
