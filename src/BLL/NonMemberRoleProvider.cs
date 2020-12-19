using System;
using System.Linq;
using System.Threading.Tasks;
using HoU.GuildBot.Shared.StrongTypes;
using HoU.GuildBot.Shared.BLL;
using HoU.GuildBot.Shared.DAL;
using HoU.GuildBot.Shared.Enums;
using HoU.GuildBot.Shared.Objects;

namespace HoU.GuildBot.BLL
{
    public class NonMemberRoleProvider : INonMemberRoleProvider
    {
        private readonly IUserStore _userStore;
        private readonly IGameRoleProvider _gameRoleProvider;
        private IDiscordAccess _discordAccess;

        public NonMemberRoleProvider(IUserStore userStore,
                                     IGameRoleProvider gameRoleProvider)
        {
            _userStore = userStore;
            _gameRoleProvider = gameRoleProvider;
        }

        private async Task<bool> CanChangeRoles(DiscordChannelID channelID, User user)
        {
            var canChangeRole = _discordAccess.CanManageRolesForUser(user.DiscordUserID);
            if (canChangeRole)
                return true;

            var createdMessages = await _discordAccess.CreateBotMessagesInChannel(channelID, new[] { $"{user.Mention}: The bot is not allowed to change your role." }).ConfigureAwait(false);
            var messageID = createdMessages[0];
            DeleteMessageAfterDelay(channelID, messageID);
            return false;
        }

        private void DeleteMessageAfterDelay(DiscordChannelID channelID, ulong messageID)
        {
            _ = Task.Run(async () =>
            {
                // Delete message after 5 minutes
                await Task.Delay(TimeSpan.FromMinutes(5)).ConfigureAwait(false);
                await _discordAccess.DeleteBotMessageInChannel(channelID, messageID).ConfigureAwait(false);
            }).ConfigureAwait(false);
        }

        private static Role MapNonMemberEmojiToStaticRole(EmojiDefinition emoji)
        {
            if (emoji == null)
                return Role.NoRole;

            if (emoji.Equals(Constants.NonMemberRolesEmojis.Wave))
                return Role.FriendOfMember;
            if (emoji.Equals(Constants.NonMemberRolesEmojis.Thinking))
                return Role.Guest;

            return Role.NoRole;
        }

        private ulong? MapNonMemberRawEmojiNameToDynamicRole(string rawEmojiName)
        {
            // If none of the static roles, look up for the dynamic roles defined in the games
            var gamesWithInterestRoles = _gameRoleProvider.Games
                                                          .Where(m => m.GameInterestRoleId != null && m.GameInterestEmojiName != null)
                                                          .ToArray();
            var matchingGame = gamesWithInterestRoles.SingleOrDefault(m => string.Equals(m.GameInterestEmojiName, rawEmojiName));
            return matchingGame?.GameInterestRoleId;
        }

        IDiscordAccess INonMemberRoleProvider.DiscordAccess
        {
            set => _discordAccess = value;
        }

        async Task INonMemberRoleProvider.SetNonMemberRole(DiscordChannelID channelID,
                                                           DiscordUserID userID,
                                                           EmojiDefinition emoji,
                                                           string rawEmojiName)
        {
            if (!_userStore.TryGetUser(userID, out var user))
                return;
            if (!await CanChangeRoles(channelID, user).ConfigureAwait(false))
                return;

            var staticRole = MapNonMemberEmojiToStaticRole(emoji);
            if (staticRole == Role.NoRole)
            {
                var dynamicRoleId = MapNonMemberRawEmojiNameToDynamicRole(rawEmojiName);
                if (dynamicRoleId == null)
                    return;

                var (success, roleName) = await _discordAccess.TryAddNonMemberRole(userID, dynamicRoleId.Value).ConfigureAwait(false);
                if (success)
                {
                    var createdMessages = await _discordAccess.CreateBotMessagesInChannel(channelID, new[] { $"{user.Mention}: The role **_{roleName}_** was **added**." }).ConfigureAwait(false);
                    var messageID = createdMessages[0];
                    DeleteMessageAfterDelay(channelID, messageID);
                    await _discordAccess.LogToDiscord($"User {user.Mention} **added** the role **_{roleName}_**.").ConfigureAwait(false);
                }
            }
            else
            {
                var added = await _discordAccess.TryAddNonMemberRole(userID, staticRole).ConfigureAwait(false);
                if (added)
                {
                    var createdMessages = await _discordAccess.CreateBotMessagesInChannel(channelID, new[] { $"{user.Mention}: The role **_{staticRole}_** was **added**." }).ConfigureAwait(false);
                    var messageID = createdMessages[0];
                    DeleteMessageAfterDelay(channelID, messageID);
                    await _discordAccess.LogToDiscord($"User {user.Mention} **added** the role **_{staticRole}_**.").ConfigureAwait(false);
                }
            }
        }

        async Task INonMemberRoleProvider.RevokeNonMemberRole(DiscordChannelID channelID,
                                                              DiscordUserID userID,
                                                              EmojiDefinition emoji,
                                                              string rawEmojiName)
        {
            if (!_userStore.TryGetUser(userID, out var user))
                return;
            if (!await CanChangeRoles(channelID, user).ConfigureAwait(false))
                return;

            var staticRole = MapNonMemberEmojiToStaticRole(emoji);
            if (staticRole == Role.NoRole)
            {
                var dynamicRoleId = MapNonMemberRawEmojiNameToDynamicRole(rawEmojiName);
                if (dynamicRoleId == null)
                    return;

                var (success, roleName) = await _discordAccess.TryRevokeNonMemberRole(userID, dynamicRoleId.Value).ConfigureAwait(false);
                if (success)
                {
                    var createdMessages = await _discordAccess.CreateBotMessagesInChannel(channelID, new[] { $"{user.Mention}: The role **_{roleName}_** was **revoked**." }).ConfigureAwait(false);
                    var messageID = createdMessages[0];
                    DeleteMessageAfterDelay(channelID, messageID);
                }
            }
            else
            {
                var revoked = await _discordAccess.TryRevokeNonMemberRole(userID, staticRole).ConfigureAwait(false);
                if (revoked)
                {
                    var createdMessages = await _discordAccess.CreateBotMessagesInChannel(channelID, new[] { $"{user.Mention}: The role **_{staticRole}_** was **revoked**." }).ConfigureAwait(false);
                    var messageID = createdMessages[0];
                    DeleteMessageAfterDelay(channelID, messageID);
                }
            }
        }
    }
}