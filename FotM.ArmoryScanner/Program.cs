using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using FotM.Domain;
using FotM.Utilities;
using log4net;
using log4net.Config;

namespace FotM.ArmoryScanner
{
    /*
     * This middle layer application will monitor armories polling N times per day,
     * merge all updates into single spec power ranking and produce RankingsUpdated messages 
     * for clients on each armory update.
     * It will also listen to GetCurrentRankings messages and respond with result calculated last.
     * http://blizzard.github.io/api-wow-docs/#pvp-api/leaderboard-api
     */

    public static class Armories
    {
        // Hardcoded stuff for now
        public static readonly ArmoryPuller US = new ArmoryPuller("us.battle.net");
        public static readonly ArmoryPuller Europe = new ArmoryPuller("eu.battle.net");
        public static readonly ArmoryPuller Korea = new ArmoryPuller("kr.battle.net");
        public static readonly ArmoryPuller Taiwan = new ArmoryPuller("tw.battle.net");
        public static readonly ArmoryPuller China = new ArmoryPuller("www.battlenet.com.cn");
    }

    class Program
    {
        private static readonly ILog Logger = LoggingExtensions.GetLogger<Program>();

        static void Main()
        {
            XmlConfigurator.Configure();

            Logger.Debug("App started");

            var armories = new[]
            {
                Armories.US,
                //Armories.Europe,
            };

            const int maxSize = 100;
            var history = armories.ToDictionary(a => a, a => new ArmoryHistory(maxSize));

            const int nRunsPerDay = 10000;
            var timeout = TimeSpan.FromDays(1.0/nRunsPerDay);

            Logger.InfoFormat("Sleep timeout set to {0}", timeout);

            int addCount = 0;

            Stopwatch stopwatch = Stopwatch.StartNew();

            while (true)
            {
                foreach (var armoryHistoryPair in history)
                {
                    var armory = armoryHistoryPair.Key;
                    var armoryHistory = armoryHistoryPair.Value;

                    var leaderboardSnapshot = armory.DownloadLeaderboard(Bracket.Threes);

                    if (armoryHistory.Update(leaderboardSnapshot))
                    {
                        ++addCount;
                    }

                    var elapsed = stopwatch.Elapsed;

                    Logger.InfoFormat("Total time running: {0}, total snapshots added: {1}, snapshots per minute: {2}", 
                        elapsed, addCount, addCount/elapsed.TotalMinutes);
                }
                
                Logger.InfoFormat("Sleeping for {0}...", timeout);
                Thread.Sleep(timeout);
            }
        }
    }
}
