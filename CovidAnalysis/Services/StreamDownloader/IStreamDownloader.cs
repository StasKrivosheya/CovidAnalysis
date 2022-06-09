using System.IO;
using System.Threading.Tasks;

namespace CovidAnalysis.Services.StreamDownloader
{
    public interface IStreamDownloader
    {
        public Task<Stream> DownloadStreamAsync(string url);
    }
}
