namespace HoU.GuildBot.DAL.Discord.Modules;

[UsedImplicitly]
[Group("birthday", "Commands related to member birthdays.")]
public class BirthdayModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IBirthdayProvider _birthdayProvider;

    public BirthdayModule(IBirthdayProvider birthdayProvider)
    {
        _birthdayProvider = birthdayProvider;
    }

    [SlashCommand("set", "Sets your birthday information.")]
    [AllowedRoles(Role.AnyGuildMember)]
    public async Task SetBirthdayAsync(short month,
                                       short day)
    {
        var response = await _birthdayProvider.SetBirthdayAsync((DiscordUserId)Context.User.Id,
                                                                month,
                                                                day);
        await RespondAsync(response, ephemeral: true);
    }

    [SlashCommand("delete", "Deletes your current birthday information.")]
    [AllowedRoles(Role.AnyGuildMember)]
    public async Task DeleteBirthdayAsync()
    {
        var response = await _birthdayProvider.DeleteBirthdayAsync((DiscordUserId)Context.User.Id);
        await RespondAsync(response, ephemeral: true);
    }
}