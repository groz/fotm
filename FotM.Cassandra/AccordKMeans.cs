using System;
using System.Collections.Generic;
using System.Linq;
using Accord.MachineLearning;

namespace FotM.Cassandra
{
    public class AccordKMeans : IKMeans<PlayerDiff>
    {
        private readonly Func<double[], double[], double> _distance;
        private readonly bool _normalize;

        public AccordKMeans(bool normalize = false, Func<double[], double[], double> distance = null)
        {
            _normalize = normalize;
            _distance = distance;
        }

        public int[] ComputeGroups(PlayerDiff[] dataSet, int nGroups)
        {
            var kmeans = (_distance == null)
                ? new KMeans(nGroups)
                : new KMeans(nGroups, _distance);

            var descriptor = new FeatureAttributeDescriptor<PlayerDiff>();

            if (_normalize)
            {
                descriptor.NormalizeFor(dataSet);
            }

            return kmeans.Compute(dataSet, descriptor);
        }
    }
}