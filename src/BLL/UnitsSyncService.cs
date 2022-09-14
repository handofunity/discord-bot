namespace HoU.GuildBot.BLL;

[UsedImplicitly]
public class UnitsSyncService : IUnitsSyncService
{
    private readonly IDiscordAccess _discordAccess;
    private readonly IUnitsAccess _unitsAccess;
    private readonly IDynamicConfiguration _dynamicConfiguration;
    private readonly ILogger<UnitsSyncService> _logger;

    public UnitsSyncService(IDiscordAccess discordAccess,
                            IUnitsAccess unitsAccess,
                            IDynamicConfiguration dynamicConfiguration,
                            ILogger<UnitsSyncService> logger)
    {
        _discordAccess = discordAccess ?? throw new ArgumentNullException(nameof(discordAccess));
        _unitsAccess = unitsAccess ?? throw new ArgumentNullException(nameof(unitsAccess));
        _dynamicConfiguration = dynamicConfiguration ?? throw new ArgumentNullException(nameof(dynamicConfiguration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    private UserModel[] SanitizeUsers(UserModel[] users)
    {
        var usersWithAocRole = _discordAccess.GetUsersInRoles(new[] { "Ashes of Creation (AoC)" });
        var result = users.Where(m => usersWithAocRole.Any(user => user.DiscordUserId == m.DiscordUserId)).ToArray();

        foreach (var userModel in result)
        {
            // Remove "a_" prefix of animated avatar IDs
            if (userModel.AvatarId != null && userModel.AvatarId.StartsWith("a_"))
                userModel.AvatarId = userModel.AvatarId.Substring(2);
        }

        return result;
    }

    // Do NOT implement this as explicit implementation, as it cannot be triggered by hangfire then!
    public async Task SyncAllUsers()
    {
        if (_dynamicConfiguration.UnitsEndpoints.Length == 0)
        {
            _logger.LogWarning("No UNITS access configured");
            return;
        }

        if (!_discordAccess.IsConnected || !_discordAccess.IsGuildAvailable)
            return;

        foreach (var unitsSyncData in _dynamicConfiguration.UnitsEndpoints.Where(m => !string.IsNullOrWhiteSpace(m.BaseAddress)
                                                                                   && !string.IsNullOrWhiteSpace(m.Secret)
                                                                                   && m.ConnectToRestApi))
        {
            if (string.IsNullOrWhiteSpace(unitsSyncData.BaseAddress))
            {
                _logger.LogWarning("UNITS base address not configured");
                continue;
            }

            if (string.IsNullOrWhiteSpace(unitsSyncData.Secret))
            {
                _logger.LogWarning("UNITS access secret not configured");
                continue;
            }

            var allowedRoles = await _unitsAccess.GetValidRoleNamesAsync(unitsSyncData);
            if (allowedRoles == null)
            {
                _logger.LogWarning("Failed to synchronize all users: {Reason}", "unable to fetch allowed roles");
                continue;
            }

            var users = _discordAccess.GetUsersInRoles(allowedRoles);
            if (users.Any())
            {
                users = SanitizeUsers(users);
                _logger.LogInformation("Sending {Count} users to the UNITS system at {Address} ...",
                                       users.Length,
                                       unitsSyncData.BaseAddress);
                var result = await _unitsAccess.SendAllUsersAsync(unitsSyncData, users);
                if (result == null)
                    continue;

                var utcNow = DateTime.UtcNow;
                var notificationRequired = utcNow.Hour == 15 && utcNow.Minute < 15;
                var onlySkippedUsers = result.SkippedUsers > 0
                                    && result.CreatedUsers == 0
                                    && result.UpdatedUsers == 0
                                    && result.UpdatedUserRoleRelations == 0
                                    && (result.Errors?.Count ?? 0) == 0;
                var shouldPostResults = notificationRequired || !onlySkippedUsers;
                if (!shouldPostResults)
                    continue;

                var sb = new StringBuilder($"Synchronized {users.Length} users with the UNIT system at {unitsSyncData.BaseAddress}:");
                sb.AppendLine();
                if (result.CreatedUsers > 0)
                    sb.AppendLine($"Created {result.CreatedUsers} users.");
                if (result.UpdatedUsers > 0)
                    sb.AppendLine($"Updated {result.UpdatedUsers} users.");
                if (result.SkippedUsers > 0)
                    sb.AppendLine($"Skipped {result.SkippedUsers} users.");
                if (result.UpdatedUserRoleRelations > 0)
                    sb.AppendLine($"Updated {result.UpdatedUserRoleRelations} user-role relations.");
                if (result.Errors != null && result.Errors.Any())
                {
                    if (notificationRequired)
                    {
                        var leadershipMention = _discordAccess.GetLeadershipMention();
                        sb.AppendLine($"**{leadershipMention} - errors synchronizing Discord with the UNIT system:**");
                    }
                    else
                    {
                        sb.AppendLine("**Errors synchronizing Discord with the UNIT system:**");
                    }

                    for (var index = 0; index < result.Errors.Count; index++)
                    {
                        var error = result.Errors[index];
                        var errorMessage = $"`{error}`";

                        // Check if this error message would create a too long Discord message.
                        if (sb.Length + errorMessage.Length > 1900 && index < result.Errors.Count - 1
                         || sb.Length + errorMessage.Length > 2000)
                        {
                            sb.AppendLine($"**{result.Errors.Count - index} more errors were truncated from this message.**");
                            break;
                        }

                        sb.AppendLine(errorMessage);
                    }
                }

                await _discordAccess.LogToDiscord(sb.ToString());
            }
            else
            {
                _logger.LogWarning("Failed to synchronize all users: {Reason}", "unable to fetch allowed users");
            }
        }
    }
}