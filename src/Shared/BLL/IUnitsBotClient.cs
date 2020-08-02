using System;
using System.Threading.Tasks;

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
    }
}