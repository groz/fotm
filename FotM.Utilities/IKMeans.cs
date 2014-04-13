namespace FotM.Utilities
{
    public interface IKMeans<T>
    {
        int[] ComputeGroups(T[] dataSet, int nGroups);
    }
}