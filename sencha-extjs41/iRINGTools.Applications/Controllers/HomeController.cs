using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net;

namespace org.iringtools.web.controllers
{
  public class HomeController : BaseController
  {
    //
    // GET: /Home/

    public ActionResult Index()
    {
      return View();
    }

    public ActionResult SPARQLQuery()
    {
      return View();
    }

  }
}
