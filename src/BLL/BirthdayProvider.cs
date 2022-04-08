using System;
using System.Linq;
using System.Threading.Tasks;
using HoU.GuildBot.Shared.BLL;
using HoU.GuildBot.Shared.DAL;
using HoU.GuildBot.Shared.StrongTypes;

namespace HoU.GuildBot.BLL;

public class BirthdayProvider : IBirthdayProvider
{
    private readonly IUserStore _userStore;
    private readonly IDatabaseAccess _databaseAccess;

    public BirthdayProvider(IUserStore userStore,
                            IDatabaseAccess databaseAccess)
    {
        _userStore = userStore;
        _databaseAccess = databaseAccess;
    }

    async Task<string> IBirthdayProvider.SetBirthdayAsync(DiscordUserId userId,
                                                          short month,
                                                          short day)
    {
        if (month is < 1 or > 12)
            return ":warning: Value of `month` must be between 1 and 12.";

        if (!DateOnly.TryParseExact($"2000.{month:D2}.{day:D2}", "yyyy.MM.dd", out var parsedDate))
            return ":warning: Value of `day` is not valid for the given `month`.";

        if (!_userStore.TryGetUser(userId, out var user))
            return ":warning: Failed to set birthday. User couldn't be identified.";

        var birthdaySet = await _databaseAccess.SetBirthdayAsync(user!, parsedDate);
        return birthdaySet
                   ? ":white_check_mark: Birthday set successfully."
                   : ":warning: Failed to set birthday.";
    }

    async Task<string> IBirthdayProvider.DeleteBirthdayAsync(DiscordUserId userId)
    {
        if (!_userStore.TryGetUser(userId, out var user))
            return ":warning: Failed to delete birthday. User couldn't be identified.";
        var birthdayDeleted = await _databaseAccess.DeleteUserBirthdayAsync(user!);
        return birthdayDeleted
                   ? ":white_check_mark: Birthday deleted successfully."
                   : ":warning: Failed to delete birthday.";
    }

    async Task<DiscordUserId[]> IBirthdayProvider.GetBirthdaysAsync(DateOnly forDate)
    {
        var internalUserIds = await _databaseAccess.GetUsersWithBirthdayAsync((short)forDate.Month,
                                                                              (short)forDate.Day);
        if (internalUserIds.Length == 0)
            return Array.Empty<DiscordUserId>();

        return _userStore.GetUsers(m => internalUserIds.Contains(m.InternalUserId))
                         .Select(m => m.DiscordUserId)
                         .ToArray();
    }
}