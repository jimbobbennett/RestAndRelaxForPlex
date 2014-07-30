using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using JimBobBennett.JimLib.Events;
using JimBobBennett.RestAndRelaxForPlex.PlexObjects;

namespace JimBobBennett.RestAndRelaxForPlex.Connection
{
    public interface IPlexServerConnection : INotifyPropertyChanged
    {
        Device Device { get; }
        string ConnectionUri { get; }
        ConnectionStatus ConnectionStatus { get; }
        event EventHandler<EventArgs<ConnectionStatus>> ConnectionStatusChanged;
 
        string Platform { get; }
        string MachineIdentifier { get; }
        string Name { get; }

        ReadOnlyObservableCollection<Video> NowPlaying { get; }
        ReadOnlyObservableCollection<Server> Clients { get; }
        PlexUser User { get; }

        Task<bool> PauseVideoAsync(Video video);
        Task<bool> PlayVideoAsync(Video video);
        Task<bool> StopVideoAsync(Video video);
    }
}