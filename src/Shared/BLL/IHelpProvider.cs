namespace HoU.GuildBot.Shared.BLL
{
    using Objects;

    public interface IHelpProvider
    {
        (string Message, EmbedData EmbedData) GetHelp(ulong userId, string helpRequest);
    }
}