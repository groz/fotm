using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MoreLinq;

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

            return (int)(result/Factorial(k));
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
    }

    public static class RatingUtils
    {
        public static int EstimatedRatingChange(double playerRating, double opponentRating, bool playerWon)
        {
            const double k = 32.0;

            double playerWinChance = 1 / (1 + Math.Pow(10, (opponentRating - playerRating) / 400.0));

            int pw = playerWon ? 1 : 0;

            return (int)(k * (pw - playerWinChance));
        }
    }
}