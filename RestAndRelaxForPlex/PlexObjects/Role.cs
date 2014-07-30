using System;
using System.Collections.Generic;
using System.Linq;
using JimBobBennett.JimLib.Extensions;
using JimBobBennett.JimLib.Xml;
using JimBobBennett.RestAndRelaxForPlex.TmdbObjects;

namespace JimBobBennett.RestAndRelaxForPlex.PlexObjects
{
    public class Role : IdTagObjectBase<Role>
    {
        public Role()
        {
            ExternalIds = new ExternalIds();
            CastCredits = new List<ICastCrewCredit>();
            CrewCredits = new List<ICastCrewCredit>();
        }

        [XmlNameMapping("Role")]
        public string RoleName { get; set; }

        public string Summary { get; set; }
        public string BirthDay { get; set; }
        public string DeathDay { get; set; }
        public string PlaceofBirth { get; set; }

        public List<ICastCrewCredit> CastCredits { get; set; }
        public List<ICastCrewCredit> CrewCredits { get; set; }

        public string Thumb { get; set; }

        public ExternalIds ExternalIds { get; set; }

        public Uri Uri
        {
            get
            {
                if (!ExternalIds.ImdbId.IsNullOrEmpty())
                    return ImdbUrl;

                if (!ExternalIds.TmdbId.IsNullOrEmpty())
                    return TmdbUrl;

                return null;
            }
        }

        public Uri SchemeUri
        {
            get
            {
                if (!ExternalIds.ImdbId.IsNullOrEmpty())
                    return ImdbSchemeUrl;

                return null;
            }
        }

        internal Uri ImdbUrl
        {
            get { return new Uri(string.Format(PlexResources.ImdbNameUrl, ExternalIds.ImdbId)); }
        }

        internal Uri TmdbUrl
        {
            get { return new Uri(string.Format(PlexResources.TmdbPersonUrl, ExternalIds.ImdbId)); }
        }

        internal Uri ImdbSchemeUrl
        {
            get { return new Uri(string.Format(PlexResources.ImdbNameSchemeUrl, ExternalIds.ImdbId)); }
        }

        protected override bool OnUpdateFrom(IdTagObjectBase<Role> newValue, List<string> updatedPropertyNames)
        {
            var isUpdated = base.OnUpdateFrom(newValue, updatedPropertyNames);
            isUpdated = UpdateValue(() => RoleName, newValue, updatedPropertyNames) | isUpdated;
            isUpdated = UpdateValue(() => Thumb, newValue, updatedPropertyNames) | isUpdated;

            return isUpdated;
        }

        public void PopulateFromTmdb(Person person)
        {
            ExternalIds.ImdbId = person.ImdbId;
            Summary = person.Summary;
            BirthDay = person.BirthDay;
            DeathDay = person.DeathDay;
            PlaceofBirth = person.PlaceofBirth;

            CastCredits = person.Credits.Cast == null ? new List<ICastCrewCredit>() : person.Credits.Cast.OfType<ICastCrewCredit>().ToList();
            CrewCredits = person.Credits.Crew == null ? new List<ICastCrewCredit>() : person.Credits.Crew.OfType<ICastCrewCredit>().ToList();
        }
    }
}
