using System.Threading.Tasks;
using JimBobBennett.RestAndRelaxForPlex.TvdbObjects;

namespace JimBobBennett.RestAndRelaxForPlex.Caches
{
    public interface ITvdbCache
    {
        Task<Series> GetSeriesAsync(string tvdbId, int seriesNumber, int episodeNumber, bool forceRefresh);
        string DumpCacheAsJson();
        void LoadCacheFromJson(string json);
    }
}