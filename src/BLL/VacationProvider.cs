namespace HoU.GuildBot.BLL;

[UsedImplicitly]
public class VacationProvider : IVacationProvider
{
    private readonly IUserStore _userStore;
    private readonly IDatabaseAccess _databaseAccess;
    private readonly IDiscordAccess _discordAccess;

    public VacationProvider(IUserStore userStore,
                            IDatabaseAccess databaseAccess,
                            IDiscordAccess discordAccess)
    {
        _userStore = userStore;
        _databaseAccess = databaseAccess;
        _discordAccess = discordAccess;
    }

    private string FormatVacations((DiscordUserId UserID, DateTime Start, DateTime End, string? Note)[] vacations)
    {
        var userDisplayNames = _discordAccess.GetUserDisplayNames(vacations.Select(m => m.UserID).Distinct());
        var orderedVacations =
            userDisplayNames.Join(vacations,
                       mapping => mapping.Key,
                       vacation => vacation.UserID,
                       (mapping, vacation) => new {UserName = mapping.Value, vacation.Start, vacation.End, vacation.Note})
                 .GroupBy(m => m.UserName)
                 .OrderBy(m => m.Key);
        return string.Join(Environment.NewLine + Environment.NewLine,
                           orderedVacations.Select(m => $"**{m.Key}**:{Environment.NewLine}" + string.Join(Environment.NewLine,
                                                            m.OrderBy(v => v.Start).Select(v =>
                                                                                               $"{v.Start:yyyy-MM-dd} - {v.End:yyyy-MM-dd}{(string.IsNullOrWhiteSpace(v.Note) ? string.Empty : $" ({v.Note})")}"))));
    }

    async Task<string> IVacationProvider.AddVacation(DiscordUserId userID, DateTime start, DateTime end, string? note)
    {
        // Check start and end
        if (start < DateTime.Today)
            return "Start date must be today or in the future.";
        if (end < start)
            return "End date must be after start date.";
        if (end > DateTime.Today.AddYears(1))
            return "End date cannot be more than 12 months into the future.";

        if (!_userStore.TryGetUser(userID, out var user))
            return "Failed to add vacation. User couldn't be identified.";
        var vacationAdded = await _databaseAccess.AddVacationAsync(user!, start, end, note?.Trim());
        return vacationAdded
                   ? "Vacation added successfully."
                   : "Failed to add vacation. Vacation collides with an existing vacation.";
    }

    async Task<string> IVacationProvider.DeleteVacation(DiscordUserId userID, DateTime start, DateTime end)
    {
        if (!_userStore.TryGetUser(userID, out var user))
            return "Failed to delete vacation. User couldn't be identified.";
        var vacationDeleted = await _databaseAccess.DeleteVacationAsync(user!, start, end);
        return vacationDeleted
                   ? "Vacation deleted successfully."
                   : "Failed to delete vacation. Matching vacation was not found.";
    }

    async Task<string> IVacationProvider.GetVacations()
    {
        var vacations = await _databaseAccess.GetVacationsAsync();
        return vacations.Length == 0
                   ? "No current or upcoming vacations."
                   : FormatVacations(vacations);
    }

    async Task<string> IVacationProvider.GetVacations(DiscordUserId userID)
    {
        if (!_userStore.TryGetUser(userID, out var user))
            return "Failed to fetch vacation. User couldn't be identified.";
        var vacations = await _databaseAccess.GetVacationsAsync(user!);
        return vacations.Length == 0
                   ? "No current or upcoming vacations."
                   : FormatVacations(vacations);
    }

    async Task<string> IVacationProvider.GetVacations(DateTime date)
    {
        var vacations = await _databaseAccess.GetVacationsAsync(date);
        return vacations.Length == 0
                   ? "No vacations for that date."
                   : FormatVacations(vacations);
    }
}