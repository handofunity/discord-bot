namespace HoU.GuildBot.Shared.BLL;

public interface IScheduledReminderProvider
{
    public Task<EmbedData[]> GetAllReminderInfosAsync();
}