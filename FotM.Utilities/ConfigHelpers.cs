using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
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
            string configPath = new Uri(currentAssembly.CodeBase).LocalPath;
            return ConfigurationManager.OpenExeConfiguration(configPath);
        }

        public static Configuration LoadConfig(string configFileName)
        {
            if (!File.Exists(configFileName))
            {
                string msg = string.Format("Config file {0} not found. The name is case-sensitive.", configFileName);
                throw new ArgumentException(msg);
            }

            var configMap = new ExeConfigurationFileMap
            {
                ExeConfigFilename = configFileName
            };
            return ConfigurationManager.OpenMappedExeConfiguration(configMap, ConfigurationUserLevel.None);
        }

        public static void InjectConfig<T>(this T obj)
        {
            var configProperties = ReflectionUtils.GetAttributedProperties<T, FromConfigAttribute>();

            var propertiesFromCurrentApp = configProperties.Where(cp => string.IsNullOrEmpty(cp.Attribute.ConfigFile));

            var propertiesPerConfigFile = configProperties.Except(propertiesFromCurrentApp)
                .GroupBy(cp => cp.Attribute.ConfigFile);

            foreach (var configFileGroup in propertiesPerConfigFile)
            {
                // Read all properties from that config file
                var configFile = LoadConfig(configFileGroup.Key);
                var appSettings = configFile.AppSettings.Settings;

                foreach (var attributedProperty in configFileGroup)
                {
                    string key = attributedProperty.Attribute.Name ?? attributedProperty.PropertyInfo.Name;
                    var cfgElement = appSettings[key];
                    if (cfgElement != null)
                    {
                        string strValue = cfgElement.Value;
                        object value = Convert.ChangeType(strValue, attributedProperty.PropertyInfo.PropertyType);
                        attributedProperty.PropertyInfo.SetValue(obj, value);
                    }
                }
            }

            // Override properties if they are set in current application's config
            foreach (var attributedProperty in configProperties)
            {
                string key = attributedProperty.Attribute.Name ?? attributedProperty.PropertyInfo.Name;
                string strValue = ConfigurationManager.AppSettings[key];
                if (strValue != null)
                {
                    object value = Convert.ChangeType(strValue, attributedProperty.PropertyInfo.PropertyType);
                    attributedProperty.PropertyInfo.SetValue(obj, value);
                }
            }
        }
    }
}
