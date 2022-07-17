namespace HoU.GuildBot.DAL.Discord.Modules;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
[Group("ignore", "Tell the bot to ignore interactions for a specific time, or stop ignoring manually.")]
public class IgnoreModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IIgnoreGuard _ignoreGuard;

    public IgnoreModule(IIgnoreGuard ignoreGuard)
    {
        _ignoreGuard = ignoreGuard;
    }

    [SlashCommand("start", "Starts ignoring interactions from the invoking user for the given amount of minutes.")]
    [AllowedRoles(Role.Developer)]
    public async Task StartIgnoreAsync([MinValue(5), MaxValue(60)] int minutes = 60)
    {
        var embedData = _ignoreGuard.EnsureOnIgnoreList((DiscordUserId)Context.User.Id,
                                                        Context.User.Username,
                                                        minutes);
        var embed = embedData.ToEmbed();
        await RespondAsync(embed: embed);
    }

    [SlashCommand("stop", "Stops ignoring interactions from the invoking user.")]
    [AllowedRoles(Role.Developer)]
    public async Task StopIgnoreAsync()
    {
        if (_ignoreGuard.TryRemoveFromIgnoreList((DiscordUserId)Context.User.Id,
                                                 Context.User.Username,
                                                 out var embedData)
         && embedData != null)
        {
            var embed = embedData.ToEmbed();
            await RespondAsync(embed: embed);
        }
    }
}