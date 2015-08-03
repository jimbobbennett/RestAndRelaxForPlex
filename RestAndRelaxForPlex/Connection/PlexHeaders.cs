using System;
using System.Collections.Generic;
using System.Text;
using JimBobBennett.RestAndRelaxForPlex.PlexObjects;

namespace JimBobBennett.RestAndRelaxForPlex.Connection
{
    internal static class PlexHeaders
    {
        public static Dictionary<string, string> CreatePlexRequest(PlexUser user = null, 
            string userName = null, string password = null)
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

            if (!string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(password))
            {
                var authBytes = Encoding.UTF8.GetBytes(userName + ":" + password);
                headers.Add("Authorization", "BASIC " + Convert.ToBase64String(authBytes));
            }

            if (user != null)
                headers.Add("X-Plex-Token", user.AuthenticationToken);

            return headers;
        }
    }
}
