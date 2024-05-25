namespace HoU.GuildBot.Shared.BLL;

public interface IUnitsBotClient
{
    // See UnitsSignalRClient.RegisterHandlers
    [UsedImplicitly]
    Task ReceiveEventCreatedMessageAsync(Uri baseAddress,
                                         int appointmentId,
                                         string eventName,
                                         string author,
                                         DateTime startTime,
                                         DateTime endTime,
                                         bool isAllDay,
                                         [CanBeNull] string cardUrl);

    // See UnitsSignalRClient.RegisterHandlers
    [UsedImplicitly]
    Task ReceiveEventRescheduledMessageAsync(Uri baseAddress,
                                             int appointmentId,
                                             string eventName,
                                             DateTime startTimeOld,
                                             DateTime endTimeOld,
                                             DateTime startTimeNew,
                                             DateTime endTimeNew,
                                             bool isAllDay,
                                             ulong[] usersToNotify);

    // See UnitsSignalRClient.RegisterHandlers
    [UsedImplicitly]
    Task ReceiveEventCanceledMessageAsync(Uri baseAddress,
                                          string eventName,
                                          DateTime startTime,
                                          DateTime endTime,
                                          bool isAllDay,
                                          ulong[] usersToNotify);

    // See UnitsSignalRClient.RegisterHandlers
    [UsedImplicitly]
    Task ReceiveEventAttendanceConfirmedMessageAsync(Uri baseAddress,
                                                     int appointmentId,
                                                     string eventName,
                                                     ulong[] usersToNotify);

    // See UnitsSignalRClient.RegisterHandlers
    [UsedImplicitly]
    Task ReceiveEventStartingSoonMessageAsync(Uri baseAddress,
                                              int appointmentId,
                                              DateTime startTime,
                                              ulong[] usersToNotify);

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