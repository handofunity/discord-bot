namespace HoU.GuildBot.BLL;

[UsedImplicitly]
public class SpamGuard : ISpamGuard
{
    private readonly IDynamicConfiguration _dynamicConfiguration;
    private readonly Queue<string> _recentMessage;

    private Dictionary<DiscordChannelId, (int SoftCap, int HardCap)> _limits;
    private DiscordChannelId[] _excludedChannelIds;

    public SpamGuard(IDynamicConfiguration dynamicConfiguration)
    {
        _dynamicConfiguration = dynamicConfiguration;
        _dynamicConfiguration.DataLoaded -= DynamicConfiguration_DataLoaded;
        _dynamicConfiguration.DataLoaded += DynamicConfiguration_DataLoaded;
        _recentMessage = new Queue<string>(25);
        (_excludedChannelIds, _limits) = LoadLimitsFromConfig();
    }

    private (DiscordChannelId[] ExcludecChannelIds, Dictionary<DiscordChannelId, (int SoftCap, int HardCap)> Limits) LoadLimitsFromConfig()
    {
        var excludedChannelIds = _dynamicConfiguration.SpamLimits
                                                      .Where(m => m.SoftCap == -1 && m.HardCap == -1)
                                                      .Select(m => m.RestrictToChannelId)
                                                      .ToArray();
        var limits = _dynamicConfiguration.SpamLimits
                                          .Where(m => excludedChannelIds.All(e => e != m.RestrictToChannelId))
                                          .ToDictionary(m => m.RestrictToChannelId, m => (m.SoftCap, m.HardCap));

        return (excludedChannelIds, limits);
    }

    private void DynamicConfiguration_DataLoaded(object? sender, EventArgs e)
    {
        (_excludedChannelIds, _limits) = LoadLimitsFromConfig();
    }

    SpamCheckResult ISpamGuard.CheckForSpam(DiscordUserId userId, DiscordChannelId channelId, string message, int attachmentsCount)
    {
        // If the spam protection is disabled for the channel, there's no need to do any further checks.
        // The whole channel won't count into the queue.
        if (_excludedChannelIds.Contains(channelId))
            return SpamCheckResult.NoSpam;

        if (string.IsNullOrEmpty(message) && attachmentsCount > 0)
        {
            // Attachment upload without optional comment
            return SpamCheckResult.NoSpam;
        }

        var builtMessage = $"{{{userId}}}-{{{channelId}}}-{{{message}}}";

        // If the maximum size is hit, remove the oldest message
        if (_recentMessage.Count == 25)
            _recentMessage.Dequeue();

        // Add the new message
        _recentMessage.Enqueue(builtMessage);

        // Get limits
        if (!_limits.TryGetValue(channelId, out var limits)) limits = _limits[(DiscordChannelId)0];

        // Check the message for soft and hard limit
        var messageCount = _recentMessage.Count(m => m == builtMessage);
        if (messageCount >= limits.HardCap)
            return SpamCheckResult.HardLimitHit;
        if (messageCount > limits.SoftCap && messageCount < limits.HardCap)
            return SpamCheckResult.BetweenSoftAndHardLimit;
        if (messageCount == limits.SoftCap)
            return SpamCheckResult.SoftLimitHit;
        return SpamCheckResult.NoSpam;
    }
}