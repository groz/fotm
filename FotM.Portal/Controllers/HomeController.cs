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
                Rating = 2225,
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
                Rating = 2150,
                Name = "Fearward",
                Ranking = 3
            }), 
            new Player(new LeaderboardEntry()
            {
                FactionId = (int)Faction.Horde,
                GenderId = (int)Gender.Female,
                RaceId = (int)Race.BloodElf,
                ClassId = (int)CharacterClass.Paladin,
                SpecId = (int)CharacterSpec.Paladin_Holy,
                Rating = 2050,
                Name = "Holinka",
                Ranking = 4
            }), 
            new Player(new LeaderboardEntry()
            {
                FactionId = (int)Faction.Horde,
                GenderId = (int)Gender.Male,
                RaceId = (int)Race.Orc,
                ClassId = (int)CharacterClass.Hunter,
                SpecId = (int)CharacterSpec.Hunter_Marksmanship,
                Rating = 2025,
                Name = "Hunter",
                Ranking = 5
            }), 
            new Player(new LeaderboardEntry()
            {
                FactionId = (int)Faction.Horde,
                GenderId = (int)Gender.Male,
                RaceId = (int)Race.Orc,
                ClassId = (int)CharacterClass.Shaman,
                SpecId = (int)CharacterSpec.Shaman_Enhancement,
                Rating = 2015,
                Name = "Sham",
                Ranking = 6
            }), 
        };

        private readonly List<TeamStats> _teamsStats;
        
        public HomeController()
        {
            _teamsStats = new List<TeamStats>()
            {
                new TeamStats(new Team(_players[0], _players[1], _players[2]), 2020, +20),
                new TeamStats(new Team(_players[3], _players[4], _players[5]), 2010, -20),
            };
        }

        public ActionResult Index()
        {
            return View(_teamsStats);
        }

        public ActionResult About()
        {
            ViewBag.Message = "World of Warcraft Armory monitoring.";

            return View();
        }
    }
}