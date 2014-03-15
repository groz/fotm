using System.Collections.Generic;
using System.Linq;
using FotM.Domain;
using FotM.Utilities;
using log4net;
using MoreLinq;

namespace FotM.ArmoryScanner
{
    class ArmoryHistory
    {
        private static readonly ILog Logger = LoggingExtensions.GetLogger<Program>();
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
            if (_lastSnapshot != null && SnapshotsEqual(_lastSnapshot, currentSnapshot))
            {
                Logger.Debug("Snapshot already in queue, skipping.");
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

        private static bool SnapshotsEqual(Leaderboard previous, Leaderboard current)
        {
#if DEBUG
            var diffNew = current.Rows.Except(previous.Rows);

            foreach (var currentEntry in diffNew)
            {
                var previousEntry = previous.Rows
                    .FirstOrDefault(e => currentEntry.CreatePlayer().Equals(e.CreatePlayer()));

                if (previousEntry != null)
                {
                    Logger.DebugFormat("{0} -> {1}", previousEntry, currentEntry);
                }
            }
#endif
            return current.Rows.SequenceEqual(previous.Rows);
        }
    }
}