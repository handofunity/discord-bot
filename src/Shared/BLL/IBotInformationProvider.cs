namespace HoU.GuildBot.Shared.BLL
{
    using Objects;

    public interface IBotInformationProvider
    {
        string GetEnvironmentName();
        string GetFormatedVersion();
        EmbedData GetData();
    }
}