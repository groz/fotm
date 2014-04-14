using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FotM.Portal.Infrastructure;
using FotM.Portal.ViewModels;

namespace FotM.Portal.Controllers
{
    [RoutePrefix("3v3")]
    public class ThreesController : Controller
    {
        [Route]
        [Route("~/")]
        [Route("Index")]
        public ActionResult Index()
        {
            ArmoryViewModel latestArmoryViewModel = ReactiveUpdateManager.Instance.GetLatestViewModel();

            ViewBag.Media = new MediaViewModel();

            return View(latestArmoryViewModel);
        }
	}
}