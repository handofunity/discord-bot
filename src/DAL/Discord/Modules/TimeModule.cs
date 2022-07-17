namespace HoU.GuildBot.DAL.Discord.Modules;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class TimeModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly ITimeInformationProvider _timeInformationProvider;

    public TimeModule(ITimeInformationProvider timeInformationProvider)
    {
        _timeInformationProvider = timeInformationProvider;
    }

    [SlashCommand("time", "Gets a list of guild time zones.")]
    [AllowedRoles(Role.AnyGuildMember)]
    public async Task TimeAsync()
    {
        var tz = _timeInformationProvider.GetCurrentTimeFormattedForConfiguredTimezones();
        var markdownBuilder = new StringBuilder()
                             .AppendLine("```md")
                             .AppendLine("Current Guild Times")
                             .AppendLine("===================");
        foreach (var s in tz)
            markdownBuilder.AppendLine(s);
        markdownBuilder.AppendLine("```");
        await RespondAsync(markdownBuilder.ToString());
    }
}