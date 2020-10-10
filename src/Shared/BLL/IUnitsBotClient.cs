using System;
using System.Threading.Tasks;
using HoU.GuildBot.Shared.StrongTypes;
using JetBrains.Annotations;

namespace HoU.GuildBot.Shared.BLL
{
    public interface IUnitsBotClient
    {
        // See UnitsSignalRClient.RegisterHandlers
        [UsedImplicitly]
        Task ReceiveEventCreatedMessageAsync(string baseAddress,
                                             int appointmentId,
                                             string eventName,
                                             DateTime startTime,
                                             DateTime endTime,
                                             bool isAllDay);

        // See UnitsSignalRClient.RegisterHandlers
        [UsedImplicitly]
        Task ReceiveEventRescheduledMessageAsync(string baseAddress, 
                                                 int appointmentId,
                                                 string eventName,
                                                 DateTime startTimeOld,
                                                 DateTime endTimeOld,
                                                 DateTime startTimeNew,
                                                 DateTime endTimeNew,
                                                 bool isAllDay,
                                                 DiscordUserID[] usersToNotify);

        // See UnitsSignalRClient.RegisterHandlers
        [UsedImplicitly]
        Task ReceiveEventCanceledMessageAsync(string baseAddress,
                                              string eventName,
                                              DateTime startTime,
                                              DateTime endTime,
                                              bool isAllDay,
                                              DiscordUserID[] usersToNotify);

        // See UnitsSignalRClient.RegisterHandlers
        [UsedImplicitly]
        Task ReceiveEventAttendanceConfirmedMessageAsync(string baseAddress,
                                                         int appointmentId,
                                                         DiscordUserID userToNotify);

        // See UnitsSignalRClient.RegisterHandlers
        [UsedImplicitly]
        Task ReceiveEventStartingSoonMessageAsync(string baseAddress,
                                                  int appointmentId,
                                                  DateTime startTime,
                                                  DiscordUserID[] usersToNotify);

        // See UnitsSignalRClient.RegisterHandlers
        [UsedImplicitly]
        Task ReceiveCreateEventVoiceChannelsMessageAsync(string baseAddress,
                                                         int appointmentId,
                                                         bool createGeneralVoiceChannel,
                                                         byte maxAmountOfGroups,
                                                         byte? maxGroupSize);
    }
}