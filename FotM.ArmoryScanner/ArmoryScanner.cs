using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FotM.Domain;
using FotM.Utilities;
using log4net;
using Newtonsoft.Json;

namespace FotM.ArmoryScanner
{
    public class ArmoryScanner
    {
        private static readonly ILog Logger = LoggingExtensions.GetLogger<ArmoryScanner>();

        private readonly Bracket _bracket;
        private readonly IArmoryPuller _dataPuller;
        private readonly ArmoryHistory _history;
        private int _updateCount;
        private Stopwatch _stopwatch;
        private Leaderboard _previousLeaderboard = null;
        private readonly Dictionary<Team, TeamStats> _teamStats = new Dictionary<Team, TeamStats>();

        public ArmoryScanner(Bracket bracket, IArmoryPuller dataPuller, int maxHistorySize)
        {
            _bracket = bracket;
            _dataPuller = dataPuller;
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

                int? previousRating = GetRating(team, _previousLeaderboard);
                int? currentRating = GetRating(team, currentLeaderboard);
                int? ratingChange = currentRating - previousRating;

                if (!ratingChange.HasValue)
                {
                    Logger.Info("Rating change couldn't be calculated (new entry), skipping.");
                    continue;
                }

                TeamStats teamStats;

                if (_teamStats.TryGetValue(team, out teamStats))
                {
                    Logger.InfoFormat("Updating team {0} with rating change {1}", team, ratingChange);
                    teamStats.Update(currentRating.Value, ratingChange.Value);
                }
                else
                {
                    Logger.InfoFormat("Adding team {0} with rating change {1}", team, ratingChange);
                    _teamStats[team] = new TeamStats(team, currentRating.Value, ratingChange.Value);
                }
            }

            if (updatedTeams.Length != 0)
                LogStats();
        }

        public string SerializeStats()
        {
            string json = JsonConvert.SerializeObject(_teamStats.Values.ToArray());

            var zipped = CompressionUtils.Zip(json);
            var base64Zipped = CompressionUtils.ZipToBase64(json);
            Logger.DebugFormat("Compressed size: {0}, base64: {1}", zipped.Length, base64Zipped.Length);

            return json;
        }

        public TeamStats[] DeserializeStats(string json)
        {
            return JsonConvert.DeserializeObject<TeamStats[]>(json);
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
                Logger.InfoFormat("Team: {0} ({1}), Seen: {2}, Rating: {3} ({4}{5})", 
                    team.Team, 
                    team.Setup, 
                    team.Stats.TimesSeen, 
                    team.Stats.Rating,
                    team.Stats.RatingChange > 0 ? "+" : "", 
                    team.Stats.RatingChange);
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

            string json = SerializeStats();
            Logger.DebugFormat("Serialized stats size: {0}", json.Length);
        }

        private int? GetRating(Team team, Leaderboard leaderboard)
        {
            var entries = team.Players.Select(p => leaderboard[p]).Where(p => p != null).ToArray();

            return entries.Length == 0 ? null : (int?)entries.Average(e => e.Rating);
        }
    }
}