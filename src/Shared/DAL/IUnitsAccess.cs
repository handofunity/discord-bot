namespace HoU.GuildBot.Shared.DAL;

public interface IUnitsAccess
{
    /// <summary>
    /// Sends the <paramref name="createdVoiceChannelsRequest"/> to the UNIT system.
    /// </summary>
    /// <param name="unitsEndpoint">The data used to sync with the UNIT system.</param>
    /// <param name="createdVoiceChannelsRequest">The createdVoiceChannelsRequest containing the information about the created voice channels</param>
    /// <returns>An awaitable <see cref="Task"/>.</returns>
    Task SendCreatedVoiceChannelsAsync(UnitsEndpoint unitsEndpoint,
                                       SyncCreatedVoiceChannelsRequest createdVoiceChannelsRequest);

    /// <summary>
    /// Sends the <paramref name="currentAttendeesRequest"/> to the UNIT system.
    /// </summary>
    /// <param name="unitsEndpoint"></param>
    /// <param name="currentAttendeesRequest"></param>
    /// <returns></returns>
    Task SendCurrentAttendeesAsync(UnitsEndpoint unitsEndpoint,
                                   SyncCurrentAttendeesRequest currentAttendeesRequest);
}