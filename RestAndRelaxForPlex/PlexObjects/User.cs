using System.Collections.Generic;

namespace JimBobBennett.RestAndRelaxForPlex.PlexObjects
{
    public class User : PlexObjectBase<User>
    {
        protected override bool OnUpdateFrom(User newValue, List<string> updatedPropertyNames)
        {
            var isUpdated = UpdateValue(() => Thumb, newValue, updatedPropertyNames);
            isUpdated = UpdateValue(() => Title, newValue, updatedPropertyNames) | isUpdated;

            return isUpdated;
        }

        public override string Key
        {
            get { return Id; }
        }

        public string Id { get; set; }
        public string Thumb { get; set; }
        public string Title { get; set; }
    }
}
