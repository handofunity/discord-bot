namespace HoU.GuildBot.BLL;

[UsedImplicitly]
public class ReminderService
{
    private readonly IDynamicConfiguration _dynamicConfiguration;
    private readonly IDiscordAccess _discordAccess;

    public ReminderService(IDynamicConfiguration dynamicConfiguration,
                           IDiscordAccess discordAccess)
    {
        _dynamicConfiguration = dynamicConfiguration ?? throw new ArgumentNullException(nameof(dynamicConfiguration));
        _discordAccess = discordAccess ?? throw new ArgumentNullException(nameof(discordAccess));
    }

    public async Task SendReminderAsync(int reminderId)
    {
        var reminderInfo = _dynamicConfiguration.ScheduledReminderInfos.SingleOrDefault(m => m.ReminderId == reminderId);
        if (reminderInfo == null)
            return;

        var (channelID, message) = reminderInfo.GetReminderDetails();
        await _discordAccess.CreateBotMessagesInChannelAsync(channelID, new[] { message });
    }
}