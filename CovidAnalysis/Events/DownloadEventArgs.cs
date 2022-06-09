using System;
namespace CovidAnalysis.Events
{
    public class DownloadEventArgs : EventArgs
    {
        public DownloadEventArgs(bool fileSaved)
        {
            FileSaved = fileSaved;
        }

        public bool FileSaved = false;
    }
}
