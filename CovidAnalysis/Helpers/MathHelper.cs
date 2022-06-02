using System;
using System.Collections.Generic;
using System.Linq;
using CovidAnalysis.Models.CalculationItems;

namespace CovidAnalysis.Helpers
{
    public static class MathHelper
    {
        #region -- Dynamic Time Warping Algorithm --

        public static DTWCalcResult CalculateDtw(double[] x1, double[] x2)
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
                var correspondingColumnsValues = new List<double>();
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

        private static double Min(double x, double y, double z)
        {
            double min = x;
            if (x <= y && x <= z)
                min = x;
            if (y <= x && y <= z)
                min = y;
            if (z <= x && z <= y)
                min = z;

            return min;
        }

        #endregion

        #region -- Forecasting --

        public static class Forecasting
        {
            // If α is small (i.e., close to 0), more weight is given to observations from the more distant past.
            // If α is large (i.e., close to 1), more weight is given to the more recent observations.
            public static double SimpleExponentialSmoothing(List<double> y, double alpha)
            {
                var N = y.Count;
                var nextValue = 0d;

                for (int i = 0; i < N; i++)
                {
                    var y_t = y[N - 1 - i];
                    var exp = Math.Pow(1 - alpha, i);

                    var finalMiltiplier = alpha * exp;
                    var addendum = finalMiltiplier * y_t;

                    nextValue += addendum;
                }

                return nextValue;
            }

            public static List<double> SimpleExponentialSmoothing(List<double> y, int horizon, double alpha)
            {
                var result = new List<double>();

                if (y.Count > 3)
                {
                    // l0 initialization
                    var l = (y[0] + y[1] + y[2]) / 3;
                    result.Add(l);

                    for (int t = 1; t < y.Count; t++)
                    {
                        var previousL = result.LastOrDefault();

                        l = (alpha * y[t]) + ((1 - alpha) * previousL);

                        result.Add(l);
                    }

                    for (int h = 0; h < horizon; h++)
                    {
                        result.Add(result.LastOrDefault());
                    }
                }

                return result;
            }

            public static List<double> HoltsLinearTrendForecast(List<double> y, int horizon, double alpha, double beta)
            {
                var result = new List<double>();

                if (y.Count > 3)
                {
                    var coefficients = GetHoltsLinearTrendCoefficients(y, alpha, beta);
                    var l_t = coefficients.Last().Item1;
                    var b_t = coefficients.Last().Item2;

                    for (int h = 1; h <= horizon; h++)
                    {
                        var forecastedY = l_t + (h * b_t);
                        result.Add(forecastedY);
                    }
                }

                return result;
            }

            private static List<(double, double)> GetHoltsLinearTrendCoefficients(List<double> y, double alpha, double beta)
            {
                var coefficients = new List<(double, double)>();

                // l0 initialization
                var l = (y[0] + y[1] + y[2]) / 3;
                // b0 initialization
                var b = 0d;
                coefficients.Add((l, b));

                for (int t = 1; t < y.Count; t++)
                {
                    var prevL = coefficients.Last().Item1;
                    var prevB = coefficients.Last().Item2;

                    l = (alpha * y[t]) + ((1 - alpha) * (prevL + prevB));
                    b = (beta * (l - prevL)) + ((1 - beta) * prevB);

                    coefficients.Add((l, b));
                }

                return coefficients;
            }

            // phi = 1  =>  Holt's method
            // phi = 0  =>  SES method
            public static List<double> DampedTrendForecast(List<double> y, int horizon, double alpha, double beta, double phi)
            {
                var result = new List<double>();

                if (y.Count > 3)
                {
                    var coefficients = GetHoltsLinearTrendCoefficients(y, alpha, beta);
                    var l_t = coefficients.Last().Item1;
                    var b_t = coefficients.Last().Item2;

                    var dampingMultiplier = 0d;
                    for (int h = 1; h <= horizon; h++)
                    {
                        dampingMultiplier += Math.Pow(phi, h);
                        var forecastedY = l_t + (dampingMultiplier * b_t);
                        result.Add(forecastedY);
                    }
                }

                return result;
            }

            // http://www.cleverstudents.ru/articles/mnk.html
            // for linear regression - linear func approximation
            public static (double, double) GetLinearRegressionCoeficients(List<double> y)
            {
                var N = y.Count;
                var x = Enumerable.Range(1, N).ToArray();

                var xSum = x.Sum();
                var ySum = y.Sum();

                var xySum = 0d;
                for (int i = 0; i < N; i++)
                {
                    xySum += x[i] * y[i];
                }

                var a = ((N * xySum) - (xSum * ySum)) / ((N * x.Sum(v => Math.Pow(v, 2))) - Math.Pow(xSum, 2));

                var b = (ySum - (a * xSum)) / N;

                return (a, b);
            }
        }

        #endregion
    }
}
