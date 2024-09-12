using System.Diagnostics.CodeAnalysis;

namespace HoU.GuildBot.BLL;

public class UnitsBotClient(IDiscordAccess _discordAccess,
                            IUnitsAccess _unitsAccess,
                            IDynamicConfiguration _dynamicConfiguration,
                            TimeProvider _timeProvider,
                            ILogger<UnitsBotClient> _logger)
    : IUnitsBotClient
{
    private static string GetDayOfMonthSuffix(int day)
    {
        if (day is >= 11 and <= 13)
            return "th";
        return (day % 10) switch
        {
            1 => "st",
            2 => "nd",
            3 => "rd",
            _ => "th"
        };
    }

    private static EmbedData GetEventEmbed(Uri baseAddress,
                                           string actionOrTitle,
                                           RGB color)
    {
        var iconUrl = GetIconUrl(baseAddress);
        return new EmbedData
        {
            Author = "UNITS: Events",
            AuthorUrl = GetEventsUrl(baseAddress),
            AuthorIconUrl = iconUrl,
            ThumbnailUrl = iconUrl,
            Title = $":calendar_spiral: {actionOrTitle}",
            Color = color
        };
    }

    private static string GetIconUrl(Uri baseAddress) => $"{baseAddress}images/logo.png";

    private static string GetEventsUrl(Uri baseAddress) => $"{baseAddress}events";

    private static string GetEventUrl(Uri baseAddress,
                                      int appointmentId) =>
        $"{baseAddress}events/{appointmentId}";

    private static void AddTimeField(List<EmbedField> fields,
                                     DateTimeOffset startTime,
                                     DateTimeOffset endTime,
                                     bool isAllDay,
                                     string? fieldTitlePostfix)
    {
        var startTimeUtc = startTime.ToUniversalTime();
        var endTimeUtc = endTime.ToUniversalTime();
        // Time
        var duration = isAllDay ? endTimeUtc.Date.AddDays(1) - startTimeUtc.Date : endTimeUtc - startTimeUtc;
        var startTimeUnix = startTimeUtc - DateTimeOffset.UnixEpoch;
        var endTimeUnix = endTimeUtc - DateTimeOffset.UnixEpoch;
        if (isAllDay)
        {
            var communityTimeString = new StringBuilder(startTimeUtc.ToString("ddd MMM dd"));
            communityTimeString.Append(GetDayOfMonthSuffix(startTimeUtc.Day) + ", ");
            communityTimeString.Append(startTimeUtc.ToString("yyyy"));
            var localTimeString = new StringBuilder($"<t:{startTimeUnix.TotalSeconds}:D>");
            if (duration.Days > 1)
            {
                communityTimeString.Append(" - ");
                communityTimeString.Append(endTimeUtc.ToString("ddd MMM dd"));
                communityTimeString.Append(GetDayOfMonthSuffix(endTimeUtc.Day) + ", ");
                communityTimeString.Append(endTimeUtc.ToString("yyyy"));
                localTimeString.Append(" - ");
                localTimeString.Append($"<t:{endTimeUnix.TotalSeconds}:D>");
            }

            fields.Add(new EmbedField("Community Time" + fieldTitlePostfix, communityTimeString.ToString(), false));
            fields.Add(new EmbedField("Local Time" + fieldTitlePostfix, localTimeString.ToString(), false));
        }
        else
        {
            var communityTimeString = new StringBuilder(startTimeUtc.ToString("ddd MMM dd"));
            communityTimeString.Append(GetDayOfMonthSuffix(startTimeUtc.Day) + ", ");
            communityTimeString.Append(startTimeUtc.ToString("yyyy"));
            communityTimeString.Append(" ⋅ ");
            communityTimeString.Append(startTimeUtc.ToString("h:mm tt"));
            communityTimeString.Append(" - ");
            communityTimeString.Append(endTimeUtc.ToString("h:mm tt"));
            communityTimeString.Append(" UTC");
            var localTimeString = $"<t:{startTimeUnix.TotalSeconds}:F> - <t:{endTimeUnix.TotalSeconds}:t>";
            fields.Add(new EmbedField("Community Time" + fieldTitlePostfix, communityTimeString.ToString(), false));
            fields.Add(new EmbedField("Local Time" + fieldTitlePostfix, localTimeString.ToString(), false));
        }
    }

    private bool TryGetUnitsEndpoint(Uri baseAddress,
                                     [NotNullWhen(true)] out UnitsEndpoint? unitsEndpoint)
    {
        unitsEndpoint = _dynamicConfiguration.UnitsEndpoints.SingleOrDefault(m => m.BaseAddress == baseAddress);
        if (unitsEndpoint is not null)
            return true;

        _logger.LogError($"Cannot find matching {nameof(IDynamicConfiguration.UnitsEndpoints)} " +
                             "for base address {BaseAddress}", baseAddress);
        return false;
    }

    private static DiscordUserId[] ToDiscordUserIds(ulong[]? userIds)
    {
        if (userIds is null)
            return [];
        return userIds.Distinct().Select(m => (DiscordUserId)m).ToArray();
    }

    private async Task<DiscordChannelId?> SendUnitsNotificationAsync(UnitsEndpoint unitsEndpoint,
        string threadName,
        EmbedData embed,
        DiscordUserId[]? usersToNotify = null)
    {
        if (usersToNotify is not null && usersToNotify.Length > 0)
        {
            return await _discordAccess.SendUnitsNotificationAsync(unitsEndpoint.UnitsEndpointId,
                threadName,
                embed,
                usersToNotify);
        }
        else
        {
            return await _discordAccess.SendUnitsNotificationAsync(unitsEndpoint.UnitsEndpointId,
                threadName,
                embed);
        }
    }

    private async Task SendUnitsNotificationAsync(DiscordChannelId threadId,
        string message,
        DiscordUserId[]? usersToNotify = null)
    {
        if (usersToNotify is not null && usersToNotify.Length > 0)
        {
            await _discordAccess.SendUnitsNotificationAsync(threadId,
                message,
                usersToNotify);
        }
        else
        {
            await _discordAccess.SendUnitsNotificationAsync(threadId,
                message);
        }
    }

    async Task IUnitsBotClient.ReceiveEventCreatedMessageAsync(Uri baseAddress,
        int appointmentId,
        string eventName,
        string author,
        DateTimeOffset startTime,
        DateTimeOffset endTime,
        bool isAllDay,
        string cardUrl)
    {
        if (!TryGetUnitsEndpoint(baseAddress, out var unitsEndpoint))
            return;

        _logger.LogDebug("Received EventCreatedMessage for event \"{EventName}\" (Id: {AppointmentId})",
                         eventName,
                         appointmentId);
        var fields = new List<EmbedField>();
        AddTimeField(fields, startTime, endTime, isAllDay, null);
        var embed = GetEventEmbed(baseAddress,
                                  eventName,
                                  Colors.BrightBlue);
        embed.Url = GetEventUrl(baseAddress, appointmentId);
        embed.Description = "A new event was created in UNITS. " +
                            $"[Click here to open the event in your browser]({embed.Url}).";
        embed.Fields = [.. fields];
        embed.FooterText = $"Created by {author}";
        if (cardUrl != null)
        {
            var imageUrl = $"{baseAddress}/{cardUrl}";
            _logger.LogDebug("Setting image URL of embed to: {ImageUrl}", imageUrl);
            embed.ImageUrl = imageUrl;
        }
        else
        {
            _logger.LogDebug("Image URL for event {AppointmentId} is null",
                             appointmentId);
        }
        var threadId = await SendUnitsNotificationAsync(unitsEndpoint, eventName, embed);
        if (threadId is not null)
            await _unitsAccess.SendCreatedThreadIdAsync(unitsEndpoint, appointmentId, threadId.Value);
    }

    async Task IUnitsBotClient.ReceiveEventRescheduledMessageAsync(Uri baseAddress,
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
        ulong[] usersToNotify)
    {
        if (!TryGetUnitsEndpoint(baseAddress, out var unitsEndpoint))
            return;

        var fields = new List<EmbedField>();
        AddTimeField(fields, startTimeOld, endTimeOld, isAllDay, " (Old)");
        AddTimeField(fields, startTimeNew, endTimeNew, isAllDay, " (New)");
        var embed = GetEventEmbed(baseAddress,
                                  eventName,
                                  Colors.Orange);
        embed.Url = GetEventUrl(baseAddress, appointmentId);
        embed.Description = "An existing event was rescheduled to a new occurence. " +
                            "If you're being mentioned, you've signed up to the old occurence. " +
                           $"[Please open the new event occurence in your browser]({embed.Url}) and to sign-up again!";
        embed.Fields = [.. fields];
        embed.FooterText = $"Rescheduled by {author}";
        if (cardUrl != null)
        {
            var imageUrl = $"{baseAddress}/{cardUrl}";
            _logger.LogDebug("Setting image URL of embed to: {ImageUrl}", imageUrl);
            embed.ImageUrl = imageUrl;
        }
        else
        {
            _logger.LogDebug("Image URL for event {AppointmentId} is null",
                             appointmentId);
        }

        if (originalThreadId is not null)
            await _discordAccess.ArchiveThreadAsync((DiscordChannelId)originalThreadId.Value);
        var userIds = ToDiscordUserIds(usersToNotify);
        var threadId = await SendUnitsNotificationAsync(unitsEndpoint, eventName, embed, userIds);
        if (threadId is not null)
        {
            await _discordAccess.TryAddUsersToThreadAsync(threadId.Value, userIds);
            await _unitsAccess.SendCreatedThreadIdAsync(unitsEndpoint, appointmentId, threadId.Value);
        }
    }

    async Task IUnitsBotClient.ReceiveEventCanceledMessageAsync(ulong threadId,
        ulong[] usersToNotify)
    {
        const string message = "This event was canceled in UNITS. " +
            "If you're being mentioned, you've signed up to the canceled event.";
        var userIds = ToDiscordUserIds(usersToNotify);
        await SendUnitsNotificationAsync((DiscordChannelId)threadId, message, userIds);
    }

    async Task IUnitsBotClient.ReceiveEventAttendanceConfirmedMessageAsync(Uri baseAddress,
        int appointmentId,
        ulong threadId,
        ulong[] usersToNotify)
    {
        var discordThreadId = (DiscordChannelId)threadId;
        var url = GetEventUrl(baseAddress, appointmentId);
        var message = "Your event attendance has been confirmed. " +
            $"[Please open the event in your browser]({url}) to **check your assigned group (or benched state)**.";
        var userIds = ToDiscordUserIds(usersToNotify);
        await SendUnitsNotificationAsync(discordThreadId, message, userIds);
        await _discordAccess.TryAddUsersToThreadAsync(discordThreadId, userIds);
    }

    async Task IUnitsBotClient.ReceiveEventStartingSoonMessageAsync(Uri baseAddress,
        int appointmentId,
        DateTimeOffset startTime,
        ulong threadId,
        ulong[] usersToNotify)
    {
        var url = GetEventUrl(baseAddress, appointmentId);
        var relativeStartTime = (startTime - DateTimeOffset.UnixEpoch).TotalSeconds;
        var message = $"Your event is starting <t:{relativeStartTime}:R>. " +
            $"[Open the event in your browser]({url}).";
        var userIds = ToDiscordUserIds(usersToNotify);
        await SendUnitsNotificationAsync((DiscordChannelId)threadId, message, userIds);
    }

    async Task IUnitsBotClient.ReceiveCreateEventVoiceChannelsMessageAsync(Uri baseAddress,
        int appointmentId,
        bool createGeneralVoiceChannel,
        byte maxAmountOfGroups,
        byte? maxGroupSize)
    {
        if (!TryGetUnitsEndpoint(baseAddress, out var unitsEndpoint))
            return;

        var voiceChannels = new List<EventVoiceChannel>();
        if (createGeneralVoiceChannel)
            voiceChannels.Add(new EventVoiceChannel(appointmentId));
        if (maxAmountOfGroups > 0 && maxGroupSize != null)
            for (byte groupNumber = 1; groupNumber <= maxAmountOfGroups; groupNumber++)
                voiceChannels.Add(new EventVoiceChannel(appointmentId, groupNumber, maxGroupSize.Value));

        var failedVoiceChannels = new List<EventVoiceChannel>();
        foreach (var eventVoiceChannel in voiceChannels)
        {
            var (voiceChannelId, error) =
                await _discordAccess.CreateVoiceChannelAsync((DiscordChannelId)_dynamicConfiguration.DiscordMapping["VoiceChannelCategoryId"],
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
                                                      (DiscordChannelId)_dynamicConfiguration.DiscordMapping
                                                          ["UnitsEventVoiceChannelsPositionAboveChannelId"]);

            await _unitsAccess.SendCreatedVoiceChannelsAsync(unitsEndpoint!,
                                                             new SyncCreatedVoiceChannelsRequest(appointmentId, voiceChannels));
        }
    }

    async Task IUnitsBotClient.ReceiveDeleteEventVoiceChannelsMessageAsync(string[] voiceChannelIds)
    {
        if (voiceChannelIds == null)
            return;

        foreach (var voiceChannelIdStr in voiceChannelIds)
        {
            if (!ulong.TryParse(voiceChannelIdStr, out var voiceChannelId))
                continue;

            try
            {
                await _discordAccess.DeleteVoiceChannelAsync((DiscordChannelId)voiceChannelId);
                _logger.LogInformation("Deleted voice channel {VoiceChannelId}", voiceChannelId);
            }
            catch (Exception e)
            {
                _logger.LogWarning(e,
                                   "Failed to delete voice channel {VoiceChannelId}",
                                   voiceChannelId);
            }
        }
    }

    async Task IUnitsBotClient.ReceiveGetCurrentAttendeesMessageAsync(Uri baseAddress,
        int appointmentId,
        int checkNumber,
        string[] voiceChannelIds)
    {
        if (voiceChannelIds == null || voiceChannelIds.Length == 0)
            return;

        if (!TryGetUnitsEndpoint(baseAddress, out var unitsEndpoint))
            return;

        var voiceChannelUsers = _discordAccess.GetUsersInVoiceChannels(voiceChannelIds);
        if (voiceChannelUsers.Any())
        {
            var request = new SyncCurrentAttendeesRequest(appointmentId,
                                                          checkNumber,
                                                          voiceChannelUsers.Select(m => new VoiceChannelAttendees(m.Key,
                                                                                        m.Value.ConvertAll(l => (ulong)l)))
                                                                           .ToList());
            await _unitsAccess.SendCurrentAttendeesAsync(unitsEndpoint!,
                                                         request);
        }
    }
}