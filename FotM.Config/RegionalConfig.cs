using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FotM.Utilities;

namespace FotM.Config
{
    /// <summary>
    /// Loads configs from Regional.Config file and overrides each setting if set in current App.Config of the running application
    /// </summary>
    public class RegionalConfig
    {
        private static readonly RegionalConfig Config = new RegionalConfig();

        public static RegionalConfig Instance
        {
            get { return Config; }
        }

        public RegionalConfig()
        {
            this.InjectConfig();
        }

        [FromConfig(ConfigFile = "Regional.config")]
        public string Region { get; private set; }

        [FromConfig(ConfigFile = "Regional.config")]
        public string ServiceBusConnectionString { get; private set; }

        [FromConfig(ConfigFile = "Regional.config")]
        public Uri BlizzardApiEndPoint { get; private set; }
    }
}