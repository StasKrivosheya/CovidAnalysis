﻿using System;
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
            var sourceArr = src.ToArray();
            var N = sourceArr.Length;
            var halfInterval = smoothingInterval / 2;

            var result = new double[N];

            if (N <= smoothingInterval)
            {
                var avg = sourceArr.Average();

                for (int i = 0; i < N; i++)
                {
                    result[i] = avg;
                }

                return src;
            }

            // prepopulating values that don't have the necessary amount of neighbors to calculate their Moving Averages
            for (int i = 0; i < halfInterval; i++)
            {
                result[i] = sourceArr[i];
                result[N - i - 1] = sourceArr[N - i - 1];
            }

            // Smoothing the rest data with Moving Averages
            for (int i = halfInterval; i < N - halfInterval; i++)
            {
                var sum = sourceArr.Skip(i + halfInterval).Take(smoothingInterval).Sum();
                var average = sum / smoothingInterval;

                result[i] = average;
            }

            return result;
        }

        // minimax normalization
        public static IEnumerable<float> GetNormalized(this IEnumerable<float> src)
        {
            var sourceArr = src.ToArray();
            var N = sourceArr.Length;
            var min = sourceArr.Min();
            var max = sourceArr.Max();

            var result = new float[N];

            for (int i = 0; i < N; i++)
            {
                result[i] = (sourceArr[i] - min) / (max - min);
            }

            return result;
        }

        // makes mean = 0, variance = 1
        public static IEnumerable<float> GetStandartized(this IEnumerable<float> src)
        {
            var sourceArr = src.ToArray();
            var N = sourceArr.Length;
            var mean = sourceArr.Average();
            var std = Math.Sqrt(Math.Pow(sourceArr.Select(u => u - mean).Sum(), 2) / N);

            var result = new float[N];

            for (int i = 0; i < N; i++)
            {
                result[i] = (sourceArr[i] - mean) / (float)std;
            }

            return result;
        }
    }
}
