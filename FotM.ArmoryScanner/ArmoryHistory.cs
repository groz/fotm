using System.Collections.Generic;
using System.Linq;
using FotM.Domain;
using FotM.Utilities;
using log4net;
using MoreLinq;

namespace FotM.ArmoryScanner
{
    class PlayerUpdate
    {
        public Player Player { get; set; }
        public LeaderboardEntry From { get; set; }
        public LeaderboardEntry To { get; set; }

        public PlayerUpdate(Player player, LeaderboardEntry from, LeaderboardEntry to)
        {
            Player = player;
            From = @from;
            To = to;
        }
    }

    class ArmoryHistory
    {
        private static readonly ILog Logger = LoggingExtensions.GetLogger<ArmoryHistory>();
        private readonly int _maxSize;
        private readonly Queue<Leaderboard> _snapshots;
        private Leaderboard _lastSnapshot = null;

        public ArmoryHistory(int maxSize)
        {
            _maxSize = maxSize;
            _snapshots = new Queue<Leaderboard>(_maxSize);
        }

        public bool Update(Leaderboard currentSnapshot)
        {
            if (_lastSnapshot != null && !IsValidSnapshot(_lastSnapshot, currentSnapshot))
            {
                Logger.Debug("Leaderboard is outdated or equal to previous.");
                return false;
            }

            if (_snapshots.Count == _maxSize)
            {
                Logger.DebugFormat("Max queue size {0} reached, removing first element.", _maxSize);
                _snapshots.Dequeue();
            }

            Logger.Debug("Added currentSnapshot to history.");
            _snapshots.Enqueue(currentSnapshot);
            _lastSnapshot = currentSnapshot;

            return true;
        }

        private static bool IsValidSnapshot(Leaderboard previous, Leaderboard current)
        {
            // discard outdated updates
            var previousState = previous.Rows.ToDictionary(r => r.CreatePlayer());
            var currentState = current.Rows.ToDictionary(r => r.CreatePlayer());

            foreach (var p in previousState)
            {
                LeaderboardEntry currentEntry;

                if (currentState.TryGetValue(p.Key, out currentEntry))
                {
                    if (p.Value.SeasonTotal > currentEntry.SeasonTotal)
                    {
                        Logger.InfoFormat("Leaderboard is outdated");
                        return false;
                    }
                }
            }

            Logger.DebugFormat("Previous snapshot: {0} -> Current snapshot: {1}", previous.Time, current.Time);
#if DEBUG
            var diffNew = current.Rows.Except(previous.Rows);

            foreach (var currentEntry in diffNew)
            {
                LeaderboardEntry previousEntry = previous.Rows
                    .FirstOrDefault(e => currentEntry.CreatePlayer().Equals(e.CreatePlayer()));

                if (previousEntry != null)
                {
                    Logger.DebugFormat("{0} -> {1}", previousEntry, currentEntry);
                }
            }
#endif
            return !current.Rows.SequenceEqual(previous.Rows);
        }
    }
}