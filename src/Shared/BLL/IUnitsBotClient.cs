﻿namespace HoU.GuildBot.Shared.BLL;

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
        ulong categoryChannelId);

    // See UnitsSignalRClient.RegisterHandlers
    [UsedImplicitly]
    Task ReceiveCreateEventVoiceChannelsMessageAsync(Uri baseAddress,
        int appointmentId,
        bool createGeneralVoiceChannel,
        byte maxAmountOfGroups,
        byte? maxGroupSize);

    // See UnitsSignalRClient.RegisterHandlers
    [UsedImplicitly]
    Task ReceiveDeleteEventCategoryChannelMessageAsync(ulong categoryChannelId);

    // See UnitsSignalRClient.RegisterHandlers
    [UsedImplicitly]
    Task ReceiveGetCurrentAttendeesMessageAsync(Uri baseAddress,
        int appointmentId,
        int checkNumber,
        ulong categoryChannelId);

    Task ReceiveRequisitionOrderCreatedMessageAsync(Uri baseAddress,
        int requisitionOrderId,
        string purpose,
        string creator,
        string importance,
        DateTimeOffset dueTime);

    Task ReceiveRequisitionOrderClosedMessageAsync(Uri baseAddress,
        int requisitionOrderId,
        ulong threadId,
        bool newDeliveriesRejected,
        bool acceptedDeliveriesRejected,
        ulong[] usersToNotify);

    Task ReceiveDeliveryCreatedMessageAsync(Uri baseAddress,
        int requisitionOrderId,
        ulong threadId,
        string deliverer);

    Task ReceiveDeliveryAcceptedMessageAsync(Uri baseAddress,
        int requisitionOrderId,
        long deliveryId,
        ulong threadId,
        ulong userToNotify);

    Task ReceiveDeliveryRejectedMessageAsync(Uri baseAddress,
        int requisitionOrderId,
        long deliveryId,
        ulong threadId,
        ulong userToNotify);
}