using FotM.Domain;
using NUnit.Framework;

namespace FotM.Cassandra.Tests
{
    [TestFixture]
    public class CassandraTests3v3: CassandraTestBase
    {
        public CassandraTests3v3(): base(Bracket.Threes)
        {
        }

        [Test]
        public void SimpleWinnersGrouping()
        {
            var p1 = CreateEntry(100, "Groz", 2000);
            var p2 = CreateEntry(101, "Srez", 2000);
            var p3 = CreateEntry(101, "Donder", 2000);
            var previousLeaderboard = CreateLeaderboard(p1, p2, p3);

            var groz = PlayerRegistry.CreatePlayerFrom(p1);
            var srez = PlayerRegistry.CreatePlayerFrom(p2);
            var donder = PlayerRegistry.CreatePlayerFrom(p3);

            var expectedTeam = new Team(groz, srez, donder);

            var c1 = CreateEntry(99, "Groz", 2010, weeklyWins: 1, seasonWins: 1);
            var c2 = CreateEntry(100, "Srez", 2010, weeklyWins: 1, seasonWins: 1);
            var c3 = CreateEntry(101, "Donder", 2010, weeklyWins: 1, seasonWins: 1);
            var currentLeaderboard = CreateLeaderboard(c1, c2, c3);

            var cassandra = new Cassandra();
            var predictedTeams = cassandra.FindTeams(previousLeaderboard, currentLeaderboard);

            CollectionAssert.AreEquivalent(new[] { expectedTeam }, predictedTeams);
        }

        [Test]
        public void ComplexWinnersGrouping()
        {
            var p1 = CreateEntry(100, "Groz", 2000);
            var p2 = CreateEntry(101, "Srez", 2000);
            var p3 = CreateEntry(102, "Donder", 2000);

            var p4 = CreateEntry(152, "Borna", 1900);
            var p5 = CreateEntry(153, "Invisibles", 1900);
            var p6 = CreateEntry(154, "Maks", 1900);
            var previousLeaderboard = CreateLeaderboard(p1, p2, p3, p4, p5, p6);

            var groz = PlayerRegistry.CreatePlayerFrom(p1);
            var srez = PlayerRegistry.CreatePlayerFrom(p2);
            var donder = PlayerRegistry.CreatePlayerFrom(p3);

            var borna = PlayerRegistry.CreatePlayerFrom(p4);
            var invis = PlayerRegistry.CreatePlayerFrom(p5);
            var maks = PlayerRegistry.CreatePlayerFrom(p6);

            var expectedTeams = new[]
            {
                new Team(groz, srez, donder),
                new Team(borna, invis, maks)
            };

            var c1 = CreateEntry(99, "Groz", 2010, weeklyWins: 1, seasonWins: 1);
            var c2 = CreateEntry(100, "Srez", 2010, weeklyWins: 1, seasonWins: 1);
            var c3 = CreateEntry(101, "Donder", 2010, weeklyWins: 1, seasonWins: 1);

            var c4 = CreateEntry(151, "Borna", 1920, weeklyWins: 1, seasonWins: 1);
            var c5 = CreateEntry(152, "Invisibles", 1920, weeklyWins: 1, seasonWins: 1);
            var c6 = CreateEntry(153, "Maks", 1920, weeklyWins: 1, seasonWins: 1);
            var currentLeaderboard = CreateLeaderboard(c1, c2, c3, c4, c5, c6);

            var cassandra = new Cassandra();
            var predictedTeams = cassandra.FindTeams(previousLeaderboard, currentLeaderboard);

            CollectionAssert.AreEquivalent(expectedTeams, predictedTeams);
        }

        [Test]
        public void SimpleLosersGrouping()
        {
            var p1 = CreateEntry(100, "Groz", 2000);
            var p2 = CreateEntry(101, "Srez", 2000);
            var p3 = CreateEntry(102, "Donder", 2000);
            var previousLeaderboard = CreateLeaderboard(p1, p2, p3);

            var groz = PlayerRegistry.CreatePlayerFrom(p1);
            var srez = PlayerRegistry.CreatePlayerFrom(p2);
            var donder = PlayerRegistry.CreatePlayerFrom(p3);

            var expectedTeam = new Team(groz, srez, donder);

            var c1 = CreateEntry(101, "Groz", 1950, weeklyLosses: 1, seasonLosses: 1);
            var c2 = CreateEntry(102, "Srez", 1950, weeklyLosses: 1, seasonLosses: 1);
            var c3 = CreateEntry(103, "Donder", 1950, weeklyLosses: 1, seasonLosses: 1);
            var currentLeaderboard = CreateLeaderboard(c1, c2, c3);

            var cassandra = new Cassandra();
            var predictedTeams = cassandra.FindTeams(previousLeaderboard, currentLeaderboard);

            CollectionAssert.AreEquivalent(new[] { expectedTeam }, predictedTeams);
        }

        [Test]
        public void ComplexLosersGrouping()
        {
            var p1 = CreateEntry(100, "Groz", 2000);
            var p2 = CreateEntry(101, "Srez", 2000);
            var p3 = CreateEntry(102, "Donder", 2000);

            var p4 = CreateEntry(152, "Borna", 1900);
            var p5 = CreateEntry(153, "Invisibles", 1900);
            var p6 = CreateEntry(154, "Maks", 1900);
            var previousLeaderboard = CreateLeaderboard(p1, p2, p3, p4, p5, p6);

            var groz = PlayerRegistry.CreatePlayerFrom(p1);
            var srez = PlayerRegistry.CreatePlayerFrom(p2);
            var donder = PlayerRegistry.CreatePlayerFrom(p3);

            var borna = PlayerRegistry.CreatePlayerFrom(p4);
            var invis = PlayerRegistry.CreatePlayerFrom(p5);
            var maks = PlayerRegistry.CreatePlayerFrom(p6);

            var expectedTeams = new[]
            {
                new Team(groz, srez, donder),
                new Team(borna, invis, maks)
            };

            var c1 = CreateEntry(99, "Groz", 2010, weeklyWins: 1, seasonWins: 1);
            var c2 = CreateEntry(100, "Srez", 2010, weeklyWins: 1, seasonWins: 1);
            var c3 = CreateEntry(101, "Donder", 2010, weeklyWins: 1, seasonWins: 1);
            var c4 = CreateEntry(151, "Borna", 1920, weeklyWins: 1, seasonWins: 1);
            var c5 = CreateEntry(152, "Invisibles", 1920, weeklyWins: 1, seasonWins: 1);
            var c6 = CreateEntry(152, "Maks", 1920, weeklyWins: 1, seasonWins: 1);
            var currentLeaderboard = CreateLeaderboard(c1, c2, c3, c4, c5, c6);

            var cassandra = new Cassandra();
            var predictedTeams = cassandra.FindTeams(previousLeaderboard, currentLeaderboard);

            CollectionAssert.AreEquivalent(expectedTeams, predictedTeams);
        }

        [Test]
        public void MixedGrouping()
        {
            var p1 = CreateEntry(100, "Groz", 2000);
            var p2 = CreateEntry(101, "Srez", 2000);
            var p3 = CreateEntry(102, "Donder", 2000);

            var p4 = CreateEntry(152, "Borna", 1900);
            var p5 = CreateEntry(153, "Invisibles", 1900);
            var p6 = CreateEntry(154, "Maks", 1900);
            var previousLeaderboard = CreateLeaderboard(p1, p2, p3, p4, p5, p6);

            var groz = PlayerRegistry.CreatePlayerFrom(p1);
            var srez = PlayerRegistry.CreatePlayerFrom(p2);
            var donder = PlayerRegistry.CreatePlayerFrom(p3);

            var borna = PlayerRegistry.CreatePlayerFrom(p4);
            var invis = PlayerRegistry.CreatePlayerFrom(p5);
            var maks = PlayerRegistry.CreatePlayerFrom(p6);

            var expectedTeams = new[]
            {
                new Team(groz, srez, donder),
                new Team(borna, invis, maks)
            };

            var c1 = CreateEntry(99, "Groz", 2010, weeklyWins: 1, seasonWins: 1);
            var c2 = CreateEntry(100, "Srez", 2010, weeklyWins: 1, seasonWins: 1);
            var c3 = CreateEntry(100, "Donder", 2010, weeklyWins: 1, seasonWins: 1);
            var c4 = CreateEntry(161, "Borna", 1890, weeklyLosses: 1, seasonLosses: 1);
            var c5 = CreateEntry(162, "Invisibles", 1890, weeklyLosses: 1, seasonLosses: 1);
            var c6 = CreateEntry(162, "Maks", 1890, weeklyLosses: 1, seasonLosses: 1);
            var currentLeaderboard = CreateLeaderboard(c1, c2, c3, c4, c5, c6);

            var cassandra = new Cassandra();
            var predictedTeams = cassandra.FindTeams(previousLeaderboard, currentLeaderboard);

            CollectionAssert.AreEquivalent(expectedTeams, predictedTeams);
        }
    }
}