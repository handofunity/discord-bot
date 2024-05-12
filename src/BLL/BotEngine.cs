namespace HoU.GuildBot.BLL;

[UsedImplicitly]
public class BotEngine : IBotEngine
{
    private readonly ILogger<BotEngine> _logger;
    private readonly IDynamicConfiguration _dynamicConfiguration;
    private readonly IDiscordAccess _discordAccess;
    private readonly IUnitsSignalRClient _unitsSignalRClient;
    private readonly IBotInformationProvider _botInformationProvider;
    private readonly IPrivacyProvider _privacyProvider;
    private bool _isFirstConnect;

    public BotEngine(ILogger<BotEngine> logger,
                     IDynamicConfiguration dynamicConfiguration,
                     IDiscordAccess discordAccess,
                     IUnitsSignalRClient unitsSignalRClient,
                     IBotInformationProvider botInformationProvider,
                     IPrivacyProvider privacyProvider)
    {
        _logger = logger;
        _dynamicConfiguration = dynamicConfiguration;
        _dynamicConfiguration.DataLoaded += DynamicConfiguration_DataLoaded;
        _discordAccess = discordAccess;
        _unitsSignalRClient = unitsSignalRClient;
        _botInformationProvider = botInformationProvider;
        _privacyProvider = privacyProvider;
        _isFirstConnect = true;
    }

    private async Task Connect()
    {
        await _discordAccess.ConnectAsync(ConnectedHandler, DisconnectedHandler);
    }

    private void SchedulePersonalReminders()
    {
        foreach (var scheduledReminderInfo in _dynamicConfiguration.ScheduledReminderInfos)
        {
            RecurringJob.AddOrUpdate<ReminderService>($"scheduled-reminder-{scheduledReminderInfo.ReminderId}",
                                                      service => service.SendReminderAsync(scheduledReminderInfo.ReminderId),
                                                      scheduledReminderInfo.CronSchedule);
        }
    }

    private void EstablishUnitsSignalRConnections()
    {
        _ = Task.Run(async () =>
        {
            if (_dynamicConfiguration.UnitsEndpoints.Length == 0)
            {
                _logger.LogWarning("No UNITS access configured");
            }
            else
            {
                try
                {
                    // Connect to UNITS to receive push notifications
                    foreach (var unitsSyncData in _dynamicConfiguration.UnitsEndpoints.Where(m => !string.IsNullOrWhiteSpace(m.BaseAddress.ToString())
                                                                                              && m.ConnectToNotificationHub))
                    {
                        await _unitsSignalRClient.ConnectAsync(unitsSyncData);
                    }
                }
                catch (Exception e)
                {
                    _logger.LogCritical(e, "Failed to initialize SignalR connections");
                }
            }
        }).ConfigureAwait(false);
    }

    async Task IBotEngine.Run(CancellationToken cancellationToken)
    {
        var cts = new CancellationTokenSource();

        _logger.LogInformation("Starting bot...");

        // Create connection to Discord
        await Connect();

        // Check if the initial connection to the Discord servers is successful,
        // because this won't be handled by the DisconnectedHandler.
        _ = Task.Run(async () =>
        {
            // In case that the Discord servers are unreachable, we won't be able to make a connection
            // for a few minutes or even hours. Because we don't want to restart and retry too often,
            // waiting 10 minutes to check for the initial connection is okay.
            await Task.Delay(new TimeSpan(0, 10, 0), CancellationToken.None);
            if (!_discordAccess.IsConnected)
            {
                _logger.LogWarning("Shutting down process, because the connection to Discord couldn't be established within 10 minutes");
                cts.CancelAfter(2000);
            }
        }, CancellationToken.None).ConfigureAwait(false);

        var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, cancellationToken);
        // Listen to calls and block the current thread
        try
        {
            await Task.Delay(-1, combinedCts.Token);
        }
        catch (TaskCanceledException)
        {
        }
    }

    private async Task ConnectedHandler()
    {
        await _discordAccess.SetCurrentGameAsync("Hand of Unity");

        if (_isFirstConnect)
        {
            _isFirstConnect = false;
            await _discordAccess.LogToDiscordAsync($"Bot started on **{_botInformationProvider.GetEnvironmentName()}** in version {_botInformationProvider.GetFormattedVersion()}.");
            // Start privacy provider clean up
            _privacyProvider.Start();

            // Register background jobs (HangFire).
            // Sync all users every 15 minutes to Keycloak.
            RecurringJob.AddOrUpdate<IKeycloakSyncService>("sync-users-to-Keycloak", service => service.SyncAllUsersAsync(), "0,15,30,45 0-23 * * *");
            // Delete flagged users in Keycloak once a day.
            RecurringJob.AddOrUpdate<IKeycloakSyncService>("delete-flagged-users-in-Keycloak", service => service.DeleteFlaggedUsersAsync(), "30 4 * * *");
            // Send personal reminders as scheduled.
            SchedulePersonalReminders();
            // Apply birthday role at 09:00 community time (UTC).
            RecurringJob.AddOrUpdate<BirthdayService>("birthday-apply-role", service => service.ApplyRoleAsync(), "0 9 * * *");
            // Remove birthday role at 23:59 community time (UTC).
            RecurringJob.AddOrUpdate<BirthdayService>("birthday-revoke-role", service => service.RevokeRoleAsync(), "59 23 * * *");
            // Remove basement role every hour.
            RecurringJob.AddOrUpdate<IRoleRemover>("remove-basement-role", remover => remover.RemoveBasementRolesAsync(), "0 0-23 * * *");
            // Remove stale trial members every day at noon.
            RecurringJob.AddOrUpdate<IRoleRemover>("remove-stale-trial-members", remover => remover.RemoveStaleTrialMembersAsync(), "0 12 * * *");

            EstablishUnitsSignalRConnections();
        }

        _logger.LogInformation("Bot ready");
    }

    private async Task DisconnectedHandler()
    {
        const int delayInSeconds = 30;
        _logger.LogWarning("Bot disconnected. Checking for connection status in {DelayInSeconds} seconds", delayInSeconds);

        // The Discord connection might get lost, but should reconnect automatically.
        // In rare cases, the reconnect doesn't work and the bot will just idle.
        // Therefore, after a disconnect, we grant the connection 30 seconds to recover.
        await Task.Delay(TimeSpan.FromSeconds(delayInSeconds));
        if (_discordAccess.IsConnected)
        {
            _logger.LogInformation("Bot re-connected successfully");
            return;
        }
        // If it doesn't recover during this period of time, we'll kill the process.
        _logger.LogWarning("Failed to recover connection to Discord. Killing the process");
        // Give the logger some time to log the message
        await Task.Delay(TimeSpan.FromSeconds(5));
        // Finally kill the process to start over
        ApplicationLifecycle.End();
    }

    private void DynamicConfiguration_DataLoaded(object? sender, EventArgs e)
    {
        SchedulePersonalReminders();
        EstablishUnitsSignalRConnections();
    }
}