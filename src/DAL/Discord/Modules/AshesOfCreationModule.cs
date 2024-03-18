namespace HoU.GuildBot.DAL.Discord.Modules;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
[Group("aoc", "Ashes of Creation related commands.")]
public class AshesOfCreationModule : InteractionModuleBase<SocketInteractionContext>
{
    private const string UnitsHouGuildRoleIdKey = "UnitsHouGuildRoleId";
    private const string UnitsFouGuildRoleIdKey = "UnitsFouGuildRoleId";
    private const string UnitsPvpRoleIdKey = "UnitsPvpRoleId";
    private const string UnitsArtisanRoleIdKey = "UnitsArtisanRoleId";
    private const string UnitsPveRoleIdKey = "UnitsPveRoleId";
    
    private readonly IImageProvider _imageProvider;
    private readonly IUnitsAccess _unitsAccess;
    private readonly IDynamicConfiguration _dynamicConfiguration;
    private readonly ILogger<AshesOfCreationModule> _logger;

    public AshesOfCreationModule(IImageProvider imageProvider,
                                 IUnitsAccess unitsAccess,
                                 IDynamicConfiguration dynamicConfiguration,
                                 ILogger<AshesOfCreationModule> logger)
    {
        _imageProvider = imageProvider;
        _unitsAccess = unitsAccess;
        _dynamicConfiguration = dynamicConfiguration;
        _logger = logger;
    }

    [SlashCommand("profile", "Shows the profile image for the given or current user.", runMode: RunMode.Async)]
    [AllowedRoles(Role.Developer)] // TODO: Change to any guild member
    public async Task GetProfileCardAsync(IGuildUser? guildUser = null)
    {
        await DeferAsync();
        guildUser ??= (IGuildUser)Context.User;
        var userId = (DiscordUserId)guildUser.Id;

        try
        {
            var endpoint = _dynamicConfiguration.UnitsEndpoints.FirstOrDefault(m => m.ConnectToRestApi);
            if (endpoint is null)
            {
                await FollowupAsync("No endpoint configured.");
                return;
            }
            
            var profileData = await _unitsAccess.GetProfileDataAsync(endpoint, userId);
            if (profileData is null)
            {
                await FollowupAsync("Failed to fetch profile data from UNITS.");
                return;
            }

            // Determine guild tag
            var guildTag = "N/A";
            var houRoleId = _dynamicConfiguration.DiscordMapping[UnitsHouGuildRoleIdKey];
            var fouRoleId = _dynamicConfiguration.DiscordMapping[UnitsFouGuildRoleIdKey];
            if (guildUser.RoleIds.Contains(houRoleId))
                guildTag = "HoU";
            else if (guildUser.RoleIds.Contains(fouRoleId))
                guildTag = "FoU";
            
            // Determine play style
            var pvpRoleId = _dynamicConfiguration.DiscordMapping[UnitsPvpRoleIdKey];
            var artisanRoleId = _dynamicConfiguration.DiscordMapping[UnitsArtisanRoleIdKey];
            var pveRoleId = _dynamicConfiguration.DiscordMapping[UnitsPveRoleIdKey];
            var hasPvpRole = guildUser.RoleIds.Contains(pvpRoleId);
            var hasArtisanRole = guildUser.RoleIds.Contains(artisanRoleId);
            var hasPveRole = guildUser.RoleIds.Contains(pveRoleId);

            await using var imageStream = await _imageProvider.CreateProfileCardImage(userId,
                                                                                      guildUser.GetAvatarUrl(ImageFormat.Png),
                                                                                      profileData,
                                                                                      guildTag,
                                                                                      hasPvpRole,
                                                                                      hasArtisanRole,
                                                                                      hasPveRole);
            await Context.Interaction.FollowupWithFileAsync(imageStream,
                                                            $"units_{userId}_{DateTime.UtcNow:yyyyMMddHHmmss}.png");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to create UNITS profile card image");
            await FollowupAsync("Failed to create image.");
        }
    }

    [SlashCommand("archetype-combinations", "Shows an overview table of all possible archetype combinations in AoC.", runMode: RunMode.Async)]
    [AllowedRoles(Role.AnyGuildMember)]
    public async Task GetArchetypeCombinationsImageAsync()
    {
        await DeferAsync();

        try
        {
            await using var imageStream = _imageProvider.LoadClassListImage();
            await FollowupWithFileAsync(imageStream, "archetype-combinations.jpg");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to create the archetype combinations image");
            await FollowupAsync("Failed to create image.");
        }
    }

    [SlashCommand("artisan-professions", "Shows an overview of all possible artisan professions in AoC.", runMode: RunMode.Async)]
    [AllowedRoles(Role.AnyGuildMember)]
    public async Task GetArtisanProfessionsImageAsync()
    {
        await DeferAsync();

        try
        {
            await using var imageStream = _imageProvider.LoadArtisanProfessionsImage();
            await FollowupWithFileAsync(imageStream, "artisan-professions.png");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to create the artisan professions image");
            await FollowupAsync("Failed to create image.");
        }
    }

    [SlashCommand("launch-roster", "Shows the possibilities to get into the AoC launch roster.", runMode: RunMode.Async)]
    [AllowedRoles(Role.AnyGuildMember)]
    public async Task GetLaunchRosterImageAsync()
    {
        await DeferAsync();

        try
        {
            await using var imageStream = _imageProvider.LoadLaunchRosterImage();
            await FollowupWithFileAsync(imageStream, "launch-roster.jpg");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to create launch roster image");
            await FollowupAsync("Failed to create image.");
        }
    }

    [SlashCommand("chart", "Shows various charts related to AoC member roles.", runMode: RunMode.Async)]
    [AllowedRoles(Role.AnyGuildMember)]
    public async Task GetAocRolesChartAsync(ChartType chartType)
    {
        await DeferAsync();
        try
        {
            Func<Stream> getImage = chartType switch
            {
                ChartType.Classes => () => _imageProvider.CreateAocClassDistributionImage(),
                ChartType.Races => () => _imageProvider.CreateAocRaceDistributionImage(),
                ChartType.PlayStyles => () => _imageProvider.CreateAocPlayStyleDistributionImage(),
                ChartType.GuildPreference => () => _imageProvider.CreateAocGuildPreferenceDistributionImage(),
                ChartType.RolePreference => () => _imageProvider.CreateAocRolePreferenceDistributionImage(),
                _ => throw new NotSupportedException($"Chart type {chartType} is not supported.")
            };
            await using var imageStream = getImage();
            await Context.Interaction.FollowupWithFileAsync(imageStream, "chart.png");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to create {ChartType} chart", chartType);
            await FollowupAsync("Failed to create chart.");
        }
    }

    /// <summary>
    /// Chart types that can be generated by <see cref="AshesOfCreationModule.GetAocRolesChartAsync"/>.
    /// </summary>
    public enum ChartType
    {
        Classes = 1,
        Races = 2,
        PlayStyles = 3,
        GuildPreference = 4,
        RolePreference = 5
    }
}