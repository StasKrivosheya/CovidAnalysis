using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CovidAnalysis.Extensions;
using CovidAnalysis.Helpers;
using CovidAnalysis.Services.LogEntryService;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using Prism.Navigation;

namespace CovidAnalysis.ViewModels
{
    public class ForecastingPageViewModel : BaseViewModel
    {
        private readonly ILogEntryService _logEntryService;

        public ForecastingPageViewModel(
            INavigationService navigationService,
            ILogEntryService logEntryService)
            : base(navigationService)
        {
            _logEntryService = logEntryService;
        }

        private PlotModel _forecastingPlot;
        public PlotModel ForecastingPlot
        {
            get => _forecastingPlot;
            set => SetProperty(ref _forecastingPlot, value);
        }

        public override async void Initialize(INavigationParameters parameters)
        {
            base.Initialize(parameters);

            await Forecast();
        }

        private async Task Forecast()
        {
            var atozmathDataSet = new List<double> { 5.2, 4.9, 5.5, 4.9, 5.2, 5.7, 5.4, 5.8, 5.9, 6, 5.2, 4.8 };
            var dummyMood = new List<double> { 2.1, 1.5, 2, 3, 0.5, 0 };
            var ukrNewCases = await _logEntryService.GetEntriesListAsync(e => e.IsoCode == "UKR");
            var warBegining = new DateTime(2022, 2, 24);
            ukrNewCases = ukrNewCases.OrderBy(e => e.Date).Where(e => e.Date < warBegining).ToList();


            var alpha = 0.3; // for SES
            var beta = 0.2; // for Holt's methos
            var phi = 0.9; // for pumped Holt's method
            var h = 31;
            var rawData = ukrNewCases.Select(e => e.NewCasesOfSicknessPerMillion).ToList();
            //var rawData = atozmathDataSet;

            var linRegrCoefficients = MathHelper.Forecasting.GetLinearRegressionCoeficients(rawData);
            var nextVal = MathHelper.Forecasting.SimpleExponentialSmoothing(rawData, alpha);

            // ---------- CREATING THE PLOT ----------

            var plotModel = new PlotModel
            {
                Title = "Forecasting",
                LegendTitle = "Legend",
                LegendPosition = LegendPosition.LeftTop,
            };

            var xAxis = new LinearAxis
            {
                Title = "Time",
                Position = AxisPosition.Bottom,
            };
            plotModel.Axes.Add(xAxis);

            var yAxis = new LinearAxis
            {
                Title = "Value",
                Position = AxisPosition.Left
            };
            plotModel.Axes.Add(yAxis);

            // ---------- DISPLAYING RAW TIME SERIES ----------

            LineSeries rawDataLine = new()
            {
                Title = "Raw data line",
                StrokeThickness = 2,
            };
            for (int i = 0; i < rawData.Count; i++)
            {
                rawDataLine.Points.Add(new DataPoint(i + 1, rawData[i]));
            }
            plotModel.Series.Add(rawDataLine);

            var signB = linRegrCoefficients.Item2 > 0 ? "+" : "-";
            LineSeries linearRegression = new()
            {
                Title = "Linear regression line: y = " + linRegrCoefficients.Item1.ToString("F5")
                + $"x {signB} " + Math.Abs(linRegrCoefficients.Item2).ToString("F5"),
                StrokeThickness = 1,
            };
            for (int i = 0; i < rawData.Count + h; i++)
            {
                var linRegrValue = linRegrCoefficients.Item1 * i + linRegrCoefficients.Item2;

                if (linRegrValue >= 0)
                {
                    linearRegression.Points.Add(new DataPoint(i, linRegrValue));
                }
            }
            plotModel.Series.Add(linearRegression);

            // ---------- SIMPLE EXPONENSIAL SMOOTHING ----------

            LineSeries SESpredictionLine = new()
            {
                Title = "Simple Exponensial Smoothing",
                StrokeThickness = 1.5,
            };

            //var expSmoothedValues = new List<double>();
            //for (int i = 1; i < rawData.Count; i++)
            //{
            //    var newValue = MathHelper.Forecasting.SimpleExponentialSmoothing(rawData.GetRange(0, i), alpha);
            //    expSmoothedValues.Add(newValue);
            //    predictionLine.Points.Add(new DataPoint(i, newValue));
            //}
            //var originalSize = expSmoothedValues.Count;
            //for (int i = originalSize; i < originalSize + h; i++)
            //{
            //    var newValue = MathHelper.Forecasting.SimpleExponentialSmoothing(expSmoothedValues, alpha);
            //    expSmoothedValues.Add(newValue);
            //    predictionLine.Points.Add(new DataPoint(i, newValue));
            //}

            var SESValues = MathHelper.Forecasting.SimpleExponentialSmoothing(rawData, h, alpha);
            for (int i = 0; i < SESValues.Count; i++)
            {
                SESpredictionLine.Points.Add(new DataPoint(i + 1, SESValues[i]));
            }
            plotModel.Series.Add(SESpredictionLine);

            // ---------- HOLT'S LINEAR TREND METHOD ----------

            LineSeries HLTpredictionLine = new()
            {
                Title = "Holt's linear trend",
                StrokeThickness = 1.5,
            };
            var HLTValues = MathHelper.Forecasting.HoltsLinearTrendForecast(rawData, h, alpha, beta);
            var startIndex = rawData.Count;
            var endIndex = startIndex + h;
            for (int t = startIndex; t < endIndex; t++)
            {
                var currentYValue = HLTValues[t - startIndex];

                if (currentYValue >= 0)
                {
                    HLTpredictionLine.Points.Add(new DataPoint(t + 1, currentYValue));
                }
            }
            plotModel.Series.Add(HLTpredictionLine);

            // ---------- DAMPED TREND METHOD ----------

            LineSeries DTRpredictionLine = new()
            {
                Title = "Damped trend method",
                StrokeThickness = 1.5,
            };
            var DTRValues = MathHelper.Forecasting.DampedTrendForecast(rawData, h, alpha, beta, phi);
            for (int t = startIndex; t < endIndex; t++)
            {
                var currentYValue = DTRValues[t - startIndex];

                if (currentYValue >= 0)
                {
                    DTRpredictionLine.Points.Add(new DataPoint(t + 1, currentYValue));
                }
            }
            plotModel.Series.Add(DTRpredictionLine);

            // ---------- UPDATING FINAL PLOT MODEL ----------

            ForecastingPlot = plotModel;
        }
    }
}
