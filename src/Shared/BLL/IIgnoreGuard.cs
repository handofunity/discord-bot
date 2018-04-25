namespace HoU.GuildBot.Shared.BLL
{
    using Objects;

    public interface IIgnoreGuard
    {
        EmbedData TryAddToIgnoreList(ulong userId, string username, string messageContent);

        EmbedData TryRemoveFromIgnoreList(ulong userId, string username);

        bool ShouldIgnoreMessage(ulong userId);
    }
}