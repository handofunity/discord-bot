namespace HoU.GuildBot.BLL
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using JetBrains.Annotations;
    using Microsoft.Extensions.Logging;
    using Shared.BLL;
    using Shared.DAL;
    using Shared.Objects;

    [UsedImplicitly]
    public class BotEngine : IBotEngine
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Fields

        private readonly ILogger<BotEngine> _logger;
        private readonly IDiscordAccess _discordAccess;

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Constructors

        public BotEngine(ILogger<BotEngine> logger,
                        IDiscordAccess discordAccess)
        {
            _logger = logger;
            _discordAccess = discordAccess;
        }

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Private Methods

        private async Task Connect(string botToken)
        {
            await _discordAccess.Connect(botToken, ConnectedHandler, DisconnectedHandler).ConfigureAwait(false);
        }

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region IBotEngine Members

        async Task IBotEngine.Run(BotEngineArguments arguments)
        {
            // Create connection to Discord
            await Connect(arguments.BotToken).ConfigureAwait(false);

            // Listen to calls
            await Task.Delay(-1).ConfigureAwait(false);
        }

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Event Handler

        private async Task ConnectedHandler()
        {
            _logger.LogInformation("Connected to Discord.");

            await _discordAccess.SetCurrentGame("serving Hand of Unity").ConfigureAwait(false);

            await Task.CompletedTask.ConfigureAwait(false);
        }

        private async Task DisconnectedHandler(string lastBotToken, Exception exception)
        {
            _logger.LogWarning("Lost connection to Discord.");
            if (exception != null)
            {
                _logger.LogError(exception, "Connection losted due to unexpected error.");
            }

            _logger.LogInformation("Connecting to Discord in 10 seconds...");
            await Task.Delay(10_000, CancellationToken.None).ConfigureAwait(false);
            await Connect(lastBotToken).ConfigureAwait(false);
        }

        #endregion
    }
}