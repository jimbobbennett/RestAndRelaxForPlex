using System.Threading.Tasks;
using JimBobBennett.RestAndRelaxForPlex.PlexObjects;
using JimBobBennett.RestAndRelaxForPlex.TmdbObjects;

namespace JimBobBennett.RestAndRelaxForPlex.Connection
{
    public interface ITmdbConnection
    {
        Task<Movie> GetMovieAsync(string tmdbId);
        Task<TvShow> GetTvShowAsync(string tmdbId, int seasonNumber, int episodeNumber);

        Task<Movie> SearchForMovieAsync(string title, int year, ExternalIds knownIds);

        Task<TvShow> SearchForTvShowAsync(string title, ExternalIds knownSeriesIds,
            int seasonNumber, int episodeNumber);

        Task<Person> GetPersonAsync(string tmdbId);
    }
}