using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FotM.Utilities;
using NUnit.Framework;

namespace FotM.Cassandra.Tests
{
    [TestFixture]
    internal class FixedSizeKMeansTests
    {
        private class Data
        {
            public Data(int x, int y)
            {
                X = x;
                Y = y;
            }

            [CassandraFeature]
            public int X { get; set; }

            [CassandraFeature]
            public int Y { get; set; }

            public override string ToString()
            {
                return string.Format("(X = {0}, Y = {1}", X, Y);
            }
        }

        private Data[] _inputData = new[]
        {
            new Data(90, 1),
            new Data(100, 0),
            new Data(1, 233),
            new Data(10, 300),
            new Data(50, 50),

            //new Data(94, 2),
            //new Data(110, 5),
            //new Data(130, 11),
            //new Data(98, 4),

            //new Data(11, 310),
            //new Data(8, 290),
            //new Data(5, 265),

        };

        private const int K = 3;
        private IKMeans<Data> _kMeans;

        [SetUp]
        public void Init()
        {
            _kMeans = new MyKMeans<Data>(true);
        }

        [Test]
        public void GroupsShouldBeOfSpecifiedSize()
        {
            int[] groups = _kMeans.ComputeGroups(_inputData, K);

            var groupedData = _inputData
                .Select((d, idx) => new {d, idx})
                .GroupBy(indexedData => groups[indexedData.idx], indexedData => indexedData.d);

            foreach (var group in groupedData)
            {
                Assert.LessOrEqual(group.Count(), K);
            }
        }

        [Test]
        public void NumberOfGroupsShouldBeMaximumPossible()
        {
            int[] groups = _kMeans.ComputeGroups(_inputData, K);

            var groupedData = _inputData
                .Select((d, idx) => new {d, idx})
                .GroupBy(indexedData => groups[indexedData.idx], indexedData => indexedData.d);

            Assert.AreEqual(3, groupedData.Count());
        }

        [Test]
        public void SplitShouldBeGood()
        {
            int[] groups = _kMeans.ComputeGroups(_inputData, K);

            Data[][] groupedData = _inputData
                .Select((data, idx) => new {d = data, idx})
                .GroupBy(indexedData => groups[indexedData.idx], indexedData => indexedData.d)
                .Select(datas => datas.ToArray())
                .ToArray();

            var expectedGroups = new[]
            {
                new[] {_inputData[0], _inputData[1]},
                new[] {_inputData[2], _inputData[3]},
                new[] {_inputData[4]},
            };

            Assert.AreEqual(expectedGroups.Length, groupedData.Length);

            foreach (var expectedGroup in expectedGroups)
            {
                Data[] found = groupedData.FirstOrDefault(datas => datas.ScrambledEquals(expectedGroup));

                Assert.IsNotNull(found);
            }
        }
    }
}
