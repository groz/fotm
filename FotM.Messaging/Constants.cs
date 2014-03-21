using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FotM.Config;
using FotM.Utilities;
using System.Configuration;

namespace FotM.Messaging
{
    public static class Constants
    {
        static Constants()
        {
            ConnectionString = RegionalConfig.Instance.ServiceBusConnectionString;
        }

        internal static readonly string ConnectionString;
        internal static readonly string StatsUpdateTopic = "stats-update-topic";
        internal static readonly string QueryLatestStatsQueue = "query-latest-stats-queue";
    }
}
