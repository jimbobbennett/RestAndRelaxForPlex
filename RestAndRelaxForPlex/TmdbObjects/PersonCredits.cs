using System.Collections.Generic;
using Newtonsoft.Json;

namespace JimBobBennett.RestAndRelaxForPlex.TmdbObjects
{
    public class PersonCredits
    {
        public PersonCredits()
        {
            Cast = new List<PersonCastCredit>();
        }

        [JsonProperty("cast")]
        public List<PersonCastCredit> Cast { get; set; }

        [JsonProperty("crew")]
        public List<PersonCrewCredit> Crew { get; set; }
    }
}
