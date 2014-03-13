using System.Collections.Generic;
using System.Linq;
using FotM.Domain;
using FotM.Utilities;
using log4net;

namespace FotM.ArmoryScanner
{
    class ArmoryHistory
    {
        private static readonly ILog Logger = LoggingExtensions.GetLogger<Program>();
        private readonly int _maxSize;
        private readonly Queue<Leaderboard> _snapshots;

        public ArmoryHistory(int maxSize)
        {
            _maxSize = maxSize;
            _snapshots = new Queue<Leaderboard>(_maxSize);
        }

        public bool AddSnapshot(Leaderboard snapshot)
        {
            if (_snapshots.Any(s => SnapshotsEqual(s, snapshot)))
            {
                Logger.Debug("Snapshot already in queue, skipping.");
                return false;
            }

            if (_snapshots.Count == _maxSize)
            {
                Logger.DebugFormat("Max queue size {0} reached, removing first element.", _maxSize);
                _snapshots.Dequeue();
            }

            Logger.Debug("Added snapshot to history.");
            _snapshots.Enqueue(snapshot);

            return true;
        }

        private static bool SnapshotsEqual(Leaderboard left, Leaderboard right)
        {
            for (int i = 0; i < left.Rows.Length; ++i)
            {
                var leftRow = left.Rows[i];
                var rightRow = right.Rows[i];

                if (!MemberCompare.Equal(leftRow, rightRow))
                {
#if DEBUG
                    // DEBUG only
                    // find same players in previous snapshots
                    Logger.DebugFormat("Previous left: {0}", leftRow);

                    var newLeftRow = right.Rows.First(r => r.Name == leftRow.Name && r.RealmId == leftRow.RealmId);

                    Logger.DebugFormat("New left: {0}", newLeftRow);

                    var previousRightRow = left.Rows.First(r => r.Name == rightRow.Name && r.RealmId == rightRow.RealmId);
                    Logger.DebugFormat("Previous right: {0}", previousRightRow);

                    Logger.DebugFormat("New right: {0}", rightRow);
#endif
                    return false;
                }
            }

            return true;
        }
    }
}