using System;
using System.Collections.Generic;
using System.Linq;
using Accord.MachineLearning;

namespace FotM.Cassandra
{
    public class AccordKMeans : IKMeans<PlayerChange>
    {
        private readonly Func<double[], double[], double> _distance;
        private readonly bool _normalize;

        public AccordKMeans(bool normalize = false, Func<double[], double[], double> distance = null)
        {
            _normalize = normalize;
            _distance = distance;
        }

        public int[] ComputeGroups(PlayerChange[] dataSet, int nGroups)
        {
            var kmeans = (_distance == null)
                ? new KMeans(nGroups)
                : new KMeans(nGroups, _distance);

            var descriptor = new FeatureAttributeDescriptor<PlayerChange>();

            if (_normalize)
            {
                descriptor.NormalizeFor(dataSet);
            }

            /*
             * history size: 80, steps 1000
            Best F2=0.639566395663957, W=[1.32,1.39,-0.50,0.03,1.34,3.01,3.49]
             */
            //descriptor.SetWeights(new[] {1.32,1.39,-0.50,0.03,1.34,3.01,3.49});

            return kmeans.Compute(dataSet, descriptor);
        }
    }
}