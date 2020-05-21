namespace HoU.GuildBot.BLL
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Hangfire;
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
        private readonly AppSettings _appSettings;
        private readonly IDiscordAccess _discordAccess;
        private readonly IBotInformationProvider _botInformationProvider;
        private readonly IPrivacyProvider _privacyProvider;
        private bool _isFirstConnect;

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Constructors

        public BotEngine(ILogger<BotEngine> logger,
                         AppSettings appSettings,
                         IDiscordAccess discordAccess,
                         IBotInformationProvider botInformationProvider,
                         IPrivacyProvider privacyProvider)
        {
            _logger = logger;
            _appSettings = appSettings;
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
            await _discordAccess.Connect(ConnectedHandler, DisconnectedHandler).ConfigureAwait(false);
        }

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region IBotEngine Members

        async Task IBotEngine.Run()
        {
            var cts = new CancellationTokenSource();

            _logger.LogInformation("Starting bot...");
            EmojiDefinition.InitializeAll();

            // Create connection to Discord
            await Connect().ConfigureAwait(false);

            // Check if the initial connection to the Discord servers is successful,
            // because this won't be handled by the DisconnectedHandler.
#pragma warning disable CS4014 // Fire & Forget
            Task.Run(async () =>
            {
                // In case that the Discord servers are unreachable, we won't be able to make a connection
                // for a few minutes or even hours. Because we don't want to restart and retry too often,
                // waiting 10 minutes to check for the initial connection is okay.
                await Task.Delay(new TimeSpan(0, 10, 0), CancellationToken.None).ConfigureAwait(false);
                if (!_discordAccess.IsConnected)
                {
                    _logger.LogWarning("Shutting down process, because the connection to Discord couldn't be established within 10 minutes.");
                    cts.CancelAfter(2000);
                }
            }, CancellationToken.None).ConfigureAwait(false);
#pragma warning restore CS4014 // Fire & Forget

            // Listen to calls and block the current thread
            try
            {
                await Task.Delay(-1, cts.Token).ConfigureAwait(false);
            }
            catch (TaskCanceledException)
            {
            }
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
                await _discordAccess.LogToDiscord($"Bot started on **{_botInformationProvider.GetEnvironmentName()}** in version {_botInformationProvider.GetFormatedVersion()}.").ConfigureAwait(false);
                // Start privacy provider clean up
                _privacyProvider.Start();

                // Register background jobs (HangFire).
                // Sync all users every 15 minutes to UNITS.
                RecurringJob.AddOrUpdate<UnitsSyncService>("sync-users-to-UNITS", service => service.SyncAllUsers(), "0,15,30,45 0-23 * * *");
                // Sync all users every 15 minutes to UnityHub.
                RecurringJob.AddOrUpdate<UnityHubSyncService>("sync-users-to-UnityHub", service => service.SyncAllUsers(), "0,15,30,45 0-23 * * *");
                // Send personal reminders as scheduled.
                if (_appSettings.PersonalReminders != null)
                {
                    foreach (var personalReminder in _appSettings.PersonalReminders)
                    {
                        RecurringJob.AddOrUpdate<PersonalReminderService>($"personal-reminder-{personalReminder.ReminderId}",
                                                                          service => service.SendReminderAsync(personalReminder.ReminderId),
                                                                          personalReminder.CronSchedule);
                    }
                }
            }

            _logger.LogInformation("Bot ready.");
        }

        private async Task DisconnectedHandler()
        {
            const int delayInSeconds = 30;
            _logger.LogWarning($"Bot disconnected. Checking for connection status in {delayInSeconds} seconds.");

            // The Discord connection might get lost, but should reconnect automatically.
            // In rare cases, the reconnect doesn't work and the bot will just idle.
            // Therefore, after a disconnect, we grant the connection 30 seconds to recover.
            await Task.Delay(TimeSpan.FromSeconds(delayInSeconds)).ConfigureAwait(false);
            if (_discordAccess.IsConnected)
            {
                _logger.LogInformation("Bot re-connected successfully.");
                return;
            }
            // If it doesn't recover during this period of time, we'll kill the process.
            _logger.LogWarning("Failed to recover connection to Discord. Killing the process.");
            // Give the logger some time to log the message
            await Task.Delay(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
            // Finally kill the process to start over
            Environment.Exit(1);
        }

        #endregion
    }
}