using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        private readonly IKMeans<PlayerChange> _kmeans;
        private readonly CassandraStats _stats;

        public Cassandra(IKMeans<PlayerChange> kmeans = null)
        {
            _stats = new CassandraStats();
            //_kmeans = kmeans ?? new NumlKMeans();
            _kmeans = kmeans ?? new AccordKMeans(normalize: true);
        }

        public IEnumerable<Team> FindTeams(IEnumerable<Leaderboard> history)
        {
            throw new NotImplementedException();
        }

        private List<Player>[] FindTeams(int bracketSize, PlayerChange[] changes)
        {
            Logger.DebugFormat("Total changed rankings: {0}", changes.Length);

            _stats.TeamsPossible += changes.Length / bracketSize;

            int nGroups = (int)Math.Ceiling((double)changes.Length / bracketSize);

            //return changes.Shuffle(new Random(15)).Select(d => d.Player).Batch(3).Select(batch => batch.ToList()).ToArray();

            if (nGroups <= 1)
                return new[] { changes.Select(d => d.Player).ToList() };

            Logger.InfoFormat("Starting K-Means for {0} groups...", nGroups);

            var teamLists = Enumerable.Range(0, nGroups).Select(i => new List<Player>()).ToArray();

            int[] playerGroups = _kmeans.ComputeGroups(changes, nGroups);

            for (int i = 0; i < playerGroups.Length; ++i)
            {
                Player player = changes[i].Player;
                int nTeam = playerGroups[i];

                teamLists[nTeam].Add(player);
            }

            return teamLists;
        }

        public Team[] FindTeams(Leaderboard previousLeaderboard, Leaderboard currentLeaderboard)
        {
            ++_stats.TotalCalls;

            int bracketSize = currentLeaderboard.Bracket.Size();

            Logger.InfoFormat("Previous leaderboard has {0} entries, current - {1}",
                previousLeaderboard.Rows.Length, currentLeaderboard.Rows.Length);

            // prepare player changes for players in both leaderboards
            var previousSet = previousLeaderboard.Rows.ToDictionary(r => r.Player(), r => r);
            var currentSet = currentLeaderboard.Rows.ToDictionary(r => r.Player(), r => r);

            var players = currentSet.Keys.Intersect(previousSet.Keys).ToHashSet();

            Logger.InfoFormat("Players in common: {0}", players.Count);

            var changes = from player in players
                          let diff = new PlayerChange(player, previousSet[player], currentSet[player])
                          where diff.HasChanges
                          select diff;

            var parts = Partition(changes.ToArray());

            var allTeams = parts
                .SelectMany(p => FindTeams(bracketSize, p))
                .Where(team => team != null && team.Any())
                .ToArray();

            var incorrectTeams = FindIncorrectTeams(allTeams, bracketSize);

            var fullTeams = allTeams.Except(incorrectTeams).Select(lst => new Team(lst)).ToArray();

            _stats.FullTeamsDetected += fullTeams.Length;

            return fullTeams;
        }

        private PlayerChange[][] Partition(PlayerChange[] changes)
        {
            var alliance = changes.Where(d => d.FactionId == 0).ToArray();
            var horde = changes.Where(d => d.FactionId == 1).ToArray();

            var allianceWinners = alliance.Where(d => d.RatingDiff > 0).ToArray();
            var allianceLosers = alliance.Where(d => d.RatingDiff <= 0).ToArray();

            var hordeWinners = horde.Where(d => d.RatingDiff > 0).ToArray();
            var hordeLosers = horde.Where(d => d.RatingDiff <= 0).ToArray();

            return new[] { allianceWinners, allianceLosers, hordeWinners, hordeLosers };
        }

        private List<Player>[] FindIncorrectTeams(List<Player>[] teamLists, int bracketSize)
        {
            var incorrectTeams = teamLists.Where(lst => lst.Count != bracketSize).ToArray();

            foreach (var team in incorrectTeams)
            {
                string roster = string.Join(",", team);
                Logger.ErrorFormat("Team roster [{0}] has incorrect size for bracket {1}x{1} and is discarded.", roster, bracketSize);

                if (team.Count < bracketSize)
                    ++_stats.IncompleteTeams;
                else
                    ++_stats.OverbookedTeams;
            }

            return incorrectTeams;
        }

        public CassandraStats Stats
        {
            get { return _stats; }
        }
    }
}