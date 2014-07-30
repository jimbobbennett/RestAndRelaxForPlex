using System.Collections.Generic;

namespace JimBobBennett.RestAndRelaxForPlex.PlexObjects
{
    public class Player : PlexObjectBase<Player>
    {
        public string MachineIdentifier { get; set; } 
        public string Platform { get; set; } 
        public string Product { get; set; }
        public PlayerState State { get; set; } 
        public string Title { get; set; }

        public Server Client { get; internal set; }

        protected override bool OnUpdateFrom(Player newValue, List<string> updatedPropertyNames)
        {
            var isUpdated = UpdateValue(() => Title, newValue, updatedPropertyNames);
            isUpdated = UpdateValue(() => Platform, newValue, updatedPropertyNames) | isUpdated;
            isUpdated = UpdateValue(() => Product, newValue, updatedPropertyNames) | isUpdated;
            isUpdated = UpdateValue(() => State, newValue, updatedPropertyNames) | isUpdated;
            isUpdated = UpdateValue(() => Title, newValue, updatedPropertyNames) | isUpdated;

            if (Client == null)
                Client = new Server();
            
            isUpdated = Client.UpdateFrom(newValue.Client) | isUpdated;

            return isUpdated;
        }

        public override string Key
        {
            get { return MachineIdentifier; }
        }
    }
}
