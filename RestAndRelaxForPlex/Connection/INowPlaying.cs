using System.Collections.ObjectModel;
using JimBobBennett.RestAndRelaxForPlex.PlexObjects;

namespace JimBobBennett.RestAndRelaxForPlex.Connection
{
    public interface INowPlaying
    {
        ReadOnlyObservableCollection<Video> VideosNowPlaying { get; }
        Video GetNowPlayingForPlayer(Video video);
        bool IsPlaying(Video video);
    }
}