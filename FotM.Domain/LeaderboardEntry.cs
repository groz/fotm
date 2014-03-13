using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FotM.Domain
{
    public class LeaderboardEntry
    {
        /*
        {
            "ranking" : 1,
            "rating" : 2654,
            "name" : "Zunniyaki",
            "realmId" : 59,
            "realmName" : "Mal'Ganis",
            "realmSlug" : "malganis",
            "raceId" : 5,
            "classId" : 5,
            "specId" : 258,
            "factionId" : 1,
            "genderId" : 0,
            "seasonWins" : 59,
            "seasonLosses" : 19,
            "weeklyWins" : 12,
            "weeklyLosses" : 4
        }
        */

        public int Ranking { get; set; }
        public int Rating { get; set; }
        public string Name { get; set; }
        public int RealmId { get; set; }
        public string RealmName { get; set; }
        public string RealmSlug { get; set; }
        public int RaceId { get; set; }
        public int ClassId { get; set; }
        public int SpecId { get; set; }
        public int FactionId { get; set; }
        public int GenderId { get; set; }
        public int SeasonWins { get; set; }
        public int SeasonLosses { get; set; }
        public int WeeklyWins { get; set; }
        public int WeeklyLosses { get; set; }

        public override string ToString()
        {
            return string.Format("Name:{0}, Ranking: {1}, Rating: {2}", Name, Ranking, Rating);
        }
    }
}
