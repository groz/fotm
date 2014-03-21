using System;
using NUnit.Framework;

namespace FotM.Utilities.Tests
{
    [TestFixture]
    public class ConfigHelpersTests
    {
        private Demo _demo;

        [SetUp]
        public void Init()
        {
            _demo = new Demo();
            _demo.InjectConfig();
        }

        [Test]
        public void InjectConfig_Should_InjectValuesFromAppConfiguration()
        {
            Assert.AreEqual(100, _demo.UnnamedProperty);
            Assert.AreEqual("some string", _demo.PropertyWithName);
        }

        [Test]
        public void InjectConfig_Should_InjectValuesFromSpecificConfiguration()
        {
            Assert.AreEqual(200, _demo.UnnamedPropertyFromCustomConfig);
            Assert.AreEqual("some custom string", _demo.PropertyWithNameFromCustomConfig);
        }

        [Test]
        public void InjectConfig_Should_OverrideValuesFromCurrentConfig()
        {
            Assert.AreEqual(300, _demo.UnnamedOverridenProperty);
            Assert.AreEqual("overriden string", _demo.OverridenPropertyWithName);
        }
    }

    class Demo
    {
        [FromConfig]
        public int UnnamedProperty { get; set; }

        [FromConfig("NamedProperty")]
        public string PropertyWithName { get; set; }

        [FromConfig(ConfigFile = "Custom.config")]
        public int UnnamedPropertyFromCustomConfig { get; set; }

        [FromConfig("NamedPropertyFromCustomConfig", ConfigFile = "Custom.config")]
        public string PropertyWithNameFromCustomConfig { get; set; }

        [FromConfig(ConfigFile = "Custom.config")]
        public int UnnamedOverridenProperty { get; set; }

        [FromConfig("NamedOverridenProperty", ConfigFile = "Custom.config")]
        public string OverridenPropertyWithName { get; set; }
    }
}