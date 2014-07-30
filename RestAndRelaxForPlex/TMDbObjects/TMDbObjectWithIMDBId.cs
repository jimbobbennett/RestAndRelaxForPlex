using Newtonsoft.Json;

namespace JimBobBennett.RestAndRelaxForPlex.TmdbObjects
{
    public abstract class TmdbObjectWithImdbId : TmdbObjectBase
    {
        [JsonProperty("imdb_id")]
        public string ImdbId { get; set; }
    }
}
