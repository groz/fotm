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
    }
}