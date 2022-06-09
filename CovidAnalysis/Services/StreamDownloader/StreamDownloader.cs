using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace CovidAnalysis.Services.StreamDownloader
{
    public class StreamDownloader : IStreamDownloader
    {
        #region -- IStreamDownloader implementation --

        public Task<Stream> DownloadStreamAsync(string url)
        {
            var client = new HttpClient();

            return client.GetStreamAsync(url);
        }

        #endregion
    }
}
