using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ServiceModel.Activation;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.ServiceModel.Description;
using System.ServiceModel.Channels;

namespace org.iringtools.adapter
{
   public class RawServiceHostFactory : WebServiceHostFactory
  {
    protected override ServiceHost CreateServiceHost(Type serviceType, Uri[] baseAddresses)
    {
      WebServiceHost webServiceHost = new RawServiceHost(serviceType, baseAddresses);

      return webServiceHost;
    }
  }

   public class RawServiceHost : WebServiceHost
   {
        public RawServiceHost()
            : base()
        {
        }
  
        public RawServiceHost(object singletonInstance, params Uri[] baseAddresses)
            : base(singletonInstance, baseAddresses)
        {
        }

        public RawServiceHost(Type serviceType, params Uri[] baseAddresses) :
            base(serviceType, baseAddresses)
        {
        }

     protected override void OnOpening()
     {
       base.OnOpening();

       if (this.Description == null)
       {
         return;
       }

       // for both user-defined and automatic endpoints, ensure they have the right behavior and content type mapper added
       foreach (ServiceEndpoint serviceEndpoint in this.Description.Endpoints)
       {
         if (serviceEndpoint.Binding != null && serviceEndpoint.Binding.CreateBindingElements().Find<WebMessageEncodingBindingElement>() != null)
         {
           SetRawContentTypeMapper(serviceEndpoint, true);
           if (serviceEndpoint.Behaviors.Find<WebHttpBehavior>() == null)
           {
             serviceEndpoint.Behaviors.Add(new WebHttpBehavior());
           }
         }
       }
     }

     internal static void SetRawContentTypeMapper(ServiceEndpoint endpoint, bool isDispatch)
     {
       Binding binding = endpoint.Binding;
       ContractDescription contract = endpoint.Contract;
       if (binding == null)
       {
         return;
       }
       CustomBinding customBinding = new CustomBinding(binding);
       BindingElementCollection elements = customBinding.Elements;
       WebMessageEncodingBindingElement encodingElement = elements.Find<WebMessageEncodingBindingElement>();
       if (encodingElement == null || encodingElement.ContentTypeMapper != null)
       {
         return;
       }
       
       encodingElement.ContentTypeMapper = new RawContentTypeMapper();
       endpoint.Binding = customBinding;

     }
   }
}