using System.Collections.Generic;
using Newtonsoft.Json;

namespace JimBobBennett.RestAndRelaxForPlex.TmdbObjects
{
    public class SearchResults
    {
        [JsonProperty("results")]
        public List<Result> Results { get; set; } 
    }
}
