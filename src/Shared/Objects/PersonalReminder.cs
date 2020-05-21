using HoU.GuildBot.Shared.Extensions;
using HoU.GuildBot.Shared.StrongTypes;

namespace HoU.GuildBot.Shared.Objects
{
    public class PersonalReminder
    {
        /// <summary>
        /// Gets or sets the reminder ID.
        /// </summary>
        public int ReminderId { get; set; }

        /// <summary>
        /// Gets or sets the CRON schedule for this reminder.
        /// </summary>
        public string CronSchedule { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="DiscordChannelID"/> (as <see cref="ulong"/> of the channel to post the reminder in.
        /// </summary>
        public ulong Channel { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="DiscordUserID"/> (as <see cref="ulong"/>) of the user to remind.
        /// </summary>
        public ulong Remind { get; set; }

        /// <summary>
        /// Gets or sets the text of the reminder.
        /// </summary>
        public string Text { get; set; }

        public (DiscordChannelID ChannelID, string Message) GetReminderInfo()
        {
            var channelId = (DiscordChannelID) Channel;
            var userId = (DiscordUserID) Remind;
            var mention = userId.ToMention();
            var message = $"{mention}: {Text}";
            return (channelId, message);
        }
    }
}