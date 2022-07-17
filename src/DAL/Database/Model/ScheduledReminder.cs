namespace HoU.GuildBot.DAL.Database.Model
{
    public partial class ScheduledReminder
    {
        public ScheduledReminder()
        {
            ScheduledReminderMention = new HashSet<ScheduledReminderMention>();
        }

        public int ScheduledReminderId { get; set; }
        public string CronSchedule { get; set; } = null!;
        public decimal DiscordChannelId { get; set; }
        public string Text { get; set; } = null!;

        public virtual ICollection<ScheduledReminderMention> ScheduledReminderMention { get; set; }
    }
}
