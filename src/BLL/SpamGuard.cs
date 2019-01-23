namespace HoU.GuildBot.BLL
{
    using System.Collections.Generic;
    using System.Linq;
    using JetBrains.Annotations;
    using Shared.BLL;
    using Shared.Enums;
    using Shared.Objects;

    [UsedImplicitly]
    public class SpamGuard : ISpamGuard
    {
        private readonly Dictionary<ulong, (byte SoftCap, byte HardCap)> _limits;
        private readonly Queue<string> _recentMessage;

        public SpamGuard(AppSettings appSettings)
        {
            _limits = appSettings.SpamLimits.ToDictionary(m => m.RestrictToChannelID ?? 0, m => (m.SoftCap, m.HardCap));
            _recentMessage = new Queue<string>(25);
        }

        SpamCheckResult ISpamGuard.CheckForSpam(ulong userId, ulong channelId, string message, int attachmentsCount)
        {
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
            if (!_limits.TryGetValue(channelId, out var limits)) limits = _limits[0];

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
}