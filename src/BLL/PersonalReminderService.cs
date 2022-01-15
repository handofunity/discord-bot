using System;
using System.Linq;
using System.Threading.Tasks;
using Hangfire.Annotations;
using HoU.GuildBot.Shared.BLL;
using HoU.GuildBot.Shared.DAL;

namespace HoU.GuildBot.BLL;

[UsedImplicitly]
public class PersonalReminderService
{
    private readonly IDynamicConfiguration _dynamicConfiguration;
    private readonly IDiscordAccess _discordAccess;

    public PersonalReminderService(IDynamicConfiguration dynamicConfiguration,
                                   IDiscordAccess discordAccess)
    {
        _dynamicConfiguration = dynamicConfiguration ?? throw new ArgumentNullException(nameof(dynamicConfiguration));
        _discordAccess = discordAccess ?? throw new ArgumentNullException(nameof(discordAccess));
    }

    public async Task SendReminderAsync(int reminderId)
    {
        var reminder = _dynamicConfiguration.PersonalReminders.SingleOrDefault(m => m.ReminderId == reminderId);
        if (reminder == null)
            return;

        var (channelID, message) = reminder.GetReminderInfo();
        await _discordAccess.CreateBotMessagesInChannelAsync(channelID, new[] {message});
    }
}