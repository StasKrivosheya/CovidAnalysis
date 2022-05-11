using System;
using System.Linq;

namespace CovidAnalysis.Helpers
{
    public static class MathHelper
    {
        public static double[,] GetTransformationsMatrix(double[] x1, double[] x2)
        {
            var n = x1.Length;
            var m = x2.Length;

            var dist = GetDistanceMatrix(x1, x2);

            var transformationsMatrix = GetPrepopulatedTransformationsMatrix(n + 1, m + 1);

            for (int i = 1; i <= n; i++)
            {
                for (int j = 1; j <= m; j++)
                {
                    var dist_i_j = dist[i - 1, j - 1];

                    var penalty = new double[]
                    {
                        transformationsMatrix[i - 1, j],
                        transformationsMatrix[i - 1, j - 1],
                        transformationsMatrix[i, j - 1],
                    }.Min();

                    transformationsMatrix[i, j] = dist_i_j + penalty;
                }
            }

            return transformationsMatrix;
        }

        private static double[,] GetDistanceMatrix(double[] x1, double[] x2)
        {
            var n = x1.Length;
            var m = x2.Length;

            var result = new double[n, m];

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    result[i, j] = Math.Abs(x1[i] - x2[j]);
                }
            }

            return result;
        }

        private static double[,] GetPrepopulatedTransformationsMatrix(int n, int m)
        {
            var result = new double[n, m];

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    if (i is 0 || j is 0)
                    {
                        result[i, j] = double.PositiveInfinity;
                    }
                    else
                    {
                        result[i, j] = 0d;
                    }
                }
            }

            result[0, 0] = 0;

            return result;
        }
    }
}
