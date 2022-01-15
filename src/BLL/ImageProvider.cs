using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using HoU.GuildBot.Shared.BLL;
using HoU.GuildBot.Shared.DAL;
using HoU.GuildBot.Shared.StrongTypes;
using SkiaSharp;

namespace HoU.GuildBot.BLL;

[UsedImplicitly]
public class ImageProvider : IImageProvider
{
    private readonly IGameRoleProvider _gameRoleProvider;
    private readonly IWebAccess _webAccess;
    private readonly IDiscordAccess _discordAccess;
    private readonly IDynamicConfiguration _dynamicConfiguration;

    public ImageProvider(IGameRoleProvider gameRoleProvider,
                         IWebAccess webAccess,
                         IDiscordAccess discordAccess,
                         IDynamicConfiguration dynamicConfiguration)
    {
        _gameRoleProvider = gameRoleProvider;
        _webAccess = webAccess;
        _discordAccess = discordAccess;
        _dynamicConfiguration = dynamicConfiguration;
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
        var guildLogoResourceName = assembly.GetManifestResourceNames().Single(m => m.EndsWith(name));
        using var guildLogoStream = assembly.GetManifestResourceStream(guildLogoResourceName);
        return SKImage.FromEncodedData(guildLogoStream);
    }

    private Stream CreateBarChartImage(BarChartDrawingData barChartDrawingData,
                                       DiscordRoleId primaryGameDiscordRoleId,
                                       string[] rolesInChart)
    {
        // Collect data
        var game = _gameRoleProvider.Games.Single(m => m.PrimaryGameDiscordRoleId == primaryGameDiscordRoleId);
        var (gameMembers, roleDistribution) = _gameRoleProvider.GetGameRoleDistribution(game);
        roleDistribution = roleDistribution.Where(m => rolesInChart.Any(r => r.EndsWith(m.Key)))
                                           .ToDictionary(m => m.Key, m => m.Value);
            
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
                                   var labelPaint = new SKPaint(labelFont) { Color = SKColors.Black };
                                   var requiredLabelSize = new SKRect();
                                   labelPaint.MeasureText(roleName, ref requiredLabelSize);
                                   canvas.DrawText(roleName,
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
                                   var color = barChartDrawingData.BarColors[roleName];
                                   var barPaint = new SKPaint { Color = color };
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
            { "Crafting", SKColors.DarkOrange }
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
        
    public async Task<Stream> CreateProfileImage(DiscordUserId userID,
                                                 string avatarUrl)
    {
        const int imageWidth = 400;
        const int imageHeight = 300;
        const int maxNameLengthInPixel = 180;

        // Get current nickname
        var displayName = _discordAccess.GetCurrentDisplayName(userID);

        // Fetch data from UNITS system
        // TODO
        var userPointsRating = "Medium";

        // Load background image
        var backgroundImage = GetImageFromResource("UnitsProfileBackground.png");

        // Load nameplate
        var nameplateImage = GetImageFromResource("UnitsProfileNameplate.png");

        // Load user image
        SKImage? userImage = null;
        var userImageBytes = await _webAccess.GetContentFromUrlAsync(avatarUrl);
        if (userImageBytes != null)
        {
            await using var userImageStream = new MemoryStream(userImageBytes);
            userImage = SKImage.FromEncodedData(userImageStream);
            // Resize to 100 x 100 px
            var bitmap = SKBitmap.FromImage(userImage);
            bitmap = bitmap.Resize(new SKSizeI(100, 100), SKFilterQuality.High);
            userImage = SKImage.FromBitmap(bitmap);
        }

        SKImage? userProfileBadgeFrameImage = null;
        // Load profile badge frame
        if (userImage != null) userProfileBadgeFrameImage = GetImageFromResource($"UnitsProfileBadge_{userPointsRating}Points.png");

        // Create image
        return CreateImage(imageWidth,
                           imageHeight,
                           bitmap =>
                           {
                               using var canvas = new SKCanvas(bitmap);

                               var ff = SKTypeface.FromFamilyName("Arial");
                               var textColor = new SKColor(231, 185, 89);

                               // Background
                               canvas.DrawImage(backgroundImage, 0, 0);

                               // Draw user image and frame
                               var offsetNameByImage = false;
                               if (userImage != null && userProfileBadgeFrameImage != null)
                               {
                                   canvas.DrawImage(userProfileBadgeFrameImage, 5, 5);
                                   canvas.DrawImage(userImage, 52, 52);
                                   offsetNameByImage = true;
                               }

                               // Draw nameplate depending on user image and frame
                               var nameplateLeftOffset = offsetNameByImage ? 201 : 105;
                               canvas.DrawImage(nameplateImage, nameplateLeftOffset, 41);

                               // Calculate name and name position
                               var nameFont = new SKFont(ff);
                               var namePaint = new SKPaint(nameFont)
                               {
                                   Color = textColor
                               };
                               SKRect nameSize = new();
                               var nameToUse = displayName;
                               do
                               {
                                   namePaint.MeasureText(nameToUse, ref nameSize);
                                   if (nameSize.Width > maxNameLengthInPixel) nameToUse = nameToUse.Substring(0, nameToUse.Length - 1);
                               } while (nameSize.Width > maxNameLengthInPixel);

                               var nameLeftOffset = nameplateLeftOffset + 95 - nameSize.Width / 2;
                               var nameTopOffset = 101 - nameSize.Height / 2;


                               // Write name depending on user image and frame
                               canvas.DrawText(nameToUse,
                                               nameLeftOffset,
                                               nameTopOffset,
                                               nameFont,
                                               namePaint);
                           });
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