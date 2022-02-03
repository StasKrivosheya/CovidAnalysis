using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using CovidAnalysis.Extensions;
using CovidAnalysis.Models.LogEntryItem;
using CovidAnalysis.Services.LogEntryService;
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
        private readonly ILogEntryService _logEntryService;

        public HomePageViewModel(
            INavigationService navigationService,
            IStreamDownloader streamDownloader,
            ILogEntryService logEntryService)
            : base(navigationService)
        {
            _streamDownloader = streamDownloader;
            _logEntryService = logEntryService;
        }

        #region -- Public properties --

        private bool _isDownloading;
        public bool IsDownloading
        {
            get => _isDownloading;
            set => SetProperty(ref _isDownloading, value);
        }

        private string _downloadReportMessage;
        public string DownloadReportMessage
        {
            get => _downloadReportMessage;
            set => SetProperty(ref _downloadReportMessage, value);
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

            // TODO: remove following line
            var st = new System.Diagnostics.Stopwatch(); st.Start();

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
            Parallel.ForEach(lines, l =>
            {
                countryEntries.Add(l.ToLogEntryItemModel());
            });

            var res = await _logEntryService.InsertEntriesAsync(countryEntries).ConfigureAwait(false);

            // TODO: remove following line
            st.Stop();
            DownloadReportMessage = $"You've successfully downloaded, parsed and saved locally {res} models. " +
                $"It took only {(double)st.ElapsedMilliseconds / 1000} secs.";

            IsDownloading = false;
        }

        #endregion
    }
}
