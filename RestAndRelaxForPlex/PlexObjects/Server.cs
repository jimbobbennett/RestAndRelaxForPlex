using System.Collections.Generic;
using JimBobBennett.JimLib.Mvvm;

namespace JimBobBennett.RestAndRelaxForPlex.PlexObjects
{
    public class Server : PlexObjectBase<Server>
    {
        public string Name { get; set; }
        public string Host { get; set; }
        public string Address { get; set; }
        public int Port { get; set; }

        [NotifyPropertyChangeDependency("Key")]
        public string MachineIdentifier { get; set; }

        protected override bool OnUpdateFrom(Server newValue, List<string> updatedPropertyNames)
        {
            var isUpdated = UpdateValue(() => Name, newValue, updatedPropertyNames);
            isUpdated = UpdateValue(() => Host, newValue, updatedPropertyNames) | isUpdated;
            isUpdated = UpdateValue(() => Address, newValue, updatedPropertyNames) | isUpdated;
            isUpdated = UpdateValue(() => Port, newValue, updatedPropertyNames) | isUpdated;

            return isUpdated;
        }

        public override string Key
        {
            get { return MachineIdentifier; }
        }
    }
}
