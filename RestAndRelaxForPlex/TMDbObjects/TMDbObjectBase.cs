using Newtonsoft.Json;

namespace JimBobBennett.RestAndRelaxForPlex.TmdbObjects
{
    public abstract class TmdbObjectBase
    {
        internal const int CurrentVersion = 1;

        protected TmdbObjectBase()
        {
            Version = CurrentVersion;
        }

        public int Version { get; private set; }

        [JsonProperty("id")]
        public string Id { get; set; }
    }
}