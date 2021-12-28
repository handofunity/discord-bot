using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HoU.GuildBot.Shared.BLL;
using HoU.GuildBot.Shared.DAL;
using HoU.GuildBot.Shared.StrongTypes;

namespace HoU.GuildBot.BLL
{
    public class RoleRemover : IRoleRemover
    {
        private readonly IDiscordAccess _discordAccess;
        private readonly IDynamicConfiguration _dynamicConfiguration;
        private readonly List<DiscordUserID> _usersToFreeFromBasement;

        public RoleRemover(IDiscordAccess? discordAccess,
                           IDynamicConfiguration? dynamicConfiguration)
        {
            _discordAccess = discordAccess ?? throw new ArgumentNullException(nameof(discordAccess));
            _dynamicConfiguration = dynamicConfiguration ?? throw new ArgumentNullException(nameof(dynamicConfiguration));
            _usersToFreeFromBasement = new List<DiscordUserID>();
        }

        async Task IRoleRemover.RemoveBasementRolesAsync()
        {
            var basementRoleId = _dynamicConfiguration.DiscordMapping["BasementRoleId"];

            // If there are any users from the last check, free them this round.
            foreach (var discordUserID in _usersToFreeFromBasement)
            {
                if (!_discordAccess.CanManageRolesForUser(discordUserID))
                    continue;

                var (success, roleName) = await _discordAccess.TryRevokeNonMemberRole(discordUserID, basementRoleId);
                if (success)
                {
                    await _discordAccess.LogToDiscord($"Automatically removed role `{roleName}` from <@{discordUserID}>.");
                    continue;
                }

                var leaderMention = _discordAccess.GetRoleMention("Leader");
                await _discordAccess.LogToDiscord($"{leaderMention}: failed to remove role `{roleName}` from <@{discordUserID}>");
            }

            // Gather users that should be freed next round.
            _usersToFreeFromBasement.Clear();
            var usersInBasement = _discordAccess.GetUsersIdsInRole(basementRoleId);
            if (usersInBasement.Any())
                _usersToFreeFromBasement.AddRange(usersInBasement);
        }
    }
}