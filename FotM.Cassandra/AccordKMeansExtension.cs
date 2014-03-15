using System;
using System.Linq;
using Accord.MachineLearning;

namespace FotM.Cassandra
{
    public static class AccordKMeansExtension
    {
        public static int[] Compute<T>(this KMeans kmeans, T[] source)
        {
            // get properties marked with Feature
            var featureProperties = typeof (T).GetProperties()
                .Where(p => p.IsDefined(typeof (FeatureAttribute), false))
                .ToArray();

            int nFeatures = featureProperties.Length;
            
            // transform to double matrix
            int nSamples = source.Length;

            var matrix = new double[nSamples][];

            for (int i = 0; i < nSamples; i++)
            {
                matrix[i] = new double[nFeatures];

                for (int j = 0; j < nFeatures; ++j)
                {
                    matrix[i][j] = Convert.ToDouble( featureProperties[j].GetValue(source[i]) );
                }
            }

            return kmeans.Compute(matrix);
        }
    }
}