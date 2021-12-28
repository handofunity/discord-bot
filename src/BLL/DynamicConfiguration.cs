using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using HoU.GuildBot.Shared.BLL;
using HoU.GuildBot.Shared.DAL;
using HoU.GuildBot.Shared.Objects;

namespace HoU.GuildBot.BLL;

public class DynamicConfiguration : IDynamicConfiguration
{
    private readonly IConfigurationDatabaseAccess _configurationDatabaseAccess;

    private EventHandler<EventArgs>? _dataLoaded;
    private UnitsEndpoint[]? _unitsEndpoints;
    private DesiredTimeZone[]? _desiredTimeZones;
    private PersonalReminder[]? _personalReminders;
    private Dictionary<string, ulong>? _discordMapping;
    private SpamLimit[]? _spamLimits;

    public DynamicConfiguration(IConfigurationDatabaseAccess? configurationDatabaseAccess)
    {
        _configurationDatabaseAccess = configurationDatabaseAccess ?? throw new ArgumentNullException(nameof(configurationDatabaseAccess));
    }

    private static InvalidOperationException InvalidAccess([CallerMemberName] string? propertyName = null)
    {
        return new InvalidOperationException($"{propertyName} was accessed before {nameof(IDynamicConfiguration.LoadAllDataAsync)} was invoked.");
    }

    event EventHandler<EventArgs> IDynamicConfiguration.DataLoaded
    {
        add => _dataLoaded += value;
        remove => _dataLoaded -= value;
    }

    UnitsEndpoint[] IDynamicConfiguration.UnitsEndpoints => _unitsEndpoints ?? throw InvalidAccess();

    DesiredTimeZone[] IDynamicConfiguration.DesiredTimeZones => _desiredTimeZones ?? throw InvalidAccess();

    PersonalReminder[] IDynamicConfiguration.PersonalReminders => _personalReminders ?? throw InvalidAccess();

    Dictionary<string, ulong> IDynamicConfiguration.DiscordMapping => _discordMapping ?? throw InvalidAccess();

    SpamLimit[] IDynamicConfiguration.SpamLimits => _spamLimits ?? throw InvalidAccess();

    async Task IDynamicConfiguration.LoadAllDataAsync()
    {
        _unitsEndpoints = await _configurationDatabaseAccess.GetAllUnitsEndpointsAsync();
        _desiredTimeZones = await _configurationDatabaseAccess.GetAllDesiredTimeZonesAsync();
        _personalReminders = await _configurationDatabaseAccess.GetAllPersonalRemindersAsync();
        _discordMapping = await _configurationDatabaseAccess.GetFullDiscordMappingAsync();
        _spamLimits = await _configurationDatabaseAccess.GetAllSpamProtectedChannelsAsync();
        _dataLoaded?.Invoke(this, EventArgs.Empty);
    }
}