using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace FotM.Utilities.Tests
{
    [TestFixture]
    public class CompressionTests
    {
        [Test]
        public void UnzippedStringShouldBeTheSameEn()
        {
            const string str = "Hello, world!";

            var zipped = CompressionUtils.Zip(str);
            string unzipped = CompressionUtils.Unzip(zipped);

            Assert.AreEqual(str, unzipped);
        }

        [Test]
        public void UnzippedStringShouldBeTheSameRu()
        {
            const string str = "Привет, мир!";

            var zipped = CompressionUtils.Zip(str);
            string unzipped = CompressionUtils.Unzip(zipped);

            Assert.AreEqual(str, unzipped);
        }
    }
}
