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
                previous.RealmSlug,
                previous.SpecId,
                previous.ClassId
            );
        }

        protected static LeaderboardEntry CreateEntry(int ranking, string name, int rating,
            int weeklyWins = 0, int weeklyLosses = 0, int seasonWins = 0, int seasonLosses = 0,
            string realmName = null, int realmId = -1, string realmSlug = null, int specId = -1, int classId = -1)
        {
            return new LeaderboardEntry()
            {
                WeeklyLosses = weeklyLosses,
                WeeklyWins = weeklyWins,
                SeasonLosses = seasonLosses,
                SeasonWins = seasonWins,
                Name = name,
                ClassId = classId,
                SpecId = specId,
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
                Rows = entries.ToArray(),
                Bracket = this.Bracket,
                Time = DateTime.Now
            };

            leaderboard.Order();

            return leaderboard;
        }

    }
}