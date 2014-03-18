namespace FotM.Cassandra
{
    public interface IFeatureDescriptor<T>
    {
        int TotalFeatures { get; }
        double GetFeatureValue(int featureIdx, T obj);
    }
}