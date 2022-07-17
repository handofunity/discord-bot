namespace HoU.GuildBot.Shared.BLL;

public interface IBotInformationProvider
{
    string GetEnvironmentName();

    string GetFormattedVersion();

    EmbedData GetData();

    Dictionary<byte, string[]> GetAvailableFonts();
}