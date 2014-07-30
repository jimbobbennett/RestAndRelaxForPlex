using JimBobBennett.RestAndRelaxForPlex.PlexObjects;
using Newtonsoft.Json;

namespace JimBobBennett.RestAndRelaxForPlex.TmdbObjects
{
    public class PersonCastCredit : PersonCreditBase, ICastCrewCredit
    {
        [JsonProperty("character")]
        public string Character { get; set; }

        public string Role { get { return Character; } }
    }
}
