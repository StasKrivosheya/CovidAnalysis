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

            // await ParallelCompareCountriesToUkraine();

            // TestOnSmallData();
        }

        #endregion

        #region -- Private helpers --

        // todo: optimize
        private async Task CompareCountriesToUkraine()
        {
            var countries = await _countryService.GetCountriesListAsync();

            var ukrNewCasesSmoothed = (await _logEntryService.GetEntriesListAsync(e => e.IsoCode == "UKR"))
                                .OrderBy(l => l.Date)
                                .Select(l => l.NewCasesOfSicknessPerMillion)
                                .GetSmoothed(7)
                                .ToArray();

            var countriesDTWComparison = new List<Tuple<CountryItemModel, double>>();

            foreach (var country in countries)
            {
                var countryNewCasesSmoothed = (await _logEntryService.GetEntriesListAsync(e => e.IsoCode == country.IsoCode))
                                .OrderBy(l => l.Date)
                                .Select(l => l.NewCasesOfSicknessPerMillion)
                                .GetSmoothed(7)
                                .ToArray();

                if (countryNewCasesSmoothed.Length >= MIN_AMOUNT_OF_ENTRIES)
                {
                    var dtwRes = MathHelper.CalculateDtw(ukrNewCasesSmoothed, countryNewCasesSmoothed);

                    countriesDTWComparison.Add(new Tuple<CountryItemModel, double>(country, dtwRes.Cost));
                }
            }

            ComparisonItems = new(countriesDTWComparison.OrderBy(t => t.Item2).ToList());
            // 77secs for 241elements - usual foreach
        }

        private void TestOnSmallData()
        {
            var x = new double[] { 0d, 2d, 0d, 1d, 0d, 0d };
            var y = new double[] { 0d, 0d, 0.5d, 2d, 0d, 1d, 0d };

            var dtwRes = MathHelper.CalculateDtw(x, y);
        }

        // gives different results from 40 to 100+ secs
        // todo: optimize
        private async Task ParallelCompareCountriesToUkraine()
        {
            var st = new Stopwatch(); st.Start();

            var countries = await _countryService.GetCountriesListAsync();

            var ukrNewCasesSmoothed = (await _logEntryService.GetEntriesListAsync(e => e.IsoCode == "UKR").ConfigureAwait(false))
                                .OrderBy(l => l.Date)
                                .Select(l => l.NewCasesOfSicknessPerMillion)
                                .GetSmoothed(7)
                                .ToArray();

            var countriesDTWComparison = new ConcurrentBag<Tuple<CountryItemModel, double>>();

            var calcTaks = countries.Select(async country =>
            {
                var countryNewCasesSmoothed = (await _logEntryService.GetEntriesListAsync(e => e.IsoCode == country.IsoCode).ConfigureAwait(false))
                                .OrderBy(l => l.Date)
                                .Select(l => l.NewCasesOfSicknessPerMillion)
                                .GetSmoothed(7)
                                .ToArray();

                if (countryNewCasesSmoothed.Length >= MIN_AMOUNT_OF_ENTRIES)
                {
                    var dtwRes = MathHelper.CalculateDtw(ukrNewCasesSmoothed, countryNewCasesSmoothed);

                    countriesDTWComparison.Add(new Tuple<CountryItemModel, double>(country, dtwRes.Cost));
                }
            });

            await Task.WhenAll(calcTaks).ConfigureAwait(false);

            ComparisonItems = new(countriesDTWComparison.OrderBy(t => t.Item2).ToList());

            st.Stop();
            var a = st.ElapsedMilliseconds / 1000;
            st.Reset();
        }

        #endregion
    }
}
