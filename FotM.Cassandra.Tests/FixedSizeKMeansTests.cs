using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace FotM.Cassandra.Tests
{
    [TestFixture]
    class FixedSizeKMeansTests
    {
        class Data
        {
            public Data(int x, int y)
            {
                X = x; Y = y;
            }

            [AccordFeature]
            public int X { get; set; }

            [AccordFeature]
            public int Y { get; set; }
        }

        [Test]
        public void GroupsShouldBeOfSpecifiedSize()
        {
            var data = new[]
            {
                new Data(90, 1),
                new Data(100, 0),
                new Data(1, 233),
                new Data(10, 300),
                new Data(50, 50),
            };

            int k = 3;

            IKMeans<Data> _kMeans = null;//new FixedSizeKmeans();
            int[] groups = _kMeans.ComputeGroups(data, k);

            var groupedData = data
                .Select((d, idx) => new {d = d, idx})
                .GroupBy(indexedData => groups[indexedData.idx], indexedData => indexedData.d);
        }

        [Test]
        public void NumberOfGroupsShouldBeMaximumPossible()
        {
        }

        [Test]
        public void SplitShouldBeGood()
        {
        }
    }
}
