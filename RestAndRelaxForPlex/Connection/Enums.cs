namespace JimBobBennett.RestAndRelaxForPlex.Connection
{
    public enum ConnectionStatus
    {
        NotConnected,
        NotAuthorized,
        Connected
    }

    public enum MyPlexConnectionStatus
    {
        NotConnected,
        Connecting,
        AuthorizationFailed,
        Connected
    }
}
