using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using org.iringtools.utility;
using System.Net;
using System.Collections.ObjectModel;
using org.iringtools.library;
using org.iringtools.mapping;
using org.w3.sparql_results;
using System.Threading;
using VDS.RDF.Query;
using System.Configuration;

namespace org.iringtools.utils.exchange
{
  /// <summary>
  /// Interaction logic for FacadeExchange.xaml
  /// </summary>
  public partial class FacadeExchange : Window
  {
    #region Member Variables
      private ObservableCollection<StatusMessage> _messages = new ObservableCollection<StatusMessage>();
      Application _app = Application.Current;
      Locator _project = null;
      EndpointApplication _application = null;
      GraphMap _graph = null;
      List<string> _graphUris = null;
      string _graphUri = String.Empty;

      string _proxyHost = String.Empty;
      string _proxyPort = String.Empty;
      string _proxyCredentialToken = String.Empty;

      string _facadeCredentialDomain = String.Empty;
      string _facadeCredentialUsername = String.Empty;
      string _facadeCredentialPassword = String.Empty;

      string _adapterCredentialDomain = String.Empty;
      string _adapterCredentialUsername = String.Empty;
      string _adapterCredentialPassword = String.Empty; 
      #endregion

    public FacadeExchange()
    {
      InitializeComponent();

      listBoxResults.ItemsSource = _messages;

      textBoxAdapterURL.Text = ConfigurationManager.AppSettings["DefaultAdapterURL"];
      textBoxTargetURL.Text = ConfigurationManager.AppSettings["DefaultFacadeURL"];

      _proxyHost = ConfigurationManager.AppSettings["ProxyHost"];
      _proxyPort = ConfigurationManager.AppSettings["ProxyPort"];
      _proxyCredentialToken = ConfigurationManager.AppSettings["ProxyCredentialToken"];
    }

    #region Controls Events
    private void buttonPull_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            _messages.Clear();

            _messages.Add(new StatusMessage { Message = "Pulling Graph from remote Façade...", ImageName = "Resources/info_22.png" });

            WebClient client = new WebClient();
            bool adapterConnect;
            bool facadeConnect;

            if (chkboxAdapterCredentials.IsChecked == true)
                adapterConnect = ShowLoginDialog(CredentialType.Adapter) == true;
            else
                adapterConnect = true;

            if (chkboxFacadeCredentials.IsChecked == true)
                facadeConnect = ShowLoginDialog(CredentialType.Facade) == true;
            else
                facadeConnect = true;

            if (adapterConnect && facadeConnect)
            {
                #region Prepare proxy and facade credentials if required
                if (!String.IsNullOrEmpty(_proxyHost) && !String.IsNullOrEmpty(_proxyPort))
                {
                    WebProxy proxy = new WebProxy(_proxyHost, Convert.ToInt16(_proxyPort));
                    proxy.Credentials = CredentialCache.DefaultCredentials;
                    client.Proxy = proxy;
                }
                else
                {
                    client.Proxy.Credentials = CredentialCache.DefaultCredentials;
                }
                if (chkboxAdapterCredentials.IsChecked == true)
                {
                    NetworkCredential credential = new NetworkCredential(_adapterCredentialUsername, _adapterCredentialPassword, _adapterCredentialDomain);
                    client.Credentials = credential;
                }
                else
                {
                    client.Credentials = CredentialCache.DefaultCredentials;
                }
                #endregion

                //client.Credentials = CredentialCache.DefaultCredentials;

                //client.Proxy.Credentials = CredentialCache.DefaultCredentials;

                client.UploadStringCompleted += new UploadStringCompletedEventHandler(client_UploadStringCompleted);
                client.Headers["Content-type"] = "application/xml";
                client.Encoding = Encoding.UTF8;

                string rootUrl = textBoxAdapterURL.Text.Substring(0, textBoxAdapterURL.Text.LastIndexOf("/"));
                string localFacadeUrl = rootUrl + "/facade/svc";

                Uri pullURI = new Uri(
                    localFacadeUrl + "/" +
                    _project.Context + "/" +
                    _application.Endpoint + "/" +
                    _graph.name + "/pull");

                Request request = new Request();
                WebCredentials targetCredentials = new WebCredentials();
                if (chkboxFacadeCredentials.IsChecked == true)
                {
                    targetCredentials.domain = _facadeCredentialDomain;
                    targetCredentials.userName = _facadeCredentialUsername;
                    targetCredentials.password = _facadeCredentialPassword;
                    targetCredentials.Encrypt();
                }
                string targetCredentialsXML = Utility.Serialize<WebCredentials>(targetCredentials, true);
                request.Add("targetEndpointUri", textBoxTargetURL.Text);
                request.Add("targetCredentials", targetCredentialsXML);
                request.Add("targetGraphBaseUri", comboBoxGraphUri.Text);

                string message = Utility.SerializeDataContract<Request>(request);

                client.UploadStringAsync(pullURI, message);
            }
            else
            {
                _messages.Add(new StatusMessage { Message = "User cancelled operation.", ImageName = "Resources/info_22.png" });
            }
        }
        catch (Exception ex)
        {
            _messages.Clear();

            _messages.Add(new StatusMessage { Message = ex.ToString(), ImageName = "Resources/error_22.png" });
        }

    }

    private void buttonPublish_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            _messages.Clear();

            _messages.Add(new StatusMessage { Message = "Publishing Graph to own Façade...", ImageName = "Resources/info_22.png" });

            WebClient client = new WebClient();
            bool connect;

            if (chkboxAdapterCredentials.IsChecked == true)
                connect = ShowLoginDialog(CredentialType.Adapter) == true;
            else
                connect = true;

            if (connect)
            {
                #region Prepare proxy and facade credentials if required
              if (!String.IsNullOrEmpty(_proxyHost) && !String.IsNullOrEmpty(_proxyPort))
                {
                    WebProxy proxy = new WebProxy(_proxyHost, Convert.ToInt16(_proxyPort));
                    proxy.Credentials = CredentialCache.DefaultCredentials;
                    client.Proxy = proxy;
                }
                else
                {
                    client.Proxy.Credentials = CredentialCache.DefaultCredentials;
                }
                if (chkboxAdapterCredentials.IsChecked == true)
                {
                    NetworkCredential credential = new NetworkCredential(_adapterCredentialUsername, _adapterCredentialPassword, _adapterCredentialDomain);
                    client.Credentials = credential;
                }
                else
                {
                    client.Credentials = CredentialCache.DefaultCredentials;
                }
                #endregion

                client.DownloadStringCompleted += new DownloadStringCompletedEventHandler(client_DownloadStringCompleted);

                string rootUrl = textBoxAdapterURL.Text.Substring(0, textBoxAdapterURL.Text.LastIndexOf("/"));
                string localFacadeUrl = rootUrl + "/facade/svc";

                Uri refreshURI = new Uri(localFacadeUrl + "/" + _project.Context + "/" + _application.Endpoint + "/" + comboBoxGraphName.Text + "/refresh");

                client.DownloadStringAsync(refreshURI);
            }
            else
            {
                _messages.Clear();

                _messages.Add(new StatusMessage { Message = "User cancelled operation.", ImageName = "Resources/info_22.png" });
            }
        }
        catch (Exception ex)
        {
            _messages.Clear();

            _messages.Add(new StatusMessage { Message = ex.ToString(), ImageName = "Resources/error_22.png" });
        }
    }

    private void buttonConnect_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            _messages.Clear();

            _messages.Add(new StatusMessage { Message = "Fetching Scopes from Adapter...", ImageName = "Resources/info_22.png" });

            Uri scopesURI = new Uri(textBoxAdapterURL.Text + "/scopes");

            WebClient client = new WebClient();
            bool connect;

            if (chkboxAdapterCredentials.IsChecked == true)
                connect = ShowLoginDialog(CredentialType.Adapter) == true;
            else
                connect = true;

            if (connect)
            {
                #region Prepare proxy and facade credentials if required
                if (!String.IsNullOrEmpty(_proxyHost) && !String.IsNullOrEmpty(_proxyPort))
                {
                    WebProxy proxy = new WebProxy(_proxyHost, Convert.ToInt16(_proxyPort));
                    proxy.Credentials = CredentialCache.DefaultCredentials;
                    client.Proxy = proxy;
                }
                else
                {
                    client.Proxy.Credentials = CredentialCache.DefaultCredentials;
                }
                if (chkboxAdapterCredentials.IsChecked == true)
                {
                    NetworkCredential credential = new NetworkCredential(_adapterCredentialUsername, _adapterCredentialPassword, _adapterCredentialDomain);
                    client.Credentials = credential;
                }
                else
                {
                    client.Credentials = CredentialCache.DefaultCredentials;
                }
                #endregion

                client.DownloadStringCompleted += new DownloadStringCompletedEventHandler(client_GetScopesCompleted);

                client.DownloadStringAsync(scopesURI); 
            }
            else
            {
                _messages.Clear();

                _messages.Add(new StatusMessage { Message = "User cancelled operation.", ImageName = "Resources/info_22.png" });
            }
        }
        catch (Exception ex)
        {
            _messages.Clear();

            _messages.Add(new StatusMessage { Message = ex.ToString(), ImageName = "Resources/error_22.png" });
        }
    }

    private void buttonExit_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            _app.Shutdown();
        }
        catch (Exception ex)
        {
            _messages.Clear();

            _messages.Add(new StatusMessage { Message = ex.ToString(), ImageName = "Resources/error_22.png" });
        }
    }

    private void buttonFacadeConnect_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            _messages.Clear();
            bool connect;

            if (chkboxFacadeCredentials.IsChecked == true)
                connect = ShowLoginDialog(CredentialType.Facade) == true;
            else
                connect = true;

            if (connect)
            {
                _messages.Add(new StatusMessage { Message = "Fetching graphs from Façade...", ImageName = "Resources/info_22.png" });

                string sparql = "SELECT DISTINCT ?g WHERE { GRAPH ?g { ?s ?p ?o } }";
                //string sparql = "PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#> SELECT * WHERE {?uri rdfs:label ?label} ORDER BY ?label LIMIT 1";
                SparqlRemoteEndpoint endpoint = new SparqlRemoteEndpoint(new Uri(textBoxTargetURL.Text), "");
                endpoint.DefaultGraphs.Add("");

                #region Prepare proxy and facade credentials if required
                if (!String.IsNullOrEmpty(_proxyHost) && !String.IsNullOrEmpty(_proxyPort))
                {
                    WebProxy proxy = new WebProxy(_proxyHost, Convert.ToInt16(_proxyPort));
                    proxy.Credentials = CredentialCache.DefaultCredentials;
                    endpoint.Proxy = proxy;
                }
                if (chkboxFacadeCredentials.IsChecked == true)
                {
                    endpoint.Credentials = new NetworkCredential();
                    endpoint.Credentials.Domain = _facadeCredentialDomain;
                    endpoint.Credentials.UserName = _facadeCredentialUsername;
                    endpoint.Credentials.Password = _facadeCredentialPassword;
                }
                #endregion

                try
                {
                    SparqlResultSet results = new SparqlResultSet();
                    try
                    {
                      results = endpoint.QueryWithResultSet(sparql);
                    }
                    //Handle dotNetRDF Deserialization Bug
                    catch (Exception ex)
                    {
                      if (ex.Message != "Unable to Parse a SPARQL Result Set since a <binding> element contains an unexpected element <result>!")
                        throw ex;
                    }

                    _graphUris = new List<string>();
                    _graphUris.Add("[Default Graph]");
                    foreach (SparqlResult result in results.Results)
                    {
                        string uri = result.Value("g").ToString();

                        if (uri != null)
                        {
                            if (!uri.StartsWith("_:"))
                            {
                                _graphUris.Add(uri);
                            }
                        }
                    }

                    comboBoxGraphUri.ItemsSource = _graphUris;
                    comboBoxGraphUri.SelectionChanged += new SelectionChangedEventHandler(comboBoxGraphUri_SelectionChanged);
                    comboBoxGraphUri.SelectedIndex = 0;
                    comboBoxGraphUri.IsEnabled = true;

                    _messages.Add(new StatusMessage
                    {
                        Message = "Successfully fetched " + _graphUris.Count +
                          " graphs from Façade.",
                        ImageName = "Resources/success_22.png"
                    });
                }
                catch (Exception ex)
                {
                    comboBoxGraphUri.IsEnabled = true;
                    buttonPull.IsEnabled = true;
                    _messages.Clear();

                    _messages.Add(new StatusMessage { Message = ex.ToString(), ImageName = "Resources/error_22.png" });
                    _messages.Add(new StatusMessage { Message = "Please type in the Graph URI.", ImageName = "Resources/info_22.png" });
                }

                listBoxResults.ScrollIntoView(listBoxResults.Items[listBoxResults.Items.Count - 1]);
            }
            else
            {
                _messages.Clear();

                _messages.Add(new StatusMessage { Message = "User cancelled operation.", ImageName = "Resources/info_22.png" });
            }
        }
        catch (Exception ex)
        {
            _messages.Clear();

            _messages.Add(new StatusMessage { Message = ex.ToString(), ImageName = "Resources/error_22.png" });
        }
    }

    private void comboBoxProjectName_SelectionChanged(object sender, RoutedEventArgs e)
    {
        try
        {
            if (comboBoxProjectName.SelectedIndex != -1)
            {
                _project = (Locator)comboBoxProjectName.SelectedItem;
                comboBoxAppName.DisplayMemberPath = "Name";
                comboBoxAppName.ItemsSource = _project.Applications;
                comboBoxAppName.SelectionChanged += new SelectionChangedEventHandler(comboBoxAppName_SelectionChanged);
                comboBoxAppName.SelectedIndex = 0;
            }
        }
        catch (Exception ex)
        {
            _messages.Clear();

            _messages.Add(new StatusMessage { Message = ex.ToString(), ImageName = "Resources/error_22.png" });
        }
    }

    private void comboBoxAppName_SelectionChanged(object sender, RoutedEventArgs e)
    {
        try
        {
            _application = (EndpointApplication)comboBoxAppName.SelectedItem;

            if (_application != null && _application.Endpoint != null && _application.Endpoint != String.Empty)
            {
                WebClient client = new WebClient();
                #region Prepare proxy and facade credentials if required
                if (!String.IsNullOrEmpty(_proxyHost) && !String.IsNullOrEmpty(_proxyPort))
                {
                    WebProxy proxy = new WebProxy(_proxyHost, Convert.ToInt16(_proxyPort));
                    proxy.Credentials = CredentialCache.DefaultCredentials;
                    client.Proxy = proxy;
                }
                else
                {
                    client.Proxy.Credentials = CredentialCache.DefaultCredentials;
                }
                if (chkboxAdapterCredentials.IsChecked == true)
                {
                    NetworkCredential credential = new NetworkCredential(_adapterCredentialUsername, _adapterCredentialPassword, _adapterCredentialDomain);
                    client.Credentials = credential;
                }
                else
                {
                    client.Credentials = CredentialCache.DefaultCredentials;
                }
                #endregion

                client.DownloadStringCompleted += new DownloadStringCompletedEventHandler(client_GetMappingCompleted);

                Uri mappingURI = new Uri(textBoxAdapterURL.Text + "/" + _project.Context + "/" + _application.Endpoint + "/mapping");

                client.DownloadStringAsync(mappingURI);
            }
        }
        catch (Exception ex)
        {
            _messages.Clear();

            _messages.Add(new StatusMessage { Message = ex.ToString(), ImageName = "Resources/error_22.png" });
        }
    }

    private void comboBoxGraphName_SelectionChanged(object sender, RoutedEventArgs e)
    {
        try
        {
            _graph = (GraphMap)comboBoxGraphName.SelectedItem;

            buttonPublish.IsEnabled = true;
        }
        catch (Exception ex)
        {
            _messages.Clear();

            _messages.Add(new StatusMessage { Message = ex.ToString(), ImageName = "Resources/error_22.png" });
        }
    }

    private void comboBoxGraphUri_SelectionChanged(object sender, RoutedEventArgs e)
    {
        try
        {
            _graphUri = comboBoxGraphUri.SelectedItem.ToString();

            buttonPull.IsEnabled = true;
        }
        catch (Exception ex)
        {
            _messages.Clear();

            _messages.Add(new StatusMessage { Message = ex.ToString(), ImageName = "Resources/error_22.png" });
        }
    }

    
    #endregion

    #region Webclient Task Complete handlers
    void client_UploadStringCompleted(object sender, UploadStringCompletedEventArgs e)
    {
        try
        {
            DisplayResults(e.Result);
        }
        catch (Exception ex)
        {
            _messages.Clear();

            _messages.Add(new StatusMessage { Message = ex.ToString(), ImageName = "Resources/error_22.png" });
        }
    }

    void client_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
    {
        try
        {
            DisplayResults(e.Result);
        }
        catch (Exception ex)
        {
            _messages.Clear();

            _messages.Add(new StatusMessage { Message = ex.ToString(), ImageName = "Resources/error_22.png" });
        }
    }

    private void client_GetScopesCompleted(object sender, DownloadStringCompletedEventArgs e)
    {
        try
        {
            ObservableCollection<Locator> scopes = e.Result.DeserializeDataContract<ObservableCollection<Locator>>();

            if (scopes != null && scopes.Count > 0)
            {
                comboBoxProjectName.DisplayMemberPath = "Name";
                comboBoxProjectName.ItemsSource = scopes;
                comboBoxProjectName.SelectionChanged += new SelectionChangedEventHandler(comboBoxProjectName_SelectionChanged);
                comboBoxProjectName.SelectedIndex = 0;

                comboBoxProjectName.IsEnabled = true;
                comboBoxAppName.IsEnabled = true;
            }

            _messages.Add(new StatusMessage
            {
                Message = "Successfully fetched " + scopes.Count +
                  " scopes from Adapter.",
                ImageName = "Resources/success_22.png"
            });

            listBoxResults.ScrollIntoView(listBoxResults.Items[listBoxResults.Items.Count - 1]);
        }
        catch (Exception ex)
        {
            _messages.Clear();

            _messages.Add(new StatusMessage { Message = ex.ToString(), ImageName = "Resources/error_22.png" });
        }
    }

    private void client_GetMappingCompleted(object sender, DownloadStringCompletedEventArgs e)
    {
        try
        {
            Mapping mapping = e.Result.DeserializeDataContract<Mapping>();

            if (mapping != null && mapping.graphMaps.Count > 0)
            {
                comboBoxGraphName.DisplayMemberPath = "name";
                comboBoxGraphName.ItemsSource = mapping.graphMaps;
                comboBoxGraphName.SelectionChanged += new SelectionChangedEventHandler(comboBoxGraphName_SelectionChanged);
                comboBoxGraphName.SelectedIndex = 0;
                comboBoxGraphName.IsEnabled = true;
            }
        }
        catch (Exception ex)
        {
            _messages.Clear();

            _messages.Add(new StatusMessage { Message = ex.ToString(), ImageName = "Resources/error_22.png" });
        }
    }
    #endregion

    #region Private Methods
    void DisplayResults(string result)
    {
        Response response = result.DeserializeDataContract<Response>();

        if (response != null && response.StatusList != null)
        {
            foreach (Status status in response.StatusList)
            {
                string imageName = "Resources/info_22.png";
                if (status.Level == StatusLevel.Error) imageName = "Resources/error_22.png";
                if (status.Level == StatusLevel.Success) imageName = "Resources/success_22.png";

                foreach (string message in status.Messages)
                {
                    _messages.Add(new StatusMessage { Message = message, ImageName = imageName });
                }
            }

            listBoxResults.ScrollIntoView(listBoxResults.Items[listBoxResults.Items.Count - 1]);
        }
    }

    private bool ShowLoginDialog(CredentialType credentialType)
    {
        Login login = new Login();
        login.Owner = this;
        if (credentialType == CredentialType.Adapter)
        {
            login.txtBlockTitle.Text = "Adapter Credentials";
        }
        else if (credentialType == CredentialType.Facade)
        {
            login.txtBlockTitle.Text = "Façade Credentials";
        }

        login.ShowDialog();

        if (login.DialogResult == true && credentialType == CredentialType.Adapter)
        {
            _adapterCredentialDomain = login.textBoxDomain.Text;
            _adapterCredentialUsername = login.textBoxUsername.Text;
            _adapterCredentialPassword = login.textBoxPassword.Password;
        }
        else if (login.DialogResult == true && credentialType == CredentialType.Facade)
        {
            _facadeCredentialDomain = login.textBoxDomain.Text;
            _facadeCredentialUsername = login.textBoxUsername.Text;
            _facadeCredentialPassword = login.textBoxPassword.Password;
        }

        return login.DialogResult == true;
    }
    #endregion
    
    private enum CredentialType
    {
        Adapter,
        Facade
    }

    public class StatusMessage
    {
      public string ImageName { get; set; }
      public string Message { get; set; }
    }    

    //public string AdapterCredentialDomain
    //{
    //    get { return _adapterCredentialDomain; }
    //    set { _adapterCredentialDomain = value; }
    //}

    //public string AdapterCredentialUsername
    //{
    //    get { return _adapterCredentialUsername; }
    //    set { _adapterCredentialUsername = value; }
    //}

    //public string AdapterCredentialPassword
    //{
    //    get { return _adapterCredentialPassword; }
    //    set { _adapterCredentialPassword = value; }
    //}

    //public string FacadeCredentialDomain
    //{
    //    get { return _facadeCredentialDomain; }
    //    set { _facadeCredentialDomain = value; }
    //}

    //public string FacadeCredentialUsername
    //{
    //    get { return _facadeCredentialUsername; }
    //    set { _facadeCredentialUsername = value; }
    //}

    //public string FacadeCredentialPassword
    //{
    //    get { return _facadeCredentialPassword; }
    //    set { _facadeCredentialPassword = value; }
    //}

  }
}
