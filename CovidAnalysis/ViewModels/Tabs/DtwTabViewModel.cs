using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CovidAnalysis.Extensions;
using CovidAnalysis.Helpers;
using CovidAnalysis.Models.CountryItem;
using CovidAnalysis.Models.LogEntryItem;
using CovidAnalysis.Services.CountryService;
using CovidAnalysis.Services.LogEntryService;
using Prism.Navigation;

namespace CovidAnalysis.ViewModels.Tabs
{
    public class DtwTabViewModel : BaseViewModel
    {
        private const int MIN_AMOUNT_OF_ENTRIES = 100;

        private readonly ILogEntryService _logEntryService;
        private readonly ICountryService _countryService;

        public DtwTabViewModel(
            INavigationService navigationService,
            ILogEntryService logEntryService,
            ICountryService countryService)
            : base(navigationService)
        {
            _logEntryService = logEntryService;
            _countryService = countryService;
        }

        #region -- Public properties --

        private ObservableCollection<Tuple<CountryItemModel, double>> _comparisonItems;
        public ObservableCollection<Tuple<CountryItemModel, double>> ComparisonItems
        {
            get => _comparisonItems;
            set => SetProperty(ref _comparisonItems, value);
        }

        #endregion

        #region -- Overrides --

        public override async void Initialize(INavigationParameters parameters)
        {
            base.Initialize(parameters);

            Title = "Countries New-Cases-Of-Sickness per mln smoothes Comparing to UKR";

            await CompareCountriesToUkraine();

            // TestOnSmallData();
        }

        #endregion

        #region -- Private helpers --

        private async Task CompareCountriesToUkraine()
        {
            var countries = await _countryService.GetCountriesListAsync();

            if (countries.Count < 1)
            {
                return;
            }

            var ukrNewCasesSmoothed = (await _logEntryService.GetEntriesListAsync(e => e.IsoCode == "UKR"))
                                .OrderBy(l => l.Date)
                                .Select(l => l.NewCasesOfSicknessPerMillion)
                                .GetSmoothed(7)
                                .ToArray();

            var tasks = countries.Select(country => PrepareItemAsync(country));

            var countriesDTWComparison = (await Task.WhenAll(tasks)).Where(x => x.Item2.Length > MIN_AMOUNT_OF_ENTRIES);

            var otherTasks = countriesDTWComparison.Select(x => Task.Run(() => (x.Item1, MathHelper.CalculateDtw(ukrNewCasesSmoothed, x.Item2).Cost)));

            var result = await Task.WhenAll(otherTasks);

            ComparisonItems = new(result.OrderBy(t => t.Cost).Select(x => x.ToTuple()));
        }

        private async Task<(CountryItemModel, double[])> PrepareItemAsync(CountryItemModel country)
        {
            var countryNewCasesSmoothed = (await _logEntryService.GetEntriesListAsync(e => e.IsoCode == country.IsoCode))
                                .OrderBy(l => l.Date)
                                .Select(l => l.NewCasesOfSicknessPerMillion)
                                .GetSmoothed(7)
                                .ToArray();

            return (country, countryNewCasesSmoothed);
        }

        private void TestOnSmallData()
        {
            var x = new double[] { 0d, 2d, 0d, 1d, 0d, 0d };
            var y = new double[] { 0d, 0d, 0.5d, 2d, 0d, 1d, 0d };

            var dtwRes = MathHelper.CalculateDtw(x, y);
        }

        #endregion
    }
}
