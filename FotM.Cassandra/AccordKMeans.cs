using System.Collections.Generic;
using System.Linq;
using Accord.MachineLearning;

namespace FotM.Cassandra
{
    public class AccordKMeans : IKMeans<PlayerDiff>
    {
        public int[] Compute(PlayerDiff[] diffs, int nGroups)
        {
            KMeans kMeans = new KMeans(nGroups);
            return kMeans.Compute(diffs);
        }
    }
}