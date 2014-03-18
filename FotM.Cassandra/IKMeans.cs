using System.Collections.Generic;

namespace FotM.Cassandra
{
    public interface IKMeans<T>
    {
        int[] ComputeGroups(T[] dataSet, int nGroups);
    }
}