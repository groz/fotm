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
            ConnectionString = ConfigHelpers.LoadConfig()
                .AppSettings
                .Settings["Microsoft.ServiceBus.ConnectionString"].ToString();
        }

        public static readonly string ConnectionString;
        public static readonly string Namespace = "fotm-test";
        public static readonly string StatsUpdateTopic = "stats-update-topic";
    }
}
