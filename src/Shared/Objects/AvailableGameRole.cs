namespace HoU.GuildBot.Shared.Objects
{
    public class AvailableGameRole
    {
        public ulong DiscordRoleID { get; set; }

        public string RoleName { get; set; }

        public AvailableGameRole Clone()
        {
            return new AvailableGameRole
            {
                DiscordRoleID = DiscordRoleID,
                RoleName = RoleName
            };
        }
    }
}