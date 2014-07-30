using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using JimBobBennett.RestAndRelaxForPlex.Caches;
using JimBobBennett.RestAndRelaxForPlex.PlexObjects;
using JimBobBennett.JimLib.Collections;
using JimBobBennett.JimLib.Events;
using JimBobBennett.JimLib.Extensions;
using JimBobBennett.JimLib.Network;
using JimBobBennett.JimLib.Xamarin.Network;
using JimBobBennett.JimLib.Xamarin.Timers;
using JimBobBennett.RestAndRelaxForPlex.TmdbObjects;
using JimBobBennett.RestAndRelaxForPlex.TvdbObjects;

namespace JimBobBennett.RestAndRelaxForPlex.Connection
{
    public class ConnectionManager : IConnectionManager
    {
        private const string IpAddress = "239.0.0.250";
        private const int Port = 32414;

        private readonly ITimer _timer;
        private readonly ILocalServerDiscovery _localServerDiscovery;
        private readonly IRestConnection _restConnection;
        private readonly IMyPlexConnection _myPlexConnection;
        private readonly ITvdbCache _tvdbCache;
        private readonly ITmdbCache _tmdbCache;

        public ReadOnlyObservableCollection<ServerConnection> ServerConnections { get; private set; }
        private readonly ObservableCollectionEx<ServerConnection> _allServerConnections = new ObservableCollectionEx<ServerConnection>(); 

        private readonly List<PlexServerConnection> _myPlexServerConnections = new List<PlexServerConnection>(); 

        private bool _isPolling;
        private readonly object _syncObject = new object();

        private readonly Dictionary<string, PlexServerConnection> _serverConnections = new Dictionary<string, PlexServerConnection>();

        public ConnectionManager(ITimer timer, ILocalServerDiscovery localServerDiscovery,
            IRestConnection restConnection, IMyPlexConnection myPlexConnection, ITvdbCache tvdbCache, 
            ITmdbCache tmdbCache, INowPlaying nowPlaying)
        {
            _timer = timer;
            _restConnection = restConnection;
            _myPlexConnection = myPlexConnection;
            _tvdbCache = tvdbCache;
            _tmdbCache = tmdbCache;

            _localServerDiscovery = localServerDiscovery;
            _localServerDiscovery.ServerDiscovered += LocalServerDiscoveryOnServerDiscovered;

            ServerConnections = new ReadOnlyObservableCollection<ServerConnection>(_allServerConnections);

            NowPlaying = nowPlaying;
        }
        
        private void UpdateConnections()
        {
            lock (_syncObject)
            {
                var toAdd = _plexServerConnections.Select(p => new ServerConnection(p)).ToList();
                _allServerConnections.UpdateToMatch(toAdd, sc => sc.Key, (sc1, sc2) => sc1.UpdateFrom(sc2));
            }
        }

        private async void LocalServerDiscoveryOnServerDiscovered(object sender, EventArgs<string> eventArgs)
        {
            await CreatePlexServerConnection(eventArgs.Value);
        }

        private async Task<bool> CreatePlexServerConnection(string ipAddress)
        {
            var connection = new PlexServerConnection(_restConnection, ipAddress, _myPlexConnection.User);
            await connection.ConnectAsync();

            var needRebuild = false;

            lock (_syncObject)
            {
                if (!_serverConnections.ContainsKey(connection.MachineIdentifier))
                {
                    _serverConnections.Add(connection.MachineIdentifier, connection);
                    _plexServerConnections.Add(connection);
                    needRebuild = true;
                }
            }

            if (needRebuild)
            {
                await RebuildNowPlaying();
                UpdateConnections();
            }

            return connection.ConnectionStatus == ConnectionStatus.Connected;
        }

        public async Task<bool> ConnectToMyPlexAsync(string username, string password)
        {
            var connected = await MakeMyPlexConnection(username, password);
            if (connected)
            {
                List<PlexServerConnection> connections;
                lock (_syncObject)
                    connections = _plexServerConnections.ToList();

                foreach (var connection in connections)
                {
                    var key = connection.MachineIdentifier;

                    connection.User = _myPlexConnection.User;

                    if (connection.ConnectionStatus == ConnectionStatus.NotAuthorized)
                    {
                        if (await connection.ConnectAsync())
                        {
                            _serverConnections.Remove(key);

                            PlexServerConnection existingConn;
                            if (_serverConnections.TryGetValue(connection.MachineIdentifier, out existingConn))
                            {
                                if (existingConn != connection)
                                {
                                    lock (_syncObject)
                                        _plexServerConnections.Remove(connection);
                                }
                            }
                        }
                    }
                }

                UpdateConnections();

                await RebuildNowPlaying();
            }

            return connected;
        }

        public async Task<bool> ConnectToServerAsync(string uri)
        {
            return await CreatePlexServerConnection(uri);
        }

        public bool IsConnectedToMyPlex { get { return _myPlexConnection.ConnectionStatus == MyPlexConnectionStatus.Connected; } }

        private async Task<bool> MakeMyPlexConnection(string username, string password)
        {
            lock (_syncObject)
            {
                if (_myPlexServerConnections.Any())
                {
                    foreach (var myPlexConnection in _myPlexServerConnections)
                    {
                        PlexServerConnection connection;

                        if (_serverConnections.TryGetValue(myPlexConnection.MachineIdentifier, out connection))
                        {
                            _serverConnections.Remove(myPlexConnection.MachineIdentifier);
                            _plexServerConnections.Remove(connection);
                        }
                    }
                }
            }

            await _myPlexConnection.ConnectAsync(username, password);

            if (_myPlexConnection.ConnectionStatus == MyPlexConnectionStatus.Connected)
            {
                var connections = (await _myPlexConnection.CreateServerConnectionsAsync())
                    .Where(s => s.ConnectionStatus == ConnectionStatus.Connected).Select(p => (PlexServerConnection)p).ToList();

                lock (_syncObject)
                {
                    _myPlexServerConnections.Clear();

                    foreach (var connection in connections.Where(s => !_serverConnections.ContainsKey(s.MachineIdentifier)))
                    {
                        _serverConnections.Add(connection.MachineIdentifier, connection);
                        _plexServerConnections.Add(connection);
                        _myPlexServerConnections.Add(connection);
                    }
                }
            }

            UpdateConnections();

            return _myPlexConnection.ConnectionStatus == MyPlexConnectionStatus.Connected;
        }

        private async Task CheckForNewMyPlexConnectionsAsync()
        {
            var connections = (await _myPlexConnection.CreateServerConnectionsAsync())
                .Where(s => s.ConnectionStatus == ConnectionStatus.Connected).Select(p => (PlexServerConnection) p).ToList();

            lock (_syncObject)
            {
                foreach (var connection in connections.Where(c => !_myPlexServerConnections.Contains(c)))
                {
                    PlexServerConnection existingConnection;
                    if (_serverConnections.TryGetValue(connection.MachineIdentifier, out existingConnection))
                    {
                        _plexServerConnections.Remove(existingConnection);
                        _myPlexServerConnections.Remove(existingConnection);
                    }

                    _serverConnections[connection.MachineIdentifier] = connection;
                    _plexServerConnections.Add(connection);
                    _myPlexServerConnections.Add(connection);
                }
            }

            UpdateConnections();
        }

        public async Task ConnectAsync()
        {
            await PollForNewConnectionsAsync();

            if (!_isPolling)
            {
                _isPolling = true;

                _timer.StartTimer(TimeSpan.FromSeconds(30), async () =>
                    {
                        await PollForNewConnectionsAsync();
                        return true;
                    });

                _timer.StartTimer(TimeSpan.FromSeconds(3), async () =>
                    {
                        IList<PlexServerConnection> connections;
                        lock (_syncObject)
                            connections = _plexServerConnections.ToList();

                        foreach (var connection in connections)
                        {
                            if (connection.ConnectionStatus == ConnectionStatus.Connected)
                                await connection.RefreshAsync();
                            else
                                await connection.ConnectAsync();
                        }

                        if (connections.Any())
                            await RebuildNowPlaying();
                        
                        return true;
                    });
            }
        }

        private async Task PollForNewConnectionsAsync()
        {
            await _localServerDiscovery.DiscoverLocalServersAsync(IpAddress, Port);
            await CheckForNewMyPlexConnectionsAsync();
        }

        public async Task<Video> RefreshVideoAsync(Video video)
        {
            try
            {
                var connection = (PlexServerConnection) video.PlexServerConnection;
                await connection.RefreshAsync();
                await RebuildNowPlaying();

                lock (_syncObject)
                    return NowPlaying.GetNowPlayingForPlayer(video);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to refresh video: " + ex);
                return null;
            }
        }

        private async Task RebuildNowPlaying()
        {
            List<Video> nowPlaying;

            lock (_syncObject)
            {
                nowPlaying = new List<Video>();
                
                foreach (var connection in _plexServerConnections)
                {
                    foreach (var video in connection.NowPlaying.Where(v => v.VideoType != VideoType.Unknown))
                    {
                        video.PlexServerConnection = connection;
                        if (!nowPlaying.Any(v => v.Player.Key == video.Player.Key && 
                            v.PlexServerConnection.MachineIdentifier == video.PlexServerConnection.MachineIdentifier))
                            nowPlaying.Add(video);
                    }
                }
            }
            
            await ((NowPlaying)NowPlaying).UpdateNowPlaying(nowPlaying);
        }

        private readonly ObservableCollectionEx<PlexServerConnection> _plexServerConnections = new ObservableCollectionEx<PlexServerConnection>();

        public INowPlaying NowPlaying { get; private set; }

        private readonly List<Video> _populatingFromExternal = new List<Video>();
        private readonly object _populatingFromExternalSyncObj = new object();

        public async Task PopulateFromExternalSourcesAsync(Video video, bool forceRefresh)
        {
            try
            {
                bool populating;
                lock (_populatingFromExternalSyncObj)
                {
                    populating = _populatingFromExternal.Contains(video);
                    if (!populating)
                        _populatingFromExternal.Add(video);
                }

                while (populating)
                {
                    await Task.Delay(100);
                    lock (_populatingFromExternalSyncObj)
                    {
                        populating = _populatingFromExternal.Contains(video);
                        if (!populating)
                            _populatingFromExternal.Add(video);
                    }
                }

                switch (video.VideoType)
                {
                    case VideoType.Movie:
                        await PopulateFromTmdb(video, forceRefresh);
                        break;
                    case VideoType.Episode:
                        var hasTvdb = video.ExternalIds.TvdbId.IsNullOrEmpty();
                        await PopulateFromTvdb(video, forceRefresh);
                        await PopulateFromTmdb(video, forceRefresh);
                        if (!hasTvdb && !video.ExternalIds.TvdbId.IsNullOrEmpty())
                            await PopulateFromTvdb(video, forceRefresh);
                        break;
                }
            }
            finally
            {
                lock (_populatingFromExternalSyncObj)
                    _populatingFromExternal.Remove(video);
            }
        }

        public async Task PopulateFromExternalSourcesAsync(Role role, bool forceRefresh)
        {
            if (role.ExternalIds.ImdbId.IsNullOrEmpty() &&
                !role.ExternalIds.TmdbId.IsNullOrEmpty())
            {
                var person = await _tmdbCache.GetPersonAsync(role.ExternalIds.TmdbId, forceRefresh);
                if (person != null)
                    role.PopulateFromTmdb(person);
            }
        }

        private async Task PopulateFromTmdb(Video video, bool forceRefresh)
        {
            if (video.HasBeenPopulatedFromTmdb && !forceRefresh) return;

            if (video.VideoType == VideoType.Episode)
            {
                var show = await _tmdbCache.GetTvShowAsync(video, forceRefresh);

                if (show != null)
                {
                    if (video.EpisodeExternalIds.ImdbId.IsNullOrEmpty())
                        video.EpisodeExternalIds.ImdbId = show.EpisodeExternalIds.ImdbId;

                    if (video.EpisodeExternalIds.TvdbId.IsNullOrEmpty())
                        video.EpisodeExternalIds.TvdbId = show.EpisodeExternalIds.TvdbId;

                    if (video.EpisodeExternalIds.TmdbId.IsNullOrEmpty())
                        video.EpisodeExternalIds.TmdbId = show.EpisodeExternalIds.Id;

                    if (video.ExternalIds.ImdbId.IsNullOrEmpty())
                        video.ExternalIds.ImdbId = show.ExternalExternalIds.ImdbId;

                    if (video.ExternalIds.TvdbId.IsNullOrEmpty())
                        video.ExternalIds.TvdbId = show.ExternalExternalIds.TvdbId;

                    if (video.ExternalIds.TmdbId.IsNullOrEmpty())
                        video.ExternalIds.TmdbId = show.ExternalExternalIds.Id;

                    MergeRoles(video, show.Credits);

                    if (video.Summary.IsNullOrEmpty())
                        video.Summary = show.Summary;

                    if (video.OriginallyAvailableAt.IsNullOrEmpty())
                        video.OriginallyAvailableAt = show.AirDate;

                    video.HasBeenPopulatedFromTmdb = true;
                }
            }
            else if (video.VideoType == VideoType.Movie)
            {
                var movie = await _tmdbCache.GetMovieAsync(video, forceRefresh);

                if (movie != null)
                {
                    if (video.ExternalIds.ImdbId.IsNullOrEmpty())
                        video.ExternalIds.ImdbId = movie.ImdbId;

                    if (video.ExternalIds.TmdbId.IsNullOrEmpty())
                        video.ExternalIds.TmdbId = movie.Id;

                    if (video.Summary.IsNullOrEmpty())
                        video.Summary = movie.Summary;

                    MergeRoles(video, movie.Credits);

                    video.HasBeenPopulatedFromTmdb = true;
                }
            }
        }

        private static void MergeRoles(Video video, Credits credits)
        {
            if (video.RolesComeFromTmdb && video.Roles != null && video.Roles.Any())
                return;

            if (credits == null) return;

            var tmdbCastMembers = new List<Cast>();
            if (credits.Cast != null)
                tmdbCastMembers.AddRange(credits.Cast);
            if (credits.GuestStars != null)
                tmdbCastMembers.AddRange(credits.GuestStars);

            if (!tmdbCastMembers.Any()) return;

            if (video.Roles == null)
                video.Roles = new ObservableCollectionEx<Role>();

            var roles = new List<Role>();
            var nextId = 0;

            foreach (var actor in tmdbCastMembers)
            {
                roles.Add(new Role
                {
                    Id = nextId,
                    RoleName = actor.Character,
                    Tag = actor.Name,
                    Thumb = actor.ProfilePath,
                    ExternalIds = new ExternalIds {TmdbId = actor.Id}
                });

                nextId++;
            }

            video.Roles.ClearAndAddRange(roles);
            video.RolesComeFromTmdb = true;
        }

        private static void MergeRoles(Video video, Series series)
        {
            if ((video.RolesComeFromTvdb || video.RolesComeFromTmdb) && video.Roles != null && video.Roles.Any())
                return;
            
            if (series.Actors == null || !series.Actors.Any())
                return;
            
            if (video.Roles == null)
                video.Roles = new ObservableCollectionEx<Role>();

            var roles = new List<Role>();
            var nextId = 0;

            foreach (var actor in series.Actors)
            {
                roles.Add(new Role
                {
                    Id = nextId,
                    RoleName = actor.Role,
                    Tag = actor.Name,
                    Thumb = actor.Image,
                    ExternalIds = new ExternalIds { TvdbId = actor.Id }
                });

                nextId++;
            }

            video.Roles.ClearAndAddRange(roles);
            video.RolesComeFromTvdb = true;
        }

        private async Task PopulateFromTvdb(Video video, bool forceRefresh)
        {
            if ((video.HasBeenPopulatedFromTvdb && !forceRefresh) || video.ExternalIds.TvdbId.IsNullOrEmpty()) return;

            var series = await _tvdbCache.GetSeriesAsync(video.ExternalIds.TvdbId, video.SeasonNumber,
                video.EpisodeNumber, forceRefresh);

            if (series != null)
            {
                if (video.ExternalIds.ImdbId.IsNullOrEmpty())
                    video.ExternalIds.ImdbId = series.ImdbId;

                var episode = series.GetEpisode(video.SeasonNumber, video.EpisodeNumber);
                if (episode != null)
                {
                    if (video.EpisodeExternalIds.ImdbId.IsNullOrEmpty())
                        video.EpisodeExternalIds.ImdbId = episode.ImdbId;

                    if (video.EpisodeExternalIds.TvdbId.IsNullOrEmpty())
                        video.EpisodeExternalIds.TvdbId = episode.Id;

                    MergeRoles(video, series);

                    video.HasBeenPopulatedFromTvdb = true;
                }
            }
        }
    }
}
