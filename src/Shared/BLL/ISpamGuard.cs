using HoU.GuildBot.Shared.Enums;
using HoU.GuildBot.Shared.StrongTypes;

namespace HoU.GuildBot.Shared.BLL;

public interface ISpamGuard
{
    SpamCheckResult CheckForSpam(DiscordUserId userId,
                                 DiscordChannelId channelId,
                                 string message,
                                 int attachmentsCount);
}