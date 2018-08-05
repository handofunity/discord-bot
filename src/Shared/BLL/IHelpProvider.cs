namespace HoU.GuildBot.Shared.BLL
{
    using Objects;
    using StrongTypes;

    public interface IHelpProvider
    {
        (string Message, EmbedData EmbedData)[] GetHelp(DiscordUserID userId, string helpRequest);
    }
}