using System.Collections.Generic;
using System.Threading.Tasks;
using HoU.GuildBot.Shared.Objects;

namespace HoU.GuildBot.Shared.DAL;

public interface IConfigurationDatabaseAccess
{
    Task<UnitsEndpoint[]> GetAllUnitsEndpointsAsync();

    Task<DesiredTimeZone[]> GetAllDesiredTimeZonesAsync();

    Task<PersonalReminder[]> GetAllPersonalRemindersAsync();

    Task<Dictionary<string, ulong>> GetFullDiscordMappingAsync();

    Task<SpamLimit[]> GetAllSpamProtectedChannelsAsync();
}