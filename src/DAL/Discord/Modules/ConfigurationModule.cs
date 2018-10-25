namespace HoU.GuildBot.DAL.Discord.Modules
{
    using System;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using global::Discord.Commands;
    using JetBrains.Annotations;
    using Microsoft.Extensions.Logging;
    using Preconditions;
    using Shared.Attributes;
    using Shared.BLL;
    using Shared.DAL;
    using Shared.Enums;
    using Shared.Extensions;
    using Shared.Objects;
    using Shared.StrongTypes;

    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class ConfigurationModule : ModuleBaseHoU
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Fields

        private readonly IMessageProvider _messageProvider;
        private readonly IGameRoleProvider _gameRoleProvider;
        private readonly IDiscordAccess _discordAccess;
        private readonly IUserStore _userStore;
        private readonly ILogger<ConfigurationModule> _logger;

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Constructors

        public ConfigurationModule(IMessageProvider messageProvider,
                                   IGameRoleProvider gameRoleProvider,
                                   IDiscordAccess discordAccess,
                                   IUserStore userStore,
                                   ILogger<ConfigurationModule> logger)
        {
            _messageProvider = messageProvider;
            _gameRoleProvider = gameRoleProvider;
            _discordAccess = discordAccess;
            _userStore = userStore;
            _logger = logger;
        }

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Commands

        [Command("list messages")]
        [CommandCategory(CommandCategory.Administration, 4)]
        [Name("List all bot messages")]
        [Summary("Lists all configurable messages the bot uses.")]
        [Alias("listmessages")]
        [RequireContext(ContextType.Guild)]
        [ResponseContext(ResponseType.AlwaysSameChannel)]
        [RolePrecondition(Role.Developer | Role.Leader)]
        public async Task ListAllMessagesAsync()
        {
            var messages = await _messageProvider.ListAllMessages().ConfigureAwait(false);
#pragma warning disable CS4014 // Fire & forget
            Task.Run(async () =>
            {
                await messages.PerformBulkOperation(async message =>
                {
                    await ReplyAsync($"```markdown{Environment.NewLine}" +
                                     $"Message: \"{message.Name}\"{Environment.NewLine}" +
                                     $"Description: {message.Description}{Environment.NewLine}" +
                                     $"================{Environment.NewLine}" +
                                     $"Current content:{Environment.NewLine}" +
                                     "================```").ConfigureAwait(false);
                    await Task.Delay(Constants.GlobalActionDelay).ConfigureAwait(false);
                    var content = message.Content;
                    if (content.StartsWith("```")
                     && content.EndsWith("```"))
                    {
                        content = $"`{content.Substring(3, content.Length - 6)}`";
                    }
                    else if (content.Length <= 1998)
                    {
                        content = $"`{content}`";
                    }
                    await ReplyAsync(content);
                }).ConfigureAwait(false);
            }).ConfigureAwait(false);
#pragma warning restore CS4014 // Fire & forget
        }

        [Command("set message")]
        [CommandCategory(CommandCategory.Administration, 5)]
        [Name("Set a specific bot message")]
        [Summary("Sets a specific, configurable message the bot uses.")]
        [Remarks("Syntax: _set message \"NAME\" \"CONTENT\"_ e.g.: _set message \"FirstServerJoinWelcome\" \"Welcome to the server!\"_")]
        [Alias("setmessage")]
        [RequireContext(ContextType.Guild)]
        [ResponseContext(ResponseType.AlwaysSameChannel)]
        [RolePrecondition(Role.Developer | Role.Leader)]
        public async Task SetMessageAsync([Remainder] string messageContent)
        {
            // Parse message content
            var regex = new Regex("^\"(?<name>\\w+)\" \"(?<content>(.|\\n|\\r)+)\"$");
            var match = regex.Match(messageContent);
            if (!match.Success)
            {
                await ReplyAsync("Couldn't parse command parameter from message content. Please use the help function to see the correct command syntax.").ConfigureAwait(false);
                return;
            }

            // Update
            var name = match.Groups["name"].Value;
            var content = match.Groups["content"].Value;
            var (success, message) = await _messageProvider.SetMessage(name, content).ConfigureAwait(false);
            if (success)
            {
                // When the message was changed successfully, log the change
                var logMessage = $"{Context.User.Username} changed the message **{name}** to:{Environment.NewLine}{content}";
                _logger.LogInformation(logMessage);
                await _discordAccess.LogToDiscord(logMessage).ConfigureAwait(false);
            }
            await ReplyAsync(message).ConfigureAwait(false);
        }

        // TODO Administration Command #6: Add game
        // TODO Administration Command #7: Edit game
        // TODO Administration Command #8: Remove game

        [Command("add game role")]
        [CommandCategory(CommandCategory.Administration, 9)]
        [Name("Adds a new game role")]
        [Summary("Adds a new game role, if it doesn't already exist.")]
        [Remarks("Syntax: _add game role \"GameShortName\" \"RoleName\" \"DiscordRoleID\"_ e.g.: _add game role \"AoC\" \"Bard\" \"5150540654445\"_")]
        [Alias("addgamerole")]
        [RequireContext(ContextType.Guild)]
        [ResponseContext(ResponseType.AlwaysSameChannel)]
        [RolePrecondition(Role.Developer | Role.Leader)]
        public async Task AddGameRoleAsync([Remainder] string messageContent)
        {
            // Parse message content
            var regex = new Regex("^\"(?<gameShortName>\\w+)\" \"(?<roleName>\\w+)\" \"(?<discordRoleID>\\d+)\"$");
            var match = regex.Match(messageContent);
            if (!match.Success)
            {
                await ReplyAsync("Couldn't parse command parameter from message content. Please use the help function to see the correct command syntax.").ConfigureAwait(false);
                return;
            }

            if(!_userStore.TryGetUser((DiscordUserID)Context.User.Id, out var user))
                return;

            // Add
            var gameShortName = match.Groups["gameShortName"].Value;
            var roleName = match.Groups["roleName"].Value;
            var discordRoleID = Convert.ToUInt64(match.Groups["discordRoleID"].Value);
            var (success, message) = await _gameRoleProvider.AddGameRole(user.InternalUserID, gameShortName, roleName, discordRoleID).ConfigureAwait(false);
            if (success)
            {
                // When the role was added successfully, log the add
                var logMessage = $"{Context.User.Username} added the role **{roleName}** to the game **{gameShortName}**.";
                _logger.LogInformation(logMessage);
                await _discordAccess.LogToDiscord(logMessage).ConfigureAwait(false);
            }

            await ReplyAsync(message).ConfigureAwait(false);
        }

        [Command("edit game role")]
        [CommandCategory(CommandCategory.Administration, 10)]
        [Name("Edits an existing game role")]
        [Summary("Edits an existing game role, if it exist.")]
        [Remarks("Syntax: edit game role \"DiscordRoleID\" \"NewRoleName\"_ e.g.: _edit game role \"5150540654445\" \"Bards\"_")]
        [Alias("editgamerole")]
        [RequireContext(ContextType.Guild)]
        [ResponseContext(ResponseType.AlwaysSameChannel)]
        [RolePrecondition(Role.Developer | Role.Leader)]
        public async Task EditGameRoleAsync([Remainder] string messageContent)
        {
            // Parse message content
            var regex = new Regex("^\"(?<discordRoleID>\\d+)\" \"(?<newRoleName>\\w+)\"$");
            var match = regex.Match(messageContent);
            if (!match.Success)
            {
                await ReplyAsync("Couldn't parse command parameter from message content. Please use the help function to see the correct command syntax.").ConfigureAwait(false);
                return;
            }

            if (!_userStore.TryGetUser((DiscordUserID)Context.User.Id, out var user))
                return;

            // Edit
            var discordRoleID = Convert.ToUInt64(match.Groups["discordRoleID"].Value);
            var newRoleName = match.Groups["newRoleName"].Value;
            var (success, message, oldRoleName) = await _gameRoleProvider.EditGameRole(user.InternalUserID, discordRoleID, newRoleName).ConfigureAwait(false);
            if (success)
            {
                // When the role was edited successfully, log the edit
                var logMessage = $"{Context.User.Username} changed the name of the role **{oldRoleName}** to **{newRoleName}**.";
                _logger.LogInformation(logMessage);
                await _discordAccess.LogToDiscord(logMessage).ConfigureAwait(false);
            }

            await ReplyAsync(message).ConfigureAwait(false);
        }

        [Command("remove game role")]
        [CommandCategory(CommandCategory.Administration, 11)]
        [Name("Remove an existing game role")]
        [Summary("Removes an existing game role, if it exist.")]
        [Remarks("Syntax: remove game role \"DiscordRoleID\"_ e.g.: _remove game role \"5150540654445\"_")]
        [Alias("removegamerole")]
        [RequireContext(ContextType.Guild)]
        [ResponseContext(ResponseType.AlwaysSameChannel)]
        [RolePrecondition(Role.Developer | Role.Leader)]
        public async Task RemoveGameRoleAsync([Remainder] string messageContent)
        {
            // Parse message content
            var regex = new Regex("^\"(?<discordRoleID>\\d+)\"$");
            var match = regex.Match(messageContent);
            if (!match.Success)
            {
                await ReplyAsync("Couldn't parse command parameter from message content. Please use the help function to see the correct command syntax.").ConfigureAwait(false);
                return;
            }

            // Remove
            var discordRoleID = Convert.ToUInt64(match.Groups["discordRoleID"].Value);
            var (success, message, oldRoleName) = await _gameRoleProvider.RemoveGameRole(discordRoleID).ConfigureAwait(false);
            if (success)
            {
                // When the role was removed successfully, log the remove
                var logMessage = $"{Context.User.Username} removed the role **{oldRoleName}**.";
                _logger.LogInformation(logMessage);
                await _discordAccess.LogToDiscord(logMessage).ConfigureAwait(false);
            }

            await ReplyAsync(message).ConfigureAwait(false);
        }

        #endregion
    }
}