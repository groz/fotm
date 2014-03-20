using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FotM.Domain;
using FotM.Portal.Infrastructure;
using FotM.Portal.ViewModels;
using Newtonsoft.Json;

namespace FotM.Portal.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ArmoryViewModel latestArmoryViewModel = ReactiveUpdateManager.Instance.GetLatestViewModel();
            return View(latestArmoryViewModel);
        }

        public ActionResult About()
        {
            ViewBag.Message = "World of Warcraft Armory monitoring.";

            return View();
        }
    }
}