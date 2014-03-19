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
        
    }
}
