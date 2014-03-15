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
using numl.Math.LinearAlgebra;
using numl.Math.Metrics;
using numl.Model;
using numl.Unsupervised;

namespace FotM.Cassandra
{
    public class Cassandra
    {
        private static readonly ILog Logger = LoggingExtensions.GetLogger<Cassandra>();

        private static readonly Descriptor PlayerDiffDescriptor = Descriptor
            .For<PlayerDiff>()
            .With(d => d.Ranking)
            .With(d => d.Rating)
            .With(d => d.SeasonWins)
            .With(d => d.SeasonLosses)
            .With(d => d.WeeklyWins)
            .With(d => d.WeeklyLosses);
        
        public IEnumerable<Team> FindTeams(IEnumerable<Leaderboard> history)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Team> FindTeams(Leaderboard previous, Leaderboard current)
        {
            Logger.InfoFormat("Previous leaderboard has {0} entries, current - {1}", previous.Rows.Length, current.Rows.Length);

            // prepare player diffs for players in both leaderboards
            var players = current.Rows.Select(PlayerRegistry.CreatePlayerFrom)
                .Intersect(previous.Rows.Select(PlayerRegistry.CreatePlayerFrom))
                .ToArray();

            Logger.InfoFormat("Players in common: {0}", players.Length);

            var allDiffs = from p in players
                let previousStat = previous.Rows.First(r => PlayerRegistry.CreatePlayerFrom(r).Equals(p))
                let currentStat = current.Rows.First(r => PlayerRegistry.CreatePlayerFrom(r).Equals(p))
                let diff = new PlayerDiff(p, previousStat, currentStat)
                select diff;

            var playerDiffs = allDiffs.Where(d => d.HasChanges).ToArray();

            Logger.DebugFormat("Total changed rankings: {0}", playerDiffs.Length);
            foreach (var playerDiff in playerDiffs)
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
                    bracketSize = 2;
                    break;
                case Bracket.Fives:
                    bracketSize = 5;
                    break;
                default:
                    throw new NotSupportedException();
            }

            int nGroups = playerDiffs.Length / bracketSize;

            if (nGroups <= 1)
                return new[] {new Team(playerDiffs.Select(d => d.Player))};

            // run K-Means
            Logger.InfoFormat("Starting K-Means for {0} groups...", nGroups);

            var teamLists = Enumerable.Range(0, nGroups).Select(i => new List<Player>()).ToArray();

            var kmeans = new KMeans { Descriptor = PlayerDiffDescriptor };
            
            int[] playerGroups = kmeans.Generate(playerDiffs, nGroups);

            for (int i = 0; i < playerGroups.Length; ++i)
            {
                Player player = playerDiffs[i].Player;
                int nTeam = playerGroups[i];

                teamLists[nTeam].Add(player);
            }

            return teamLists.Select(lst => new Team(lst));
        }
    }

    public class PlayerDiff
    {
        public PlayerDiff(Player player, LeaderboardEntry previous, LeaderboardEntry current)
        {
            this.Player = player;
            this.Ranking = current.Ranking - previous.Ranking;
            this.WeeklyWins = current.WeeklyWins - previous.WeeklyWins;
            this.WeeklyLosses = current.WeeklyLosses - previous.WeeklyLosses;
            this.SeasonWins = current.SeasonWins - previous.SeasonWins;
            this.SeasonLosses = current.SeasonLosses - previous.SeasonLosses;
            this.Rating = current.Rating - previous.Rating;
        }

        public bool HasChanges
        {
            get
            {
                return !(WeeklyWins == 0 && WeeklyLosses == 0 && SeasonLosses == 0 && SeasonWins == 0
                       && Rating == 0);
            }
        }

        public Player Player { get; private set; }

        public double Ranking { get; private set; }
        public double WeeklyWins { get; private set; }
        public double WeeklyLosses { get; private set; }
        public double SeasonWins { get; private set; }
        public double SeasonLosses { get; private set; }
        public double Rating { get; private set; }
    }
}