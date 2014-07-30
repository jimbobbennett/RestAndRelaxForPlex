using System.Collections.Generic;
using JimBobBennett.JimLib.Collections;
using JimBobBennett.JimLib.Mvvm;

namespace JimBobBennett.RestAndRelaxForPlex.PlexObjects
{
    public class MediaContainer : PlexObjectBase<MediaContainer>
    {
        public string PublicAddress { get; set; }
        public string FriendlyName { get; set; }
        public string Platform { get; set; }

        [NotifyPropertyChangeDependency("Key")]
        public string MachineIdentifier { get; set; }

        public ObservableCollectionEx<Device> Devices { get; set; }

        public ObservableCollectionEx<Video> Videos { get; set; }

        public ObservableCollectionEx<Server> Servers { get; set; }

        public override string ToString()
        {
            return PublicAddress;
        }

        protected override bool OnUpdateFrom(MediaContainer newValue, List<string> updatedPropertyNames)
        {
            var isUpdated = UpdateValue(() => PublicAddress, newValue, updatedPropertyNames);
            isUpdated = UpdateValue(() => FriendlyName, newValue, updatedPropertyNames) | isUpdated;
            isUpdated = UpdateValue(() => Platform, newValue, updatedPropertyNames) | isUpdated;

            if (Devices == null) Devices = new ObservableCollectionEx<Device>();
            if (Videos == null) Videos = new ObservableCollectionEx<Video>();
            if (Servers == null) Servers = new ObservableCollectionEx<Server>();

            isUpdated = Devices.UpdateToMatch(newValue.Devices, r => r.Key, (d1, d2) => d1.UpdateFrom(d2)) | isUpdated;
            isUpdated = Videos.UpdateToMatch(newValue.Videos, r => r.Key, (v1, v2) => v1.UpdateFrom(v2)) | isUpdated;
            isUpdated = Servers.UpdateToMatch(newValue.Servers, r => r.Key, (s1, s2) => s1.UpdateFrom(s2)) | isUpdated;

            return isUpdated;
        }

        public override string Key
        {
            get { return MachineIdentifier; }
        }
    }
}
