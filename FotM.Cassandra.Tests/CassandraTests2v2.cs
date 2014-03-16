using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using FotM.Domain;
using FotM.TestingUtilities;
using NUnit.Framework;

namespace FotM.Cassandra.Tests
{
    [TestFixture]
    public class CassandraTests2v2 : ArmoryTestingBase
    {
        private readonly LeaderboardEntry _groz, _srez, _borna, _invis;
        private readonly Leaderboard _previousLeaderboard;
        private readonly Team _moskva, _limita;

        public CassandraTests2v2() : base(Bracket.Twos)
        {
            _groz = CreateEntry(100, "Groz", 2000);
            _srez = CreateEntry(101, "Srez", 2000);
            _borna = CreateEntry(152, "Borna", 1900);
            _invis = CreateEntry(153, "Invisibles", 1900);

            _previousLeaderboard = CreateLeaderboard(_groz, _srez, _borna, _invis);

            _moskva = new Team(_groz.CreatePlayer(), _srez.CreatePlayer());
            _limita = new Team(_borna.CreatePlayer(), _invis.CreatePlayer());
        }


        [Test]
        public void SimpleWinnersGrouping()
        {
            var c1 = UpdateEntry(_groz, +10);
            var c2 = UpdateEntry(_srez, +10);
            var currentLeaderboard = CreateLeaderboard(c1, c2);

            var cassandra = new Cassandra();
            var predictedTeams = cassandra.FindTeams(_previousLeaderboard, currentLeaderboard);

            CollectionAssert.AreEquivalent(new[] {_moskva}, predictedTeams);
        }

        [Test]
        public void ComplexWinnersGrouping()
        {
            var expectedTeams = new[]
            {
                _moskva,
                _limita
            };

            var c1 = UpdateEntry(_groz, +10);
            var c2 = UpdateEntry(_srez, +10);
            var c3 = UpdateEntry(_borna, +20);
            var c4 = UpdateEntry(_invis, +20);
            var currentLeaderboard = CreateLeaderboard(c1, c2, c3, c4);

            var cassandra = new Cassandra();
            var predictedTeams = cassandra.FindTeams(_previousLeaderboard, currentLeaderboard);

            CollectionAssert.AreEquivalent(expectedTeams, predictedTeams);
        }

        [Test]
        public void SimpleLosersGrouping()
        {
            var c1 = UpdateEntry(_groz, -10);
            var c2 = UpdateEntry(_srez, -10);
            var currentLeaderboard = CreateLeaderboard(c1, c2);

            var cassandra = new Cassandra();
            var predictedTeams = cassandra.FindTeams(_previousLeaderboard, currentLeaderboard);

            CollectionAssert.AreEquivalent(new[] {_moskva}, predictedTeams);
        }

        [Test]
        public void ComplexLosersGrouping()
        {
            var expectedTeams = new[]
            {
                _moskva,
                _limita
            };

            var c1 = UpdateEntry(_groz, -10);
            var c2 = UpdateEntry(_srez, -10);
            var c3 = UpdateEntry(_borna, -20);
            var c4 = UpdateEntry(_invis, -20);
            var currentLeaderboard = CreateLeaderboard(c1, c2, c3, c4);

            var cassandra = new Cassandra();
            var predictedTeams = cassandra.FindTeams(_previousLeaderboard, currentLeaderboard);

            CollectionAssert.AreEquivalent(expectedTeams, predictedTeams);
        }

        [Test]
        public void MixedGrouping()
        {
            var expectedTeams = new[]
            {
                _moskva,
                _limita
            };

            var c1 = UpdateEntry(_groz, +10);
            var c2 = UpdateEntry(_srez, +10);
            var c3 = UpdateEntry(_borna, -10);
            var c4 = UpdateEntry(_invis, -10);
            var currentLeaderboard = CreateLeaderboard(c1, c2, c3, c4);

            var cassandra = new Cassandra();
            var predictedTeams = cassandra.FindTeams(_previousLeaderboard, currentLeaderboard);

            CollectionAssert.AreEquivalent(expectedTeams, predictedTeams);
        }
    }
}