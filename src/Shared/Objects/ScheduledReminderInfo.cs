using System;
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
    public (DiscordChannelId ChannelID, string Message) GetReminderDetails()
    {
        var channelId = Channel;
        var mentions = GetMentions();
        var message = $"{mentions}: {Text}";
        return (channelId, message);

        string GetMentions()
        {
            if (RemindUsers.Length == 0 && RemindRoles.Length == 0)
                throw new InvalidOperationException($"Reminder {ReminderId} has no mentions. At least one mention is required.");
            
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