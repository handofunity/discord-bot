using System;
using System.Threading.Tasks;
using HoU.GuildBot.Shared.StrongTypes;

namespace HoU.GuildBot.Shared.BLL
{
    public interface IUnitsBotClient
    {
        Task ReceiveEventCreatedMessageAsync(string baseAddress,
                                             int appointmentId,
                                             string eventName,
                                             DateTime startTime,
                                             DateTime endTime,
                                             bool isAllDay);

        Task ReceiveEventRescheduledMessageAsync(string baseAddress, 
                                                 int appointmentId,
                                                 string eventName,
                                                 DateTime startTimeOld,
                                                 DateTime endTimeOld,
                                                 DateTime startTimeNew,
                                                 DateTime endTimeNew,
                                                 bool isAllDay,
                                                 DiscordUserID[] usersToNotify);

        Task ReceiveEventCanceledMessageAsync(string eventName,
                                              DateTime startTime,
                                              DateTime endTime,
                                              bool isAllDay,
                                              DiscordUserID[] usersToNotify);
    }
}