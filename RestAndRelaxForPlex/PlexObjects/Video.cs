using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using JimBobBennett.RestAndRelaxForPlex.Connection;
using JimBobBennett.JimLib.Collections;
using JimBobBennett.JimLib.Extensions;
using JimBobBennett.JimLib.Mvvm;
using JimBobBennett.JimLib.Xml;
using Xamarin.Forms;

namespace JimBobBennett.RestAndRelaxForPlex.PlexObjects
{
    public class Video : PlexObjectBase<Video>
    {
        public ExternalIds EpisodeExternalIds { get; private set; }
        public ExternalIds ExternalIds { get; private set; }

        public Video()
        {
            EpisodeExternalIds = new ExternalIds();
            ExternalIds = new ExternalIds();
            User = new User();
            Player = new Player();
            Roles = new ObservableCollectionEx<Role>();
            Genres = new ObservableCollectionEx<Genre>();
            Producers = new ObservableCollectionEx<Producer>();
            Writers = new ObservableCollectionEx<Writer>();
            Directors = new ObservableCollectionEx<Director>();
        }

        private Player _player;
        private string _guid;
        private double _viewOffset;
        private User _user;
        private ImageSource _thumbImageSource;
        private ImageSource _artImageSource;

        [NotifyPropertyChangeDependency("Key")]
        public string Guid
        {
            get { return _guid; }
            set
            {
                if (_guid == value) return;

                _guid = value;
                BuildIds();
            }
        }

        private void BuildIds()
        {
            if (Guid.StartsWith("com.plexapp.agents.imdb://", StringComparison.OrdinalIgnoreCase))
            {
                var id = Guid.Replace("com.plexapp.agents.imdb://", "");
                var end = id.IndexOf("?", StringComparison.Ordinal);
                if (end > 1)
                    id = id.Substring(0, end);

                ExternalIds.ImdbId = id;
            }
            else
                ExternalIds.ImdbId = null;

            if (Guid.StartsWith("com.plexapp.agents.thetvdb://"))
            {
                var bit = Guid.Replace("com.plexapp.agents.thetvdb://", "");
                var bits = bit.Split('/');

                ExternalIds.TvdbId = bits[0];
            }
            else
                ExternalIds.TvdbId = null;

            if (Guid.StartsWith("com.plexapp.agents.themoviedb://"))
            {
                var id = Guid.Replace("com.plexapp.agents.themoviedb://", "");
                var end = id.IndexOf("?", StringComparison.Ordinal);
                if (end > 1)
                    id = id.Substring(0, end);

                ExternalIds.TmdbId = id;
            }
            else
                ExternalIds.TmdbId = null;
        }

        public string Title { get; set; }
        public string Summary { get; set; }
        public string Tagline { get; set; }
        public string Art { get; set; }
        public string OriginallyAvailableAt { get; set; }

        [NotifyPropertyChangeDependency("VideoImageSource")]
        public string Thumb { get; set; }

        [NotifyPropertyChangeDependency("VideoImageSource")]
        public string ParentThumb { get; set; }

        [NotifyPropertyChangeDependency("VideoImageSource")]
        public string GrandParentThumb { get; set; }

        public string VideoImageSource
        {
            get
            {
                if (!ParentThumb.IsNullOrEmpty())
                    return ParentThumb;
                if (!GrandParentThumb.IsNullOrEmpty())
                    return GrandParentThumb;
                return Thumb;
            }
        }

        [NotifyPropertyChangeDependency("Progress")]
        public double ViewOffset
        {
            get { return _viewOffset; }
            set
            {
                if (Math.Abs(_viewOffset - value) < double.Epsilon) return;

                _viewOffset = value;
                RaisePropertyChanged();
            }
        }

        [NotifyPropertyChangeDependency("Progress")]
        public double Duration { get; set; }

        public double Progress
        {
            get { return Duration <= 0 ? 0 : ViewOffset/Duration; }
        }

        public string PlayerName
        {
            get { return Player.Title; }
        }

        public int Year { get; set; }

        public PlayerState PlayerState
        {
            get { return Player.State; }
            set
            {
                if (Player.State == value) return;

                Player.State = value;
                RaisePropertyChanged();
            }
        }

        public string UserThumb { get { return User.Thumb; } }
        public string UserName { get { return User.Title; } }

        [NotifyPropertyChangeDependency("UserName")]
        [NotifyPropertyChangeDependency("UserThumb")]
        public User User
        {
            get { return _user; }
            set
            {
                if (_user == value) return;

                if (_user != null)
                    _user.PropertyChanged -= UserOnPropertyChanged;

                _user = value;

                if (_user != null)
                    _user.PropertyChanged += UserOnPropertyChanged;
            }
        }

        private void UserOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyNameMatches(() => _user.Thumb))
                RaisePropertyChanged(() => UserThumb);

            if (e.PropertyNameMatches(() => _user.Title))
                RaisePropertyChanged(() => UserName);
        }

        [NotifyPropertyChangeDependency("PlayerName")]
        [NotifyPropertyChangeDependency("PlayerState")]
        public Player Player
        {
            get { return _player; }
            set
            {
                if (Equals(Player, value)) return;

                if (_player != null)
                    _player.PropertyChanged -= PlayerOnPropertyChanged;

                _player = value;

                if (_player != null)
                    _player.PropertyChanged += PlayerOnPropertyChanged;
            }
        }

        private void PlayerOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyNameMatches(() => _player.State))
                RaisePropertyChanged(() => PlayerState);

            if (e.PropertyNameMatches(() => _player.Title))
                RaisePropertyChanged(() => PlayerName);
        }

        public ObservableCollectionEx<Role> Roles { get; set; }
        public ObservableCollectionEx<Genre> Genres { get; set; }
        public ObservableCollectionEx<Producer> Producers { get; set; }
        public ObservableCollectionEx<Writer> Writers { get; set; }
        public ObservableCollectionEx<Director> Directors { get; set; }

        public string UriSource
        {
            get
            {
                if (!EpisodeExternalIds.ImdbId.IsNullOrEmpty())
                    return "IMDB";

                if (!ExternalIds.ImdbId.IsNullOrEmpty())
                    return "IMDB";

                if (!EpisodeExternalIds.TvdbId.IsNullOrEmpty())
                    return "TheTVDB";

                if (!ExternalIds.TmdbId.IsNullOrEmpty())
                    return "TMDb";
                
                if (!ExternalIds.TvdbId.IsNullOrEmpty())
                    return "TheTVDB";

                return null;
            }
        }

        public Uri Uri
        {
            get
            {
                if (!EpisodeExternalIds.ImdbId.IsNullOrEmpty())
                    return ImdbEpisodeUri;

                if (!ExternalIds.ImdbId.IsNullOrEmpty())
                    return ImdbUri;

                if (!EpisodeExternalIds.TvdbId.IsNullOrEmpty())
                    return TvdbEpisodeUri;

                if (!ExternalIds.TmdbId.IsNullOrEmpty())
                    return TmdbUri;
                
                if (!ExternalIds.TvdbId.IsNullOrEmpty())
                    return TvdbUri;
                
                return null;
            }
        }

        internal Uri TvdbUri
        {
            get { return new Uri(string.Format(PlexResources.TheTvdbSeriesUrl, ExternalIds.TvdbId)); }
        }

        internal Uri TmdbUri
        {
            get
            {
                switch (VideoType)
                {
                    case VideoType.Movie:
                        return new Uri(string.Format(PlexResources.TmdbMovieUrl, ExternalIds.TmdbId));
                    case VideoType.Episode:
                        return new Uri(string.Format(PlexResources.TmdbTvShowUrl, ExternalIds.TmdbId));
                }

                return null;
            }
        }

        internal Uri ImdbUri
        {
            get { return new Uri(string.Format(PlexResources.ImdbTitleUrl, ExternalIds.ImdbId)); }
        }

        internal Uri TvdbEpisodeUri
        {
            get { return new Uri(string.Format(PlexResources.TheTvdbEpisodeUrl, EpisodeExternalIds.TvdbId)); }
        }

        internal Uri TmdbEpisodeUri
        {
            get
            {
                return new Uri(string.Format(PlexResources.TmdbTvShowEpisodeUrl, ExternalIds.TmdbId,
                    SeasonNumber, EpisodeNumber));
            }
        }

        internal Uri ImdbEpisodeUri
        {
            get { return new Uri(string.Format(PlexResources.ImdbTitleUrl, EpisodeExternalIds.ImdbId)); }
        }

        public Uri SchemeUri
        {
            get {

                if (!EpisodeExternalIds.ImdbId.IsNullOrEmpty())
                    return new Uri(string.Format(PlexResources.ImdbTitleSchemeUrl, EpisodeExternalIds.ImdbId));

                if (!ExternalIds.ImdbId.IsNullOrEmpty())
                    return new Uri(string.Format(PlexResources.ImdbTitleSchemeUrl, ExternalIds.ImdbId)); 
                
                return null;
            }
        }

        [XmlNameMapping("grandparentTitle")]
        public string Show { get; set; }

        [XmlNameMapping("parentIndex")]
        public int SeasonNumber { get; set; }

        [XmlNameMapping("index")]
        public int EpisodeNumber { get; set; }

        public string Type { get; set; }

        public VideoType VideoType
        {
            get
            {
                if (string.Equals(Type, "Movie", StringComparison.OrdinalIgnoreCase))
                    return VideoType.Movie;

                if (string.Equals(Type, "Episode", StringComparison.OrdinalIgnoreCase))
                    return VideoType.Episode;

                return VideoType.Unknown;
            }
        }

        protected override bool OnUpdateFrom(Video newValue, List<string> updatedPropertyNames)
        {
            var isUpdated = UpdateValue(() => Title, newValue, updatedPropertyNames);
            isUpdated = UpdateValue(() => Summary, newValue, updatedPropertyNames) | isUpdated;
            isUpdated = UpdateValue(() => Guid, newValue, updatedPropertyNames) | isUpdated;
            isUpdated = UpdateValue(() => Art, newValue, updatedPropertyNames) | isUpdated;

            var thumbUpdated = UpdateValue(() => Thumb, newValue, updatedPropertyNames);
            thumbUpdated = UpdateValue(() => ParentThumb, newValue, updatedPropertyNames) | thumbUpdated;
            thumbUpdated = UpdateValue(() => GrandParentThumb, newValue, updatedPropertyNames) | thumbUpdated;

            if (thumbUpdated)
            {
                isUpdated = true;
                ThumbImageSource = null;
            }

            isUpdated = UpdateValue(() => ViewOffset, newValue, updatedPropertyNames) | isUpdated;
            isUpdated = UpdateValue(() => Duration, newValue, updatedPropertyNames) | isUpdated;
            isUpdated = UpdateValue(() => Show, newValue, updatedPropertyNames) | isUpdated;
            isUpdated = UpdateValue(() => SeasonNumber, newValue, updatedPropertyNames) | isUpdated;
            isUpdated = UpdateValue(() => EpisodeNumber, newValue, updatedPropertyNames) | isUpdated;
            isUpdated = UpdateValue(() => Type, newValue, updatedPropertyNames) | isUpdated;

            if (Player == null) Player = new Player();
            if (User == null) User = new User();
            if (Roles == null) Roles = new ObservableCollectionEx<Role>();
            if (Genres == null) Genres = new ObservableCollectionEx<Genre>();
            if (Producers == null) Producers = new ObservableCollectionEx<Producer>();
            if (Writers == null) Writers = new ObservableCollectionEx<Writer>();
            if (Directors == null) Directors = new ObservableCollectionEx<Director>();

            isUpdated = Player.UpdateFrom(newValue.Player) | isUpdated;
            isUpdated = User.UpdateFrom(newValue.User) | isUpdated;
            isUpdated = Roles.UpdateToMatch(newValue.Roles, r => r.Key, (r1, r2) => r1.UpdateFrom(r2)) | isUpdated;
            isUpdated = Genres.UpdateToMatch(newValue.Genres, r => r.Key, (r1, r2) => r1.UpdateFrom(r2)) | isUpdated;
            isUpdated = Producers.UpdateToMatch(newValue.Producers, r => r.Key, (r1, r2) => r1.UpdateFrom(r2)) | isUpdated;
            isUpdated = Writers.UpdateToMatch(newValue.Writers, r => r.Key, (r1, r2) => r1.UpdateFrom(r2)) | isUpdated;
            isUpdated = Directors.UpdateToMatch(newValue.Directors, r => r.Key, (r1, r2) => r1.UpdateFrom(r2)) | isUpdated;

            isUpdated = ExternalIds.UpdateFrom(newValue.ExternalIds) | isUpdated;
            isUpdated = EpisodeExternalIds.UpdateFrom(newValue.EpisodeExternalIds) | isUpdated;

            return isUpdated;
        }

        public override string Key
        {
            get { return Guid; }
        }

        public ImageSource ThumbImageSource
        {
            get { return _thumbImageSource; }
            internal set
            {
                if (_thumbImageSource == value) return;

                _thumbImageSource = value;
                RaisePropertyChanged();
            }
        }

        public ImageSource ArtImageSource
        {
            get { return _artImageSource; }
            internal set
            {
                if (_artImageSource == value) return;

                _artImageSource = value;
                RaisePropertyChanged();
            }
        }

        internal IPlexServerConnection PlexServerConnection { get; set; }

        public string ConnectionUri
        {
            get { return PlexServerConnection == null ? null : PlexServerConnection.ConnectionUri; }
        }

        public async Task<bool> PlayAsync()
        {
            if (PlexServerConnection != null)
                return await PlexServerConnection.PlayVideoAsync(this);

            return false;
        }

        public async Task<bool> PauseAsync()
        {
            if (PlexServerConnection != null)
                return await PlexServerConnection.PauseVideoAsync(this);

            return false;
        }

        public async Task<bool> StopAsync()
        {
            if (PlexServerConnection != null)
                return await PlexServerConnection.StopVideoAsync(this);

            return false;
        }

        internal bool HasBeenPopulatedFromTvdb { get; set; }
        internal bool HasBeenPopulatedFromTmdb { get; set; }

        internal bool RolesComeFromTvdb { get; set; }
        internal bool RolesComeFromTmdb { get; set; }
    }
}
