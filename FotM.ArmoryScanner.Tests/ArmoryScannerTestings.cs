using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FotM.Domain;
using FotM.TestingUtilities;
using NUnit.Framework;

namespace FotM.ArmoryScanner.Tests
{
    [TestFixture]
    public class ArmoryScannerTests: ArmoryTestingBase
    {
        private readonly LeaderboardEntry _groz, _srez, _donder, _borna, _invis, _maks;
        private readonly Leaderboard _previousLeaderboard;
        private readonly Leaderboard _currentLeaderboard;
        private readonly Team _moskva, _limita;
        private IArmoryPuller _dataPuller;

        public ArmoryScannerTests()
            : base(Bracket.Threes)
        {
            _groz = CreateEntry(100, "Groz", 2000);
            _srez = CreateEntry(101, "Srez", 2000);
            _donder = CreateEntry(101, "Donder", 2000);
            _borna = CreateEntry(152, "Borna", 1900);
            _invis = CreateEntry(153, "Invisibles", 1900);
            _maks = CreateEntry(154, "Maks", 1900);

            _previousLeaderboard = CreateLeaderboard(_groz, _srez, _donder, _borna, _invis, _maks);

            _moskva = new Team(_groz.Player(), _srez.Player(), _donder.Player());
            _limita = new Team(_borna.Player(), _invis.Player(), _maks.Player());

            var c1 = UpdateEntry(_groz, +10);
            var c2 = UpdateEntry(_srez, +10);
            var c3 = UpdateEntry(_donder, +10);
            var c4 = UpdateEntry(_borna, -20);
            var c5 = UpdateEntry(_invis, -20);
            var c6 = UpdateEntry(_maks, -20);
            _currentLeaderboard = CreateLeaderboard(c1, c2, c3, c4, c5, c6);
        }

        class ArmoryPullerDouble: IArmoryPuller
        {
            private readonly Queue<Leaderboard> _history;
            private Leaderboard _last;

            public ArmoryPullerDouble(params Leaderboard[] history)
            {
                _history = new Queue<Leaderboard>(history);
            }

            public Leaderboard DownloadLeaderboard(Bracket bracket, string locale = Locale.EnUs)
            {
                Leaderboard result = _history.Count != 0
                    ? _history.Dequeue()
                    : _last;

                _last = result;

                return result;
            }
        }

        [SetUp]
        public void Init()
        {
            _dataPuller = new ArmoryPullerDouble(_previousLeaderboard, _currentLeaderboard);
        }

        [Test]
        public void StatsShouldBeEmptyPriorToScan()
        {
            var armoryScanner = new ArmoryScanner(Bracket.Threes, _dataPuller, 10);

            var json = armoryScanner.SerializeStats();
            TeamStats[] deserializedStats = armoryScanner.DeserializeStats(json);

            CollectionAssert.IsEmpty(deserializedStats);
        }

        [Test]
        public void StatsShouldBeEmptyAfterOneScan()
        {
            var armoryScanner = new ArmoryScanner(Bracket.Threes, _dataPuller, 10);
            armoryScanner.Scan();

            var json = armoryScanner.SerializeStats();
            TeamStats[] deserializedStats = armoryScanner.DeserializeStats(json);

            CollectionAssert.IsEmpty(deserializedStats);
        }

        [Test]
        public void StatsShouldNotBeEmptyAfterScans()
        {
            var armoryScanner = new ArmoryScanner(Bracket.Threes, _dataPuller, 10);
            armoryScanner.Scan();
            armoryScanner.Scan();

            var json = armoryScanner.SerializeStats();
            TeamStats[] deserializedStats = null;
            
            Assert.DoesNotThrow(() =>
            {
                deserializedStats = armoryScanner.DeserializeStats(json);
            });

            Assert.NotNull(deserializedStats);

            CollectionAssert.IsNotEmpty(deserializedStats);
        }

        [Test]
        public void StatsShouldContainTeamsAfterScans()
        {
            var armoryScanner = new ArmoryScanner(Bracket.Threes, _dataPuller, 10);
            armoryScanner.Scan();
            armoryScanner.Scan();

            var json = armoryScanner.SerializeStats();
            TeamStats[] deserializedStats = armoryScanner.DeserializeStats(json);

            var teams = deserializedStats.Select(ts => ts.Team).ToArray();

            CollectionAssert.Contains(teams, _moskva);
            CollectionAssert.Contains(teams, _limita);
        }
    }
}
