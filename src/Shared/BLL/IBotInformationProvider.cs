namespace HoU.GuildBot.Shared.BLL
{
    using System.Collections.Generic;
    using Objects;

    public interface IBotInformationProvider
    {
        string GetEnvironmentName();
        string GetFormatedVersion();
        EmbedData GetData();
        Dictionary<byte, string[]> GetAvailableFonts();
    }
}