using System;
using System.Linq;
using System.Threading.Tasks;
using HoU.GuildBot.Shared.BLL;
using HoU.GuildBot.Shared.DAL;
using HoU.GuildBot.Shared.Enums;
using HoU.GuildBot.Shared.Objects;
using HoU.GuildBot.Shared.StrongTypes;

namespace HoU.GuildBot.BLL
{
    public class DiscordUserEventHandler : IDiscordUserEventHandler
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Fields

        private readonly IUserStore _userStore;
        private readonly IPrivacyProvider _privacyProvider;
        private readonly INonMemberRoleProvider _nonMemberRoleProvider;
        private readonly IGameRoleProvider _gameRoleProvider;
        private readonly IDatabaseAccess _databaseAccess;
        private readonly AppSettings _appSettings;
        private IDiscordAccess _discordAccess;

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Constructors

        public DiscordUserEventHandler(IUserStore userStore,
                                       IPrivacyProvider privacyProvider,
                                       INonMemberRoleProvider nonMemberRoleProvider,
                                       IGameRoleProvider gameRoleProvider,
                                       IDatabaseAccess databaseAccess,
                                       AppSettings appSettings)
        {
            _userStore = userStore;
            _privacyProvider = privacyProvider;
            _nonMemberRoleProvider = nonMemberRoleProvider;
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
            _ = Task.Run(async () =>
            {
                await _userStore.AddUserIfNewAsync(userID, roles).ConfigureAwait(false);
            }).ConfigureAwait(false);
        }

        void IDiscordUserEventHandler.HandleLeft(DiscordUserID userID,
                                                 string username,
                                                 ushort discriminatorValue,
                                                 DateTimeOffset? joinedAt,
                                                 string[] roles)
        {
            if(!_userStore.TryGetUser(userID, out var user))
                return;
            _ = Task.Run(async () =>
            {
                await _userStore.RemoveUser(userID).ConfigureAwait(false);
                await _privacyProvider.DeleteUserRelatedData(user).ConfigureAwait(false);
                var now = DateTime.UtcNow;
                // Only post to Discord log if the user was on the server for more than 10 minutes, or the time on the server cannot be determined.
                if (!joinedAt.HasValue
                  || (now - joinedAt.Value.UtcDateTime).TotalMinutes > 10)
                {
                    var leaderMention = _discordAccess.GetRoleMention(Constants.RoleNames.LeaderRoleName);
                    var officerMention = _discordAccess.GetRoleMention(Constants.RoleNames.OfficerRoleName);
                    var formattedRolesMessage = roles.Length == 0
                                                   ? string.Empty
                                                   : $"; Roles: {string.Join(", ", roles.Select(m => "`" + m + "`"))}";
                    await _discordAccess.LogToDiscord(
                                                      $"{leaderMention} {officerMention}: User `{username}#{discriminatorValue}` " +
                                                      $"(Membership level: **{user.Roles}**{formattedRolesMessage}) " +
                                                      $"has left the server on {now:D} at {now:HH:mm:ss} UTC.")
                                        .ConfigureAwait(false);
                }
                // If it has been less than 10 minutes, write to the #public-chat, so people will know that a new user left before greeting them.
                else
                {
                    await _discordAccess.CreateBotMessageInWelcomeChannel($"User `{username}#{discriminatorValue}` has left the server.")
                                        .ConfigureAwait(false);
                }
            }).ConfigureAwait(false);
        }

        UserRolesChangedResult IDiscordUserEventHandler.HandleRolesChanged(DiscordUserID userID, Role oldRoles, Role newRoles)
        {
            if (!_userStore.TryGetUser(userID, out var user))
                return new UserRolesChangedResult();
            user.Roles = newRoles;

            // Check if the role change was a promotion
            Role promotedTo;
            if (!oldRoles.HasFlag(Role.TrialMember)
             && !oldRoles.HasFlag(Role.Member)
             && !oldRoles.HasFlag(Role.Coordinator)
             && !oldRoles.HasFlag(Role.Officer)
             && !oldRoles.HasFlag(Role.Leader)
             && newRoles.HasFlag(Role.TrialMember))
            {
                promotedTo = Role.TrialMember;
            }
            else
            {
                return new UserRolesChangedResult();
            }
            
            // Return result for announcement and logging the promotion
            var description = $"Congratulations {user.Mention}, you've been promoted to the rank **{promotedTo}**. Welcome aboard!";
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

        async Task IDiscordUserEventHandler.HandleReactionAdded(DiscordChannelID channelID,
                                                                DiscordUserID userID,
                                                                ulong messageID,
                                                                EmojiDefinition emoji,
                                                                string rawEmojiName)
        {
            if (emoji == null && rawEmojiName == null)
                return;

            // Channel must be a role channel
            if (channelID != _appSettings.AshesOfCreationRoleChannelId
                && channelID != _appSettings.WorldOfWarcraftRoleChannelId
                && channelID != _appSettings.GamesRolesChannelId
                && channelID != _appSettings.InfoAndRolesChannelId)
                return;

            if (emoji != null && messageID == _gameRoleProvider.AocGameRoleMenuMessageID)
                // If the message is the AoC role menu message, forward the data to the game role provider
                await _gameRoleProvider.SetGameRole(channelID, userID, _gameRoleProvider.Games.Single(m => m.ShortName == Constants.RoleMenuGameShortNames.AshesOfCreation), emoji)
                                       .ConfigureAwait(false);
            else if (emoji != null && messageID == _gameRoleProvider.WowGameRoleMenuMessageID)
                // If the message is the WoW role menu message, forward the data to the game role provider
                await _gameRoleProvider.SetGameRole(channelID, userID, _gameRoleProvider.Games.Single(m => m.ShortName == Constants.RoleMenuGameShortNames.WorldOfWarcraftClassic), emoji)
                                       .ConfigureAwait(false);
            else if (emoji != null && emoji == Constants.GamesRolesEmojis.Joystick && _gameRoleProvider.GamesRolesMenuMessageIDs.Contains(messageID))
            {
                // If the message is one of the games roles menu messages, forward the data to the game role provider
                var messages = await _discordAccess.GetBotMessagesInChannel(channelID).ConfigureAwait(false);
                var message = messages.Single(m => m.MessageID == messageID);
                var game = _gameRoleProvider.Games.Where(m => m.PrimaryGameDiscordRoleID != null).SingleOrDefault(m => message.Content.Contains(m.LongName));
                if (game != null) await _gameRoleProvider.SetPrimaryGameRole(channelID, userID, game).ConfigureAwait(false);
            }
            else if (messageID == _appSettings.FriendOrGuestMessageId || messageID == _appSettings.NonMemberGameInterestMessageId)
            {
                // If the message is from the friend or guest menu, forward the data to the non-member role provider.
                await _nonMemberRoleProvider.SetNonMemberRole(channelID, userID, emoji, rawEmojiName).ConfigureAwait(false);
            }
        }

        async Task IDiscordUserEventHandler.HandleReactionRemoved(DiscordChannelID channelID,
                                                                  DiscordUserID userID,
                                                                  ulong messageID,
                                                                  EmojiDefinition emoji,
                                                                  string rawEmojiName)
        {
            if (emoji == null && rawEmojiName == null)
                return;

            // Channel must be a role channel
            if (channelID != _appSettings.AshesOfCreationRoleChannelId
                && channelID != _appSettings.WorldOfWarcraftRoleChannelId
                && channelID != _appSettings.GamesRolesChannelId
                && channelID != _appSettings.InfoAndRolesChannelId)
                return;

            if (emoji != null && messageID == _gameRoleProvider.AocGameRoleMenuMessageID)
                // If the message is the AoC role menu message, forward the data to the game role provider
                await _gameRoleProvider.RevokeGameRole(channelID, userID, _gameRoleProvider.Games.Single(m => m.ShortName == Constants.RoleMenuGameShortNames.AshesOfCreation), emoji)
                                       .ConfigureAwait(false);
            else if (emoji != null && messageID == _gameRoleProvider.WowGameRoleMenuMessageID)
                // If the message is the WoW role menu message, forward the data to the game role provider
                await _gameRoleProvider.RevokeGameRole(channelID, userID, _gameRoleProvider.Games.Single(m => m.ShortName == Constants.RoleMenuGameShortNames.WorldOfWarcraftClassic), emoji)
                                       .ConfigureAwait(false);
            else if (emoji != null && emoji == Constants.GamesRolesEmojis.Joystick && _gameRoleProvider.GamesRolesMenuMessageIDs.Contains(messageID))
            {
                // If the message is one of the games roles menu messages, forward the data to the game role provider
                var messages = await _discordAccess.GetBotMessagesInChannel(channelID).ConfigureAwait(false);
                var message = messages.Single(m => m.MessageID == messageID);
                var game = _gameRoleProvider.Games.Where(m => m.PrimaryGameDiscordRoleID != null).SingleOrDefault(m => message.Content.Contains(m.LongName));
                if (game != null) await _gameRoleProvider.RevokePrimaryGameRole(channelID, userID, game).ConfigureAwait(false);
            }
            else if (messageID == _appSettings.FriendOrGuestMessageId || messageID == _appSettings.NonMemberGameInterestMessageId)
            {
                // If the message is from the friend or guest menu, forward the data to the non-member role provider.
                await _nonMemberRoleProvider.RevokeNonMemberRole(channelID, userID, emoji, rawEmojiName).ConfigureAwait(false);
            }
        }

        #endregion
    }
}