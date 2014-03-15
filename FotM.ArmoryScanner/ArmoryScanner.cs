using System;
using FotM.Domain;

namespace FotM.ArmoryScanner
{
    class ArmoryScanner
    {
        private readonly Bracket _bracket;
        private readonly ArmoryPuller _dataPuller;
        private readonly ArmoryHistory _history;

        public ArmoryScanner(Bracket bracket, string regionHost, int maxHistorySize)
        {
            _bracket = bracket;
            _dataPuller = new ArmoryPuller(regionHost);
            _history = new ArmoryHistory(maxHistorySize);
        }

        public void Scan(Action<ArmoryHistory, Leaderboard> onUpdate)
        {
            Leaderboard currentLeaderboard = _dataPuller.DownloadLeaderboard(_bracket);

            if (_history.Update(currentLeaderboard))
            {
                onUpdate(_history, currentLeaderboard);
            }
        }
    }
}