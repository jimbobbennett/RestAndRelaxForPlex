using System.Collections.Generic;

namespace JimBobBennett.RestAndRelaxForPlex.PlexObjects
{
    public class Connection : PlexObjectBase<Connection>
    {
        public string Uri { get; set; }

        public override string ToString()
        {
            return Uri;
        }

        protected override bool OnUpdateFrom(Connection newValue, List<string> updatedPropertyNames)
        {
            return false;
        }

        public override string Key
        {
            get { return Uri; }
        }
    }
}
