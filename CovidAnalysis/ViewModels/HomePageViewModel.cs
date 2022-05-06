using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CovidAnalysis.Extensions;
using CovidAnalysis.Models.LogEntryItem;
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
        private readonly IPageDialogService _pageDialogService;

        public HomePageViewModel(
            INavigationService navigationService,
            IStreamDownloader streamDownloader,
            ILogEntryService logEntryService,
            IPageDialogService pageDialogService)
            : base(navigationService)
        {
            _streamDownloader = streamDownloader;
            _logEntryService = logEntryService;
            _pageDialogService = pageDialogService;
        }

        #region -- Public properties --

        private bool _isDownloading;
        public bool IsDownloading
        {
            get => _isDownloading;
            set => SetProperty(ref _isDownloading, value);
        }

        private PlotModel _firstChartPlotModel;
        public PlotModel FirstChartPlotModel
        {
            get => _firstChartPlotModel;
            set => SetProperty(ref _firstChartPlotModel, value);
        }

        private ICommand _downloadCommand;
        public ICommand DownloadCommand => _downloadCommand ??= new DelegateCommand(async () => await OnDownloadCommandAsync());

        #endregion

        #region -- Overrides --

        public override async void Initialize(INavigationParameters parameters)
        {
            base.Initialize(parameters);

            Title = "Home page";

            await DispayMortalityAsync("UKR");
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
            // <ISO-code, FullName>
            var countries = new ConcurrentDictionary<string, string>();

            Parallel.ForEach(lines, l =>
            {
                var entry = l.ToLogEntryItemModel();
                countryEntries.Add(entry);
                countries.TryAdd(entry.IsoCode, entry.Country);
            });

            await _logEntryService.DeleteAllEnrtiesAsync().ConfigureAwait(false);
            var insertedCount = await _logEntryService.InsertEntriesAsync(countryEntries).ConfigureAwait(false);

            IsDownloading = false;

            var downloadReportMessage = $"You've successfully downloaded all newest data.\nThere are {insertedCount} models in your local repository now.";
            Device.BeginInvokeOnMainThread(async () =>
            {
                await _pageDialogService.DisplayAlertAsync("Success", downloadReportMessage, "Ok");
            });

            await DispayMortalityAsync("UKR");
        }

        private async Task DispayMortalityAsync(string countryIsoCode)
        {
            var entriesToDisplay = await _logEntryService.GetEntriesListAsync(e => e.IsoCode == countryIsoCode);

            if (entriesToDisplay is null
                || entriesToDisplay.Count is 0)
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    await _pageDialogService.DisplayAlertAsync("No data",
                        $"You either don't have any data yet, or ISO code {countryIsoCode} is wrong.\nTry getting last data.",
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

            LineSeries rawDataLineSeries = new()
            {
                MarkerFill = OxyColor.Parse("#0000FF"),
                Title = $"New death per million, {countryIsoCode}",
            };

            foreach (var entry in entriesToDisplay)
            {
                rawDataLineSeries.Points.Add(new DataPoint(entry.Date.ToOADate(), entry.NewDeathsPerMillion));
            }

            plotModel.Series.Add(rawDataLineSeries);

            LineSeries smoothedDataLineSeries = new()
            {
                MarkerFill = OxyColor.Parse("#FF0000"),
                Title = $"New death per million SMOOTHED, {countryIsoCode}",
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

            FirstChartPlotModel = plotModel;
        }

        #endregion
    }
}
