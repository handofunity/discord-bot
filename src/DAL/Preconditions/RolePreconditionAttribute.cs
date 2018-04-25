namespace HoU.GuildBot.DAL.Preconditions
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Discord.Commands;
    using Shared.Enums;

    public class RolePreconditionAttribute : PreconditionAttribute
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Fields

        private readonly Role _allowedRoles;

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Constructors

        public RolePreconditionAttribute(Role allowedRoles)
        {
            _allowedRoles = allowedRoles;
        }

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Base Overrides

        public override async Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            var guildUser = await context.Guild.GetUserAsync(context.User.Id).ConfigureAwait(false);
            var userRoles = guildUser.RoleIds;
            var guildRoles = context.Guild.Roles.Where(m => userRoles.Contains(m.Id)).Select(m => m.Name);
            // Check for the first matching role
            foreach (var guildRole in guildRoles)
            {
                if (!Enum.TryParse(guildRole, out Role parsedGuildRole))
                    continue;

                // If the value could be parsed, check if it is one of the allowed roles
                if (_allowedRoles.HasFlag(parsedGuildRole))
                    return PreconditionResult.FromSuccess();
            }

            // If none of the roles could be parsed, or none is allowed, return the error result.
            return PreconditionResult.FromError($"**{guildUser.Username}**: This command is not available for your current roles.");
        }

        #endregion
    }
}