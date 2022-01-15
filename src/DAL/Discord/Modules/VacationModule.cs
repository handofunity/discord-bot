using System;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using JetBrains.Annotations;
using HoU.GuildBot.DAL.Discord.Preconditions;
using HoU.GuildBot.Shared.BLL;
using HoU.GuildBot.Shared.Enums;
using HoU.GuildBot.Shared.StrongTypes;

namespace HoU.GuildBot.DAL.Discord.Modules;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
[Group("vacation", "Commands related to vacations.")]
public class VacationModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IVacationProvider _vacationProvider;

    public VacationModule(IVacationProvider vacationProvider)
    {
        _vacationProvider = vacationProvider;
    }

    [SlashCommand("add", "Adds a vacation entry.")]
    [AllowedRoles(Role.AnyGuildMember)]
    public async Task AddVacationAsync([Summary(description: "Format: yyyy-MM-dd")] DateTime startDate,
                                       [Summary(description: "Format: yyyy-MM-dd")] DateTime? endDate = null,
                                       [Summary(description: "Optional note that you want to leave for the leadership.")] string? note = null)
    {
        endDate ??= startDate;

        var response = await _vacationProvider.AddVacation((DiscordUserId)Context.User.Id, startDate, endDate.Value, note);
        await RespondAsync(response, ephemeral: true);
    }

    [SlashCommand("list", "Lists all current and upcoming vacations, or for a specific date.")]
    [AllowedRoles(Role.Leader | Role.Officer)]
    public async Task ListAllVacationsAsync([Summary(description: "Format: yyyy-MM-dd")] DateTime? dateFilter = null)
    {
        if (dateFilter == null)
        {
            var response = await _vacationProvider.GetVacations();
            await RespondAsync(response);
        }
        else
        {
            var response = await _vacationProvider.GetVacations(dateFilter.Value);
            await RespondAsync(response);
        }
    }

    [SlashCommand("list-today", "Lists all vacations for today.")]
    [AllowedRoles(Role.Leader | Role.Officer)]
    public async Task ListVacationsForTodayAsync()
    {
        var response = await _vacationProvider.GetVacations(DateTime.Today);
        await RespondAsync(response);
    }

    [SlashCommand("list-tomorrow", "Lists all vacations for tomorrow.")]
    [AllowedRoles(Role.Leader | Role.Officer)]
    public async Task ListVacationsForTomorrowAsync()
    {
        var response = await _vacationProvider.GetVacations(DateTime.Today.AddDays(1));
        await RespondAsync(response);
    }

    [SlashCommand("list-user", "Lists all current and upcoming vacations for a specific user.")]
    [AllowedRoles(Role.Leader | Role.Officer)]
    public async Task ListVacationsForUserAsync(IUser user)
    {
        var response = await _vacationProvider.GetVacations((DiscordUserId)user.Id);
        await RespondAsync(response);
    }

    [SlashCommand("list-mine", "Lists all current and upcoming vacations for you.")]
    [AllowedRoles(Role.AnyGuildMember)]
    public async Task ListUserVacationsAsync()
    {
        var response = await _vacationProvider.GetVacations((DiscordUserId)Context.User.Id);
        await RespondAsync(response, ephemeral: true);
    }

    [SlashCommand("delete", "Deletes a vacation entry.")]
    [AllowedRoles(Role.AnyGuildMember)]
    public async Task DeleteVacationAsync([Summary(description: "Format: yyyy-MM-dd")] DateTime startDate,
                                          [Summary(description: "Format: yyyy-MM-dd")] DateTime? endDate = null)
    {
        endDate ??= startDate;

        var response = await _vacationProvider.DeleteVacation((DiscordUserId)Context.User.Id, startDate, endDate.Value);
        await RespondAsync(response, ephemeral: true);
    }
}