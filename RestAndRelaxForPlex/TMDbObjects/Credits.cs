using System.Collections.Generic;
using Newtonsoft.Json;

namespace JimBobBennett.RestAndRelaxForPlex.TmdbObjects
{
    public class Credits : TmdbObjectBase
    {
        [JsonProperty("cast")]
        public List<Cast> Cast { get; set; }

        [JsonProperty("guest_stars")]
        public List<Cast> GuestStars { get; set; } 
    }
}
