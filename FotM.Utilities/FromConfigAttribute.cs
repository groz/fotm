using System;

namespace FotM.Utilities
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class FromConfigAttribute : Attribute
    {
        public string Name { get; set; }
        public string ConfigFile { get; set; }

        public FromConfigAttribute(string name = null, string configFile = null)
        {
            Name = name;
            ConfigFile = configFile;
        }
    }
}