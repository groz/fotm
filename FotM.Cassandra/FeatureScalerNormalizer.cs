using System.Linq;
using Accord.Math;
using FotM.Utilities;

namespace FotM.Cassandra
{
    public class FeatureScalerNormalizer<T> : IFeatureDescriptor<T>
    {
        private readonly IFeatureDescriptor<T> _descriptor;
        private readonly double[] _means;
        private readonly double[] _scales;

        public FeatureScalerNormalizer(IFeatureDescriptor<T> descriptor, T[] trainingSet)
        {
            _descriptor = descriptor;
            int nFeatures = descriptor.TotalFeatures;
            _means = new double[nFeatures];
            _scales = new double[nFeatures];

            for (int iFeature = 0; iFeature < nFeatures; ++iFeature)
            {
                double[] values = trainingSet
                    .Select(x => descriptor.GetFeatureValue(iFeature, x))
                    .ToArray();

                double min, max;
                values.MinMaxAvg(out min, out max, out _means[iFeature]);

                double range = max - min;
                _scales[iFeature] = range.IsRelativelyEqual(0, 1e-5) ? 1 : range;
            }
        }

        public int TotalFeatures
        {
            get { return _descriptor.TotalFeatures; }
        }

        public double GetFeatureValue(int featureIdx, T obj)
        {
            double featureValue = _descriptor.GetFeatureValue(featureIdx, obj);
            return (featureValue - _means[featureIdx])/_scales[featureIdx];
        }
    }
}