using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Hosting;

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
                configFileName = Path.Combine(HostingEnvironment.ApplicationPhysicalPath, "bin", configFileName); // TODO: find out how to do it properly

                if (!File.Exists(configFileName))
                {
                    string msg = string.Format("Config file {0} not found. The name is case-sensitive.", configFileName);
                    throw new ArgumentException(msg);
                }
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
                        var value = strValue.ConvertTo(attributedProperty.PropertyInfo.PropertyType);
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
                    var value = strValue.ConvertTo(attributedProperty.PropertyInfo.PropertyType);
                    attributedProperty.PropertyInfo.SetValue(obj, value);
                }
            }
        }

        public static TResult ConvertTo<TResult>(this object value)
        {
            return (TResult)value.ConvertTo(typeof (TResult));
        }

        public static object ConvertTo(this object value, Type t)
        {
            try
            {
                return Convert.ChangeType(value, t);
            }
            catch (Exception ex)
            {
                if (ex is InvalidCastException || ex is FormatException)
                {
                    // try invoking ctor for the type
                    return Activator.CreateInstance(t, value);
                }
                throw;
            }
        }
    }
}
