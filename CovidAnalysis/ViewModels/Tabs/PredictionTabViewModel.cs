using System;
using System.Collections.Generic;
using System.Linq;
using CovidAnalysis.Models.LogEntryItem;
using CovidAnalysis.Services.LogEntryService;
using Microsoft.ML;
using Microsoft.ML.Data;
using Prism.Navigation;

namespace CovidAnalysis.ViewModels.Tabs
{
    public class PredictionTabViewModel : BaseViewModel
    {
        private readonly ILogEntryService _logEntryService;

        public PredictionTabViewModel(
            INavigationService navigationService,
            ILogEntryService logEntryService)
            : base(navigationService)
        {
            _logEntryService = logEntryService;
        }

        #region -- Overrides --

        public override async void Initialize(INavigationParameters parameters)
        {
            base.Initialize(parameters);

            var ukrEntries = await _logEntryService.GetEntriesListAsync(e => e.IsoCode == "UKR");
            ukrEntries = ukrEntries.OrderBy(e => e.Date).ToList();

            var ukrEntriesCleared = ukrEntries.Where(e => e.Date < new DateTime(2022, 2, 24));

            TestMicrosoftML(ukrEntriesCleared);
        }

        #endregion

        #region -- Private helpers --

        private void TestMicrosoftML(IEnumerable<LogEntryItemModel> entries)
        {
            // error described
            // https://github.com/dotnet/machinelearning/issues/3764

            var mlContext = new MLContext();

            var featureDimension = entries.Count();

            var definedSchema = SchemaDefinition.Create(typeof(LogEntryItemModel));

            var dataView = mlContext.Data.LoadFromEnumerable(entries, definedSchema);

            IDataView firstYearData = mlContext.Data.FilterByCustomPredicate<LogEntryItemModel>(dataView, e => e.Date <= new DateTime(2021, 3, 13));
            IDataView secondYearData = mlContext.Data.FilterByCustomPredicate<LogEntryItemModel>(dataView, e => e.Date > new DateTime(2021, 3, 13));

            var forecastingPipeline = mlContext.Forecasting.ForecastBySsa(
                    outputColumnName: "ForecastedData",
                    inputColumnName: "TotalCasesOfSicknessPerMillion",
                    windowSize: 7,
                    seriesLength: 30,
                    trainSize: 365,
                    horizon: 7,
                    confidenceLevel: 0.95f,
                    confidenceLowerBoundColumn: "ForecastLowerBound",
                    confidenceUpperBoundColumn: "ForecastUpperBound");

            var forecaster = forecastingPipeline.Fit(firstYearData);
        }

        #endregion
    }
}
