using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using HoU.GuildBot.DAL.Discord.Preconditions;
using HoU.GuildBot.Shared.BLL;
using HoU.GuildBot.Shared.DAL;
using HoU.GuildBot.Shared.Enums;
using HoU.GuildBot.Shared.Extensions;
using HoU.GuildBot.Shared.Objects;
using HoU.GuildBot.Shared.StrongTypes;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace HoU.GuildBot.DAL.Discord.Modules;

public partial class ConfigModule
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    [Group("game", "Game configuration for the bot.")]
    public class ConfigGameModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly IGameRoleProvider _gameRoleProvider;
        private readonly IDiscordAccess _discordAccess;
        private readonly IUserStore _userStore;
        private readonly ILogger<ConfigGameModule> _logger;

        public ConfigGameModule(IGameRoleProvider gameRoleProvider,
                                IDiscordAccess discordAccess,
                                IUserStore userStore,
                                ILogger<ConfigGameModule> logger)
        {
            _gameRoleProvider = gameRoleProvider;
            _discordAccess = discordAccess;
            _userStore = userStore;
            _logger = logger;
        }

        [SlashCommand("list", "Lists the configured games and their details.", runMode: RunMode.Async)]
        [AllowedRoles(Role.Developer | Role.Leader | Role.Officer)]
        public async Task ListAllGamesAsync()
        {
            var embedData = _gameRoleProvider.GetGameInfoAsEmbedData(null);
            await RespondAsync($"Found {embedData.Count} game(s):");
            await embedData.PerformBulkOperation(async data =>
            {
                var embed = data.ToEmbed();
                await ReplyAsync(embed: embed);
            });
        }

        [SlashCommand("get", "Gets details about a specific game.", runMode: RunMode.Async)]
        [AllowedRoles(Role.Developer | Role.Leader | Role.Officer)]
        public async Task GetGameAsync(string searchFilter)
        {
            // Search for games
            var embedData = _gameRoleProvider.GetGameInfoAsEmbedData(searchFilter);
            if (embedData.Count == 0)
            {
                await RespondAsync($"Couldn't find any game matching '{searchFilter}'.");
            }
            else
            {
                await RespondAsync($"Found {embedData.Count} game(s) matching '{searchFilter}':");
                await embedData.PerformBulkOperation(async data =>
                {
                    // Display result
                    var embed = data.ToEmbed();
                    await ReplyAsync(string.Empty, false, embed);
                });
            }
        }

        [SlashCommand("add", "Adds a new game, if it doesn't already exist.")]
        [AllowedRoles(Role.Developer | Role.Leader | Role.Officer)]
        public async Task AddGameAsync(IRole game)
        {
            if (!_userStore.TryGetUser((DiscordUserId)Context.User.Id, out var user))
            {
                await RespondAsync("Couldn't find you in the user store. Game was not added.");
                return;
            }

            // Add
            var (success, message, addedGame) = await _gameRoleProvider.AddGameAsync(user!.InternalUserId, (DiscordRoleId)game.Id);
            if (success && addedGame != null)
            {
                // When the game was added successfully, log the add
                var logMessage = $"{Context.User.Username} added the game **{addedGame.DisplayName} ({game.Mention})**.";
                _logger.LogInformation(logMessage);
                await _discordAccess.LogToDiscord(logMessage);
            }

            await RespondAsync(message);
        }

        [SlashCommand("inc-games-menu", "Sets if the game shall be included in the games menu.")]
        [AllowedRoles(Role.Developer | Role.Leader | Role.Officer)]
        public async Task IncludeGameInGamesMenuAsync(IRole game, bool include)
        {
            if (!_userStore.TryGetUser((DiscordUserId)Context.User.Id, out var user))
            {
                await RespondAsync("Couldn't find you in the user store. Game was not edited.");
                return;
            }

            // Update
            _discordAccess.EnsureDisplayNamesAreSet(_gameRoleProvider.Games);
            var (success, message, updatedGame) =
                await _gameRoleProvider.UpdateGameAsync(user!.InternalUserId,
                                                        (DiscordRoleId)game.Id,
                                                        g => g.IncludeInGamesMenu = include);
            if (success && updatedGame != null)
            {
                // When the game was edited successfully, log the add
                var logMessage = $"{Context.User.Username} changed the property **{nameof(AvailableGame.IncludeInGamesMenu)}** of the game "
                               + $"**{updatedGame.DisplayName}** ({game.Mention}) to **{include}**.";
                _logger.LogInformation(logMessage);
                await _discordAccess.LogToDiscord(logMessage);
            }

            await RespondAsync(message);
        }

        [SlashCommand("inc-statistic", "Sets if the game shall be included in the guild member statistic.")]
        [AllowedRoles(Role.Developer | Role.Leader | Role.Officer)]
        public async Task IncludeGameInGuildMemberStatisticAsync(IRole game, bool include)
        {
            if (!_userStore.TryGetUser((DiscordUserId)Context.User.Id, out var user))
            {
                await RespondAsync("Couldn't find you in the user store. Game was not edited.");
                return;
            }

            // Update
            _discordAccess.EnsureDisplayNamesAreSet(_gameRoleProvider.Games);
            var (success, message, updatedGame) =
                await _gameRoleProvider.UpdateGameAsync(user!.InternalUserId,
                                                        (DiscordRoleId)game.Id,
                                                        g => g.IncludeInGuildMembersStatistic = include);
            if (success && updatedGame != null)
            {
                // When the game was edited successfully, log the add
                var logMessage = $"{Context.User.Username} changed the property **{nameof(AvailableGame.IncludeInGuildMembersStatistic)}** of the game "
                               + $"**{updatedGame.DisplayName}** to **{include}**.";
                _logger.LogInformation(logMessage);
                await _discordAccess.LogToDiscord(logMessage);
            }

            await RespondAsync(message);
        }

        [SlashCommand("set-interest-role", "Sets the role that should be used if non-members have interest to play the game.")]
        [AllowedRoles(Role.Developer | Role.Leader | Role.Officer)]
        public async Task SetGameInterestRoleAsync(IRole game, IRole? interestRole = null)
        {
            if (!_userStore.TryGetUser((DiscordUserId)Context.User.Id, out var user))
            {
                await RespondAsync("Couldn't find you in the user store. Game was not edited.");
                return;
            }

            // Update
            _discordAccess.EnsureDisplayNamesAreSet(_gameRoleProvider.Games);
            var (success, message, updatedGame) =
                await _gameRoleProvider.UpdateGameAsync(user!.InternalUserId,
                                                        (DiscordRoleId)game.Id,
                                                        g => g.GameInterestRoleId = interestRole == null
                                                                                        ? null
                                                                                        : (DiscordRoleId)interestRole.Id);
            if (success && updatedGame != null)
            {
                // When the game was edited successfully, log the add
                var logMessage = $"{Context.User.Username} changed the property **{nameof(AvailableGame.GameInterestRoleId)}** of the game "
                               + $"**{updatedGame.DisplayName}** ({game.Mention}) to **{(interestRole == null ? "<NULL>" : interestRole.Mention)}**.";
                _logger.LogInformation(logMessage);
                await _discordAccess.LogToDiscord(logMessage);

                // If the game can be used as "interest role", send an additional message.
                if (updatedGame.GameInterestRoleId != null)
                {
                    var leaderMention = _discordAccess.GetRoleMention(Constants.RoleNames.LeaderRoleName);
                    var gameInterestNotification =
                        $"{leaderMention} The game '{updatedGame.DisplayName}' can now be used as \"game interest\" in the infos and roles channel.";
                    await _discordAccess.LogToDiscord(gameInterestNotification);
                }
            }

            await RespondAsync(message);
        }

        [SlashCommand("remove", "Removes an existing game, if it exist.")]
        [AllowedRoles(Role.Developer | Role.Leader)]
        public async Task RemoveGameAsync(IRole game)
        {
            // Remove
            var (success, message, removedGame) = await _gameRoleProvider.RemoveGameAsync((DiscordRoleId)game.Id);
            if (success && removedGame != null)
            {
                // When the game was removed successfully, log the remove
                var logMessage = $"{Context.User.Username} removed the game **{removedGame.DisplayName}** ({game.Mention}).";
                _logger.LogInformation(logMessage);
                await _discordAccess.LogToDiscord(logMessage);
            }

            await RespondAsync(message);
        }

        [SlashCommand("add-role", "Adds a new game role, if it doesn't already exist.")]
        [AllowedRoles(Role.Developer | Role.Leader | Role.Officer)]
        public async Task AddGameRoleAsync(IRole game, IRole roleToAdd)
        {
            if (!_userStore.TryGetUser((DiscordUserId)Context.User.Id, out var user))
            {
                await RespondAsync("Couldn't find you in the user store. Game role was not added.");
                return;
            }

            // Add
            var (success, message, addedGameRole) = await _gameRoleProvider.AddGameRoleAsync(user!.InternalUserId,
                                                                                                 (DiscordRoleId)game.Id,
                                                                                                 (DiscordRoleId)roleToAdd.Id);
            if (success && addedGameRole != null)
            {
                // When the role was added successfully, log the add
                var logMessage = $"{Context.User.Username} added the role **{addedGameRole.DisplayName}** ({roleToAdd.Mention}) to the game **{game.Mention}**.";
                _logger.LogInformation(logMessage);
                await _discordAccess.LogToDiscord(logMessage);
            }

            await RespondAsync(message);
        }

        [SlashCommand("remove-role", "Removes an existing game role, if it is currently configured for the game.")]
        [AllowedRoles(Role.Developer | Role.Leader)]
        public async Task RemoveGameRoleAsync(IRole role)
        {
            // Remove
            var (success, message, removedGameRole) = await _gameRoleProvider.RemoveGameRoleAsync((DiscordRoleId)role.Id);
            if (success && removedGameRole != null)
            {
                // When the role was removed successfully, log the remove
                var logMessage = $"{Context.User.Username} removed the role **{removedGameRole.DisplayName}** ({role.Mention}).";
                _logger.LogInformation(logMessage);
                await _discordAccess.LogToDiscord(logMessage);
            }

            await RespondAsync(message);
        }
    }
}