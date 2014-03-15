using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using FotM.Domain;
using NUnit.Framework;

namespace FotM.Cassandra.Tests
{
    [TestFixture]
    public class CassandraTests2v2: CassandraTestBase
    {
        public CassandraTests2v2(): base(Bracket.Twos)
        {
        }

        [Test]
        public void SimpleWinnersGrouping()
        {
            var p1 = CreateEntry(100, "Groz", 2000);
            var p2 = CreateEntry(101, "Srez", 2000);
            var previousLeaderboard = CreateLeaderboard(p1, p2);

            var groz = PlayerRegistry.CreatePlayerFrom(p1);
            var srez = PlayerRegistry.CreatePlayerFrom(p2);

            var expectedTeam = new Team(groz, srez);

            var c1 = CreateEntry(99, "Groz", 2010, weeklyWins: 1, seasonWins: 1);
            var c2 = CreateEntry(100, "Srez", 2010, weeklyWins: 1, seasonWins: 1);
            var currentLeaderboard = CreateLeaderboard(c1, c2);

            var cassandra = new Cassandra();
            var predictedTeams = cassandra.FindTeams(previousLeaderboard, currentLeaderboard);

            CollectionAssert.AreEquivalent(new[] {expectedTeam}, predictedTeams);
        }

        [Test]
        public void ComplexWinnersGrouping()
        {
            var p1 = CreateEntry(100, "Groz", 2000);
            var p2 = CreateEntry(101, "Srez", 2000);
            var p3 = CreateEntry(152, "Borna", 1900);
            var p4 = CreateEntry(153, "Invisibles", 1900);
            var previousLeaderboard = CreateLeaderboard(p1, p2, p3, p4);

            var groz = PlayerRegistry.CreatePlayerFrom(p1);
            var srez = PlayerRegistry.CreatePlayerFrom(p2);
            var borna = PlayerRegistry.CreatePlayerFrom(p3);
            var invis = PlayerRegistry.CreatePlayerFrom(p4);

            var expectedTeams = new[]
            {
                new Team(groz, srez),
                new Team(borna, invis)
            };

            var c1 = CreateEntry(99, "Groz", 2010, weeklyWins: 1, seasonWins: 1);
            var c2 = CreateEntry(100, "Srez", 2010, weeklyWins: 1, seasonWins: 1);
            var c3 = CreateEntry(151, "Borna", 1920, weeklyWins: 1, seasonWins: 1);
            var c4 = CreateEntry(152, "Invisibles", 1920, weeklyWins: 1, seasonWins: 1);
            var currentLeaderboard = CreateLeaderboard(c1, c2, c3, c4);

            var cassandra = new Cassandra();
            var predictedTeams = cassandra.FindTeams(previousLeaderboard, currentLeaderboard);

            CollectionAssert.AreEquivalent(expectedTeams, predictedTeams);
        }

        [Test]
        public void SimpleLosersGrouping()
        {
            var p1 = CreateEntry(100, "Groz", 2000);
            var p2 = CreateEntry(101, "Srez", 2000);
            var previousLeaderboard = CreateLeaderboard(p1, p2);

            var groz = PlayerRegistry.CreatePlayerFrom(p1);
            var srez = PlayerRegistry.CreatePlayerFrom(p2);

            var expectedTeam = new Team(groz, srez);

            var c1 = CreateEntry(101, "Groz", 1950, weeklyLosses: 1, seasonLosses: 1);
            var c2 = CreateEntry(102, "Srez", 1950, weeklyLosses: 1, seasonLosses: 1);
            var currentLeaderboard = CreateLeaderboard(c1, c2);

            var cassandra = new Cassandra();
            var predictedTeams = cassandra.FindTeams(previousLeaderboard, currentLeaderboard);

            CollectionAssert.AreEquivalent(new[] {expectedTeam}, predictedTeams);
        }

        [Test]
        public void ComplexLosersGrouping()
        {
            var p1 = CreateEntry(100, "Groz", 2000);
            var p2 = CreateEntry(101, "Srez", 2000);
            var p3 = CreateEntry(152, "Borna", 1900);
            var p4 = CreateEntry(153, "Invisibles", 1900);
            var previousLeaderboard = CreateLeaderboard(p1, p2, p3, p4);

            var groz = PlayerRegistry.CreatePlayerFrom(p1);
            var srez = PlayerRegistry.CreatePlayerFrom(p2);
            var borna = PlayerRegistry.CreatePlayerFrom(p3);
            var invis = PlayerRegistry.CreatePlayerFrom(p4);

            var expectedTeams = new[]
            {
                new Team(groz, srez),
                new Team(borna, invis)
            };

            var c1 = CreateEntry(99, "Groz", 2010, weeklyWins: 1, seasonWins: 1);
            var c2 = CreateEntry(100, "Srez", 2010, weeklyWins: 1, seasonWins: 1);
            var c3 = CreateEntry(151, "Borna", 1920, weeklyWins: 1, seasonWins: 1);
            var c4 = CreateEntry(152, "Invisibles", 1920, weeklyWins: 1, seasonWins: 1);
            var currentLeaderboard = CreateLeaderboard(c1, c2, c3, c4);

            var cassandra = new Cassandra();
            var predictedTeams = cassandra.FindTeams(previousLeaderboard, currentLeaderboard);

            CollectionAssert.AreEquivalent(expectedTeams, predictedTeams);
        }

        [Test]
        public void MixedGrouping()
        {
            var p1 = CreateEntry(100, "Groz", 2000);
            var p2 = CreateEntry(101, "Srez", 2000);
            var p3 = CreateEntry(152, "Borna", 1900);
            var p4 = CreateEntry(153, "Invisibles", 1900);
            var previousLeaderboard = CreateLeaderboard(p1, p2, p3, p4);

            var groz = PlayerRegistry.CreatePlayerFrom(p1);
            var srez = PlayerRegistry.CreatePlayerFrom(p2);
            var borna = PlayerRegistry.CreatePlayerFrom(p3);
            var invis = PlayerRegistry.CreatePlayerFrom(p4);

            var expectedTeams = new[]
            {
                new Team(groz, srez),
                new Team(borna, invis)
            };

            var c1 = CreateEntry(99, "Groz", 2010, weeklyWins: 1, seasonWins: 1);
            var c2 = CreateEntry(100, "Srez", 2010, weeklyWins: 1, seasonWins: 1);
            var c3 = CreateEntry(161, "Borna", 1890, weeklyLosses: 1, seasonLosses: 1);
            var c4 = CreateEntry(162, "Invisibles", 1890, weeklyLosses: 1, seasonLosses: 1);
            var currentLeaderboard = CreateLeaderboard(c1, c2, c3, c4);

            var cassandra = new Cassandra();
            var predictedTeams = cassandra.FindTeams(previousLeaderboard, currentLeaderboard);

            CollectionAssert.AreEquivalent(expectedTeams, predictedTeams);
        }
    }
}
