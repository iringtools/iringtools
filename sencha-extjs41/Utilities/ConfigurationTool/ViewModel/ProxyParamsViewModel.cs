using System;
using System.Windows;
using System.Xml;
using ConfigurationTool.DataAccess;
using ConfigurationTool.Model;
using System.Windows.Input;
using System.Windows.Media;
using org.iringtools.utility;
using System.Xml.Linq;
using System.Linq;

namespace ConfigurationTool.ViewModel
{
    class ProxyParamsViewModel : ViewModelBase
    {
        // Private variables
        ProxyRepository _proxyRepository;
        RelayCommand _updateCommand;
        RelayCommand _updateRegCommand;
        RelayCommand _resetCommand;
        RelayCommand _resetRegistryCommand;
        RelayCommand _getIRTFolderCommand;

        public ProxyRepository ProxyParams
        {
            get
            {
                return _proxyRepository;
            }
        }

        public ProxyParamsViewModel(ProxyRepository proxyRepository)
        {
            if (proxyRepository == null)
            {
                throw new ArgumentNullException("proxyRepository");
            }

            _proxyRepository = proxyRepository;
            _proxyParams = new ProxyParams();
            _proxyParams.AppName = "Proxy Configuration Tool";
            _proxyParams.RegAppName = "Registry Configuration Tool";
            Reset();
        }

        public void Reset()
        {
            //_proxyParams.iRingToolsFolder = "";
            _proxyParams.Username = "";
            _proxyParams.Password = "";
            _proxyParams.RegUsername = "";
            _proxyParams.RegPassword = "";
            _proxyParams.Domain = "";
            _proxyRepository.Refresh();
            _proxyParams.ProxyHost = _proxyRepository.ProxyHost;
            _proxyParams.ProxyPort = _proxyRepository.ProxyPort;
        }

        ProxyParams _proxyParams;
        public ProxyParams ProxyParameters
        {
            get
            {
                return _proxyParams;
            }
            set
            {
                _proxyParams = value;
            }
        }

        public string AppName
        {
            get
            {
                return _proxyParams.AppName;
            }
            set
            {
                _proxyParams.AppName = value;
            }
        }
        public string RegAppName
        {
            get
            {
                return _proxyParams.RegAppName;
            }
            set
            {
                _proxyParams.RegAppName = value;
            }
        }

        public bool IsEnable
        {
            get
            {
                return _proxyParams.IsEnable;
            }
            set
            {
                _proxyParams.IsEnable = value;
            }
        }

        public string ServicesWebConfig
        {
            get
            {
                return _proxyParams.ServicesWebConfig;
            }
            set
            {
                _proxyParams.ServicesWebConfig = value;
                //if (String.IsNullOrEmpty(value))
                //{
                //    throw new ApplicationException("Folder location is required.");
                //}
            }
        }

        public string Username
        {
            get
            {
                if (String.IsNullOrEmpty(_proxyParams.Domain))
                {
                    return _proxyParams.Username;
                }
                else
                {
                    return _proxyParams.Domain + @"\" + _proxyParams.Username;
                }
            }
            set
            {
                string userName = String.Empty;
                string domainName = String.Empty;
                if (value.Contains(@"\"))
                {
                    string[] delimiters = { @"\" };
                    string[] parts = value.Split(delimiters, StringSplitOptions.None);
                    userName = parts[1];
                    domainName = parts[0];
                }
                else
                {
                    userName = value;
                }

                _proxyParams.Username = userName;
                _proxyParams.Domain = domainName;

                //if (String.IsNullOrEmpty(value))
                //  {
                //      throw new ApplicationException("Username is required.");
                //  }
            }
        }

        public string RegPassword
        {
            get
            {
                return _proxyParams.RegPassword;
            }
            set
            {
                _proxyParams.RegPassword = value;
                //if (String.IsNullOrEmpty(value))
                //{
                //    throw new ApplicationException("Password is required.");
                //}
            }
        }
        public string RegUsername
        {
            get
            {
                if (!String.IsNullOrEmpty(_proxyParams.RegUsername))
                {
                    return _proxyParams.RegUsername;
                }
                else
                {
                    return string.Empty;
                }

            }
            set
            {
                string userName = String.Empty;
                string domainName = String.Empty;
                if (value.Contains(@"\"))
                {
                    string[] delimiters = { @"\" };
                    string[] parts = value.Split(delimiters, StringSplitOptions.None);
                    userName = parts[1];
                    domainName = parts[0];
                }
                else
                {
                    userName = value;
                }

                _proxyParams.RegUsername = userName;
            }
        }

        public string Password
        {
            get
            {
                return _proxyParams.Password;
            }
            set
            {
                _proxyParams.Password = value;
                //if (String.IsNullOrEmpty(value))
                //{
                //    throw new ApplicationException("Password is required.");
                //}
            }
        }

        public string ProxyHost
        {
            get
            {
                return _proxyParams.ProxyHost;
            }
            set
            {
                _proxyParams.ProxyHost = value;
                //if (String.IsNullOrEmpty(value))
                //{
                //    throw new ApplicationException("Proxy Host is required.");
                //}
            }
        }

        public string ProxyPort
        {
            get
            {
                return _proxyParams.ProxyPort;
            }
            set
            {
                _proxyParams.ProxyPort = value;
            }
        }

        private Brush _bgBrush;
        public Brush BackgroundBrush
        {
            get
            {
                return _bgBrush;
            }
            set
            {
                _bgBrush = value;
                OnPropertyChanged("BackgroundBrush");
            }
        }

        public ICommand UpdateCommand
        {
            get
            {
                if (_updateCommand == null)
                {
                    _updateCommand = new RelayCommand(param => this.UpdateCommandExecute(), param => this.UpdateCommandCanExecute);
                }
                return _updateCommand;
            }
        }
        #region Registatry Configuration
        public ICommand UpdateRegistryCommand
        {
            get
            {
                if (_updateRegCommand == null)
                {
                    _updateRegCommand = new RelayCommand(param => this.UpdateRegistryCommandExecute(), param => this.UpdateRegistryCommandCanExecute);
                }
                return _updateRegCommand;
            }
        }


        void UpdateRegistryCommandExecute()
        {
            try
            {
                bool isValid = true;
                if (_proxyParams.ServicesWebConfig == "")
                {
                    isValid = false;
                    OnPropertyChanged("ServicesWebConfig");
                }

                if (!isValid)
                {
                    MessageBox.Show("One or more parameters invalid.", "Validation Failed", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                XDocument adminConfig = XDocument.Load(_proxyParams.ServicesWebConfig);


                if (!String.IsNullOrEmpty(_proxyParams.RegUsername))
                {
                    WebCredentials credentials = new WebCredentials
                    {
                        userName = _proxyParams.RegUsername,
                        password = _proxyParams.RegPassword,
                        //domain = _proxyParams.Domain
                    };
                    credentials.Encrypt();

                    adminConfig.UpdateSetting("RegistryCredentialToken", credentials.encryptedToken);
                }
                else
                {
                    adminConfig.RemoveSetting("RegistryCredentialToken");
                }

                adminConfig.Save(_proxyParams.ServicesWebConfig);

                MessageBox.Show("Update complete.", "Update Complete", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception exc)
            {
                MessageBox.Show("Error occurred while getting updating proxy configuration. " + exc.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }

            // sample code
            //bool isupdated = true;

            //if (_proxyParams.iRingToolsFolder!="")
            //{
            //    isupdated = false;
            //}
            //if (isupdated)
            //{
            //    BackgroundBrush = new SolidColorBrush(Colors.Green);
            //}
            //else
            //{
            //    BackgroundBrush = new SolidColorBrush(Colors.White);
            //}
        }
        void UpdateCommandExecute()
        {
            try
            {
                bool isValid = true;
                if (_proxyParams.ServicesWebConfig == "")
                {
                    isValid = false;
                    OnPropertyChanged("ServicesWebConfig");
                }

                if (!isValid)
                {
                    MessageBox.Show("One or more parameters invalid.", "Validation Failed", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                XDocument adminConfig = XDocument.Load(_proxyParams.ServicesWebConfig);

                if (String.IsNullOrEmpty(_proxyParams.ProxyHost))
                {
                    adminConfig.RemoveSetting("ProxyCredentialToken");
                    adminConfig.RemoveSetting("ProxyHost");
                    adminConfig.RemoveSetting("ProxyPort");
                }
                else
                {
                    if (!String.IsNullOrEmpty(_proxyParams.Username))
                    {
                        WebCredentials credentials = new WebCredentials
                        {
                            userName = _proxyParams.Username,
                            password = _proxyParams.Password,
                            domain = _proxyParams.Domain
                        };
                        credentials.Encrypt();

                        adminConfig.UpdateSetting("ProxyCredentialToken", credentials.encryptedToken);
                    }
                    else
                    {
                        adminConfig.RemoveSetting("ProxyCredentialToken");
                    }

                    string proxyPort = _proxyParams.ProxyPort;
                    if (String.IsNullOrEmpty(proxyPort)) proxyPort = "8080";
                    _proxyParams.ProxyPort = proxyPort;
                    OnPropertyChanged("ProxyPort");

                    adminConfig.UpdateSetting("ProxyHost", _proxyParams.ProxyHost);
                    adminConfig.UpdateSetting("ProxyPort", proxyPort);
                }

                adminConfig.Save(_proxyParams.ServicesWebConfig);

                MessageBox.Show("Update complete.", "Update Complete", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception exc)
            {
                MessageBox.Show("Error occurred while getting updating proxy configuration. " + exc.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }

            // sample code
            //bool isupdated = true;

            //if (_proxyParams.iRingToolsFolder!="")
            //{
            //    isupdated = false;
            //}
            //if (isupdated)
            //{
            //    BackgroundBrush = new SolidColorBrush(Colors.Green);
            //}
            //else
            //{
            //    BackgroundBrush = new SolidColorBrush(Colors.White);
            //}
        }

        bool UpdateRegistryCommandCanExecute
        {
            get
            {
                //if (_proxyParams.Username!="")
                //{
                //    return false;
                //}
                return true;
            }
        }

        public ICommand ResetRegistryCommand
        {
            get
            {
                if (_resetRegistryCommand == null)
                {
                    _resetRegistryCommand = new RelayCommand(param => this.ResetRegistryCommandExecute(), param => this.ResetRegistryCanExecute);
                }
                return _resetRegistryCommand;
            }
        }
        void ResetRegistryCommandExecute()
        {
            Reset();
            OnPropertyChanged("ServicesWebConfig");
            OnPropertyChanged("RegUsername");
            OnPropertyChanged("RegPassword");
        }
        bool ResetRegistryCanExecute
        {
            get
            {
                return true;
            }
        }
        #endregion
        bool UpdateCommandCanExecute
        {
            get
            {
                //if (_proxyParams.Username!="")
                //{
                //    return false;
                //}
                return true;
            }
        }
        public ICommand ResetCommand
        {
            get
            {
                if (_resetCommand == null)
                {
                    _resetCommand = new RelayCommand(param => this.ResetCommandExecute(), param => this.ResetCanExecute);
                }
                return _resetCommand;
            }
        }

        void ResetCommandExecute()
        {
            Reset();
            OnPropertyChanged("ServicesWebConfig");
            OnPropertyChanged("Username");
            OnPropertyChanged("Password");
            OnPropertyChanged("ProxyHost");
            OnPropertyChanged("ProxyPort");
        }

        bool ResetCanExecute
        {
            get
            {
                return true;
            }
        }

        public ICommand GetIRTFolder
        {
            get
            {
                if (_getIRTFolderCommand == null)
                {
                    _getIRTFolderCommand = new RelayCommand(param => this.GetIRTFolderExecute(), param => this.GetIRTFolderCanExecute);
                }
                return _getIRTFolderCommand;
            }
        }

        void GetIRTFolderExecute()
        {
            try
            {
                // Configure open file dialog box
                Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
                dlg.FileName = "Web.config"; // Default file name
                dlg.DefaultExt = ".config"; // Default file extension
                dlg.Filter = "Web Configuration (.config)|*.config"; // Filter files by extension
                dlg.Title = "Select the file Web.config in the iRINGTools Services folder";
                Nullable<bool> result = dlg.ShowDialog();
                if (result == true)
                {
                    // Get folder path
                    _proxyParams.ServicesWebConfig = dlg.FileName;
                    OnPropertyChanged("ServicesWebConfig");

                    XDocument adminConfig = XDocument.Load(_proxyParams.ServicesWebConfig);

                    string encryptedCredentials = adminConfig.GetSetting("ProxyCredentialToken");
                    string encryptedRegCredentials = adminConfig.GetSetting("RegistryCredentialToken");

                    WebCredentials credentials = new WebCredentials(encryptedCredentials);
                    WebCredentials regcredentials = new WebCredentials(encryptedRegCredentials);
                    if (credentials.isEncrypted) credentials.Decrypt();

                    this.Username = credentials.domain + @"\" + credentials.userName;
                    OnPropertyChanged("Username");

                    this.Password = credentials.password;
                    OnPropertyChanged("Password");

                    this.ProxyHost = adminConfig.GetSetting("ProxyHost");
                    OnPropertyChanged("ProxyHost");

                    this.ProxyPort = adminConfig.GetSetting("ProxyPort");
                    OnPropertyChanged("ProxyPort");



                    if (regcredentials.isEncrypted) regcredentials.Decrypt();
                    if (encryptedRegCredentials != string.Empty)
                    {
                        this.RegUsername = regcredentials.userName;
                        OnPropertyChanged("RegUsername");

                        this.RegPassword = regcredentials.password;
                        OnPropertyChanged("RegPassword");

                        this.IsEnable = true;
                        OnPropertyChanged("IsEnable");
                       
                    }
                    else
                    {
                        this.IsEnable = false;
                        OnPropertyChanged("IsEnable");

                        this.RegUsername = "";
                        OnPropertyChanged("RegUsername");

                        this.RegPassword = "";
                        OnPropertyChanged("RegPassword");
                    }



                }
            }
            catch (Exception exc)
            {
                MessageBox.Show("Error occurred while getting folder. " + exc.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        bool GetIRTFolderCanExecute
        {
            get
            {
                return true;
            }
        }

        protected override void OnDispose()
        {
            //todo: clear something?
        }
    }

    public static class MyExtensions
    {
        public static void UpdateSetting(this XDocument adminConfig, string key, string value)
        {
            XElement setting = (
                   from c in adminConfig.Descendants("appSettings").Descendants("add")
                   where c.Attribute("key").Value.Equals(key)
                   select c
                 ).FirstOrDefault();

            if (setting == null)
            {
                setting = new XElement(
                  "add",
                  new XAttribute("key", key),
                  new XAttribute("value", value)
                );

                adminConfig.Root.Element("appSettings").Add(setting);
            }
            else
            {
                setting.Attribute("value").Value = value;
            }
        }

        public static void RemoveSetting(this XDocument adminConfig, string key)
        {
            XElement setting = (
                   from c in adminConfig.Descendants("appSettings").Descendants("add")
                   where c.Attribute("key").Value.Equals(key)
                   select c
                 ).FirstOrDefault();

            if (setting != null)
            {
                setting.Remove();
            }
        }

        public static string GetSetting(this XDocument adminConfig, string key)
        {
            XElement setting = (
                   from c in adminConfig.Descendants("appSettings").Descendants("add")
                   where c.Attribute("key").Value.Equals(key)
                   select c
                 ).FirstOrDefault();

            if (setting == null)
            {
                return String.Empty;
            }
            else
            {
                return setting.Attribute("value").Value;
            }
        }
    }
}
