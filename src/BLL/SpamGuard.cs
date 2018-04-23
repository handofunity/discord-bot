namespace HoU.GuildBot.BLL
{
    using System.Collections.Generic;
    using System.Linq;
    using JetBrains.Annotations;
    using Shared.BLL;
    using Shared.Enums;

    [UsedImplicitly]
    public class SpamGuard : ISpamGuard
    {
        private const int _SOFT_LIMIT = 5;
        private const int _HARD_LIMIT = 7;

        private readonly Queue<string> _recentMessage;

        public SpamGuard()
        {
            _recentMessage = new Queue<string>(25);
        }

        SpamCheckResult ISpamGuard.CheckForSpam(ulong userId, ulong channelId, string message)
        {
            var builtMessage = $"{{{userId}}}-{{{channelId}}}-{{{message}}}";

            // If the maximum size is hit, remove the oldest message
            if (_recentMessage.Count == 25)
                _recentMessage.Dequeue();

            // Add the new message
            _recentMessage.Enqueue(builtMessage);

            // Check the message for soft and hard limit
            var messageCount = _recentMessage.Count(m => m == builtMessage);
            if (messageCount >= _HARD_LIMIT)
                return SpamCheckResult.HardLimitHit;
            if (messageCount > _SOFT_LIMIT && messageCount < _HARD_LIMIT)
                return SpamCheckResult.BetweenSoftAndHardLimit;
            if (messageCount == _SOFT_LIMIT)
                return SpamCheckResult.SoftLimitHit;
            return SpamCheckResult.NoSpam;
        }
    }
}