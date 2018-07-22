namespace HoU.GuildBot.Shared.Objects
{
    using StrongTypes;

    public class User
    {
        public InternalUserID InternalUserID { get; }

        public DiscordUserID DiscordUserID { get; }

        public string Mention => $"<@{DiscordUserID}>";

        public User(InternalUserID internalUserID, DiscordUserID discordUserID)
        {
            InternalUserID = internalUserID;
            DiscordUserID = discordUserID;
        }
    }
}