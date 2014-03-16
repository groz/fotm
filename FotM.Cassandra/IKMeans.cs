using System.Collections.Generic;

namespace FotM.Cassandra
{
    public interface IKMeans<T>
    {
        int[] Compute(T[] diffs, int nGroups);
    }
}