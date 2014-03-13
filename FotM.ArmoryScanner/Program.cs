using FotM.Domain;
using FotM.Utilities;
using log4net;
using log4net.Config;

namespace FotM.ArmoryScanner
{
    class Program
    {
        private static readonly ILog Logger = LoggingExtensions.GetLogger<Program>();

        static void Main()
        {
            XmlConfigurator.Configure();

            Logger.Debug("App started");

            var rawArmoryPuller = new RawJsonPuller("http://us.battle.net/api/wow/leaderboard/");

            var result = rawArmoryPuller.DownloadJson<Leaderboard>("2v2");
            result.Bracket = Bracket.Threes;
            string str = result.Rows[0].Name;
            Logger.Info(str);
        }
    }

    // http://blizzard.github.io/api-wow-docs/#pvp-api/leaderboard-api
}
