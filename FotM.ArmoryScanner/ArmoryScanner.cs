using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using FotM.Domain;
using FotM.Messaging;
using FotM.Utilities;
using log4net;
using Newtonsoft.Json;

namespace FotM.ArmoryScanner
{
    public class ArmoryScanner
    {
        private static readonly ILog Logger = LoggingExtensions.GetLogger<ArmoryScanner>();

        private const string DbFile = "data.txt"; // TODO: add real DB

        private readonly Bracket _bracket;
        private readonly IArmoryPuller _dataPuller;
        private readonly ArmoryHistory _history;
        private int _updateCount;
        private Stopwatch _stopwatch;
        private Leaderboard _previousLeaderboard = null;
        private readonly Dictionary<Team, TeamStats> _teamStats = new Dictionary<Team, TeamStats>();

        private readonly StatsUpdatePublisher _statsUpdatePublisher = new StatsUpdatePublisher();
        private readonly QueryLatestStatsClient _queryLatestStatsClient = new QueryLatestStatsClient();

        public ArmoryScanner(Bracket bracket, IArmoryPuller dataPuller, int maxHistorySize)
        {
            _bracket = bracket;
            _dataPuller = dataPuller;
            _history = new ArmoryHistory(maxHistorySize);

            LoadFromDb();
        }

        private void SaveToDb()
        {
            Logger.InfoFormat("Saving all data to file {0}", DbFile);

            try
            {
                string serializedStats = SerializeStats();
                File.WriteAllText(DbFile, serializedStats);
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("Saving to file failed with {0}.", ex.Message);
            }
        }

        private void LoadFromDb()
        {
            if (File.Exists(DbFile))
            {
                try
                {
                    Logger.InfoFormat("File {0} found, trying to load persisted data...", DbFile);

                    string json = File.ReadAllText(DbFile);

                    TeamStats[] persistedStats = DeserializeStats(json);

                    foreach (var teamStats in persistedStats)
                    {
                        _teamStats[teamStats.Team] = teamStats;
                    }

                    Logger.InfoFormat("Persisted data loaded successfully.");
                }
                catch (Exception ex)
                {
                    Logger.ErrorFormat("Loading persisted data failed with {0}. Initializing anew.", ex.Message);
                }
            }
            else
            {
                Logger.InfoFormat("File {0} not found. Initializing anew.", DbFile);
            }
        }

        private bool OnQueryLatestStatsMessage(QueryLatestStatsMessage msg)
        {
            Logger.InfoFormat("Received QueryLatestStatsMessage from {0}", msg.QueryingHost);

            if (_teamStats.Any())
            {
                // TODO: refactor to publish to the private queue of requester instead of all topic listeners
                PublishStats();
                return true;
            }

            Logger.InfoFormat("No stats to send to {0}.", msg.QueryingHost);

            return false;
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

                TimeSpan waitPeriod = TimeSpan.FromSeconds(10);
                Logger.InfoFormat("Checking for messages/sleeping for {0}/{0}...", waitPeriod);
                _queryLatestStatsClient.Receive(OnQueryLatestStatsMessage, waitPeriod);
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
            {
                LogStats();
                PublishStats();
                SaveToDb();
            }

            RemoveHealers();
            SaveToDb();
        }

        private void RemoveHealers()
        {
            var overhealingTeams = _teamStats.Values
                .Where(ts => ts.Team.Players.Count(Healers.IsHealer) >= 2)  // fuck the double healers
                .Select(ts => ts.Team)
                .ToArray();

            foreach (var overhealers in overhealingTeams)
            {
                _teamStats.Remove(overhealers);
                Logger.InfoFormat("Overhealing team {0} removed", overhealers);
            }
        }

        public void PublishStats()
        {
            var updateMessage = new StatsUpdateMessage()
            {
                Stats = _teamStats.Values.ToArray()
            };

            Logger.InfoFormat("Publishing update message...");
            _statsUpdatePublisher.Publish(updateMessage);
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

            foreach (var team in verifiedTeams.OrderByDescending(t => t.Stats.Rating))
            {
                Logger.InfoFormat("Team: {0} ({1}), Rating: {3} ({4}{5}), TimesSeen: {2}, Updated: {6}", 
                    team.Team, 
                    team.Setup, 
                    team.Stats.TimesSeen, 
                    team.Stats.Rating,
                    team.Stats.RatingChange > 0 ? "+" : "", 
                    team.Stats.RatingChange,
                    team.Stats.UpdatedUtc.ToLocalTime());
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