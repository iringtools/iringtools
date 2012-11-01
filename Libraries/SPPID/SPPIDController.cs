using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using System.Collections.Specialized;

namespace org.iringtools.adapter.datalayer
{
    public class SPPIDController : Controller
    {
        private NameValueCollection _settings = null;
        private ISPPIDRepository _repository { get; set; }
        //private string _keyFormat = "Configuration.{0}.{1}";

        public SPPIDController()
            : this(new SPPIDRepository())
        {
        }
         public SPPIDController(ISPPIDRepository repository)
    {
      _settings = ConfigurationManager.AppSettings;
      _repository = repository;
    }
        //
        // GET: /SPPID/

        public ActionResult Index()
        {
            return View();
        }
        public JsonResult UpdateConfig(FormCollection form)
        {
            string siteConn=string.Format(@"user id={0};password={1};Data Source={2}\{3};Initial Catalog={4}",form["dbUserName"],form["dbPassword"],form["dbServer"],form["dbInstance"],form["dbName"]);
            string plantConn=string.Format(@"user id={0};password={1};Data Source={2}\{3};Initial Catalog={4}",form["dbplantUserName"],form["dbplantPassword"],form["dbplantServer"],form["dbplantInstance"],form["dbplantName"]);
            string staggConn = string.Format(@"user id={0};password={1};Data Source={2}\{3};Initial Catalog={4}", form["dbstageUserName"], form["dbstagePassword"], form["dbstageServer"], form["dbstageInstance"], form["dbstageName"]);
             string success = _repository.UpdateConfig(form["scope"], form["app"],form["_datalayer"],siteConn,plantConn,staggConn);
           

            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }

    }
}
