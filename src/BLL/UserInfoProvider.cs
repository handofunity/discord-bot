namespace HoU.GuildBot.BLL
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using JetBrains.Annotations;
    using Shared.BLL;
    using Shared.DAL;
    using Shared.Objects;
    using Shared.StrongTypes;

    [UsedImplicitly]
    public class UserInfoProvider : IUserInfoProvider
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Fields

        private readonly IUserStore _userStore;
        private readonly IDiscordAccess _discordAccess;
        private readonly IDatabaseAccess _databaseAccess;

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Constructors

        public UserInfoProvider(IUserStore userStore,
                                IDiscordAccess discordAccess,
                                IDatabaseAccess databaseAccess)
        {
            _userStore = userStore;
            _discordAccess = discordAccess;
            _databaseAccess = databaseAccess;
        }

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Private Methods

        private EmbedData CreateEmbedDataForWhoIs(User user)
        {
            var userName = _discordAccess.GetUserNames(new[] { user.DiscordUserID })[user.DiscordUserID];
            return new EmbedData
            {
                Color = Colors.LightGreen,
                Title = $"\"Who is\" information about {userName}",
                Fields = new[]
                {
                    new EmbedField("DiscordUserID", user.DiscordUserID, false),
                    new EmbedField("InternalUserID", user.InternalUserID, false),
                    new EmbedField("Is Guild Member", user.IsGuildMember, false),
                    new EmbedField("Bot Permission Roles", user.Roles, false),
                }
            };
        }

        private EmbedData CreateEmbedDataForWhoIsUnknownUser()
        {
            return new EmbedData
            {
                Title = "Unknown user",
                Color = Colors.Red,
                Description = "No user found matching the given ID."
            };
        }

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region IUserInfoProvider Members

        async Task<EmbedData> IUserInfoProvider.GetLastSeenInfo()
        {
            var ids = _userStore.GetUsers(m => m.IsGuildMember).Select(m => m.DiscordUserID).ToArray();
            // LastSeen = null equals the user is currently online
            var data = new List<(DiscordUserID UserID, string Username, bool IsOnline, DateTime? LastSeen)>(ids.Length);

            // Fetch all user names
            var usernames = _discordAccess.GetUserNames(ids);

            // Fetch data for online members
            foreach (var userID in ids)
            {
                var isOnline = _discordAccess.IsUserOnline(userID);
                if (isOnline)
                    data.Add((userID, usernames[userID], true, null));
            }

            // Fetch data for offline members
            var missingUserIDs = ids.Except(data.Select(m => m.UserID)).ToArray();
            var missingUsers = new List<User>();
            foreach (var discordUserID in missingUserIDs)
            {
                if (!_userStore.TryGetUser(discordUserID, out var user))
                    continue;
                missingUsers.Add(user);
            }
            var lastSeenData = await _databaseAccess.GetLastSeenInfoForUsers(missingUsers.ToArray()).ConfigureAwait(false);
            var noInfoFallback = new DateTime(2018, 1, 1);
            foreach (var lsd in lastSeenData)
            {
                var user = missingUsers.Single(m => m.InternalUserID == lsd.UserID);
                data.Add((user.DiscordUserID, usernames[user.DiscordUserID], false, lsd.LastSeen ?? (DateTime?)noInfoFallback));
            }

            // Format
            var result = string.Join(Environment.NewLine,
                data.OrderByDescending(m => m.IsOnline)
                    .ThenByDescending(m => m.LastSeen)
                    .ThenBy(m => m.Username)
                    .Select(m => $"{(m.IsOnline ? "Online" : m.LastSeen.Value.ToString("yyyy-MM-dd HH:mm"))}: {m.Username}"));

            return await Task.FromResult(new EmbedData
            {
                Color = Colors.LightOrange,
                Title = "Last seen times for all guild members",
                Description = result
            }).ConfigureAwait(false);
        }

        EmbedData IUserInfoProvider.WhoIs(DiscordUserID userID)
        {
            return _userStore.TryGetUser(userID, out var user)
                       ? CreateEmbedDataForWhoIs(user)
                       : CreateEmbedDataForWhoIsUnknownUser();
        }

        EmbedData IUserInfoProvider.WhoIs(string username, string remainderContent)
        {
            var regex = new Regex(
                "^((?<DiscordUserIdGroup>DiscordUserID:(?<DiscordUserID>\\d+))|(?<InternalUserIdGroup>InternalUserID:(?<InternalUserID>\\d+)))");
            var match = regex.Match(remainderContent);
            if (match.Groups["DiscordUserIdGroup"].Success)
            {
                var discordUserID = (DiscordUserID) ulong.Parse(match.Groups["DiscordUserID"].ToString());
                return _userStore.TryGetUser(discordUserID, out var user)
                           ? CreateEmbedDataForWhoIs(user)
                           : CreateEmbedDataForWhoIsUnknownUser();
            }

            if (match.Groups["InternalUserIdGroup"].Success)
            {
                var internalUserID = (InternalUserID) int.Parse(match.Groups["InternalUserID"].ToString());
                return _userStore.TryGetUser(internalUserID, out var user)
                           ? CreateEmbedDataForWhoIs(user)
                           : CreateEmbedDataForWhoIsUnknownUser();
            }

            return new EmbedData
            {
                Title = Constants.InvalidCommandUsageTitle,
                Color = Colors.Red,
                Description = $"**{username}**: Correct command syntax: _DiscordUserID:123546874548_ or _InternalUserID:16_"
            };
        }

        #endregion
    }
}