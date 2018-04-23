namespace HoU.GuildBot.Shared.BLL
{
    using Enums;

    public interface ISpamGuard
    {
        SpamCheckResult CheckForSpam(ulong userId, ulong channelId, string message);
    }
}