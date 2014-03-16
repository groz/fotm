using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FotM.Utilities
{
    public static class ConfigHelpers
    {
        public static Configuration LoadConfig()
        {
            /* usage:
             * private static readonly Configuration Config = ConfigHelpers.LoadConfig();
             */
            Assembly currentAssembly = Assembly.GetCallingAssembly();
            return ConfigurationManager.OpenExeConfiguration(currentAssembly.Location);
        }
    }
}
