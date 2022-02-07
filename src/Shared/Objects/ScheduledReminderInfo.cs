using System.Text;
using HoU.GuildBot.Shared.Extensions;
using HoU.GuildBot.Shared.StrongTypes;

namespace HoU.GuildBot.Shared.Objects;

public record ScheduledReminderInfo(int ReminderId,
                                    string CronSchedule,
                                    DiscordChannelId Channel,
                                    DiscordUserId[] RemindUsers,
                                    DiscordRoleId[] RemindRoles,
                                    string Text)
{
    /// <summary>
    /// Gets or sets the reminder ID.
    /// </summary>
    public int ReminderId { get; } = ReminderId;

    /// <summary>
    /// Gets or sets the CRON schedule for this reminder.
    /// </summary>
    public string CronSchedule { get; } = CronSchedule;

    /// <summary>
    /// Gets or sets the <see cref="DiscordChannelId"/> of the channel to post the reminder in.
    /// </summary>
    public DiscordChannelId Channel { get; } = Channel;

    /// <summary>
    /// Gets or sets the <see cref="DiscordUserId"/>s of the users to remind.
    /// </summary>
    public DiscordUserId[] RemindUsers { get; } = RemindUsers;

    /// <summary>
    /// Gets or sets the <see cref="DiscordRoleId"/>s of the roles to remind.
    /// </summary>
    public DiscordRoleId[] RemindRoles { get; } = RemindRoles;

    /// <summary>
    /// Gets or sets the text of the reminder.
    /// </summary>
    public string Text { get; } = Text;

    public (DiscordChannelId ChannelID, string Message) GetReminderDetails()
    {
        var channelId = Channel;
        var mentions = GetMentions();
        var message = $"{mentions}: {Text}";
        return (channelId, message);

        string GetMentions()
        {
            var sb = new StringBuilder();

            foreach (var discordUserId in RemindUsers)
                sb.Append($"{discordUserId.ToMention()} ");

            foreach (var discordRoleId in RemindRoles)
                sb.Append($"{discordRoleId.ToMention()} ");

            // Remove the last whitespace.
            if (sb.Length > 0)
                sb.Remove(sb.Length - 1, 1);

            return sb.ToString();
        }
    }
}