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
    using Shared.DAL;

    [UsedImplicitly]
    public class DiscordAccess : IDiscordAccess
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Fields

        private readonly ILogger<DiscordAccess> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Constructors

        public DiscordAccess(ILogger<DiscordAccess> logger,
                             IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _client = new DiscordSocketClient();
            _commands = new CommandService();
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

            var argPos = 0;
            if (userMessage.HasStringPrefix("hou!", ref argPos, StringComparison.OrdinalIgnoreCase) // Take action when the prefix is at the beginning
             || userMessage.HasMentionPrefix(_client.CurrentUser, ref argPos)) // Take action when the bot is mentioned
            {
                var context = new SocketCommandContext(_client, userMessage);
                var result = await _commands.ExecuteAsync(context, argPos, _serviceProvider).ConfigureAwait(false);
                if (!result.IsSuccess && result.Error != CommandError.UnknownCommand)
                {
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
    }
}