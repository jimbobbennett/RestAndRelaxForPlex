using Newtonsoft.Json;

namespace JimBobBennett.RestAndRelaxForPlex.TmdbObjects
{
    public class TvShow : TmdbObjectBase
    {
        public Credits Credits { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("first_air_date")]
        public string FirstAirDate { get; set; }

        [JsonProperty("air_date")]
        public string AirDate { get; set; }

        [JsonProperty("overview")]
        public string Summary { get; set; }

        public TmdbExternalIds ExternalExternalIds { get; set; }
        public TmdbExternalIds EpisodeExternalIds { get; set; }

        internal static TvShow CloneTvShow(TvShow tvShow)
        {
            return JsonConvert.DeserializeObject<TvShow>(JsonConvert.SerializeObject(tvShow));
        }
    }
}
