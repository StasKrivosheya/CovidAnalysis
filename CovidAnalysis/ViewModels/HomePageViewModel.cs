using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CovidAnalysis.Events;
using CovidAnalysis.Services.Downloader;
using Prism.Commands;
using Prism.Navigation;

namespace CovidAnalysis.ViewModels
{
    public class HomePageViewModel : BaseViewModel
    {
        private readonly IDownloader _downloader;

        private string _filepath;

        public HomePageViewModel(
            INavigationService navigationService,
            IDownloader downloader)
            : base(navigationService)
        {
            _downloader = downloader;
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

            _downloader.OnFileDownloaded += OnFileDownloaded;

            Title = "Home page";
        }

        public override void Destroy()
        {
            base.Destroy();

            _downloader.OnFileDownloaded -= OnFileDownloaded;
        }

        #endregion

        #region -- Private helpers --

        private Task OnDownloadCommandAsync()
        {
            IsDownloading = true;

            // TODO: request permissions
            _filepath = _downloader.DownloadFile("https://covid.ourworldindata.org/data/owid-covid-data.csv", "XF_Downloads");

            return Task.CompletedTask;
        }

        private void OnFileDownloaded(object sender, DownloadEventArgs e)
        {
            if (e.FileSaved)
            {
                var file = System.IO.File.ReadLines(_filepath).ToList();

                IsDownloading = false;
            }
            else
            {
                // TODO: notify via prism page dialog service
            }
        }

        #endregion
    }
}
