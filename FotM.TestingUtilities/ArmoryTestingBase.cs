using System;
using System.Linq;
using FotM.Domain;

namespace FotM.TestingUtilities
{
    public class ArmoryTestingBase
    {
        protected readonly Bracket Bracket;

        public ArmoryTestingBase(Bracket bracket)
        {
            this.Bracket = bracket;
        }

        protected static LeaderboardEntry UpdateEntry(LeaderboardEntry previous, int ratingChange)
        {
            int change = Math.Sign(ratingChange);

            return CreateEntry(
                previous.Ranking - change, 
                previous.Name, 
                previous.Rating + ratingChange, 
                previous.WeeklyWins + (change > 0 ? 1 : 0), 
                previous.WeeklyLosses + (change > 0 ? 0 : 1),
                previous.SeasonWins + (change > 0 ? 1 : 0),
                previous.SeasonLosses + (change > 0 ? 0 : 1),
                previous.RealmName,
                previous.RealmId,
                previous.RealmSlug
            );
        }

        protected static LeaderboardEntry CreateEntry(int ranking, string name, int rating,
            int weeklyWins = 0, int weeklyLosses = 0, int seasonWins = 0, int seasonLosses = 0,
            string realmName = null, int realmId = -1, string realmSlug = null)
        {
            return new LeaderboardEntry()
            {
                WeeklyLosses = weeklyLosses,
                WeeklyWins = weeklyWins,
                SeasonLosses = seasonLosses,
                SeasonWins = seasonWins,
                Name = name,
                ClassId = 0,
                SpecId = 0,
                Rating = rating,
                Ranking = ranking,
                RealmId = realmId == -1 ? 1 : realmId,
                RealmName = string.IsNullOrEmpty(realmName) ? "TestRealm" : realmName,
                RealmSlug = string.IsNullOrEmpty(realmSlug) ? "TestRealmSlug" : realmSlug,
            };
        }

        protected Leaderboard CreateLeaderboard(params LeaderboardEntry[] entries)
        {
            var leaderboard = new Leaderboard()
            {
                Rows = entries.Union(StaticRankings).ToArray(),
                Bracket = this.Bracket,
                Time = DateTime.Now
            };

            leaderboard.Order();

            return leaderboard;
        }

        protected static readonly LeaderboardEntry[] StaticRankings =
        {
            CreateEntry(10, "Gothiques", 2500),
            CreateEntry(11, "Phenomenon", 2400),
            CreateEntry(12, "Joker", 2300),
            CreateEntry(503, "Nub", 1500)
        };
    }
}