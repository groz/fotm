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
                Leaderboard result = _history.Count == 0
                    ? _history.Dequeue()
                    : _last;

                _last = result;

                return result;
            }
        }

        private readonly IArmoryPuller _dataPuller = new ArmoryPullerDouble();

        public ArmoryScannerTests(): base(Bracket.Threes)
        {
        }

        [Test]
        public void Test1()
        {
            var armoryScanner = new ArmoryScanner(Bracket.Threes, _dataPuller, 10);
            armoryScanner.Scan();
        }
    }
}
