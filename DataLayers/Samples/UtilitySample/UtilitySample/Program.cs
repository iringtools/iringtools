using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Xml.Linq;
using System.Web;
using org.iringtools.adapter;
using org.iringtools.library;
using org.iringtools.utility;
using org.iringtools.sdk.objects.widgets;
using System.Text;
using System.Collections;
using System.Net;
using System.IO;

namespace org.iringtools.sdk.objects
{
  class Program
  {
    private WebHttpClient _widgetServiceClient = null;
    private string numberOfWidgets;
    private string _fileName = @".\ExampleOfUsingWidgetsServices.txt";
    private System.IO.StreamWriter file;

    public Program()
    {
      //Adapter Service does all this...
      Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
      file = new System.IO.StreamWriter(_fileName);
      NameValueCollection appSettings = System.Configuration.ConfigurationManager.AppSettings;
      ServiceSettings settings = new ServiceSettings();
      settings.AppendSettings(appSettings);

      //This is how DataLayers should use settings...
      WebProxyCredentials webProxyCredentials = settings.GetWebProxyCredentials();

      _widgetServiceClient = new WebHttpClient(settings["WidgetServiceUri"],
        webProxyCredentials.userName,
        webProxyCredentials.password,
        webProxyCredentials.domain,
        webProxyCredentials.proxyHost,
        webProxyCredentials.proxyPort);
      
      //In a DataLayer this is automatic and is not required
      _widgetServiceClient.AccessToken = settings["AccessToken"];
    }

    public void ShowGetAllWidgets()
    {
      Widgets widgets = _widgetServiceClient.Get<Widgets>("");
      numberOfWidgets = widgets.Count.ToString();
      Write(widgets, "Example of getting all widgets using Widgets Services\r\n" + "The widgets are showing bellow:\r\n");
    }

    public void ShowGetWidgetByIdentifier()
    {
      Widgets widgets = _widgetServiceClient.Get<Widgets>("");
      Widget widget = _widgetServiceClient.Get<Widget>(string.Format("/{0}", widgets[0].Id));
      Write(widget, "Example of getting a widget using Widgets Services\r\n" + "The widget is showing bellow:\r\n");
    }

    public void ShowCreateWidget()
    {
      int indexOfWidget = int.Parse(numberOfWidgets) + 1;
      Widget widget = new Widget
        {
          Id = indexOfWidget,
          Name = "Thing" + numberOfWidgets,
          Description = "Sample Object" + numberOfWidgets,
          Color = Color.Green,
          Material = "Oak Wood",
          Length = 3.14,
          Height = 4.0,
          Width = 5.25,
          LengthUOM = LengthUOM.inch,
          Weight = 10,
          WeightUOM = WeightUOM.pounds
        };

      _widgetServiceClient.Post<Widget>("", widget, true);

      Widget addedWidget = _widgetServiceClient.Get<Widget>(indexOfWidget.ToString());
      Write(addedWidget, "Example of adding a new widget to the end of the widgets list using Widgets Services\r\n" + "Added widget is showing following:\r\n");
    }

    public void ShowUpdateWidgets()
    {
      Widgets exitingWidgets = _widgetServiceClient.Get<Widgets>("");
      Widgets widgets = new Widgets
        {
          new Widget
          {
            Id = exitingWidgets[0].Id,
            Name = "Thing1",
            Description = "Sample Object 1",
            Color = Color.Black,
            Material = "Pine Wood",
            Length = 8,
            Height = 9.0,
            Width = 6.3,
            LengthUOM = LengthUOM.meter,
            Weight = 25,
            WeightUOM = WeightUOM.kilograms
          },
          new Widget
          {
            Id = exitingWidgets[1].Id,
            Name = "Thing2",
            Description = "Sample Object 2",
            Color = Color.Violet,
            Material = "Plastic",
            Length = 5.38,
            Height = 40.8,
            Width = 28,
            LengthUOM = LengthUOM.inch,
            Weight = 40,
            WeightUOM = WeightUOM.grams
          }
        };

      string response = _widgetServiceClient.Post<Widgets>("/update", widgets, true);

      Widgets modifiedWidgets = _widgetServiceClient.Get<Widgets>("");
      Widget widget1 = _widgetServiceClient.Get<Widget>(string.Format("/{0}", modifiedWidgets[0].Id));
      Widget widget2 = _widgetServiceClient.Get<Widget>(string.Format("/{0}", modifiedWidgets[1].Id));
      Widgets widgetsUpdated = new Widgets();
      widgetsUpdated.Add(widget1);
      widgetsUpdated.Add(widget2);
      Write(widgetsUpdated, "Example of updating existing widgets using Widgets Services\r\n" + "Updated widges are showing bellow:\r\n");
    }

    public void ShowDeleteWidgetByIdentifier()
    {
      file.WriteLine("Example of deleting a widget using Widgets Services:\r\n");
      Widgets widgets = _widgetServiceClient.Get<Widgets>("");
      file.WriteLine("Before deletion, the number of exitsing widgets are " + widgets.Count.ToString());
      _widgetServiceClient.Get<string>(string.Format("/delete/{0}", widgets[0].Id));
      widgets = _widgetServiceClient.Get<Widgets>("");
      file.WriteLine("After deletion, the number of exitsing widgets are " + widgets.Count.ToString());
    }

    private void Write<T>(T obj, string msg)
    {      
      XDocument xDocument = ToXml(obj);
      file.Write(msg);
      file.Write("\r\n");
      file.Write(xDocument.ToString() + "\r\n");
      file.Write("\r\n");
    }

    public void CloseFile()
    {
      file.Close();
    }

    private XDocument ToXml<T>(T widgets)
    {
      XDocument xDocument = null;

      try
      {
        string xml = Utility.SerializeDataContract<T>(widgets);
        XElement xElement = XElement.Parse(xml);
        xDocument = new XDocument(xElement);
      }
      catch (Exception)
      {        
      }

      return xDocument;
    }

    static void Main(string[] args)
    {
      Program program = new Program();
      program.ShowGetAllWidgets();
      program.ShowGetWidgetByIdentifier();
      program.ShowUpdateWidgets();
      program.ShowDeleteWidgetByIdentifier();
      program.CloseFile();
    }
  }
}
