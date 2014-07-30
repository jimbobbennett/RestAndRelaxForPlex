using System;
using JimBobBennett.JimLib.Extensions;
using Newtonsoft.Json;

namespace JimBobBennett.RestAndRelaxForPlex.TmdbObjects
{
    public class PersonCreditBase
    {
        [JsonProperty("title")]
        public string MovieTitle { get; set; }

        [JsonProperty("poster_path")]
        public string Thumb { get; set; }

        [JsonProperty("release_date")]
        public string ReleaseDateString { get; set; }

        [JsonProperty("first_air_date")]
        public string FirstAirDate { get; set; }

        [JsonProperty("name")]
        public string TvShowTitle { get; set; }

        public string Title { get { return MovieTitle.IsNullOrEmpty() ? TvShowTitle : MovieTitle; } }

        public DateTime ReleaseDate
        {
            get
            {
                var dateString = ReleaseDateString.IsNullOrEmpty() ? FirstAirDate : ReleaseDateString;

                if (!dateString.IsNullOrEmpty())
                {
                    DateTime date;
                    if (DateTime.TryParse(dateString, out date))
                        return date;
                }

                return DateTime.MaxValue;
            }
        }
    }
}