using System.Globalization;

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

    private async Task<DateTime?> TryParseDateAsync(string input)
    {
        if (DateTime.TryParseExact(input, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            return date;

        await FollowupAsync("Invalid date value - format must be `yyyy-MM-dd`");
        return null;
    }

    [SlashCommand("add", "Adds a vacation entry.")]
    [AllowedRoles(Role.AnyGuildMember)]
    public async Task AddVacationAsync([Summary(description: "Format: yyyy-MM-dd")] string startDate,
                                       [Summary(description: "Format: yyyy-MM-dd")] string? endDate = null,
                                       [Summary(description: "Optional note that you want to leave for the leadership.")] string? note = null)
    {
        await DeferAsync();

        endDate ??= startDate;
        var startDateValue = await TryParseDateAsync(startDate);
        if (startDateValue is null)
            return;
        var endDateValue = await TryParseDateAsync(endDate);
        if (endDateValue is null)
            return;

        var response = await _vacationProvider.AddVacation((DiscordUserId)Context.User.Id, startDateValue.Value, endDateValue.Value, note);
        await FollowupAsync(response, ephemeral: true);
    }

    [SlashCommand("list", "Lists all current and upcoming vacations, or for a specific date.")]
    [AllowedRoles(Role.Leader | Role.Officer | Role.Coordinator)]
    public async Task ListAllVacationsAsync([Summary(description: "Format: yyyy-MM-dd")] string? dateFilter = null)
    {
        await DeferAsync();
        if (dateFilter == null)
        {
            var response = await _vacationProvider.GetVacations();
            await FollowupAsync(response);
        }
        else
        {
            var dateFilterValue = await TryParseDateAsync(dateFilter);
            if (dateFilterValue is null)
                return;
            var response = await _vacationProvider.GetVacations(dateFilterValue.Value);
            await FollowupAsync(response);
        }
    }

    [SlashCommand("list-today", "Lists all vacations for today.")]
    [AllowedRoles(Role.Leader | Role.Officer | Role.Coordinator)]
    public async Task ListVacationsForTodayAsync()
    {
        await DeferAsync();
        var response = await _vacationProvider.GetVacations(DateTime.Today);
        await FollowupAsync(response);
    }

    [SlashCommand("list-tomorrow", "Lists all vacations for tomorrow.")]
    [AllowedRoles(Role.Leader | Role.Officer | Role.Coordinator)]
    public async Task ListVacationsForTomorrowAsync()
    {
        await DeferAsync();
        var response = await _vacationProvider.GetVacations(DateTime.Today.AddDays(1));
        await FollowupAsync(response);
    }

    [SlashCommand("list-user", "Lists all current and upcoming vacations for a specific user.")]
    [AllowedRoles(Role.Leader | Role.Officer | Role.Coordinator)]
    public async Task ListVacationsForUserAsync(IUser user)
    {
        await DeferAsync();
        var response = await _vacationProvider.GetVacations((DiscordUserId)user.Id);
        await FollowupAsync(response);
    }

    [SlashCommand("list-mine", "Lists all current and upcoming vacations for you.")]
    [AllowedRoles(Role.AnyGuildMember)]
    public async Task ListUserVacationsAsync()
    {
        await DeferAsync();
        var response = await _vacationProvider.GetVacations((DiscordUserId)Context.User.Id);
        await FollowupAsync(response, ephemeral: true);
    }

    [SlashCommand("delete", "Deletes a vacation entry.")]
    [AllowedRoles(Role.AnyGuildMember)]
    public async Task DeleteVacationAsync([Summary(description: "Format: yyyy-MM-dd")] string startDate,
                                          [Summary(description: "Format: yyyy-MM-dd")] string? endDate = null)
    {
        await DeferAsync();

        endDate ??= startDate;
        var startDateValue = await TryParseDateAsync(startDate);
        if (startDateValue is null)
            return;
        var endDateValue = await TryParseDateAsync(endDate);
        if (endDateValue is null)
            return;

        var response = await _vacationProvider.DeleteVacation((DiscordUserId)Context.User.Id, startDateValue.Value, endDateValue.Value);
        await FollowupAsync(response, ephemeral: true);
    }
}