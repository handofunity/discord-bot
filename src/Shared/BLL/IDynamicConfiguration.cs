namespace HoU.GuildBot.Shared.BLL;

public interface IDynamicConfiguration
{
    /// <summary>
    /// Gets invoked at the end of <see cref="LoadAllDataAsync"/>.
    /// Use this event handler to refresh dependent business logic.
    /// </summary>
    event EventHandler<EventArgs> DataLoaded;

    UnitsEndpoint[] UnitsEndpoints { get; }
    
    KeycloakEndpoint[] KeycloakEndpoints { get; }

    DesiredTimeZone[] DesiredTimeZones { get; }

    ScheduledReminderInfo[] ScheduledReminderInfos { get; }

    SpamLimit[] SpamLimits { get; }

    Dictionary<string, ulong> DiscordMapping { get; }

    /// <summary>
    /// Wipes all properties and fills them with the data from the database.
    /// </summary>
    /// <returns>An awaitable <see cref="Task"/>.</returns>
    Task LoadAllDataAsync();

    /// <summary>
    /// Wipes the <see cref="ScheduledReminderInfos"/> property and fills it with the data from the database.
    /// </summary>
    /// <returns>An awaitable <see cref="Task"/>.</returns>
    Task LoadScheduledReminderInfosAsync();
}