using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using HoU.GuildBot.Shared.BLL;
using HoU.GuildBot.Shared.Objects;
using HoU.GuildBot.Shared.StrongTypes;

namespace HoU.GuildBot.BLL;

[UsedImplicitly]
public class IgnoreGuard : IIgnoreGuard
{
    private readonly IBotInformationProvider _botInformationProvider;
    private readonly Dictionary<DiscordUserId, DateTime> _ignoreList;
    
    public IgnoreGuard(IBotInformationProvider botInformationProvider)
    {
        _botInformationProvider = botInformationProvider;
        _ignoreList = new Dictionary<DiscordUserId, DateTime>();
    }
    
    EmbedData IIgnoreGuard.EnsureOnIgnoreList(DiscordUserId userID,
                                              string username,
                                              int minutes)
    {
        // Update or insert value
        var ignoreUntil = DateTime.Now.ToUniversalTime().AddMinutes(minutes);
        _ignoreList[userID] = ignoreUntil;
        return new EmbedData
        {
            Title = "Added to ignore list",
            Color = Colors.Green,
            Description = $"**{username}**: You will be ignored for the next {minutes} minutes "
                        + $"(until {ignoreUntil:dd.MM.yyyy HH:mm:ss} UTC) in the "
                        + $"environment **{_botInformationProvider.GetEnvironmentName()}**."
        };
    }

    bool IIgnoreGuard.TryRemoveFromIgnoreList(DiscordUserId userID,
                                              string username,
                                              out EmbedData? embedData)
    {
        if (!_ignoreList.ContainsKey(userID))
        {
            embedData = null;
            return false;
        }

        _ignoreList.Remove(userID);
        embedData = new EmbedData
        {
            Title = "Removed from ignore list",
            Color = Colors.Green,
            Description = $"**{username}**: You will no longer be ignored in the environment **{_botInformationProvider.GetEnvironmentName()}**."
        };
        return true;
    }

    bool IIgnoreGuard.ShouldIgnore(DiscordUserId userID)
    {
        if (!_ignoreList.TryGetValue(userID, out var ignoreUntil)) return false;
        if (ignoreUntil > DateTime.UtcNow)
            return true;
        _ignoreList.Remove(userID);
        return false;
    }
}