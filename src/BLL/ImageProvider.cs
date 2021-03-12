using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using HoU.GuildBot.Shared.BLL;
using HoU.GuildBot.Shared.DAL;
using HoU.GuildBot.Shared.Objects;
using HoU.GuildBot.Shared.StrongTypes;

namespace HoU.GuildBot.BLL
{
    [UsedImplicitly]
    public class ImageProvider : IImageProvider
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Fields

        private readonly IGameRoleProvider _gameRoleProvider;
        private readonly IWebAccess _webAccess;
        private readonly IDiscordAccess _discordAccess;

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Constructors

        public ImageProvider(IGameRoleProvider gameRoleProvider,
                             IWebAccess webAccess,
                             IDiscordAccess discordAccess)
        {
            _gameRoleProvider = gameRoleProvider;
            _webAccess = webAccess;
            _discordAccess = discordAccess;
        }

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Private Methods

        private static Stream CreateImage(int width,
                                          int height,
                                          Action<Graphics> graphicsModifier)
        {
            var image = new Bitmap(width, height);
            var graphics = Graphics.FromImage(image);

            // Modify graphic
            graphicsModifier(graphics);

            // Prepare result stream
            var result = new MemoryStream();
            image.Save(result, ImageFormat.Png);

            // Set result pointer back to 0 for the result consumer to read
            result.Position = 0;
            // Closing the stream is up to the consumer
            return result;
        }

        private static Image GetImageFromResource(string name)
        {
            var assembly = typeof(ImageProvider).Assembly;
            var guildLogoResourceName = assembly.GetManifestResourceNames().Single(m => m.EndsWith(name));
            using (var guildLogoStream = assembly.GetManifestResourceStream(guildLogoResourceName))
            {
                return Image.FromStream(guildLogoStream, true, true);
            }
        }

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region IImageProvider Members

        Stream IImageProvider.CreateAocClassDistributionImage()
        {
            const int imageWidth = 1000;
            const int imageHeight = 600;
            const int contentTopOffset = 100;
            const int indentIncrement = 125;
            const int barTopOffset = contentTopOffset + 50;
            const int barWidth = 55;
            const int barMaxHeight = 380;
            const int labelsTopOffset = barTopOffset + barMaxHeight + 25;
            const int horizontalInfoTextMargin = 15;
            const int verticalInfoTextMargin = 5;

            // Collect data
            var game = _gameRoleProvider.Games.Single(m => m.ShortName == Constants.RoleMenuGameShortNames.AshesOfCreation);
            var (gameMembers, roleDistribution) = _gameRoleProvider.GetGameRoleDistribution(game);
            var rolesInChart = new[]
            {
                nameof(Constants.AocRoleEmojis.Fighter),
                nameof(Constants.AocRoleEmojis.Tank),
                nameof(Constants.AocRoleEmojis.Mage),
                nameof(Constants.AocRoleEmojis.Summoner),
                nameof(Constants.AocRoleEmojis.Ranger),
                nameof(Constants.AocRoleEmojis.Cleric),
                nameof(Constants.AocRoleEmojis.Bard),
                nameof(Constants.AocRoleEmojis.Rogue)
            };
            roleDistribution = roleDistribution.Where(m => rolesInChart.Any(r => r.EndsWith(m.Key)))
                                               .ToDictionary(m => m.Key, m => m.Value);

            // Load background and foreground image
            var backgroundImage = GetImageFromResource("AoCRolesBackground_Right.png");
            var foregroundImage = GetImageFromResource("AoCClassForeground.png");

            // Create image
            return CreateImage(imageWidth,
                               imageHeight,
                               graphics =>
                               {
                                   var ff = new FontFamily("Arial");
                                   
                                   // Draw background image
                                   graphics.DrawImage(backgroundImage, new PointF(0, 0));

                                   // Draw meta info
                                   var infoFont = new Font(ff, 14);
                                   var gameMembersText = $"Game Members: {gameMembers}";
                                   var createdOnText = $"Created: {DateTime.UtcNow:yyyy-MM-dd}";
                                   var gameMembersRequiredSize = graphics.MeasureString(gameMembersText, infoFont);
                                   var createdOnRequiredSize = graphics.MeasureString(createdOnText, infoFont);
                                   var topOffset = (contentTopOffset - ((int) gameMembersRequiredSize.Height + (int) createdOnRequiredSize.Height + verticalInfoTextMargin)) / 2;
                                   graphics.DrawString(gameMembersText, infoFont, Brushes.Black, imageWidth - horizontalInfoTextMargin - gameMembersRequiredSize.Width, topOffset);
                                   topOffset = topOffset + (int) gameMembersRequiredSize.Height + verticalInfoTextMargin;
                                   graphics.DrawString(createdOnText, infoFont, Brushes.Black, imageWidth - horizontalInfoTextMargin - createdOnRequiredSize.Width, topOffset);
                                   
                                   // Data
                                   var indent = 0;
                                   var labelFont = new Font(ff, 12, FontStyle.Bold);
                                   var amountFont = new Font(ff, 16, FontStyle.Bold);
                                   var pens = new Dictionary<string, Pen>
                                   {
                                       {"Fighter", new Pen(Color.BurlyWood)},
                                       {"Tank", new Pen(Color.CornflowerBlue)},
                                       {"Mage", new Pen(Color.DarkOrange)},
                                       {"Summoner", new Pen(Color.Indigo)},
                                       {"Ranger", new Pen(Color.DarkGreen)},
                                       {"Cleric", new Pen(Color.Goldenrod)},
                                       {"Bard", new Pen(Color.LightPink)},
                                       {"Rogue", new Pen(Color.IndianRed)}
                                   };
                                   double maxCount = roleDistribution.Values.Max();
                                   foreach (var d in roleDistribution.OrderByDescending(m => m.Value).ThenBy(m => m.Key))
                                   {
                                       // Bar label
                                       var requiredSize = graphics.MeasureString(d.Key, labelFont);
                                       graphics.DrawString(d.Key, labelFont, Brushes.Black, new PointF(indent + (indentIncrement - requiredSize.Width) / 2, labelsTopOffset));

                                       // Bar
                                       var pen = pens[d.Key];
                                       var height = (int) (d.Value / maxCount * barMaxHeight);
                                       var currentBarTopOffset = barTopOffset + barMaxHeight - height;
                                       var leftOffset = indent + (indentIncrement - barWidth) / 2;
                                       var rect = new Rectangle(leftOffset, currentBarTopOffset, barWidth, height);
                                       graphics.FillRectangle(pen.Brush, rect);
                                       graphics.DrawRectangle(pen, leftOffset, currentBarTopOffset, barWidth, height);

                                       // Amount label
                                       var amount = d.Value.ToString(CultureInfo.InvariantCulture);
                                       requiredSize = graphics.MeasureString(amount, amountFont);
                                       graphics.DrawString(amount,
                                                        amountFont,
                                                        Brushes.Black,
                                                        new PointF(indent + (indentIncrement - requiredSize.Width) / 2, currentBarTopOffset - 10 - requiredSize.Height));

                                       indent += indentIncrement;
                                   }

                                   // Draw foreground image
                                   graphics.DrawImage(foregroundImage, new PointF(0, 0));
                               });
        }

        Stream IImageProvider.CreateAocPlayStyleDistributionImage()
        {
            const int imageWidth = 1000;
            const int imageHeight = 600;
            const int contentTopOffset = 100;
            const int indentIncrement = 278;
            const int barTopOffset = contentTopOffset + 50;
            const int barWidth = 55;
            const int barMaxHeight = 380;
            const int labelsTopOffset = barTopOffset + barMaxHeight + 25;
            const int horizontalInfoTextMargin = 15;
            const int verticalInfoTextMargin = 5;

            // Collect data
            var game = _gameRoleProvider.Games.Single(m => m.ShortName == Constants.RoleMenuGameShortNames.AshesOfCreation);
            var (gameMembers, roleDistribution) = _gameRoleProvider.GetGameRoleDistribution(game);
            var rolesInChart = new[]
            {
                nameof(Constants.AocRoleEmojis.PvE),
                nameof(Constants.AocRoleEmojis.PvP),
                nameof(Constants.AocRoleEmojis.Crafting)
            };
            roleDistribution = roleDistribution.Where(m => rolesInChart.Any(r => r.EndsWith(m.Key)))
                                               .ToDictionary(m => m.Key, m => m.Value);

            // Load background and foreground image
            var backgroundImage = GetImageFromResource("AoCRolesBackground_Centered.png");
            var foregroundImage = GetImageFromResource("AoCPlaystyleForeground.png");

            // Create image
            return CreateImage(imageWidth,
                               imageHeight,
                               graphics =>
                               {
                                   var ff = new FontFamily("Arial");

                                   // Draw background image
                                   graphics.DrawImage(backgroundImage, new PointF(0, 0));

                                   // Draw meta info
                                   var infoFont = new Font(ff, 14);
                                   var gameMembersText = $"Game Members: {gameMembers}";
                                   var createdOnText = $"Created: {DateTime.UtcNow:yyyy-MM-dd}";
                                   var gameMembersRequiredSize = graphics.MeasureString(gameMembersText, infoFont);
                                   var createdOnRequiredSize = graphics.MeasureString(createdOnText, infoFont);
                                   var topOffset = (contentTopOffset - ((int)gameMembersRequiredSize.Height + (int)createdOnRequiredSize.Height + verticalInfoTextMargin)) / 2;
                                   graphics.DrawString(gameMembersText, infoFont, Brushes.Black, imageWidth - horizontalInfoTextMargin - gameMembersRequiredSize.Width, topOffset);
                                   topOffset = topOffset + (int)gameMembersRequiredSize.Height + verticalInfoTextMargin;
                                   graphics.DrawString(createdOnText, infoFont, Brushes.Black, imageWidth - horizontalInfoTextMargin - createdOnRequiredSize.Width, topOffset);
                                   
                                   // Data
                                   var indent = 83;
                                   var labelFont = new Font(ff, 12, FontStyle.Bold);
                                   var amountFont = new Font(ff, 16, FontStyle.Bold);
                                   var pens = new Dictionary<string, Pen>
                                   {
                                       {"PvE", new Pen(Color.Green)},
                                       {"PvP", new Pen(Color.DarkRed)},
                                       {"Crafting", new Pen(Color.DarkOrange)}
                                   };
                                   double maxCount = roleDistribution.Values.Max();
                                   foreach (var d in roleDistribution.OrderByDescending(m => m.Value).ThenBy(m => m.Key))
                                   {
                                       // Bar label
                                       var requiredSize = graphics.MeasureString(d.Key, labelFont);
                                       graphics.DrawString(d.Key, labelFont, Brushes.Black, new PointF(indent + (indentIncrement - requiredSize.Width) / 2, labelsTopOffset));

                                       // Bar
                                       var pen = pens[d.Key];
                                       var height = (int)(d.Value / maxCount * barMaxHeight);
                                       var currentBarTopOffset = barTopOffset + barMaxHeight - height;
                                       var leftOffset = indent + (indentIncrement - barWidth) / 2;
                                       var rect = new Rectangle(leftOffset, currentBarTopOffset, barWidth, height);
                                       graphics.FillRectangle(pen.Brush, rect);
                                       graphics.DrawRectangle(pen, leftOffset, currentBarTopOffset, barWidth, height);

                                       // Amount label
                                       var amount = d.Value.ToString(CultureInfo.InvariantCulture);
                                       requiredSize = graphics.MeasureString(amount, amountFont);
                                       graphics.DrawString(amount,
                                                        amountFont,
                                                        Brushes.Black,
                                                        new PointF(indent + (indentIncrement - requiredSize.Width) / 2, currentBarTopOffset - 10 - requiredSize.Height));

                                       indent += indentIncrement;
                                   }

                                   // Draw foreground image
                                   graphics.DrawImage(foregroundImage, new PointF(0, 0));
                               });
        }

        Stream IImageProvider.CreateAocRaceDistributionImage()
        {
            const int imageWidth = 1000;
            const int imageHeight = 600;
            const int contentTopOffset = 100;
            const float indentIncrement = 110.33f;
            const int barTopOffset = contentTopOffset + 50;
            const int barWidth = 55;
            const int barMaxHeight = 380;
            const int labelsTopOffset = barTopOffset + barMaxHeight + 25;
            const int horizontalInfoTextMargin = 15;
            const int verticalInfoTextMargin = 5;

            // Collect data
            var game = _gameRoleProvider.Games.Single(m => m.ShortName == Constants.RoleMenuGameShortNames.AshesOfCreation);
            var (gameMembers, roleDistribution) = _gameRoleProvider.GetGameRoleDistribution(game);
            var rolesInChart = new[]
            {
                nameof(Constants.AocRoleEmojis.Kaelar),
                nameof(Constants.AocRoleEmojis.Vaelune),
                nameof(Constants.AocRoleEmojis.Empyrean),
                nameof(Constants.AocRoleEmojis.Pyrai),
                nameof(Constants.AocRoleEmojis.Renkai),
                nameof(Constants.AocRoleEmojis.Vek),
                nameof(Constants.AocRoleEmojis.Dunir),
                nameof(Constants.AocRoleEmojis.Nikua),
                nameof(Constants.AocRoleEmojis.Tulnar)
            };
            roleDistribution = roleDistribution.Where(m => rolesInChart.Any(r => r.EndsWith(m.Key)))
                                               .ToDictionary(m => m.Key, m => m.Value);

            // Load background and foreground image
            var backgroundImage = GetImageFromResource("AoCRolesBackground_Right.png");
            var foregroundImage = GetImageFromResource("AoCRaceForeground.png");

            // Create image
            return CreateImage(imageWidth,
                               imageHeight,
                               graphics =>
                               {
                                   var ff = new FontFamily("Arial");

                                   // Draw background image
                                   graphics.DrawImage(backgroundImage, new PointF(0, 0));

                                   // Draw meta info
                                   var infoFont = new Font(ff, 14);
                                   var gameMembersText = $"Game Members: {gameMembers}";
                                   var createdOnText = $"Created: {DateTime.UtcNow:yyyy-MM-dd}";
                                   var gameMembersRequiredSize = graphics.MeasureString(gameMembersText, infoFont);
                                   var createdOnRequiredSize = graphics.MeasureString(createdOnText, infoFont);
                                   var topOffset = (contentTopOffset - ((int)gameMembersRequiredSize.Height + (int)createdOnRequiredSize.Height + verticalInfoTextMargin)) / 2;
                                   graphics.DrawString(gameMembersText, infoFont, Brushes.Black, imageWidth - horizontalInfoTextMargin - gameMembersRequiredSize.Width, topOffset);
                                   topOffset = topOffset + (int)gameMembersRequiredSize.Height + verticalInfoTextMargin;
                                   graphics.DrawString(createdOnText, infoFont, Brushes.Black, imageWidth - horizontalInfoTextMargin - createdOnRequiredSize.Width, topOffset);

                                   // Data
                                   var indent = 3f;
                                   var labelFont = new Font(ff, 12, FontStyle.Bold);
                                   var amountFont = new Font(ff, 16, FontStyle.Bold);
                                   var pens = new Dictionary<string, Pen>
                                   {
                                       {"Kaelar", new Pen(Color.CornflowerBlue)},
                                       {"Vaelune", new Pen(Color.LightSkyBlue)},
                                       {"Empyrean", new Pen(Color.Indigo)},
                                       {"Pyrai", new Pen(Color.BlueViolet)},
                                       {"Renkai", new Pen(Color.SeaGreen)},
                                       {"Vek", new Pen(Color.ForestGreen)},
                                       {"Dunir", new Pen(Color.Brown)},
                                       {"Nikua", new Pen(Color.Crimson)},
                                       {"Tulnar", new Pen(Color.Gold)},
                                   };
                                   double maxCount = roleDistribution.Values.Max();
                                   foreach (var d in roleDistribution.OrderByDescending(m => m.Value).ThenBy(m => m.Key))
                                   {
                                       // Bar label
                                       var requiredSize = graphics.MeasureString(d.Key, labelFont);
                                       graphics.DrawString(d.Key, labelFont, Brushes.Black, new PointF(indent + (indentIncrement - requiredSize.Width) / 2, labelsTopOffset));

                                       // Bar
                                       var pen = pens[d.Key];
                                       var height = (int)(d.Value / maxCount * barMaxHeight);
                                       var currentBarTopOffset = barTopOffset + barMaxHeight - height;
                                       var leftOffset = indent + (indentIncrement - barWidth) / 2;
                                       var rect = new Rectangle((int)Math.Round(leftOffset), currentBarTopOffset, barWidth, height);
                                       graphics.FillRectangle(pen.Brush, rect);
                                       graphics.DrawRectangle(pen, leftOffset, currentBarTopOffset, barWidth, height);

                                       // Amount label
                                       var amount = d.Value.ToString(CultureInfo.InvariantCulture);
                                       requiredSize = graphics.MeasureString(amount, amountFont);
                                       graphics.DrawString(amount,
                                                        amountFont,
                                                        Brushes.Black,
                                                        new PointF(indent + (indentIncrement - requiredSize.Width) / 2, currentBarTopOffset - 10 - requiredSize.Height));

                                       indent += indentIncrement;
                                   }

                                   // Draw foreground image
                                   graphics.DrawImage(foregroundImage, new PointF(0, 0));
                               });
        }

        public async Task<Stream> CreateProfileImage(DiscordUserID userID,
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
            Image userImage = null;
            var userImageBytes = await _webAccess.GetDiscordAvatarByUrl(avatarUrl);
            if (userImageBytes != null)
            {
                using (var userImageStream = new MemoryStream(userImageBytes))
                {
                    userImage = Image.FromStream(userImageStream);
                    // Resize to 100 x 100 px
                    userImage = new Bitmap(userImage, 100, 100);
                }
            }

            Image userProfileBadgeFrameImage = null;
            // Load profile badge frame
            if (userImage != null) userProfileBadgeFrameImage = GetImageFromResource($"UnitsProfileBadge_{userPointsRating}Points.png");

            // Create image
            return CreateImage(imageWidth,
                               imageHeight,
                               graphics =>
                               {
                                   var ff = new FontFamily("Arial");
                                   var textColor = Color.FromArgb(231, 185, 89);
                                   var textBrush = new SolidBrush(textColor);

                                   // Background
                                   graphics.DrawImage(backgroundImage, new PointF(0, 0));

                                   // Draw user image and frame
                                   var offsetNameByImage = false;
                                   if (userImage != null && userProfileBadgeFrameImage != null)
                                   {
                                       graphics.DrawImage(userProfileBadgeFrameImage, new PointF(5, 5));
                                       graphics.DrawImage(userImage, new PointF(52, 52));
                                       offsetNameByImage = true;
                                   }

                                   // Draw nameplate depending on user image and frame
                                   var nameplateLeftOffset = offsetNameByImage ? 201 : 105;
                                   graphics.DrawImage(nameplateImage, new PointF(nameplateLeftOffset, 41));

                                   // Calculate name and name position
                                   var nameFont = new Font(ff, 12, FontStyle.Regular);
                                   SizeF nameSize;
                                   var nameToUse = displayName;
                                   do
                                   {
                                       nameSize = graphics.MeasureString(nameToUse, nameFont);
                                       if (nameSize.Width > maxNameLengthInPixel) nameToUse = nameToUse.Substring(0, nameToUse.Length - 1);
                                   } while (nameSize.Width > maxNameLengthInPixel);

                                   var nameLeftOffset = nameplateLeftOffset + 95 - nameSize.Width / 2;
                                   var nameTopOffset = 101 - nameSize.Height / 2;

                                   
                                   // Write name depending on user image and frame
                                   graphics.DrawString(nameToUse,
                                                       nameFont,
                                                       textBrush,
                                                       new PointF(nameLeftOffset, nameTopOffset));
                               });
        }

        Stream IImageProvider.LoadClassListImage()
        {
            var content = GetImageFromResource("AoCClassList.jpg");
            return CreateImage(1629,
                               916,
                               graphics =>
                               {
                                   graphics.DrawImage(content, new PointF(0, 0));
                               });
        }

        #endregion
    }
}