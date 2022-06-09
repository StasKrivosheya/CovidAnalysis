using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
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

        #region -- Public properties --

        private int _horizon = 31;
        public int Horizon
        {
            get => _horizon;
            set => SetProperty(ref _horizon, value);
        }

        // for SES
        private double _alpha = 0.3;
        public double Alpha
        {
            get => _alpha;
            set => SetProperty(ref _alpha, value);
        }

        // for Holt's methos
        private double _beta = 0.2;
        public double Beta
        {
            get => _beta;
            set => SetProperty(ref _beta, value);
        }

        private double _gamma = 0.1;
        public double Gamma
        {
            get => _gamma;
            set => SetProperty(ref _gamma, value);
        }

        private int _season = 365;
        public int Season
        {
            get => _season;
            set => SetProperty(ref _season, value);
        }

        private PlotModel _forecastingPlot;
        public PlotModel ForecastingPlot
        {
            get => _forecastingPlot;
            set => SetProperty(ref _forecastingPlot, value);
        }

        #endregion

        #region -- Overrides --

        public override async void Initialize(INavigationParameters parameters)
        {
            base.Initialize(parameters);

            await ForecastAsync();
        }

        protected override async void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            base.OnPropertyChanged(args);

            if (args.PropertyName is nameof(Horizon)
                                    or nameof(Alpha)
                                    or nameof(Beta)
                                    or nameof(Gamma)
                                    or nameof(Season))
            {
                await ForecastAsync();
            }
        }

        #endregion

        #region -- Private helpers --

        private async Task ForecastAsync()
        {
            var atozmathDataSet = new List<double> { 5.2, 4.9, 5.5, 4.9, 5.2, 5.7, 5.4, 5.8, 5.9, 6, 5.2, 4.8 };

            var ukrNewCases = await _logEntryService.GetEntriesListAsync(e => e.IsoCode == "UKR");
            var warBegining = new DateTime(2022, 2, 24);
            ukrNewCases = ukrNewCases.OrderBy(e => e.Date).Where(e => e.Date < warBegining).ToList();

            var phi = 0.9; // for pumped Holt's method
            var rawData = ukrNewCases.Select(e => e.NewCasesOfSicknessPerMillion).ToList();

            var linRegrCoefficients = MathHelper.Forecasting.GetLinearRegressionCoeficients(rawData);
            var nextVal = MathHelper.Forecasting.SimpleExponentialSmoothing(rawData, Alpha);

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
            for (int i = 0; i < rawData.Count + Horizon; i++)
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
                Title = "Single exponenеial smoothing",
                StrokeThickness = 1.5,
            };

            var SESValues = MathHelper.Forecasting.SimpleExponentialSmoothing(rawData, Horizon, Alpha);
            for (int i = 0; i < SESValues.Count; i++)
            {
                SESpredictionLine.Points.Add(new DataPoint(i + 1, SESValues[i]));
            }
            plotModel.Series.Add(SESpredictionLine);

            // ---------- HOLT'S LINEAR TREND METHOD ----------

            LineSeries HLTpredictionLine = new()
            {
                Title = "Holt's linear trend (double exponential smoothing)",
                Color = OxyColor.FromRgb(120, 48, 191),
                StrokeThickness = 1.5,
            };
            var HLTValues = MathHelper.Forecasting.HoltsLinearTrendForecast(rawData, Horizon, Alpha, Beta);
            var startIndex = rawData.Count;
            var endIndex = startIndex + Horizon;
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
                Title = "Damped trend method (modified Holt's method)",
                Color = OxyColor.FromRgb(33, 60, 217),
                StrokeThickness = 1.5,
            };
            var DTRValues = MathHelper.Forecasting.DampedTrendForecast(rawData, Horizon, Alpha, Beta, phi);
            for (int t = startIndex; t < endIndex; t++)
            {
                var currentYValue = DTRValues[t - startIndex];

                if (currentYValue >= 0)
                {
                    DTRpredictionLine.Points.Add(new DataPoint(t + 1, currentYValue));
                }
            }
            plotModel.Series.Add(DTRpredictionLine);

            // ---------- HOLT-WINTER'S EXPONENTIAL SMOOTHING ----------

            LineSeries HWEpredictionLine = new()
            {
                Title = "Holt-Winters method (triple exponential smoothing)",
                Color = OxyColor.FromRgb(36, 191, 184),
                StrokeThickness = 1.5,
            };
            var HWEValues = MathHelper.Forecasting.HoltWintersExponentialSmoothing(rawData, Horizon, Season, Alpha, Beta, Gamma);
            for (int t = startIndex; t < endIndex; t++)
            {
                var currentYValue = HWEValues[t - startIndex];

                if (currentYValue >= 0)
                {
                    HWEpredictionLine.Points.Add(new DataPoint(t + 1, currentYValue));
                }
            }
            plotModel.Series.Add(HWEpredictionLine);

            // ---------- UPDATING FINAL PLOT MODEL ----------

            ForecastingPlot = plotModel;
        }

        #endregion
    }
}
