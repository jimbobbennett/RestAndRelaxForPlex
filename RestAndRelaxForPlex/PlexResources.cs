namespace JimBobBennett.RestAndRelaxForPlex
{
    internal static class PlexResources
    {
        public const string MyPlexSignIn = "users/sign_in.xml";
        public const string MyPlexDevices = "devices.xml";

        public const string MyPlexBaseUrl = "https://plex.tv";

        public const string ServerSessions = "status/sessions";
        public const string ServerClients = "clients";

        public const string ClientPause = "player/playback/pause";
        public const string ClientPlay = "player/playback/play";
        public const string ClientStop = "player/playback/stop";

        public const string TheTvdbBaseUrl = "http://thetvdb.com";
        public const string TheTvdbSeries = "data/series/{0}/all";
        public const string TheTvdbSeriesActors = "data/series/{0}/actors.xml";
        public const string TheTvdbActorImageRoot = "http://thetvdb.com/banners/";
        public const string TheTvdbSeriesUrl = "http://www.thetvdb.com/?tab=series&id={0}";
        public const string TheTvdbEpisodeUrl = "http://thetvdb.com/?tab=episode&id={0}";

        public const string TmdbBaseUrl = "https://api.themoviedb.org";
        public const string TmdbActorImageRoot = "http://image.tmdb.org/t/p/original";
        public const string TmdbMovie = "3/movie/{0}?api_key={1}";
        public const string TmdbPerson = "3/person/{0}?api_key={1}";
        public const string TmdbPersonCredits = "3/person/{0}/combined_credits?api_key={1}";
        public const string TmdbPersonUrl = "https://www.themoviedb.org/person/{0}";
        public const string TmdbCredits = "3/movie/{0}/credits?api_key={1}";
        public const string TmdbSearchMovie = "/3/search/movie?query={0}&include_adult=true&year={1}&api_key={2}";
        public const string TmdbMovieUrl = "https://www.themoviedb.org/movie/{0}";
        public const string TmdbTvShow = "3/tv/{0}?api_key={1}";
        public const string TmdbTvShowCredits = "3/tv/{0}/season/{1}/episode/{2}/credits?api_key={3}";
        public const string TmdbTvShowExternalIds = "3/tv/{0}/season/{1}/episode/{2}/external_ids?api_key={3}";
        public const string TmdbTvShowSeriesExternalIds = "3/tv/{0}/external_ids?api_key={1}";
        public const string TmdbSearchTvShow = "/3/search/tv?query={0}&api_key={1}";
        public const string TmdbTvShowUrl = "https://www.themoviedb.org/tv/{0}";
        public const string TmdbTvShowEpisodeUrl = "https://www.themoviedb.org/tv/{0}/season/{1}/episode/{2}";
        
        public const string ImdbNameUrl = "http://www.imdb.com/name/{0}/";
        public const string ImdbNameSchemeUrl = "imdb:///name/{0}/";
        public const string ImdbTitleUrl = "http://www.imdb.com/title/{0}";
        public const string ImdbTitleSchemeUrl = "imdb:///title/{0}/";
    }
}
