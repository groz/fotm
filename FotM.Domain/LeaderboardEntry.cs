using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FotM.Utilities;

namespace FotM.Domain
{
    public class LeaderboardEntry : IEquatable<LeaderboardEntry>
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

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((LeaderboardEntry) obj);
        }

        public bool Equals(LeaderboardEntry other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Rating == other.Rating && string.Equals(Name, other.Name) && RealmId == other.RealmId &&
                   SeasonLosses == other.SeasonLosses && SeasonWins == other.SeasonWins;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = Rating;
                hashCode = (hashCode*397) ^ (Name != null ? Name.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ RealmId;
                hashCode = (hashCode*397) ^ SeasonLosses;
                hashCode = (hashCode*397) ^ SeasonWins;
                return hashCode;
            }
        }
    }
}