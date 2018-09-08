namespace HoU.GuildBot.BLL
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using JetBrains.Annotations;
    using Microsoft.Extensions.Logging;
    using Shared.BLL;
    using Shared.DAL;
    using Shared.Objects;
    using Shared.StrongTypes;

    [UsedImplicitly]
    public class GameRoleProvider : IGameRoleProvider
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Fields

        private readonly IUserStore _userStore;
        private readonly IDatabaseAccess _databaseAccess;
        private readonly ILogger<GameRoleProvider> _logger;
        private readonly List<AvailableGame> _games;

        private IDiscordAccess _discordAccess;

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Constructors

        public GameRoleProvider(IUserStore userStore,
                                IDatabaseAccess databaseAccess,
                                ILogger<GameRoleProvider> logger)
        {
            _userStore = userStore;
            _databaseAccess = databaseAccess;
            _logger = logger;
            _games = new List<AvailableGame>();
        }

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Private Methods

        private async Task<bool> CanChangeRoles(DiscordChannelID channelID, User user)
        {
            var canChangeRole = _discordAccess.CanManageRolesForUser(user.DiscordUserID);
            if (canChangeRole)
                return true;

            var createdMessages = await _discordAccess.CreateBotMessagesInChannel(channelID, new[] {$"{user.Mention}: The bot is not allowed to change your role."}).ConfigureAwait(false);
            var messageID = createdMessages[0];
            DeleteMessageAfterDelay(channelID, messageID);
            return false;
        }

        private async Task<bool> IsValidClassName(DiscordChannelID channelID, User user, AvailableGame game, string className)
        {
            if (game.ClassNames.Any(m => m == className))
                return true;
            
            var createdMessages = await _discordAccess.CreateBotMessagesInChannel(channelID, new[] { $"{user.Mention}: Class name '{className}' is not valid for {game}." }).ConfigureAwait(false);
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

        private static string EmojiToClassName(string emoji)
        {
            switch (emoji)
            {
                case Constants.AocRoleEmojis.Bard:
                    return nameof(Constants.AocRoleEmojis.Bard);
                case Constants.AocRoleEmojis.Cleric:
                    return nameof(Constants.AocRoleEmojis.Cleric);
                case Constants.AocRoleEmojis.Fighter:
                    return nameof(Constants.AocRoleEmojis.Fighter);
                case Constants.AocRoleEmojis.Mage:
                    return nameof(Constants.AocRoleEmojis.Mage);
                case Constants.AocRoleEmojis.Ranger:
                    return nameof(Constants.AocRoleEmojis.Ranger);
                case Constants.AocRoleEmojis.Rogue:
                    return nameof(Constants.AocRoleEmojis.Rogue);
                case Constants.AocRoleEmojis.Summoner:
                    return nameof(Constants.AocRoleEmojis.Summoner);
                case Constants.AocRoleEmojis.Tank:
                    return nameof(Constants.AocRoleEmojis.Tank);
                default:
                    throw new ArgumentOutOfRangeException(nameof(emoji), $"Emoji '{emoji}' is unknown.");
            }
        }

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region IGameRoleProvider Members

        IDiscordAccess IGameRoleProvider.DiscordAccess
        {
            set => _discordAccess = value;
        }

        ulong IGameRoleProvider.AocGameRoleMenuMessageID { get; set; }

        public IReadOnlyList<AvailableGame> Games => _games;

        async Task IGameRoleProvider.SetGameRole(DiscordChannelID channelID, DiscordUserID userID, AvailableGame game, string emoji)
        {
            if (!_userStore.TryGetUser(userID, out var user))
                return;
            if (!await CanChangeRoles(channelID, user).ConfigureAwait(false))
                return;

            var className = EmojiToClassName(emoji);
            if (!await IsValidClassName(channelID, user, game, className))
                return;

            var added = await _discordAccess.TryAddGameRole(userID, game, className).ConfigureAwait(false);
            if (added)
            {
                var createdMessages = await _discordAccess.CreateBotMessagesInChannel(channelID, new[] { $"{user.Mention}: The class **_{className}_** for the game _{game.LongName}_ was **added**." }).ConfigureAwait(false);
                var messageID = createdMessages[0];
                DeleteMessageAfterDelay(channelID, messageID);
                await _discordAccess.LogToDiscord($"User {user.Mention} **added** the role **_{className}_** for the game _{game.LongName}_.").ConfigureAwait(false);
            }
        }

        async Task IGameRoleProvider.RevokeGameRole(DiscordChannelID channelID, DiscordUserID userID, AvailableGame game, string emoji)
        {
            if (!_userStore.TryGetUser(userID, out var user))
                return;
            if (!await CanChangeRoles(channelID, user).ConfigureAwait(false))
                return;

            var className = EmojiToClassName(emoji);
            if (!await IsValidClassName(channelID, user, game, className))
                return;

            var revoked = await _discordAccess.TryRevokeGameRole(userID, game, className).ConfigureAwait(false);
            if (revoked)
            {
                var createdMessages = await _discordAccess.CreateBotMessagesInChannel(channelID, new[] { $"{user.Mention}: The class **_{className}_** for the game _{game.LongName}_ was **revoked**." }).ConfigureAwait(false);
                var messageID = createdMessages[0];
                DeleteMessageAfterDelay(channelID, messageID);
            }
        }

        async Task IGameRoleProvider.LoadAvailableGames()
        {
            _games.Clear();
            var games = await _databaseAccess.GetAvailableGames().ConfigureAwait(false);
            _games.AddRange(games);
            foreach (var availableGame in _games)
                _discordAccess.GetClassNamesForGame(availableGame);
            _logger.LogInformation($"Loaded {_games.Count} games with a total count of {_games.Sum(m => m.ClassNames.Count)} roles.");
        }

        #endregion
    }
}