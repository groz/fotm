using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FotM.Utilities
{
    public static class CollectionExtensions
    {
        private static int _shuffleSeed = new Random().Next();

        /// <summary>
        /// Randomly shuffles source collection.
        /// </summary>
        /// <typeparam name="T">Type of elements in source collection.</typeparam>
        /// <param name="source">Collection (it will be consumed) to shuffle.</param>
        /// <param name="rng">Random number generator for shuffling.</param>
        /// <returns>Shuffled collection.</returns>
        public static IEnumerable<T> Shuffle<T>(
            this IEnumerable<T> source,
            Random rng = null)
        {
            rng = rng ?? new Random(Interlocked.Increment(ref _shuffleSeed));

            T[] arr = source.ToArray();

            for (int i = arr.Length - 1; i >= 0; --i)
            {
                int j = rng.Next(i + 1);
                yield return arr[j];
                arr[j] = arr[i];    // mini-swap
            }
        }

        public static T[] GetValues<T>() where T: struct
        {
            return Enum.GetValues(typeof (T)).Cast<T>().ToArray();
        }

        public static bool ScrambledEquals<T>(this IEnumerable<T> list1, IEnumerable<T> list2)
        {
            var cnt = new Dictionary<T, int>();
            foreach (T s in list1)
            {
                if (cnt.ContainsKey(s))
                {
                    cnt[s]++;
                }
                else
                {
                    cnt.Add(s, 1);
                }
            }
            foreach (T s in list2)
            {
                if (cnt.ContainsKey(s))
                {
                    cnt[s]--;
                }
                else
                {
                    return false;
                }
            }
            return cnt.Values.All(c => c == 0);
        }

        public static Tuple<T, int> MinimumElement<T>(this IEnumerable<T> source)
            where T : IComparable<T>
        {
            return source.MinimumElement(t => t);
        }

        public static Tuple<T, int> MinimumElement<T, TKey>(this IEnumerable<T> source, Func<T, TKey> selector)
            where TKey: IComparable<TKey>
        {
            if (source == null)
                throw new ArgumentException("Can't find minimum element in null collection");

            T minValue = default (T);
            int? minIdx = null;

            int i = 0;

            foreach (var element in source)
            {
                if (!minIdx.HasValue)
                {
                    minIdx = 0;
                    minValue = element;
                    ++i;
                    continue;
                }
                else if (selector(element).CompareTo(selector(minValue)) < 0)
                {
                    minValue = element;
                    minIdx = i;
                }

                ++i;
            }

            if (!minIdx.HasValue)
                throw new ArgumentException("Can't find minimum element in empty collection");

            return Tuple.Create(minValue, minIdx.Value);
        }
    }
}
