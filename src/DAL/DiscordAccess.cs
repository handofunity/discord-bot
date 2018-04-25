namespace HoU.GuildBot.DAL
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Discord;
    using Discord.Commands;
    using Discord.WebSocket;
    using JetBrains.Annotations;
    using Microsoft.Extensions.Logging;
    using Shared.BLL;
    using Shared.DAL;
    using Shared.Enums;

    [UsedImplicitly]
    public class DiscordAccess : IDiscordAccess
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Fields

        private readonly ILogger<DiscordAccess> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly ISpamGuard _spamGuard;
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Constructors

        public DiscordAccess(ILogger<DiscordAccess> logger,
                             IServiceProvider serviceProvider,
                             ISpamGuard spamGuard)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _spamGuard = spamGuard;
            _client = new DiscordSocketClient();
            _commands = new CommandService();
        }

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Private Methods

        private async Task<bool> IsSpam(SocketMessage userMessage)
        {
            var checkResult = _spamGuard.CheckForSpam(userMessage.Author.Id, userMessage.Channel.Id, userMessage.Content);
            switch (checkResult)
            {
                case SpamCheckResult.SoftLimitHit:
                {
                    var g = _client.Guilds.Single(m => m.TextChannels.Contains(userMessage.Channel));
                    var leaderRole = g.Roles.Single(m => m.Name == "Leader");
                    var officerRole = g.Roles.Single(m => m.Name == "Officer");
                    var embedBuilder = new EmbedBuilder()
                                       .WithColor(Color.DarkPurple)
                                       .WithTitle("Spam warning")
                                       .WithDescription("Please refrain from further spamming in this channel.");
                    var embed = embedBuilder.Build();
                    await userMessage
                          .Channel.SendMessageAsync($"{userMessage.Author.Mention} - {leaderRole.Mention} and {officerRole.Mention} have been notified.", false, embed)
                          .ConfigureAwait(false);
                    return true;
                }
                case SpamCheckResult.BetweenSoftAndHardLimit:
                {
                    return true;
                }
                case SpamCheckResult.HardLimitHit:
                {
                    var g = _client.Guilds.Single(m => m.TextChannels.Contains(userMessage.Channel));
                    var guildUser = g.GetUser(userMessage.Author.Id);
                    await guildUser.KickAsync("Excesive spam.", RequestOptions.Default).ConfigureAwait(false);
                    return true;
                }
            }

            return false;
        }

        private async Task ProcessMessage(SocketUserMessage userMessage)
        {
            var argPos = 0;
            if (userMessage.HasStringPrefix("hou!", ref argPos, StringComparison.OrdinalIgnoreCase) // Take action when the prefix is at the beginning
             || userMessage.HasMentionPrefix(_client.CurrentUser, ref argPos)) // Take action when the bot is mentioned
            {
                var context = new SocketCommandContext(_client, userMessage);
                var result = await _commands.ExecuteAsync(context, argPos, _serviceProvider).ConfigureAwait(false);
                if (!result.IsSuccess && result.Error != CommandError.UnknownCommand)
                {
                    // Handle error during command execution
                    var embedBuilder = new EmbedBuilder()
                                       .WithColor(Color.Red)
                                       .WithTitle("Error during command execution")
                                       .WithDescription(result.ErrorReason);
                    var embed = embedBuilder.Build();
                    _logger.LogWarning(result.ErrorReason);
                    await userMessage.Channel.SendMessageAsync(string.Empty, false, embed).ConfigureAwait(false);
                }
            }
        }

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region IDiscordAccess Members

        async Task IDiscordAccess.Connect(string botToken, Func<Task> connectedHandler, Func<string, Exception, Task> disconnectedHandler)
        {
            if (botToken == null)
                throw new ArgumentNullException(nameof(botToken));
            if (connectedHandler == null)
                throw new ArgumentNullException(nameof(connectedHandler));
            if (disconnectedHandler == null)
                throw new ArgumentNullException(nameof(disconnectedHandler));
            if (string.IsNullOrWhiteSpace(botToken))
                throw new ArgumentException(nameof(botToken));

            try
            {
                _logger.LogInformation("Connecting to Discord...");
                _client.Connected += async () =>
                {
                    await _commands.AddModulesAsync(typeof(DiscordAccess).Assembly).ConfigureAwait(false);
                    _client.MessageReceived += Client_MessageReceived;
                    await connectedHandler().ConfigureAwait(false);
                };
                _client.Disconnected += exception =>
                {
                    _client.MessageReceived -= Client_MessageReceived;
                    return disconnectedHandler(botToken, exception);
                };

                await _client.LoginAsync(TokenType.Bot, botToken).ConfigureAwait(false);
                await _client.StartAsync().ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unexpected error while connecting to Discord.");
            }
        }

        async Task IDiscordAccess.SetCurrentGame(string gameName)
        {
            await _client.SetGameAsync(gameName).ConfigureAwait(false);
        }

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Event Handler

        private async Task Client_MessageReceived(SocketMessage message)
        {
            SocketUserMessage userMessage;
            if ((userMessage = message as SocketUserMessage) == null)
                // If the message is not a user message, we don't need to handle it
                return;

            if (userMessage.Author.Id == _client.CurrentUser.Id || userMessage.Author.IsBot)
                // If the message is from this bot, or any other bot, we don't need to handle it
                return;

            if (_client.DMChannels.Contains(userMessage.Channel))
                // We don't reply to direct messages
                return;

            // Check for spam
            if (await IsSpam(userMessage).ConfigureAwait(false)) return;

            // If the message is no spam, process message
            await ProcessMessage(userMessage).ConfigureAwait(false);
        }

        #endregion
    }
}