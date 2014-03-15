using System.Diagnostics;
using System.Linq;
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

    public static class ArmoryConstants
    {
        // Hardcoded stuff for now
        public static readonly string US = "us.battle.net";
        public static readonly string Europe = "eu.battle.net";
        public static readonly string Korea = "kr.battle.net";
        public static readonly string Taiwan = "tw.battle.net";
        public static readonly string China = "www.battlenet.com.cn";
    }

    class Program
    {
        private static readonly ILog Logger = LoggingExtensions.GetLogger<Program>();

        static void Main()
        {
            XmlConfigurator.Configure();

            Logger.Info("App started");

            var usArmoryScanner = new ArmoryScanner(Bracket.Threes, ArmoryConstants.Europe, maxHistorySize: 100);

            Runner runner = Runner.TimesPerDay(usArmoryScanner.Scan, nTimes: 30000);

            runner.Run();
        }
    }
}
