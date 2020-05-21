using System;
using System.Linq;
using System.Threading.Tasks;
using Hangfire.Annotations;
using HoU.GuildBot.Shared.DAL;
using HoU.GuildBot.Shared.Objects;

namespace HoU.GuildBot.BLL
{
    [UsedImplicitly]
    public class PersonalReminderService
    {
        private readonly AppSettings _appSettings;
        private readonly IDiscordAccess _discordAccess;

        public PersonalReminderService(AppSettings appSettings,
                                       IDiscordAccess discordAccess)
        {
            _appSettings = appSettings ?? throw new ArgumentNullException(nameof(appSettings));
            _discordAccess = discordAccess ?? throw new ArgumentNullException(nameof(discordAccess));
        }

        public async Task SendReminderAsync(int reminderId)
        {
            var reminder = _appSettings.PersonalReminders?.SingleOrDefault(m => m.ReminderId == reminderId);
            if (reminder == null)
                return;

            var (channelID, message) = reminder.GetReminderInfo();
            await _discordAccess.CreateBotMessagesInChannel(channelID, new[] {message});
        }
    }
}