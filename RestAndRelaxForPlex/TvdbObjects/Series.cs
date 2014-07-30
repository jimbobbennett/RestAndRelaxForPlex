using System.Collections.Generic;
using Newtonsoft.Json;

namespace JimBobBennett.RestAndRelaxForPlex.TvdbObjects
{
    public class Series : TvdbObjectBase
    {
        public string SeriesName { get; set; }

        public List<Actor> Actors { get; internal set; }

        private Dictionary<int, Dictionary<int, Episode>> _episodesBySeasonAndEpisode = new Dictionary<int, Dictionary<int, Episode>>();
        private List<Episode> _episodes;

        public List<Episode> Episodes
        {
            get { return _episodes; }
            set
            {
                if (_episodes == value) return;
                _episodes = value;

                var dict = new Dictionary<int, Dictionary<int, Episode>>();

                foreach (var episode in _episodes)
                {
                    Dictionary<int, Episode> episodesBySeason;

                    if (!dict.TryGetValue(episode.SeasonNumber, out episodesBySeason))
                    {
                        episodesBySeason = new Dictionary<int, Episode>();
                        dict.Add(episode.SeasonNumber, episodesBySeason);
                    }

                    episodesBySeason[episode.EpisodeNumber] = episode;
                }

                _episodesBySeasonAndEpisode = dict;
            }
        }

        public Episode GetEpisode(int seasonNumber, int episodeNumber)
        {
            Dictionary<int, Episode> episodesBySeason;

            if (!_episodesBySeasonAndEpisode.TryGetValue(seasonNumber, out episodesBySeason))
                return null;

            Episode episode;
            episodesBySeason.TryGetValue(episodeNumber, out episode);
            return episode;
        }

        internal static Series CloneSeries(Series series)
        {
            return JsonConvert.DeserializeObject<Series>(JsonConvert.SerializeObject(series));
        }
    }
}
