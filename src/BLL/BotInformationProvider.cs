namespace HoU.GuildBot.BLL
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using Shared.BLL;
    using Shared.Objects;

    public class BotInformationProvider : IBotInformationProvider
    {
        private readonly RuntimeInformation _runtimeInformation;

        public BotInformationProvider(RuntimeInformation runtimeInformation)
        {
            _runtimeInformation = runtimeInformation;
        }

        string IBotInformationProvider.GetEnvironmentName() => _runtimeInformation.Environment;

        string IBotInformationProvider.GetFormatedVersion() => _runtimeInformation.Version.ToString(3);

        EmbedData IBotInformationProvider.GetData()
        {
            return new EmbedData
            {
                Title = "Bot information",
                Color = Colors.Orange,
                Fields = new []
                {
                    new EmbedField("Environment", _runtimeInformation.Environment, true), 
                    new EmbedField("Version", _runtimeInformation.Version, true), 
                    new EmbedField("UP-time", (DateTime.Now.ToUniversalTime() - _runtimeInformation.StartTime).ToString(@"dd\.hh\:mm\:ss"), true), 
                    new EmbedField("Start-time", _runtimeInformation.StartTime.ToString("dd.MM.yyyy HH:mm:ss") + " UTC", false), 
                    new EmbedField("Server time", DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss \"UTC\"zzz"), false)
                }
            };
        }

        Dictionary<byte, string[]> IBotInformationProvider.GetAvailableFonts()
        {
            var allFonts = FontFamily.Families
                                     .Where(m => !string.IsNullOrWhiteSpace(m.Name))
                                     .Select(m => m.Name)
                                     .OrderBy(m => m)
                                     .ToArray();
            var fontGroups = new Dictionary<byte, List<string>>();
            byte fontGroup = 0;
            string pendingFont = null;
            foreach (var font in allFonts)
            {
                if (!fontGroups.TryGetValue(fontGroup, out var fontList))
                {
                    fontList = new List<string>();
                    fontGroups[fontGroup] = fontList;
                    if (pendingFont != null)
                    {
                        fontList.Add(pendingFont);
                        pendingFont = null;
                    }
                }

                var fontGroupLength = fontList.Any() ? fontList.Sum(m => m.Length) : 0;
                var newLength = fontGroupLength + font.Length;
                if (newLength <= 1000)
                {
                    fontList.Add(font);
                }
                else
                {
                    fontGroup++;
                    pendingFont = font;
                }
            }

            return fontGroups.ToDictionary(m => m.Key, m => m.Value.ToArray());
        }
    }
}