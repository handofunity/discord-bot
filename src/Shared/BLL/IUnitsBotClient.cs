namespace HoU.GuildBot.Shared.BLL;

public interface IUnitsBotClient
{
    // See UnitsSignalRClient.RegisterHandlers
    [UsedImplicitly]
    Task ReceiveEventCreatedMessageAsync(Uri baseAddress,
        int appointmentId,
        string eventName,
        string author,
        DateTimeOffset startTime,
        DateTimeOffset endTime,
        bool isAllDay,
        [CanBeNull] string cardUrl);

    // See UnitsSignalRClient.RegisterHandlers
    [UsedImplicitly]
    Task ReceiveEventRescheduledMessageAsync(Uri baseAddress,
        int appointmentId,
        string eventName,
        string author,
        DateTimeOffset startTimeOld,
        DateTimeOffset endTimeOld,
        DateTimeOffset startTimeNew,
        DateTimeOffset endTimeNew,
        bool isAllDay,
        string? cardUrl,
        ulong? originalThreadId,
        ulong[] usersToNotify);

    // See UnitsSignalRClient.RegisterHandlers
    [UsedImplicitly]
    Task ReceiveEventCanceledMessageAsync(ulong threadId,
        ulong[] usersToNotify);

    // See UnitsSignalRClient.RegisterHandlers
    [UsedImplicitly]
    Task ReceiveEventAttendanceConfirmedMessageAsync(Uri baseAddress,
        int appointmentId,
        ulong threadId,
        ulong[] usersToNotify);

    // See UnitsSignalRClient.RegisterHandlers
    [UsedImplicitly]
    Task ReceiveEventStartingSoonMessageAsync(Uri baseAddress,
        int appointmentId,
        DateTimeOffset startTime,
        ulong threadId,
        ulong[] usersToNotify,
        ulong? generalVoiceChannelId);

    // See UnitsSignalRClient.RegisterHandlers
    [UsedImplicitly]
    Task ReceiveCreateEventVoiceChannelsMessageAsync(Uri baseAddress,
        int appointmentId,
        bool createGeneralVoiceChannel,
        byte maxAmountOfGroups,
        byte? maxGroupSize);

    // See UnitsSignalRClient.RegisterHandlers
    [UsedImplicitly]
    Task ReceiveDeleteEventVoiceChannelsMessageAsync(string[] voiceChannelIds);

    // See UnitsSignalRClient.RegisterHandlers
    [UsedImplicitly]
    Task ReceiveGetCurrentAttendeesMessageAsync(Uri baseAddress,
        int appointmentId,
        int checkNumber,
        string[] voiceChannelIds);
}