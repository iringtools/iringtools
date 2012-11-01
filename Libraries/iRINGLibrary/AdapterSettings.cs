using System;
using System.Linq;
using System.ServiceModel;
using org.iringtools.library;
using StaticDust.Configuration;
using org.iringtools.utility;
using System.Collections;

namespace org.iringtools.adapter
{
  public class AdapterSettings : ServiceSettings
  {
    public AdapterSettings() : base()
    {
      this.Add("InterfaceService", @"http://localhost/services/facade/query");
      this.Add("ReferenceDataServiceUri", @"http://localhost/services/refdata");
      this.Add("JavaCoreUri", @"http://localhost/services/dir");
      this.Add("DefaultProjectionFormat", "json");
      this.Add("DefaultListProjectionFormat", "json");
      this.Add("EndpointTimeout", "30000");
      this.Add("dotNetRDFServer", @".\SQLEXPRESS");
      this.Add("dotNetRDFCatalog", "FacadeDb");
      this.Add("dotNetRDFUser", "dotNetRDF");
      this.Add("dotNetRDFPassword", "dotNetRDF");
      this.Add("TrimData", "False");
      this.Add("DumpSettings", "False");
      this.Add("ExecutingAssemblyName", "App_Code");
      this.Add("DefaultStyleSheet", @".\App_Data\default.css");
      this.Add("ValidateLinks", "False");
      this.Add("DisplayLinks", "False");

      if (OperationContext.Current != null)
      {
        string baseAddress = OperationContext.Current.Host.BaseAddresses[0].ToString();

        if (!baseAddress.EndsWith("/"))
            baseAddress = baseAddress + "/";

        this.Add("GraphBaseUri", baseAddress);
      }
      else
      {
        this.Add("GraphBaseUri", @"http://localhost:54321/data");
        //this.Add("GraphBaseUri", @"http://yourcompany.com/");
      }
    }

    //Append Scope specific {projectName}.{appName}.config settings.
    public void AppendSettings(AppSettingsReader settings)
    {
      foreach (string key in settings.Keys)
      {
        if (key.Equals("GraphBaseUri"))
        {
          string baseAddress = settings[key].ToString();
          
          if (!baseAddress.EndsWith("/"))
            baseAddress = baseAddress + "/";
          
          this[key] = baseAddress;
        }

        if (key.Equals("DefaultProjectionFormat") ||
            key.Equals("ValidateLinks") ||
            key.Equals("DisplayLinks"))
        {
          string format = settings[key].ToString();
          this[key] = format;
        }

        //Protect existing settings, but add new ones.
        if (!this.AllKeys.Contains(key, StringComparer.CurrentCultureIgnoreCase))
        {
          this.Add(key, settings[key].ToString());
        }
        else if (this[key] == String.Empty)
          this[key] = settings[key].ToString();
      }
    }

    //Append KeyRing from IdentityProvider.
    public void AppendKeyRing(IDictionary keyRing)
    {
      if (keyRing != null)
      {
        foreach (string key in keyRing.Keys)
        {
          object valueObj = keyRing[key];

          string value = String.Empty;
          if (valueObj != null)
            value = valueObj.ToString();

          //Protect existing settings, but add new ones.
          if (!this.AllKeys.Contains(key, StringComparer.CurrentCultureIgnoreCase))
          {
            this.Add(key, value);
          }
        }
      }
    }
  }  
}
