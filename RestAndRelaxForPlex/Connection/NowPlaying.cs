using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using JimBobBennett.JimLib.Async;
using JimBobBennett.JimLib.Collections;
using JimBobBennett.JimLib.Extensions;
using JimBobBennett.JimLib.Xamarin.Images;
using JimBobBennett.RestAndRelaxForPlex.PlexObjects;

namespace JimBobBennett.RestAndRelaxForPlex.Connection
{
    public class NowPlaying : INowPlaying
    {
        private readonly IImageHelper _imageHelper;
        public ReadOnlyObservableCollection<Video> VideosNowPlaying { get; private set; }

        public Video GetNowPlayingForPlayer(Video video)
        {
            return GetVideo(video.PlexServerConnection.MachineIdentifier, video.Player.Key);
        }

        private Video GetVideo(string connectionMachineId, string playerId)
        {
            Dictionary<string, Video> byPlayer;
            if (_nowPlayingByServerAndPlayer.TryGetValue(connectionMachineId, out byPlayer))
            {
                Video video;
                if (byPlayer.TryGetValue(playerId, out video))
                    return video;
            }

            return null;
        }

        private readonly ObservableCollectionEx<Video> _videosNowPlaying= new ObservableCollectionEx<Video>();

        private readonly Dictionary<string, Dictionary<string, Video>> _nowPlayingByServerAndPlayer = new Dictionary<string, Dictionary<string, Video>>();

        public NowPlaying(IImageHelper imageHelper)
        {
            _imageHelper = imageHelper;
            _videosNowPlaying.RaiseResetOnRestartEvents = false;
            VideosNowPlaying = new ReadOnlyObservableCollection<Video>(_videosNowPlaying);
        }

        private readonly object _syncObj = new object();

        internal async Task UpdateNowPlaying(List<Video> nowPlaying)
        {
            await Task.Run(() =>
                {
                    try
                    {
                        lock (_syncObj)
                        {
                            _videosNowPlaying.StopEvents = true;

                            // first delete all that are not playing
                            foreach (var video in _videosNowPlaying.ToList())
                                RemoveIfRequired(nowPlaying, video);

                            // now add all new ones or update
                            foreach (var video in nowPlaying)
                                AddOrUpdateVideo(video);
                        }
                    }
                    finally
                    {
                        _videosNowPlaying.StopEvents = false;
                    }
                });
        }

        private void RemoveIfRequired(IEnumerable<Video> nowPlaying, Video video)
        {
            if (!nowPlaying.Any(v => VideosMatchByServerAndPlayer(v, video)))
            {
                var connectionKey = video.PlexServerConnection.MachineIdentifier;
                var playerKey = video.Player.MachineIdentifier;
                
                Dictionary<string, Video> byPlayer;
                if (_nowPlayingByServerAndPlayer.TryGetValue(connectionKey, out byPlayer))
                {
                    if (byPlayer.Remove(playerKey) && !byPlayer.Any())
                        _nowPlayingByServerAndPlayer.Remove(connectionKey);
                }

                _videosNowPlaying.Remove(video);
            }
        }

        private void AddOrUpdateVideo(Video video)
        {
            var connectionKey = video.PlexServerConnection.MachineIdentifier;
            var playerKey = video.Player.MachineIdentifier;

            Dictionary<string, Video> byPlayer;
            if (!_nowPlayingByServerAndPlayer.TryGetValue(connectionKey, out byPlayer))
            {
                byPlayer = new Dictionary<string, Video>();
                _nowPlayingByServerAndPlayer.Add(connectionKey, byPlayer);
            }

            Video oldVideo;
            if (!byPlayer.TryGetValue(playerKey, out oldVideo))
            {
                AsyncHelper.RunSync(() => LoadThumbImageIfRequired(video));
                byPlayer.Add(playerKey, video);
                InsertVideo(video);
            }
            else
            {
                var matches = video.Guid == oldVideo.Guid;

                if (matches)
                {
                    oldVideo.ViewOffset = video.ViewOffset;
                    oldVideo.PlayerState = video.PlayerState;

                    AsyncHelper.RunSync(() => LoadThumbImageIfRequired(oldVideo));
                }
                else
                {
                    AsyncHelper.RunSync(() => LoadThumbImageIfRequired(video));
                    byPlayer[playerKey] = video;

                    _videosNowPlaying.Remove(oldVideo);
                    InsertVideo(video);
                }
            }
        }

        private void InsertVideo(Video video)
        {
            for (var i = 0; i < _videosNowPlaying.Count; i++)
            {
                var compare = String.Compare(_videosNowPlaying[i].Title, video.Title, StringComparison.OrdinalIgnoreCase);
                if (compare >= 0)
                {
                    _videosNowPlaying.Insert(i, video);
                    return;
                }
            }

            _videosNowPlaying.Add(video);
        }

        private async Task LoadThumbImageIfRequired(Video video)
        {
            if (!video.VideoImageSource.IsNullOrEmpty() && video.ThumbImageSource == null)
            {
                Debug.WriteLine("Loading image for " + video.Title + " from " + video.VideoImageSource + " with token " + 
                    video.PlexServerConnection.User.AuthenticationToken);

                var image = await _imageHelper.GetImageAsync(video.PlexServerConnection.ConnectionUri,
                    video.VideoImageSource, headers: PlexHeaders.CreatePlexRequest(video.PlexServerConnection.User),
                    canCache: true);

                Debug.WriteLine(image == null ? "Loading image failed." : "Loading image success!");

                if (image != null)
                    video.ThumbImageSource = image;
            }

            if (!video.Art.IsNullOrEmpty() && video.ArtImageSource == null)
            {
                Debug.WriteLine("Loading image for " + video.Title + " from " + video.Art + " with token " +
                    video.PlexServerConnection.User.AuthenticationToken);

                var image = await _imageHelper.GetImageAsync(video.PlexServerConnection.ConnectionUri,
                    video.Art, headers: PlexHeaders.CreatePlexRequest(video.PlexServerConnection.User),
                    canCache: true);

                Debug.WriteLine(image == null ? "Loading image failed." : "Loading image success!");

                if (image != null)
                    video.ArtImageSource = image;
            }
        }

        private static bool VideosMatchByServerAndPlayer(Video v, Video video)
        {
            return v.PlexServerConnection.MachineIdentifier == video.PlexServerConnection.MachineIdentifier && 
                   v.Player.MachineIdentifier == video.Player.MachineIdentifier;
        }

        public bool IsPlaying(Video video)
        {
            lock (_syncObj)
                return VideosNowPlaying.Contains(video);
        }
    }
}
