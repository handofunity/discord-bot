namespace HoU.GuildBot.DAL.Discord.Modules;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
[Group("tea-party", "Commands related to the tea party.")]
public class TeaPartyModule : InteractionModuleBase<SocketInteractionContext>
{
    private const string TeaPartyAttendeeRoleIdKey = "TeaPartyAttendeeRoleId";
    
    private readonly IDynamicConfiguration _dynamicConfiguration;
    private readonly ILogger<TeaPartyModule> _logger;

    public TeaPartyModule(IDynamicConfiguration dynamicConfiguration,
                          ILogger<TeaPartyModule> logger)
    {
        _dynamicConfiguration = dynamicConfiguration;
        _logger = logger;
    }
    
    // Command to remove the tea party role from all users
    [SlashCommand("remove-role", "Removes the tea party attendee role from all users.")]
    [AllowedRoles(Role.Leader | Role.Officer)]
    public async Task RemoveRoleAsync()
    {
        await DeferAsync();

        var teaPartyAttendeeRoleId = _dynamicConfiguration.DiscordMapping[TeaPartyAttendeeRoleIdKey];
        var teaPartyRole = Context.Guild.Roles.FirstOrDefault(r => r.Id == teaPartyAttendeeRoleId);
        if (teaPartyRole == null)
        {
            await FollowupAsync("The tea party attendee role does not exist.");
            return;
        }

        try
        {
            var users = Context.Guild.Users.Where(u => u.Roles.Contains(teaPartyRole)).ToArray();
            if (users.Length == 0)
            {
                await FollowupAsync($"No users have the {teaPartyRole.Mention} role.");
                return;
            }
            
            foreach (var user in users)
            {
                await user.RemoveRoleAsync(teaPartyRole);
            }

            _logger.LogInformation("{User} removed the tea party attendee role from {UserCount} users",
                                   Context.User.Username,
                                   users.Length);
            await FollowupAsync($"Removed the {teaPartyRole.Mention} role from {users.Length} users.");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to remove tea party attendee role from users");
            await FollowupAsync($"An error occurred while removing the {teaPartyRole.Mention} role from users.");
        }
    }
}