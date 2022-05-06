using System.Collections.Generic;
using System.Linq;

namespace CovidAnalysis.Extensions
{
    public static class IEnumerableExtension
    {
        /// <summary>
        /// Smoothing Data with Moving Averages
        /// <see cref="https://www.dallasfed.org/research/basics/moving.aspx"/>
        /// </summary>
        /// <param name="src"></param>
        /// <param name="smoothingInterval">The amount of values to take into account while smoothing</param>
        /// <returns>Smoothed collection</returns>
        public static IEnumerable<double> GetSmoothed(this IEnumerable<double> src, int smoothingInterval)
        {
            var sourceList = src.ToArray();
            var N = sourceList.Length;
            var halfInterval = smoothingInterval / 2;

            var result = new double[N];

            // prepopulating values that don't have necessary amount of neigbors to calculate its Moving Averages
            for (int i = 0; i < halfInterval; i++)
            {
                result[i] = sourceList[i];
                result[N - i - 1] = sourceList[N - i - 1];
            }

            // Smoothing rest Data with Moving Averages
            for (int i = halfInterval; i < N - halfInterval; i++)
            {
                var sum = sourceList.Skip(i + halfInterval).Take(smoothingInterval).Sum();
                var average = sum / smoothingInterval;

                result[i] = average;
            }

            return result;
        }
    }
}
