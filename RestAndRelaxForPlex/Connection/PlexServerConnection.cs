using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using JimBobBennett.JimLib.Events;
using JimBobBennett.RestAndRelaxForPlex.PlexObjects;
using JimBobBennett.JimLib.Collections;
using JimBobBennett.JimLib.Mvvm;
using JimBobBennett.JimLib.Network;

namespace JimBobBennett.RestAndRelaxForPlex.Connection
{
    public class PlexServerConnection : NotificationObject, IPlexServerConnection
    {
        private readonly IRestConnection _restConnection;
        private Device _device;
        private string _connectionUri;
        private MediaContainer _mediaContainer;

        [NotifyPropertyChangeDependency("Name")]
        public Device Device
        {
            get { return _device; }
            private set
            {
                if (Equals(Device, value)) return;
                _device = value;
                RaisePropertyChanged();
            }
        }

        [NotifyPropertyChangeDependency("Name")]
        public string ConnectionUri
        {
            get { return _connectionUri; }
            private set
            {
                if (Equals(ConnectionUri, value)) return;
                _connectionUri = value;
                RaisePropertyChanged();
            }
        }

        [NotifyPropertyChangeDependency("IsOnLine")]
        [NotifyPropertyChangeDependency("Platform")]
        [NotifyPropertyChangeDependency("MachineIdentifier")]
        [NotifyPropertyChangeDependency("Name")]
        public MediaContainer MediaContainer
        {
            get { return _mediaContainer; }
            private set
            {
                if (Equals(MediaContainer, value)) return;
                _mediaContainer = value;
                RaisePropertyChanged();
            }
        }

        public ConnectionStatus ConnectionStatus
        {
            get { return _connectionStatus; }
            private set
            {
                if (_connectionStatus == value) return;

                _connectionStatus = value;
                RaisePropertyChanged();

                WeakEventManager.GetWeakEventManager(this).RaiseEvent(this,
                    new EventArgs<ConnectionStatus>(ConnectionStatus), "ConnectionStatusChanged");
            }
        }

        public event EventHandler<EventArgs<ConnectionStatus>> ConnectionStatusChanged
        {
            add { WeakEventManager.GetWeakEventManager(this).AddEventHandler("ConnectionStatusChanged", value); }
            remove { WeakEventManager.GetWeakEventManager(this).RemoveEventHandler("ConnectionStatusChanged", value); }
        }

        public string Platform { get { return MediaContainer != null ? MediaContainer.Platform : string.Empty; } }

        public string MachineIdentifier
        {
            get
            {
                if (MediaContainer != null)
                    return MediaContainer.MachineIdentifier;

                return Device != null ? Device.ClientIdentifier : ConnectionUri;
            }
        }

        public string Name
        {
            get
            {
                if (MediaContainer != null)
                    return MediaContainer.FriendlyName;

                return Device != null ? Device.Name : ConnectionUri;
            }
        }
        
        private readonly ObservableCollectionEx<Video> _nowPlaying = new ObservableCollectionEx<Video>();
        private readonly ObservableCollectionEx<Server> _clients = new ObservableCollectionEx<Server>();
        private ConnectionStatus _connectionStatus;

        public ReadOnlyObservableCollection<Video> NowPlaying { get; private set; }
        public ReadOnlyObservableCollection<Server> Clients { get; private set; }
        public PlexUser User { get; set; }

        private PlexServerConnection(IRestConnection restConnection)
        {
            _restConnection = restConnection;
            NowPlaying = new ReadOnlyObservableCollection<Video>(_nowPlaying);
            Clients = new ReadOnlyObservableCollection<Server>(_clients);
        }

        public PlexServerConnection(IRestConnection restConnection, Device device, PlexUser user = null)
            : this(restConnection)
        {
            User = user;
            Device = device;
        }

        public PlexServerConnection(IRestConnection restConnection, string uri, PlexUser user = null)
            : this(restConnection)
        {
            User = user;
            ConnectionUri = TidyUrl(uri);
        }

        internal async Task<bool> ConnectAsync()
        {
            if (Device != null)
                await MakeConnectionAsync(Device.Connections);
            else
                await TryConnectionAsync(ConnectionUri);

            if (ConnectionStatus == ConnectionStatus.Connected)
                await RefreshSessionAsync();

            return ConnectionStatus == ConnectionStatus.Connected;
        }

        private static string TidyUrl(string uri)
        {
            if (!uri.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
                uri = "http://" + uri;

            if (uri.IndexOf(':', 5) == -1)
                uri += ":32400";

            return uri;
        }

        private async Task MakeConnectionAsync(IEnumerable<PlexObjects.Connection> connections)
        {
            foreach (var connection in connections.Reverse().Where(c => c.Uri != "http://:0"))
            {
                try
                {
                    if (await TryConnectionAsync(connection.Uri))
                    {
                        ConnectionUri = connection.Uri;
                        return;
                    }
                }
// ReSharper disable once EmptyGeneralCatchClause
                catch (Exception)
                {
                    
                }
            }

            ClearMediaContainer();
        }

        private void ClearMediaContainer()
        {
            MediaContainer = null;
            ConnectionStatus = ConnectionStatus.NotConnected;

            _nowPlaying.Clear();
            _clients.Clear();
        }

        private bool _needsAuth;

        private async Task<RestResponse<T>> MakePlexRequestAsync<T, TData>(string uri, string resource) 
            where T : class, new() where TData : class
        {
            if (_needsAuth)
            {
                return await _restConnection.MakeRequestAsync<T, TData>(Method.Get,
                ResponseType.Xml, uri, resource, headers: PlexHeaders.CreatePlexRequest(User));
            }

            var retVal = await _restConnection.MakeRequestAsync<T, TData>(Method.Get,
                ResponseType.Xml, uri, resource, headers: PlexHeaders.CreatePlexRequest());

            if ((retVal == null || retVal.ResponseObject == null) && User != null)
            {
                _needsAuth = true;
                retVal = await _restConnection.MakeRequestAsync<T, TData>(Method.Get,
                ResponseType.Xml, uri, resource, headers: PlexHeaders.CreatePlexRequest(User));
            }

            return retVal;
        }

        private async Task<bool> TryConnectionAsync(string uri)
        {
            var response = await MakePlexRequestAsync<MediaContainer, string>(uri, "/");

            if (response == null || response.ResponseObject == null)
            {
                if (response != null && response.StatusCode == 401)
                    ConnectionStatus = ConnectionStatus.NotAuthorized;
                else
                    ConnectionStatus = ConnectionStatus.NotConnected;

                MediaContainer = null;
                return false;
            }
            
            if (MediaContainer == null)
            {
                MediaContainer = response.ResponseObject;
                await RefreshSessionAsync();
            }
            else
            {
                if (response.ResponseObject == null)
                    ClearMediaContainer();
                else
                {
                    if (MediaContainer.UpdateFrom(response.ResponseObject))
                        RaisePropertyChanged(() => MediaContainer);

                    await RefreshSessionAsync();
                }
            }

            ConnectionStatus = ConnectionStatus.Connected;

            return true;
        }

        public async Task RefreshAsync()
        {
            var connected = await TryConnectionAsync(ConnectionUri);

            if (connected)
                await RefreshSessionAsync();
            else
                ClearMediaContainer();
        }

        private async Task RefreshSessionAsync()
        {
            IList<Video> videos;
            IList<Server> clients;

            try
            {
                videos = await GetNowPlayingAsync();
            }
            catch
            {
                videos = new List<Video>();
            }

            try
            {
                clients = await GetClientsAsync();
            }
            catch
            {
                clients = new List<Server>();
            }

            foreach (var video in videos)
            {
                var client = clients.FirstOrDefault(c => c.Key == video.Player.Key);
                if (client != null)
                    video.Player.Client = client;
            }

            _clients.ClearAndAddRange(clients);
            _nowPlaying.ClearAndAddRange(videos);
        }

        private async Task<IList<Video>> GetNowPlayingAsync()
        {
            if (ConnectionUri == null)
                return new List<Video>();

            var container = await MakePlexRequestAsync<MediaContainer, string>(ConnectionUri, PlexResources.ServerSessions);

            if (container == null)
                return new List<Video>();

            if (container.ResponseObject == null || container.ResponseObject.Videos == null)
                return new List<Video>();

            return container.ResponseObject.Videos;
        }

        private async Task<IList<Server>> GetClientsAsync()
        {
            var container = await MakePlexRequestAsync<MediaContainer, string>(ConnectionUri,
                PlexResources.ServerClients);

            if (container == null)
                return new List<Server>();

            return container.ResponseObject != null  ? container.ResponseObject.Servers : new ObservableCollectionEx<Server>();
        }

        public async Task<bool> PauseVideoAsync(Video video)
        {
            return await ChangeClientPlayback(video, PlexResources.ClientPause);
        }

        public async Task<bool> PlayVideoAsync(Video video)
        {
            return await ChangeClientPlayback(video, PlexResources.ClientPlay);
        }

        public async Task<bool> StopVideoAsync(Video video)
        {
            return await ChangeClientPlayback(video, PlexResources.ClientStop);
        }

        private async Task<bool> ChangeClientPlayback(Video video, string action)
        {
            if (video != null && video.Player != null && video.Player.Client != null)
            {
                var client = video.Player.Client;

                var clientUriBuilder = new UriBuilder
                {
                    Port = client.Port,
                    Host = client.Host,
                    Scheme = "http"
                };

                var response = await MakePlexRequestAsync<Response, string>(clientUriBuilder.Uri.ToString(), action);

                return response != null && response.StatusCode == 200;
            }

            return false;
        }
    }
}
