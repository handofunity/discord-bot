using System;
using System.Threading.Tasks;
using Discord.Interactions;
using HoU.GuildBot.DAL.Discord.Preconditions;
using HoU.GuildBot.Shared.BLL;
using HoU.GuildBot.Shared.Enums;
using HoU.GuildBot.Shared.Objects;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace HoU.GuildBot.DAL.Discord.Modules;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
[Group("admin", "Administrative commands.")]
public class AdminModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IUnitsSyncService _unitsSyncService;
    private readonly ILogger<AdminModule> _logger;

    public AdminModule(IUnitsSyncService unitsSyncService,
                       ILogger<AdminModule> logger)
    {
        _unitsSyncService = unitsSyncService;
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

    [SlashCommand("sync-units", "Synchronizes all current guild users with the configured UNITS endpoints.", runMode: RunMode.Async)]
    [AllowedRoles(Role.Leader | Role.Officer)]
    public async Task SyncUnitsAsync()
    {
        await DeferAsync();
        await _unitsSyncService.SyncAllUsers();
        await FollowupAsync("Synchronization finished.");
    }
}