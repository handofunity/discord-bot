using HoU.GuildBot.Shared.Extensions;
using HoU.GuildBot.Shared.StrongTypes;

namespace HoU.GuildBot.Shared.Objects;

public record PersonalReminder
{
    /// <summary>
    /// Gets or sets the reminder ID.
    /// </summary>
    public int ReminderId { get; }

    /// <summary>
    /// Gets or sets the CRON schedule for this reminder.
    /// </summary>
    public string CronSchedule { get; }

    /// <summary>
    /// Gets or sets the <see cref="DiscordChannelId"/> (as <see cref="ulong"/> of the channel to post the reminder in.
    /// </summary>
    public DiscordChannelId Channel { get; }

    /// <summary>
    /// Gets or sets the <see cref="DiscordUserId"/> (as <see cref="ulong"/>) of the user to remind.
    /// </summary>
    public DiscordUserId Remind { get; }

    /// <summary>
    /// Gets or sets the text of the reminder.
    /// </summary>
    public string Text { get; }

    public PersonalReminder(int reminderId,
                            string cronSchedule,
                            DiscordChannelId channel,
                            DiscordUserId remind,
                            string text)
    {
        ReminderId = reminderId;
        CronSchedule = cronSchedule;
        Channel = channel;
        Remind = remind;
        Text = text;
    }

    public (DiscordChannelId ChannelID, string Message) GetReminderInfo()
    {
        var channelId = Channel;
        var userId = Remind;
        var mention = userId.ToMention();
        var message = $"{mention}: {Text}";
        return (channelId, message);
    }
}