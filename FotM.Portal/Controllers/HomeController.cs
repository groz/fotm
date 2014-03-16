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
            new Player(new LeaderboardEntry()
            {
                FactionId = (int)Faction.Alliance,
                GenderId = (int)Gender.Male,
                RaceId = (int)Race.Dwarf,
                ClassId = (int)CharacterClass.Warrior,
                SpecId = (int)CharacterSpec.Warrior_Arms,
                Rating = 2350,
                Name = "Mortalstriek",
                Ranking = 1
            }), 
            new Player(new LeaderboardEntry()
            {
                FactionId = (int)Faction.Alliance,
                GenderId = (int)Gender.Female,
                RaceId = (int)Race.NightElf,
                ClassId = (int)CharacterClass.Druid,
                SpecId = (int)CharacterSpec.Druid_Restoration,
                Rating = 2125,
                Name = "Lifebloom",
                Ranking = 2
            }), 
            new Player(new LeaderboardEntry()
            {
                FactionId = (int)Faction.Alliance,
                GenderId = (int)Gender.Female,
                RaceId = (int)Race.Dwarf,
                ClassId = (int)CharacterClass.Priest,
                SpecId = (int)CharacterSpec.Priest_Holy,
                Rating = 2250,
                Name = "Fearward",
                Ranking = 3
            }), 
        };

        private readonly List<Team> _currentTeams;
        
        public HomeController()
        {
            _currentTeams = new List<Team>()
            {
                new Team(_players[0], _players[1], _players[2])
            };
        }

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