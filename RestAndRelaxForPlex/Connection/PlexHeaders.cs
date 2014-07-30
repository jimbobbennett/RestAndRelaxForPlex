using System.Collections.Generic;
using JimBobBennett.RestAndRelaxForPlex.PlexObjects;

namespace JimBobBennett.RestAndRelaxForPlex.Connection
{
    internal static class PlexHeaders
    {
        public static Dictionary<string, string> CreatePlexRequest(PlexUser user = null)
        {
            var headers = new Dictionary<string, string>
            {
                {"X-Plex-Platform", "Windows"},
                {"X-Plex-Platform-Version", "NT"}, 
                {"X-Plex-Provides", "player"},
                {"X-Plex-Client-Identifier", "RestAndRelaxForPlex"},
                {"X-Plex-Product", "PlexWMC"},
                {"X-Plex-Version", "0"}
            };

            if (user != null)
                headers.Add("X-Plex-Token", user.AuthenticationToken);

            return headers;
        }
    }
}
