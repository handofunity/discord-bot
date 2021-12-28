using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HoU.GuildBot.DAL.Database.Model;
using HoU.GuildBot.Shared.DAL;
using HoU.GuildBot.Shared.Objects;
using HoU.GuildBot.Shared.StrongTypes;
using Microsoft.EntityFrameworkCore;
using DesiredTimeZone = HoU.GuildBot.Shared.Objects.DesiredTimeZone;
using PersonalReminder = HoU.GuildBot.Shared.Objects.PersonalReminder;
using UnitsEndpoint = HoU.GuildBot.Shared.Objects.UnitsEndpoint;

namespace HoU.GuildBot.DAL.Database;

public class ConfigurationDatabaseAccess : IConfigurationDatabaseAccess
{
    private readonly DbContextOptions<HandOfUnityContext> _handOfUnityContextOptions;

    public ConfigurationDatabaseAccess(RootSettings rootSettings)
    {
        var builder = new DbContextOptionsBuilder<HandOfUnityContext>();
        builder.UseSqlServer(rootSettings.ConnectionStringForOwnDatabase);
        builder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        _handOfUnityContextOptions = builder.Options;
    }

    private HandOfUnityContext GetDbContext() => new(_handOfUnityContextOptions);

    async Task<UnitsEndpoint[]> IConfigurationDatabaseAccess.GetAllUnitsEndpointsAsync()
    {
        await using var entities = GetDbContext();
        var dbEntries = await entities.UnitsEndpoint.ToArrayAsync();
        return dbEntries.Select(m => new UnitsEndpoint
                         {
                             BaseAddress = m.BaseAddress,
                             Secret = m.Secret,
                             ConnectToRestApi = m.ConnectToRestApi,
                             ConnectToNotificationHub = m.ConnectToNotificationsHub
                         })
                        .ToArray();
    }

    async Task<DesiredTimeZone[]> IConfigurationDatabaseAccess.GetAllDesiredTimeZonesAsync()
    {
        await using var entities = GetDbContext();
        var dbEntries = await entities.DesiredTimeZone.ToArrayAsync();
        return dbEntries.Select(m => new DesiredTimeZone
                         {
                             TimeZoneId = m.DesiredTimeZoneKey,
                             InvariantDisplayName = m.InvariantDisplayName
                         })
                        .ToArray();
    }

    async Task<PersonalReminder[]> IConfigurationDatabaseAccess.GetAllPersonalRemindersAsync()
    {
        await using var entities = GetDbContext();
        var dbEntries = await entities.PersonalReminder.ToArrayAsync();
        return dbEntries.Select(m => new PersonalReminder
                         {
                             ReminderId = m.PersonalReminderID,
                             CronSchedule = m.CronSchedule,
                             Channel = (ulong)m.DiscordChannelID,
                             Remind = (ulong)m.UserToRemind,
                             Text = m.Text
                         })
                        .ToArray();
    }

    async Task<Dictionary<string, ulong>> IConfigurationDatabaseAccess.GetFullDiscordMappingAsync()
    {
        await using var entities = GetDbContext();
        var dbEntries = await entities.DiscordMapping.ToArrayAsync();
        return dbEntries.ToDictionary(m => m.DiscordMappingKey,
                                      m => (ulong)m.DiscordID);
    }

    async Task<SpamLimit[]> IConfigurationDatabaseAccess.GetAllSpamProtectedChannelsAsync()
    {
        await using var entities = GetDbContext();
        var dbEntries = await entities.SpamProtectedChannel.ToArrayAsync();
        return dbEntries.Select(m => new SpamLimit
                         {
                             RestrictToChannelID = (DiscordChannelID)(ulong)m.SpamProtectedChannelID,
                             SoftCap = m.SoftCap,
                             HardCap = m.HardCap
                         })
                        .ToArray();
    }
}