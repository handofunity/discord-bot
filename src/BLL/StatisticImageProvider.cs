namespace HoU.GuildBot.BLL
{
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using JetBrains.Annotations;
    using Shared.BLL;
    using Shared.DAL;

    [UsedImplicitly]
    public class StatisticImageProvider : IStatisticImageProvider
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Fields

        private readonly IGameRoleProvider _gameRoleProvider;

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Constructors

        public StatisticImageProvider(IGameRoleProvider gameRoleProvider)
        {
            _gameRoleProvider = gameRoleProvider;
        }

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region IStatisticImageProvider Members

        async Task<Stream> IStatisticImageProvider.CreateAocRolesImage()
        {
            // Collect data
            var game = _gameRoleProvider.Games.Single(m => m.ShortName == "AoC");
            var distribution = _gameRoleProvider.GetGameRoleDistribution(game);

            // Create base image
            var image = new Bitmap(1000, 500);
            var graph = Graphics.FromImage(image);

            // Background
            graph.Clear(Color.LightGray);

            const int indentIncrement = 125;

            // Lines
            for (var i = 1; i <= 7; i++)
            {
                var offset = i * indentIncrement;
                graph.DrawLine(new Pen(Color.Black), offset, 0, offset, image.Height);
            }

            // Data
            var indent = 0;
            var font = new Font(new FontFamily("Arial"), 16, FontStyle.Bold);
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
            double maxCount = distribution.Values.Max();
            foreach (var d in distribution)
            {
                // Class label
                var requiredSize = graph.MeasureString(d.Key, font);
                graph.DrawString(d.Key, font, Brushes.Black, new PointF(indent + (indentIncrement - requiredSize.Width) / 2, 450));

                // Bar
                var pen = pens[d.Key];
                const int width = 55;
                const int maxHeight = 380;
                var height = (int)(d.Value / maxCount * maxHeight);
                var barTopOffset = 50 + maxHeight - height;
                var leftOffset = indent + (indentIncrement - width) / 2;
                var rect = new Rectangle(leftOffset, barTopOffset, width, height);
                graph.FillRectangle(pen.Brush, rect);
                graph.DrawRectangle(pen, leftOffset, barTopOffset, width, height);

                // Amount label
                var amount = d.Value.ToString(CultureInfo.InvariantCulture);
                requiredSize = graph.MeasureString(amount, font);
                graph.DrawString(amount, font, Brushes.Black, new PointF(indent + (indentIncrement - requiredSize.Width) / 2, barTopOffset - 10 - requiredSize.Height));

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