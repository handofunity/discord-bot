namespace HoU.GuildBot.DAL.Discord.Modules;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
[Group("dev", "Administrative commands.")]
public class DeveloperModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IKeycloakSyncService _keycloakSyncService;
    private readonly ILogger<DeveloperModule> _logger;

    public DeveloperModule(IKeycloakSyncService keycloakSyncService,
                           ILogger<DeveloperModule> logger)
    {
        _keycloakSyncService = keycloakSyncService;
        _logger = logger;
    }

    [SlashCommand("restart", "Restarts the bot instance.", runMode: RunMode.Async)]
    [AllowedRoles(Role.Developer)]
    public async Task Restart()
    {
        _logger.LogInformation("Shutdown triggered by 'restart' command. Application will shut down in 10 seconds ...");
        await RespondAsync("Shutting down in 10 seconds. Restart will be performed automatically.");
        _ = Task.Run(async () =>
        {
            await Task.Delay(TimeSpan.FromSeconds(10));
            ApplicationLifecycle.End();
        }).ConfigureAwait(false);
    }

    [SlashCommand("sync-keycloak", "Synchronizes all current guild users with the configured Keycloak endpoints.", runMode: RunMode.Async)]
    [AllowedRoles(Role.Leader | Role.Officer)]
    public async Task SyncUnitsAsync()
    {
        await DeferAsync();
        await _keycloakSyncService.SyncAllUsersAsync();
        await _keycloakSyncService.DeleteFlaggedUsersAsync();
        await FollowupAsync("Synchronization finished.");
    }
}