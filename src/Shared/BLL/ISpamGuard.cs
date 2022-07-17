namespace HoU.GuildBot.Shared.BLL;

public interface ISpamGuard
{
    SpamCheckResult CheckForSpam(DiscordUserId userId,
                                 DiscordChannelId channelId,
                                 string message,
                                 int attachmentsCount);
}