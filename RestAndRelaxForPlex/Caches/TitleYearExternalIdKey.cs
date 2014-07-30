using System;

namespace JimBobBennett.RestAndRelaxForPlex.Caches
{
    public class TitleYearExternalIdKey : IEquatable<TitleYearExternalIdKey>
    {
        public TitleYearExternalIdKey(string title, int year, string imdbId, string tvdbId = null)
        {
            Title = title;
            ImdbId = imdbId;
            TvdbId = tvdbId;
            Year = year;
        }

        public TitleYearExternalIdKey()
        {
        }

        public string Title { get; set; }
        public string ImdbId { get; set; }
        public int Year { get; set; }
        public string TvdbId { get; set; }

        public bool Equals(TitleYearExternalIdKey other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Title, other.Title) && 
                   string.Equals(ImdbId, other.ImdbId) && 
                   Year == other.Year && 
                   string.Equals(TvdbId, other.TvdbId);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((TitleYearExternalIdKey) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Title != null ? Title.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (ImdbId != null ? ImdbId.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ Year;
                hashCode = (hashCode*397) ^ (TvdbId != null ? TvdbId.GetHashCode() : 0);
                return hashCode;
            }
        }

        public static bool operator ==(TitleYearExternalIdKey left, TitleYearExternalIdKey right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(TitleYearExternalIdKey left, TitleYearExternalIdKey right)
        {
            return !Equals(left, right);
        }
    }
}