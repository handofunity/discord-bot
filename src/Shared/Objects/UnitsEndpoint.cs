namespace HoU.GuildBot.Shared.Objects
{
    public class UnitsEndpoint
    {
        public string BaseAddress { get; set; }

        public string Secret { get; set; }

        public bool ConnectToRestApi { get; set; }

        public bool ConnectToNotificationHub { get; set; }
    }
}