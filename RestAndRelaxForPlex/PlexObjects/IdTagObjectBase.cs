using System.Collections.Generic;
using JimBobBennett.JimLib.Mvvm;

namespace JimBobBennett.RestAndRelaxForPlex.PlexObjects
{
    public abstract class IdTagObjectBase<T> : PlexObjectBase<IdTagObjectBase<T>>
    {
        [NotifyPropertyChangeDependency("Key")]
        public long Id { get; set; }

        public string Tag { get; set; }

        protected override bool OnUpdateFrom(IdTagObjectBase<T> newValue, List<string> updatedPropertyNames)
        {
            return UpdateValue(() => Tag, newValue, updatedPropertyNames);
        }

        public override string Key
        {
            get { return Id.ToString(); }
        }
    }
}
