namespace HoU.GuildBot.BLL;

[UsedImplicitly]
public class ImageProvider : IImageProvider
{
    private readonly IGameRoleProvider _gameRoleProvider;
    private readonly IWebAccess _webAccess;
    private readonly IDiscordAccess _discordAccess;
    private readonly IDynamicConfiguration _dynamicConfiguration;
    private readonly IUnitsAccess _unitsAccess;

    public ImageProvider(IGameRoleProvider gameRoleProvider,
                         IWebAccess webAccess,
                         IDiscordAccess discordAccess,
                         IDynamicConfiguration dynamicConfiguration,
                         IUnitsAccess unitsAccess)
    {
        _gameRoleProvider = gameRoleProvider;
        _webAccess = webAccess;
        _discordAccess = discordAccess;
        _dynamicConfiguration = dynamicConfiguration;
        _unitsAccess = unitsAccess;
    }

    private static Stream CreateImage(int width,
                                      int height,
                                      Action<SKBitmap> bitmapModifier)
    {
        var image = SKImage.Create(new SKImageInfo(width, height));
        var bitmap = SKBitmap.FromImage(image);

        // Modify graphic
        bitmapModifier(bitmap);

        // Prepare result stream
        var result = new MemoryStream();
        bitmap.Encode(result, SKEncodedImageFormat.Png, 100);

        // Set result pointer back to 0 for the result consumer to read
        result.Position = 0;
        // Closing the stream is up to the consumer
        return result;
    }

    private static SKImage GetImageFromResource(string name)
    {
        var assembly = typeof(ImageProvider).Assembly;
        var resourceName = assembly.GetManifestResourceNames().Single(m => m.EndsWith(name));
        using var resourceStream = assembly.GetManifestResourceStream(resourceName);
        return SKImage.FromEncodedData(resourceStream);
    }

    private Stream CreateBarChartImage(BarChartDrawingData barChartDrawingData,
                                       DiscordRoleId primaryGameDiscordRoleId,
                                       string[] rolesInChart,
                                       Dictionary<string, string>? barLabelOverrides = null)
    {
        // Collect data
        var game = _gameRoleProvider.Games.Single(m => m.PrimaryGameDiscordRoleId == primaryGameDiscordRoleId);
        var (gameMembers, roleDistribution) = _gameRoleProvider.GetGameRoleDistribution(game);
        roleDistribution = (from rd in roleDistribution
                            from ric in rolesInChart
                            where rd.Key.EndsWith(ric)
                            select new { RoleName = ric, Count = rd.Value })
           .ToDictionary(m => m.RoleName, m => m.Count);
            
        // Load background and foreground image
        var backgroundImage = GetImageFromResource(barChartDrawingData.BackgroundImageName);
        var foregroundImage = GetImageFromResource(barChartDrawingData.ForegroundImageName);

        // Create image
        return CreateImage(BarChartDrawingData.ImageWidth,
                           BarChartDrawingData.ImageHeight,
                           bitmap =>
                           {
                               using var canvas = new SKCanvas(bitmap);
                               var arial = SKTypeface.FromFamilyName("Arial");
                               var arialBold = SKTypeface.FromFamilyName("Arial", SKFontStyleWeight.Bold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright);

                               // Draw background image
                               canvas.DrawImage(backgroundImage, 0, 0);

                               // Draw meta info
                               var infoFont = new SKFont(arial, 16f);
                               var infoPaint = new SKPaint(infoFont)
                               {
                                   Color = SKColors.Black
                               };
                               var gameMembersText = $"Game Members: {gameMembers}";
                               var createdOnText = $"Created: {DateTime.UtcNow:yyyy-MM-dd}";
                               var gameMembersRequiredSize = new SKRect();
                               var createdOnRequiredSize = new SKRect();
                               infoPaint.MeasureText(gameMembersText, ref gameMembersRequiredSize);
                               infoPaint.MeasureText(createdOnText, ref createdOnRequiredSize);
                               var metaTopOffset = (BarChartDrawingData.ContentTopOffset -
                                                    ((int)gameMembersRequiredSize.Height + (int)createdOnRequiredSize.Height +
                                                     BarChartDrawingData.VerticalInfoTextMargin)) / 2 + 15;
                               canvas.DrawText(gameMembersText,
                                               BarChartDrawingData.ImageWidth - BarChartDrawingData.HorizontalInfoTextMargin - gameMembersRequiredSize.Width,
                                               metaTopOffset,
                                               infoFont,
                                               infoPaint);
                               metaTopOffset = metaTopOffset + (int)gameMembersRequiredSize.Height + BarChartDrawingData.VerticalInfoTextMargin;
                               canvas.DrawText(createdOnText,
                                               BarChartDrawingData.ImageWidth - BarChartDrawingData.HorizontalInfoTextMargin - createdOnRequiredSize.Width,
                                               metaTopOffset,
                                               infoFont,
                                               infoPaint);

                               // Data
                               var indent = barChartDrawingData.InitialIndent;
                               var labelFont = new SKFont(arialBold, barChartDrawingData.LabelFontSize);
                               var amountFont = new SKFont(arialBold, 16f);
                               double maxCount = roleDistribution.Values.Max();
                               foreach (var (roleName, roleCount) in roleDistribution.OrderByDescending(m => m.Value).ThenBy(m => m.Key))
                               {
                                   // Bar label
                                   if (barLabelOverrides is null || !barLabelOverrides.TryGetValue(roleName, out var barLabelText))
                                       barLabelText = roleName;
                                   var labelPaint = new SKPaint(labelFont) { Color = SKColors.Black };
                                   var requiredLabelSize = new SKRect();
                                   labelPaint.MeasureText(barLabelText, ref requiredLabelSize);
                                   canvas.DrawText(barLabelText,
                                                   indent + (barChartDrawingData.IndentIncrement - requiredLabelSize.Width) / 2,
                                                   BarChartDrawingData.LabelsTopOffset,
                                                   labelFont,
                                                   labelPaint);

                                   // Bar
                                   var barHeight = (int)(roleCount / maxCount * BarChartDrawingData.BarMaxHeight);
                                   var topOffset = BarChartDrawingData.BarTopOffset + BarChartDrawingData.BarMaxHeight - barHeight;
                                   var leftOffset = indent + (barChartDrawingData.IndentIncrement - BarChartDrawingData.BarWidth) / 2;
                                   var rightOffset = leftOffset + BarChartDrawingData.BarWidth;
                                   var bottomOffset = topOffset + barHeight;
                                   var rect = new SKRect(leftOffset, topOffset, rightOffset, bottomOffset);
                                   var barColor = barChartDrawingData.BarColors[roleName];
                                   var barPaint = new SKPaint { Color = barColor };
                                   canvas.DrawRect(rect, barPaint);

                                   // Amount label
                                   var amountPaint = new SKPaint(amountFont) { Color = SKColors.Black };
                                   var amount = roleCount.ToString(CultureInfo.InvariantCulture);
                                   amountPaint.MeasureText(amount, ref requiredLabelSize);
                                   canvas.DrawText(amount,
                                                   indent + (barChartDrawingData.IndentIncrement - requiredLabelSize.Width) / 2,
                                                   topOffset - 10 - requiredLabelSize.Height,
                                                   amountFont,
                                                   amountPaint);

                                   indent += barChartDrawingData.IndentIncrement;
                               }

                               // Draw foreground image
                               canvas.DrawImage(foregroundImage, 0, 0);
                           });
    }

    Stream IImageProvider.CreateAocClassDistributionImage()
    {
        var barColors = new Dictionary<string, SKColor>
        {
            { "Fighter", SKColors.BurlyWood },
            { "Tank", SKColors.CornflowerBlue },
            { "Mage", SKColors.DarkOrange },
            { "Summoner", SKColors.Indigo },
            { "Ranger", SKColors.DarkGreen },
            { "Cleric", SKColors.Goldenrod },
            { "Bard", SKColors.LightPink },
            { "Rogue", SKColors.IndianRed }
        };
        var rolesInChart = barColors.Keys.ToArray();
        return CreateBarChartImage(new BarChartDrawingData("AoCRolesBackground_Right.png",
                                                           "AoCClassForeground.png",
                                                           barColors,
                                                           125f,
                                                           0f),
                                   (DiscordRoleId)_dynamicConfiguration.DiscordMapping["AshesOfCreationPrimaryGameDiscordRoleId"],
                                   rolesInChart);
    }

    Stream IImageProvider.CreateAocPlayStyleDistributionImage()
    {
        var barColors = new Dictionary<string, SKColor>
        {
            { "PvE", SKColors.Green },
            { "PvP", SKColors.DarkRed },
            { "Artisan", SKColors.DarkOrange }
        };
        var rolesInChart = barColors.Keys.ToArray();
        return CreateBarChartImage(new BarChartDrawingData("AoCRolesBackground_Centered.png",
                                                           "AoCPlaystyleForeground.png",
                                                           barColors,
                                                           278f,
                                                           83f),
                                   (DiscordRoleId)_dynamicConfiguration.DiscordMapping["AshesOfCreationPrimaryGameDiscordRoleId"],
                                   rolesInChart);
    }

    Stream IImageProvider.CreateAocRaceDistributionImage()
    {
        var barColors = new Dictionary<string, SKColor>
        {
            { "Kaelar", SKColors.CornflowerBlue },
            { "Vaelune", SKColors.LightSkyBlue },
            { "Empyrean", SKColors.Indigo },
            { "Pyrai", SKColors.BlueViolet },
            { "Renkai", SKColors.SeaGreen },
            { "Vek", SKColors.ForestGreen },
            { "Dunir", SKColors.Brown },
            { "Nikua", SKColors.Crimson },
            { "Tulnar", SKColors.Gold },
        };
        var rolesInChart = barColors.Keys.ToArray();
        return CreateBarChartImage(new BarChartDrawingData("AoCRolesBackground_Right.png",
                                                           "AoCRaceForeground.png",
                                                           barColors,
                                                           110.33f,
                                                           3f),
                                   (DiscordRoleId)_dynamicConfiguration.DiscordMapping["AshesOfCreationPrimaryGameDiscordRoleId"],
                                   rolesInChart);
    }

    Stream IImageProvider.CreateAocGuildPreferenceDistributionImage()
    {
        var barColors = new Dictionary<string, SKColor>
        {
            { "Hand of Unity Guild", SKColors.CornflowerBlue },
            { "Fellowship of Unity Guild", SKColors.LightSkyBlue }
        };
        var rolesInChart = barColors.Keys.ToArray();
        var barLabelOverrides = new Dictionary<string, string>
        {
            { "Hand of Unity Guild", "HoU" },
            { "Fellowship of Unity Guild", "FoU" }
        };
        return CreateBarChartImage(new BarChartDrawingData("AoCRolesBackground_Right.png",
                                                           "AoCGuildPreferenceForeground.png",
                                                           barColors,
                                                           368f,
                                                           131.5f)
                                   {
                                       LabelFontSize = 16f
                                   },
                                   (DiscordRoleId)_dynamicConfiguration.DiscordMapping["AshesOfCreationPrimaryGameDiscordRoleId"],
                                   rolesInChart,
                                   barLabelOverrides);
    }

    Stream IImageProvider.LoadLaunchRosterImage()
    {
        var content = GetImageFromResource("AoCLaunchRoster.jpg");
        return CreateImage(1320,
                           600,
                           bitmap =>
                           {
                               using var canvas = new SKCanvas(bitmap);
                               canvas.DrawImage(content, 0, 0);
                           });
    }

    public async Task<Stream> CreateProfileCardImage(DiscordUserId userID,
                                                     string avatarUrl,
                                                     ProfileInfoResponse profileData,
                                                     string guildTag,
                                                     bool hasPvpRole,
                                                     bool hasArtisanRole,
                                                     bool hasPveRole)
    {
        const int imageWidth = 1000;
        const int imageHeight = 750;

        var escapedRankName = profileData.SeasonalRankName.ToLower().Replace(" ", "-");

        // Get current nickname
        var displayName = _discordAccess.GetCurrentDisplayName(userID);

        // Load rank-based background image
        var backgroundImage = GetImageFromResource($"units_profiles.{escapedRankName}-background.png");

        // Load rank-based avatar frame
        var avatarFrameImage = GetImageFromResource($"units_profiles.{escapedRankName}-frame.png");
        
        // Load archetype images
        var archetypeImages = new Dictionary<string, SKImage>();
        foreach (var character in profileData.Characters)
        {
            if (!archetypeImages.ContainsKey(character.PrimaryArchetype))
                LoadArchetypeImage(archetypeImages, character.PrimaryArchetype);

            if (!archetypeImages.ContainsKey(character.SecondaryArchetype))
                LoadArchetypeImage(archetypeImages, character.SecondaryArchetype);
        }

        // Load user image
        SKImage? userImage = null;
        var userImageBytes = await _webAccess.GetContentFromUrlAsync(avatarUrl);
        if (userImageBytes != null)
        {
            await using var userImageStream = new MemoryStream(userImageBytes);
            userImage = SKImage.FromEncodedData(userImageStream);
            // Resize to 275 x 275 px
            var bitmap = SKBitmap.FromImage(userImage);
            bitmap = bitmap.Resize(new SKSizeI(275, 275), SKFilterQuality.High);
            userImage = SKImage.FromBitmap(bitmap);
        }

        // Create image
        return CreateImage(imageWidth,
                           imageHeight,
                           bitmap =>
                           {
                               ProfileCardAssembler.AssembleProfileCard(bitmap,
                                                                        displayName,
                                                                        backgroundImage,
                                                                        avatarFrameImage,
                                                                        userImage,
                                                                        archetypeImages,
                                                                        profileData,
                                                                        guildTag,
                                                                        hasPvpRole,
                                                                        hasArtisanRole,
                                                                        hasPveRole);
                           });

        void LoadArchetypeImage(IDictionary<string, SKImage> skImages,
                                string archetypeName)
        {
            var archetypeImage = GetImageFromResource($"archetypes.{archetypeName.ToLower()}.png");
            skImages.Add(archetypeName, archetypeImage);
        }
    }

    Stream IImageProvider.LoadClassListImage()
    {
        var content = GetImageFromResource("AoCClassList.jpg");
        return CreateImage(1629,
                           916,
                           bitmap =>
                           {
                               using var canvas = new SKCanvas(bitmap);
                               canvas.DrawImage(content, 0, 0);
                           });
    }

    Stream IImageProvider.LoadArtisanProfessionsImage()
    {
        var content = GetImageFromResource("AoCArtisanProfessions.png");
        return CreateImage(999,
                           599,
                           bitmap =>
                           {
                               using var canvas = new SKCanvas(bitmap);
                               canvas.DrawImage(content, 0, 0);
                           });
    }

    Stream IImageProvider.CreateLostArkPlayStyleDistributionImage()
    {
        var barColors = new Dictionary<string, SKColor>
        {
            { "DPS", SKColors.DarkRed },
            { "Support", SKColors.ForestGreen }
        };
        var rolesInChart = barColors.Keys.ToArray();
        return CreateBarChartImage(new BarChartDrawingData("LostArkPlaystyleBackground.png",
                                                           "LostArkPlaystyleForeground.png",
                                                           barColors,
                                                           369f,
                                                           132f),
                                   (DiscordRoleId)_dynamicConfiguration.DiscordMapping["LostArkPrimaryGameRoleId"],
                                   rolesInChart);
    }

    Stream IImageProvider.CreateWowRetailPlayStyleDistributionImage()
    {
        var barColors = new Dictionary<string, SKColor>
        {
            { "Melee DPS", SKColors.DarkRed },
            { "Ranged DPS", SKColors.Teal },
            { "Healer", SKColors.ForestGreen },
            { "Tank", SKColors.SaddleBrown }
        };
        var rolesInChart = barColors.Keys.ToArray();
        return CreateBarChartImage(new BarChartDrawingData("WowRetailPlaystyleBackground.png",
                                                           "WowRetailPlaystyleForeground.png",
                                                           barColors,
                                                           222f,
                                                           56f),
                                   (DiscordRoleId)_dynamicConfiguration.DiscordMapping["WowRetailPrimaryGameRoleId"],
                                   rolesInChart);
    }

    Stream IImageProvider.CreateAocRolePreferenceDistributionImage()
    {
        var barColors = new Dictionary<string, SKColor>
        {
            { "Damage Dealer Role", SKColors.DarkRed },
            { "Support Role", SKColors.ForestGreen },
            { "Tank Role", SKColors.SaddleBrown }
        };
        var rolesInChart = barColors.Keys.ToArray();
        var barLabelOverrides = new Dictionary<string, string>
        {
            { "Damage Dealer Role", "DPS" },
            { "Support Role", "Support" },
            { "Tank Role", "Tank" }
        };
        return CreateBarChartImage(new BarChartDrawingData("AoCRolesBackground_Centered.png",
                                                           "AoCRolePreferenceForeground.png",
                                                           barColors,
                                                           278f,
                                                           83f),
                                   (DiscordRoleId)_dynamicConfiguration.DiscordMapping["AshesOfCreationPrimaryGameDiscordRoleId"],
                                   rolesInChart,
                                   barLabelOverrides);
    }

    Stream IImageProvider.CreateTnlRolePreferenceDistributionImage()
    {
        var barColors = new Dictionary<string, SKColor>
        {
            { "DPS", SKColors.DarkRed },
            { "Healer", SKColors.ForestGreen },
            { "Tank", SKColors.SaddleBrown }
        };
        var rolesInChart = barColors.Keys.ToArray();
        return CreateBarChartImage(new BarChartDrawingData("TnLRolesBackground_Centered.png",
                                                           "TnLRolePreferenceForeground.png",
                                                           barColors,
                                                           278f,
                                                           83f),
                                   (DiscordRoleId)_dynamicConfiguration.DiscordMapping["ThroneAndLibertyPrimaryGameDiscordRoleId"],
                                   rolesInChart);
    }

    Stream IImageProvider.CreateTnlWeaponDistributionImage()
    {
        var barColors = new Dictionary<string, SKColor>
        {
            { "XBow", SKColors.YellowGreen },
            { "LBow", SKColors.ForestGreen },
            { "Daggers", SKColors.IndianRed },
            { "GS", SKColors.DarkRed },
            { "SnS", SKColors.DarkOrange },
            { "Staff", SKColors.MidnightBlue },
            { "Wand", SKColors.DodgerBlue }
        };
        var rolesInChart = barColors.Keys.ToArray();
        return CreateBarChartImage(new BarChartDrawingData("TnLRolesBackground_Centered.png",
                                                           "TnLWeaponForeground.png",
                                                           barColors,
                                                           139f,
                                                           14f),
                                   (DiscordRoleId)_dynamicConfiguration.DiscordMapping["ThroneAndLibertyPrimaryGameDiscordRoleId"],
                                   rolesInChart);
    }

    private class BarChartDrawingData
    {
        public const int ImageWidth = 1000;
        public const int ImageHeight = 600;
        public const int ContentTopOffset = 100;
        public const int BarTopOffset = ContentTopOffset + 50;
        public const int BarMaxHeight = 380;
        public const int BarWidth = 55;
        public const int LabelsTopOffset = BarTopOffset + BarMaxHeight + 35;
        public const int HorizontalInfoTextMargin = 25;
        public const int VerticalInfoTextMargin = 20;

        public string BackgroundImageName { get; }
        public string ForegroundImageName { get; }
        public Dictionary<string, SKColor> BarColors { get; }
        public float IndentIncrement { get; }
        public float InitialIndent { get; }

        public float LabelFontSize { get; set; }

        public BarChartDrawingData(string backgroundImageName,
                                   string foregroundImageName,
                                   Dictionary<string, SKColor> barColors,
                                   float indentIncrement,
                                   float initialIndent)
        {
            BackgroundImageName = backgroundImageName;
            ForegroundImageName = foregroundImageName;
            BarColors = barColors;
            IndentIncrement = indentIncrement;
            InitialIndent = initialIndent;

            LabelFontSize = 14f;
        }
    }
}