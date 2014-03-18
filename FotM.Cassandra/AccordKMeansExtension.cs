using System.Collections.Generic;
using Accord.MachineLearning;

namespace FotM.Cassandra
{
    public static class AccordKMeansExtension
    {
        public static int[] Compute<T>(this IClusteringAlgorithm<double[]> kmeans, 
            T[] source,
            IFeatureDescriptor<T> featureDescriptor)
        {
            int nFeatures = featureDescriptor.TotalFeatures;
            
            // transform to double matrix
            int nSamples = source.Length;

            var matrix = new double[nSamples][];

            for (int i = 0; i < nSamples; i++)
            {
                matrix[i] = new double[nFeatures];

                for (int j = 0; j < nFeatures; ++j)
                {
                    double featureValue = featureDescriptor.GetFeatureValue(j, source[i]);
                    matrix[i][j] = featureValue;
                }
            }

            return kmeans.Compute(matrix, 1e-05);
        }
    }
}