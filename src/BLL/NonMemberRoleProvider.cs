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
                                                                       IEnumerable<DiscordRoleId> selectedRoleIds,
                                                                       RoleToggleMode roleToggleMode)
    {
        if (!_userStore.TryGetUser(userId, out var user))
            return "Couldn't find user to edit.";
        if (!DiscordAccess.CanManageRolesForUser(userId))
            return "The bot is not allowed to change your roles.";

        var staticRole = MapCustomIdToStaticRole(customId);
        if (staticRole == Role.NoRole)
            return await ToggleGameInterestRolesAsync(userId, selectedRoleIds, roleToggleMode, user!);

        if (roleToggleMode != RoleToggleMode.Dynamic)
            return "Failed to toggle static role.";
        
        return await ToggleStaticRolesAsync(userId, staticRole, user!);
    }

    private async Task<string> ToggleGameInterestRolesAsync(DiscordUserId userId,
                                                            IEnumerable<DiscordRoleId> values,
                                                            RoleToggleMode roleToggleMode,
                                                            User user)
    {
        var selectedGameInterestRoles = values.Where(IsGameInterestRole).ToArray();
        var currentUserRoleIds = DiscordAccess.GetUserRoles(userId);

        var sb = new StringBuilder();
        var logSb = new StringBuilder();
        
        switch (roleToggleMode)
        {
            case RoleToggleMode.Assign:
            {
                var rolesToAdd = selectedGameInterestRoles.Except(currentUserRoleIds).ToArray();
                foreach (var discordRoleId in rolesToAdd)
                {
                    var (success, roleName) = await DiscordAccess.TryAddNonMemberRoleAsync(userId, discordRoleId);
                    sb.AppendLine(success
                                      ? $"Successfully assigned the role **{roleName}**."
                                      : $"Failed to assign the role **{roleName}**.");
                    if (success)
                        logSb.AppendLine($"User {user.Mention} **added** the role **_{roleName}_**.");
                }

                break;
            }
            case RoleToggleMode.Revoke:
            {
                var rolesToRemove = selectedGameInterestRoles.Intersect(currentUserRoleIds).ToArray();
                foreach (var discordRoleId in rolesToRemove)
                {
                    var (success, roleName) = await DiscordAccess.TryRevokeNonMemberRoleAsync(userId, discordRoleId);
                    sb.AppendLine(success
                                      ? $"Successfully revoked the role **{roleName}**."
                                      : $"Failed to revoke the role **{roleName}**.");
                    if (success)
                        logSb.AppendLine($"User {user.Mention} **revoked** the role **_{roleName}_**.");
                }

                break;
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(roleToggleMode), roleToggleMode, null);
        }

        if (logSb.Length > 0)
            await DiscordAccess.LogToDiscordAsync(logSb.ToString());

        return sb.Length > 0
                   ? sb.ToString()
                   : "No roles were added or removed.";
    }

    private async Task<string> ToggleStaticRolesAsync(DiscordUserId userId,
                                                      Role staticRole,
                                                      User user)
    {
        var revoke = user.Roles.HasFlag(Role.Guest) && staticRole == Role.Guest
                  || user.Roles.HasFlag(Role.FriendOfMember) && staticRole == Role.FriendOfMember;

        if (revoke)
        {
            var removed = await DiscordAccess.TryRevokeNonMemberRoleAsync(userId, staticRole);
            if (!removed)
                return "Failed to identify matching role.";

            await DiscordAccess.LogToDiscordAsync($"User {user.Mention} **removed** the role **_{staticRole}_**.");
            return $"The role **_{staticRole}_** was **removed**.";
        }

        var added = await DiscordAccess.TryAddNonMemberRoleAsync(userId, staticRole);
        if (!added)
            return "Failed to identify matching role.";

        await DiscordAccess.LogToDiscordAsync($"User {user.Mention} **added** the role **_{staticRole}_**.");
        return $"The role **_{staticRole}_** was **added**.";
    }
}