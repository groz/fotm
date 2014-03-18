using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace FotM.Utilities.Tests
{
    [TestFixture]
    public class MathUtilsTests
    {
        [Test]
        public void MinMaxAvgTest()
        {
            double[] data = new[] {1.0, 2, 6};

            double min, max, avg;
            data.MinMaxAvg(out min, out max, out avg);

            Assert.AreEqual(1.0, min);
            Assert.AreEqual(6.0, max);
            Assert.AreEqual(3.0, avg);
        }
    }
}
