namespace HoU.GuildBot.BLL
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using JetBrains.Annotations;
    using Shared.BLL;
    using Shared.DAL;
    using Shared.StrongTypes;

    [UsedImplicitly]
    public class VacationProvider : IVacationProvider
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Fields

        private readonly IUserStore _userStore;
        private readonly IDatabaseAccess _databaseAccess;
        private readonly IDiscordAccess _discordAccess;

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Constructors

        public VacationProvider(IUserStore userStore,
                                IDatabaseAccess databaseAccess,
                                IDiscordAccess discordAccess)
        {
            _userStore = userStore;
            _databaseAccess = databaseAccess;
            _discordAccess = discordAccess;
        }

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Private Methods

        private string FormatVacations((DiscordUserID UserID, DateTime Start, DateTime End, string Note)[] vacations)
        {
            var names = _discordAccess.GetUserNames(vacations.Select(m => m.UserID).Distinct());
            var orderedVacations =
                names.Join(vacations,
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

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region IVacationProvider Members

        async Task<string> IVacationProvider.AddVacation(DiscordUserID userID, DateTime start, DateTime end, string note)
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
            var vacationAdded = await _databaseAccess.AddVacation(user, start, end, note?.Trim()).ConfigureAwait(false);
            return vacationAdded
                       ? "Vacation added successfully."
                       : "Failed to add vacation. Vacation collides with an existing vacation.";
        }

        async Task<string> IVacationProvider.DeleteVacation(DiscordUserID userID, DateTime start, DateTime end)
        {
            if (!_userStore.TryGetUser(userID, out var user))
                return "Failed to delete vacation. User couldn't be identified.";
            var vacationDeleted = await _databaseAccess.DeleteVacation(user, start, end).ConfigureAwait(false);
            return vacationDeleted
                       ? "Vacation deleted successfully."
                       : "Failed to delete vacation. Matching vacation was not found.";
        }

        async Task<string> IVacationProvider.GetVacations()
        {
            var vacations = await _databaseAccess.GetVacations().ConfigureAwait(false);
            return vacations.Length == 0
                       ? "No current or upcoming vacations."
                       : FormatVacations(vacations);
        }

        async Task<string> IVacationProvider.GetVacations(DiscordUserID userID)
        {
            if (!_userStore.TryGetUser(userID, out var user))
                return "Failed to fetch vacation. User couldn't be identified.";
            var vacations = await _databaseAccess.GetVacations(user).ConfigureAwait(false);
            return vacations.Length == 0
                       ? "No current or upcoming vacations."
                       : FormatVacations(vacations);
        }

        async Task<string> IVacationProvider.GetVacations(DateTime date)
        {
            var vacations = await _databaseAccess.GetVacations(date).ConfigureAwait(false);
            return vacations.Length == 0
                       ? "No vacations for that date."
                       : FormatVacations(vacations);
        }

        #endregion
    }
}