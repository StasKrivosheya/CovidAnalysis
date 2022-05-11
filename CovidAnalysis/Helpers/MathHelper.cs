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

            var tracebackMatrix = new Tuple<int, int>[n + 1, m + 1];

            // calculating transformations and traceback matrices
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

                    if (penalty == transformationsMatrix[i - 1, j])
                    {
                        tracebackMatrix[i, j] = new Tuple<int, int>(i - 1, j);
                    }
                    else if (penalty == transformationsMatrix[i - 1, j - 1])
                    {
                        tracebackMatrix[i, j] = new Tuple<int, int>(i - 1, j - 1);
                    }
                    else
                    {
                        tracebackMatrix[i, j] = new Tuple<int, int>(i, j - 1);
                    }
                }
            }

            // extracting shortest path from trac–eback matrix
            // calculating summary cost

            var shortestPath = new List<Tuple<int, int>>();
            var resultCost = 0d;

            var k = n;
            var l = m;
            for (int i = n + m; i > 0; i--)
            {
                resultCost += transformationsMatrix[k, l];

                shortestPath.Add(new Tuple<int, int>(k, l));

                var nextElementCoordinates = tracebackMatrix[k, l];

                k = nextElementCoordinates.Item1;
                l = nextElementCoordinates.Item2;

                if (k is 0 && l is 0)
                {
                    break;
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
