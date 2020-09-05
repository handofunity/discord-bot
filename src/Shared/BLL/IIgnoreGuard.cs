using HoU.GuildBot.Shared.Objects;
using HoU.GuildBot.Shared.StrongTypes;

namespace HoU.GuildBot.Shared.BLL
{
    public interface IIgnoreGuard
    {
        EmbedData TryAddToIgnoreList(DiscordUserID userID, string username, string messageContent);

        EmbedData TryRemoveFromIgnoreList(DiscordUserID userID, string username);

        bool ShouldIgnoreMessage(DiscordUserID userID);
    }
}