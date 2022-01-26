using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using CovidAnalysis.Events;
using CovidAnalysis.iOS.Services;
using CovidAnalysis.Services.Downloader;
using Xamarin.Forms;

[assembly: Dependency(typeof(iOSDownloader))]
namespace CovidAnalysis.iOS.Services
{
    public class iOSDownloader : IDownloader
    {
        public event EventHandler<DownloadEventArgs> OnFileDownloaded;

        public string DownloadFile(string url, string folder)
        {
            string pathToNewFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), folder);
            Directory.CreateDirectory(pathToNewFolder);
            var pathToNewFile = string.Empty;

            try
            {
                WebClient webClient = new WebClient();
                webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(Completed);
                pathToNewFile = Path.Combine(pathToNewFolder, Path.GetFileName(url));
                webClient.DownloadFileAsync(new Uri(url), pathToNewFile);
            }
            catch (Exception ex)
            {
                if (OnFileDownloaded != null)
                    OnFileDownloaded.Invoke(this, new DownloadEventArgs(false));
            }

            return pathToNewFile;
        }

        private void Completed(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                if (OnFileDownloaded != null)
                    OnFileDownloaded.Invoke(this, new DownloadEventArgs(false));
            }
            else
            {
                if (OnFileDownloaded != null)
                    OnFileDownloaded.Invoke(this, new DownloadEventArgs(true));
            }
        }
    }
}
