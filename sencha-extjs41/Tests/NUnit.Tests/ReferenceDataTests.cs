using org.iringtools.adapter;
using org.iringtools.refdata;
using org.iringtools.library;
using org.iringtools.utility;
using org.ids_adi.qmxf;
using System.IO;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using System.Xml;
using System.Linq;
using System.Xml.Linq;
using System;
using StaticDust.Configuration;
using NUnit.Framework;
using org.iringtools.refdata.federation;

namespace NUnit.Tests
{
    [TestFixture]
    public class ReferenceDataTests
    {
        private ReferenceDataProvider _refdataProvider = null;
        private ReferenceDataSettings _settings = null;
        private string _baseDirectory = string.Empty;

        public ReferenceDataTests()
        {
          _baseDirectory = Directory.GetCurrentDirectory();
          _baseDirectory = _baseDirectory.Substring(0, _baseDirectory.LastIndexOf("\\Bin"));
          Directory.SetCurrentDirectory(_baseDirectory);
          AdapterSettings adapterSettings = new AdapterSettings();
          adapterSettings.AppendSettings(new AppSettingsReader("App.config"));
          _settings = new ReferenceDataSettings();
          _settings.AppendSettings((ServiceSettings)adapterSettings);
          _settings["BaseDirectoryPath"] = _baseDirectory;
          _refdataProvider = new ReferenceDataProvider(_settings);
        }

        //public ReferenceDataTests()
        //{
        //    _settings = new ReferenceDataSettings();
        //    _settings.AppendSettings(ConfigurationManager.AppSettings);
        //  //  _settings["BaseDirectoryPath"] = @"E:\iring-tools\branches\2.0.x\Tests\NUnit.Tests";
        //    _baseDirectory = Directory.GetCurrentDirectory();
        //    _baseDirectory = _baseDirectory.Substring(0, _baseDirectory.LastIndexOf("\\Bin"));
        //    _settings["BaseDirectoryPath"] = _baseDirectory;
        //    Directory.SetCurrentDirectory(_baseDirectory);
        //    _refdataProvider = new ReferenceDataProvider(_settings);
        //}

        [Test]
        public void Search()
        {
            RefDataEntities _entities = _refdataProvider.Search("possible individual");
            Assert.AreNotEqual(0, _entities.Total);

        }

        [Test]
        public void GetClassLabel()
        {
            Entity entity = _refdataProvider.GetClassLabel("R99781532089");
            StringAssert.IsMatch("ISO 15926-4 POSSIBLE INDIVIDUAL", entity.Label);
        }

        [Test]
        public void GetRepositories()
        {
            List<Repository> repositories = _refdataProvider.GetRepositories();
            Assert.AreNotEqual(0, repositories.Count);
        }

        [Test]
        public void GetClass()
        {
          QMXF qmxf = _refdataProvider.GetClass("R99781532089");
          Assert.IsNotNull(qmxf);
        }

        //[Test]
        //public void PostClass()
        //{
        //    ClassDefinition _classdef = null;
        //    Classification _classif = null;
        //    Specialization _spec = null;

        //    QMXF qmxf = new QMXF();
        //    foreach (Repository rep in _refdataProvider.GetRepositories())
        //    {
        //        if (rep.isReadOnly == false)
        //        {
        //            qmxf.targetRepository = rep.name;
        //            break;
        //        }
        //    }
        //    _classdef = new ClassDefinition();
        //    _classif = new Classification();
        //    _spec = new Specialization();

        //    _classif.label = "ISO 15926-4 INDIVIDUAL CLASS";
        //    _classif.reference = "http://rdl.rdlfacade.org/data#R82407290725";
        //    Description descr = new Description();
        //    descr.value = "A 15926-4 possible individual is a 15926-4 thing that exists in space and time. This includes: - things where any of the space time dimensions are vanishingly small, - those that are either all space for any time, or all time and any space, - the entirety of all space time - things that actually exist, or have existed, - things that are fictional or conjectured and possibly exist in the past, present or future, - temporal parts (states) of other individuals, - things that have a specific position, but zero extent in one or more dimensions, such as points, lines, and surfaces. In this context existence is based upon being imaginable within some consistent logic, including actual, hypothetical, planned, expected, or required individuals.";
        //    EntityType et = new EntityType();
        //    et.reference = "http://dm.rdlfacade.org/data#ClassOfIndividual";
        //    _classdef.classification.Add(_classif);

        //    _spec.label = "ISO 15926-4 THING";
        //    _spec.reference = "http://rdl.rdlfacade.org/data#R95758656562";
        //    _classdef.specialization.Add(_spec);

        //    _classdef.description.Add(descr);
        //    _classdef.entityType = et;
        //    _classdef.identifier = "http://rdl.rdlfacade.org/data#R99781532089";


        //    qmxf.classDefinitions.Add(_classdef);

        //    Response response = _refdataProvider.PostClass(qmxf);
        //    Assert.AreEqual(StatusLevel.Success, response.Level);
        //    Assert.AreNotEqual(0, response.StatusList.Count);
        //}

        [Test]
        public void GetTemplate()
        {
            QMXF qmxf = _refdataProvider.GetTemplate("R89987134385");
            Assert.IsNotNull(qmxf);
        }
    }
}
