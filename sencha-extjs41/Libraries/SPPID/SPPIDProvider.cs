using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using org.iringtools.adapter;
using org.iringtools.utility;
using org.iringtools.library;

namespace org.iringtools.adapter.datalayer
{
    class SPPIDProvider : IDisposable
    {
        private AdapterSettings _settings = null;
        private string _configurationPath = string.Empty;
        private SPPIDConfiguration _configuration = null;

        public SPPIDProvider(SPPIDConfiguration configuration)
        {
            InitializeProvider(configuration);
        }
        public SPPIDProvider(AdapterSettings settings)
        {
            _settings = settings;
            _configurationPath = Path.Combine(_settings["AppDataPath"], "SPPID-configuration." + _settings["Scope"] + ".xml");

            if (File.Exists(_configurationPath))
            {
                InitializeProvider(Utility.Read<SPPIDConfiguration>(_configurationPath));

                //if (_configuration.Generate)
                //{
                // _configuration = ProcessConfiguration(_configuration, null);
                //  _configuration.Generate = false;
                //  Utility.Write<SPPIDConfiguration>(_configuration, _configurationPath, true);
                // }
            }
        }

        public void InitializeProvider(SPPIDConfiguration configuration)
        {
            if (configuration != null)
            {
                _configuration = configuration;
                //if (File.Exists(_configuration.Location))
                //{
                // // if (_stream == null) _stream = OpenStream(_configuration.Location);
                //  if (_document == null) _document = GetDocument(_configuration.Location);
                //  if (_configuration.Generate)
                //  {
                //    _configuration = ProcessConfiguration(_configuration, null);
                //    _configuration.Generate = false;
                //    Utility.Write<SPPIDConfiguration>(_configuration, _configurationPath, true);
                //  }
                //}
            }
        }

        public void Dispose()
        {
            
        }

    }
}
