using System.Collections.Generic;
using System.Linq;
using JimBobBennett.JimLib.Collections;
using JimBobBennett.JimLib.Mvvm;
using JimBobBennett.JimLib.Xml;

namespace JimBobBennett.RestAndRelaxForPlex.PlexObjects
{
    public class Device : PlexObjectBase<Device>
    {
        [XmlNameMapping("Device")]
        public string DeviceName { get; set; }

        [NotifyPropertyChangeDependency("Key")]
        public string ClientIdentifier { get; set; }

        public string Name { get; set; }
        public string PublicAddress { get; set; }
        public string Product { get; set; }
        public string ProductVersion { get; set; }
        public string Platform { get; set; }
        public string PlatformVersion { get; set; }
        public string Model { get; set; }
        public string Vendor { get; set; }
        public string Provides { get; set; }
        public string Version { get; set; }
        public string Id { get; set; }
        public string Token { get; set; }
        public long CreatedAt { get; set; }
        public long LastSeenAt { get; set; }
        public string ScreenResolution { get; set; }
        public string ScreenDensity { get; set; }

        public ObservableCollectionEx<Connection> Connections { get; set; }

        public override string ToString()
        {
            return Name;
        }

        protected override bool OnUpdateFrom(Device newDevice, List<string> updatedPropertyNames)
        {
            var isUpdated = UpdateValue(() => CreatedAt, newDevice, updatedPropertyNames);
            isUpdated = UpdateValue(() => Id, newDevice, updatedPropertyNames) | isUpdated;
            isUpdated = UpdateValue(() => LastSeenAt, newDevice, updatedPropertyNames) | isUpdated;
            isUpdated = UpdateValue(() => Name, newDevice, updatedPropertyNames) | isUpdated;
            isUpdated = UpdateValue(() => Platform, newDevice, updatedPropertyNames) | isUpdated;
            isUpdated = UpdateValue(() => PlatformVersion, newDevice, updatedPropertyNames) | isUpdated;
            isUpdated = UpdateValue(() => ProductVersion, newDevice, updatedPropertyNames) | isUpdated;
            isUpdated = UpdateValue(() => Provides, newDevice, updatedPropertyNames) | isUpdated;
            isUpdated = UpdateValue(() => PublicAddress, newDevice, updatedPropertyNames) | isUpdated;
            isUpdated = UpdateValue(() => ScreenDensity, newDevice, updatedPropertyNames) | isUpdated;
            isUpdated = UpdateValue(() => ScreenResolution, newDevice, updatedPropertyNames) | isUpdated;
            isUpdated = UpdateValue(() => Token, newDevice, updatedPropertyNames) | isUpdated;
            isUpdated = UpdateValue(() => Vendor, newDevice, updatedPropertyNames) | isUpdated;
            isUpdated = UpdateValue(() => Version, newDevice, updatedPropertyNames) | isUpdated;

            if (Connections == null)
                Connections = new ObservableCollectionEx<Connection>();

            foreach (var connection in Connections.ToList().Where(con => newDevice.Connections.All(c => c.Uri != con.Uri)))
            {
                Connections.Remove(connection);
                isUpdated = true;
            }

            foreach (var connection in newDevice.Connections.Where(con => Connections.All(c => c.Uri != con.Uri)))
            {
                Connections.Add(connection);
                isUpdated = true;
            }
            
            return isUpdated;
        }

        public override string Key
        {
            get { return ClientIdentifier; }
        }
    }
}