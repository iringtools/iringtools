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
using Ninject;
using Ninject.Extensions.Xml;

namespace iringtools.sdk.sp3ddatalayer
{
    [TestFixture]
    public class SP3DDataLayerTest
    {
        private string _baseDirectory = string.Empty;
        private IKernel _kernel = null;
        private NameValueCollection _settings;
        private AdapterSettings _adapterSettings;
        private SP3DDataLayer _sp3dDataLayer;

        public SP3DDataLayerTest()
        {
            _settings = new NameValueCollection();

            _settings["XmlPath"] = @".\12345_000\";
            _settings["ProjectName"] = "12345_000";
            _settings["ApplicationName"] = "SP3D";
            
            _baseDirectory = Directory.GetCurrentDirectory();
            _baseDirectory = _baseDirectory.Substring(0, _baseDirectory.LastIndexOf("\\bin"));
            _settings["BaseDirectoryPath"] = _baseDirectory;
            Directory.SetCurrentDirectory(_baseDirectory);

            _adapterSettings = new AdapterSettings();
            _adapterSettings.AppendSettings(_settings);

            string appSettingsPath = String.Format("{0}12345_000.SP3D.config",
                _adapterSettings["XmlPath"]
            );

            if (File.Exists(appSettingsPath))
            {
                AppSettingsReader appSettings = new AppSettingsReader(appSettingsPath);
                _adapterSettings.AppendSettings(appSettings);
            }

            var ninjectSettings = new NinjectSettings { LoadExtensions = false };
            _kernel = new StandardKernel(ninjectSettings);

            _kernel.Load(new XmlExtensionModule());

            string relativePath = String.Format(@"{0}BindingConfiguration.{1}.{2}.xml",
            _settings["XmlPath"],
            _settings["ProjectName"],
            _settings["ApplicationName"]
          );

            //Ninject Extension requires fully qualified path.
            string bindingConfigurationPath = Path.Combine(
              _settings["BaseDirectoryPath"],
              relativePath
            );

            _kernel.Load(bindingConfigurationPath);

           // _sp3dDataLayer = _kernel.Get<SP3DDataLayer>(); This will reset the new updated adaptersettings with default values.
            
            _sp3dDataLayer = new SP3DDataLayer(_adapterSettings);
        }

        [Test]
        public void Create()
        {
            IList<string> identifiers = new List<string>() { 
                "Equip-001", 
                "Equip-002",
                "Equip-00323", 
                "Equip-004"
            };

            Random random = new Random();

            IList<IDataObject> dataObjects = _sp3dDataLayer.Create("Equipment", identifiers);
            foreach (IDataObject dataObject in dataObjects)
            {
                dataObject.SetPropertyValue("PumpType", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("PumpDriverType", "PDT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("DesignTemp", (double)random.Next(2, 10));
                dataObject.SetPropertyValue("DesignPressure", (double)random.Next(2, 10));
                dataObject.SetPropertyValue("Capacity", (double)random.Next(2, 10));
                dataObject.SetPropertyValue("SpecificGravity", (double)random.Next(2, 10));
                dataObject.SetPropertyValue("DifferentialPressure", (double)random.Next(2, 10));
            }
            Response actual = _sp3dDataLayer.Post(dataObjects);

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
                "Equip-001", 
                "Equip-002", 
                "Equip-003", 
                "Equip-004" 
            };

            IList<IDataObject> dataObjects = _sp3dDataLayer.Get("Equipment", identifiers);

            if (!(dataObjects.Count() > 0))
            {
                throw new AssertionException("No Rows returned.");
            }

            foreach (IDataObject dataObject in dataObjects)
            {
                Assert.IsNotNull(dataObject.GetPropertyValue("PumpType"));
                Assert.IsNotNull(dataObject.GetPropertyValue("PumpDriverType"));
                Assert.IsNotNull(dataObject.GetPropertyValue("DesignTemp"));
                Assert.IsNotNull(dataObject.GetPropertyValue("DesignPressure"));
                Assert.IsNotNull(dataObject.GetPropertyValue("Capacity"));
                Assert.IsNotNull(dataObject.GetPropertyValue("SpecificGravity"));
                Assert.IsNotNull(dataObject.GetPropertyValue("DifferentialPressure"));
            }
        }

    }
}
