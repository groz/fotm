using System;
using System.Collections;
using System.Collections.Generic;
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
            return x*x;
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

            return (int) (result/Factorial(k));
        }

        public static IEnumerable<T[]> Combinations<T>(this IEnumerable<T> source, int k)
        {
            var src = source.ToArray();

            return Accord.Math.Combinatorics.Combinations(src, k);
        }

        public static IEnumerable<IEnumerable<T>> CartesianProduct<T>(this IEnumerable<IEnumerable<T>> sequences)
        {
            IEnumerable<IEnumerable<T>> emptyProduct = new[] {Enumerable.Empty<T>()};
            return sequences.Aggregate(
                emptyProduct,
                (accumulator, sequence) =>
                    from accseq in accumulator
                    from item in sequence
                    select accseq.Concat(new[] {item})
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

        public static double EuclideanDistance(double[] vectorX, double[] vectorY)
        {
            return vectorX.Select((x, i) => (x - vectorY[i]).Squared()).Sum();
        }

    }

    public static class Functional
    {
        public static double[] FindMinimum(
            Func<double[], double> f,
            double[] q, double learningRate,
            Action<int, double[], double> reportProgress = null)
        {
            const double dx = 1e-12;

            double[] qnext = q.ToArray();

            double diff = 0;

            // if we are not descending after N iterations break
            int iteration = 0;

            do
            {
                q = qnext.ToArray();

                for (int i = 0; i < q.Length; ++i)
                {
                    qnext[i] = q[i] - learningRate*PartialDerivative(f, q, i, dx);
                }

                if (reportProgress != null)
                {
                    reportProgress(iteration, qnext, f(qnext));
                }

                diff = f(qnext) - f(q);
                iteration++;
            } while (diff < 0); // while we are actually descending

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
            return (fNext - fCurrent)/dv;
        }

        public static double Derivative(Func<double, double> f, double x, double dx)
        {
            return (f(x + dx) - f(x))/dx;
        }

        public static double Length(double[] vector)
        {
            return Math.Sqrt(vector.Sum(x => x*x));
        }
    }
}
