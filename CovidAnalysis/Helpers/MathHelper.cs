using System;
using System.Collections.Generic;
using System.Linq;
using CovidAnalysis.Models.CalculationItems;

namespace CovidAnalysis.Helpers
{
    public static class MathHelper
    {
        #region -- Dynamic Time Warping Algorithm --

        public static DTWCalcResult CalculateDtw(float[] x1, float[] x2)
        {
            var n = x1.Length;
            var m = x2.Length;

            var dist = GetDistanceMatrix(x1, x2);

            var transformationsMatrix = GetPrepopulatedTransformationsMatrix(n + 1, m + 1);

            var tracebackMatrix = new (int, int)[n + 1, m + 1];

            // calculating transformations and traceback matrices
            for (int i = 1; i <= n; i++)
            {
                for (int j = 1; j <= m; j++)
                {
                    var dist_i_j = dist[i - 1, j - 1];

                    var penalty = Min(transformationsMatrix[i - 1, j],
                        transformationsMatrix[i - 1, j - 1],
                        transformationsMatrix[i, j - 1]);

                    transformationsMatrix[i, j] = dist_i_j + penalty;

                    if (penalty == transformationsMatrix[i - 1, j])
                    {
                        tracebackMatrix[i, j] = new(i - 1, j);
                    }
                    else if (penalty == transformationsMatrix[i - 1, j - 1])
                    {
                        tracebackMatrix[i, j] = new(i - 1, j - 1);
                    }
                    else
                    {
                        tracebackMatrix[i, j] = new(i, j - 1);
                    }
                }
            }

            // extracting shortest path from traceback matrix
            // calculating summary cost

            var shortestPath = new List<Tuple<int, int>>();
            var resultCost = 0f;

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

            // creating a correspondence between elements of two time series
            var matchingElements = new List<MatchingElementsModel>();

            shortestPath.Reverse();

            for (int i = 0; i < n; i++)
            {
                // taking the coordinates of all points (from the current row) that belongs to the optimal path
                // - 1 is needed because the shortestPath contains indices of the extended transformations matrix
                var currentRowTuples = shortestPath.Where(coordinate => coordinate.Item1 - 1 == i).ToList();

                // extracting corresponding column
                var correspondingColumns = currentRowTuples.Select(coordinate => coordinate.Item2 - 1).ToList();

                // extracting corresponding x2 values
                var correspondingColumnsValues = new List<float>();
                foreach (var index in correspondingColumns)
                {
                    correspondingColumnsValues.Add(x2[index]);
                }

                matchingElements.Add(new MatchingElementsModel
                {
                    X1Value = x1[i],
                    CorrespondingX2Values = correspondingColumnsValues,
                });
            }

            resultCost /= shortestPath.Count;

            var result = new DTWCalcResult
            {
                Cost = resultCost,
                MatchingElements = matchingElements
            };

            return result;
        }

        private static float[,] GetDistanceMatrix(float[] x1, float[] x2)
        {
            var n = x1.Length;
            var m = x2.Length;

            var result = new float[n, m];

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    result[i, j] = Math.Abs(x1[i] - x2[j]);
                }
            }

            return result;
        }

        private static float[,] GetPrepopulatedTransformationsMatrix(int n, int m)
        {
            var result = new float[n, m];

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    if (i is 0 || j is 0)
                    {
                        result[i, j] = float.PositiveInfinity;
                    }
                    else
                    {
                        result[i, j] = 0f;
                    }
                }
            }

            result[0, 0] = 0;

            return result;
        }

        private static float Min(float x, float y, float z)
        {
            float min = x;
            if (x <= y && x <= z)
                min = x;
            if (y <= x && y <= z)
                min = y;
            if (z <= x && z <= y)
                min = z;

            return min;
        }

        #endregion
    }
}
