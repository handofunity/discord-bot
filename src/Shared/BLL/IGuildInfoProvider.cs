using HoU.GuildBot.Shared.Objects;

namespace HoU.GuildBot.Shared.BLL
{
    public interface IGuildInfoProvider
    {
        EmbedData GetGuildMemberStatus();
    }
}