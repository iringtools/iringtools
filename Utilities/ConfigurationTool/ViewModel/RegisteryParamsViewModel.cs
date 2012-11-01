using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProxyConfig.DataAccess;
using ProxyConfig.Model;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows;
using System.Xml.Linq;
using org.iringtools.utility;

namespace ProxyConfig.ViewModel
{
    class RegisteryParamsViewModel : ViewModelBase
    {
        // Private variables
        RegisteryRepository _registeryRepository;
        RelayCommand _updateCommand;
        RelayCommand _resetCommand;
        RelayCommand _getIRTFolderCommand;
        public RegisteryRepository RegisteryParams
        {
            get
            {
                return _registeryRepository;
            }
        }

        public RegisteryParamsViewModel(RegisteryRepository registeryRepository)
    {
        if (registeryRepository == null)
      {
        throw new ArgumentNullException("registeryRepository");
      }

        _registeryRepository = registeryRepository;
        _registeryParams = new RegisteryParams();
        _registeryParams.RegAppName = "Registery Configuration Tool";
      Reset();
    }

    public void Reset()
    {
      //_registeryParams.iRingToolsFolder = "";
        _registeryParams.Username = "";
        _registeryParams.Password = "";
      _registeryParams.Domain = "";
      _registeryRepository.Refresh();
      _registeryParams.ProxyHost = _registeryParams.ProxyHost;
      _registeryParams.ProxyPort = _registeryParams.ProxyPort;
    }

    RegisteryParams _registeryParams;
    public RegisteryParams RegisteryParameters
    {
      get
      {
          return _registeryParams;
      }
      set
      {
          _registeryParams = value;
      }
    }

    public string RegAppName
    {
      get
      {
          return _registeryParams.RegAppName;
      }
      set
      {
          _registeryParams.RegAppName = value;
      }
    }

    public string ServicesWebConfig
    {
      get
      {
          return _registeryParams.ServicesWebConfig;
      }
      set
      {
          _registeryParams.ServicesWebConfig = value;
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
          if (String.IsNullOrEmpty(_registeryParams.Domain))
        {
            return _registeryParams.Username;
        }
        else
        {
            return _registeryParams.Domain + @"\" + _registeryParams.Username;
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

        _registeryParams.Username = userName;
        _registeryParams.Domain = domainName;

        //if (String.IsNullOrEmpty(value))
        //  {
        //      throw new ApplicationException("Username is required.");
        //  }
      }
    }

    public string Password
    {
      get
      {
          return _registeryParams.Password;
      }
      set
      {
          _registeryParams.Password = value;
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
          return _registeryParams.ProxyHost;
      }
      set
      {
          _registeryParams.ProxyHost = value;
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
          return _registeryParams.ProxyPort;
      }
      set
      {
          _registeryParams.ProxyPort = value;
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

    void UpdateCommandExecute()
    {
      try
      {
        bool isValid = true;
        if (_registeryParams.ServicesWebConfig == "")
        {
          isValid = false;
          OnPropertyChanged("ServicesWebConfig");
        }

        if (!isValid)
        {
          MessageBox.Show("One or more parameters invalid.", "Validation Failed", MessageBoxButton.OK, MessageBoxImage.Information);
          return;
        }

        XDocument adminConfig = XDocument.Load(_registeryParams.ServicesWebConfig);

        if (String.IsNullOrEmpty(_registeryParams.ProxyHost))
        {
            adminConfig.RemoveSetting("RegistryCredentialToken");
         // adminConfig.RemoveSetting("ProxyHost");
         // adminConfig.RemoveSetting("ProxyPort");
        }
        else
        {
            if (!String.IsNullOrEmpty(_registeryParams.Username))
          {
            WebCredentials credentials = new WebCredentials
            {
                userName = _registeryParams.Username,
                password = _registeryParams.Password,
                domain = _registeryParams.Domain
            };
            credentials.Encrypt();

            adminConfig.UpdateSetting("RegistryCredentialToken", credentials.encryptedToken);
          }
          else
          {
            adminConfig.RemoveSetting("MCraftFileServer");
          }

            string proxyPort = _registeryParams.ProxyPort;
          if (String.IsNullOrEmpty(proxyPort)) proxyPort = "8080";
          _registeryParams.ProxyPort = proxyPort;
          OnPropertyChanged("ProxyPort");

         // adminConfig.UpdateSetting("ProxyHost", _registeryParams.ProxyHost);
         // adminConfig.UpdateSetting("ProxyPort", proxyPort);
        }

        adminConfig.Save(_registeryParams.ServicesWebConfig);

        MessageBox.Show("Update complete.", "Update Complete", MessageBoxButton.OK, MessageBoxImage.Information);
      }
      catch (Exception exc)
      {
        MessageBox.Show("Error occurred while getting updating proxy configuration. " + exc.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
      }

      // sample code
      //bool isupdated = true;

      //if (_registeryParams.iRingToolsFolder!="")
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

    bool UpdateCommandCanExecute
    {
      get
      {
        //if (_registeryParams.Username!="")
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
          _registeryParams.ServicesWebConfig = dlg.FileName;
          OnPropertyChanged("ServicesWebConfig");

          XDocument adminConfig = XDocument.Load(_registeryParams.ServicesWebConfig);

          string encryptedCredentials = adminConfig.GetSetting("RegistryCredentialToken");

          WebCredentials credentials = new WebCredentials(encryptedCredentials);
          if (credentials.isEncrypted) credentials.Decrypt();

          this.Username = credentials.domain + @"\" + credentials.userName;
          OnPropertyChanged("Username");

          this.Password = credentials.password;
          OnPropertyChanged("Password");

         // this.ProxyHost = adminConfig.GetSetting("ProxyHost");
          OnPropertyChanged("ProxyHost");

         // this.ProxyPort = adminConfig.GetSetting("ProxyPort");
          OnPropertyChanged("ProxyPort");
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

  //public static class MyRegExtensions
  //{
  //  public static void UpdateSetting(this XDocument adminConfig, string key, string value)
  //  {
  //    XElement setting = (
  //           from c in adminConfig.Descendants("appSettings").Descendants("add")
  //           where c.Attribute("key").Value.Equals(key)
  //           select c
  //         ).FirstOrDefault();

  //    if (setting == null)
  //    {
  //      setting = new XElement(
  //        "add",
  //        new XAttribute("key", key),
  //        new XAttribute("value", value)
  //      );

  //      adminConfig.Root.Element("appSettings").Add(setting);
  //    }
  //    else
  //    {
  //      setting.Attribute("value").Value = value;
  //    }
  //  }

  //  public static void RemoveSetting(this XDocument adminConfig, string key)
  //  {
  //    XElement setting = (
  //           from c in adminConfig.Descendants("appSettings").Descendants("add")
  //           where c.Attribute("key").Value.Equals(key)
  //           select c
  //         ).FirstOrDefault();

  //    if (setting != null)
  //    {
  //      setting.Remove();
  //    }
  //  }

  //  public static string GetSetting(this XDocument adminConfig, string key)
  //  {
  //    XElement setting = (
  //           from c in adminConfig.Descendants("appSettings").Descendants("add")
  //           where c.Attribute("key").Value.Equals(key)
  //           select c
  //         ).FirstOrDefault();

  //    if (setting == null)
  //    {
  //      return String.Empty;
  //    }
  //    else
  //    {
  //      return setting.Attribute("value").Value;
  //    }
  //  }
  //}
}

