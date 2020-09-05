using System.Collections.Generic;
using HoU.GuildBot.Shared.Objects;

namespace HoU.GuildBot.Shared.BLL
{
    public interface IBotInformationProvider
    {
        string GetEnvironmentName();
        string GetFormatedVersion();
        EmbedData GetData();
        Dictionary<byte, string[]> GetAvailableFonts();
    }
}