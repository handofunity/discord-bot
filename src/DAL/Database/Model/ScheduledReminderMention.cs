using System;
using System.Collections.Generic;

namespace HoU.GuildBot.DAL.Database.Model
{
    public partial class ScheduledReminderMention
    {
        public int ScheduledReminderMentionId { get; set; }
        public int ScheduledReminderId { get; set; }
        public decimal? DiscordUserId { get; set; }
        public decimal? DiscordRoleId { get; set; }

        public virtual ScheduledReminder? ScheduledReminder { get; set; } = null!;
    }
}
