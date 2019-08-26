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
    using Shared.Enums;
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

        private static string AocEmojiToClassName(EmojiDefinition emoji)
        {
            if (emoji.Equals(Constants.AocRoleEmojis.Bard))
                return nameof(Constants.AocRoleEmojis.Bard);
            if (emoji.Equals(Constants.AocRoleEmojis.Cleric))
                return nameof(Constants.AocRoleEmojis.Cleric);
            if (emoji.Equals(Constants.AocRoleEmojis.Fighter))
                return nameof(Constants.AocRoleEmojis.Fighter);
            if (emoji.Equals(Constants.AocRoleEmojis.Mage))
                return nameof(Constants.AocRoleEmojis.Mage);
            if (emoji.Equals(Constants.AocRoleEmojis.Ranger))
                return nameof(Constants.AocRoleEmojis.Ranger);
            if (emoji.Equals(Constants.AocRoleEmojis.Rogue))
                return nameof(Constants.AocRoleEmojis.Rogue);
            if (emoji.Equals(Constants.AocRoleEmojis.Summoner))
                return nameof(Constants.AocRoleEmojis.Summoner);
            if (emoji.Equals(Constants.AocRoleEmojis.Tank))
                return nameof(Constants.AocRoleEmojis.Tank);

            throw new ArgumentOutOfRangeException(nameof(emoji), "Emoji is unknown.");
        }

        private static string WowEmojiToClassName(EmojiDefinition emoji)
        {
            if (emoji.Equals(Constants.WowRoleEmojis.Druid))
                return nameof(Constants.WowRoleEmojis.Druid);
            if (emoji.Equals(Constants.WowRoleEmojis.Hunter))
                return nameof(Constants.WowRoleEmojis.Hunter);
            if (emoji.Equals(Constants.WowRoleEmojis.Mage))
                return nameof(Constants.WowRoleEmojis.Mage);
            if (emoji.Equals(Constants.WowRoleEmojis.Paladin))
                return nameof(Constants.WowRoleEmojis.Paladin);
            if (emoji.Equals(Constants.WowRoleEmojis.Priest))
                return nameof(Constants.WowRoleEmojis.Priest);
            if (emoji.Equals(Constants.WowRoleEmojis.Rogue))
                return nameof(Constants.WowRoleEmojis.Rogue);
            if (emoji.Equals(Constants.WowRoleEmojis.Warlock))
                return nameof(Constants.WowRoleEmojis.Warlock);
            if (emoji.Equals(Constants.WowRoleEmojis.Warrior))
                return nameof(Constants.WowRoleEmojis.Warrior);

            throw new ArgumentOutOfRangeException(nameof(emoji), "Emoji is unknown.");
        }

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region IGameRoleProvider Members

        public event EventHandler<GameChangedEventArgs> GameChanged;

        IDiscordAccess IGameRoleProvider.DiscordAccess
        {
            set => _discordAccess = value;
        }

        ulong IGameRoleProvider.AocGameRoleMenuMessageID { get; set; }

        ulong IGameRoleProvider.WowGameRoleMenuMessageID { get; set; }

        ulong[] IGameRoleProvider.GamesRolesMenuMessageIDs { get; set; }

        public IReadOnlyList<AvailableGame> Games => _games;

        IReadOnlyList<EmbedData> IGameRoleProvider.GetGameInfoAsEmbedData(string filter)
        {
            var result = new List<EmbedData>();
            var caseInsensitiveFilter = filter?.ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(caseInsensitiveFilter))
                caseInsensitiveFilter = null;

            foreach (var game in _games.OrderBy(m => m.LongName))
            {
                if (caseInsensitiveFilter != null
                    && !game.ShortName.ToLowerInvariant().Contains(caseInsensitiveFilter)
                    && !game.LongName.ToLowerInvariant().Contains(caseInsensitiveFilter))
                {
                    continue;
                }
                
                var fields = new List<EmbedField>();
                var ed = new EmbedData
                {
                    Color = Colors.LightGreen,
                    Title = $"Game information for \"{game.LongName}\" ({game.ShortName})"
                };

                // Primary game role ID
                if (game.PrimaryGameDiscordRoleID != null)
                    fields.Add(new EmbedField(nameof(AvailableGame.PrimaryGameDiscordRoleID), game.PrimaryGameDiscordRoleID.Value, false));

                // Flags
                fields.Add(new EmbedField(nameof(AvailableGame.IncludeInGuildMembersStatistic), game.IncludeInGuildMembersStatistic, false));
                fields.Add(new EmbedField(nameof(AvailableGame.IncludeInGamesMenu), game.IncludeInGamesMenu, false));

                // Game role IDs
                if (game.AvailableRoles.Count > 0)
                {
                    fields.AddRange(game.AvailableRoles
                                        .OrderBy(m => m.RoleName)
                                        .Select(m => new EmbedField($"Game role '{m.RoleName}'", $"DiscordRoleID: {m.DiscordRoleID}", false)));
                }

                ed.Fields = fields.ToArray();
                result.Add(ed);
            }

            return result;
        }

        async Task IGameRoleProvider.SetGameRole(DiscordChannelID channelID, DiscordUserID userID, AvailableGame game, EmojiDefinition emoji)
        {
            if (!_userStore.TryGetUser(userID, out var user))
                return;
            if (!await CanChangeRoles(channelID, user).ConfigureAwait(false))
                return;

            string className;
            switch (game.ShortName)
            {
                case Constants.RoleMenuGameShortNames.AshesOfCreation:
                    className = AocEmojiToClassName(emoji);
                    break;
                case Constants.RoleMenuGameShortNames.WorldOfWarcraftClassic:
                    className = WowEmojiToClassName(emoji);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(game), $"Game '{game.ShortName}' ist not valid for '{nameof(IGameRoleProvider.SetGameRole)}'.");
            }
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

        async Task IGameRoleProvider.RevokeGameRole(DiscordChannelID channelID, DiscordUserID userID, AvailableGame game, EmojiDefinition emoji)
        {
            if (!_userStore.TryGetUser(userID, out var user))
                return;
            if (!await CanChangeRoles(channelID, user).ConfigureAwait(false))
                return;

            string className;
            switch (game.ShortName)
            {
                case Constants.RoleMenuGameShortNames.AshesOfCreation:
                    className = AocEmojiToClassName(emoji);
                    break;
                case Constants.RoleMenuGameShortNames.WorldOfWarcraftClassic:
                    className = WowEmojiToClassName(emoji);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(game), $"Game '{game.ShortName}' ist not valid for '{nameof(IGameRoleProvider.SetGameRole)}'.");
            }
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

        async Task IGameRoleProvider.SetPrimaryGameRole(DiscordChannelID channelID,
                                                        DiscordUserID userID,
                                                        AvailableGame game)
        {
            if (!_userStore.TryGetUser(userID, out var user))
                return;
            if (!await CanChangeRoles(channelID, user).ConfigureAwait(false))
                return;

            var added = await _discordAccess.TryAddPrimaryGameRole(userID, game).ConfigureAwait(false);
            if (added)
            {
                var createdMessages = await _discordAccess.CreateBotMessagesInChannel(channelID, new[] { $"{user.Mention}: The primary role for the game _{game.LongName}_ was **added**." }).ConfigureAwait(false);
                var messageID = createdMessages[0];
                DeleteMessageAfterDelay(channelID, messageID);
                await _discordAccess.LogToDiscord($"User {user.Mention} **added** the primary role for the game _{game.LongName}_.").ConfigureAwait(false);
            }
        }

        async Task IGameRoleProvider.RevokePrimaryGameRole(DiscordChannelID channelID,
                                                           DiscordUserID userID,
                                                           AvailableGame game)
        {
            if (!_userStore.TryGetUser(userID, out var user))
                return;
            if (!await CanChangeRoles(channelID, user).ConfigureAwait(false))
                return;

            var revoked = await _discordAccess.TryRevokePrimaryGameRole(userID, game).ConfigureAwait(false);
            if (revoked)
            {
                var createdMessages = await _discordAccess.CreateBotMessagesInChannel(channelID, new[] { $"{user.Mention}: The primary role for the game _{game.LongName}_ was **revoked**." }).ConfigureAwait(false);
                var messageID = createdMessages[0];
                DeleteMessageAfterDelay(channelID, messageID);
                await _discordAccess.LogToDiscord($"User {user.Mention} **removed** the primary role for the game _{game.LongName}_.").ConfigureAwait(false);
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
                var count = _discordAccess.CountGuildMembersWithRoles(new []{$"{game.ShortName} - {className.RoleName}"});
                distribution.Add(className.RoleName, count);
            }

            var gameMembers = _discordAccess.CountGuildMembersWithRoles(new []{$"{game.LongName} ({game.ShortName})"});

            return (gameMembers, distribution);
        }

        async Task<(bool Success, string Message)> IGameRoleProvider.AddGame(InternalUserID userID,
                                                                             string gameLongName,
                                                                             string gameShortName,
                                                                             ulong? primaryGameDiscordRoleID)
        {
            // Precondition
            if (_games.Any(m => m.LongName == gameLongName))
                return (false, $"Game with the long name `{gameLongName}` already exists.");
            if (_games.Any(m => m.ShortName == gameShortName))
                return (false, $"Game with the short name `{gameShortName}` already exists.");
            if (primaryGameDiscordRoleID != null && !_discordAccess.DoesRoleExist(primaryGameDiscordRoleID.Value))
                return (false, $"Role with the ID `{primaryGameDiscordRoleID.Value}` doesn't exist.");

            // Act
            var (success, error) = await _databaseAccess.TryAddGame(userID, gameLongName, gameShortName, primaryGameDiscordRoleID).ConfigureAwait(false);
            if (!success)
                return (false, $"Failed to add the game: {error}");

            // Update cache
            var newGame = new AvailableGame
            {
                LongName = gameLongName,
                ShortName = gameShortName,
                PrimaryGameDiscordRoleID = primaryGameDiscordRoleID
            };
            _games.Add(newGame);

            GameChanged?.Invoke(this, new GameChangedEventArgs(newGame, GameModification.Added));

            return (true, "The game was added successfully.");
        }

        async Task<(bool Success, string Message, string OldValue)> IGameRoleProvider.EditGame(InternalUserID userID,
                                                                                               string gameShortName,
                                                                                               string property,
                                                                                               string newValue)
        {
            // Precondition
            var gameID = await _databaseAccess.TryGetGameID(gameShortName).ConfigureAwait(false);
            if (gameID == null)
                return (false, $"Couldn't find any game with the short name `{gameShortName}`.", null);

            // Act
            var cachedGame = _games.SingleOrDefault(m => m.ShortName == gameShortName);
            if (cachedGame == null)
                return (false, $"Couldn't find any game with the short name `{gameShortName}`.", null);

            var clone = cachedGame.Clone();

            // If the property is known, and does not equal the current value, it will be updated
            string oldValue;
            switch (property)
            {
                case nameof(AvailableGame.LongName):
                    if (clone.LongName == newValue)
                        return (false, "New value equals the current value.", null);
                    oldValue = clone.LongName;
                    clone.LongName = newValue;
                    break;
                case nameof(AvailableGame.ShortName):
                    if (clone.ShortName == newValue)
                        return (false, "New value equals the current value.", null);
                    oldValue = clone.ShortName;
                    clone.ShortName = newValue;
                    break;
                case nameof(AvailableGame.PrimaryGameDiscordRoleID):
                    if (newValue == "NULL")
                    {
                        oldValue = clone.PrimaryGameDiscordRoleID?.ToString() ?? "<null>";
                        clone.PrimaryGameDiscordRoleID = null;
                    }
                    else
                    {
                        if (!ulong.TryParse(newValue, out var newUlongValue))
                            return (false, "New value cannot be parsed to type ulong.", null);
                        if (clone.PrimaryGameDiscordRoleID == newUlongValue)
                            return (false, "New value equals the current value.", null);
                        oldValue = clone.PrimaryGameDiscordRoleID?.ToString() ?? "<null>";
                        clone.PrimaryGameDiscordRoleID = newUlongValue;
                    }
                    break;
                case nameof(AvailableGame.IncludeInGuildMembersStatistic):
                    if (!bool.TryParse(newValue, out var newIncludeInGuildMembersStatisticValue))
                        return (false, "New value cannot be parsed to type bool.", null);
                    if (clone.IncludeInGuildMembersStatistic == newIncludeInGuildMembersStatisticValue)
                        return (false, "New value equals the current value.", null);
                    oldValue = clone.IncludeInGuildMembersStatistic.ToString();
                    clone.IncludeInGuildMembersStatistic = newIncludeInGuildMembersStatisticValue;
                    break;
                case nameof(AvailableGame.IncludeInGamesMenu):
                    if (!bool.TryParse(newValue, out var newIncludeInGamesMenuValue))
                        return (false, "New value cannot be parsed to type bool.", null);
                    if (clone.IncludeInGamesMenu == newIncludeInGamesMenuValue)
                        return (false, "New value equals the current value.", null);
                    oldValue = clone.IncludeInGamesMenu.ToString();
                    clone.IncludeInGamesMenu = newIncludeInGamesMenuValue;
                    break;
                default:
                    return (false, $"The property `{property}` is not valid.", null);
            }
            var (success, error) = await _databaseAccess.TryEditGame(userID, gameID.Value, clone).ConfigureAwait(false);
            if (!success) return (false, $"Failed to edit the game: {error}", null);

            // Update cache
            cachedGame.LongName = clone.LongName;
            cachedGame.ShortName = clone.ShortName;
            cachedGame.PrimaryGameDiscordRoleID = clone.PrimaryGameDiscordRoleID;
            cachedGame.IncludeInGuildMembersStatistic = clone.IncludeInGuildMembersStatistic;
            cachedGame.IncludeInGamesMenu = clone.IncludeInGamesMenu;

            GameChanged?.Invoke(this, new GameChangedEventArgs(cachedGame, GameModification.Edited));

            return (true, "The game was edited successfully.", oldValue);
        }

        async Task<(bool Success, string Message)> IGameRoleProvider.RemoveGame(string gameShortName)
        {
            // Precondition
            var gameID = await _databaseAccess.TryGetGameID(gameShortName).ConfigureAwait(false);
            if (gameID == null)
                return (false, $"Couldn't find any game with the short name `{gameShortName}`.");

            // Act
            var (success, error) = await _databaseAccess.TryRemoveGame(gameID.Value).ConfigureAwait(false);
            if (!success)
                return (false, $"Failed to remove the game: {error}");

            // Update cache
            var cachedGame = _games.SingleOrDefault(m => m.ShortName == gameShortName);
            if (cachedGame != null)
                _games.Remove(cachedGame);

            GameChanged?.Invoke(this, new GameChangedEventArgs(cachedGame, GameModification.Removed));

            return (true, "The game was removed successfully.");
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

            GameChanged?.Invoke(this, new GameChangedEventArgs(game, GameModification.RoleAdded));

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

            GameChanged?.Invoke(this, new GameChangedEventArgs(gameWithCachedRole, GameModification.RoleRemoved));

            return (true, "The game role was removed successfully.", gameRole.Value.CurrentName);
        }

        #endregion
    }
}