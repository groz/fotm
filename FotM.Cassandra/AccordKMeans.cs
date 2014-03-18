using System;
using System.Collections.Generic;
using System.Linq;
using Accord.MachineLearning;

namespace FotM.Cassandra
{
    public class AccordKMeans : IKMeans<PlayerDiff>
    {
        private readonly Func<double[], double[], double> _distance;

        public AccordKMeans(Func<double[], double[], double> distance = null)
        {
            _distance = distance;
        }

        public int[] Compute(PlayerDiff[] diffs, int nGroups)
        {
            KMeans kMeans = (_distance == null)
                ? new KMeans(nGroups)
                : new KMeans(nGroups, _distance);

            return kMeans.Compute(diffs);
        }
    }
}