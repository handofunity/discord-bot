using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using HoU.GuildBot.Shared.BLL;
using HoU.GuildBot.Shared.Objects;
using HoU.GuildBot.Shared.StrongTypes;

namespace HoU.GuildBot.BLL
{
    [UsedImplicitly]
    public class IgnoreGuard : IIgnoreGuard
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Fields

        private readonly IBotInformationProvider _botInformationProvider;
        private readonly Dictionary<DiscordUserID, DateTime> _ignoreList;

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Constructors

        public IgnoreGuard(IBotInformationProvider botInformationProvider)
        {
            _botInformationProvider = botInformationProvider;
            _ignoreList = new Dictionary<DiscordUserID, DateTime>();
        }

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region IIgnoreGuard Members

        EmbedData IIgnoreGuard.TryAddToIgnoreList(DiscordUserID userID, string username, string remainderContent)
        {
            var regex = new Regex(@"for (?<minutes>\d+) minutes\.?");
            var match = regex.Match(remainderContent);
            if (!match.Success)
                return new EmbedData
                {
                    Title = Constants.InvalidCommandUsageTitle,
                    Color = Colors.Red,
                    Description = $"**{username}**: Correct command syntax: _ignore me for **45** minutes_"
                };

            // On success, we parse the minutes
            var minutes = int.Parse(match.Groups["minutes"].ToString());

            // Check if the minutes value is between 3 and 60 minutes
            if (minutes < 3 || minutes > 60)
            {
                return new EmbedData
                {
                    Title = Constants.InvalidCommandUsageTitle,
                    Color = Colors.Red,
                    Description = $"**{username}**: You cannot be ignored by less than 3 or more than 60 minutes."
                };
            }

            // Update or insert value
            var ignoreUntil = DateTime.Now.ToUniversalTime().AddMinutes(minutes);
            _ignoreList[userID] = ignoreUntil;
            return new EmbedData
            {
                Title = "Ignore complete",
                Color = Colors.Green,
                Description = $"**{username}**: You will be ignored for the next {minutes} minutes (until {ignoreUntil:dd.MM.yyyy HH:mm:ss} UTC) in the environment **{_botInformationProvider.GetEnvironmentName()}**."
            };
        }

        EmbedData IIgnoreGuard.TryRemoveFromIgnoreList(DiscordUserID userID, string username)
        {
            if (!_ignoreList.ContainsKey(userID))
                return null;

            _ignoreList.Remove(userID);
            return new EmbedData
            {
                Title = "Notice complete",
                Color = Colors.Green,
                Description = $"**{username}**: You will no longer be ignored in the environment **{_botInformationProvider.GetEnvironmentName()}**."
            };
        }

        bool IIgnoreGuard.ShouldIgnoreMessage(DiscordUserID userID)
        {
            if (!_ignoreList.TryGetValue(userID, out var ignoreUntil)) return false;
            if (ignoreUntil > DateTime.UtcNow)
                return true;
            _ignoreList.Remove(userID);
            return false;
        }

        #endregion
    }
}