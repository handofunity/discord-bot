namespace HoU.GuildBot.BLL
{
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
        private readonly IPrivacyProvider _privacyProvider;
        private bool _isFirstConnect;

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Constructors

        public BotEngine(ILogger<BotEngine> logger,
                         IDiscordAccess discordAccess,
                         IBotInformationProvider botInformationProvider,
                         IPrivacyProvider privacyProvider)
        {
            _logger = logger;
            _discordAccess = discordAccess;
            _botInformationProvider = botInformationProvider;
            _privacyProvider = privacyProvider;
            _isFirstConnect = true;
        }

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Private Methods

        private async Task Connect()
        {
            await _discordAccess.Connect(ConnectedHandler).ConfigureAwait(false);
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
            await _discordAccess.SetCurrentGame("Hand of Unity | hou!help").ConfigureAwait(false);

            if (_isFirstConnect)
            {
                _isFirstConnect = false;
                await _discordAccess.LogToDiscord($"Bot started on **{_botInformationProvider.GetEnvironmentName()}** in version {_botInformationProvider.GetFormatedVersion()}.");
                // Start privacy provider clean up
                _privacyProvider.Start();
            }

            _logger.LogInformation("Bot ready.");
        }

        #endregion
    }
}