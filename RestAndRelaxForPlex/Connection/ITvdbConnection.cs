using System.Threading.Tasks;
using JimBobBennett.RestAndRelaxForPlex.TvdbObjects;

namespace JimBobBennett.RestAndRelaxForPlex.Connection
{
    public interface ITvdbConnection
    {
        Task<Series> GetSeriesAsync(string tvdbId);
    }
}