namespace HoU.GuildBot.BLL
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using JetBrains.Annotations;
    using Microsoft.Extensions.Logging;
    using Shared.BLL;
    using Shared.DAL;

    [UsedImplicitly]
    public class BotEngine : IBotEngine
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Fields

        private readonly ILogger<BotEngine> _logger;
        private readonly IDiscordAccess _discordAccess;
        private readonly IBotInformationProvider _botInformationProvider;

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Constructors

        public BotEngine(ILogger<BotEngine> logger,
                         IDiscordAccess discordAccess,
                         IBotInformationProvider botInformationProvider)
        {
            _logger = logger;
            _discordAccess = discordAccess;
            _botInformationProvider = botInformationProvider;
        }

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Private Methods

        private async Task Connect()
        {
            await _discordAccess.Connect(ConnectedHandler, DisconnectedHandler).ConfigureAwait(false);
        }

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region IBotEngine Members

        async Task IBotEngine.Run()
        {
            _logger.LogInformation("Starting bot...");

            // Create connection to Discord
            await Connect().ConfigureAwait(false);

            // Listen to calls and block the current thread
            await Task.Delay(-1).ConfigureAwait(false);
        }

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Event Handler

        private async Task ConnectedHandler()
        {
            _logger.LogInformation("Connected to Discord.");
            
            await _discordAccess.SetCurrentGame("Hand of Unity | hou!help").ConfigureAwait(false);
            await _discordAccess.Log($"Bot started in the environment **{_botInformationProvider.GetEnvironmentName()}**.");
        }

        private async Task DisconnectedHandler(Exception exception)
        {
            _logger.LogWarning("Lost connection to Discord.");
            if (exception != null)
            {
                _logger.LogError(exception, "Connection losted due to unexpected error.");
            }

            _logger.LogInformation("Connecting to Discord in 10 seconds...");
            await Task.Delay(10_000, CancellationToken.None).ConfigureAwait(false);
            await Connect().ConfigureAwait(false);
        }

        #endregion
    }
}