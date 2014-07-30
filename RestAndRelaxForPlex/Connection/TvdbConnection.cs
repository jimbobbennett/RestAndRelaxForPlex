using System.Linq;
using System.Threading.Tasks;
using JimBobBennett.JimLib.Network;
using JimBobBennett.RestAndRelaxForPlex.TvdbObjects;

namespace JimBobBennett.RestAndRelaxForPlex.Connection
{
    public class TvdbConnection : ITvdbConnection
    {
        private readonly IRestConnection _restConnection;

        public TvdbConnection(IRestConnection restConnection)
        {
            _restConnection = restConnection;
        }

        public async Task<Series> GetSeriesAsync(string tvdbId)
        {
            var data = await _restConnection.MakeRequestAsync<Data, object>(Method.Get, ResponseType.Xml,
                PlexResources.TheTvdbBaseUrl, string.Format(PlexResources.TheTvdbSeries, tvdbId),
                timeout:30000);

            if (data == null || data.ResponseObject == null) return null;

            var series = data.ResponseObject.Series;
            series.Episodes = data.ResponseObject.Episodes;

            if (!await GetActorsForSeriesAsync(series)) return null;

            return series;
        }

        private async Task<bool> GetActorsForSeriesAsync(Series series)
        {
            var data = await _restConnection.MakeRequestAsync<SeriesActors, object>(Method.Get, ResponseType.Xml,
                PlexResources.TheTvdbBaseUrl, string.Format(PlexResources.TheTvdbSeriesActors, series.Id));

            if (data != null && data.ResponseObject != null)
            {
                var actors = data.ResponseObject;
                foreach (var actor in actors.Actors.Where(a => !a.Image.StartsWith(PlexResources.TheTvdbActorImageRoot)))
                    actor.Image = PlexResources.TheTvdbActorImageRoot + actor.Image;

                series.Actors = data.ResponseObject.Actors;

                return true;
            }

            return false;
        }
    }
}
