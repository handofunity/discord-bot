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
            if (game.AvailableRoles.Any(m => m.RoleName == className))
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

        IReadOnlyList<EmbedData> IGameRoleProvider.GetGameInfoAsEmbedData()
        {
            var result = new List<EmbedData>();

            foreach (var game in _games.OrderBy(m => m.LongName))
            {
                var ed = new EmbedData
                {
                    Color = Colors.LightGreen,
                    Title = $"Game information for \"{game.LongName}\" ({game.ShortName})"
                };

                if (game.AvailableRoles.Count == 0)
                {
                    ed.Description = "This game has no roles assigned.";
                }
                else
                {
                    ed.Description = "The following roles are assigned to the game:";
                    ed.Fields = game.AvailableRoles
                                    .OrderBy(m => m.RoleName)
                                    .Select(m => new EmbedField(m.RoleName, $"DiscordRoleID: {m.DiscordRoleID}", false))
                                    .ToArray();
                }

                result.Add(ed);
            }

            return result;
        }

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
            _logger.LogInformation($"Loaded {_games.Count} games with a total count of {_games.Sum(m => m.AvailableRoles.Count)} roles.");
        }

        (int GameMembers, Dictionary<string, int> RoleDistribution) IGameRoleProvider.GetGameRoleDistribution(AvailableGame game)
        {
            var distribution = new Dictionary<string, int>();
            foreach (var className in game.AvailableRoles)
            {
                var count = _discordAccess.CountMembersWithRole($"{game.ShortName} - {className.RoleName}");
                distribution.Add(className.RoleName, count);
            }

            var gameMembers = _discordAccess.CountMembersWithRole($"{game.ShortName} ({game.LongName})");

            return (gameMembers, distribution);
        }

        async Task<(bool Success, string Message)> IGameRoleProvider.AddGameRole(InternalUserID userID, 
                                                                                 string gameShortName,
                                                                                 string roleName,
                                                                                 ulong discordRoleID)
        {
            // Precondition
            var gameID = await _databaseAccess.TryGetGameID(gameShortName).ConfigureAwait(false);
            if (gameID == null)
                return (false, $"Couldn't find any game with the short name `{gameShortName}`.");

            // Act
            var (success, error) = await _databaseAccess.TryAddGameRole(userID, gameID.Value, roleName, discordRoleID).ConfigureAwait(false);
            if (!success) return (false, $"Failed to add the game role: {error}");

            // Update cache
            var game = _games.Single(m => m.ShortName == gameShortName);
            game.AvailableRoles.Add(new AvailableGameRole
            {
                DiscordRoleID = discordRoleID,
                RoleName = roleName
            });

            return (true, "The game role was added successfully.");
        }

        async Task<(bool Success, string Message, string OldRoleName)> IGameRoleProvider.EditGameRole(InternalUserID userID, 
                                                                                                      ulong discordRoleID,
                                                                                                      string newRoleName)
        {
            // Preconditions
            var gameRole = await _databaseAccess.TryGetGameRole(discordRoleID).ConfigureAwait(false);
            if (gameRole == null)
                return (false, $"Couldn't find any game role with the Discord role ID `{discordRoleID}`.", null);
            if (string.IsNullOrWhiteSpace(newRoleName))
                return (false, "New role name cannot be empty.", null);
            if (gameRole.Value.CurrentName == newRoleName)
                return (false, "New role name is the same as the current one.", null);

            // Act
            var (success, error) = await _databaseAccess.TryEditGameRole(userID, gameRole.Value.ID, newRoleName).ConfigureAwait(false);
            if (!success) return (false, $"Failed to edit the game role: {error}", null);

            // Update cache
            var cachedGameRole = _games.SelectMany(m => m.AvailableRoles).Single(m => m.DiscordRoleID == discordRoleID);
            cachedGameRole.RoleName = newRoleName;

            return (true, "The game role was edited successfully.", gameRole.Value.CurrentName);
        }

        async Task<(bool Success, string Message, string OldRoleName)> IGameRoleProvider.RemoveGameRole(ulong discordRoleID)
        {
            // Precondition
            var gameRole = await _databaseAccess.TryGetGameRole(discordRoleID).ConfigureAwait(false);
            if (gameRole == null)
                return (false, $"Couldn't find any game role with the Discord role ID `{discordRoleID}`.", null);

            // Act
            var (success, error) = await _databaseAccess.TryRemoveGameRole(gameRole.Value.ID).ConfigureAwait(false);
            if (!success) return (false, $"Failed to remove the game role: {error}", null);

            // Update cache
            var cachedGameRole = _games.SelectMany(m => m.AvailableRoles).Single(m => m.DiscordRoleID == discordRoleID);
            var gameWithCachedRole = _games.Single(m => m.AvailableRoles.Contains(cachedGameRole));
            gameWithCachedRole.AvailableRoles.Remove(cachedGameRole);

            return (true, "The game role was removed successfully.", gameRole.Value.CurrentName);
        }

        #endregion
    }
}