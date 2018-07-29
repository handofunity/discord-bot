namespace HoU.GuildBot.Shared.BLL
{
    using Objects;
    using StrongTypes;

    public interface IIgnoreGuard
    {
        EmbedData TryAddToIgnoreList(DiscordUserID userID, string username, string messageContent);

        EmbedData TryRemoveFromIgnoreList(DiscordUserID userID, string username);

        bool ShouldIgnoreMessage(DiscordUserID userID);
    }
}