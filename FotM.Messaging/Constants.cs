using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FotM.Utilities;

namespace FotM.Messaging
{
    public static class Constants
    {
        static Constants()
        {
            var cfg = ConfigHelpers.LoadConfig();
            var appSettings = cfg.AppSettings.Settings;
            ConnectionString = appSettings["Microsoft.ServiceBus.ConnectionString"].Value;
        }

        public static readonly string ConnectionString;
        public static readonly string Namespace = "fotm-test";
        public static readonly string StatsUpdateTopic = "stats-update-topic";
        public static readonly string QueryLatestStatsQueue = "query-latest-stats-queue";
    }
}
