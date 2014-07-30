using System;
using System.Collections.Generic;

namespace JimBobBennett.RestAndRelaxForPlex.PlexObjects
{
    public class ExternalIds : PlexObjectBase<ExternalIds>
    {
        private readonly string _key;

        public ExternalIds()
        {
            _key = Guid.NewGuid().ToString("N");
        }

        public string ImdbId { get; set; }
        public string TvdbId { get; set; }
        public string TmdbId { get; set; }

        protected override bool OnUpdateFrom(ExternalIds newValue, List<string> updatedPropertyNames)
        {
            var isUpdated = UpdateValue(() => ImdbId, newValue, updatedPropertyNames);
            isUpdated = UpdateValue(() => TvdbId, newValue, updatedPropertyNames) | isUpdated;
            isUpdated = UpdateValue(() => TmdbId, newValue, updatedPropertyNames) | isUpdated;

            return isUpdated;
        }

        public override string Key { get { return _key; }}

        public override string ToString()
        {
            return "IMDB id: " + ImdbId + "\n" +
                   "The TVDB id: " + TvdbId + "\n" +
                   "TMDB id: " + TmdbId;
        }
    }
}
