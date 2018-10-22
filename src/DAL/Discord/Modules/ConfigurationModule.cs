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

    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class ConfigurationModule : ModuleBaseHoU
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Fields

        private readonly IMessageProvider _messageProvider;
        private readonly IDiscordAccess _discordAccess;
        private readonly ILogger<ConfigurationModule> _logger;

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Constructors

        public ConfigurationModule(IMessageProvider messageProvider,
                                   IDiscordAccess discordAccess,
                                   ILogger<ConfigurationModule> logger)
        {
            _messageProvider = messageProvider;
            _discordAccess = discordAccess;
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
        public async Task SetMessage([Remainder] string messageContent)
        {
            // Parse message content
            // "(?<name>\w+)" "(?<content>(.|\n|\r)+)"
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
            var response = await _messageProvider.SetMessage(name, content).ConfigureAwait(false);
            if (response.Success)
            {
                // When the message was changed successfully, log the change
                var logMessage = $"{Context.User.Username} changed the message **{name}** to:{Environment.NewLine}{content}";
                _logger.LogInformation(logMessage);
                await _discordAccess.LogToDiscord(logMessage).ConfigureAwait(false);
            }
            await ReplyAsync(response.Response).ConfigureAwait(false);
        }

        #endregion
    }
}