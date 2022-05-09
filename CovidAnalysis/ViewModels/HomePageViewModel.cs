using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CovidAnalysis.Extensions;
using CovidAnalysis.Models.CountryItem;
using CovidAnalysis.Models.LogEntryItem;
using CovidAnalysis.Pages;
using CovidAnalysis.Services.CountryService;
using CovidAnalysis.Services.LogEntryService;
using CovidAnalysis.Services.StreamDownloader;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using Prism.Commands;
using Prism.Navigation;
using Prism.Services;
using Xamarin.Forms;

namespace CovidAnalysis.ViewModels
{
    public class HomePageViewModel : BaseViewModel
    {
        private readonly IStreamDownloader _streamDownloader;
        private readonly ILogEntryService _logEntryService;
        private readonly ICountryService _countryService;
        private readonly IPageDialogService _pageDialogService;

        private CountryItemModel _selectedCountry;

        public HomePageViewModel(
            INavigationService navigationService,
            IStreamDownloader streamDownloader,
            ILogEntryService logEntryService,
            ICountryService countryService,
            IPageDialogService pageDialogService)
            : base(navigationService)
        {
            _streamDownloader = streamDownloader;
            _logEntryService = logEntryService;
            _countryService = countryService;
            _pageDialogService = pageDialogService;
        }

        #region -- Public properties --

        private bool _isDownloading;
        public bool IsDownloading
        {
            get => _isDownloading;
            set => SetProperty(ref _isDownloading, value);
        }

        private PlotModel _mortalityChartPlotModel;
        public PlotModel MortalityChartPlotModel
        {
            get => _mortalityChartPlotModel;
            set => SetProperty(ref _mortalityChartPlotModel, value);
        }

        private string _countryPickerText = "Choose country ▼";
        public string CountryPickerText
        {
            get => _countryPickerText;
            set => SetProperty(ref _countryPickerText, value);
        }

        private bool _shouldShowRawData = true;
        public bool ShouldShowRawData
        {
            get => _shouldShowRawData;
            set => SetProperty(ref _shouldShowRawData, value);
        }

        private bool _shouldShowSmoothedData = false;
        public bool ShouldShowSmoothedData
        {
            get => _shouldShowSmoothedData;
            set => SetProperty(ref _shouldShowSmoothedData, value);
        }

        private ICommand _downloadCommand;
        public ICommand DownloadCommand => _downloadCommand ??= new DelegateCommand(async () => await OnDownloadCommandAsync());

        private ICommand _selectCountryCommand;
        public ICommand SelectCountryCommand => _selectCountryCommand ??= new DelegateCommand(async () => await OnSelectCountryCommandAsync());

        #endregion

        #region -- Overrides --

        public override async void Initialize(INavigationParameters parameters)
        {
            base.Initialize(parameters);

            Title = "Home page";

            _selectedCountry = await _countryService.GetCountryAsync(c => c.IsoCode == Constants.DEFAULT_COUNTRY_ISO);
            await DispayMortalityAsync(_selectedCountry, ShouldShowRawData, ShouldShowSmoothedData);
        }

        public override async void OnNavigatedTo(INavigationParameters parameters)
        {
            base.OnNavigatedTo(parameters);

            if (parameters.TryGetValue(Constants.Navigation.SELECTED_COUNTRY, out CountryItemModel country))
            {
                CountryPickerText = country.CountryName;

                await DispayMortalityAsync(country, ShouldShowRawData, ShouldShowSmoothedData);
            }
        }

        protected override async void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            base.OnPropertyChanged(args);

            if (args.PropertyName is nameof(ShouldShowRawData) or nameof(ShouldShowSmoothedData))
            {
                await DispayMortalityAsync(_selectedCountry, ShouldShowRawData, ShouldShowSmoothedData);
            }
        }

        #endregion

        #region -- Private helpers --

        private async Task OnSelectCountryCommandAsync()
        {
            var countries = await _countryService.GetCountriesListAsync();

            var parameters = new NavigationParameters
            {
                {Constants.Navigation.COLLECTION_FOR_SELECTION, countries },
            };

            await NavigationService.NavigateAsync(nameof(SelectOnePopupPage), parameters);
        }

        private async Task OnDownloadCommandAsync()
        {
            IsDownloading = true;

            using var stream = await _streamDownloader.DownloadStreamAsync(Constants.CSV_DATA_SOURCE_LINK).ConfigureAwait(false);
            using var reader = new StreamReader(stream);

            // unnecessary for now, but in case we'll need to see the table header
            var headerLine = await reader.ReadLineAsync().ConfigureAwait(false);

            var line = string.Empty;
            var lines = new List<string>();

            while ((line = await reader.ReadLineAsync().ConfigureAwait(false)) != null)
            {
                lines.Add(line);
            }
            
            var countryEntries = new ConcurrentBag<LogEntryItemModel>();
            var newCountries = new ConcurrentDictionary<string, CountryItemModel>();

            Parallel.ForEach(lines, l =>
            {
                var entry = l.ToLogEntryItemModel();
                countryEntries.Add(entry);
                newCountries.TryAdd(
                    entry.IsoCode,
                    new CountryItemModel
                    {
                        IsoCode = entry.IsoCode,
                        CountryName = entry.Country,
                    });
            });

            await _logEntryService.DeleteAllEnrtiesAsync().ConfigureAwait(false);
            var insertedCount = await _logEntryService.InsertEntriesAsync(countryEntries).ConfigureAwait(false);

            var oldCountries = await _countryService.GetCountriesListAsync();

            if (oldCountries is null
                || oldCountries.Count is 0)
            {
                await _countryService.InsertEntriesAsync(newCountries.Select(c => c.Value)).ConfigureAwait(false);
            }
            else if (oldCountries.Count != newCountries.Count)
            {
                await _countryService.DeleteAllCountriesAsync().ConfigureAwait(false);
                await _countryService.InsertEntriesAsync(newCountries.Select(c => c.Value)).ConfigureAwait(false);
            }

            IsDownloading = false;

            var downloadReportMessage = $"You've successfully downloaded all newest data.\nThere are {insertedCount} models in your local repository now.";
            Device.BeginInvokeOnMainThread(async () =>
            {
                await _pageDialogService.DisplayAlertAsync("Success", downloadReportMessage, "Ok");
            });

            _selectedCountry = await _countryService.GetCountryAsync(c => c.IsoCode == Constants.DEFAULT_COUNTRY_ISO);
            await DispayMortalityAsync(_selectedCountry, ShouldShowRawData, ShouldShowSmoothedData);
        }

        private async Task DispayMortalityAsync(CountryItemModel country, bool shouldShowRawData, bool shouldShowSmoothedData)
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

            if (shouldShowRawData)
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

            if (shouldShowSmoothedData)
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
    }
}
