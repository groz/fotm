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
        private readonly Dictionary<Team, TeamStats> _teamStats = new Dictionary<Team, TeamStats>();

        class TeamStats
        {
            public TeamStats()
            {
                RatingChange = 0;
                Occurences = 1;
            }

            public double RatingChange { get; private set; }
            public int Occurences { get; private set; }

            public bool IsVerified
            {
                get
                {
                    return Occurences >= 2; // same setup seen twice or more
                }
            }

            public void Update(double ratingChange)
            {
                RatingChange += ratingChange;
                ++Occurences;
            }
        }

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

            try
            {
                Leaderboard currentLeaderboard = _dataPuller.DownloadLeaderboard(_bracket);

                if (_history.Update(currentLeaderboard))
                {
                    OnUpdate(_history, currentLeaderboard);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
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
                Logger.InfoFormat("Team {0} found", team);

                double? ratingChange = CalcRatingChange(team, _previousLeaderboard, currentLeaderboard);

                if (!ratingChange.HasValue)
                {
                    Logger.Info("Rating change couldn't be calculated (new entry), skipping.");
                    continue;
                }

                TeamStats teamStats;

                if (_teamStats.TryGetValue(team, out teamStats))
                {
                    Logger.InfoFormat("Updating team {0} with rating change {1}", team, ratingChange);
                    teamStats.Update(ratingChange.Value);
                }
                else
                {
                    Logger.InfoFormat("Adding team {0} with rating change {1}", team, ratingChange);
                    _teamStats[team] = new TeamStats();
                }
            }

            LogStats();
        }

        private void LogStats()
        {
            Logger.Info("Top teams:");

            var verifiedTeams = _teamStats
                .Where(t => t.Value.IsVerified)
                .Select(t => new
                {
                    Team = t.Key,
                    Setup = new TeamSetup(t.Key),
                    Stats = t.Value
                })
                .ToArray();

            foreach (var team in verifiedTeams.OrderByDescending(t => t.Stats.RatingChange))
            {
                Logger.InfoFormat("Team: {0} ({1}), Seen: {2}, RatingChange: {3}", 
                    team.Team, team.Setup, team.Stats.Occurences, team.Stats.RatingChange);
            }

            Logger.Info("Top setups:");

            var setups = verifiedTeams
                .GroupBy(t => t.Setup)
                .Select(setupGroup => new
                {
                    Setup = setupGroup.Key,
                    Count = setupGroup.Count() // number of those teams
                }).ToArray();

            var total = setups.Sum(s => s.Count);

            foreach (var teamSetup in setups.OrderByDescending(ts => ts.Count))
            {
                Logger.InfoFormat("{0}: {1}/{2}", teamSetup.Setup, teamSetup.Count, total);
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
}