using HoU.GuildBot.Shared.Enums;
using HoU.GuildBot.Shared.StrongTypes;

namespace HoU.GuildBot.Shared.BLL
{
    public interface ISpamGuard
    {
        SpamCheckResult CheckForSpam(DiscordUserID userId, DiscordChannelID channelId, string message, int attachmentsCount);
    }
}