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

            if (ConnectionString == null)
            {
                // only load library specific config if the key is not found in current AppSettings
                var cfg = ConfigHelpers.LoadConfig();
                var appSettings = cfg.AppSettings.Settings;
                ConnectionString = appSettings[key].Value;
            }
        }

        public static readonly string ConnectionString;
        public static readonly string Namespace = "fotm-test";
        public static readonly string StatsUpdateTopic = "stats-update-topic";
        public static readonly string QueryLatestStatsQueue = "query-latest-stats-queue";
    }
}
