using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HoU.GuildBot.Shared.BLL;
using HoU.GuildBot.Shared.DAL;
using HoU.GuildBot.Shared.Objects;
using HoU.GuildBot.Shared.StrongTypes;

namespace HoU.GuildBot.BLL
{
    public class UnitsBotClient : IUnitsBotClient
    {
        private readonly IDiscordAccess _discordAccess;

        public UnitsBotClient(IDiscordAccess discordAccess)
        {
            _discordAccess = discordAccess ?? throw new ArgumentNullException(nameof(discordAccess));
        }

        private void AddTimeRelatedFields(List<EmbedField> fields,
                                          DateTime startTime,
                                          DateTime endTime,
                                          bool isAllDay,
                                          string postfix)
        {
            var duration = isAllDay ? endTime.Date.AddDays(1) - startTime.Date : endTime - startTime;
            if (isAllDay)
            {
                if (duration.Days > 1)
                {
                    fields.Add(new EmbedField("Start Date" + postfix, startTime.Date.ToString("yyyy-MM-dd"), false));
                    fields.Add(new EmbedField("End Date" + postfix, endTime.Date.ToString("yyyy-MM-dd"), false));
                    fields.Add(new EmbedField("Duration" + postfix, $"{duration} day", false));
                }
                else
                {
                    fields.Add(new EmbedField("Date" + postfix, startTime.Date.ToString("yyyy-MM-dd"), false));
                    fields.Add(new EmbedField("Duration" + postfix, "1 day", false));
                }
            }
            else
            {
                fields.Add(new EmbedField("Start Time" + postfix, startTime.ToString("yyyy-MM-dd HH:mm:ss \"UTC\"zzz"), false));
                fields.Add(new EmbedField("End Time" + postfix, endTime.ToString("yyyy-MM-dd HH:mm:ss \"UTC\"zzz"), false));
                fields.Add(new EmbedField("Duration" + postfix, $"{duration} h", false));
            }
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
            AddTimeRelatedFields(fields, startTime, endTime, isAllDay, null);
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

        async Task IUnitsBotClient.ReceiveEventRescheduledMessageAsync(string baseAddress, 
                                                                       int appointmentId,
                                                                       string eventName,
                                                                       DateTime startTimeOld,
                                                                       DateTime endTimeOld,
                                                                       DateTime startTimeNew,
                                                                       DateTime endTimeNew,
                                                                       bool isAllDay)
        {
            var fields = new List<EmbedField>
            {
                new EmbedField("Title", eventName, false)
            };
            AddTimeRelatedFields(fields, startTimeOld, endTimeOld, isAllDay, " (Old)");
            AddTimeRelatedFields(fields, startTimeNew, endTimeNew, isAllDay, " (New)");
            fields.Add(new EmbedField("Details", $"{baseAddress}/events/{appointmentId}", false));
            var embed = new EmbedData
            {
                Title = "Event was rescheduled",
                Color = Colors.LightOrange,
                Description = "An existing event was rescheduled in UNITS.",
                Fields = fields.ToArray()
            };
            await _discordAccess.SendUnitsNotificationAsync(embed);
        }

        public async Task ReceiveEventCanceledMessageAsync(string eventName,
                                                           DateTime startTime,
                                                           DateTime endTime,
                                                           bool isAllDay,
                                                           DiscordUserID[] usersToNotify)
        {
            var fields = new List<EmbedField>
            {
                new EmbedField("Title", eventName, false)
            };
            AddTimeRelatedFields(fields, startTime, endTime, isAllDay, null);
            var embed = new EmbedData
            {
                Title = "Event canceled",
                Color = Colors.Red,
                Description = "An event was canceled in UNITS.",
                Fields = fields.ToArray()
            };
            await _discordAccess.SendUnitsNotificationAsync(embed, usersToNotify);
        }
    }
}