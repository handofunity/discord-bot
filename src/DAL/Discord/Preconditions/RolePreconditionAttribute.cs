namespace HoU.GuildBot.DAL.Discord.Preconditions
{
    using System;
    using System.Threading.Tasks;
    using global::Discord.Commands;
    using Shared.BLL;
    using Shared.Enums;
    using Shared.StrongTypes;

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
            var userStore = (IUserStore)services.GetService(typeof(IUserStore));
            var userRoles = userStore.GetUser((DiscordUserID)context.User.Id).Roles;
            var isAllowed = (AllowedRoles & userRoles) != Role.NoRole;
            return Task.FromResult(isAllowed
                                       ? PreconditionResult.FromSuccess()
                                       : PreconditionResult.FromError($"**{context.User.Username}**: This command is not available for your current roles."));
        }

        #endregion
    }
}