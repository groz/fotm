using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
        private Leaderboard _previousLeaderboard = null;
        private readonly Dictionary<Team, double> _teamRatings = new Dictionary<Team, double>();

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

            if (_previousLeaderboard != null)
            {
                UpdateTeams(currentLeaderboard);
            }

            _previousLeaderboard = currentLeaderboard;
        }

        private void UpdateTeams(Leaderboard currentLeaderboard)
        {
            var cassandra = new Cassandra.Cassandra();

            Team[] updatedTeams = cassandra.FindTeams(_previousLeaderboard, currentLeaderboard);

            foreach (var team in updatedTeams)
            {
                Logger.InfoFormat("Team found: {0}", team);

                double? ratingChange = CalcRatingChange(team, _previousLeaderboard, currentLeaderboard);

                if (!ratingChange.HasValue)
                {
                    Logger.Info("Rating change couldn't be calculated (new entry), skipping.");
                    continue;
                }

                if (_teamRatings.ContainsKey(team))
                {
                    Logger.InfoFormat("Updating team {0} with rating change {1}", team, ratingChange);
                    _teamRatings[team] += ratingChange.Value;
                }
                else
                {
                    Logger.InfoFormat("Adding team {0} with rating change {1}", team, ratingChange);
                    _teamRatings[team] = ratingChange.Value;
                }
            }
        }

        private double? CalcRatingChange(Team team, Leaderboard previousLeaderboard, Leaderboard currentLeaderboard)
        {
            var entries = (from player in team
                let previousEntry = previousLeaderboard.Rows.FirstOrDefault(r => r.CreatePlayer().Equals(player))
                where previousEntry != null
                let currentEntry = currentLeaderboard.Rows.FirstOrDefault(r => r.CreatePlayer().Equals(player))
                where currentEntry != null
                select new {player, previousEntry, currentEntry}).ToArray();

            return entries.Any()
                ? entries.Average(e => e.currentEntry.Rating - e.previousEntry.Rating)
                : (double?) null;
        }
    }

    class TeamSetup : IEquatable<TeamSetup>
    {
        public int[] ClassIds { get; private set; }

        public TeamSetup(Team team)
        {
            this.ClassIds = team.Players.Select(p => p.ClassId).OrderBy(id => id).ToArray();
        }

        public override string ToString()
        {
            return string.Join(",", ClassIds);
        }

        public bool Equals(TeamSetup other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return ClassIds.SequenceEqual(other.ClassIds);
        }

        public override int GetHashCode()
        {
            return (ClassIds != null ? ClassIds.GetHashCode() : 0);
        }

        public override bool Equals(object other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals((TeamSetup)other);
        }
    }
}