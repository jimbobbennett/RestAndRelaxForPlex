using JimBobBennett.RestAndRelaxForPlex.PlexObjects;
using Newtonsoft.Json;

namespace JimBobBennett.RestAndRelaxForPlex.TmdbObjects
{
    public class PersonCrewCredit : PersonCreditBase, ICastCrewCredit
    {
        [JsonProperty("job")]
        public string Job { get; set; }

        public string Role { get { return Job; } }
    }
}
