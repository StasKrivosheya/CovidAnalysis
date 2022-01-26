using System;
using CovidAnalysis.Events;

namespace CovidAnalysis.Services.Downloader
{
    public interface IDownloader
    {
        string DownloadFile(string url, string folder);
        event EventHandler<DownloadEventArgs> OnFileDownloaded;
    }
}
