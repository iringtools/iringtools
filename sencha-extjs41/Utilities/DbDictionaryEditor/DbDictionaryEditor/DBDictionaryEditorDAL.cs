using System;
using System.Net;
using org.iringtools.library;
using System.Text;
using System.Collections.Generic;
using System.ComponentModel;
using org.iringtools.modulelibrary.events;
using org.iringtools.utility;

namespace DbDictionaryEditor
{
    public class DBDictionaryEditorDAL
    {
        public event EventHandler<EventArgs> OnDataArrived;
        public event EventHandler<EventArgs> OnError;

        private WebClient _scopesClient;
        private WebClient _dbDictionaryClient;
        private WebClient _dbschemaClient;
        private WebClient _savedbdictionaryClient;
        private WebClient _dbdictionariesClient;
        private WebClient _providersClient;
        private WebClient _postdbdictionaryClient;
        private WebClient _clearClient;
        private WebClient _deleteClient;
        private WebClient _testClient;

        private string _dbDictionaryServiceUri;
        private string _adapterServiceUri;

        public DBDictionaryEditorDAL() 
        {
            #region Webclients
            _scopesClient = new WebClient();
            _dbDictionaryClient = new WebClient();
            _dbschemaClient = new WebClient();
            _savedbdictionaryClient = new WebClient();
            _dbdictionariesClient = new WebClient();
            _providersClient = new WebClient();
            _postdbdictionaryClient = new WebClient();
            _clearClient = new WebClient();
            _deleteClient = new WebClient();
            _testClient = new WebClient();
            #endregion

            _scopesClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(OnCompletedEvent);
            _dbDictionaryClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(OnCompletedEvent);
            _dbschemaClient.UploadStringCompleted += new UploadStringCompletedEventHandler(OnCompletedEvent);
            _savedbdictionaryClient.UploadStringCompleted += new UploadStringCompletedEventHandler(OnCompletedEvent);
            _dbdictionariesClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(OnCompletedEvent);
            _providersClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(OnCompletedEvent);
            _clearClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(OnCompletedEvent);
            _postdbdictionaryClient.UploadStringCompleted += new UploadStringCompletedEventHandler(OnCompletedEvent);
            _deleteClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(OnCompletedEvent);

            _dbDictionaryServiceUri = App.Current.Resources["DBDictionaryServiceURI"].ToString();
            _adapterServiceUri = App.Current.Resources["AdapterServiceUri"].ToString();
        }

        //public Collection<ScopeProject> GetScopes()
        //{
        //    StringBuilder sb = new StringBuilder();
        //    sb.Append(_adapterServiceUri);
        //    sb.Append("/scopes");
        //    _scopesClient.DownloadStringAsync(new Uri(sb.ToString()));
        //    return null;
        //}

        public DatabaseDictionary GetDbDictionary(string project, string application)
        {
            if (!_dbDictionaryClient.IsBusy)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(_dbDictionaryServiceUri);
                sb.Append("/");
                sb.Append(project);
                sb.Append("/");
                sb.Append(application);
                sb.Append("/dbdictionary");
                _dbDictionaryClient.DownloadStringAsync(new Uri(sb.ToString()));
            }
            return null;
        }

        public Response SaveDatabaseDictionary(DatabaseDictionary dict, string project, string application)
        {
            string message = Utility.SerializeDataContract<DatabaseDictionary>(dict);

            StringBuilder sb = new StringBuilder();
            sb.Append(_dbDictionaryServiceUri);
            sb.Append("/");
            sb.Append(project);
            sb.Append("/");
            sb.Append(application);
            sb.Append("/savedbdictionary");
            
            _savedbdictionaryClient.Headers["Content-type"] = "application/xml";
            _savedbdictionaryClient.Encoding = Encoding.UTF8;
            _savedbdictionaryClient.UploadStringAsync(new Uri(sb.ToString()), "POST", message);

            return null;
        }

        public DatabaseDictionary GetDatabaseSchema(string connString, string dbProvider)
        {
            Request request = new Request();
            request.Add("connectionString", connString);
            request.Add("dbProvider", dbProvider);
            string message = Utility.SerializeDataContract<Request>(request);

            StringBuilder sb = new StringBuilder();
            sb.Append(_dbDictionaryServiceUri);
            sb.Append("/dbschema");

            _dbschemaClient.Headers["Content-type"] = "application/xml";
            _dbschemaClient.Encoding = Encoding.UTF8;
            _dbschemaClient.UploadStringAsync(new Uri(sb.ToString()), "POST", message);

            return null;            
        }

        public List<string> GetExistingDbDictionaryFiles()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(_dbDictionaryServiceUri);
            sb.Append("/dbdictionaries");
            _dbdictionariesClient.DownloadStringAsync(new Uri(sb.ToString()));
            return null;
        }

        public String[] GetProviders()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(_dbDictionaryServiceUri);
            sb.Append("/providers");
            _providersClient.DownloadStringAsync(new Uri(sb.ToString()));
            return null;              
        }

        public Response PostDictionaryToAdapterService(string projectName, string applicationName, DatabaseDictionary dictionary)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(_adapterServiceUri);
            sb.Append("/");
            sb.Append(projectName);
            sb.Append("/");
            sb.Append(applicationName);
            sb.Append("/dbdictionary");

            string message = Utility.SerializeDataContract<DatabaseDictionary>(dictionary);

            _postdbdictionaryClient.Headers["Content-type"] = "application/xml";
            _postdbdictionaryClient.Encoding = Encoding.UTF8;
            _postdbdictionaryClient.UploadStringAsync(new Uri(sb.ToString()), "POST", message);

            return null;  
        }

        public Response ClearTripleStore(string projectName, string applicationName)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(_adapterServiceUri);
            sb.Append("/");
            sb.Append(projectName);
            sb.Append("/");
            sb.Append(applicationName);
            sb.Append("/clear");
            _clearClient.DownloadStringAsync(new Uri(sb.ToString()));
            return null;            
        }

        public Response DeleteApp(string projectName, string applicationName)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(_adapterServiceUri);
            sb.Append("/");
            sb.Append(projectName);
            sb.Append("/");
            sb.Append(applicationName);
            sb.Append("/delete");
            _deleteClient.DownloadStringAsync(new Uri(sb.ToString()));
            return null;
        }

        void OnCompletedEvent(object sender, AsyncCompletedEventArgs e)
        {
            if (OnDataArrived == null)
                return;

            // Our event argument
            CompletedEventArgs args = null;

            //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
            #region // Your Handler HERE (template to copy/paste)
            // <Method> data arrived event handler 
            if (sender == _testClient)
            {
                // Configure event argument
                args = new CompletedEventArgs
                {
                    // Define your method in CompletedEventType and assign
                    CompletedType = CompletedEventType.NotDefined,
                    Data = "Assign the expected result here"
                };
            }
            #endregion
            
            //if (sender == _scopesClient)
            //{
            //    string result = ((DownloadStringCompletedEventArgs)e).Result;

            //    Collection<ScopeProject> scopes = result.DeserializeDataContract<Collection<ScopeProject>>();

            //    // If the cast failed then return
            //    if (scopes == null)
            //        return;

            //    // Configure event argument
            //    args = new CompletedEventArgs
            //    {
            //        // Define your method in CompletedEventType and assign
            //        CompletedType = CompletedEventType.GetScopes,
            //        Data = scopes,
            //    };
            //}

            if (sender == _dbDictionaryClient)
            {
                try
                {
                    string result = ((DownloadStringCompletedEventArgs)e).Result;

                    DatabaseDictionary dbDictionary = result.DeserializeDataContract<DatabaseDictionary>();

                    // If the cast failed then return
                    if (dbDictionary == null)
                        return;

                    // Configure event argument
                    args = new CompletedEventArgs
                    {
                        // Define your method in CompletedEventType and assign
                        CompletedType = CompletedEventType.GetDbDictionary,
                        Data = dbDictionary,
                    };
                }
                catch (Exception ex)
                {
                    // Configure event argument
                    args = new CompletedEventArgs
                    {
                        // Define your method in CompletedEventType and assign
                        CompletedType = CompletedEventType.GetDbDictionary,
                        Error = ex,
                        FriendlyErrorMessage = "Error Getting Database Dictionary from DBDictionaryService.",
                    };
                }
            }

            if (sender == _dbschemaClient)
            {
                try
                {
                    string result = ((UploadStringCompletedEventArgs)e).Result;

                    DatabaseDictionary dbSchema = result.DeserializeDataContract<DatabaseDictionary>();

                    // If the cast failed then return
                    if (dbSchema == null)
                        return;

                    // Configure event argument
                    args = new CompletedEventArgs
                    {
                        // Define your method in CompletedEventType and assign
                        CompletedType = CompletedEventType.GetDatabaseSchema,
                        Data = dbSchema,
                    };
                }
                catch (Exception ex)
                {
                    // Configure event argument
                    args = new CompletedEventArgs
                    {
                        // Define your method in CompletedEventType and assign
                        CompletedType = CompletedEventType.GetDbDictionary,
                        Error = ex,
                        FriendlyErrorMessage = "Error Getting Database Schema from DBDictionaryService.",
                    };
                }
            }

            if (sender == _savedbdictionaryClient)
            {
                try
                {
                    string result = ((UploadStringCompletedEventArgs)e).Result;

                    Response response = result.DeserializeDataContract<Response>();

                    // Configure event argument
                    args = new CompletedEventArgs
                    {
                        // Define your method in CompletedEventType and assign
                        CompletedType = CompletedEventType.SaveDatabaseDictionary,
                        Data = response,
                    };
                }
                catch (Exception ex)
                {
                    // Configure event argument
                    args = new CompletedEventArgs
                    {
                        // Define your method in CompletedEventType and assign
                        CompletedType = CompletedEventType.GetDbDictionary,
                        Error = ex,
                        FriendlyErrorMessage = "Error while saving Database Dictionary through DBDictionaryService.",
                    };
                }
            }

            if (sender == _dbdictionariesClient)
            {
                try
                {
                    string result = ((DownloadStringCompletedEventArgs)e).Result;

                    List<string> dbDictionaries = result.DeserializeDataContract<List<string>>();

                    // If the cast failed then return
                    if (dbDictionaries == null)
                        return;

                    // Configure event argument
                    args = new CompletedEventArgs
                    {
                        // Define your method in CompletedEventType and assign
                        CompletedType = CompletedEventType.GetExistingDbDictionaryFiles,
                        Data = dbDictionaries,
                    };
                }
                catch (Exception ex)
                {
                    // Configure event argument
                    args = new CompletedEventArgs
                    {
                        // Define your method in CompletedEventType and assign
                        CompletedType = CompletedEventType.GetDbDictionary,
                        Error = ex,
                        FriendlyErrorMessage = "Error Getting existing Database Dictionary Files from DBDictionaryService.",
                    };
                }
            }

            if (sender == _providersClient)
            {
                try
                {
                    string result = ((DownloadStringCompletedEventArgs)e).Result;

                    string[] providers = result.DeserializeDataContract<string[]>();

                    // If the cast failed then return
                    if (providers == null)
                        return;

                    // Configure event argument
                    args = new CompletedEventArgs
                    {
                        // Define your method in CompletedEventType and assign
                        CompletedType = CompletedEventType.GetProviders,
                        Data = providers,
                    };
                }
                catch (Exception ex)
                {
                    // Configure event argument
                    args = new CompletedEventArgs
                    {
                        // Define your method in CompletedEventType and assign
                        CompletedType = CompletedEventType.GetDbDictionary,
                        Error = ex,
                        FriendlyErrorMessage = "Error Getting Provider Names from DBDictionaryService.",
                    };
                }
            }

            if (sender == _postdbdictionaryClient)
            {
                try
                {
                    string result = ((UploadStringCompletedEventArgs)e).Result;

                    Response response = result.DeserializeDataContract<Response>();

                    // Configure event argument
                    args = new CompletedEventArgs
                    {
                        // Define your method in CompletedEventType and assign
                        CompletedType = CompletedEventType.PostDictionaryToAdapterService,
                        Data = response,
                    };
                }
                catch (Exception ex)
                {
                    // Configure event argument
                    args = new CompletedEventArgs
                    {
                        // Define your method in CompletedEventType and assign
                        CompletedType = CompletedEventType.GetDbDictionary,
                        Error = ex,
                        FriendlyErrorMessage = "Error while posting Database Dictionary to the AdapterService.",
                    };
                }
            }

            if (sender == _clearClient)
            {
                try
                {
                    string result = ((DownloadStringCompletedEventArgs)e).Result;

                    Response response = result.DeserializeDataContract<Response>();

                    // Configure event argument
                    args = new CompletedEventArgs
                    {
                        // Define your method in CompletedEventType and assign
                        CompletedType = CompletedEventType.ClearTripleStore,
                        Data = response,
                    };
                }
                catch (Exception ex)
                {
                    // Configure event argument
                    args = new CompletedEventArgs
                    {
                        // Define your method in CompletedEventType and assign
                        CompletedType = CompletedEventType.GetDbDictionary,
                        Error = ex,
                        FriendlyErrorMessage = "Error while clearing triple store through the AdapterService.",
                    };
                }
            }

            if (sender == _deleteClient)
            {
                try
                {
                    string result = ((DownloadStringCompletedEventArgs)e).Result;

                    Response response = result.DeserializeDataContract<Response>();

                    // Configure event argument
                    args = new CompletedEventArgs
                    {
                        // Define your method in CompletedEventType and assign
                        CompletedType = CompletedEventType.DeleteApp,
                        Data = response,
                    };
                }
                catch (Exception ex)
                {
                    // Configure event argument
                    args = new CompletedEventArgs
                    {
                        // Define your method in CompletedEventType and assign
                        CompletedType = CompletedEventType.GetDbDictionary,
                        Error = ex,
                        FriendlyErrorMessage = "Error while deleting the app from AdapterService.",
                    };
                }
            }

            if (args != null)
                OnDataArrived(sender, args);

        }

    }
}
