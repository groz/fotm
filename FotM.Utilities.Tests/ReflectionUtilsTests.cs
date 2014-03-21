using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace FotM.Utilities.Tests
{
    [TestFixture]
    public class ReflectionUtilsTests
    {
        class DemoAttribute: Attribute
        {
            public int V { get; set; }

            public DemoAttribute(int v)
            {
                V = v;
            }
        }

        public class Demo
        {
            [DemoAttribute(11)]
            public int Field1 { get; set; }

            [DemoAttribute(22)]
            public int Field2 { get; set; }

            public int Field3 { get; set; }
        }

        [Test]
        public void GetAttributedProperty_Should_FindAllAttributedProperties()
        {
            var attributedProperties = ReflectionUtils.GetAttributedProperties<Demo, DemoAttribute>();

            Assert.AreEqual(2, attributedProperties.Length);

            var properties = attributedProperties.Select(ap => ap.PropertyInfo);
            CollectionAssert.Contains(properties, typeof(Demo).GetProperty("Field1"));
            CollectionAssert.Contains(properties, typeof(Demo).GetProperty("Field2"));
        }

        [Test]
        public void GetAttributedProperty_Should_MatchAttributeValuesCorrectly()
        {
            var attributedProperties = ReflectionUtils.GetAttributedProperties<Demo, DemoAttribute>();

            Assert.AreEqual(2, attributedProperties.Length);

            var field1Property = attributedProperties.First(p => p.PropertyInfo.Name == "Field1");
            var field2Property = attributedProperties.First(p => p.PropertyInfo.Name == "Field2");

            Assert.AreEqual(11, field1Property.Attribute.V);
            Assert.AreEqual(22, field2Property.Attribute.V);
        }

        [Test]
        public void GetAttributedProperty_Should_ReturnCalcPropertyValue()
        {
            var attributedProperties = ReflectionUtils.GetAttributedProperties<Demo, DemoAttribute>();

            Assert.AreEqual(2, attributedProperties.Length);

            var field1Property = attributedProperties.First(p => p.PropertyInfo.Name == "Field1");
            var field2Property = attributedProperties.First(p => p.PropertyInfo.Name == "Field2");

            var demoObject = new Demo {Field1 = 1, Field2 = 2};

            Assert.AreEqual(1, field1Property.GetValue<int>(demoObject));
            Assert.AreEqual(2, field2Property.GetValue<int>(demoObject));
        }
    }
}
