namespace HoU.GuildBot.BLL
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using JetBrains.Annotations;
    using Shared.DAL;
    using Shared.Objects;

    [UsedImplicitly]
    public class UnitsSyncService
    {
        private readonly IDiscordAccess _discordAccess;
        private readonly IUnitsAccess _unitsAccess;

        public UnitsSyncService(IDiscordAccess discordAccess,
                                IUnitsAccess unitsAccess)
        {
            _discordAccess = discordAccess ?? throw new ArgumentNullException(nameof(discordAccess));
            _unitsAccess = unitsAccess ?? throw new ArgumentNullException(nameof(unitsAccess));
        }

        private void SanitizeUsers(IEnumerable<UserModel> users)
        {
            foreach (var userModel in users)
            {
                // Remove "a_" prefix of animated avatar IDs
                if (userModel.AvatarId != null && userModel.AvatarId.StartsWith("a_"))
                    userModel.AvatarId = userModel.AvatarId.Substring(2);
            }
        }

        public async Task SyncAllUsers()
        {
            if (!_discordAccess.IsConnected || !_discordAccess.IsGuildAvailable)
                return;

            var allowedRoles = await _unitsAccess.GetValidRoleNamesAsync();
            if (allowedRoles == null)
            {
                await _discordAccess.LogToDiscord("Failed to synchronize all users: unable to fetch allowed roles.");
                return;
            }

            var users = _discordAccess.GetUsersInRoles(allowedRoles);
            if (users.Any())
            {
                SanitizeUsers(users);
                var result = await _unitsAccess.SendAllUsersAsync(users);
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
                        var leadershipMention = _discordAccess.GetLeadershipMention();
                        sb.AppendLine($"**{leadershipMention} - errors synchronizing Discord with the UNIT system:**");
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
                await _discordAccess.LogToDiscord("Failed to synchronize all users: unable to fetch allowed users.");
            }
        }
    }
}