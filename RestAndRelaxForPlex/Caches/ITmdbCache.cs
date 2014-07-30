using System.Threading.Tasks;
using JimBobBennett.RestAndRelaxForPlex.PlexObjects;
using JimBobBennett.RestAndRelaxForPlex.TmdbObjects;

namespace JimBobBennett.RestAndRelaxForPlex.Caches
{
    public interface ITmdbCache
    {
        Task<Movie> GetMovieAsync(Video video, bool forceRefresh);
        Task<TvShow> GetTvShowAsync(Video video, bool forceRefresh);
        Task<Person> GetPersonAsync(string tmdbId, bool forceRefresh);
        string DumpCacheAsJson();
        void LoadCacheFromJson(string json);
    }
}