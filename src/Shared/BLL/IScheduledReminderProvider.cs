using System.Threading.Tasks;
using HoU.GuildBot.Shared.Objects;

namespace HoU.GuildBot.Shared.BLL;

public interface IScheduledReminderProvider
{
    public Task<EmbedData[]> GetAllReminderInfosAsync();
}