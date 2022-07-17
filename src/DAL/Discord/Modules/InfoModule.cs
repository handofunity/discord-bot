namespace HoU.GuildBot.DAL.Discord.Modules;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
[Group("info", "Informational commands for the developer.")]
public class InfoModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IBotInformationProvider _botInformationProvider;
    private readonly ILogger<InfoModule> _logger;

    public InfoModule(IBotInformationProvider botInformationProvider,
                      ILogger<InfoModule> logger)
    {
        _botInformationProvider = botInformationProvider;
        _logger = logger;
    }

    [SlashCommand("show", "Shows information about the current bot instance.")]
    [AllowedRoles(Role.Developer | Role.Leader)]
    public async Task InfoAsync()
    {
        _logger.LogDebug("Received \"info\" command request ...");
        var data = _botInformationProvider.GetData();
        var embed = data.ToEmbed();
        await RespondAsync(string.Empty, new[] { embed });
    }

    [SlashCommand("list-fonts", "Lists all available fonts.")]
    [AllowedRoles(Role.Developer)]
    public async Task ListFontsAsync()
    {
        await DeferAsync();
        var fonts = _botInformationProvider.GetAvailableFonts();
        var messages = new List<string>();
        foreach (var (key, value) in fonts)
        {
            var markdownBuilder = new StringBuilder()
                                 .AppendLine("```md")
                                 .AppendLine($"Available Fonts ({key + 1}/{fonts.Keys.Count})")
                                 .AppendLine("===============");
            foreach (var f in value)
                markdownBuilder.AppendLine(f);
            markdownBuilder.AppendLine("```");
            messages.Add(markdownBuilder.ToString());
        }
        await messages.PerformBulkOperation(async s => await FollowupAsync(s));
    }

    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    [Group("sub", "module")]
    public class InfoSubModule : InteractionModuleBase<SocketInteractionContext>
    {

        [SlashCommand("party", "Starts a party")]
        public async Task PartyAsync()
        {
            await RespondAsync("Party!");
        }
    }
}