using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HoU.GuildBot.Shared.BLL;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using HoU.GuildBot.Shared.DAL;
using HoU.GuildBot.Shared.Objects;

namespace HoU.GuildBot.BLL
{
    [UsedImplicitly]
    public class UnitsSyncService : IUnitsSyncService
    {
        private readonly IDiscordAccess _discordAccess;
        private readonly IUnitsAccess _unitsAccess;
        private readonly AppSettings _appSettings;
        private readonly ILogger<UnitsSyncService> _logger;

        public UnitsSyncService(IDiscordAccess discordAccess,
                                IUnitsAccess unitsAccess,
                                AppSettings appSettings,
                                ILogger<UnitsSyncService> logger)
        {
            _discordAccess = discordAccess ?? throw new ArgumentNullException(nameof(discordAccess));
            _unitsAccess = unitsAccess ?? throw new ArgumentNullException(nameof(unitsAccess));
            _appSettings = appSettings ?? throw new ArgumentNullException(nameof(appSettings));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private UserModel[] SanitizeUsers(UserModel[] users)
        {
            var usersWithAocRole = _discordAccess.GetUsersInRoles(new[] {"Ashes of Creation (AoC)"});
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
            if (!_discordAccess.IsConnected || !_discordAccess.IsGuildAvailable)
                return;

            foreach (var unitsSyncData in _appSettings.UnitsAccess.Where(m => !string.IsNullOrWhiteSpace(m.BaseAddress) && !string.IsNullOrWhiteSpace(m.Secret)))
            {
                var allowedRoles = await _unitsAccess.GetValidRoleNamesAsync(unitsSyncData);
                if (allowedRoles == null)
                {
                    _logger.LogWarning("Failed to synchronize all users: {Reason}.", "unable to fetch allowed roles");
                    continue;
                }

                var users = _discordAccess.GetUsersInRoles(allowedRoles);
                if (users.Any())
                {
                    users = SanitizeUsers(users);
                    _logger.LogInformation("Sending {Count} users to the UNITS system at {Address} ...", users.Length, unitsSyncData.BaseAddress);
                    var result = await _unitsAccess.SendAllUsersAsync(unitsSyncData, users);
                    if (result != null)
                    {
                        var sb = new StringBuilder($"Synchronized {users.Length} users with the UNIT system:");
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
                            var utcNow = DateTime.UtcNow;
                            if (utcNow.Hour == 15 && utcNow.Minute < 15)
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
                                if (sb.Length + errorMessage.Length > 1900 && index < result.Errors.Count - 1 || sb.Length + errorMessage.Length > 2000)
                                {
                                    sb.AppendLine($"**{result.Errors.Count - index} more errors were truncated from this message.**");
                                    break;
                                }

                                sb.AppendLine(errorMessage);
                            }
                        }

                        await _discordAccess.LogToDiscord(sb.ToString());
                    }
                }
                else
                {
                    _logger.LogWarning("Failed to synchronize all users: {Reason}.", "unable to fetch allowed users");
                }
            }
        }
    }
}