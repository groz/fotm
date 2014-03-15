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
using MoreLinq;

namespace FotM.Cassandra
{
    public class Cassandra
    {
        private static readonly ILog Logger = LoggingExtensions.GetLogger<Cassandra>();

        public IEnumerable<Team> FindTeams(IEnumerable<Leaderboard> history)
        {
            throw new NotImplementedException();
        }

        int GetBracketSize(Bracket bracket)
        {
            int bracketSize;

            switch (bracket)
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

            return bracketSize;
        }

        public Team[] FindTeams(Leaderboard previousLeaderboard, Leaderboard currentLeaderboard)
        {
            Logger.InfoFormat("Previous leaderboard has {0} entries, current - {1}", 
                previousLeaderboard.Rows.Length, currentLeaderboard.Rows.Length);

            // prepare player diffs for players in both leaderboards
            var previousSet = previousLeaderboard.Rows.ToDictionary(r => r.CreatePlayer(), r => r);
            var currentSet = currentLeaderboard.Rows.ToDictionary(r => r.CreatePlayer(), r => r);

            var players = currentSet.Keys.Intersect(previousSet.Keys).ToHashSet();

            Logger.InfoFormat("Players in common: {0}", players.Count);

            PlayerDiff[] diffs = (
                from p in players
                let previousStat = previousSet[p]
                let currentStat = currentSet[p]
                let diff = new PlayerDiff(p, previousStat, currentStat)
                where diff.HasChanges
                select diff).ToArray();
            
            Logger.DebugFormat("Total changed rankings: {0}", diffs.Length);
            foreach (var playerDiff in diffs)
            {
                Logger.DebugFormat("Player {0}", playerDiff.Player);
            }

            int bracketSize = GetBracketSize(currentLeaderboard.Bracket);

            int nGroups = diffs.Length / bracketSize;

            if (nGroups <= 1)
                return new[] {new Team(diffs.Select(d => d.Player))};

            Logger.InfoFormat("Starting K-Means for {0} groups...", nGroups);

            var teamLists = Enumerable.Range(0, nGroups).Select(i => new List<Player>()).ToArray();

            var kmeans = new KMeans(nGroups);
            
            int[] playerGroups = kmeans.Compute(diffs);

            for (int i = 0; i < playerGroups.Length; ++i)
            {
                Player player = diffs[i].Player;
                int nTeam = playerGroups[i];

                teamLists[nTeam].Add(player);
            }

            FindIncompleteTeams(teamLists, bracketSize);
            var overbookedTeams = FindOverbookedTeams(teamLists, bracketSize);
            
            return teamLists.Except(overbookedTeams).Select(lst => new Team(lst)).ToArray();
        }

        private List<Player>[] FindOverbookedTeams(List<Player>[] teamLists, int bracketSize)
        {
            var overbookedTeams = teamLists.Where(lst => lst.Count > bracketSize).ToArray();

            foreach (var team in overbookedTeams)
            {
                string roster = string.Join(",", team);
                Logger.ErrorFormat("Team roster [{0}] is overbooked for bracket {1} and discarded.", roster, bracketSize);
            }

            return overbookedTeams;
        }

        private IEnumerable<List<Player>> FindIncompleteTeams(List<Player>[] teamLists, int bracketSize)
        {
            var incompleteTeams = teamLists.Where(lst => lst.Count < bracketSize);

            foreach (var team in incompleteTeams)
            {
                string roster = string.Join(",", team);
                Logger.InfoFormat("Team roster [{0}] is incomplete.", roster);
            }

            return incompleteTeams;
        }
    }
}