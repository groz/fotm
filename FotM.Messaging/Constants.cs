using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FotM.Utilities;
using System.Configuration;

namespace FotM.Messaging
{
    public static class Constants
    {
        static Constants()
        {
            string key = "Microsoft.ServiceBus.ConnectionString";
            ConnectionString = ConfigurationManager.AppSettings[key];

            // only load library specific config if the key is not found in current AppSettings
            if (ConnectionString == null)
            {
                var cfg = ConfigHelpers.LoadConfig();
                var appSettings = cfg.AppSettings.Settings;
                ConnectionString = appSettings[key].Value;
            }
        }

        internal static readonly string ConnectionString;
        internal static readonly string StatsUpdateTopic = "stats-update-topic";
        internal static readonly string QueryLatestStatsQueue = "query-latest-stats-queue";
    }
}
