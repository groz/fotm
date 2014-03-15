using System;
using System.Diagnostics;
using FotM.Domain;
using FotM.Utilities;
using log4net;

namespace FotM.ArmoryScanner
{
    class ArmoryScanner
    {
        private static readonly ILog Logger = LoggingExtensions.GetLogger<ArmoryScanner>();

        private readonly Bracket _bracket;
        private readonly ArmoryPuller _dataPuller;
        private readonly ArmoryHistory _history;
        private int _updateCount;
        private Stopwatch _stopwatch;

        public ArmoryScanner(Bracket bracket, string regionHost, int maxHistorySize)
        {
            _bracket = bracket;
            _dataPuller = new ArmoryPuller(regionHost);
            _history = new ArmoryHistory(maxHistorySize);
        }

        public void Scan()
        {
            if (_stopwatch == null)
            {
                _stopwatch = Stopwatch.StartNew();
            }

            Leaderboard currentLeaderboard = _dataPuller.DownloadLeaderboard(_bracket);

            if (_history.Update(currentLeaderboard))
            {
                OnUpdate(_history, currentLeaderboard);
            }
        }

        private void OnUpdate(ArmoryHistory history, Leaderboard currentLeaderboard)
        {
            ++_updateCount;
            var elapsed = _stopwatch.Elapsed;

            Logger.InfoFormat("Total time running: {0}, total snapshots added: {1}, snapshots per minute: {2}",
                elapsed, _updateCount, _updateCount / elapsed.TotalMinutes);
        }
    }
}