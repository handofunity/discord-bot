namespace HoU.GuildBot.Shared.BLL;

public interface IBirthdayProvider
{
    Task<string> SetBirthdayAsync(DiscordUserId userId,
                             short month,
                             short day);

    Task<string> DeleteBirthdayAsync(DiscordUserId userId);

    Task<DiscordUserId[]> GetBirthdaysAsync(DateOnly forDate);
}