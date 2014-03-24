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
        readonly Random _rng = new Random(35250);
        readonly FeatureAttributeDescriptor<T> _descriptor = new FeatureAttributeDescriptor<T>();

        public MyKMeans(bool normalize, Func<IEnumerable<T>, double> groupMetric = null)
        {
            _normalize = normalize;
            _regroupMetric = groupMetric ?? SimpleGroupMetric;
        }

        private double SimpleGroupMetric(IEnumerable<T> group)
        {
            Vector[] input = group
                .Select(pt => _descriptor.GetFeatureVector(pt).ToVector())
                .ToArray();

            return input.Mean().Length;
        }

        private int[] ComputeGroupsImpl(T[] dataSet, int k)
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
                return result;
            }

            if (_normalize)
            {
                _descriptor.NormalizeFor(dataSet);
            }

            Vector[] input = dataSet
                .Select(pt => _descriptor.GetFeatureVector(pt).ToVector())
                .ToArray();

            int nFeatures = _descriptor.TotalFeatures;

            //Vector[] centroids = new Vector[k];
            //Vector[] centroids = DumbInit();
            //Vector[] centroids = FarthestInit(input, k);
            Vector[] centroids = KmeansPlusPlusInit(input, k);

            // Forgy initialization of centroids (as opposed to Random Partition)

            // run until convergence
            int nIterations = 0;
            bool changed = true;
            while (changed)
            {
                ++nIterations;
                changed = false;

                // I. assignment step
                for (int i = 0; i < m; ++i)
                {
                    // find the nearest centroid
                    var nearestCentroid = centroids
                        .Select((centroid, index) => new { centroid, index = index })
                        .MinBy(cj => Vector.SquaredDistance(cj.centroid, input[i]));

                    // add point to that cluster
                    int previousClusterIndex = result[i];
                    result[i] = nearestCentroid.index;

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
                        centroids[i] = Vector.Zero(nFeatures);
                    }
                }
            }

            return result;
        }

        public int[] ComputeGroups(T[] dataSet, int k)
        {
            // call it number of times with different seeds and select the best according to some metric
            const int nRuns = 20;

            int[][] runs = Enumerable.Range(0, nRuns)
                .Select(i => ComputeGroupsImpl(dataSet, k))
                .ToArray();

            double[] distances = new double[nRuns];

            for (int i = 0; i < nRuns; ++i)
            {
                int[] runResult = runs[i];

                var groups = dataSet
                    .Select((data, idx) => new {data, idx})
                    .GroupBy(
                        indexedData => runResult[indexedData.idx],
                        indexedData => indexedData.data);

                double totalDistance = groups.Average(g => _regroupMetric(g));
                distances[i] = totalDistance;
            }

            int minIndex = Array.IndexOf(distances, distances.Min());
            return runs[minIndex];
        }

        private Vector[] KmeansPlusPlusInit(Vector[] input, int k)
        {
            int m = input.Length;

            Vector[] centroids = new Vector[k];

            centroids[0] = input[_rng.Next(m)];

            for (int i = 1; i < k; i++)
            {
                double[] dx2 = new double[m];

                for (int j = 0; j < m; ++j)
                {
                    Vector x = input[j];

                    var currentCentroids = centroids.Take(i);

                    dx2[j] = currentCentroids.Min(c => Vector.SquaredDistance(c, x));
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

        private Vector[] FarthestInit(Vector[] input, int k)
        {
            int m = input.Length;
            Vector[] centroids = new Vector[k];

            // Pick farthest initial points for clusters
            //centroids[0] = input[0];
            centroids[0] = input[_rng.Next(m)];

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