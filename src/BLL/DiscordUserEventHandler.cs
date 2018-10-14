namespace HoU.GuildBot.BLL
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Shared.BLL;
    using Shared.DAL;
    using Shared.Enums;
    using Shared.Extensions;
    using Shared.Objects;
    using Shared.StrongTypes;

    public class DiscordUserEventHandler : IDiscordUserEventHandler
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Fields

        private readonly IUserStore _userStore;
        private readonly IPrivacyProvider _privacyProvider;
        private readonly IGameRoleProvider _gameRoleProvider;
        private readonly IDatabaseAccess _databaseAccess;
        private readonly AppSettings _appSettings;
        private IDiscordAccess _discordAccess;

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Constructors

        public DiscordUserEventHandler(IUserStore userStore,
                                       IPrivacyProvider privacyProvider,
                                       IGameRoleProvider gameRoleProvider,
                                       IDatabaseAccess databaseAccess,
                                       AppSettings appSettings)
        {
            _userStore = userStore;
            _privacyProvider = privacyProvider;
            _gameRoleProvider = gameRoleProvider;
            _databaseAccess = databaseAccess;
            _appSettings = appSettings;
        }

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region IDiscordUserEventHandler Members
            
        IDiscordAccess IDiscordUserEventHandler.DiscordAccess
        {
            set => _discordAccess = value;
        }

        void IDiscordUserEventHandler.HandleJoined(DiscordUserID userID, Role roles)
        {
#pragma warning disable CS4014 // Fire & forget
            Task.Run(async () =>
            {
                var result = await _userStore.GetOrAddUser(userID, roles).ConfigureAwait(false);
                if (result.IsNew)
                    await _discordAccess.SendWelcomeMessage(userID).ConfigureAwait(false);
            }).ConfigureAwait(false);
#pragma warning restore CS4014 // Fire & forget
        }

        void IDiscordUserEventHandler.HandleLeft(DiscordUserID userID, string username, ushort discriminatorValue)
        {
            if(!_userStore.TryGetUser(userID, out var user))
                return;
#pragma warning disable CS4014 // Fire & Forget
            Task.Run(async () =>
            {
                await _userStore.RemoveUser(userID).ConfigureAwait(false);
                await _privacyProvider.DeleteUserRelatedData(user).ConfigureAwait(false);
                var leaderMention = _discordAccess.GetRoleMention(Constants.RoleNames.LeaderRoleName);
                var officerMention = _discordAccess.GetRoleMention(Constants.RoleNames.OfficerRoleName);
                var now = DateTime.UtcNow;
                await _discordAccess.LogToDiscord(
                    $"{leaderMention} {officerMention} - User **{username}#{discriminatorValue}** (Role: {user.Roles}) has left the server on {now:D} at {now:HH:mm:ss} UTC.");
            }).ConfigureAwait(false);
#pragma warning restore CS4014 // Fire & Forget
        }

        UserRolesChangedResult IDiscordUserEventHandler.HandleRolesChanged(DiscordUserID userID, Role oldRoles, Role newRoles)
        {
            if (!_userStore.TryGetUser(userID, out var user))
                return new UserRolesChangedResult();
            user.Roles = newRoles;

            // Check if the role change was a promotion
            Role promotedTo;
            if (!oldRoles.HasFlag(Role.Recruit)
             && !oldRoles.HasFlag(Role.Member)
             && !oldRoles.HasFlag(Role.Coordinator)
             && !oldRoles.HasFlag(Role.Officer)
             && !oldRoles.HasFlag(Role.Leader)
             && newRoles.HasFlag(Role.Recruit))
            {
                promotedTo = Role.Recruit;
            }
            else
            {
                return new UserRolesChangedResult();
            }
            
            // Return result for announcement and logging the promotion
            var description = $"Congratulations {user.Mention}, you've been promoted to the rank **{promotedTo}**.";
            if (promotedTo == Role.Recruit)
                description += " Welcome aboard!";
            var a = new EmbedData
            {
                Title = "Promotion",
                Color = Colors.BrightBlue,
                Description = description
            };
            return new UserRolesChangedResult(a, $"{user.Mention} has been promoted to **{promotedTo}**.");
        }

        async Task IDiscordUserEventHandler.HandleStatusChanged(DiscordUserID userID, bool wasOnline, bool isOnline)
        {
            if (!_userStore.TryGetUser(userID, out var user))
                return;

            // We're only updating the info when the user goes offline
            if (!(wasOnline && !isOnline))
                return; // If the user does not change from online to offline, we can return here

            // Only save status for guild members, not guests
            if (!user.IsGuildMember)
                return;

            await _databaseAccess.UpdateUserInfoLastSeen(user, DateTime.UtcNow).ConfigureAwait(false);
        }

        async Task IDiscordUserEventHandler.HandleReactionAdded(DiscordChannelID channelID, DiscordUserID userID, ulong messageID, string emoji)
        {
            // Currently, only reactions in the AshesOfCreationRoleChannelId must be handled
            if (channelID != _appSettings.AshesOfCreationRoleChannelId)
                return;

            // If the message is the AoC role menu message, forward the data to the game role provider
            if (messageID == _gameRoleProvider.AocGameRoleMenuMessageID)
                await _gameRoleProvider.SetGameRole(channelID, userID, _gameRoleProvider.Games.Single(m => m.ShortName == "AoC"), emoji)
                                       .ConfigureAwait(false);
        }

        async Task IDiscordUserEventHandler.HandleReactionRemoved(DiscordChannelID channelID, DiscordUserID userID, ulong messageID, string emoji)
        {
            // Currently, only reactions in the AshesOfCreationRoleChannelId must be handled
            if (channelID != _appSettings.AshesOfCreationRoleChannelId)
                return;

            // If the message is the AoC role menu message, forward the data to the game role provider
            if (messageID == _gameRoleProvider.AocGameRoleMenuMessageID)
                await _gameRoleProvider.RevokeGameRole(channelID, userID, _gameRoleProvider.Games.Single(m => m.ShortName == "AoC"), emoji)
                                       .ConfigureAwait(false);
        }

        #endregion
    }
}