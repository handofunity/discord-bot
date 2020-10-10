using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HoU.GuildBot.Shared.BLL;
using HoU.GuildBot.Shared.DAL;
using HoU.GuildBot.Shared.Objects;
using HoU.GuildBot.Shared.StrongTypes;
using Microsoft.Extensions.Logging;

namespace HoU.GuildBot.BLL
{
    public class UnitsBotClient : IUnitsBotClient
    {
        private readonly IDiscordAccess _discordAccess;
        private readonly IUnitsAccess _unitsAccess;
        private readonly AppSettings _appSettings;
        private readonly ILogger<UnitsBotClient> _logger;

        public UnitsBotClient(IDiscordAccess discordAccess,
                              IUnitsAccess unitsAccess,
                              AppSettings appSettings,
                              ILogger<UnitsBotClient> logger)
        {
            _discordAccess = discordAccess ?? throw new ArgumentNullException(nameof(discordAccess));
            _unitsAccess = unitsAccess ?? throw new ArgumentNullException(nameof(unitsAccess));
            _appSettings = appSettings ?? throw new ArgumentNullException(nameof(appSettings));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private static EmbedData GetEventEmbed(string baseAddress,
                                               string action,
                                               RGB color)
        {
            var iconUrl = GetIconUrl(baseAddress);
            return new EmbedData
            {
                Author = "UNITS: Events",
                AuthorUrl = GetEventsUrl(baseAddress),
                AuthorIconUrl = iconUrl,
                ThumbnailUrl = iconUrl,
                Title = $":calendar_spiral: {action}",
                Color = color
            };
        }

        private static string GetIconUrl(string baseAddress) => $"{baseAddress}/images/logo.png";

        private static string GetEventsUrl(string baseAddress) => $"{baseAddress}/events";

        private static string GetEventUrl(string baseAddress,
                                          int appointmentId) =>
            $"{baseAddress}/events/{appointmentId}";

        private static void AddTimeRelatedFields(List<EmbedField> fields,
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
                    fields.Add(new EmbedField(":calendar: Start Date" + postfix, startTime.Date.ToString("yyyy-MM-dd"), false));
                    fields.Add(new EmbedField(":calendar: End Date" + postfix, endTime.Date.ToString("yyyy-MM-dd"), false));
                    fields.Add(new EmbedField(":stopwatch: Duration" + postfix, $"{duration} day", false));
                }
                else
                {
                    fields.Add(new EmbedField(":calendar: Date" + postfix, startTime.Date.ToString("yyyy-MM-dd"), false));
                    fields.Add(new EmbedField(":stopwatch: Duration" + postfix, "1 day", false));
                }
            }
            else
            {
                fields.Add(new EmbedField(":watch: Start Time" + postfix, startTime.ToString("yyyy-MM-dd HH:mm:ss \"UTC\"zzz"), false));
                fields.Add(new EmbedField(":watch: End Time" + postfix, endTime.ToString("yyyy-MM-dd HH:mm:ss \"UTC\"zzz"), false));
                fields.Add(new EmbedField(":stopwatch: Duration" + postfix, $"{duration} h", false));
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
                new EmbedField(":bookmark: Title", eventName, false)
            };
            AddTimeRelatedFields(fields, startTime, endTime, isAllDay, null);
            var embed = GetEventEmbed(baseAddress,
                                      "Event created",
                                      Colors.BrightBlue);
            embed.Url = GetEventUrl(baseAddress, appointmentId);
            embed.Description = $"A [new event]({embed.Url}) was created in UNITS. " +
                                "Click to open the event in your browser.";
            embed.Fields = fields.ToArray();
            await _discordAccess.SendUnitsNotificationAsync(embed);
        }

        async Task IUnitsBotClient.ReceiveEventRescheduledMessageAsync(string baseAddress, 
                                                                       int appointmentId,
                                                                       string eventName,
                                                                       DateTime startTimeOld,
                                                                       DateTime endTimeOld,
                                                                       DateTime startTimeNew,
                                                                       DateTime endTimeNew,
                                                                       bool isAllDay,
                                                                       DiscordUserID[] usersToNotify)
        {
            var fields = new List<EmbedField>
            {
                new EmbedField(":bookmark: Title", eventName, false)
            };
            AddTimeRelatedFields(fields, startTimeOld, endTimeOld, isAllDay, " (Old)");
            AddTimeRelatedFields(fields, startTimeNew, endTimeNew, isAllDay, " (New)");
            var embed = GetEventEmbed(baseAddress,
                                      "Event rescheduled",
                                      Colors.Orange);
            embed.Url = GetEventUrl(baseAddress, appointmentId);
            embed.Description = $"An existing event was [rescheduled to a new occurence]({embed.Url}). " +
                                "If you're being mentioned, you've signed up to the old occurence. " +
                                "Click to open the new event occurence in your browser and to sign-up again!";
            embed.Fields = fields.ToArray();

            if (usersToNotify != null && usersToNotify.Any())
            {
                await _discordAccess.SendUnitsNotificationAsync(embed, usersToNotify);
            }
            else
            {
                await _discordAccess.SendUnitsNotificationAsync(embed);
            }
        }

        async Task IUnitsBotClient.ReceiveEventCanceledMessageAsync(string baseAddress,
                                                                    string eventName,
                                                                    DateTime startTime,
                                                                    DateTime endTime,
                                                                    bool isAllDay,
                                                                    DiscordUserID[] usersToNotify)
        {
            var fields = new List<EmbedField>
            {
                new EmbedField(":bookmark: Title", eventName, false)
            };
            AddTimeRelatedFields(fields, startTime, endTime, isAllDay, null);
            var embed = GetEventEmbed(baseAddress,
                                      "Event canceled",
                                      Colors.Red);
            embed.Description = "An existing event was canceled in UNITS.";
            embed.Fields = fields.ToArray();

            if (usersToNotify != null && usersToNotify.Any())
            {
                await _discordAccess.SendUnitsNotificationAsync(embed, usersToNotify);
            }
            else
            {
                await _discordAccess.SendUnitsNotificationAsync(embed);
            }
        }

        async Task IUnitsBotClient.ReceiveEventAttendanceConfirmedMessageAsync(string baseAddress,
                                                                               int appointmentId,
                                                                               DiscordUserID userToNotify)
        {
            var embed = GetEventEmbed(baseAddress,
                                      "Event attendance confirmed",
                                      Colors.Green);
            embed.Url = GetEventUrl(baseAddress, appointmentId);
            embed.Description = $"Your [event attendance]({embed.Url}) has been confirmed. " +
                                "Click to open the event in your browser.";
            await _discordAccess.SendUnitsNotificationAsync(embed, new[] {userToNotify});
        }

        async Task IUnitsBotClient.ReceiveEventStartingSoonMessageAsync(string baseAddress,
                                                                        int appointmentId,
                                                                        DateTime startTime,
                                                                        DiscordUserID[] usersToNotify)
        {
            var minutes = (int)(startTime - DateTime.UtcNow).TotalMinutes;
            var embed = GetEventEmbed(baseAddress,
                                      "Event starting soon",
                                      Colors.LightOrange);
            embed.Url = GetEventUrl(baseAddress, appointmentId);
            embed.Description = $"Your [event]({embed.Url}) is starting in {minutes} minutes. " +
                                "Click to open the event in your browser.";

            if (usersToNotify != null && usersToNotify.Any())
            {
                await _discordAccess.SendUnitsNotificationAsync(embed, usersToNotify);
            }
            else
            {
                await _discordAccess.SendUnitsNotificationAsync(embed);
            }
        }

        async Task IUnitsBotClient.ReceiveCreateEventVoiceChannelsMessageAsync(string baseAddress,
                                                                               int appointmentId,
                                                                               bool createGeneralVoiceChannel,
                                                                               byte maxAmountOfGroups,
                                                                               byte? maxGroupSize)
        {
            var unitsSyncData = _appSettings.UnitsAccess.SingleOrDefault(m => m.BaseAddress == baseAddress);
            if (unitsSyncData == null)
            {
                _logger.LogError($"Cannot find matching sync-endpoint in {nameof(AppSettings)}.{nameof(AppSettings.UnitsAccess)} " +
                                 "for base address {BaseAddress}.", baseAddress);
                return;
            }

            var voiceChannels = new List<EventVoiceChannel>();
            if (createGeneralVoiceChannel)
                voiceChannels.Add(new EventVoiceChannel(appointmentId));
            if (maxAmountOfGroups > 0 && maxGroupSize != null)
                for (byte groupNumber = 1; groupNumber <= maxAmountOfGroups; groupNumber++)
                    voiceChannels.Add(new EventVoiceChannel(appointmentId, groupNumber, maxGroupSize.Value));

            var failedVoiceChannels = new List<EventVoiceChannel>();
            foreach (var eventVoiceChannel in voiceChannels)
            {
                var (voiceChannelId, error) = await _discordAccess.CreateVoiceChannel(_appSettings.VoiceChannelCategoryId,
                                                                                      eventVoiceChannel.DisplayName,
                                                                                      eventVoiceChannel.MaxUsersInChannel);
                if (error != null)
                {
                    failedVoiceChannels.Add(eventVoiceChannel);
                    _logger.LogWarning("Failed to create voice channel for event '{AppointmentId}: {Error}'",
                                       appointmentId,
                                       error);
                }
                else
                {
                    eventVoiceChannel.DiscordVoiceChannelIdValue = voiceChannelId;
                }
                await Task.Delay(200);
            }

            foreach (var eventVoiceChannel in failedVoiceChannels)
                voiceChannels.Remove(eventVoiceChannel);

            if (voiceChannels.Any())
            {
                await _discordAccess.ReorderChannelsAsync(voiceChannels.Select(m => m.DiscordVoiceChannelIdValue).ToArray(),
                                                     _appSettings.UnitsEventVoiceChannelsPositionAboveChannelId);

                await _unitsAccess.SendCreatedVoiceChannelsAsync(unitsSyncData,
                                                              new SyncCreatedVoiceChannelsRequest(appointmentId, voiceChannels));
            }
        }
    }
}