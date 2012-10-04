using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Text;
using org.iringtools.sdk.objects.widgets;

namespace WidgetService
{
    // Start the service and browse to http://<machine_name>:<port>/Service1/help to view the service's generated help page
    // NOTE: By default, a new instance of the service is created for each call; change the InstanceContextMode to Single if you want
    // a single instance of the service to process all calls.	
    [ServiceContract]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    // NOTE: If the service is renamed, remember to update the global.asax.cs file
    public class WidgetService
    {
        private WidgetProvider _widgetProvider = null;
        public WidgetService()
        {
          _widgetProvider = new WidgetProvider();
        }       

        [WebInvoke(UriTemplate = "", Method = "POST")]
        public Widget Create(Widget instance)
        {
          return _widgetProvider.CreateWidget(instance);
        }

        [WebGet(UriTemplate = "")]
        public Widgets GetAll()
        {
          return _widgetProvider.ReadWidgets(null);            
        }

        [WebGet(UriTemplate = "{identifier}")]
        public Widget Get(string identifier)
        {
          Widget widget = _widgetProvider.ReadWidget(Int32.Parse(identifier));
          return widget;         
        }

        [WebInvoke(UriTemplate = "/update", Method = "POST")]
        public int Update(Widgets widgets)
        {
          int response = _widgetProvider.UpdateWidgets(widgets);
          return response;
        }

        [WebInvoke(UriTemplate = "/delete/{identifier}", Method = "GET")]
        public int Delete(string identifier)
        {
          int response = _widgetProvider.DeleteWidgets(Int32.Parse(identifier));
          return response;
        }
    }
}
