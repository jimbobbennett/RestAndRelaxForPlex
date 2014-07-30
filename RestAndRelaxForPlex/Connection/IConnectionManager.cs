using System.Collections.ObjectModel;
using System.Threading.Tasks;
using JimBobBennett.RestAndRelaxForPlex.PlexObjects;

namespace JimBobBennett.RestAndRelaxForPlex.Connection
{
    public interface IConnectionManager
    {
        Task<bool> ConnectToMyPlexAsync(string username, string password);
        Task<bool> ConnectToServerAsync(string uri);
        bool IsConnectedToMyPlex { get; }
        Task ConnectAsync();
        Task<Video> RefreshVideoAsync(Video video);

        INowPlaying NowPlaying { get; }
        ReadOnlyObservableCollection<ServerConnection> ServerConnections { get; }

        Task PopulateFromExternalSourcesAsync(Video video, bool forceRefresh);
        Task PopulateFromExternalSourcesAsync(Role role, bool forceRefresh);
    }
}