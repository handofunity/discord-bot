namespace HoU.GuildBot.BLL;

public class RoleRemover : IRoleRemover
{
    private const string TrialMemberRoleIdKey = "TrialMemberRoleId";
    
    private readonly IDiscordAccess _discordAccess;
    private readonly IDynamicConfiguration _dynamicConfiguration;
    private readonly IUserStore _userStore;
    private readonly IDatabaseAccess _databaseAccess;
    private readonly ILogger<RoleRemover> _logger;
    private readonly List<DiscordUserId> _usersToFreeFromBasement;

    public RoleRemover(IDiscordAccess? discordAccess,
                       IDynamicConfiguration? dynamicConfiguration,
                       IUserStore userStore,
                       IDatabaseAccess databaseAccess,
                       ILogger<RoleRemover> logger)
    {
        _discordAccess = discordAccess ?? throw new ArgumentNullException(nameof(discordAccess));
        _dynamicConfiguration = dynamicConfiguration ?? throw new ArgumentNullException(nameof(dynamicConfiguration));
        _userStore = userStore;
        _databaseAccess = databaseAccess;
        _logger = logger;
        _usersToFreeFromBasement = new List<DiscordUserId>();
    }

    async Task IRoleRemover.RemoveBasementRolesAsync()
    {
        var basementRoleId = (DiscordRoleId)_dynamicConfiguration.DiscordMapping["BasementRoleId"];

        // If there are any users from the last check, free them this round.
        foreach (var discordUserID in _usersToFreeFromBasement)
        {
            if (!_discordAccess.CanManageRolesForUser(discordUserID))
                continue;

            var (success, roleName) = await _discordAccess.TryRevokeNonMemberRoleAsync(discordUserID, basementRoleId);
            if (success)
            {
                await _discordAccess.LogToDiscordAsync($"Automatically removed role `{roleName}` from <@{discordUserID}>.");
                continue;
            }

            var leaderMention = _discordAccess.GetRoleMention("Leader");
            await _discordAccess.LogToDiscordAsync($"{leaderMention}: failed to remove role `{roleName}` from <@{discordUserID}>");
        }

        // Gather users that should be freed next round.
        _usersToFreeFromBasement.Clear();
        var usersInBasement = _discordAccess.GetUsersIdsInRole(basementRoleId);
        if (usersInBasement.Any())
            _usersToFreeFromBasement.AddRange(usersInBasement);
    }

    async Task IRoleRemover.RemoveStaleTrialMembersAsync()
    {
        var trialMemberRoleId = (DiscordRoleId)_dynamicConfiguration.DiscordMapping[TrialMemberRoleIdKey];
        var trialMemberIds = _discordAccess.GetUsersIdsInRole(trialMemberRoleId);
        var today = DateOnly.FromDateTime(DateTime.Today);
        
        // Iterate all trial members
        foreach (var trialMemberId in trialMemberIds)
        {
            if (!_userStore.TryGetUser(trialMemberId, out var user))
            {
                _logger.LogWarning("User {UserId} is in trial member role but not in the user store", trialMemberId);
                continue;
            }
            
            // Check if the promotion date is null. In that case, persist today's date.
            if (user!.PromotedToTrialMemberDate is null)
            {
                user.PromotedToTrialMemberDate = today;
                await _databaseAccess.UpdateUserInfoPromotionToTrialMemberDateAsync(user);
                continue;
            }
            
            // Check if their promotion date to trial member is more than 90 days ago.
            if (user.PromotedToTrialMemberDate.Value.AddDays(90) > today)
                continue;
            
            // Remove all roles from user and set back to guest role.
            await _discordAccess.LogToDiscordAsync($"User {user.Mention} has been a trial member for more than 90 days."
                                                 + "All roles will be removed, `Guest` role will be applied instead.");
            await _discordAccess.RevokeAllRolesAsync(trialMemberId);
            var added = await _discordAccess.TryAddNonMemberRoleAsync(trialMemberId, Role.Guest);
            // ReSharper disable once InvertIf
            if (!added)
            {
                var leaderMention = _discordAccess.GetRoleMention("Leader");
                await _discordAccess.LogToDiscordAsync($"{leaderMention}: failed to add role `Guest` to {trialMemberId.ToMention()}.");
            }
        }
    }
}