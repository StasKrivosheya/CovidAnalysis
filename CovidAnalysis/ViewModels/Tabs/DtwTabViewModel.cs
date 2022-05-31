using System;
using System.Collections.ObjectModel;
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

        private ObservableCollection<Tuple<CountryItemModel, float>> _comparisonItems;
        public ObservableCollection<Tuple<CountryItemModel, float>> ComparisonItems
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

            // await CompareCountriesToUkraine(); todo: uncomment line

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

        private async Task<(CountryItemModel, float[])> PrepareItemAsync(CountryItemModel country)
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
            var x = new float[] { 0f, 2f, 0f, 1f, 0f, 0f };
            var y = new float[] { 0f, 0f, 0.5f, 2f, 0f, 1f, 0f };

            var dtwRes = MathHelper.CalculateDtw(x, y);
        }

        #endregion
    }
}
