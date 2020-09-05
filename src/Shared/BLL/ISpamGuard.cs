using HoU.GuildBot.Shared.Enums;

namespace HoU.GuildBot.Shared.BLL
{
    public interface ISpamGuard
    {
        SpamCheckResult CheckForSpam(ulong userId, ulong channelId, string message, int attachmentsCount);
    }
}