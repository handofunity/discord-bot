namespace HoU.GuildBot.BLL;

public class ScheduledReminderProvider : IScheduledReminderProvider
{
    private readonly IConfigurationDatabaseAccess _configurationDatabaseAccess;

    public ScheduledReminderProvider(IConfigurationDatabaseAccess configurationDatabaseAccess)
    {
        _configurationDatabaseAccess = configurationDatabaseAccess;
    }

    async Task<EmbedData[]> IScheduledReminderProvider.GetAllReminderInfosAsync()
    {
        var reminders = await _configurationDatabaseAccess.GetAllScheduledReminderInfosAsync();
        var result = new EmbedData[reminders.Length];

        for (var i = 0; i < reminders.Length; i++)
        {
            var reminder = reminders[i];

            result[i] = new EmbedData
            {
                Title = $"ScheduledReminder {reminder.ReminderId}",
                Fields = new[]
                {
                    new EmbedField("Cron expression", reminder.CronSchedule, true),
                    new EmbedField("Next occurrence (UTC)",
                                   CronExpression.Parse(reminder.CronSchedule).GetNextOccurrence(DateTime.UtcNow)?.ToString("s")
                                ?? "<unknown>",
                                   true),
                    new EmbedField("Text", reminder.Text, false),
                    new EmbedField("User mentions", reminder.RemindUsers.Any() ? string.Join(", ", reminder.RemindUsers.Select(m => m.ToString())) : "<none>", false),
                    new EmbedField("Role mentions", reminder.RemindRoles.Any() ? string.Join(", ", reminder.RemindRoles.Select(m => m.ToString())) : "<none>", false)
                }
            };
        }

        return result;
    }
}