namespace JimBobBennett.RestAndRelaxForPlex.PlexObjects
{
    public class PlexUser
    {
        public string Username { get; set; }

        public string Email { get; set; }

        public int Id { get; set; }

        public string Thumb { get; set; }

        public string QueueEmail { get; set; }

        public string QueueUid { get; set; }

        public string CloudSyncDevice { get; set; }

        public string AuthenticationToken { get; set; }
    }
}