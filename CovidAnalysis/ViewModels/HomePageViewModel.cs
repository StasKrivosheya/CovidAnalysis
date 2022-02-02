using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Input;
using CovidAnalysis.Events;
using CovidAnalysis.Models.LogEntryItem;
using CovidAnalysis.Services.StreamDownloader;
using Prism.Commands;
using Prism.Navigation;

namespace CovidAnalysis.ViewModels
{
    public class HomePageViewModel : BaseViewModel
    {
        // tmp aka storage <ISO-code, entries>
        private Dictionary<string, List<LogEntryItemModel>> _logEntries = new Dictionary<string, List<LogEntryItemModel>>();
        //

        private readonly IStreamDownloader _streamDownloader;

        public HomePageViewModel(
            INavigationService navigationService,
            IStreamDownloader streamDownloader)
            : base(navigationService)
        {
            _streamDownloader = streamDownloader;
        }

        #region -- Public properties --

        private bool _isDownloading;
        public bool IsDownloading
        {
            get => _isDownloading;
            set => SetProperty(ref _isDownloading, value);
        }

        private ICommand _downloadCommand;
        public ICommand DownloadCommand => _downloadCommand ??= new DelegateCommand(async () => await OnDownloadCommandAsync());

        #endregion

        #region -- Overrides --

        public override void Initialize(INavigationParameters parameters)
        {
            base.Initialize(parameters);

            Title = "Home page";
        }

        #endregion

        #region -- Private helpers --

        private async Task OnDownloadCommandAsync()
        {
            IsDownloading = true;

            using var stream = await _streamDownloader.DownloadStreamAsync("https://covid.ourworldindata.org/data/owid-covid-data.csv");
            using var reader = new StreamReader(stream);

            // unnecessary for now, but in case we'll need to see the table header
            var headerLine = await reader.ReadLineAsync();

            var line = string.Empty;
            var countryEntries = new List<LogEntryItemModel>();

            while ((line = await reader.ReadLineAsync()) != null)
            {
                var values = line.Split(',');

                var yyyyMmDd = values[3].Split('-');
                var entryDate = new DateTime(year: int.Parse(yyyyMmDd[0]), month: int.Parse(yyyyMmDd[1]), day: int.Parse(yyyyMmDd[2]));

                if (!double.TryParse(values[4], NumberStyles.Any, CultureInfo.InvariantCulture, out var currentlySick))
                {
                    currentlySick = 0;
                }

                if (!double.TryParse(values[5], NumberStyles.Any, CultureInfo.InvariantCulture, out var newCasesOfSickness))
                {
                    newCasesOfSickness = 0;
                }

                if (!double.TryParse(values[7], NumberStyles.Any, CultureInfo.InvariantCulture, out var totalDeaths))
                {
                    totalDeaths = 0;
                }

                if (!double.TryParse(values[8], NumberStyles.Any, CultureInfo.InvariantCulture, out var newDeathsForToday))
                {
                    newDeathsForToday = 0;
                }

                if (!double.TryParse(values[12], NumberStyles.Any, CultureInfo.InvariantCulture, out var newCasesSmoothedPerMillion))
                {
                    newCasesSmoothedPerMillion = 0d;
                }

                if (!double.TryParse(values[15], NumberStyles.Any, CultureInfo.InvariantCulture, out var newDeathsSmoothedPerMillion))
                {
                    newDeathsSmoothedPerMillion = 0d;
                }

                var parsedEndtry = new LogEntryItemModel
                {
                    IsoCode = values[0],
                    Country = values[2],
                    Date = entryDate,
                    CurrentlySick = (int)currentlySick,
                    NewCasesOfSickness = (int)newCasesOfSickness,
                    NewCasesSmoothedPerMillion = newCasesSmoothedPerMillion,
                    TotalDeaths = (int)totalDeaths,
                    NewDeathsForToday = (int)newDeathsForToday,
                    NewDeathsSmoothedPerMillion = newDeathsSmoothedPerMillion,
                };

                if (countryEntries.LastOrDefault() is LogEntryItemModel lastModel
                    && lastModel.IsoCode != values[0])
                {
                    _logEntries.Add(lastModel.IsoCode, countryEntries);

                    countryEntries = new List<LogEntryItemModel>();
                }

                countryEntries.Add(parsedEndtry);
            }

            IsDownloading = false;
        }

        #endregion
    }
}
