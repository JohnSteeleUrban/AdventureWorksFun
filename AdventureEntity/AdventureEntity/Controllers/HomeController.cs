using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AdventureEntity.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
        public ActionResult Report()
        {
            //TODO open in new page and keep old.
            //Response.Write("<script>");
            //Response.Write(@"window.open('/Reports/SalesTerritory.aspx','_blank')");
            //Response.Write("</script>");
            Response.Redirect(@"~/Reports/SalesTerritory.aspx");
            return new EmptyResult();
        }
    }
}