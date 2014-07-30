namespace JimBobBennett.RestAndRelaxForPlex.TvdbObjects
{
    public class Episode : TvdbObjectBase
    {
        public string EpisodeName { get; set; }
        public int EpisodeNumber { get; set; }
        public int SeasonNumber { get; set; }
    }
}
