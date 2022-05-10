using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CovidAnalysis.Extensions;
using CovidAnalysis.Models.CountryItem;
using CovidAnalysis.Pages;
using CovidAnalysis.Services.CountryService;
using CovidAnalysis.Services.LogEntryService;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using Prism.Commands;
using Prism.Navigation;
using Prism.Services;
using Xamarin.Forms;

namespace CovidAnalysis.ViewModels.Tabs
{
    public class MortalityChartTabViewModel : BaseViewModel
    {
        private readonly ILogEntryService _logEntryService;
        private readonly ICountryService _countryService;
        private readonly IPageDialogService _pageDialogService;

        public MortalityChartTabViewModel(
            INavigationService navigationService,
            ILogEntryService logEntryService,
            ICountryService countryService,
            IPageDialogService pageDialogService)
            : base(navigationService)
        {
            _logEntryService = logEntryService;
            _countryService = countryService;
            _pageDialogService = pageDialogService;
        }

        #region -- Public properties --

        private bool _shouldShowRawData = true;
        public bool ShouldShowRawData
        {
            get => _shouldShowRawData;
            set => SetProperty(ref _shouldShowRawData, value);
        }

        private bool _shouldShowSmoothedData;
        public bool ShouldShowSmoothedData
        {
            get => _shouldShowSmoothedData;
            set => SetProperty(ref _shouldShowSmoothedData, value);
        }

        private PlotModel _mortalityChartPlotModel;
        public PlotModel MortalityChartPlotModel
        {
            get => _mortalityChartPlotModel;
            set => SetProperty(ref _mortalityChartPlotModel, value);
        }

        private CountryItemModel _selectedCountry;
        public CountryItemModel SelectedCountry
        {
            get => _selectedCountry;
            set => SetProperty(ref _selectedCountry, value);
        }

        private ICommand _selectCountryCommand;
        public ICommand SelectCountryCommand => _selectCountryCommand ??= new DelegateCommand(async () => await OnSelectCountryCommandAsync());

        #endregion

        #region -- Overrides --

        public override async void Initialize(INavigationParameters parameters)
        {
            base.Initialize(parameters);

            SelectedCountry = await _countryService.GetCountryAsync(c => c.IsoCode == Constants.DEFAULT_COUNTRY_ISO);
            await DispayMortalityAsync(SelectedCountry);
        }

        public override async void OnNavigatedTo(INavigationParameters parameters)
        {
            base.OnNavigatedTo(parameters);

            if (parameters.TryGetValue(Constants.Navigation.SELECTED_COUNTRY, out CountryItemModel country))
            {
                SelectedCountry = country;

                await DispayMortalityAsync(SelectedCountry);
            }
        }

        protected override async void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            base.OnPropertyChanged(args);

            if (args.PropertyName is nameof(SelectedCountry) or nameof(ShouldShowRawData) or nameof(ShouldShowSmoothedData))
            {
                await DispayMortalityAsync(SelectedCountry);
            }
        }

        #endregion

        #region -- Public helpers --

        public async Task DispayMortalityAsync(CountryItemModel country)
        {
            if (country is null) return;

            var entriesToDisplay = await _logEntryService.GetEntriesListAsync(e => e.IsoCode == country.IsoCode);

            if (entriesToDisplay is null
                || entriesToDisplay.Count is 0)
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    await _pageDialogService.DisplayAlertAsync("No data",
                        $"You either don't have any data yet, or ISO code {country.IsoCode} is wrong.\nTry getting last data.",
                        "Ok");
                });

                return;
            }

            entriesToDisplay = entriesToDisplay.OrderBy(e => e.Date).ToList();

            var plotModel = new PlotModel
            {
                Title = "New deaths attributed to COVID-19 per 1,000,000 people",
                LegendTitle = "Legend",
                LegendPosition = LegendPosition.LeftTop,
            };

            var xAxis = new LinearAxis
            {
                Title = "Date",
                Position = AxisPosition.Bottom,
                LabelFormatter = (param) => DateTime.FromOADate(param).ToString("MMM, yyyy")
            };
            plotModel.Axes.Add(xAxis);

            var yAxis = new LinearAxis
            {
                Title = "New deaths",
                Position = AxisPosition.Left
            };
            plotModel.Axes.Add(yAxis);

            if (ShouldShowRawData)
            {
                LineSeries rawDataLineSeries = new()
                {
                    Color = OxyColor.Parse("#5EA701"),
                    Title = $"New death per million, {country.IsoCode}",
                };

                foreach (var entry in entriesToDisplay)
                {
                    rawDataLineSeries.Points.Add(new DataPoint(entry.Date.ToOADate(), entry.NewDeathsPerMillion));
                }

                plotModel.Series.Add(rawDataLineSeries);
            }

            if (ShouldShowSmoothedData)
            {
                LineSeries smoothedDataLineSeries = new()
                {
                    Color = OxyColor.Parse("#D39D00"),
                    Title = $"New death per million SMOOTHED, {country.IsoCode}",
                };

                var newDeathsCasesSmoothed = entriesToDisplay.Select(u => u.NewDeathsPerMillion).GetSmoothed(7).ToList();

                for (int i = 0; i < entriesToDisplay.Count - 1; i++)
                {
                    entriesToDisplay[i].NewDeathsPerMillion = newDeathsCasesSmoothed[i];
                }

                foreach (var entry in entriesToDisplay.OrderBy(e => e.Date))
                {
                    smoothedDataLineSeries.Points.Add(new DataPoint(entry.Date.ToOADate(), entry.NewDeathsPerMillion));
                }

                plotModel.Series.Add(smoothedDataLineSeries);
            }

            MortalityChartPlotModel = plotModel;
        }

        #endregion

        #region -- Private helpers --

        private async Task OnSelectCountryCommandAsync()
        {
            var countries = await _countryService.GetCountriesListAsync();

            var parameters = new NavigationParameters
            {
                { Constants.Navigation.COLLECTION_FOR_SELECTION, countries },
            };

            await NavigationService.NavigateAsync(nameof(SelectOnePopupPage), parameters);
        }

        #endregion
    }
}
