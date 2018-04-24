namespace HoU.GuildBot.BLL
{
    using System;
    using Shared.BLL;
    using Shared.Objects;

    public class BotInformationProvider : IBotInformationProvider
    {
        private readonly RuntimeInformation _runtimeInformation;

        public BotInformationProvider(RuntimeInformation runtimeInformation)
        {
            _runtimeInformation = runtimeInformation;
        }

        EmbedData IBotInformationProvider.GetData()
        {
            return new EmbedData
            {
                Title = "Bot information",
                Color = (230, 126, 34),
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
    }
}