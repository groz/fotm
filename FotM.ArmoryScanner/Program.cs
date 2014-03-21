using System.Diagnostics;
using System.Linq;
using FotM.Config;
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

    class Program
    {
        private static readonly ILog Logger = LoggingExtensions.GetLogger<Program>();

        static void Main()
        {
            XmlConfigurator.Configure();

            Logger.Info("App started");

            var puller = new ArmoryPuller(RegionalConfig.Instance.BlizzardApiEndPoint);
            var usArmoryScanner = new ArmoryScanner(Bracket.Threes, puller, maxHistorySize: 100);

            while (true)
            {
                usArmoryScanner.Scan();
            }
        }
    }
}
