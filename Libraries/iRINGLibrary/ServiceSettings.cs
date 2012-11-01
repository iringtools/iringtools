using System;
using System.Collections.Specialized;
using org.iringtools.utility;
using System.Net;
using System.ServiceModel;
using org.iringtools.mapping;
using System.IO;
using log4net;

namespace org.iringtools.library
{
  public class ServiceSettings : NameValueCollection
  {
    private static readonly ILog _logger = LogManager.GetLogger(typeof(ServiceSettings));
    
    public ServiceSettings()
    {
      this.Add("BaseDirectoryPath", AppDomain.CurrentDomain.BaseDirectory);
      this.Add("AppCodePath", @".\App_Code\");
      this.Add("AppDataPath", @".\App_Data\");
      this.Add("XmlPath", this["AppDataPath"]);  // for backward compatibility
      this.Add("DataLayersPath", @".\App_Data\DataLayers\");
      this.Add("ProxyCredentialToken", String.Empty);
      this.Add("ProxyHost", String.Empty);
      this.Add("ProxyPort", String.Empty);
      this.Add("IgnoreSslErrors", "True");
      this.Add("PrimaryClassificationStyle", "Type");
      this.Add("SecondaryClassificationStyle", "Template");
      this.Add("ClassificationTemplateFile", this["AppDataPath"] + "ClassificationTemplate.xml");

      if (OperationContext.Current != null)
      {
        string baseAddress = OperationContext.Current.Host.BaseAddresses[0].ToString();

        if (!baseAddress.EndsWith("/"))
          baseAddress = baseAddress + "/";

        this.Add("BaseAddress", baseAddress);
      }
      else
      {
        this.Add("BaseAddress", @"http://www.example.com/");
      }
    }

    //Append Web.config settings
    public void AppendSettings(NameValueCollection settings)
    {
      foreach (string key in settings.AllKeys)
      {
        if (key.Equals("PrimaryClassificationStyle") || key.Equals("SecondaryClassificationStyle"))
        {
          if (key.Equals("PrimaryClassificationStyle") && settings[key].ToString() == ClassificationStyle.Template.ToString())
            throw new Exception("Primary Classification Style value can only be 'Type' or 'Both'!");

          if (settings[key].ToString().ToUpper() == ClassificationStyle.Type.ToString().ToUpper() ||
              settings[key].ToString().ToUpper() == ClassificationStyle.Template.ToString().ToUpper() ||
              settings[key].ToString().ToUpper() == ClassificationStyle.Both.ToString().ToUpper())
          {
            this[key] = Utility.TitleCase(settings[key].ToString());  // override the default
          }
        }
        else
        {
          //Override existing settings, and create new ones
          this.Set(key, settings[key]);
        }
      }

      //Consolidate legacy XML folder to App_Data folder
      if (this["AppDataPath"] == @".\App_Data\" && Directory.Exists(@".\XML\"))
      {
        try
        {
          string[] srcFiles = Directory.GetFiles(@".\XML\", "*.*");

          foreach (string srcFile in srcFiles)
          {
            string fileName = Path.GetFileName(srcFile);
            string destFile = Path.Combine(this["AppDataPath"], fileName);

            if (File.Exists(destFile))
              File.Delete(srcFile);
            else
              File.Move(srcFile, destFile);
          }
        }
        catch (Exception e)
        {
          _logger.Warn("Error moving legacy XML files to App Data: " + e);
        }
      }
    }

    public NetworkCredential GetProxyCredential()
    {
      return GetWebProxyCredentials().GetNetworkCredential();
    }

    public WebProxyCredentials GetWebProxyCredentials()
    {
      WebProxyCredentials proxyCredentials = null;

      if (String.IsNullOrEmpty( this["ProxyCredentialToken"]) )
      {
        proxyCredentials = new WebProxyCredentials();
      }
      else
      {
        // Settings included an encrypted credential string so hopefully the ProxyPort and ProxyHost have also been provided
        // Note that ProxyBypassOnLocal and ProxyBypassList are optional
        int portNumber = 8080;
        Int32.TryParse(this["ProxyPort"], out portNumber);

        proxyCredentials = new WebProxyCredentials(
          this["ProxyCredentialToken"],
          this["ProxyHost"], 
          portNumber,
          this["ProxyBypassOnLocal"],
          this["ProxyBypassList"]);
      }

      return proxyCredentials;
    }
  }  
}
