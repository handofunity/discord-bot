using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HoU.GuildBot.Shared.BLL;
using HoU.GuildBot.Shared.DAL;
using HoU.GuildBot.Shared.Objects;

namespace HoU.GuildBot.BLL
{
    public class UnitsBotClient : IUnitsBotClient
    {
        private readonly IDiscordAccess _discordAccess;

        public UnitsBotClient(IDiscordAccess discordAccess)
        {
            _discordAccess = discordAccess ?? throw new ArgumentNullException(nameof(discordAccess));
        }

        async Task IUnitsBotClient.ReceiveEventCreatedMessageAsync(string baseAddress,
                                                                   int appointmentId,
                                                                   string eventName,
                                                                   DateTime startTime,
                                                                   DateTime endTime,
                                                                   bool isAllDay)
        {
            var fields = new List<EmbedField>
            {
                new EmbedField("Title", eventName, false)
            };
            var duration = isAllDay ? endTime.Date.AddDays(1) - startTime.Date : endTime - startTime;
            if (isAllDay)
            {
                if (duration.Days > 1)
                {
                    fields.Add(new EmbedField("Start Date", startTime.Date.ToString("yyyy-MM-dd"), false));
                    fields.Add(new EmbedField("End Date", endTime.Date.ToString("yyyy-MM-dd"), false));
                    fields.Add(new EmbedField("Duration", $"{duration} day", false));
                }
                else
                {
                    fields.Add(new EmbedField("Date", startTime.Date.ToString("yyyy-MM-dd"), false));
                    fields.Add(new EmbedField("Duration", "1 day", false));
                }
            }
            else
            {
                fields.Add(new EmbedField("Start Time", startTime.ToString("yyyy-MM-dd HH:mm:ss \"UTC\"zzz"), false));
                fields.Add(new EmbedField("End Time", endTime.ToString("yyyy-MM-dd HH:mm:ss \"UTC\"zzz"), false));
                fields.Add(new EmbedField("Duration", $"{duration} h", false));
            }
            fields.Add(new EmbedField("Details", $"{baseAddress}/events/{appointmentId}", false));
            var embed = new EmbedData
            {
                Title = "New event created",
                Color = Colors.Green,
                Description = "A new event was created in UNITS.",
                Fields = fields.ToArray()
            };
            await _discordAccess.SendUnitsNotificationAsync(embed);
        }
    }
}