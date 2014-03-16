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

            var section = cfg.AppSettings;
            var strs = cfg.ConnectionStrings;

            Console.WriteLine(section.Settings.Count);

            ConnectionString = section
                .Settings["Microsoft.ServiceBus.ConnectionString"]
                .ToString();
        }

        public static readonly string ConnectionString;
        public static readonly string Namespace = "fotm-test";
        public static readonly string StatsUpdateTopic = "stats-update-topic";
    }
}
