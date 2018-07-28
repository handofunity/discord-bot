namespace HoU.GuildBot.Shared.BLL
{
    using Objects;

    public interface IGuildInfoProvider
    {
        EmbedData GetGuildMemberStatus();
    }
}