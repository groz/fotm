using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace FotM.Cassandra.Tests
{
    [TestFixture]
    public class CassandraUtilsTests
    {
        class Temp
        {
            [CassandraFeature]
            public int Feature1 { get; set; }

            [CassandraFeature]
            public int Feature2 { get; set; }

            public int NonFeature { get; set; } 
        }

        [Test]
        public void FeatureAttributeDescriptor_GetFeatureIndex()
        {
            var descriptor = new FeatureAttributeDescriptor<Temp>();

            Assert.DoesNotThrow(() => descriptor.GetFeatureIndex("Feature1"));
            Assert.DoesNotThrow(() => descriptor.GetFeatureIndex("Feature2"));
            Assert.Throws<ArgumentException>(() => descriptor.GetFeatureIndex("NonFeature"));
        }

        [Test]
        public void FeatureAttributeDescriptor_GetFeatureValueString()
        {
            var descriptor = new FeatureAttributeDescriptor<Temp>();

            var obj = new Temp {Feature1 = 35};

            //Assert.AreEqual(35, descriptor.GetFeatureValue("Feature1", obj));
        }

        [Test]
        public void FeatureAttributeDescriptor_GetFeatureValueByIndex()
        {
            var descriptor = new FeatureAttributeDescriptor<Temp>();

            var obj = new Temp { Feature1 = 35, Feature2 = 70};

            var idx = descriptor.GetFeatureIndex("Feature2");

            Assert.AreEqual(70, descriptor.GetFeatureValue(idx, obj));
        }

    }
}
