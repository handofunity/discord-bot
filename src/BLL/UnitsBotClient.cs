using System;
using System.Threading.Tasks;
using HoU.GuildBot.Shared.BLL;

namespace HoU.GuildBot.BLL
{
    public class UnitsBotClient : IUnitsBotClient
    {
        async Task IUnitsBotClient.ReceiveEventCreatedMessageAsync(int appointmentId,
                                                                   string eventName,
                                                                   DateTime startTime,
                                                                   DateTime endTime,
                                                                   bool isAllDay)
        {
            throw new NotImplementedException();
        }
    }
}