using Newtonsoft.Json;

namespace JimBobBennett.RestAndRelaxForPlex.TmdbObjects
{
    public class TmdbExternalIds : TmdbObjectWithImdbId
    {
        [JsonProperty("tvdb_id")]
        public string TvdbId { get; set; }
    }
}
