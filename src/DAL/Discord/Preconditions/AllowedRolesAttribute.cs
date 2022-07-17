namespace HoU.GuildBot.DAL.Discord.Preconditions;

[AttributeUsage(AttributeTargets.Method)]
public class AllowedRolesAttribute : PreconditionAttribute
{
    private readonly Role _allowedRoles;

    public AllowedRolesAttribute(Role allowedRoles)
    {
        _allowedRoles = allowedRoles;
    }

    public override Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context,
                                                                    ICommandInfo commandInfo,
                                                                    IServiceProvider services)
    {
        var userStore = (IUserStore)services.GetRequiredService(typeof(IUserStore));
        if (!userStore.TryGetUser((DiscordUserId)context.User.Id, out var user))
            return Task.FromResult(PreconditionResult.FromError("Couldn't determine user permission roles."));

        var isAllowed = (_allowedRoles & user!.Roles) != Role.NoRole;

        return Task.FromResult(isAllowed
                                   ? PreconditionResult.FromSuccess()
                                   : PreconditionResult.FromError("This interaction is not available with your current roles."));
    }
}