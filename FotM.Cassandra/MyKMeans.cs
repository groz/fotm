using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FotM.Utilities;
using MoreLinq;

namespace FotM.Cassandra
{
    public class MyKMeans<T> : IKMeans<T>
    {
        private readonly bool _normalize;
        private readonly Func<IEnumerable<T>, double> _regroupMetric;
        readonly Random _rng = new Random(15250);
        readonly FeatureAttributeDescriptor<T> _descriptor = new FeatureAttributeDescriptor<T>();

        public MyKMeans(bool normalize, Func<IEnumerable<T>, double> regroupMetric = null)
        {
            _normalize = normalize;
            _regroupMetric = regroupMetric;
        }

        private double Distortion(int[] grouping, Vector[] input, Vector[] centroids)
        {
            int m = input.Length;

            double sum = 0;

            for (int i = 0; i < m; ++i)
            {
                int ci = grouping[i]; // index of cluster to which i-th point belongs
                sum += Vector.SquaredDistance(input[i], centroids[ci]);
            }

            return sum / m;
        }

        private int[] ComputeGroupsImpl(T[] dataSet, int k, int? initialPoint, out double distortion)
        {
            int m = dataSet.Length;
            int[] result = new int[m];

            // if number of points in the dataset <= k then assign each to it's own group and return
            if (m <= k)
            {
                for (int i = 0; i < m; ++i)
                {
                    result[i] = i;
                }
                distortion = 0;
                return result;
            }

            if (_normalize)
            {
                _descriptor.NormalizeFor(dataSet);
            }

            Vector[] input = dataSet
                .Select(pt => _descriptor.GetFeatureVector(pt).ToVector())
                .ToArray();

            int n = _descriptor.TotalFeatures;

            //Vector[] centroids = new Vector[k];
            //Vector[] centroids = DumbInit();
            //Vector[] centroids = FarthestInit(input, k);
            Vector[] centroids = KmeansPlusPlusInit(input, k, initialPoint);

            // Forgy initialization of centroids (as opposed to Random Partition)

            // run until convergence
            bool changed = true;
            while (changed)
            {
                changed = false;

                // I. assignment step
                for (int i = 0; i < m; ++i)
                {
                    // find the nearest centroid
                    int nearestCentroid = centroids
                        .MinimumElement(centroid => Vector.SquaredDistance(centroid, input[i]))
                        .Item2;

                    // add point to that cluster
                    int previousClusterIndex = result[i];
                    result[i] = nearestCentroid;

                    if (previousClusterIndex != result[i])
                        changed = true;
                }

                // II. update centroid values
                for (int i = 0; i < k; ++i)
                {
                    var points = result
                        .Select((centroidIdx, index) => new { centroidIdx, index })
                        .Where(r => r.centroidIdx == i)
                        .Select(r => input[r.index])
                        .ToArray();

                    if (points.Length != 0)
                    {
                        centroids[i] = points.Mean();
                    }
                    else
                    {
                        centroids[i] = Vector.Zero(n);
                    }
                }
            }

            distortion = Distortion(result, input, centroids);

            return result;
        }

        public int[] ComputeGroups(T[] dataSet, int k)
        {
            // call it number of times with different seeds and select the best according to some metric
            const int nRuns = 100;
            const int nMinimum = 50;

            int[][] runs = new int[nRuns][];
            double[] distortions = new double[nRuns];

            for (int i = 0; i < nRuns; ++i)
            {
                runs[i] = ComputeGroupsImpl(dataSet, k, i%k, out distortions[i]);
            }
            
            if (_regroupMetric == null)
            {
                // return the one with minimal distortion
                Tuple<double, int> min = distortions.MinimumElement();
                return runs[min.Item2];
            }

            // apply regroup metric to minimally distorted variations
            int[][] minimallyDistorted = runs
                .Select((runResult, idx) => new {runResult, idx})
                .OrderBy(r => distortions[r.idx])
                .Take(nMinimum)
                .Select(r => r.runResult)
                .ToArray();

            double[] distances = new double[nMinimum];

            for (int i = 0; i < nMinimum; ++i)
            {
                int[] runResult = minimallyDistorted[i];

                var groups = dataSet
                    .Select((data, idx) => new {data, idx})
                    .GroupBy(
                        indexedData => runResult[indexedData.idx],
                        indexedData => indexedData.data);

                double totalDistance = groups.Average(g => _regroupMetric(g));
                distances[i] = totalDistance;
            }

            int minIndex = distances.MinimumElement().Item2;
            return minimallyDistorted[minIndex];
        }

        private Vector[] KmeansPlusPlusInit(Vector[] input, int k, int? initialPoint = null)
        {
            int m = input.Length;

            Vector[] centroids = new Vector[k];

            centroids[0] = input[initialPoint ?? _rng.Next(m)];

            for (int i = 1; i < k; i++)
            {
                double[] dx2 = new double[m];

                for (int j = 0; j < m; ++j)
                {
                    var currentCentroids = centroids.Take(i);

                    dx2[j] = currentCentroids.Min(c => Vector.SquaredDistance(c, input[j]));
                }

                double sum = dx2.Sum();
                double rnd = _rng.NextDouble() * sum;

                int idx = 0;
                double runningSum = 0;

                while (runningSum < rnd)
                {
                    runningSum += dx2[idx];
                    ++idx;
                }

                centroids[i] = input[idx-1];
            }

            return centroids;
        }

        private Vector[] FarthestInit(Vector[] input, int k, int? initialPoint)
        {
            int m = input.Length;
            Vector[] centroids = new Vector[k];

            // Pick farthest initial points for clusters
            //centroids[0] = input[0];
            centroids[0] = input[initialPoint ?? _rng.Next(m)];

            for (int i = 1; i < k; ++i)
            {
                // find farthest point from clusters [0..i)

                var currentClusters = centroids.Take(i);

                Vector farthestPoint = input.MaxBy(dataPoint =>
                    currentClusters.Sum(cluster => Vector.SquaredDistance(cluster, dataPoint))
                    );

                centroids[i] = farthestPoint;
            }

            return centroids;
        }
    }
}