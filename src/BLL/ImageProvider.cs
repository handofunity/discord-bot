namespace HoU.GuildBot.BLL
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using JetBrains.Annotations;
    using Shared.BLL;

    [UsedImplicitly]
    public class ImageProvider : IImageProvider
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Fields

        private readonly IGameRoleProvider _gameRoleProvider;

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Constructors

        public ImageProvider(IGameRoleProvider gameRoleProvider)
        {
            _gameRoleProvider = gameRoleProvider;
        }

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region IImageProvider Members

        Stream IImageProvider.CreateAocRolesImage()
        {
            const int imageHeight = 600;
            const int imageWidth = 1000;
            const int contentTopOffset = 100;
            const int indentIncrement = 125;
            const int contentAreaHeight = imageHeight - contentTopOffset;
            const int barTopOffset = contentTopOffset + 50;
            const int barWidth = 55;
            const int barMaxHeight = 380;
            const int labelsTopOffset = barTopOffset + barMaxHeight + 25;
            const int infoTextMargin = 5;

            var ff = new FontFamily("Arial");

            // Collect data
            var game = _gameRoleProvider.Games.Single(m => m.ShortName == "AoC");
            var distribution = _gameRoleProvider.GetGameRoleDistribution(game);

            // Load guild logo
            var assembly = typeof(ImageProvider).Assembly;
            var guildLogoResourceName = assembly.GetManifestResourceNames().Single(m => m.EndsWith("GuildLogo.png"));
            Image guildLogo;
            using (var guildLogoStream = assembly.GetManifestResourceStream(guildLogoResourceName))
            {
                guildLogo = Image.FromStream(guildLogoStream, true, true);
            }

            // Create base image
            var image = new Bitmap(imageWidth, imageHeight);
            var graph = Graphics.FromImage(image);

            // Background
            graph.Clear(Color.White);
            
            // Draw guild logo
            graph.DrawImage(guildLogo, new PointF(0, 0));

            // Draw header
            var titleFont = new Font(ff, 28, FontStyle.Bold);
            var title = $"Class Distribution for {game.ShortName}";
            var requiredSize = graph.MeasureString("title", titleFont);
            var leftOffset = guildLogo.Width;
            var topOffset = (int)((guildLogo.Height - requiredSize.Height) / 2);
            graph.DrawString(title, titleFont, Brushes.Black, leftOffset, topOffset);

            var infoFont = new Font(ff, 16);
            var gameMembersText = $"Game Members: {distribution.GameMembers}";
            var createdOnText = $"Created: {DateTime.UtcNow:yyyy-MM-dd}";
            var gameMembersRequiredSize = graph.MeasureString(gameMembersText, infoFont);
            var createdOnRequiredSize = graph.MeasureString(createdOnText, infoFont);
            topOffset = (contentTopOffset - ((int)gameMembersRequiredSize.Height + (int)createdOnRequiredSize.Height + infoTextMargin)) / 2;
            graph.DrawString(gameMembersText, infoFont, Brushes.Black, imageWidth - infoTextMargin - gameMembersRequiredSize.Width, topOffset);
            topOffset = topOffset + (int)gameMembersRequiredSize.Height + infoTextMargin;
            graph.DrawString(createdOnText, infoFont, Brushes.Black, imageWidth - infoTextMargin - createdOnRequiredSize.Width, topOffset);

            // Lines
            var blackPen = new Pen(Color.Black);
            for (var i = 1; i <= 7; i++)
            {
                var offset = i * indentIncrement;
                graph.DrawLine(blackPen, offset, contentTopOffset, offset, contentTopOffset + contentAreaHeight);
            }
            graph.DrawLine(blackPen, 0, contentTopOffset, imageWidth, contentTopOffset);

            // Data
            var indent = 0;
            var labelFont = new Font(ff, 14, FontStyle.Bold);
            var amountFont = new Font(ff, 16, FontStyle.Bold);
            var pens = new Dictionary<string, Pen>
            {
                {"Fighter", new Pen(Color.BurlyWood) },
                {"Tank", new Pen(Color.CornflowerBlue) },
                {"Mage", new Pen(Color.DarkOrange) },
                {"Summoner", new Pen(Color.Indigo) },
                {"Ranger", new Pen(Color.DarkGreen) },
                {"Cleric", new Pen(Color.Goldenrod) },
                {"Bard", new Pen(Color.LightPink) },
                {"Rogue", new Pen(Color.IndianRed) }
            };
            double maxCount = distribution.RoleDistribution.Values.Max();
            foreach (var d in distribution.RoleDistribution.OrderByDescending(m => m.Value).ThenBy(m => m.Key))
            {
                // Class label
                requiredSize = graph.MeasureString(d.Key, labelFont);
                graph.DrawString(d.Key, labelFont, Brushes.Black, new PointF(indent + (indentIncrement - requiredSize.Width) / 2, labelsTopOffset));

                // Bar
                var pen = pens[d.Key];
                var height = (int)(d.Value / maxCount * barMaxHeight);
                var currentBarTopOffset = barTopOffset + barMaxHeight - height;
                leftOffset = indent + (indentIncrement - barWidth) / 2;
                var rect = new Rectangle(leftOffset, currentBarTopOffset, barWidth, height);
                graph.FillRectangle(pen.Brush, rect);
                graph.DrawRectangle(pen, leftOffset, currentBarTopOffset, barWidth, height);

                // Amount label
                var amount = d.Value.ToString(CultureInfo.InvariantCulture);
                requiredSize = graph.MeasureString(amount, amountFont);
                graph.DrawString(amount, amountFont, Brushes.Black, new PointF(indent + (indentIncrement - requiredSize.Width) / 2, currentBarTopOffset - 10 - requiredSize.Height));

                indent += indentIncrement;
            }

            // Prepare result stream
            var result = new MemoryStream();
            image.Save(result, ImageFormat.Png);

            // Set result pointer back to 0 for the result consumer to read
            result.Position = 0;
            // Closing the stream is up to the consumer
            return result;
        }

        #endregion
    }
}