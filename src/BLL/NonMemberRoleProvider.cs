namespace HoU.GuildBot.BLL
{
    using System;
    using System.Threading.Tasks;
    using Shared.StrongTypes;
    using Shared.BLL;
    using Shared.DAL;
    using Shared.Enums;
    using Shared.Objects;

    public class NonMemberRoleProvider : INonMemberRoleProvider
    {
        private readonly IUserStore _userStore;
        private IDiscordAccess _discordAccess;

        public NonMemberRoleProvider(IUserStore userStore)
        {
            _userStore = userStore;
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
#pragma warning disable CS4014 // Fire & Forget
            Task.Run(async () =>
            {
                // Delete message after 5 minutes
                await Task.Delay(TimeSpan.FromMinutes(5)).ConfigureAwait(false);
                await _discordAccess.DeleteBotMessageInChannel(channelID, messageID).ConfigureAwait(false);
            }).ConfigureAwait(false);
#pragma warning restore CS4014 // Fire & Forget
        }

        private static Role MapNonMemberEmojiToRole(EmojiDefinition emoji)
        {
            if (emoji.Equals(Constants.NonMemberRolesEmojis.Wave))
                return Role.FriendOfMember;
            if (emoji.Equals(Constants.NonMemberRolesEmojis.Thinking))
                return Role.Guest;
            if (emoji.Equals(Constants.NonMemberRolesEmojis.GameInterestAshesOfCreation))
                return Role.GameInterestAshesOfCreation;
            if (emoji.Equals(Constants.NonMemberRolesEmojis.GameInterestWorldOfWarcraftClassic))
                return Role.GameInterestWorldOfWarcraftClassic;
            if (emoji.Equals(Constants.NonMemberRolesEmojis.GameInterestOath))
                return Role.GameInterestOath;
            if (emoji.Equals(Constants.NonMemberRolesEmojis.GameInterestShopTitans))
                return Role.GameInterestShopTitans;
            if (emoji.Equals(Constants.NonMemberRolesEmojis.GameInterestFinalFantasy14))
                return Role.GameInterestFinalFantasy14;

            throw new ArgumentOutOfRangeException(nameof(emoji));
        }

        IDiscordAccess INonMemberRoleProvider.DiscordAccess
        {
            set => _discordAccess = value;
        }

        async Task INonMemberRoleProvider.SetNonMemberRole(DiscordChannelID channelID,
                                                           DiscordUserID userID,
                                                           EmojiDefinition emoji)
        {
            if (!_userStore.TryGetUser(userID, out var user))
                return;
            if (!await CanChangeRoles(channelID, user).ConfigureAwait(false))
                return;

            var targetRole = MapNonMemberEmojiToRole(emoji);

            var added = await _discordAccess.TryAddNonMemberRole(userID, targetRole).ConfigureAwait(false);
            if (added)
            {
                var createdMessages = await _discordAccess.CreateBotMessagesInChannel(channelID, new[] {$"{user.Mention}: The role **_{targetRole}_** was **added**."}).ConfigureAwait(false);
                var messageID = createdMessages[0];
                DeleteMessageAfterDelay(channelID, messageID);
                await _discordAccess.LogToDiscord($"User {user.Mention} **added** the role **_{targetRole}_**.").ConfigureAwait(false);
            }
        }

        async Task INonMemberRoleProvider.RevokeNonMemberRole(DiscordChannelID channelID,
                                                              DiscordUserID userID,
                                                              EmojiDefinition emoji)
        {
            if (!_userStore.TryGetUser(userID, out var user))
                return;
            if (!await CanChangeRoles(channelID, user).ConfigureAwait(false))
                return;

            var targetRole = MapNonMemberEmojiToRole(emoji);

            var revoked = await _discordAccess.TryRevokeNonMemberRole(userID, targetRole).ConfigureAwait(false);
            if (revoked)
            {
                var createdMessages = await _discordAccess.CreateBotMessagesInChannel(channelID, new[] {$"{user.Mention}: The role **_{targetRole}_** was **revoked**."}).ConfigureAwait(false);
                var messageID = createdMessages[0];
                DeleteMessageAfterDelay(channelID, messageID);
            }
        }
    }
}