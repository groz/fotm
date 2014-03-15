using System;
using System.Linq;
using FotM.Domain;

namespace FotM.Cassandra.Tests
{
    public class CassandraTestBase
    {
        protected readonly Bracket Bracket = Bracket.Rbg;

        public CassandraTestBase(Bracket bracket)
        {
            this.Bracket = bracket;
        }

        protected static LeaderboardEntry UpdateEntry(LeaderboardEntry previous, int ratingChange)
        {
            int change = Math.Sign(ratingChange);

            return CreateEntry(
                previous.Ranking - change, 
                previous.Name, 
                previous.Rating - ratingChange, 
                previous.WeeklyWins + (change > 0 ? 1 : 0), 
                previous.WeeklyLosses - (change > 0 ? 1 : 0),
                previous.SeasonWins + (change > 0 ? 1 : 0),
                previous.SeasonLosses - (change > 0 ? 1 : 0)
            );
        }

        protected static LeaderboardEntry CreateEntry(int ranking, string name, int rating,
            int weeklyWins = 0, int weeklyLosses = 0, int seasonWins = 0, int seasonLosses = 0)
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
                RealmId = 1,
                RealmName = "TestRealm"
            };
        }

        protected Leaderboard CreateLeaderboard(params LeaderboardEntry[] entries)
        {
            return new Leaderboard()
            {
                Rows = entries.Union(StaticRankings).ToArray(),
                Bracket = this.Bracket
            };
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