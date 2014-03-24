using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace FotM.Utilities
{
    public static class MathUtils
    {
        public static void MinMaxAvg(this double[] array, out double min, out double max, out double avg)
        {
            min = max = avg = array[0];

            for (int i = 1; i < array.Length; ++i)
            {
                if (array[i] < min)
                    min = array[i];
                if (array[i] > max)
                    max = array[i];

                avg += array[i];
            }

            avg /= array.Length;
        }

        public static double Squared(this double x)
        {
            return x * x;
        }

        public static int NumberOfCombinations(int n, int k)
        {
            if (k > n)
                return 0;

            long result = n;

            for (int i = 1; i < k; ++i)
            {
                result *= n - i;
            }

            return (int)(result / Factorial(k));
        }

        public static IEnumerable<T[]> Combinations<T>(this IEnumerable<T> source, int k)
        {
            var src = source.ToArray();

            return Accord.Math.Combinatorics.Combinations(src, k);
        }

        public static IEnumerable<IEnumerable<T>> CartesianProduct<T>(this IEnumerable<IEnumerable<T>> sequences)
        {
            IEnumerable<IEnumerable<T>> emptyProduct = new[] { Enumerable.Empty<T>() };
            return sequences.Aggregate(
                emptyProduct,
                (accumulator, sequence) =>
                    from accseq in accumulator
                    from item in sequence
                    select accseq.Concat(new[] { item })
                );
        }


        public static int Factorial(int n)
        {
            int[] fact =
            {
                1, 1, 2, 6, 24, 120, 720, 5040, 40320,
                362880, 3628800, 39916800, 479001600
            };

            if (n > 12)
                throw new OverflowException("N should be <= 12");

            return fact[n];
        }

        public static double EuclideanDistance(IEnumerable<double> x, IEnumerable<double> y)
        {
            return x.Zip(y, (xi, yi) => Squared((xi - yi))).Sum();
        }
    }

    public static class Functional
    {
        public static double[] FindMinimum(
            Func<double[], double> f,
            double[] q, double learningRate,
            double eps,
            int nMaxIterations)
        {
            double[] qnext = q.ToArray();

            // if we are not descending after N iterations break
            int iteration = 0;

            do
            {
                Trace.WriteLine(iteration);

                q = qnext.ToArray();

                for (int i = 0; i < q.Length; ++i)
                {
                    qnext[i] = q[i] - learningRate * PartialDerivative(f, q, i, eps);
                }

            } while (iteration++ < nMaxIterations);

            return q;
        }

        public static double PartialDerivative(Func<double[], double> f, double[] q, int nParam, double dv)
        {
            //Func<double, double> g = p =>
            //{
            //    var @params = q.ToArray();
            //    @params[nParam] = p;
            //    return f(@params);
            //};

            //return Derivative(g, q[nParam], dv);

            // Only for optimization :(
            var paramValue = q[nParam];

            var fCurrent = f(q);
            q[nParam] += dv;
            var fNext = f(q);

            q[nParam] = paramValue;
            return (fNext - fCurrent) / dv;
        }

        public static double Derivative(Func<double, double> f, double x, double dx)
        {
            return (f(x + dx) - f(x)) / dx;
        }

        public static double Length(double[] vector)
        {
            return Math.Sqrt(vector.Sum(x => x * x));
        }
    }

    public class Vector : List<double>
    {
        public Vector(params double[] arr)
            : base(arr)
        {
        }

        public Vector(IEnumerable<double> arr)
            : base(arr)
        {
        }

        public static Vector Zero(int size)
        {
            return new Vector(Enumerable.Repeat(0.0, size));
        }

        public static Vector Sum(Vector left, Vector right)
        {
            var result = Enumerable.Zip(left, right, (li, ri) => li + ri);
            return new Vector(result);
        }

        public double Length
        {
            get { return Math.Sqrt(SquaredLength); }
        }

        public double SquaredLength
        {
            get { return this.Select(v => v.Squared()).Sum(); }
        }

        public override string ToString()
        {
            return "(" + string.Join(", ", this.Select(v => v.ToString("F1"))) + ")";
        }

        public static double SquaredDistance(Vector left, Vector right)
        {
            return MathUtils.EuclideanDistance(left, right);
        }
    }

    public static class VectorExtensions
    {
        public static Vector Scale(this Vector v, double factor)
        {
            return new Vector(v.Select(vi => vi * factor));
        }

        public static Vector ToVector(this double[] arr)
        {
            return new Vector(arr);
        }

        public static Vector Mean(this Vector[] vectors)
        {
            int n = vectors.Length;
            return vectors.Aggregate(Vector.Sum).Scale(1 / (double)n);
        }
    }
}
