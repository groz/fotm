using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Accord.MachineLearning;
using Accord.Math;
using FotM.Domain;
using System.Collections.Generic;
using FotM.Domain;
using FotM.Utilities;
using log4net;

namespace FotM.Cassandra
{
    public class Cassandra
    {
        private static readonly ILog Logger = LoggingExtensions.GetLogger<Cassandra>();

        public IEnumerable<Team> FindTeams(IEnumerable<Leaderboard> history)
        {
            throw new NotImplementedException();
        }

        public Team[] FindTeams(Leaderboard previous, Leaderboard current)
        {
            Logger.InfoFormat("Previous leaderboard has {0} entries, current - {1}", previous.Rows.Length, current.Rows.Length);

            // prepare player diffs for players in both leaderboards
            var players = current.Rows.Select(PlayerRegistry.CreatePlayerFrom)
                .Intersect(previous.Rows.Select(PlayerRegistry.CreatePlayerFrom))
                .ToArray();

            Logger.InfoFormat("Players in common: {0}", players.Length);

            var allDiffs = from p in players
                let previousStat = Enumerable.First(previous.Rows, r => PlayerRegistry.CreatePlayerFrom(r).Equals(p))
                let currentStat = Enumerable.First(current.Rows, r => PlayerRegistry.CreatePlayerFrom(r).Equals(p))
                let diff = new PlayerDiff(p, previousStat, currentStat)
                select diff;

            PlayerDiff[] changedEntries = allDiffs.Where(d => d.HasChanges).ToArray();

            Logger.DebugFormat("Total changed rankings: {0}", changedEntries.Length);
            foreach (var playerDiff in changedEntries)
            {
                Logger.DebugFormat("Player {0}", playerDiff.Player);
            }

            int bracketSize;
            switch (previous.Bracket)
            {
                case Bracket.Twos:
                    bracketSize = 2;
                    break;
                case Bracket.Threes:
                    bracketSize = 3;
                    break;
                case Bracket.Fives:
                    bracketSize = 5;
                    break;
                default:
                    throw new NotSupportedException();
            }

            int nGroups = changedEntries.Length / bracketSize;

            if (nGroups <= 1)
                return new[] {new Team(changedEntries.Select(d => d.Player))};

            Logger.InfoFormat("Starting K-Means for {0} groups...", nGroups);

            var teamLists = Enumerable.Range(0, nGroups).Select(i => new List<Player>()).ToArray();

            var kmeans = new KMeans(nGroups);
            
            int[] playerGroups = kmeans.Compute(changedEntries);

            for (int i = 0; i < playerGroups.Length; ++i)
            {
                Player player = changedEntries[i].Player;
                int nTeam = playerGroups[i];

                teamLists[nTeam].Add(player);
            }

            return teamLists.Select(lst => new Team(lst)).ToArray();
        }
    }
}