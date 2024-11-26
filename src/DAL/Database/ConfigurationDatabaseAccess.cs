﻿using DesiredTimeZone = HoU.GuildBot.Shared.Objects.DesiredTimeZone;
using KeycloakEndpoint = HoU.GuildBot.Shared.Objects.KeycloakEndpoint;
using UnitsEndpoint = HoU.GuildBot.Shared.Objects.UnitsEndpoint;

namespace HoU.GuildBot.DAL.Database;

public class ConfigurationDatabaseAccess : IConfigurationDatabaseAccess
{
    private readonly DbContextOptions<HandOfUnityContext> _handOfUnityContextOptions;

    public ConfigurationDatabaseAccess(RootSettings rootSettings)
    {
        var builder = new DbContextOptionsBuilder<HandOfUnityContext>();
        builder.UseNpgsql(rootSettings.ConnectionStringForOwnDatabase);
        builder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        _handOfUnityContextOptions = builder.Options;
    }

    private HandOfUnityContext GetDbContext() => new(_handOfUnityContextOptions);

    private static ScheduledReminderInfo Map(ScheduledReminder sr) =>
        new(sr.ScheduledReminderId,
            sr.CronSchedule,
            (DiscordChannelId)(ulong)sr.DiscordChannelId,
            sr.ScheduledReminderMention.Where(mention => mention.DiscordUserId != null)
              .Select(mention => (DiscordUserId)mention.DiscordUserId.GetValueOrDefault())
              .ToArray(),
            sr.ScheduledReminderMention.Where(mention => mention.DiscordRoleId != null)
              .Select(mention => (DiscordRoleId)mention.DiscordRoleId.GetValueOrDefault())
              .ToArray(),
            sr.Text);

    private static KeycloakEndpoint Map(DAL.Database.Model.KeycloakEndpoint keycloakEndpoint) =>
        new(new Uri(keycloakEndpoint.BaseUrl),
            new Uri(keycloakEndpoint.AccessTokenUrl),
            keycloakEndpoint.ClientId,
            keycloakEndpoint.ClientSecret,
            keycloakEndpoint.Realm);

    async Task<UnitsEndpoint[]> IConfigurationDatabaseAccess.GetAllUnitsEndpointsAsync()
    {
        await using var entities = GetDbContext();
        var dbEntries = await entities.UnitsEndpoint
                                      .Include(m => m.KeycloakEndpoint)
                                      .AsNoTrackingWithIdentityResolution()
                                      .ToArrayAsync();
        return dbEntries.Select(m => new UnitsEndpoint(m.UnitsEndpointId,
                new Uri(m.BaseAddress),
                m.ConnectToRestApi,
                m.ConnectToNotificationsHub,
                Map(m.KeycloakEndpoint!),
                m.NewEventPingDiscordRoleId is null
                    ? null
                    : (DiscordRoleId)(ulong)m.NewEventPingDiscordRoleId,
                m.Chapter))
            .ToArray();
    }

    async Task<KeycloakEndpoint[]> IConfigurationDatabaseAccess.GetAllKeycloakEndpointsAsync()
    {
        await using var entities = GetDbContext();
        var dbEntries = await entities.KeycloakEndpoint.AsNoTracking().ToArrayAsync();
        return dbEntries.Select(Map).ToArray();
    }

    async Task<DesiredTimeZone[]> IConfigurationDatabaseAccess.GetAllDesiredTimeZonesAsync()
    {
        await using var entities = GetDbContext();
        var dbEntries = await entities.DesiredTimeZone.ToArrayAsync();
        return dbEntries.Select(m => new DesiredTimeZone(m.DesiredTimeZoneKey,
                                                         m.InvariantDisplayName))
                        .ToArray();
    }

    async Task<ScheduledReminderInfo[]> IConfigurationDatabaseAccess.GetAllScheduledReminderInfosAsync()
    {
        await using var entities = GetDbContext();
        var dbEntries = await entities.ScheduledReminder
                                      .Include(m => m.ScheduledReminderMention)
                                      .ToArrayAsync();
        return dbEntries.Select(Map).ToArray();
    }

    async Task<Dictionary<string, ulong>> IConfigurationDatabaseAccess.GetFullDiscordMappingAsync()
    {
        await using var entities = GetDbContext();
        var dbEntries = await entities.DiscordMapping.ToArrayAsync();
        return dbEntries.ToDictionary(m => m.DiscordMappingKey,
                                      m => (ulong)m.DiscordId);
    }

    async Task<SpamLimit[]> IConfigurationDatabaseAccess.GetAllSpamProtectedChannelsAsync()
    {
        await using var entities = GetDbContext();
        var dbEntries = await entities.SpamProtectedChannel.ToArrayAsync();
        return dbEntries.Select(m => new SpamLimit
                         {
                             RestrictToChannelId = (DiscordChannelId)(ulong)m.SpamProtectedChannelId,
                             SoftCap = m.SoftCap,
                             HardCap = m.HardCap
                         })
                        .ToArray();
    }

    async Task<int> IConfigurationDatabaseAccess.CreateScheduledReminderAsync(ScheduledReminderInfo scheduledReminderInfo)
    {
        await using var entities = GetDbContext();
        var sr = new ScheduledReminder
        {
            CronSchedule = scheduledReminderInfo.CronSchedule,
            DiscordChannelId = (ulong)scheduledReminderInfo.Channel,
            Text = scheduledReminderInfo.Text
        };
        entities.ScheduledReminder.Add(sr);
        await entities.SaveChangesAsync();
        return sr.ScheduledReminderId;
    }

    async Task IConfigurationDatabaseAccess.UpdateScheduledReminderAsync(ScheduledReminderInfo scheduledReminderInfo)
    {
        await using var entities = GetDbContext();
        var entity = await entities.ScheduledReminder
                                   .AsTracking()
                                   .FirstAsync(m => m.ScheduledReminderId == scheduledReminderInfo.ReminderId);

        entity.CronSchedule = scheduledReminderInfo.CronSchedule;
        entity.DiscordChannelId = (decimal)scheduledReminderInfo.Channel;
        entity.Text = scheduledReminderInfo.Text;

        await entities.SaveChangesAsync();
    }

    async Task<ScheduledReminderInfo?> IConfigurationDatabaseAccess.GetScheduledReminderInfosAsync(int scheduledReminderId)
    {
        await using var entities = GetDbContext();
        var result = await entities.ScheduledReminder
                                   .Include(m => m.ScheduledReminderMention)
                                   .FirstOrDefaultAsync(m => m.ScheduledReminderId == scheduledReminderId);
        return result == null
                   ? null
                   : Map(result) ;
    }

    async Task IConfigurationDatabaseAccess.AddScheduledReminderMentionAsync(int scheduledReminderId,
                                                                             DiscordUserId discordUserId)
    {
        await using var entities = GetDbContext();
        entities.ScheduledReminderMention.Add(new ScheduledReminderMention
        {
            ScheduledReminderId = scheduledReminderId,
            DiscordUserId = (ulong)discordUserId
        });

        await entities.SaveChangesAsync();
    }

    async Task IConfigurationDatabaseAccess.AddScheduledReminderMentionAsync(int scheduledReminderId,
                                                                             DiscordRoleId discordRoleId)
    {
        await using var entities = GetDbContext();
        entities.ScheduledReminderMention.Add(new ScheduledReminderMention
        {
            ScheduledReminderId = scheduledReminderId,
            DiscordRoleId = (ulong)discordRoleId
        });

        await entities.SaveChangesAsync();
    }
}