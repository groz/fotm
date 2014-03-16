using FotM.Domain;
using FotM.TestingUtilities;
using NUnit.Framework;

namespace FotM.Cassandra.Tests
{
    [TestFixture]
    public class CassandraTests3v3 : ArmoryTestingBase
    {
        private readonly LeaderboardEntry _groz, _srez, _donder, _borna, _invis, _maks;
        private readonly Leaderboard _previousLeaderboard;
        private readonly Team _moskva, _limita;

        public CassandraTests3v3() : base(Bracket.Threes)
        {
            _groz = CreateEntry(100, "Groz", 2000);
            _srez = CreateEntry(101, "Srez", 2000);
            _donder = CreateEntry(101, "Donder", 2000);
            _borna = CreateEntry(152, "Borna", 1900);
            _invis = CreateEntry(153, "Invisibles", 1900);
            _maks = CreateEntry(154, "Maks", 1900);

            _previousLeaderboard = CreateLeaderboard(_groz, _srez, _donder, _borna, _invis, _maks);

            _moskva = new Team(_groz.CreatePlayer(), _srez.CreatePlayer(), _donder.CreatePlayer());
            _limita = new Team(_borna.CreatePlayer(), _invis.CreatePlayer(), _maks.CreatePlayer());
        }

        [Test]
        public void SimpleWinnersGrouping()
        {
            var c1 = UpdateEntry(_groz, +10);
            var c2 = UpdateEntry(_srez, +10);
            var c3 = UpdateEntry(_donder, +10);
            var currentLeaderboard = CreateLeaderboard(c1, c2, c3);

            var cassandra = new Cassandra();
            var predictedTeams = cassandra.FindTeams(_previousLeaderboard, currentLeaderboard);

            var expectedTeams = new[] { _moskva };

            CollectionAssert.AreEquivalent(expectedTeams, predictedTeams);
        }

        [Test]
        public void ComplexWinnersGrouping()
        {
            var c1 = UpdateEntry(_groz, +10);
            var c2 = UpdateEntry(_srez, +10);
            var c3 = UpdateEntry(_donder, +10);

            var c4 = UpdateEntry(_borna, +20);
            var c5 = UpdateEntry(_invis, +20);
            var c6 = UpdateEntry(_maks, +20);

            var currentLeaderboard = CreateLeaderboard(c1, c2, c3, c4, c5, c6);

            var expectedTeams = new[]
            {
                _moskva, _limita
            };

            var cassandra = new Cassandra();
            var predictedTeams = cassandra.FindTeams(_previousLeaderboard, currentLeaderboard);

            CollectionAssert.AreEquivalent(expectedTeams, predictedTeams);
        }

        [Test]
        public void SimpleLosersGrouping()
        {
            var expectedTeam = _moskva;

            var c1 = UpdateEntry(_groz, -10);
            var c2 = UpdateEntry(_srez, -10);
            var c3 = UpdateEntry(_donder, -10);
            var currentLeaderboard = CreateLeaderboard(c1, c2, c3);

            var cassandra = new Cassandra();
            var predictedTeams = cassandra.FindTeams(_previousLeaderboard, currentLeaderboard);

            CollectionAssert.AreEquivalent(new[] { expectedTeam }, predictedTeams);
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
            var c3 = UpdateEntry(_donder, -10);
            var c4 = UpdateEntry(_borna, -30);
            var c5 = UpdateEntry(_invis, -30);
            var c6 = UpdateEntry(_maks, -30);
            var currentLeaderboard = CreateLeaderboard(c1, c2, c3, c4, c5, c6);

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
            var c3 = UpdateEntry(_donder, +10);
            var c4 = UpdateEntry(_borna, -20);
            var c5 = UpdateEntry(_invis, -20);
            var c6 = UpdateEntry(_maks, -20);
            var currentLeaderboard = CreateLeaderboard(c1, c2, c3, c4, c5, c6);

            var cassandra = new Cassandra();
            var predictedTeams = cassandra.FindTeams(_previousLeaderboard, currentLeaderboard);

            CollectionAssert.AreEquivalent(expectedTeams, predictedTeams);
        }
    }
}