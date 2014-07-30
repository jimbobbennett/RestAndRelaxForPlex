using JimBobBennett.JimLib.Events;
using JimBobBennett.JimLib.Mvvm;

namespace JimBobBennett.RestAndRelaxForPlex.Connection
{
    public class ServerConnection : NotificationObject
    {
        public ServerConnection(IPlexServerConnection plexServerConnection)
        {
            Key = plexServerConnection.MachineIdentifier;
            plexServerConnection.ConnectionStatusChanged += OnConnectionStatusChanged;

            PopulateFromPlexConnection(plexServerConnection);
        }

        private void PopulateFromPlexConnection(IPlexServerConnection plexServerConnection)
        {
            Title = plexServerConnection.Name;
            Platform = plexServerConnection.Platform;
            ConnectionStatus = plexServerConnection.ConnectionStatus;

            RaisePropertyChangedForAll();
        }

        private void OnConnectionStatusChanged(object sender, EventArgs<ConnectionStatus> eventArgs)
        {
            PopulateFromPlexConnection((IPlexServerConnection)sender);
        }

        public bool UpdateFrom(ServerConnection other)
        {
            var updated = false;

            if (Title != other.Title)
            {
                Title = other.Title;
                updated = true;
            }

            if (Platform != other.Platform)
            {
                Platform = other.Platform;
                updated = true;
            }

            if (ConnectionStatus != other.ConnectionStatus)
            {
                ConnectionStatus = other.ConnectionStatus;
                updated = true;
            }

            if (updated)
                RaisePropertyChangedForAll();

            return updated;
        }

        public string Title { get; private set; }
        public string Platform { get; private set; }
        public ConnectionStatus ConnectionStatus { get; private set; }

        public string Key { get; private set; }

        public bool IsConnected { get { return ConnectionStatus == ConnectionStatus.Connected; } }
    }
}
