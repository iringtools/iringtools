using System;
using System.Web.Mvc;
using org.iringtools.web.Models;
using org.iringtools.web.controllers;
using org.iringtools.library;

namespace org.iringtools.web.Controllers
{
  public class FacadeController : BaseController
  {
    private IFacadeRepository _facadeRepository = null;

    public FacadeController() : this(new FacadeRepository()) { }

    public FacadeController(IFacadeRepository repository)
    {
      _facadeRepository = repository;
    }

    public JsonResult RefreshFacade(FormCollection form)
    {
      var result = new JsonResult();
      try
      {
        //string[] vars = form["scope"].Split('/');
        string scope = form["contextName"];
        string app = form["endpoint"];
        string graph = form["graph"];
        string baseUrl = form["baseUrl"];
        Response resp = _facadeRepository.RefreshGraph(scope, app, graph, baseUrl);
      }
      catch
      {
        return Json(new { success = false }, JsonRequestBehavior.AllowGet);
      }
      return Json(new { success = true }, JsonRequestBehavior.AllowGet);
    }

  }
}
