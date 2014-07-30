using Newtonsoft.Json;

namespace JimBobBennett.RestAndRelaxForPlex.TmdbObjects
{
    public class Person : TmdbObjectWithImdbId
    {
        public Person()
        {
            Credits = new PersonCredits();
        }

        [JsonProperty("name")]
        public string Name { get; set; }
        
        [JsonProperty("biography")]
        public string Summary { get; set; }
        
        [JsonProperty("birthday")]
        public string BirthDay { get; set; }
        
        [JsonProperty("deathday")]
        public string DeathDay { get; set; }
        
        [JsonProperty("place_of_birth")]
        public string PlaceofBirth { get; set; }

        internal static Person ClonePerson(Person person)
        {
            return JsonConvert.DeserializeObject<Person>(JsonConvert.SerializeObject(person));
        }

        public PersonCredits Credits { get; set; }
    }
}
