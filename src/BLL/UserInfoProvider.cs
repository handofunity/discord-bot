namespace HoU.GuildBot.BLL
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Shared.BLL;
    using Shared.DAL;
    using Shared.Objects;
    using Shared.StrongTypes;

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
                missingUsers.Add(_userStore.GetUser(discordUserID));
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

        #endregion
    }
}