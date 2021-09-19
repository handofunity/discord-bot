using System.Linq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using HoU.GuildBot.Shared.BLL;
using HoU.GuildBot.Shared.DAL;
using HoU.GuildBot.Shared.Objects;

namespace HoU.GuildBot.BLL
{
    [UsedImplicitly]
    public class BotEngine : IBotEngine
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Fields

        private readonly ILogger<BotEngine> _logger;
        private readonly AppSettings _appSettings;
        private readonly IDiscordAccess _discordAccess;
        private readonly IUnitsSignalRClient _unitsSignalRClient;
        private readonly IBotInformationProvider _botInformationProvider;
        private readonly IPrivacyProvider _privacyProvider;
        private bool _isFirstConnect;

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Constructors

        public BotEngine(ILogger<BotEngine> logger,
                         AppSettings appSettings,
                         IDiscordAccess discordAccess,
                         IUnitsSignalRClient unitsSignalRClient,
                         IBotInformationProvider botInformationProvider,
                         IPrivacyProvider privacyProvider)
        {
            _logger = logger;
            _appSettings = appSettings;
            _discordAccess = discordAccess;
            _unitsSignalRClient = unitsSignalRClient;
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
            _ = Task.Run(async () =>
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
                RecurringJob.AddOrUpdate<IUnitsSyncService>("sync-users-to-UNITS", service => service.SyncAllUsers(), "0,15,30,45 0-23 * * *");
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
                RecurringJob.AddOrUpdate<IRoleRemover>("remove-basement-role", remover => remover.RemoveBasementRolesAsync(), "0 0-23 * * *");

                _ = Task.Run(async () =>
                {
                    if (_appSettings.UnitsAccess == null
                        || _appSettings.UnitsAccess.Length == 0)
                    {
                        _logger.LogWarning("No UNITS access configured.");
                    }
                    else
                    {
                        try
                        {
                            // Connect to UNITS to receive push notifications
                            foreach (var unitsSyncData in _appSettings.UnitsAccess.Where(m => !string.IsNullOrWhiteSpace(m.BaseAddress)
                                                                                              && !string.IsNullOrWhiteSpace(m.Secret)
                                                                                              && m.ConnectToNotificationHub))
                            {
                                await _unitsSignalRClient.ConnectAsync(unitsSyncData);
                            }
                        }
                        catch (Exception e)
                        {
                            _logger.LogCritical(e, "Failed to initialize SignalR connections.");
                        }
                    }
                });
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
            ApplicationLifecycle.End();
        }

        #endregion
    }
}