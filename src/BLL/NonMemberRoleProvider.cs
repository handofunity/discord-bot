namespace HoU.GuildBot.BLL;

public class NonMemberRoleProvider : INonMemberRoleProvider
{
    private readonly IUserStore _userStore;
    private readonly IGameRoleProvider _gameRoleProvider;
    private IDiscordAccess? _discordAccess;

    public NonMemberRoleProvider(IUserStore userStore,
                                 IGameRoleProvider gameRoleProvider)
    {
        _userStore = userStore;
        _gameRoleProvider = gameRoleProvider;
    }

    private static Role MapCustomIdToStaticRole(string customId) =>
        customId switch
        {
            Constants.FriendOrGuestMenu.FriendOfMemberCustomId => Role.FriendOfMember,
            Constants.FriendOrGuestMenu.GuestCustomId => Role.Guest,
            Constants.GameInterestMenu.CustomId => Role.NoRole,
            _ => throw new ArgumentOutOfRangeException(nameof(customId))
        };

    private bool IsGameInterestRole(DiscordRoleId roleId) =>
        _gameRoleProvider.Games
                         .Any(m => m.GameInterestRoleId != null && m.GameInterestRoleId == roleId);

    public IDiscordAccess DiscordAccess
    {
        set => _discordAccess = value;
        private get => _discordAccess ?? throw new InvalidOperationException();
    }

    async Task<string> INonMemberRoleProvider.ToggleNonMemberRoleAsync(DiscordUserId userId,
                                                                       string customId,
                                                                       IReadOnlyCollection<string>? availableOptions,
                                                                       IReadOnlyCollection<string> selectedValues)
    {
        if (!_userStore.TryGetUser(userId, out var user))
            return "Couldn't find user to edit.";
        if (!DiscordAccess.CanManageRolesForUser(userId))
            return "The bot is not allowed to change your roles.";

        var staticRole = MapCustomIdToStaticRole(customId);
        return staticRole == Role.NoRole
                   ? await ToggleGameInterestRolesAsync(userId, availableOptions, selectedValues, user!)
                   : await ToggleStaticRolesAsync(userId, staticRole, user!);
    }

    private async Task<string> ToggleGameInterestRolesAsync(DiscordUserId userId,
                                                            IReadOnlyCollection<string>? availableOptions,
                                                            IReadOnlyCollection<string> values,
                                                            User user)
    {
        if (availableOptions == null)
            throw new ArgumentNullException(nameof(availableOptions),
                                            $"The {nameof(availableOptions)} are required for game interest roles.");

        var desiredRoleIds = availableOptions.Intersect(values)
                                             .Select(m => (DiscordRoleId)ulong.Parse(m))
                                             .Where(IsGameInterestRole)
                                             .ToArray();
        var undesiredRoleIds = availableOptions.Except(values)
                                               .Select(m => (DiscordRoleId)ulong.Parse(m))
                                               .Where(IsGameInterestRole)
                                               .ToArray();
        var userRoleIds = DiscordAccess.GetUserRoles(userId);
        var rolesToAdd = desiredRoleIds.Except(userRoleIds).ToArray();
        var rolesToRemove = undesiredRoleIds.Intersect(userRoleIds).ToArray();

        var sb = new StringBuilder();
        var logSb = new StringBuilder();

        foreach (var discordRoleId in rolesToAdd)
        {
            var (success, roleName) = await DiscordAccess.TryAddNonMemberRole(userId, discordRoleId);
            sb.AppendLine(success
                              ? $"Successfully assigned the role **{roleName}**."
                              : $"Failed to assign the role **{roleName}**.");
            if (success)
                logSb.AppendLine($"User {user!.Mention} **added** the role **_{roleName}_**.");
        }

        foreach (var discordRoleId in rolesToRemove)
        {
            var (success, roleName) = await DiscordAccess.TryRevokeNonMemberRole(userId, discordRoleId);
            sb.AppendLine(success
                              ? $"Successfully revoked the role **{roleName}**."
                              : $"Failed to revoke the role **{roleName}**.");
            if (success)
                logSb.AppendLine($"User {user!.Mention} **revoked** the role **_{roleName}_**.");
        }

        await DiscordAccess.LogToDiscord(logSb.ToString());

        return sb.Length > 0
                   ? sb.ToString()
                   : "No roles were added or removed.";
    }

    private async Task<string> ToggleStaticRolesAsync(DiscordUserId userId,
                                                      Role staticRole,
                                                      User user)
    {
        var remove = user.Roles.HasFlag(Role.Guest) && staticRole == Role.Guest
                  || user.Roles.HasFlag(Role.FriendOfMember) && staticRole == Role.FriendOfMember;

        if (remove)
        {
            var removed = await DiscordAccess.TryRevokeNonMemberRole(userId, staticRole);
            if (!removed)
                return "Failed to identify matching role.";

            await DiscordAccess.LogToDiscord($"User {user.Mention} **removed** the role **_{staticRole}_**.");
            return $"The role **_{staticRole}_** was **removed**.";
        }

        var added = await DiscordAccess.TryAddNonMemberRole(userId, staticRole);
        if (!added)
            return "Failed to identify matching role.";

        await DiscordAccess.LogToDiscord($"User {user.Mention} **added** the role **_{staticRole}_**.");
        return $"The role **_{staticRole}_** was **added**.";
    }
}