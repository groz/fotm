using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FotM.Domain;
using NUnit.Framework;

namespace FotM.Cassandra.Tests
{
    [TestFixture]
    public class CassandraTests2v2
    {
        private LeaderboardEntry CreateEntry(int ranking, string name, int rating)
        {
            return new LeaderboardEntry()
            {
                WeeklyLosses = 0,
                WeeklyWins = 0,
                SeasonLosses = 0,
                SeasonWins = 0,
                Name = name,
                ClassId = 0,
                SpecId = 0,
                Rating = rating,
                Ranking = ranking,
                RealmId = 1,
                RealmName = "TestRealm"
            };
        }

        private Leaderboard From(params LeaderboardEntry[] entries)
        {
            return new Leaderboard()
            {
                Rows = entries
            };
        }

        [Test]
        public void SimpleWinnersGrouping_2v2()
        {
            var p1 = CreateEntry(100, "Tagir", 2000);
            var p2 = CreateEntry(100, "Sergey", 2000);
            var previousLeaderboard = From(p1, p2);

            var tagir = PlayerRegistry.CreatePlayerFrom(p1);
            var sergey = PlayerRegistry.CreatePlayerFrom(p2);

            var expectedTeam = new Team(tagir, sergey);

            var c1 = CreateEntry(100, "Tagir", 2010);
            var c2 = CreateEntry(100, "Sergey", 2010);
            var currentLeaderboard = From(c1, c2);

            var cassandra = new Cassandra();
            var predictedTeams = cassandra.FindTeams(previousLeaderboard, currentLeaderboard);

            Assert.AreEqual(expectedTeam, predictedTeams);
        }

        [Test]
        public void ComplexWinnersGrouping_2v2()
        {
        }

        [Test]
        public void SimpleLosersGrouping_2v2()
        {
        }

        [Test]
        public void ComplexLosersGrouping_2v2()
        {
        }
    }
}
