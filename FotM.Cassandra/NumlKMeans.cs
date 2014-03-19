using System.Collections.Generic;
using numl.Math.Metrics;
using numl.Model;
using numl.Unsupervised;

namespace FotM.Cassandra
{
    public class NumlKMeans: IKMeans<PlayerDiff>
    {
        private readonly IDistance _distanceMetric;

        private static readonly Descriptor DiffDescriptor = Descriptor.Create<PlayerDiff>();

        public NumlKMeans(IDistance distanceMetric = null)
        {
            _distanceMetric = distanceMetric ?? new HammingDistance();
        }

        public int[] ComputeGroups(PlayerDiff[] dataSet, int nGroups)
        {
            var kMeans = new KMeans();
            kMeans.Descriptor = DiffDescriptor;

            if (_distanceMetric != null) 
            return kMeans.Generate(dataSet, nGroups, _distanceMetric);
            else return kMeans.Generate(dataSet, nGroups);
        }
    }
}