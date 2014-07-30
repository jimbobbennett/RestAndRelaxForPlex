using JimBobBennett.JimLib.Xml;

namespace JimBobBennett.RestAndRelaxForPlex.TvdbObjects
{
    public abstract class TvdbObjectBase
    {
        internal const int CurrentVersion = 2;

        protected TvdbObjectBase()
        {
            Version = CurrentVersion;
        }

        public int Version { get; private set; }

        public string Id { get; set; }

        [XmlNameMapping("IMDB_ID")]
        public string ImdbId { get; set; }
    }
}