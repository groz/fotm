using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FotM.Domain;
using FotM.Portal.ViewModels;

namespace FotM.Portal.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "World of Warcraft Armory monitoring.";

            return View();
        }
    }
}