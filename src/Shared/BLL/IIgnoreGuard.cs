using HoU.GuildBot.Shared.Objects;
using HoU.GuildBot.Shared.StrongTypes;

namespace HoU.GuildBot.Shared.BLL;

public interface IIgnoreGuard
{
    EmbedData EnsureOnIgnoreList(DiscordUserId userID,
                                 string username,
                                 int minutes);

    bool TryRemoveFromIgnoreList(DiscordUserId userID,
                                 string username,
                                 out EmbedData? embedData);

    bool ShouldIgnore(DiscordUserId userID);
}