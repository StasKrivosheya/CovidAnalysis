﻿using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CovidAnalysis.Extensions;
using CovidAnalysis.Models.CountryItem;
using CovidAnalysis.Models.LogEntryItem;
using CovidAnalysis.Services.CountryService;
using CovidAnalysis.Services.LogEntryService;
using CovidAnalysis.Services.StreamDownloader;
using CovidAnalysis.ViewModels.Tabs;
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

            MortalityChartTabViewModel = new MortalityChartTabViewModel(navigationService, logEntryService, countryService, pageDialogService);
            IncidenceChartTabViewModel = new IncidenceChartTabViewModel(navigationService, logEntryService, countryService, pageDialogService);
        }

        #region -- Public properties --

        private int _selectedViewModelIndex;
        public int SelectedViewModelIndex
        {
            get => _selectedViewModelIndex;
            set => SetProperty(ref _selectedViewModelIndex, value);
        }

        private bool _isDownloading;
        public bool IsDownloading
        {
            get => _isDownloading;
            set => SetProperty(ref _isDownloading, value);
        }

        private MortalityChartTabViewModel _mortalityChartTabViewModel;
        public MortalityChartTabViewModel MortalityChartTabViewModel
        {
            get => _mortalityChartTabViewModel;
            set => SetProperty(ref _mortalityChartTabViewModel, value);
        }

        private IncidenceChartTabViewModel _incidenceChartTabViewModel;
        public IncidenceChartTabViewModel IncidenceChartTabViewModel
        {
            get => _incidenceChartTabViewModel;
            set => SetProperty(ref _incidenceChartTabViewModel, value);
        }

        private ICommand _downloadCommand;
        public ICommand DownloadCommand => _downloadCommand ??= new DelegateCommand(async () => await OnDownloadCommandAsync());

        #endregion

        #region -- Overrides --

        public override void Initialize(INavigationParameters parameters)
        {
            base.Initialize(parameters);

            Title = "Home page";

            MortalityChartTabViewModel.Initialize(parameters);
            IncidenceChartTabViewModel.Initialize(parameters);
        }

        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            base.OnNavigatedTo(parameters);

            if (SelectedViewModelIndex is 0)
            {
                MortalityChartTabViewModel.OnNavigatedTo(parameters);
            }
            else if (SelectedViewModelIndex is 1)
            {
                IncidenceChartTabViewModel.OnNavigatedTo(parameters);
            }
        }

        #endregion

        #region -- Private helpers --

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

            if (MortalityChartTabViewModel.SelectedCountry is null)
            {
                MortalityChartTabViewModel.SelectedCountry = await _countryService.GetCountryAsync(c => c.IsoCode == Constants.DEFAULT_COUNTRY_ISO);
            }

            await MortalityChartTabViewModel.DispayMortalityAsync(MortalityChartTabViewModel.SelectedCountry);
        }

        #endregion
    }
}
