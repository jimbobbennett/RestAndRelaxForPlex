using Newtonsoft.Json;

namespace JimBobBennett.RestAndRelaxForPlex.TmdbObjects
{
    public class Result : TmdbObjectBase
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("release_date")]
        public string ReleaseDate { get; set; }

        [JsonProperty("first_air_date")]
        public string FirstAirDate { get; set; }
    }
}
