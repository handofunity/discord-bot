namespace HoU.GuildBot.Shared.BLL
{
    using Objects;

    public interface IHelpProvider
    {
        (string Message, EmbedData Embed) GetHelp(ulong userId, string helpRequest);
    }
}