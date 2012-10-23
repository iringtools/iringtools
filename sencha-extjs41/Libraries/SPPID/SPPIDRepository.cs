using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

using org.iringtools.adapter;
using org.iringtools.library;
using org.iringtools.utility;
using Ninject;
using System.Xml.Linq;



namespace org.iringtools.adapter.datalayer
{
    public interface ISPPIDRepository
    {
        string UpdateConfig(string scope, string application, string datalayer, string siteConString, string plantConString, string stageConString);
    }
    public class SPPIDRepository : ISPPIDRepository
    {
        private AdapterSettings _settings { get; set; }
        private SPPIDProvider _provider { get; set; }
        private WebHttpClient _client { get; set; }

        [Inject]
        public SPPIDRepository()
        {
            _settings = new AdapterSettings();
            _settings.AppendSettings(ConfigurationManager.AppSettings);
            _client = new WebHttpClient(_settings["AdapterServiceUri"]);
        }

        private SPPIDProvider InitializeProvider(SPPIDConfiguration configuration)
        {
            if (_provider == null)
            {
                _provider = new SPPIDProvider(configuration);
            }

            return _provider;
        }

        public string UpdateConfig(string scope, string application, string datalayer, string siteConString, string plantConString, string stageConString)
        {
            List<MultiPartMessage> requestMessages = new List<MultiPartMessage>();

            requestMessages.Add(new MultiPartMessage
            {
                name = "DataLayer",
                message = datalayer,
                type = MultipartMessageType.FormData
            });
            requestMessages.Add(new MultiPartMessage
            {
                name = "SiteConnectionString",
                message = siteConString,
                type = MultipartMessageType.FormData
            });
            requestMessages.Add(new MultiPartMessage
            {
                name = "PlantConnectionString",
                message = plantConString,
                type = MultipartMessageType.FormData
            });
            requestMessages.Add(new MultiPartMessage
            {
                name = "StagingConnectionString",
                message = stageConString,
                type = MultipartMessageType.FormData
            });
            _client.PostMultipartMessage(string.Format("/{0}/{1}/configure", scope, application), requestMessages);

            return "SUCCESS";
        }
    }

}
