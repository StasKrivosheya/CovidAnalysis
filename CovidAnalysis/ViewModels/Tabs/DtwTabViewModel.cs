using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CovidAnalysis.Extensions;
using CovidAnalysis.Helpers;
using CovidAnalysis.Models.CountryItem;
using CovidAnalysis.Services.CountryService;
using CovidAnalysis.Services.LogEntryService;
using Prism.Commands;
using Prism.Navigation;
using Xamarin.CommunityToolkit.UI.Views;

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

        private LayoutState _TabState = LayoutState.Empty;
        public LayoutState TabState
        {
            get => _TabState;
            set => SetProperty(ref _TabState, value);
        }

        private ICommand _calcRating;
        public ICommand CalculateRatingCommand => _calcRating ??= new DelegateCommand(async () => await OnCalculateRatingommandAsync());

        #endregion

        #region -- Overrides --

        public override void Initialize(INavigationParameters parameters)
        {
            base.Initialize(parameters);

            Title = "Countries New-Cases-Of-Sickness per mln smoothes Comparing to UKR";

            // TestOnSmallData();
        }

        #endregion

        #region -- Private helpers --

        private async Task OnCalculateRatingommandAsync()
        {
            TabState = LayoutState.Loading;

            await CompareCountriesToUkraineAsync();

            TabState = LayoutState.Success;
        }

        private async Task CompareCountriesToUkraineAsync()
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
