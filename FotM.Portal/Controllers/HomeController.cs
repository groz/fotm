using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FotM.Domain;

namespace FotM.Portal.Controllers
{
    public class HomeController : Controller
    {
        private readonly Player[] _players = new[]
        {
            new Player(), 
        };

        private readonly List<Team> _currentTeams = new List<Team>()
        {
            new Team()
        };
        
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