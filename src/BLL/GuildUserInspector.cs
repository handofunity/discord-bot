namespace HoU.GuildBot.BLL
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using JetBrains.Annotations;
    using Shared.BLL;
    using Shared.DAL;
    using Shared.Objects;
    using Shared.StrongTypes;

    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class GuildUserInspector : IGuildUserInspector
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Fields

        private readonly IGuildUserRegistry _guildUserRegistry;
        private readonly IUserStore _userStore;
        private readonly IDatabaseAccess _databaseAccess;
        private IDiscordAccess _discordAccess;

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Constructors

        public GuildUserInspector(IGuildUserRegistry guildUserRegistry,
                                  IUserStore userStore,
                                  IDatabaseAccess databaseAccess)
        {
            _guildUserRegistry = guildUserRegistry;
            _userStore = userStore;
            _databaseAccess = databaseAccess;
        }

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region IGuildUserInspector Members

        IDiscordAccess IGuildUserInspector.DiscordAccess
        {
            set => _discordAccess = value;
        }

        async Task<EmbedData> IGuildUserInspector.GetLastSeenInfo()
        {
            var ids = _guildUserRegistry.GetGuildMemberUserIds();
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
                missingUsers.Add(await _userStore.GetUser(discordUserID).ConfigureAwait(false));
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

        async Task IGuildUserInspector.UpdateLastSeenInfo(DiscordUserID userID, bool wasOnline, bool isOnline)
        {
            // We're only updating the info when the user goes offline
            if (!(wasOnline && !isOnline))
                return; // If the user does not change from online to offline, we can return here

            // Only save status for guild members, not guests
            if (!_guildUserRegistry.IsGuildMember(userID))
                return;

            var user = await _userStore.GetUser(userID).ConfigureAwait(false);
            await _databaseAccess.UpdateUserInfoLastSeen(user, DateTime.UtcNow).ConfigureAwait(false);
        }

        #endregion
    }
}