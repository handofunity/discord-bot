using System.Collections.Generic;
using System.Threading.Tasks;
using HoU.GuildBot.Shared.Objects;
using HoU.GuildBot.Shared.StrongTypes;

namespace HoU.GuildBot.Shared.DAL;

public interface IConfigurationDatabaseAccess
{
    Task<UnitsEndpoint[]> GetAllUnitsEndpointsAsync();

    Task<DesiredTimeZone[]> GetAllDesiredTimeZonesAsync();

    Task<ScheduledReminderInfo[]> GetAllScheduledReminderInfosAsync();

    Task<Dictionary<string, ulong>> GetFullDiscordMappingAsync();

    Task<SpamLimit[]> GetAllSpamProtectedChannelsAsync();

    /// <summary>
    /// Creates a scheduled reminder info in the database <b>without</b> any mentions.
    /// </summary>
    /// <param name="scheduledReminderInfo">The info that will be persisted in the database.</param>
    /// <returns>The Id of the created <see cref="ScheduledReminderInfo"/>. Should be used to add mentions.</returns>
    Task<int> CreateScheduledReminderAsync(ScheduledReminderInfo scheduledReminderInfo);

    /// <summary>
    /// Tries to get the <see cref="ScheduledReminderInfo"/> for the given <paramref name="scheduledReminderId"/>.
    /// </summary>
    /// <param name="scheduledReminderId">The Id of the <see cref="ScheduledReminderInfo"/> to look up.</param>
    /// <returns>The matching <see cref="ScheduledReminderInfo"/> if found, otherwise <b>null</b>.</returns>
    Task<ScheduledReminderInfo?> GetScheduledReminderInfosAsync(int scheduledReminderId);

    /// <summary>
    /// Adds the <paramref name="discordUserId"/> as a mention for the given <paramref name="scheduledReminderId"/>.
    /// </summary>
    /// <param name="scheduledReminderId">The Id of the reminder.</param>
    /// <param name="discordUserId">The Id of the user to mention.</param>
    /// <returns>Any error or success message.</returns>
    Task AddScheduledReminderMentionAsync(int scheduledReminderId, DiscordUserId discordUserId);

    /// <summary>
    /// Adds the <paramref name="discordRoleId"/> as a mention for the given <paramref name="scheduledReminderId"/>.
    /// </summary>
    /// <param name="scheduledReminderId">The Id of the reminder.</param>
    /// <param name="discordRoleId">The Id of the role to mention.</param>
    /// <returns>Any error or success message.</returns>
    Task AddScheduledReminderMentionAsync(int scheduledReminderId, DiscordRoleId discordRoleId);
}