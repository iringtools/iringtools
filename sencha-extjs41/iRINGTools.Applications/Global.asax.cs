using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

using Ninject;
using Ninject.Modules;
using Ninject.Web.Mvc;

using iRINGTools.Web.Models;

namespace iRINGTools.Web
{
  // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
  // visit http://go.microsoft.com/?LinkId=9394801

  public class MvcApplication : NinjectHttpApplication
  {
    private bool _isUnauthorized = false;

    public static void RegisterRoutes(RouteCollection routes)
    {
      routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
      routes.IgnoreRoute("{*favicon}", new { favicon = @"(.*/)?favicon.ico(/.*)?" });

      routes.MapRoute(
          "MappingRoute", // Route name
          "mapping/{action}/{scope}/{application}/{graphMap}", // URL with parameters
          new { controller = "Mapping", action = "Index", scope = "", application = "", graphMap = UrlParameter.Optional } // Parameter defaults
      );

      routes.MapRoute(
          "Default", // Route name
          "{controller}/{action}/{id}", // URL with parameters
          new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
      );

    }

    protected override void OnApplicationStarted()
    {
      AreaRegistration.RegisterAllAreas();
      RegisterRoutes(RouteTable.Routes);
    }

    protected void Application_BeginRequest(object sender, EventArgs e)
    {
      _isUnauthorized = false;
    }

    protected void Application_EndRequest()
    {
      if (_isUnauthorized)
      {
        Context.Response.Clear();
        Context.Response.StatusCode = 401;
        Context.Response.Write("Unauthorized");
      }
    }

    protected void Application_Error(Object sender, System.EventArgs e)
    {
      Exception exception = Server.GetLastError();

      if (exception.GetType() == typeof(UnauthorizedAccessException))
      {
        _isUnauthorized = true;
      }
    }

    protected override IKernel CreateKernel()
    {
      var modules = new INinjectModule[]
      {
          new ServiceModule()
      };

      return new StandardKernel(modules);
    }
  }

  internal class ServiceModule : NinjectModule
  {
    public override void Load()
    {
      //Bind<IFormsAuthentication>().To<FormsAuthenticationService>();
      //Bind<IMembershipService>().To<AccountMembershipService>();
      //Bind<MembershipProvider>().ToConstant(Membership.Provider);
      Bind<IAdapterRepository>().To<AdapterRepository>();
     // Bind<org.iringtools.adapter.datalayer.ISPPIDRepository>().To<org.iringtools.adapter.datalayer.SPPIDRepository>();
    }
  }
}