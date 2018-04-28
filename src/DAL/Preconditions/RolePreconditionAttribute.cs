namespace HoU.GuildBot.DAL.Preconditions
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Discord.Commands;
    using Shared.BLL;
    using Shared.Enums;

    [AttributeUsage(AttributeTargets.Method)]
    public class RolePreconditionAttribute : PreconditionAttribute
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Properties

        public Role AllowedRoles { get; }

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Constructors

        public RolePreconditionAttribute(Role allowedRoles)
        {
            AllowedRoles = allowedRoles;
        }

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Base Overrides

        public override Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            var gur = (IGuildUserRegistry)services.GetService(typeof(IGuildUserRegistry));
            var userRoles = gur.GetGuildUserRoles(context.User.Id);
            var isAllowed = (AllowedRoles & userRoles) != Role.Undefined;
            return Task.FromResult(isAllowed
                                       ? PreconditionResult.FromSuccess()
                                       : PreconditionResult.FromError($"**{context.User.Username}**: This command is not available for your current roles."));
        }

        #endregion
    }
}