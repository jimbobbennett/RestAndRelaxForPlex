using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using JimBobBennett.JimLib.Collections;
using JimBobBennett.JimLib.Extensions;
using JimBobBennett.RestAndRelaxForPlex.Connection;
using JimBobBennett.RestAndRelaxForPlex.TvdbObjects;
using Newtonsoft.Json;

namespace JimBobBennett.RestAndRelaxForPlex.Caches
{
    public class TvdbCache : ITvdbCache
    {
        private readonly ITvdbConnection _tvdbConnection;

        private readonly CacheWithFail<string, Series> _seriesCache = new CacheWithFail<string, Series>();
 
        public TvdbCache(ITvdbConnection tvdbConnection)
        {
            _tvdbConnection = tvdbConnection;
        }

        public async Task<Series> GetSeriesAsync(string tvdbId, int seriesNumber, int episodeNumber, bool forceRefresh)
        {
            var foundSeries = await _seriesCache.GetOrAddAsync(tvdbId, async s =>
                {
                    var series = await _tvdbConnection.GetSeriesAsync(s);
                    return (series == null || series.GetEpisode(seriesNumber, episodeNumber) == null) ? null : series;
                }, forceRefresh);

            if (foundSeries == null || foundSeries.GetEpisode(seriesNumber, episodeNumber) == null)
            {
                _seriesCache.MarkAsFailed(tvdbId);
                return null;
            }

            return foundSeries;
        }

        public string DumpCacheAsJson()
        {
            var caches = new Dictionary<string, string>
            {
                {"_seriesCache", _seriesCache.DumpCacheAsJson()}
            };

            return JsonConvert.SerializeObject(caches);
        }

        public void LoadCacheFromJson(string json)
        {
            if (json.IsNullOrEmpty()) return;

            try
            {
                var caches = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

                string output;
                if (caches.TryGetValue("_seriesCache", out output))
                    _seriesCache.LoadCacheFromJson(output);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to deserialize caches: " + ex.Message);

                _seriesCache.Clear();
            }
        }
    }
}
